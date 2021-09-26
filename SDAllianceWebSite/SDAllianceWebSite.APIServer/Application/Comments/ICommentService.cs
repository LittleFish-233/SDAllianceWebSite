using BootstrapBlazor.Components;
using SDAllianceWebSite.APIServer.Application.Comments.Dtos;
using SDAllianceWebSite.Shared.Application.Dtos;
using SDAllianceWebSite.Shared.Model;
using SDAllianceWebSite.Shared.ViewModel.Admin;
using SDAllianceWebSite.Shared.ViewModel.Coments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SDAllianceWebSite.APIServer.Application.Comments
{
    public interface ICommentService
    {
        Task<PagedResultDto<CommentViewModel>> GetPaginatedResult(GetCommentInput input, CommentType type, string Id, string rankName, string ascriptionUserId);
        Task<QueryData<ListCommentAloneModel>> GetPaginatedResult(QueryPageOptions options, ListCommentAloneModel searchModel, CommentType type = CommentType.None, string objectId = "");
    }
}
