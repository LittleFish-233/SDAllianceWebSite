using SDAllianceWebSite.Shared.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SDAllianceWebSite.APIServer.Application.Comments.Dtos
{
    public class GetCommentInput : PagedSortedAndFilterInput
    {
        public GetCommentInput()
        {
            Sorting = "Id desc";
            ScreeningConditions = "全部";
        }
    }
}
