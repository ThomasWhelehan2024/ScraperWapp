using ScraperWapp.Data.DTOS;

namespace ScraperWapp.Services
{
    public class AnalysisService
    {
        private readonly ScraperService _scraperService;
        public AnalysisService(ScraperService scraperService)
        {
            _scraperService = scraperService;
        }

        public IList<RankingDto> GetRankings(IList<string> entries)
        {
            var results = new List<RankingDto>();
            try
            {
                for (int i = 0; i < entries.Count; i++)
                {
                    var html = entries[i];
                    var tag = _scraperService.GetOuterHtml(html, "<a class=\"result__url\"");

                    if (tag == null)
                        tag = _scraperService.GetOuterHtml(html, "<a class=\"result__url sep--after");

                    if (tag == null)
                        continue;

                    int start = tag.Html.IndexOf('>') + 1;

                    int end = tag.Html.LastIndexOf("</a>", StringComparison.OrdinalIgnoreCase);

                    var visibleText = tag.Html.Substring(start, end - start).Trim();

                    results.Add(new RankingDto { Rank = i + 1, Url = visibleText });
                }
                return results;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error analyzing results: {ex.Message}");
                return new List<RankingDto>();
            }
        }

        public IList<RankingDto> GetMatchingRankings(IList<RankingDto> rankings, string domain)
        {
            return rankings.Where(r => r.Url.Contains(domain, StringComparison.OrdinalIgnoreCase)).ToList();
        }
    }
}
