using Contracts.Value;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Extensions.Mapping;

namespace WebApi.Services.Implementations;

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
        return value.ToContract();
    }

    public async Task<ValueResponse?> GetByIdAsync(int id)
    {
        var value = await _context.Values.FindAsync((decimal)id);
        return value?.ToContract();
    }

    public async Task<IEnumerable<ValueResponse>> GetAllAsync(int page = 1, int pageSize = 50)
    {
        var values = await _context.Values
            .OrderByDescending(v => v.DateTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return values.Select(v => v.ToContract());
    }

    public async Task<IEnumerable<ValueResponse>> GetByAccountAsync(string account, int limit = 100)
    {
        var values = await _context.Values
            .Where(v => v.Description == account)
            .OrderByDescending(v => v.DateTime)
            .Take(limit)
            .ToListAsync();

        return values.Select(v => v.ToContract());
    }

    public async Task<IEnumerable<ValueResponse>> GetLatestAsync()
    {
        var values = await _context.Values
            .OrderByDescending(v => v.DateTime)
            .Take(100)
            .ToListAsync();

        return values.Select(v => v.ToContract());
    }

    public async Task<IEnumerable<ValueResponse>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var values = await _context.Values
            .Where(v => v.DateTime >= startDate && v.DateTime <= endDate)
            .OrderByDescending(v => v.DateTime)
            .ToListAsync();

        return values.Select(v => v.ToContract());
    }

    public async Task<bool> UpdateAsync(int id, UpdateValueRequest request)
    {
        var value = await _context.Values.FindAsync((decimal)id);
        if (value == null) return false;

        value.UpdateFromRequest(request);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var value = await _context.Values.FindAsync((decimal)id);
        if (value == null) return false;

        _context.Values.Remove(value);
        await _context.SaveChangesAsync();
        return true;
    }
}
