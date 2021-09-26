using BootstrapBlazor.Components;
using SDAllianceWebSite.Shared.ViewModel.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SDAllianceWebSite.APIServer.Application.ErrorCounts
{
    public interface IErrorCountService
    {
        Task<QueryData<ListErrorCountAloneModel>> GetPaginatedResult(QueryPageOptions options, ListErrorCountAloneModel searchModel);
    }
}
