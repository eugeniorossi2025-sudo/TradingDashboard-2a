using Contracts.Value;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Extensions.Mapping;

namespace WebApi.Services.Implementations;

/// <summary>
/// Service for managing telemetry values.
/// </summary>
public class ValueService : IValueService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ValueService> _logger;

    public ValueService(AppDbContext context, ILogger<ValueService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ValueResponse> CreateAsync(CreateValueRequest request)
    {
        var value = request.MapToEntity();

        _context.Values.Add(value);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Value created - ID: {Id}, Account: {Account}, Tavolo: {Tavolo}, User: {UserId}",
            value.Id, value.Account, value.Tavolo, value.IdUser);

        return value.ToContract();
    }

    public async Task<ValueResponse?> GetByIdAsync(int id)
    {
        _logger.LogDebug("Getting value by ID: {Id}", id);
        var value = await _context.Values.FindAsync(id);

        if (value == null)
        {
            _logger.LogWarning("Value not found - ID: {Id}", id);
            return null;
        }

        return value.ToContract();
    }

    public async Task<IEnumerable<ValueResponse>> GetAllAsync(int page = 1, int pageSize = 50)
    {
        _logger.LogDebug("Getting values - Page: {Page}, PageSize: {PageSize}", page, pageSize);

        var values = await _context.Values
            .OrderByDescending(v => v.DateTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        _logger.LogInformation("Retrieved {Count} values", values.Count);

        return values.Select(v => v.ToContract());
    }

    public async Task<IEnumerable<ValueResponse>> GetByAccountAsync(string account, int limit = 100)
    {
        _logger.LogDebug("Getting values by account: {Account}, Limit: {Limit}", account, limit);

        var values = await _context.Values
            .Where(v => v.Account == account)
            .OrderByDescending(v => v.DateTime)
            .Take(limit)
            .ToListAsync();

        _logger.LogInformation("Retrieved {Count} values for account {Account}", values.Count, account);

        return values.Select(v => v.ToContract());
    }

    public async Task<IEnumerable<ValueResponse>> GetLatestAsync()
    {
        var twoHoursAgo = DateTime.Now.AddHours(-2);

        _logger.LogDebug("Getting latest values since {DateTime}", twoHoursAgo);

        var values = await _context.Values
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

        _logger.LogInformation("Retrieved {Count} latest values", values.Count);

        return values.Select(v => v.ToContract());
    }

    public async Task<IEnumerable<ValueResponse>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        _logger.LogDebug("Getting values by date range: {StartDate} to {EndDate}", startDate, endDate);

        var values = await _context.Values
            .Where(v => v.DateTime >= startDate && v.DateTime <= endDate)
            .OrderByDescending(v => v.DateTime)
            .ToListAsync();

        _logger.LogInformation("Retrieved {Count} values in date range", values.Count);

        return values.Select(v => v.ToContract());
    }

    public async Task<bool> UpdateAsync(int id, UpdateValueRequest request)
    {
        _logger.LogDebug("Updating value ID: {Id}", id);

        var value = await _context.Values.FindAsync(id);
        if (value == null)
        {
            _logger.LogWarning("Value not found for update - ID: {Id}", id);
            return false;
        }

        value.UpdateFromRequest(request);

        await _context.SaveChangesAsync();

        _logger.LogInformation("Value updated - ID: {Id}", id);

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        _logger.LogDebug("Deleting value ID: {Id}", id);

        var value = await _context.Values.FindAsync(id);
        if (value == null)
        {
            _logger.LogWarning("Value not found for deletion - ID: {Id}", id);
            return false;
        }

        _context.Values.Remove(value);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Value deleted - ID: {Id}", id);

        return true;
    }
}