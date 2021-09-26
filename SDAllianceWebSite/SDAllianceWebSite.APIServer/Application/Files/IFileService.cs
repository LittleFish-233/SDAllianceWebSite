using BootstrapBlazor.Components;
using SDAllianceWebSite.Shared.Application.Dtos;
using SDAllianceWebSite.Shared.ViewModel.Admin;
using SDAllianceWebSite.Shared.ViewModel.Files;
using SDAllianceWebSite.Shared.ViewModel.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SDAllianceWebSite.APIServer.Application.Files
{
    public interface IFileService
    {
        Task<PagedResultDto<ImageInforTipViewModel>> GetPaginatedResult(PagedSortedAndFilterInput input);

        Task<QueryData<ListFileAloneModel>> GetPaginatedResult(QueryPageOptions options, ListFileAloneModel searchModel);

    }
}
