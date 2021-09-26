using BootstrapBlazor.Components;
using SDAllianceWebSite.Shared.Application.Dtos;
using SDAllianceWebSite.Shared.ViewModel.Admin;
using SDAllianceWebSite.Shared.ViewModel.Favorites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SDAllianceWebSite.APIServer.Application.Favorites
{
    public interface IFavoriteObjectService
    {
        Task<QueryData<ListFavoriteObjectAloneModel>> GetPaginatedResult(QueryPageOptions options, ListFavoriteObjectAloneModel searchModel, long favoriteFolderId = 0);

        Task<PagedResultDto<FavoriteObjectAloneViewModel>> GetPaginatedResult(PagedSortedAndFilterInput input, long favoriteFolderId);
    }
}
