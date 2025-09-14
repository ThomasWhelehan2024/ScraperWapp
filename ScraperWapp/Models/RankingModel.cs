namespace ScraperWapp.BackEnd.Models;

public class RankingModel : IRankingModel
{
    public int Rank { get; set; }
    public string Url { get; set; }
    public string Type { get; set; } = "Organic";
}