using BootstrapBlazor.Components;
using SDAllianceWebSite.Shared.ViewModel.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SDAllianceWebSite.APIServer.Application.Favorites
{
    public interface IFavoriteFolderService
    {
        Task<QueryData<ListFavoriteFolderAloneModel>> GetPaginatedResult(QueryPageOptions options, ListFavoriteFolderAloneModel searchModel, string userId = "");
    }
}
