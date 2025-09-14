using Microsoft.AspNetCore.Mvc.Routing;
using ScraperWapp.BackEnd.Interfaces.Adapters;
using ScraperWapp.Data.DTOS;
using ScraperWapp.Helpers;
using ScraperWapp.Services;

namespace ScraperWapp.Adapter
{
    public class DuckDuckGoClient : ISearchEngineClient
    {
        private readonly HttpClient _httpClient;
        private readonly IScraperService _scrapingService;

        public DuckDuckGoClient(IHttpClientFactory httpClientFactory, IScraperService scrapingService)
        {
            _scrapingService = scrapingService;
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/139.0.0.0 Safari/537.36");
            _httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml");
            _httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-GB,en;q=0.9");
        }

        public async Task<string> GetHtmlAsync(string url, ITagModel? form = null)
        {
            HttpResponseMessage response;
            if (form != null)
            {
                var inputDictionary = form.Inputs.ToDictionary(i => i.Name, i => i.Value ?? "");

                url = CustomUrlHelper.BuildGetUrl(url, inputDictionary);
                response = await _httpClient.GetAsync(url);
            }
            else
            {
                var inputDictionary = new Dictionary<string, string> { { "q", "land registry searches software" } };
                url = CustomUrlHelper.BuildGetUrl(url, inputDictionary);
                response = await _httpClient.GetAsync(url);
            }
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<List<string>> FetchPagesAsync(string url)
        {
            var html = await GetHtmlAsync(url);

            var pagesHtml = new List<string>();

            pagesHtml.Add(html);

            var pageMetaData = _scrapingService.FindTag(html, "<form");

            if (pageMetaData == null)
                throw new Exception("Could not find search form");

            int pageOffset = pageMetaData.Inputs.Where(i => i.Name == "s")
                                                .Select(pageMetaData => int.TryParse(pageMetaData.Value, out var s) ? s : 0)
                                                .FirstOrDefault();
            int dcOffset = pageMetaData.Inputs.Where(i => i.Name == "dc")
                                                .Select(pageMetaData => int.TryParse(pageMetaData.Value, out var dc) ? dc : 0)
                                                .FirstOrDefault();

            while (pageOffset <= 101)
            {
                var sInput = pageMetaData.Inputs.FirstOrDefault(i => i.Name == "s");
                if (sInput != null)
                    sInput.Value = pageOffset.ToString();

                var dcInput = pageMetaData.Inputs.FirstOrDefault(i => i.Name == "dc");
                if (dcInput != null)
                    dcInput.Value = dcOffset.ToString();


                html = await GetHtmlAsync(url, pageMetaData);
                pagesHtml.Add(html);

                if (pageMetaData == null)
                    break;

                pageOffset += 15;
                dcOffset += 17;

                // Wait 1–2 seconds to avoid bot detection
                await Task.Delay(Random.Shared.Next(2000, 5000));

            }
            // Update offset 's

            return pagesHtml;
        }
    }
}
