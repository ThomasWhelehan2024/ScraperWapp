namespace ScraperWapp.BackEnd.Models;

public interface IRankingModel
{
    int Rank { get; set; }
    string Url { get; set; }
    string Type { get; set; }
}