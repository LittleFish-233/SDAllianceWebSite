﻿
using SDAllianceWebSite.Shared.Application.Dtos;

namespace SDAllianceWebSite.APIServer.Application.Search.Dtos
{
    public class GetSearchInput : PagedSortedAndFilterInput
    {
        public GetSearchInput()
        {
            Sorting = "Id";
            ScreeningConditions = "全部";
            MaxResultCount = 4;
        }
    }
}
