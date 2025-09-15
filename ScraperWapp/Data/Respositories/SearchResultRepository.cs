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
    
    public async Task<List<SearchResultDbModel>> GetByUrl(DateTime date)
    {
        IQueryable<SearchResultDbModel> query = _db.SearchResults.AsNoTracking();

        return await query.Where(entry =>
            entry.Date.Date == date.Date).ToListAsync();
    }

    public async Task<List<SearchResultDbModel>> GetByUrl(string url)
    {
        IQueryable<SearchResultDbModel> query = _db.SearchResults.AsNoTracking();

        return await query.Where(entry => url.Contains(entry.Url)).ToListAsync();
    }

    
    public async Task<List<TopRankModel>> GetTopRanks(int daysSince, int noOfResults)
    {
        IQueryable<SearchResultDbModel> query = _db.SearchResults.AsNoTracking();

        var result = await query.Where(entry => entry.Date.Date >= DateTime.Today.Date.AddDays(-daysSince))
            .GroupBy(entry => entry.Url)
            .Select(g => new TopRankModel
            {
                Url = g.Key,
                AverageRank = g.Average(entry => entry.Rank),
            })
            .OrderBy(entry => entry.AverageRank)
            .Take(noOfResults)
            .ToListAsync();
        return result;
    }
    public async Task AddData(IList<SearchResultDbModel> searchResults)
    {
        IQueryable<SearchResultDbModel> query = _db.SearchResults.AsNoTracking();

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
        IQueryable<SearchResultDbModel> query = _db.SearchResults.AsNoTracking();

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
    public async Task<List<SearchResultDbModel>> GetByDate(DateTime date)
    {
        IQueryable<SearchResultDbModel> query = _db.SearchResults.AsNoTracking();
        
        return await query.Where(entry => entry.Date.Date == date.Date)
                          .ToListAsync();
    }

    public async Task<List<SearchResultDbModel>> GetHistoricalData(string url)
    {
        IQueryable<SearchResultDbModel> query = _db.SearchResults.AsNoTracking();
        
        return await query.Where(entry => entry.Url.Contains(url))
            .ToListAsync();
    }
    
}

