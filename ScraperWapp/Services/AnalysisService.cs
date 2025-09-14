using ScraperWapp.BackEnd.Interfaces.Services;
using ScraperWapp.BackEnd.Models;
using ScraperWapp.Data.DTOS;
using ScraperWapp.Orchestrators;

namespace ScraperWapp.Services
{
    public class AnalysisService : IAnalysisService
    {
        private readonly IScraperService _scraperService;
        private readonly ILogger<AnalysisService> _logger;
        public AnalysisService(IScraperService scraperService, ILogger<AnalysisService> logger)
        {
            _scraperService = scraperService;
            _logger = logger;
        }

        public IList<IRankingModel> GetRankings(IList<string> entries)
        {
            IList<IRankingModel> results = new List<IRankingModel>();
            try
            {
                for (int i = 0; i < entries.Count; i++)
                {
                    IRankingModel result = new RankingModel();
                    var html = entries[i];
                    if (html.Contains("result--ad"))
                    {
                        result.Type = "Advert";
                    }

                    var tag = _scraperService.GetOuterHtml(html, "<a class=\"result__url\"");

                    if (tag == null)
                        tag = _scraperService.GetOuterHtml(html, "<a class=\"result__url sep--after");

                    if (tag == null)
                        continue;

                    int start = tag.Html.IndexOf('>') + 1;

                    int end = tag.Html.LastIndexOf("</a>", StringComparison.OrdinalIgnoreCase);

                    var visibleText = tag.Html.Substring(start, end - start).Trim();

                    result.Rank = i + 1;
                    result.Url = visibleText;
                    results.Add(result);
                }
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Error analyzing results: {ex.Message}");
                return new List<IRankingModel>();
            }
        }
        
        public IList<IRankingModel> GetMatchingRankings(IList<IRankingModel> rankings, string domain)
        {
            return rankings.Where(r => r.Url.Contains(domain, StringComparison.OrdinalIgnoreCase)).ToList();
        }
    }
}
