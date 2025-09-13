using ScraperWapp.Adapter;
using ScraperWapp.Data.DTOS;
using ScraperWapp.Services;
using System.Collections.Generic;

namespace ScraperWapp.Orchestrators
{
    public class DdgOrchestrator
    {
        private readonly DuckDuckGoClient _ddgClient;
        private readonly ScraperService _scrapingService;
        private readonly AnalysisService _analysisService;
        public DdgOrchestrator(DuckDuckGoClient ddgClient, ScraperService scrapingService, AnalysisService analysisService)
        {
            _ddgClient = ddgClient;
            _scrapingService = scrapingService;
            _analysisService = analysisService;
        }

        public async Task<IList<RankingDto>> CollectResultsAsync()
        {
            var pages = await _ddgClient.FetchPagesAsync("https://duckduckgo.com/html/?q=land+registry+searches+software");

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
