using SDAllianceWebSite.Shared.Application.Dtos;

namespace SDAllianceWebSite.Shared.Application.Examines.Dtos
{
    public class GetExamineInput : PagedSortedAndFilterInput
    {
        public GetExamineInput()
        {
            Sorting = "Id desc";
            ScreeningConditions = "待审核";
        }
    }
}
