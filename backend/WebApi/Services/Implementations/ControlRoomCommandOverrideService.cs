using Entities;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;

namespace WebApi.Services.Implementations;

public class ControlRoomCommandOverrideService : IControlRoomCommandOverrideService
{
    public const int ContinueActionCode = 0;
    public const int ResetMartingaleActionCode = 2;
    public const string ContinueCommandType = "Continue";
    public const string ResetMartingaleCommandType = "ResetMartingale";

    private readonly AppDbContext _context;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private bool _schemaEnsured;

    public ControlRoomCommandOverrideService(AppDbContext context) => _context = context;

    public async Task EnsureSchemaAsync(CancellationToken cancellationToken = default)
    {
        if (_schemaEnsured)
            return;

        await _gate.WaitAsync(cancellationToken);
        try
        {
            if (_schemaEnsured)
                return;

            const string sql = """
IF OBJECT_ID(N'[dbo].[ControlRoomCommandOverrides]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ControlRoomCommandOverrides](
        [PC] NVARCHAR(50) NOT NULL CONSTRAINT [PK_ControlRoomCommandOverrides] PRIMARY KEY,
        [ActionCode] INT NOT NULL,
        [CommandType] NVARCHAR(32) NOT NULL,
        [CreatedAtUtc] DATETIME2 NOT NULL CONSTRAINT [DF_ControlRoomCommandOverrides_CreatedAtUtc] DEFAULT(SYSUTCDATETIME()),
        [CreatedByUserId] INT NULL
    );
END;
""";
            await _context.Database.ExecuteSqlRawAsync(sql, cancellationToken);
            _schemaEnsured = true;
        }
        finally
        {
            _gate.Release();
        }
    }

    public Task<ControlRoomCommandOverrideResult> SetContinueAsync(string pc, int? userId, CancellationToken cancellationToken = default) =>
        UpsertAsync(pc, ContinueActionCode, ContinueCommandType, userId, cancellationToken);

    public Task<ControlRoomCommandOverrideResult> SetResetMartingaleAsync(string pc, int? userId, CancellationToken cancellationToken = default) =>
        UpsertAsync(pc, ResetMartingaleActionCode, ResetMartingaleCommandType, userId, cancellationToken);

    private async Task<ControlRoomCommandOverrideResult> UpsertAsync(
        string pc,
        int actionCode,
        string commandType,
        int? userId,
        CancellationToken cancellationToken)
    {
        var normalizedPc = NormalizePc(pc);
        await EnsureSchemaAsync(cancellationToken);

        var row = await _context.ControlRoomCommandOverrides
            .FirstOrDefaultAsync(x => x.Pc == normalizedPc, cancellationToken);

        if (row == null)
        {
            row = new ControlRoomCommandOverride
            {
                Pc = normalizedPc,
                ActionCode = actionCode,
                CommandType = commandType,
                CreatedAtUtc = DateTime.UtcNow,
                CreatedByUserId = userId
            };
            _context.ControlRoomCommandOverrides.Add(row);
        }
        else
        {
            row.ActionCode = actionCode;
            row.CommandType = commandType;
            row.CreatedAtUtc = DateTime.UtcNow;
            row.CreatedByUserId = userId;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new ControlRoomCommandOverrideResult
        {
            Pc = normalizedPc,
            ActionCode = actionCode,
            CommandType = commandType
        };
    }

    private static string NormalizePc(string pc) =>
        string.IsNullOrWhiteSpace(pc) ? string.Empty : pc.Trim();
}
