using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Models;

namespace WebApi.Services.Implementations;

/// <summary>
/// Upserts live bot row on production <c>Pc_CurrentStatus</c> (same SP as Decisore engine).
/// </summary>
public class PcCurrentStatusMirrorService : IPcCurrentStatusMirrorService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PcCurrentStatusMirrorService> _logger;

    public PcCurrentStatusMirrorService(
        AppDbContext context,
        IConfiguration configuration,
        ILogger<PcCurrentStatusMirrorService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task MirrorAsync(MirrorPcStatusRequest request, CancellationToken cancellationToken = default)
    {
        var key = long.Parse(DateTime.Now.ToString("yyyyMMddHHmmssfff"));
        var connString = _configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection is not configured.");

        await using var conn = new SqlConnection(connString);
        await conn.OpenAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(request.LastAdviceJson))
        {
            await ExecuteFullUpsertAsync(conn, request, key, cancellationToken);
        }
        else
        {
            await ExecuteSimpleUpsertAsync(conn, request, key, cancellationToken);
        }

        _logger.LogInformation(
            "Collaudo mirror upsert PC={Computer} margine={Margine} mazzo={Mazzo} stato={Stato}",
            request.Computer, request.Margine, request.Mazzo, request.Stato);
    }

    private static async Task ExecuteSimpleUpsertAsync(
        SqlConnection conn,
        MirrorPcStatusRequest request,
        long key,
        CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand("Upsert_Pc_CurrentStatus_Simple", conn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cmd.Parameters.AddWithValue("@COMPUTER", request.Computer);
        cmd.Parameters.AddWithValue("@KEY", key);
        cmd.Parameters.AddWithValue("@PBT", (object?)request.Pbt ?? " ");
        cmd.Parameters.AddWithValue("@ACCOUNT", (object?)request.Account ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@TAVOLO", (object?)request.Tavolo ?? DBNull.Value);
        cmd.Parameters.Add("@SALDO_INIZIALE", SqlDbType.Decimal).Value = request.SaldoIniziale;
        cmd.Parameters.Add("@SALDO_ISTANTANEO", SqlDbType.Decimal).Value = request.SaldoIstantaneo;
        cmd.Parameters.Add("@MARGINE", SqlDbType.Decimal).Value = request.Margine;
        cmd.Parameters.Add("@VALORE_GIOCATO", SqlDbType.Decimal).Value = request.ValoreGiocato;
        cmd.Parameters.AddWithValue("@COLPO_MARTINGALA", request.ColpoMartingala);
        cmd.Parameters.AddWithValue("@STATO", (object?)request.Stato ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@MAZZO", (object?)request.Mazzo ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@MAZZO_CALCOLATO", (object?)request.Mazzo ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@CHOSEN_COLOR", (object?)request.Colore ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ORE", request.Ore);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task ExecuteFullUpsertAsync(
        SqlConnection conn,
        MirrorPcStatusRequest request,
        long key,
        CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand("Upsert_Pc_CurrentStatus", conn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cmd.Parameters.AddWithValue("@COMPUTER", request.Computer);
        cmd.Parameters.AddWithValue("@KEY", key);
        cmd.Parameters.AddWithValue("@ACCOUNT", (object?)request.Account ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@TAVOLO", (object?)request.Tavolo ?? DBNull.Value);
        cmd.Parameters.Add("@SALDO_INIZIALE", SqlDbType.Decimal).Value = request.SaldoIniziale;
        cmd.Parameters.Add("@SALDO_ISTANTANEO", SqlDbType.Decimal).Value = request.SaldoIstantaneo;
        cmd.Parameters.Add("@MARGINE", SqlDbType.Decimal).Value = request.Margine;
        cmd.Parameters.Add("@VALORE_GIOCATO", SqlDbType.Decimal).Value = request.ValoreGiocato;
        cmd.Parameters.AddWithValue("@COLPO_MARTINGALA", request.ColpoMartingala);
        cmd.Parameters.AddWithValue("@STATO", (object?)request.Stato ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@MAZZO", (object?)request.Mazzo ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@MAZZO_CALCOLATO", (object?)request.Mazzo ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@PBT", (object?)request.Pbt ?? " ");
        cmd.Parameters.AddWithValue("@CHOSEN_COLOR", (object?)request.Colore ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ORE", request.Ore);
        cmd.Parameters.AddWithValue("@LAST_ADVICE", request.LastAdviceJson ?? string.Empty);
        cmd.Parameters.AddWithValue("@LAST_INFO", string.Empty);
        cmd.Parameters.AddWithValue("@MISSION_SNAPSHOT", string.Empty);
        cmd.Parameters.AddWithValue("@VALUTAZIONE_RISULTATO", string.Empty);
        cmd.Parameters.AddWithValue("@LAST_ACTION", 0);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<MirrorPcStatusRequest?> GetPcStatusAsync(string computer, CancellationToken cancellationToken = default)
    {
        var row = await _context.PcCurrentStatuses
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Computer == computer, cancellationToken);

        if (row == null)
            return null;

        return new MirrorPcStatusRequest
        {
            Computer = row.Computer,
            Account = row.Account,
            Tavolo = row.Tavolo,
            SaldoIniziale = row.SaldoIniziale,
            SaldoIstantaneo = row.SaldoIstantaneo,
            Margine = row.Margine,
            ValoreGiocato = row.ValoreGiocato,
            ColpoMartingala = row.ColpoMartingala,
            Stato = row.Stato,
            Mazzo = row.Mazzo,
            Pbt = row.Pbt,
            Ore = row.Ore,
            Colore = row.Colore,
            LastAdviceJson = row.LastAdvice
        };
    }
}
