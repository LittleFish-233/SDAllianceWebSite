
using SDAllianceWebSite.Shared.Application.Dtos;

namespace SDAllianceWebSite.APIServer.Application.Users.Dtos
{
    public class GetMessageInput : PagedSortedAndFilterInput
    {
        public GetMessageInput()
        {
            Sorting = "Id desc";
            ScreeningConditions = "全部";
        }
    }
}
