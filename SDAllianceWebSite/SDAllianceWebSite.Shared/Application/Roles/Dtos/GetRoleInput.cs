using SDAllianceWebSite.Shared.Application.Dtos;

namespace SDAllianceWebSite.Shared.Application.Roles.Dtos
{
    public class GetRoleInput : PagedSortedAndFilterInput
    {
        public GetRoleInput()
        {
            Sorting = "Id";
            ScreeningConditions = "全部";
        }
    }

}
