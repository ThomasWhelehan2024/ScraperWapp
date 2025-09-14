using ScraperWapp.BackEnd.Models;

namespace ScraperWapp.BackEnd.Interfaces.Services;

public interface IAnalysisService
{
    IList<IRankingModel> GetRankings(IList<string> entries);
    IList<IRankingModel> GetMatchingRankings(IList<IRankingModel> rankings, string domain);
}
