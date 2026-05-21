using Microsoft.EntityFrameworkCore;
using WebApi.Controllers;
using WebApi.Data;

namespace WebApi.Services.Implementations;

/// <summary>
/// Service for managing dashboard operations.
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardService"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public DashboardService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets the latest dashboard statistics asynchronously.
    /// </summary>
    /// <returns>Dashboard statistics.</returns>
    public async Task<DashboardStatistics> GetDashboardStatisticsAsync()
    {
        var twoHoursAgo = DateTime.Now.AddHours(-2);

        // Query SQL raw per ottenere gli ultimi valori per ogni account/tavolo
        var latestValues = await _context.Values
            .FromSqlRaw(@"
                WITH LatestValues AS (
                    SELECT *,
                           ROW_NUMBER() OVER (PARTITION BY ACCOUNT, TAVOLO ORDER BY DateTime DESC) AS rn
                    FROM [Values]
                    WHERE DateTime >= {0} AND ACCOUNT IS NOT NULL AND TAVOLO IS NOT NULL
                )
                SELECT * FROM LatestValues WHERE rn = 1
            ", twoHoursAgo)
            .ToListAsync();

        if (!latestValues.Any())
            return new DashboardStatistics
            {
                TotalMargine = 0,
                TempoTrascorso = "0",
                MargineMin = 0,
                MargineMax = 0,
                MargineAttuale = 0,
                TotaleRighe = 0
            };

        var totalMargine = latestValues.Sum(v => v.Margine ?? 0);
        var maxTime = latestValues
            .Where(v => !string.IsNullOrEmpty(v.Tempo))
            .Select(v => v.Tempo)
            .DefaultIfEmpty("0")
            .Max() ?? "0";

        // Prendi dati grafico ultime 24h
        var twentyFourHoursAgo = DateTime.Now.AddHours(-24);
        var chartData = await _context.Values
            .Where(v => v.DateTime >= twentyFourHoursAgo && v.Margine.HasValue)
            .Select(v => v.Margine!.Value)
            .ToListAsync();

        decimal margineMin = 0;
        decimal margineMax = 0;

        if (chartData.Any())
        {
            margineMin = chartData.Min();
            margineMax = chartData.Max();
        }

        return new DashboardStatistics
        {
            TotalMargine = totalMargine,
            TempoTrascorso = maxTime,
            MargineMin = margineMin,
            MargineMax = margineMax,
            MargineAttuale = totalMargine,
            TotaleRighe = latestValues.Count
        };
    }

    /// <summary>
    /// Gets complete dashboard data with table rows and chart data.
    /// </summary>
    /// <returns>Complete dashboard response.</returns>
    public async Task<DashboardResponse> GetDashboardDataAsync()
    {
        var twoHoursAgo = DateTime.Now.AddHours(-2);

        // Query SQL raw per ottenere gli ultimi valori per ogni account/tavolo
        var latestValues = await _context.Values
            .FromSqlRaw(@"
                WITH LatestValues AS (
                    SELECT *,
                           ROW_NUMBER() OVER (PARTITION BY ACCOUNT, TAVOLO ORDER BY DateTime DESC) AS rn
                    FROM [Values]
                    WHERE DateTime >= {0} AND ACCOUNT IS NOT NULL AND TAVOLO IS NOT NULL
                )
                SELECT * FROM LatestValues WHERE rn = 1
            ", twoHoursAgo)
            .ToListAsync();

        var tableRows = latestValues.Select(v => new DashboardTableRow
        {
            MinutiPassati = ((int)(DateTime.Now - v.DateTime).TotalMinutes).ToString(),
            Account = v.Account,
            Tavolo = v.Tavolo?.ToString(),
            Mazzo = v.Mazzo?.ToString(),
            Margine = v.Margine ?? 0,
            MediaOra = v.MediaOra ?? 0,
            Stato = v.Stato,
            Colore = v.Colore,
            ColpoMartingala = v.ColpoMartingala?.ToString(),
            Valutazione = v.Valutazione,
            Reason = v.Reason,
            Prediction = v.Prediction,
            Ore = v.Tempo,
            SaldoIniziale = 0,
            SaldoIstantaneo = 0
        }).ToList();

        // Prendi dati grafico ultime 24h
        var twentyFourHoursAgo = DateTime.Now.AddHours(-24);
        var chartData = await _context.Values
            .Where(v => v.DateTime >= twentyFourHoursAgo && v.Margine.HasValue)
            .OrderBy(v => v.DateTime)
            .Select(v => new ChartDataPoint
            {
                DateTime = v.DateTime,
                Margine = v.Margine!.Value
            })
            .ToListAsync();

        var statistics = new DashboardStatistics
        {
            TotalMargine = tableRows.Sum(r => r.Margine),
            TempoTrascorso = tableRows
                .Where(r => !string.IsNullOrEmpty(r.Ore))
                .Select(r => r.Ore)
                .DefaultIfEmpty("0")
                .Max() ?? "0",
            MargineMin = chartData.Any() ? chartData.Min(c => c.Margine) : 0,
            MargineMax = chartData.Any() ? chartData.Max(c => c.Margine) : 0,
            MargineAttuale = tableRows.Sum(r => r.Margine),
            TotaleRighe = tableRows.Count
        };

        return new DashboardResponse
        {
            Tables = tableRows,
            ChartData = chartData,
            Statistics = statistics
        };
    }
}