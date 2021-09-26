using SDAllianceWebSite.Shared.Model;
using SDAllianceWebSite.Shared.Application.Dtos;
using System.Threading.Tasks;
using SDAllianceWebSite.Shared.ViewModel.Admin;
using BootstrapBlazor.Components;
using SDAllianceWebSite.Shared.Model.ExamineModel;
using SDAllianceWebSite.Shared.ExamineModel;

namespace SDAllianceWebSite.APIServer.ExamineX
{
    public interface IExamineService
    {
        Task<PagedResultDto<ExaminedNormalListModel>> GetPaginatedResult(GetExamineInput input, string userId = "");

        Task<QueryData<ListExamineAloneModel>> GetPaginatedResult(QueryPageOptions options, ListExamineAloneModel searchModel);

        Task<bool> GetExamineView(Models.ExaminedViewModel model, Examine examine);
       
        /// <summary>
        /// 获取当前用户对该 文章 部分的待审核记录
        /// </summary>
        /// <param name="articleId">文章Id</param>
        /// <param name="userId">用户Id</param>
        /// <param name="operation">用操作表示部分</param>
        /// <returns>审核记录</returns>
        Task<Examine> GetUserArticleActiveExamineAsync(long articleId, string userId, Operation operation);
        /// <summary>
        /// 处理 EditArticleMain 审核成功后调用更新数据
        /// </summary>
        /// <param name="article">关联文章</param>
        /// <param name="examine">审核数据模型</param>
        /// <returns></returns>
        Task ExamineEditArticleMainAsync(Article article, ArticleMain examine);
        /// <summary>
        /// 文章 创建 数据处理完毕后调用该方法 通用
        /// </summary>
        /// <param name="article">文章</param>
        /// <param name="user">用户</param>
        /// <param name="isAdmin">是否为管理员</param>
        /// <param name="examineStr">序列化的审核数据字符串</param>
        /// <param name="operation">用操作表示部分</param>
        /// <param name="note">备注</param>
        /// <returns></returns>
        Task<bool> UniversalCreateArticleExaminedAsync(Article article, ApplicationUser user, bool isAdmin, string examineStr, Operation operation, string note);
        /// <summary>
        /// 文章 编辑 数据处理完毕后调用该方法 通用
        /// </summary>
        /// <param name="article">文章</param>
        /// <param name="user">用户</param>
        /// <param name="isAdmin">是否为管理员</param>
        /// <param name="examineStr">序列化的审核数据字符串</param>
        /// <param name="operation">用操作表示部分</param>
        /// <param name="note">备注</param>
        /// <returns></returns>
        Task UniversalEditArticleExaminedAsync(Article article, ApplicationUser user, bool isAdmin, string examineStr, Operation operation, string note);
        /// <summary>
        /// 处理 EditArticleRelevanes 审核成功后调用更新数据
        /// </summary>
        /// <param name="article">关联文章</param>
        /// <param name="articleRelecancesModel">审核数据模型</param>
        /// <returns></returns>
        Task ExamineEditArticleRelevancesAsync(Article article, ArticleRelecancesModel articleRelecancesModel);
        /// <summary>
        /// 处理 EditArticleMainPage 审核成功后调用更新数据
        /// </summary>
        /// <param name="article">关联文章</param>
        /// <param name="examine">审核数据模型</param>
        /// <returns></returns>
        Task ExamineEditArticleMainPageAsync(Article article, string examine);

        /// <summary>
        /// 为批量导入的文章建立审核记录
        /// </summary>
        /// <param name="model">文章</param>
        /// <param name="user">用户</param>
        /// <param name="note">备注</param>
        /// <returns></returns>
        Task AddBatchArticleExaminesAsync(Article model, ApplicationUser user, string note);
      
        /// <summary>
        /// 处理 EditUserMain 审核成功后调用更新数据
        /// </summary>
        /// <param name="user">关联用户</param>
        /// <param name="examine">审核数据模型</param>
        /// <returns></returns>
        Task ExamineEditUserMainAsync(ApplicationUser user, UserMain examine);
        /// <summary>
        /// 处理 UserMainPage 审核成功后调用更新数据
        /// </summary>
        /// <param name="user">关联用户</param>
        /// <param name="examine">审核数据模型</param>
        /// <returns></returns>
        Task ExamineEditUserMainPageAsync(ApplicationUser user, string examine);
        /// <summary>
        /// 用户信息 编辑 数据处理完毕后调用该方法 通用
        /// </summary>
        /// <param name="user">用户</param>
        /// <param name="isAdmin">是否为管理员</param>
        /// <param name="examineStr">序列化的审核数据字符串</param>
        /// <param name="operation">用操作表示部分</param>
        /// <param name="note">备注</param>
        /// <returns></returns>
        Task UniversalEditUserExaminedAsync(ApplicationUser user, bool isAdmin, string examineStr, Operation operation, string note);
        /// <summary>
        /// 获取当前用户对该 用户信息 部分的待审核记录
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <param name="operation">用操作表示部分</param>
        /// <returns>审核记录</returns>
        Task<Examine> GetUserInforActiveExamineAsync(string userId, Operation operation);
    }
}
