using ScraperWapp.BackEnd.Models;
using ScraperWapp.Data.DTOS;

namespace ScraperWapp.Orchestrators;

public interface IOrchestrator
{
    Task<IList<IRankingModel>> CollectResultsAsync();
}