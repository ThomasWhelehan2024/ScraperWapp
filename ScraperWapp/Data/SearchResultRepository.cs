using Microsoft.EntityFrameworkCore;
using ScraperWapp.Data.Database;
using ScraperWapp.Data.DTOS;
using Serilog;

namespace ScraperWapp.Data;

public class SearchResultRepository
{
    private readonly AppDbContext _db;

    public SearchResultRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<SearchResultDb>> GetByUrl(DateTime date)
    {
        IQueryable<SearchResultDb> query = _db.SearchResults.AsNoTracking();

        return await query.Where(entry =>
            entry.Date.Date == date.Date).ToListAsync();
    }

    public async Task<List<SearchResultDb>> GetByUrl(string url)
    {
        IQueryable<SearchResultDb> query = _db.SearchResults.AsNoTracking();

        return await query.Where(entry => url.Contains(entry.Url)).ToListAsync();
    }

    
    public async Task<List<TopRankDto>> GetTopRanks(int daysSince, int noOfResults)
    {
        IQueryable<SearchResultDb> query = _db.SearchResults.AsNoTracking();

        var result = await query.Where(entry => entry.Date.Date >= DateTime.Today.Date.AddDays(-daysSince))
            .GroupBy(entry => entry.Url)
            .Select(g => new TopRankDto
            {
                Url = g.Key,
                AverageRank = g.Average(entry => entry.Rank),
            })
            .OrderBy(entry => entry.AverageRank)
            .Take(noOfResults)
            .ToListAsync();
        return result;
    }
    public async Task AddData(IList<SearchResultDb> searchResults)
    {
        IQueryable<SearchResultDb> query = _db.SearchResults.AsNoTracking();

        try
        {
            await _db.SearchResults.AddRangeAsync(searchResults);
            await _db.SaveChangesAsync();
        }
        catch
        {
            Log.Error("Could not add search results to database");
        }
    }

    public async Task RemoveData()
    {
        IQueryable<SearchResultDb> query = _db.SearchResults.AsNoTracking();

        var data = await _db.SearchResults.Where(entry => entry.Date.Date < DateTime.Today.Date).ToListAsync();
        try
        {
            _db.SearchResults.RemoveRange(data);
            await _db.SaveChangesAsync();
        }
        catch
        {
            Log.Error("Could not remove search results");
        }
    }
    public async Task<List<SearchResultDb>> GetByDate(DateTime date)
    {
        IQueryable<SearchResultDb> query = _db.SearchResults.AsNoTracking();
        
        return await query.Where(entry => entry.Date.Date == date.Date)
                          .ToListAsync();
    }

    public async Task<List<SearchResultDb>> GetHistoricalData(string url)
    {
        IQueryable<SearchResultDb> query = _db.SearchResults.AsNoTracking();
        
        return await query.Where(entry => entry.Url.Contains(url))
            .ToListAsync();
    }
    
}

