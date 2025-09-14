using ScraperWapp.Data.DTOS;

namespace ScraperWapp.Services;

public interface IScraperService
{
    HtmlBlockModel? GetOuterHtml(string html, string startTag);
    TagModel? FindTag(string html, string startTag);
    bool ValidateForm(string formHtml, IList<string> attributes);
    TagModel ParseForm(string formHtml, IList<string> attributes);
    IList<string> SplitRawEntries(string rawEntries, string pattern);
}