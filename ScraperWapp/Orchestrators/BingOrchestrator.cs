using ScraperWapp.Adapter;
using ScraperWapp.Data.DTOS;
using ScraperWapp.Services;

namespace ScraperWapp.Orchestrators
{
    public class BingOrchestrator
    {
        private readonly BingClient _bingClient;
        private readonly ScraperService _scrapingService;
        private readonly AnalysisService _analysisService;
        public BingOrchestrator(BingClient bingClient, ScraperService scrapingService, AnalysisService analysisService)
        {
            _bingClient = bingClient;
            _scrapingService = scrapingService;
            _analysisService = analysisService;
        }

        public async Task<IList<RankingDto>> CollectResultsAsync()
        {
            var pages = await _bingClient.FetchPagesAsync("www.google.co.uk/search?num=100&q=land+registry+search");

            string startDiv = "<div class=\"serp__results\">";
            string metaDataDiv = @"<form action=""/html/"" method=""post"">";

            List<string> entries = new List<string>();

            foreach (var page in pages)
            {
                var rawEntries = _scrapingService.GetOuterHtml(page, startDiv);
                entries.AddRange(_scrapingService.SplitRawEntries(rawEntries.Html, @"<div class=""result results_links results_links_deep\s+[^""]+"">"));
            }
            entries = entries.GetRange(0, Math.Min(100, entries.Count)).ToList();

            var rankings = _analysisService.GetRankings(entries);

            return rankings;
        }
    }
}
