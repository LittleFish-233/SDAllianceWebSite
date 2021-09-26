
using SDAllianceWebSite.Shared.Model;
using SDAllianceWebSite.APIServer.Application.Articles.Dtos;
using SDAllianceWebSite.Shared.Application.Dtos;
using System.Threading.Tasks;
using SDAllianceWebSite.Shared.ViewModel.Admin;
using BootstrapBlazor.Components;
using SDAllianceWebSite.Shared.ViewModel.Search;
using System.Collections.Generic;
using SDAllianceWebSite.Shared.Model.ExamineModel;

namespace SDAllianceWebSite.APIServer.Application.Articles
{
    public interface IArticleService
    {
        Task<PagedResultDto<Article>> GetPaginatedResult(GetArticleInput input);

        Task<QueryData<ListArticleAloneModel>> GetPaginatedResult(QueryPageOptions options, ListArticleAloneModel searchModel);

        Task<PagedResultDto<ArticleInforTipViewModel>> GetPaginatedResult(PagedSortedAndFilterInput input);

        void UpdateArticleDataMain(Article article, ArticleMain examine);

        void UpdateArticleDataRelevances(Article article, ArticleRelecancesModel examine);

        void UpdateArticleDataMainPage(Article article, string examine);

        void UpdateArticleData(Article article, Examine examine);
    }
}
