using BootstrapBlazor.Components;
using SDAllianceWebSite.APIServer.Application.Users.Dtos;
using SDAllianceWebSite.Shared.Application.Dtos;
using SDAllianceWebSite.Shared.Model;
using SDAllianceWebSite.Shared.ViewModel.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SDAllianceWebSite.APIServer.Application.Messages
{
    public interface IMessageService
    {
        Task<PagedResultDto<Shared.Model.Message>> GetPaginatedResult(GetMessageInput input, string userId);

        Task<QueryData<ListMessageAloneModel>> GetPaginatedResult(QueryPageOptions options, ListMessageAloneModel searchModel);
    }
}
