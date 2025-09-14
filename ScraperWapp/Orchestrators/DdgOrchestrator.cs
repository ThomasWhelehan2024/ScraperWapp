using ScraperWapp.Adapter;
using ScraperWapp.Data.DTOS;
using ScraperWapp.Services;
using System.Collections.Generic;
using System.Globalization;
using ScraperWapp.Data;
using ScraperWapp.Data.Database;

namespace ScraperWapp.Orchestrators
{
    public class DdgOrchestrator
    {
        private readonly DuckDuckGoClient _ddgClient;
        private readonly ScraperService _scrapingService;
        private readonly AnalysisService _analysisService;
        private readonly SearchResultRepository _searchResultRepository;

        public DdgOrchestrator(DuckDuckGoClient ddgClient, ScraperService scrapingService,
            AnalysisService analysisService, SearchResultRepository searchResultRepository)
        {
            _ddgClient = ddgClient;
            _scrapingService = scrapingService;
            _analysisService = analysisService;
            _searchResultRepository = searchResultRepository;
        }

        public async Task<IList<RankingDto>> CollectResultsAsync()
        {
            IList<SearchResultDb> todaysResults = new List<SearchResultDb>();
            todaysResults = await _searchResultRepository.GetByDate(DateTime.Today);

            if (todaysResults.Any())
            {
                return todaysResults.Select(r => new RankingDto
                {
                    Type = r.Type,
                    Rank = r.Rank,
                    Url = r.Url,
                }).OrderByDescending(r => r.Rank).ToList();
            }

            var pages = await _ddgClient.FetchPagesAsync("https://duckduckgo.com/html/");

            string startDiv = "<div class=\"serp__results\">";
            string metaDataDiv = @"<form action=""/html/"" method=""post"">";

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

            await _searchResultRepository.AddData(rankings.Select(r => new SearchResultDb
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
            IList<SearchResultDb> searchResults = new List<SearchResultDb>();
            var results = await _searchResultRepository.GetByDate(startDate);
            if (!results.Any())
            {
                var csvPath = Path.Combine(AppContext.BaseDirectory, "land_registry_searches_30days.csv");
                if (File.Exists(csvPath))
                {
                    int idCounter = 1;
                    foreach (var line in File.ReadLines(csvPath).Skip(1))
                    {
                        var columns = line.Split(',');
                        if (columns.Length == 4)
                        {
                            searchResults.Add(new SearchResultDb
                            {
                                Rank = int.Parse(columns[0]),
                                Url = columns[1],
                                Type = columns[2],
                                Date = DateTime.Parse(columns[3])
                            });
                        }
                    }

                    if (searchResults.Any())
                    {
                        await _searchResultRepository.AddData(searchResults);
                    }
                }
            }
        }
    }
}

