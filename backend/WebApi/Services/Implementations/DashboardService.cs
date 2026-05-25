using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WebApi.Controllers;
using WebApi.Data;

namespace WebApi.Services.Implementations;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _context;

    public DashboardService(AppDbContext context)
    {
        _context = context;
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
        var tableRows = rows.Select(v => new DashboardTableRow
        {
            MinutiPassati = ((int)(DateTime.Now - v.LastUpdate).TotalMinutes).ToString(),
            Computer = v.Computer,
            Account = v.Account,
            Tavolo = v.Tavolo,
            Mazzo = v.Mazzo,
            Margine = v.Margine,
            MediaOra = v.MediaOra,
            Stato = v.Stato,
            Colore = v.Colore,
            Pbt = v.Pbt,
            ColpoMartingala = v.ColpoMartingala.ToString(),
            ValoreGiocato = v.ValoreGiocato.ToString(),
            Valutazione = v.ValutazioneRisultato,
            Reason = ExtractAdviceReason(v.LastAdvice),
            LastAdvice = v.LastAdvice,
            LastInfo = v.LastInfo,
            Prediction = v.LastInfo,
            Ore = v.Ore.ToString(),
            SaldoIniziale = v.SaldoIniziale,
            SaldoIstantaneo = v.SaldoIstantaneo,
            DtUltimo = v.LastUpdate,
            LastUpdate = v.LastUpdate
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
        return await _context.Margini
            .AsNoTracking()
            .Where(m => m.Data != null)
            .OrderBy(m => m.Data)
            .TakeLast(limit)
            .Select(m => new ChartDataPoint
            {
                DateTime = m.Data!.Value,
                Margine = m.MargineValue ?? 0m
            })
            .ToListAsync();
    }

    public async Task<DashboardTelemetry> GetLatestTelemetryAsync()
    {
        var row = await _context.Statistiche
            .AsNoTracking()
            .OrderByDescending(s => s.DataInizio)
            .FirstOrDefaultAsync();

        if (row == null)
            return new DashboardTelemetry();

        var result = new DashboardTelemetry
        {
            SessionStart = row.DataInizio,
            SessionEnd = row.DataFine,
            MargineTot = row.MargineTot,
            MargineMin = row.MargineMin,
            MargineMax = row.MargineMax,
            Elapsed = row.Elapsed
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
            }
            catch { /* telemetry JSON malformed — return partial result */ }
        }

        return result;
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
}
