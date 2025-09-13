using ScraperWapp.Data.DTOS;
using System.Text.RegularExpressions;

namespace ScraperWapp.Services
{
    public class ScraperService
    {
        public ScraperService()
        {

        }
        public HtmlBlockDto? GetOuterHtml(string html, string startTag)
        {
            var tagNameMatch = Regex.Match(startTag, @"<\s*(\w+)", RegexOptions.IgnoreCase);

            if (!tagNameMatch.Success)
                return null;

            string tagName = tagNameMatch.Groups[1].Value;

            int startIndex = html.IndexOf(startTag, StringComparison.OrdinalIgnoreCase);

            if (startIndex == -1)
                return null;

            int pos = startIndex + startTag.Length;
            int depth = 1;

            string openTag = "<" + tagName;
            string closeTag = "</" + tagName + ">";

            while (depth > 0 && pos < html.Length)
            {
                int nextOpen = html.IndexOf(openTag, pos, StringComparison.OrdinalIgnoreCase);
                int nextClose = html.IndexOf(closeTag, pos, StringComparison.OrdinalIgnoreCase);

                if (nextClose == -1)
                    return null; // malformed HTML

                if (nextOpen != -1 && nextOpen < nextClose)
                {
                    depth++;
                    pos = nextOpen + openTag.Length;
                }
                else
                {
                    depth--;
                    pos = nextClose + closeTag.Length;
                }
            }

            // Grab from the opening tag to the closing tag
            return new HtmlBlockDto
            {
                Html = html.Substring(startIndex, pos - startIndex),
                EndIndex = pos
            };
        }


        public TagDto? FindTag(string html, string startTag, Func<TagDto, bool> predicate)
        {
            int pos = 0;

            while (pos < html.Length)
            {
                // Get the next <form> block
                var block = GetOuterHtml(html.Substring(pos), startTag);
                if (block == null) break;

                // Parse the form into a MetaForm object
                var form = ParseForm(block.Html);

                // Check if this form matches the predicate
                if (predicate(form))
                {
                    return form; // Found the one we want
                }

                pos += block.EndIndex;
            }

            return null; // No matching form found
        }

        public IList<string> SplitRawEntries(string rawEntries, string pattern)
        {
            if (string.IsNullOrEmpty(rawEntries))
                return new List<string>();

            var regex = new Regex(pattern, RegexOptions.IgnoreCase);
            var matches = regex.Matches(rawEntries);

            var results = new List<string>();

            foreach (Match match in matches)
            {
                // start from the match onwards
                string entry = rawEntries.Substring(match.Index);

                // get the actual opening tag (could be div/ol/span/etc)
                string startTag = match.Value;

                HtmlBlockDto block = GetOuterHtml(entry, startTag);
                if (!string.IsNullOrWhiteSpace(block.Html))
                    results.Add(block.Html);
            }

            return results;
        }

        public TagDto ParseForm(string formHtml)
        {
            var metaForm = new TagDto();

            var formMatch = Regex.Match(
                formHtml,
                @"<form[^>]*action\s*=\s*[""']([^""']+)[""'][^>]*method\s*=\s*[""']([^""']+)[""']",
                RegexOptions.IgnoreCase
            );

            if (formMatch.Success)
            {
                metaForm.Action = formMatch.Groups[1].Value;
                metaForm.Method = formMatch.Groups[2].Value;
            }

            var inputMatches = Regex.Matches(
                formHtml,
                @"<input\b[^>]*name\s*=\s*[""']([^""']+)[""'][^>]*value\s*=\s*[""']([^""']*)[""'][^>]*>",
                RegexOptions.IgnoreCase
            );

            foreach (Match match in inputMatches)
            {
                metaForm.Inputs.Add(new MetaFormInputDto
                {
                    Name = match.Groups[1].Value,
                    Value = match.Groups[2].Value
                });
            }
            return metaForm;
        }
    }
}
