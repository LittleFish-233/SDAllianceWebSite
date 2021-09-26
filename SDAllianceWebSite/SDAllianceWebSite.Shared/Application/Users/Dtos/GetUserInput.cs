using SDAllianceWebSite.Shared.Application.Dtos;

namespace SDAllianceWebSite.Shared.Application.Users.Dtos
{
    public class GetUserInput : PagedSortedAndFilterInput
    {
        public GetUserInput()
        {
            Sorting = "Id";
            ScreeningConditions = "全部";
        }
    }
}
