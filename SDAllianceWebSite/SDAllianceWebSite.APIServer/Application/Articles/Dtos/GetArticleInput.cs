﻿using SDAllianceWebSite.Shared.Application.Dtos;

namespace SDAllianceWebSite.APIServer.Application.Articles.Dtos
{
    public class GetArticleInput : PagedSortedAndFilterInput
    {
        public GetArticleInput()
        {
            Sorting = "Id";
            ScreeningConditions = "全部";
        }
    }
}
