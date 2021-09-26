using SDAllianceWebSite.Shared.Application.Dtos;
using SDAllianceWebSite.Shared.Application.Examines.Dtos;
using SDAllianceWebSite.Shared.Model;
using SDAllianceWebSite.Shared.ViewModel.Admin;
using System.Collections.Generic;
using static SDAllianceWebSite.Shared.Application.Examines.ExamineService;

namespace SDAllianceWebSite.Shared.Application.Examines
{
    public interface IExamineService
    {
        public PagedResultDto<ExaminedNormalListModel> GetPaginatedResult(GetExamineInput input, List<ExaminedNormalListModel> examines, GetExaminePagedType type);
    }
}
