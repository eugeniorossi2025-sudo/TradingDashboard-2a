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
