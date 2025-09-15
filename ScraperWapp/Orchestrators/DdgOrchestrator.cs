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
        private readonly SearchResultStore _searchResultStore;
        private readonly ILogger<DdgOrchestrator> _logger;

        public DdgOrchestrator(ISearchEngineClient ddgClient, IScraperService scrapingService,
            IAnalysisService analysisService, SearchResultStore searchResultStore)
        {
            _ddgClient = ddgClient;
            _scrapingService = scrapingService;
            _analysisService = analysisService;
            _searchResultStore = searchResultStore;
        }

        
        public async Task<IList<IRankingModel>> CollectResultsAsync()
        {
            IList<SearchResultDbModel> todaysResults = new List<SearchResultDbModel>();
            todaysResults = _searchResultStore.GetByDate(DateTime.Today);

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

            _searchResultStore.AddData(rankings.Select(r => new SearchResultDbModel
            {
                Type = r.Type,
                Rank = r.Rank,
                Url = r.Url,
                Date = DateTime.Today
            }).ToList());

            return rankings;
        }

        
    }
}
   

