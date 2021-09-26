using SDAllianceWebSite.Shared.Application.Dtos;

namespace SDAllianceWebSite.APIServer.ExamineX
{
    public class GetExamineInput : PagedSortedAndFilterInput
    {
        public GetExamineInput()
        {
            Sorting = "Id";
            ScreeningConditions = "待审核";
        }
    }
}
