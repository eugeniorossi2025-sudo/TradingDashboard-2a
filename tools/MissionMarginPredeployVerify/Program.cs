using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WebApi.Data;
using WebApi.Services;
using WebApi.Services.Implementations;

// dotnet run --project tools/MissionMarginPredeployVerify
// Optional: --connection "Server=..."

var connection = GetConnection(args);
if (string.IsNullOrWhiteSpace(connection))
{
    Console.Error.WriteLine("Missing SQL connection. Set DASH2A_SQL or pass --connection.");
    return 1;
}

var expectedSpot = new Dictionary<int, decimal>
{
    [102] = 39.60m,
    [103] = 99.20m,
    [104] = 125.10m,
    [105] = 254.40m,
};

var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseSqlServer(connection)
    .Options;

await using var db = new AppDbContext(options);
var builder = new MissionReportBuilder(db);

var dbSessions = await db.MissionSessions
    .AsNoTracking()
    .Where(s => s.Completed)
    .OrderByDescending(s => s.Id)
    .Take(20)
    .Select(s => new { s.Id, s.TotalMargin, s.RuntimeMode })
    .ToListAsync();

if (dbSessions.Count == 0)
{
    Console.Error.WriteLine("No completed missions in database.");
    return 2;
}

Console.WriteLine("=== Mission report pre-deploy verify (last 20 completed) ===");
Console.WriteLine($"DB sessions: {dbSessions.Count}");
Console.WriteLine();

var fail = 0;
var heroRegex = new Regex(
    @"RISULTATO PERIODO[^<]*</div>\s*<div class=""heroValue[^""]*"">([^<]+)</div>",
    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

foreach (var row in dbSessions.OrderBy(s => s.Id))
{
    var report = await builder.BuildSessionReportAsync(row.Id);
    if (report == null)
    {
        Console.WriteLine($"FAIL #{row.Id}: report builder returned null");
        fail++;
        continue;
    }

    var session = report.Sessions.SingleOrDefault(s => s.SessionId == row.Id);
    if (session == null)
    {
        Console.WriteLine($"FAIL #{row.Id}: session missing from report");
        fail++;
        continue;
    }

    var html = MissionReportHtmlBuilder.Build(report);
    var htmlHero = TryParseHeroMargin(html, heroRegex);
    var periodResult = report.Totals.PeriodResultEuro;
    var missionMarginEuro = session.MissionMarginEuro;
    var periodNet = session.PeriodNetPnlEuro;
    var dbMargin = row.TotalMargin;

    var okHero = htmlHero.HasValue && Math.Abs(htmlHero.Value - periodResult) < 0.005m;
    var okDbMargin = Math.Abs(dbMargin - missionMarginEuro) < 0.005m;
    var okAll = okHero && okDbMargin;

    if (!okAll) fail++;

    var status = okAll ? "PASS" : "FAIL";
    Console.WriteLine(
        $"{status} #{row.Id} PeriodResult={periodResult:0.00} HTML={htmlHero?.ToString("0.00", CultureInfo.InvariantCulture) ?? "n/a"} MissionMargin={missionMarginEuro:0.00} DB={dbMargin:0.00} PeriodNetPnl={periodNet:0.00}");

    if (expectedSpot.TryGetValue(row.Id, out var expected))
    {
        var spotOk = Math.Abs(dbMargin - expected) < 0.005m
                     && Math.Abs(missionMarginEuro - expected) < 0.005m;
        var spotStatus = spotOk ? "PASS" : "FAIL";
        if (!spotOk) fail++;
        Console.WriteLine($"  {spotStatus} spot-check closing margin expected {expected:0.00} €");
    }
}

Console.WriteLine();
if (fail > 0)
{
    Console.WriteLine($"FAILED: {fail} issue(s). Deploy blocked.");
    return 3;
}

Console.WriteLine("ALL PASS: HTML hero = periodResultEuro; MissionMarginEuro matches MissionSessions.TotalMargin.");
return 0;

static string? GetConnection(string[] args)
{
    for (var i = 0; i < args.Length - 1; i++)
    {
        if (string.Equals(args[i], "--connection", StringComparison.OrdinalIgnoreCase))
            return args[i + 1];
    }

    return Environment.GetEnvironmentVariable("DASH2A_SQL");
}

static decimal? TryParseHeroMargin(string html, Regex heroRegex)
{
    var m = heroRegex.Match(html);
    if (!m.Success) return null;
    var raw = m.Groups[1].Value.Trim()
        .Replace("€", "", StringComparison.Ordinal)
        .Replace("+", "", StringComparison.Ordinal)
        .Replace(" ", "", StringComparison.Ordinal)
        .Replace(",", ".", StringComparison.Ordinal);
    return decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : null;
}
