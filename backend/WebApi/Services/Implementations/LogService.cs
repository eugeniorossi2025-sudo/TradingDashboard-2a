using Contracts.Log;
using Entities;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Extensions.Mapping;

namespace WebApi.Services.Implementations;

public class LogService : ILogService
{
    private readonly AppDbContext _context;

    public LogService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedApiLogResult> GetPagedAsync(
        DateTime? from,
        DateTime? to,
        string? category,
        int? action,
        string? description,
        int page,
        int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : Math.Min(pageSize, 200);

        var query = _context.Logs.AsNoTracking().AsQueryable();

        if (from.HasValue)
        {
            query = query.Where(l => l.CreatedAt >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(l => l.CreatedAt <= to.Value);
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(l => l.Category.Contains(category));
        }

        if (action.HasValue)
        {
            query = query.Where(l => l.Action == action.Value);
        }

        if (!string.IsNullOrWhiteSpace(description))
        {
            query = query.Where(l => l.Description.Contains(description));
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedApiLogResult
        {
            Items = items.Select(l => l.ToDto()).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize),
        };
    }

    public async Task<IEnumerable<Log>> GetAllAsync()
    {
        return await _context.Logs.AsNoTracking().OrderByDescending(l => l.CreatedAt).ToListAsync();
    }

    public async Task<Log?> GetByIdAsync(int id)
    {
        return await _context.Logs.FindAsync(id);
    }

    public async Task<Log> CreateAsync(CreateLogRequest request)
    {
        var log = request.ToEntity();
        _context.Logs.Add(log);
        await _context.SaveChangesAsync();
        return log;
    }

    public async Task<bool> UpdateAsync(int id, UpdateLogRequest request)
    {
        var log = await _context.Logs.FindAsync(id);
        if (log == null) return false;

        log.UpdateFromRequest(request);
        _context.Logs.Update(log);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var log = await _context.Logs.FindAsync(id);
        if (log == null) return false;

        _context.Logs.Remove(log);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> DeleteFilteredAsync(DateTime? from, DateTime? to, string? category, int? action)
    {
        var query = _context.Logs.AsQueryable();

        if (from.HasValue)
        {
            query = query.Where(l => l.CreatedAt >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(l => l.CreatedAt <= to.Value);
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(l => l.Category.Contains(category));
        }

        if (action.HasValue)
        {
            query = query.Where(l => l.Action == action.Value);
        }

        var logs = await query.ToListAsync();
        if (logs.Count == 0)
        {
            return 0;
        }

        _context.Logs.RemoveRange(logs);
        await _context.SaveChangesAsync();
        return logs.Count;
    }
}
