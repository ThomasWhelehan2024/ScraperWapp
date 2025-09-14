using ScraperWapp.Data.DTOS;

namespace ScraperWapp.BackEnd.Interfaces.Adapters;

public interface ISearchEngineClient
{
    Task<string> GetHtmlAsync(string url, ITagModel? form = null);
    Task<List<string>> FetchPagesAsync(string url);
}