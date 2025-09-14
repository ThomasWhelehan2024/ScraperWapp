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


        public TagDto? FindTag(string html, string startTag)
        {
            int pos = 0;

            while (pos < html.Length)
            {
                // Get the next <form> block
                var block = GetOuterHtml(html.Substring(pos), startTag);
                if (block == null) break;

                var metaFormAttributes = new List<string> { "q", "s","v","o"
                                                                 ,"dc","api","vqd" };
                // Parse the form into a MetaForm object
                bool isMetaForm = ValidateForm(block.Html, metaFormAttributes);

                if (isMetaForm)
                {
                    var form = ParseForm(block.Html, metaFormAttributes);
                    return form;
                }

                // Check if this form matches the predicate


                pos += block.EndIndex;
            }

            return null; // No matching form found
        }
        public bool ValidateForm(string formHtml, IList<string> attributes)
        {
            foreach (var attr in attributes)
            {
                if (formHtml.Contains($"name=\"{attr}\""))
                {
                    continue;
                }
                else { return false; }
            }
            return true;
        }

        public TagDto ParseForm(string formHtml, IList<string> attributes)
        {
            var metaForm = new TagDto();

            var splitForm = formHtml.Split(new[] { "<", ">" }, StringSplitOptions.RemoveEmptyEntries);

            if (splitForm.Length == 0)
                return metaForm;


            metaForm.Action = Regex.Match(splitForm[0], @"action\s*=\s*[""']([^""']+)[""']", RegexOptions.IgnoreCase).Groups[1].Value;
            metaForm.Method = Regex.Match(splitForm[0], @"method\s*=\s*[""']([^""']+)[""']", RegexOptions.IgnoreCase).Groups[1].Value;

            foreach (var att in attributes)
            {
                var inputMatch = splitForm.FirstOrDefault(s => s.Contains($"name=\"{att}\""));
                if (inputMatch != null)
                {
                    var nameMatch = Regex.Match(inputMatch, @"name\s*=\s*[""']([^""']+)[""']", RegexOptions.IgnoreCase);
                    var valueMatch = Regex.Match(inputMatch, @"value\s*=\s*[""']([^""']*)[""']", RegexOptions.IgnoreCase);
                    if (nameMatch.Success)
                    {
                        metaForm.Inputs.Add(new MetaFormInputDto
                        {
                            Name = nameMatch.Groups[1].Value,
                            Value = valueMatch.Success ? valueMatch.Groups[1].Value : null
                        });
                    }
                }
            }
            return metaForm;

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

       
        
    }
}
