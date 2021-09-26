
using SDAllianceWebSite.Shared.Application.Dtos;
using SDAllianceWebSite.Shared.Application.Search.Dtos;
using SDAllianceWebSite.Shared.ViewModel.Home;
using System.Threading.Tasks;

namespace SDAllianceWebSite.APIServer.Application.Search
{
    public interface ISearchService
    {
        Task<PagedResultDto<SearchAloneModel>> GetPaginatedResult(GetSearchInput input);
    }
}
