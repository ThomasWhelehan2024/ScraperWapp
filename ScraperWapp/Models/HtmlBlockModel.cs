namespace ScraperWapp.Data.DTOS
{
    public class HtmlBlockModel : IHtmlBlockModel
    {
        public string Html { get; set; }
        public int EndIndex { get; set; }
    }
}
