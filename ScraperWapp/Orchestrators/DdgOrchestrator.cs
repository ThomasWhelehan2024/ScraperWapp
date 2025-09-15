using ScraperWapp.Services;
using ScraperWapp.BackEnd.Interfaces.Adapters;
using ScraperWapp.BackEnd.Interfaces.Services;
using ScraperWapp.BackEnd.Models;
using ScraperWapp.Data;
using ScraperWapp.Data.Database;

namespace ScraperWapp.Orchestrators
{
    public class DdgOrchestrator : IOrchestrator
    {
        private readonly ISearchEngineClient _ddgClient;
        private readonly IScraperService _scrapingService;
        private readonly IAnalysisService _analysisService;
        private readonly SearchResultRepository _searchResultRepository;
        private readonly ILogger<DdgOrchestrator> _logger;

        public DdgOrchestrator(ISearchEngineClient ddgClient, IScraperService scrapingService,
            IAnalysisService analysisService, SearchResultRepository searchResultRepository)
        {
            _ddgClient = ddgClient;
            _scrapingService = scrapingService;
            _analysisService = analysisService;
            _searchResultRepository = searchResultRepository;
        }

        
        public async Task<IList<IRankingModel>> CollectResultsAsync()
        {
            IList<SearchResultDbModel> todaysResults = new List<SearchResultDbModel>();
            todaysResults = await _searchResultRepository.GetByDate(DateTime.Today);

            if (todaysResults.Any())
            {
                IList<IRankingModel> results = todaysResults.Select(r => (IRankingModel)new RankingModel
                {
                    Type = r.Type,
                    Rank = r.Rank,
                    Url = r.Url,
                }).OrderByDescending(r => r.Rank).ToList();
                return results;
            }

            var pages = await _ddgClient.FetchPagesAsync("https://duckduckgo.com/html/");

            string startDiv = "<div class=\"serp__results\">";

            List<string> entries = new List<string>();

            foreach (var page in pages)
            {
                var rawEntries = _scrapingService.GetOuterHtml(page, startDiv);
                if (rawEntries == null)
                    continue;
                entries.AddRange(_scrapingService.SplitRawEntries(rawEntries.Html,
                    @"<div class=""result results_links results_links_deep\s+[^""]+"">"));
            }

            entries = entries.GetRange(0, Math.Min(100, entries.Count)).ToList();

            var rankings = _analysisService.GetRankings(entries);

            await _searchResultRepository.AddData(rankings.Select(r => new SearchResultDbModel
            {
                Type = r.Type,
                Rank = r.Rank,
                Url = r.Url,
                Date = DateTime.Today
            }).ToList());

            return rankings;
        }

        public async Task SeedResultsAsync()
        {
            DateTime startDate = DateTime.Today.AddDays(-20);
            IList<SearchResultDbModel> searchResults = new List<SearchResultDbModel>();
            var results = await _searchResultRepository.GetByDate(startDate);
            if (!results.Any())
            {
                var csvPath = Path.Combine(AppContext.BaseDirectory, "land_registry_searches_4week.csv");
                if (File.Exists(csvPath))
                {
                    int idCounter = 1;
                    foreach (var line in File.ReadLines(csvPath).Skip(1))
                    {
                        var columns = line.Split(',');
                        if (columns.Length == 4 && 
                            int.TryParse(columns[0], out int rank) &&
                            DateTime.TryParse(columns[3], out DateTime date))
                        {
                            searchResults.Add(new SearchResultDbModel
                            {
                                Rank = int.Parse(columns[0]),
                                Url = columns[1],
                                Type = columns[2],
                                Date = date
                            });
                        }
                    }

                    if (!searchResults.Any())
                    {
                        _logger.LogInformation("No results found");
                        throw new Exception("No data found in CSV file");
                    }
                    else { 
                        try 
                        {
                            await _searchResultRepository.AddData(searchResults);
                        }                       
                        catch(Exception ex)
                        {
                           throw new Exception("Error", ex);
                        } 
                    }
                    
                }
                else
                {
                    throw new FileNotFoundException("CSV file not found", csvPath);
                }
            }
        }
    }
}

