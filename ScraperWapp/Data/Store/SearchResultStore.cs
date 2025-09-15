using Microsoft.EntityFrameworkCore;
using ScraperWapp.Data.Database;
using ScraperWapp.Data.DTOS;
using Serilog;

namespace ScraperWapp.Data;

public class SearchResultStore 
{
    private readonly List<SearchResultDbModel> _results = new();

 
    public List<SearchResultDbModel> GetByUrl(DateTime date)
    {
        return _results.Where(entry =>
            entry.Date.Date == date.Date).ToList();
    }

    public List<SearchResultDbModel> GetByUrl(string url)
    {
        
        return _results.Where(entry => url.Contains(entry.Url)).ToList();
    }

    
    public List<TopRankModel> GetTopRanks(int daysSince, int noOfResults)
    {
        
        var data = _results.Where(entry => entry.Date.Date >= DateTime.Today.Date.AddDays(-daysSince))
            .GroupBy(entry => entry.Url)
            .Select(g => new TopRankModel
            {
                Url = g.Key,
                AverageRank = g.Average(entry => entry.Rank),
            })
            .OrderBy(entry => entry.AverageRank)
            .Take(noOfResults)
            .ToList();
        return data;
    }
    public void AddData(IList<SearchResultDbModel> searchResults)
    {
        
        try
        {
            _results.AddRange(searchResults);
        }
        catch
        {
            Log.Error("Could not add search results to database");
        }
    }

    public void RemoveData()
    {
        var data = _results.Where(entry => entry.Date.Date < DateTime.Today.Date).ToList();
        try
        {
            foreach (var entry in data)
            {
                _results.Remove(entry);
            }
        }
        catch
        {
            Log.Error("Could not remove search results");
        }
    }
    public List<SearchResultDbModel> GetByDate(DateTime date)
    {   
        return _results.Where(entry => entry.Date.Date == date.Date)
                       .ToList();
    }

    public List<SearchResultDbModel> GetHistoricalData(string url)
    {
        return _results.Where(entry => entry.Url.Contains(url))
            .ToList();
    }

    public void SeedResults()
    {
        var csvPath = Path.Combine(AppContext.BaseDirectory, "land_registry_searches_4week.csv");

        if (!File.Exists(csvPath))
        {
            throw new FileNotFoundException("CSV file not found: " + csvPath);
        }

        foreach (var line in File.ReadLines(csvPath).Skip(1))
        {
            var columns = line.Split(',');
            if (columns.Length == 4 &&
                int.TryParse(columns[0], out int rank) &&
                DateTime.TryParse(columns[3], out DateTime date))
            {
                _results.Add(new SearchResultDbModel
                {
                    Rank = rank,
                    Url = columns[1],
                    Type = columns[2],
                    Date = date
                });
            }
        }
    }

}

