using System.Globalization;
using System.Net;
using System.Text.Json;
using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WebApi.Controllers;
using WebApi.Data;
using WebApi.Options;
using WebApi.Services;

namespace WebApi.Services.Implementations;

public class DashboardService : IDashboardService
{
    private static readonly JsonSerializerOptions DeciderJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly AppDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly DeciderOptions _deciderOptions;

    public DashboardService(
        AppDbContext context,
        IHttpClientFactory httpClientFactory,
        IOptions<DeciderOptions> deciderOptions)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _deciderOptions = deciderOptions.Value;
    }

    public async Task<DashboardStatistics> GetDashboardStatisticsAsync()
    {
        var rows = await GetCurrentStatusRowsAsync();
        if (!rows.Any())
        {
            return new DashboardStatistics
            {
                TotalMargine = 0,
                TempoTrascorso = "0",
                MargineMin = 0,
                MargineMax = 0,
                MargineAttuale = 0,
                TotaleRighe = 0
            };
        }

        var margines = rows.Select(r => r.Margine).ToList();
        return new DashboardStatistics
        {
            TotalMargine = margines.Sum(),
            TempoTrascorso = rows.Max(r => r.Ore).ToString(),
            MargineMin = margines.Min(),
            MargineMax = margines.Max(),
            MargineAttuale = margines.Sum(),
            TotaleRighe = rows.Count
        };
    }

    public async Task<DashboardResponse> GetDashboardDataAsync()
    {
        var rows = await GetCurrentStatusRowsAsync();
        var tableRows = rows.Select(v =>
        {
            var adviceFields = ExtractAdviceFields(v.LastAdvice);
            return new DashboardTableRow
            {
                MinutiPassati = ((int)(DateTime.Now - v.LastUpdate).TotalMinutes).ToString(),
                Computer = v.Computer,
                Account = v.Account,
                Tavolo = v.Tavolo,
                Mazzo = v.Mazzo,
                Margine = v.Margine,
                MediaOra = v.MediaOra,
                Stato = v.Stato,
                Colore = ResolvePlayedColor(v),
                ChosenColor = v.ChosenColor,
                Pbt = v.Pbt,
                ColpoMartingala = v.ColpoMartingala.ToString(),
                ValoreGiocato = v.ValoreGiocato.ToString(),
                Valutazione = v.ValutazioneRisultato,
                Reason = adviceFields?.Reason ?? ExtractAdviceReason(v.LastAdvice),
                LastAdvice = v.LastAdvice,
                LastInfo = v.LastInfo,
                Prediction = v.LastInfo,
                Ore = v.Ore.ToString(),
                SaldoIniziale = v.SaldoIniziale,
                SaldoIstantaneo = v.SaldoIstantaneo,
                DtUltimo = v.LastUpdate,
                LastUpdate = v.LastUpdate,
                AdviceStopL6 = adviceFields?.StopL6,
                AdviceGlobalL5Loss = adviceFields?.GlobalL5Loss,
                AdviceGlobalAuthL6Counter = adviceFields?.GlobalAuthL6Counter,
                AdviceActionCode = adviceFields?.ActionCode,
                AdviceMartingala = adviceFields?.Martingala,
                AdviceHotZone = adviceFields?.HotZone,
                AdviceHotZoneLabel = adviceFields?.HotZoneLabel
            };
        }).ToList();

        var chartData = tableRows
            .Select(r => new ChartDataPoint
            {
                DateTime = DateTime.Now,
                Margine = r.Margine
            })
            .ToList();

        return new DashboardResponse
        {
            Tables = tableRows,
            ChartData = chartData,
            Statistics = new DashboardStatistics
            {
                TotalMargine = tableRows.Sum(r => r.Margine),
                TempoTrascorso = tableRows.Select(r => r.Ore).DefaultIfEmpty("0").Max() ?? "0",
                MargineMin = tableRows.Any() ? tableRows.Min(r => r.Margine) : 0,
                MargineMax = tableRows.Any() ? tableRows.Max(r => r.Margine) : 0,
                MargineAttuale = tableRows.Sum(r => r.Margine),
                TotaleRighe = tableRows.Count
            }
        };
    }

    public async Task<List<ChartDataPoint>> GetMarginiChartAsync(int limit = 200)
    {
        var openMission = await _context.MissionSessions
            .AsNoTracking()
            .Where(session => !session.Completed)
            .OrderByDescending(session => session.StartTime)
            .Select(session => new { session.Id, session.StartTime })
            .FirstOrDefaultAsync();

        if (openMission != null)
        {
            var missionSamples = await _context.MissionMarginSamples
                .AsNoTracking()
                .Where(sample => sample.SessionId == openMission.Id)
                .Select(sample => new ChartDataPoint
                {
                    DateTime = sample.Timestamp,
                    Margine = sample.TotalMargin
                })
                .ToListAsync();

            var liveMargins = await _context.Margini
                .AsNoTracking()
                .Where(m => m.Data != null && m.Data.Value >= openMission.StartTime)
                .Select(m => new ChartDataPoint
                {
                    DateTime = m.Data!.Value,
                    Margine = m.MargineValue ?? 0m
                })
                .ToListAsync();

            return missionSamples
                .Concat(liveMargins)
                .GroupBy(point => point.DateTime)
                .Select(group => group.Last())
                .OrderBy(point => point.DateTime)
                .ToList();
        }

        var items = await _context.Margini
            .AsNoTracking()
            .Where(m => m.Data != null)
            .OrderByDescending(m => m.Data)
            .Take(limit)
            .Select(m => new ChartDataPoint
            {
                DateTime = m.Data!.Value,
                Margine = m.MargineValue ?? 0m
            })
            .ToListAsync();

        items.Reverse();
        return items;
    }

    public async Task<DashboardTelemetry> GetLatestTelemetryAsync()
    {
        if (!await _context.PcCurrentStatuses.AsNoTracking().AnyAsync())
        {
            return new DashboardTelemetry
            {
                SecurityFilterEnabled = await GetSecurityFilterEnabledAsync()
            };
        }

        var row = await _context.Statistiche
            .AsNoTracking()
            .OrderByDescending(s => s.DataInizio)
            .ThenByDescending(s => s.Id)
            .FirstOrDefaultAsync();

        if (row == null)
        {
            return new DashboardTelemetry
            {
                SecurityFilterEnabled = await GetSecurityFilterEnabledAsync()
            };
        }

        var result = new DashboardTelemetry
        {
            SessionStart = row.DataInizio,
            SessionEnd = row.DataFine,
            MargineTot = row.MargineTot,
            MargineMin = row.MargineMin,
            MargineMax = row.MargineMax,
            Elapsed = row.Elapsed,
            RawTelemetry = row.Telemetry
        };

        if (!string.IsNullOrWhiteSpace(row.Telemetry))
        {
            try
            {
                using var doc = JsonDocument.Parse(row.Telemetry);
                var root = doc.RootElement;

                if (root.TryGetProperty("GlobalPauseScalping", out var gps))
                    result.GlobalPauseScalping = gps.GetBoolean();
                if (root.TryGetProperty("GlobalPauseScalpingDetails", out var gpsd))
                    result.GlobalPauseScalpingDetails = gpsd.GetString() ?? "Pausa non attiva";
                if (root.TryGetProperty("GlobalPauseScalpingDuration", out var gpsdur))
                    result.GlobalPauseScalpingDuration = gpsdur.GetString() ?? "0";
                if (root.TryGetProperty("INC", out var inc))
                    result.Inc = (decimal)inc.GetDouble();
                if (root.TryGetProperty("EWMA", out var ewma))
                    result.Ewma = (decimal)ewma.GetDouble();
                if (root.TryGetProperty("TotalPBHandsPlayed", out var pbh))
                    result.TotalPbHandsPlayed = pbh.GetInt32();
                if (root.TryGetProperty("TotalL5Played", out var l5p))
                    result.TotalL5Played = l5p.GetInt32();
                if (root.TryGetProperty("TotalL5Won", out var l5w))
                    result.TotalL5Won = l5w.GetInt32();
                if (root.TryGetProperty("TotalL5Lost", out var l5l))
                    result.TotalL5Lost = l5l.GetInt32();
                if (root.TryGetProperty("SpotID", out var sid))
                    result.SpotId = sid.GetInt32();
                if (root.TryGetProperty("TotalL8Played", out var l8p))
                    result.TotalL8Played = l8p.GetInt32();
                if (root.TryGetProperty("TotalL8Won", out var l8w))
                    result.TotalL8Won = l8w.GetInt32();
                if (root.TryGetProperty("TotalL8Lost", out var l8l))
                    result.TotalL8Lost = l8l.GetInt32();
                if (root.TryGetProperty("TotalAuthL6Authorized", out var tla))
                    result.TotalAuthL6Authorized = tla.GetInt32();
                if (root.TryGetProperty("TotalPauseScalpingSoglieActivated", out var tpss))
                    result.TotalPauseScalpingSoglieActivated = tpss.GetInt32();
                if (root.TryGetProperty("TotalPauseScalpingEWMAActivated", out var tpse))
                    result.TotalPauseScalpingEWMAActivated = tpse.GetInt32();
                if (root.TryGetProperty("SpotPBHandsPlayed", out var sphp))
                    result.SpotPbHandsPlayed = sphp.GetInt32();
                if (root.TryGetProperty("SpotAuthL6Counter", out var salc))
                    result.SpotAuthL6Counter = salc.GetInt32();
                if (root.TryGetProperty("SpotL5Loss", out var sl5l))
                    result.SpotL5Loss = sl5l.GetInt32();
                if (root.TryGetProperty("SecurityFilterEnabled", out var sfe))
                    result.SecurityFilterEnabled = sfe.GetBoolean();
                if (root.TryGetProperty("SecurityFilterMinScore", out var sfms))
                    result.SecurityFilterMinScore = sfms.GetInt32();
                if (root.TryGetProperty("SecurityFilterMinStreak", out var sfst))
                    result.SecurityFilterMinStreak = sfst.GetInt32();
                if (root.TryGetProperty("SecurityFilterMaxShoeHand", out var sfmh))
                    result.SecurityFilterMaxShoeHand = sfmh.GetInt32();
                if (root.TryGetProperty("SecurityFilterMaxAvgSeconds", out var sfma))
                    result.SecurityFilterMaxAvgSeconds = (decimal)sfma.GetDouble();
                if (root.TryGetProperty("SecurityFilterVeryFastSeconds", out var sfvf))
                    result.SecurityFilterVeryFastSeconds = (decimal)sfvf.GetDouble();
                if (root.TryGetProperty("SecurityFilterDeltaWindow", out var sfdw))
                    result.SecurityFilterDeltaWindow = sfdw.GetInt32();
                if (root.TryGetProperty("TotalSecurityFilterActivated", out var tsfa))
                    result.TotalSecurityFilterActivated = tsfa.GetInt32();
                if (root.TryGetProperty("TotalSecurityFilterPreventedL6", out var tsfp))
                    result.TotalSecurityFilterPreventedL6 = tsfp.GetInt32();
                if (root.TryGetProperty("LastAvgHandSeconds", out var lah))
                    result.LastAvgHandSeconds = (decimal)lah.GetDouble();
                if (root.TryGetProperty("ActiveSecurityFilterBots", out var asfb))
                    result.ActiveSecurityFilterBots = asfb.GetInt32();
                if (root.TryGetProperty("SecurityFilterByBot", out var sfbb) && sfbb.ValueKind == JsonValueKind.Object)
                {
                    result.SecurityFilterByBot = JsonSerializer.Deserialize<Dictionary<string, SecurityFilterBotTelemetryDto>>(sfbb.GetRawText())
                        ?? new Dictionary<string, SecurityFilterBotTelemetryDto>();
                }
            }
            catch { /* telemetry JSON malformed — return partial result */ }
        }

        await ApplySecurityFilterConfigAsync(result);
        return result;
    }

    public async Task<SecurityFilterBotTelemetryDto?> GetSecurityFilterBotDetailAsync(string computer, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(computer) || !_deciderOptions.Enabled)
            return null;

        var client = _httpClientFactory.CreateClient(nameof(DashboardService));
        client.Timeout = TimeSpan.FromSeconds(10);

        var url = _deciderOptions.ProactiveUrl($"security-filter/{Uri.EscapeDataString(computer.Trim())}");
        try
        {
            using var response = await client.GetAsync(url, cancellationToken);
            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;
            if (!response.IsSuccessStatusCode)
                return null;

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            return await JsonSerializer.DeserializeAsync<SecurityFilterBotTelemetryDto>(stream, DeciderJsonOptions, cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    private async Task ApplySecurityFilterConfigAsync(DashboardTelemetry result)
    {
        var keys = new[]
        {
            "SECURITY_FILTER_ENABLED",
            "SECURITY_FILTER_MIN_SCORE",
            "SECURITY_FILTER_MIN_STREAK",
            "SECURITY_FILTER_MAX_SHOE_HAND",
            "SECURITY_FILTER_MAX_AVG_SECONDS",
            "SECURITY_FILTER_VERY_FAST_SECONDS",
            "SECURITY_FILTER_DELTA_WINDOW"
        };

        var configs = await _context.Configurations
            .AsNoTracking()
            .Where(c => keys.Contains(c.Key))
            .ToDictionaryAsync(c => c.Key, c => c.Value);

        if (configs.TryGetValue("SECURITY_FILTER_ENABLED", out var enabled))
            result.SecurityFilterEnabled = ParseEnabledFlag(enabled);

        if (configs.TryGetValue("SECURITY_FILTER_MIN_SCORE", out var minScore) &&
            int.TryParse(minScore, NumberStyles.Integer, CultureInfo.InvariantCulture, out var ms))
            result.SecurityFilterMinScore = ms;

        if (configs.TryGetValue("SECURITY_FILTER_MIN_STREAK", out var minStreak) &&
            int.TryParse(minStreak, NumberStyles.Integer, CultureInfo.InvariantCulture, out var mst))
            result.SecurityFilterMinStreak = mst;

        if (configs.TryGetValue("SECURITY_FILTER_MAX_SHOE_HAND", out var maxShoe) &&
            int.TryParse(maxShoe, NumberStyles.Integer, CultureInfo.InvariantCulture, out var msh))
            result.SecurityFilterMaxShoeHand = msh;

        if (configs.TryGetValue("SECURITY_FILTER_MAX_AVG_SECONDS", out var maxAvg) &&
            double.TryParse(maxAvg, NumberStyles.Float, CultureInfo.InvariantCulture, out var mas))
            result.SecurityFilterMaxAvgSeconds = (decimal)mas;

        if (configs.TryGetValue("SECURITY_FILTER_VERY_FAST_SECONDS", out var veryFast) &&
            double.TryParse(veryFast, NumberStyles.Float, CultureInfo.InvariantCulture, out var vfs))
            result.SecurityFilterVeryFastSeconds = (decimal)vfs;

        if (configs.TryGetValue("SECURITY_FILTER_DELTA_WINDOW", out var deltaWindow) &&
            int.TryParse(deltaWindow, NumberStyles.Integer, CultureInfo.InvariantCulture, out var dw))
            result.SecurityFilterDeltaWindow = dw;
    }

    private static bool ParseEnabledFlag(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return true;

        return value.Trim().Equals("1", StringComparison.OrdinalIgnoreCase)
            || value.Trim().Equals("true", StringComparison.OrdinalIgnoreCase)
            || value.Trim().Equals("yes", StringComparison.OrdinalIgnoreCase)
            || value.Trim().Equals("on", StringComparison.OrdinalIgnoreCase);
    }

    private async Task<bool> GetSecurityFilterEnabledAsync()
    {
        var value = await _context.Configurations
            .AsNoTracking()
            .Where(c => c.Key == "SECURITY_FILTER_ENABLED")
            .Select(c => c.Value)
            .FirstOrDefaultAsync();

        return ParseEnabledFlag(value);
    }

    private Task<List<Entities.PcCurrentStatus>> GetCurrentStatusRowsAsync()
    {
        return _context.PcCurrentStatuses
            .AsNoTracking()
            .OrderBy(v => v.Computer)
            .ToListAsync();
    }

    private static string? ExtractAdviceReason(string? lastAdvice)
    {
        if (string.IsNullOrWhiteSpace(lastAdvice))
            return null;

        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(lastAdvice);
            if (doc.RootElement.TryGetProperty("Reason", out var reason))
                return reason.GetString();
        }
        catch
        {
            /* legacy plain-text reason */
        }

        return lastAdvice.Length <= 64 ? lastAdvice : "Default";
    }

    private static LastAdviceFields? ExtractAdviceFields(string? lastAdvice)
    {
        if (string.IsNullOrWhiteSpace(lastAdvice))
            return null;

        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(lastAdvice);
            var root = doc.RootElement;
            var fields = new LastAdviceFields();

            if (root.TryGetProperty("Reason", out var reason))
                fields.Reason = reason.GetString();
            if (root.TryGetProperty("StopL6", out var stopL6))
                fields.StopL6 = stopL6.GetBoolean();
            if (root.TryGetProperty("GlobalL5Loss", out var gl5l))
                fields.GlobalL5Loss = gl5l.GetInt32();
            if (root.TryGetProperty("GlobalAuthL6Counter", out var galc))
                fields.GlobalAuthL6Counter = galc.GetInt32();
            if (root.TryGetProperty("ActionCode", out var ac))
                fields.ActionCode = ac.GetInt32();
            if (root.TryGetProperty("Martingala", out var mart))
                fields.Martingala = mart.GetInt32();
            if (root.TryGetProperty("HotZone", out var hz))
                fields.HotZone = hz.GetBoolean();
            if (root.TryGetProperty("HotZoneLabel", out var hzl))
                fields.HotZoneLabel = hzl.GetString();

            return fields;
        }
        catch
        {
            return null;
        }
    }

    private static string? ResolvePlayedColor(PcCurrentStatus row)
    {
        var chosen = row.ChosenColor?.Trim();
        if (!string.IsNullOrEmpty(chosen))
            return chosen.ToUpperInvariant();

        var legacy = row.Colore?.Trim();
        if (string.IsNullOrEmpty(legacy))
            return null;

        return legacy.ToUpperInvariant() switch
        {
            "P" or "PLAYER" or "BLU" or "BLU_PLAY" or "BLUE" => "P",
            "B" or "BANKER" or "ROSSO" or "RED" or "RED_BANK" => "B",
            "T" or "TIE" or "VERDE" or "GREEN" => "T",
            _ => legacy.Length == 1 ? legacy.ToUpperInvariant() : legacy
        };
    }
}
