using SDAllianceWebSite.Shared.Model;
using SDAllianceWebSite.APIServer.DataReositories;
using SDAllianceWebSite.Shared.Application.Dtos;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using SDAllianceWebSite.APIServer.Application.Helper;
using BootstrapBlazor.Components;
using SDAllianceWebSite.Shared.ViewModel.Admin;
using System;
using System.Collections.Concurrent;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Information;
using SDAllianceWebSite.Shared.ExamineModel;
using SDAllianceWebSite.Shared.Model.ExamineModel;
using SDAllianceWebSite.Shared.ViewModel;
using System.IO;
using Newtonsoft.Json;
using SDAllianceWebSite.Shared.Helper;
using Markdig;
using Markdown = Markdig.Markdown;
using SDAllianceWebSite.APIServer.Application.Articles;
using SDAllianceWebSite.Shared.Pages.Admin;
using SDAllianceWebSite.APIServer.Application.Users;
using SDAllianceWebSite.Shared.ViewModel.Articles;

namespace SDAllianceWebSite.APIServer.ExamineX
{
    public class ExamineService : IExamineService
    {
        private readonly IRepository<Examine, int> _examineRepository;
        private readonly IAppHelper _appHelper;
        private readonly IRepository<Article, long> _articleRepository;
        private readonly IRepository<Comment, long> _commentRepository;
        private readonly IRepository<ApplicationUser, string> _userRepository;
        private readonly IArticleService _articleService;
        private readonly IUserService _userService;

        private static readonly ConcurrentDictionary<Type, Func<IEnumerable<Examine>, string, SortOrder, IEnumerable<Examine>>> SortLambdaCacheEntry = new();

        public ExamineService(IRepository<Examine, int> examineRepository, IAppHelper appHelper,
            IArticleService articleService,  IUserService userService, IRepository<ApplicationUser, string> userRepository,
        IRepository<Article, long> articleRepository, IRepository<Comment, long> commentRepository)
        {
            _examineRepository = examineRepository;
            _appHelper = appHelper;
            _articleRepository = articleRepository;
            _commentRepository = commentRepository;
            _articleService = articleService;
            _userService = userService;
            _userRepository = userRepository;
        }

        public async Task<PagedResultDto<ExaminedNormalListModel>> GetPaginatedResult(GetExamineInput input,  string userId = "")
        {
            IQueryable<Examine> query = null;
            if (string.IsNullOrWhiteSpace(userId) == false)
            {
                query = _examineRepository.GetAll().AsNoTracking().Where(s => s.ApplicationUserId == userId);

            }
            else
            {
                query = _examineRepository.GetAll().AsNoTracking();
            }

            //判断是否是条件筛选
            if (!string.IsNullOrWhiteSpace(input.ScreeningConditions))
            {
                switch (input.ScreeningConditions)
                {
                    case "待审核":
                        query = query.Where(s => s.IsPassed == null);
                        break;
                    case "已通过":
                        query = query.Where(s => s.IsPassed == true);
                        break;
                    case "未通过":
                        query = query.Where(s => s.IsPassed == false);
                        break;

                }
            }
            //判断输入的查询名称是否为空
            if (!string.IsNullOrWhiteSpace(input.FilterText))
            {
                //尝试将查询翻译成操作
                Operation operation = Operation.None;
                switch (input.FilterText)
                {
                    case "修改用户主页":
                        operation = Operation.UserMainPage;
                        break;
                    case "编辑文章主要信息":
                        operation = Operation.EditArticleMain;
                        break;
                    case "编辑文章关联词条":
                        operation = Operation.EditArticleRelevanes;
                        break;
                    case "编辑文章内容":
                        operation = Operation.EditArticleMainPage;
                        break;
                }
                query = query.Where(s => s.ApplicationUser.UserName.Contains(input.FilterText)
                  || s.Context.Contains(input.FilterText)
                  || (s.Article != null && s.Article.Name.Contains(input.FilterText))
                  || s.Operation == operation);
            }
            //统计查询数据的总条数
            var count = query.Count();
            //根据需求进行排序，然后进行分页逻辑的计算
            query = query.OrderBy(input.Sorting).Skip((input.CurrentPage - 1) * input.MaxResultCount).Take(input.MaxResultCount);

            //将结果转换为List集合 加载到内存中
            List<ExaminedNormalListModel> models = null;
            if (count != 0)
            {
                models = await _appHelper.GetExaminesToNormalListAsync(query);
            }
            else
            {
                models = new List<ExaminedNormalListModel>();
            }
            var dtos = new PagedResultDto<ExaminedNormalListModel>
            {
                TotalCount = count,
                CurrentPage = input.CurrentPage,
                MaxResultCount = input.MaxResultCount,
                Data = models,
                FilterText = input.FilterText,
                Sorting = input.Sorting,
                ScreeningConditions = input.ScreeningConditions
            };
            return dtos;
        }

        public Task<QueryData<ListExamineAloneModel>> GetPaginatedResult(QueryPageOptions options, ListExamineAloneModel searchModel)
        {
            IEnumerable<Examine> items = _examineRepository.GetAll().AsNoTracking();
            // 处理高级搜索

            if (!string.IsNullOrWhiteSpace(searchModel.ArticleId?.ToString()))
            {
                items = items.Where(item => item.ArticleId.ToString()?.Contains(searchModel.ArticleId.ToString(), StringComparison.OrdinalIgnoreCase) ?? false);
            }


            if (!string.IsNullOrWhiteSpace(searchModel.ApplicationUserId?.ToString()))
            {
                items = items.Where(item => item.ApplicationUserId.ToString()?.Contains(searchModel.ApplicationUserId.ToString(), StringComparison.OrdinalIgnoreCase) ?? false);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.Comments))
            {
                items = items.Where(item => item.Comments?.Contains(searchModel.Comments, StringComparison.OrdinalIgnoreCase) ?? false);
            }
            if (searchModel.Operation != null)
            {
                items = items.Where(item => item.Operation == searchModel.Operation);
            }


            // 处理 Searchable=true 列与 SeachText 模糊搜索
            if (options.Searchs.Any())
            {

                // items = items.Where(options.Searchs.GetFilterFunc<Entry>(FilterLogic.Or));
            }
            else
            {
                // 处理 SearchText 模糊搜索
                if (!string.IsNullOrWhiteSpace(options.SearchText))
                {
                    items = items.Where(item => (item.ArticleId.ToString()?.Contains(options.SearchText) ?? false)
                                 || (item.ApplicationUserId.ToString()?.Contains(options.SearchText) ?? false));
                }
            }
            // 过滤
            /* var isFiltered = false;
             if (options.Filters.Any())
             {
                 items = items.Where(options.Filters.GetFilterFunc<Entry>());
                 isFiltered = true;
             }*/

            // 排序
            var isSorted = false;
            if (!string.IsNullOrWhiteSpace(options.SortName))
            {
                // 外部未进行排序，内部自动进行排序处理
                var invoker = SortLambdaCacheEntry.GetOrAdd(typeof(Examine), key => LambdaExtensions.GetSortLambda<Examine>().Compile());
                items = invoker(items, options.SortName, options.SortOrder);
                isSorted = true;
            }

            // 设置记录总数
            var total = items.Count();

            // 内存分页
            items = items.Skip((options.PageIndex - 1) * options.PageItems).Take(options.PageItems).ToList();

            //复制数据
            List<ListExamineAloneModel> resultItems = new List<ListExamineAloneModel>();
            foreach (var item in items)
            {
                resultItems.Add(new ListExamineAloneModel
                {
                    Id = item.Id,
                    Operation = item.Operation,
                    IsPassed = item.IsPassed,
                    PassedTime = item.PassedTime,
                    ApplyTime = item.ApplyTime,
                    Comments = item.Comments,
                    ApplicationUserId = item.ApplicationUserId,
                    CommentId = item.CommentId,
                    PassedAdminName = item.PassedAdminName,
                    ArticleId = item.ArticleId,
                    ContributionValue = item.ContributionValue
                });
            }

            return Task.FromResult(new QueryData<ListExamineAloneModel>()
            {
                Items = resultItems,
                TotalCount = total,
                IsSorted = isSorted,
                // IsFiltered = isFiltered
            });
        }



        public async Task<bool> GetExamineView(Models.ExaminedViewModel model, Examine examine)
        {
            switch (model.Operation)
            {
                case Operation.UserMainPage:
                   return GetUserMainPageExamineView(model, examine);
                case Operation.EditUserMain:
                    return await GetEditUserMainExamineView(model, examine);
              
                case Operation.EditArticleMain:
                    return await GetEditArticleMainExamineView(model, examine);
                case Operation.EditArticleRelevanes:
                    return await GetEditArticleRelevanesExamineView(model, examine);
                case Operation.EditArticleMainPage:
                    return await GetEditArticleMainPageExamineView(model, examine);
          
                case Operation.PubulishComment:
                    return await GetPubulishCommentExamineView(model, examine);
          
            }
            return false;
        }


        #region 获取审核记录视图


        #region 文章
        public async Task<bool> GetEditArticleMainExamineView(Models.ExaminedViewModel model, Examine examine)
        {
            model.Type = "文章";
            Article article = await _articleRepository.FirstOrDefaultAsync(s => s.Id == examine.ArticleId);
            if (article == null)
            {
                return false;
            }
            model.EntryId = article.Id;
            model.EntryName = article.Name;
            //序列化数据
            ArticleMain articleMain = null;
            using (TextReader str = new StringReader(examine.Context))
            {
                JsonSerializer serializer = new JsonSerializer();
                articleMain = (ArticleMain)serializer.Deserialize(str, typeof(ArticleMain));
            }

            ArticleMain articleMainBefore = new ArticleMain
            {
                Name = article.Name,
                BriefIntroduction = article.BriefIntroduction,
                MainPicture = _appHelper.GetImagePath(article.MainPicture, "app.png"),
                BackgroundPicture = _appHelper.GetImagePath(article.BackgroundPicture, "app.png"),
                SmallBackgroundPicture = _appHelper.GetImagePath(article.SmallBackgroundPicture, "app.png"),
                Type = article.Type,
                OriginalLink = article.OriginalLink,
                OriginalAuthor = article.OriginalAuthor,
                PubishTime = article.PubishTime,
                DisplayName = article.DisplayName
            };
            articleMain.MainPicture = _appHelper.GetImagePath(articleMain.MainPicture, "app.png");
            articleMain.BackgroundPicture = _appHelper.GetImagePath(articleMain.BackgroundPicture, "app.png");
            articleMain.SmallBackgroundPicture = _appHelper.GetImagePath(articleMain.SmallBackgroundPicture, "app.png");
            //判断是否是等待审核状态
            if (examine.IsPassed != null)
            {
                HtmlDiff.HtmlDiff htmlDiff = new HtmlDiff.HtmlDiff(articleMain.Name ?? "", article.Name ?? "");
                model.EditOverview = "<h5>名称</h5>" + htmlDiff.Build().Replace("\r\n", "<br>");
                htmlDiff = new HtmlDiff.HtmlDiff(articleMain.BriefIntroduction ?? "", article.BriefIntroduction ?? "");
                model.EditOverview += "<h5>简介</h5>" + htmlDiff.Build().Replace("\r\n", "<br>");
                htmlDiff = new HtmlDiff.HtmlDiff(articleMain.Type.ToString() ?? "", article.Type.ToString() ?? "");
                model.EditOverview += "<h5>类型</h5>" + htmlDiff.Build().Replace("\r\n", "<br>");
                htmlDiff = new HtmlDiff.HtmlDiff(articleMain.PubishTime.ToString("D") ?? "", article.PubishTime.ToString("D") ?? "");
                model.EditOverview += "<h5>发布日期</h5>" + htmlDiff.Build().Replace("\r\n", "<br>");
                htmlDiff = new HtmlDiff.HtmlDiff(articleMain.OriginalAuthor ?? "", article.OriginalAuthor ?? "");
                model.EditOverview += "<h5>原作者</h5>" + htmlDiff.Build().Replace("\r\n", "<br>");
                htmlDiff = new HtmlDiff.HtmlDiff(articleMain.OriginalLink ?? "", article.OriginalLink ?? "");
                model.EditOverview += "<h5>原文链接</h5>" + htmlDiff.Build().Replace("\r\n", "<br>");

                model.BeforeModel = articleMain;
                model.AfterModel = articleMainBefore;
            }
            else
            {
                HtmlDiff.HtmlDiff htmlDiff = new HtmlDiff.HtmlDiff(article.Name ?? "", articleMain.Name ?? "");
                model.EditOverview = "<h5>名称</h5>" + htmlDiff.Build().Replace("\r\n", "<br>");
                htmlDiff = new HtmlDiff.HtmlDiff(article.BriefIntroduction ?? "", articleMain.BriefIntroduction ?? "");
                model.EditOverview += "<h5>简介</h5>" + htmlDiff.Build().Replace("\r\n", "<br>");
                htmlDiff = new HtmlDiff.HtmlDiff(article.Type.ToString() ?? "", articleMain.Type.ToString() ?? "");
                model.EditOverview += "<h5>类型</h5>" + htmlDiff.Build().Replace("\r\n", "<br>");
                htmlDiff = new HtmlDiff.HtmlDiff(article.PubishTime.ToString("D") ?? "", articleMain.PubishTime.ToString("D") ?? "");
                model.EditOverview += "<h5>发布日期</h5>" + htmlDiff.Build().Replace("\r\n", "<br>");
                htmlDiff = new HtmlDiff.HtmlDiff(article.OriginalAuthor ?? "", articleMain.OriginalAuthor ?? "");
                model.EditOverview += "<h5>原作者</h5>" + htmlDiff.Build().Replace("\r\n", "<br>");
                htmlDiff = new HtmlDiff.HtmlDiff(article.OriginalLink ?? "", articleMain.OriginalLink ?? "");
                model.EditOverview += "<h5>原文链接</h5>" + htmlDiff.Build().Replace("\r\n", "<br>");



                model.AfterModel = articleMain;
                model.BeforeModel = articleMainBefore;
            }

            return true;
        }

        public async Task<bool> GetEditArticleRelevanesExamineView(Models.ExaminedViewModel model, Examine examine)
        {
            model.Type = "文章";
            Article article = await _articleRepository.GetAll()
                   .Include(s => s.Relevances)
                   .FirstOrDefaultAsync(s => s.Id == examine.ArticleId);
            if (article == null)
            {

                return false;
            }
            model.EntryId = article.Id;
            model.EntryName = article.Name;
            //序列化相关性列表
            //先读取词条信息
            var relevances = new List<RelevancesViewModel>();
            foreach (var item in article.Relevances)
            {
                bool isAdd = false;

                //遍历信息列表寻找关键词
                foreach (var infor in relevances)
                {
                    if (infor.Modifier == item.Modifier)
                    {
                        //关键词相同则添加
                        infor.Informations.Add(new RelevancesKeyValueModel
                        {
                            DisplayName = item.DisplayName,
                            DisplayValue = item.DisplayValue,
                            Link = item.Link
                        });
                        isAdd = true;
                        break;
                    }
                }
                if (isAdd == false)
                {
                    //没有找到关键词 则新建关键词
                    var temp = new RelevancesViewModel
                    {
                        Modifier = item.Modifier,
                        Informations = new List<RelevancesKeyValueModel>()
                    };
                    temp.Informations.Add(new RelevancesKeyValueModel
                    {
                        DisplayName = item.DisplayName,
                        DisplayValue = item.DisplayValue,
                        Link = item.Link

                    });
                    relevances.Add(temp);
                }
            }


            //序列化相关性列表
            //先读取词条信息
            var relevances_examine = new List<RelevancesViewModel>();
            foreach (var item in article.Relevances)
            {
                bool isAdd = false;

                //遍历信息列表寻找关键词
                foreach (var infor in relevances_examine)
                {
                    if (infor.Modifier == item.Modifier)
                    {
                        //关键词相同则添加
                        infor.Informations.Add(new RelevancesKeyValueModel
                        {
                            DisplayName = item.DisplayName,
                            DisplayValue = item.DisplayValue,
                            Link = item.Link
                        });
                        isAdd = true;
                        break;
                    }
                }
                if (isAdd == false)
                {
                    //没有找到关键词 则新建关键词
                    var temp = new RelevancesViewModel
                    {
                        Modifier = item.Modifier,
                        Informations = new List<RelevancesKeyValueModel>()
                    };
                    temp.Informations.Add(new RelevancesKeyValueModel
                    {
                        DisplayName = item.DisplayName,
                        DisplayValue = item.DisplayValue,
                        Link = item.Link

                    });
                    relevances_examine.Add(temp);
                }
            }
            //再读取当前用户等待审核的信息
            //序列化数据
            ArticleRelecancesModel articleRelevancesModel = null;
            using (TextReader str = new StringReader(examine.Context))
            {
                JsonSerializer serializer = new JsonSerializer();
                articleRelevancesModel = (ArticleRelecancesModel)serializer.Deserialize(str, typeof(ArticleRelecancesModel));
            }
            foreach (var item in articleRelevancesModel.Relevances)
            {
                bool isAdd = false;

                //遍历信息列表寻找关键词
                foreach (var infor in relevances_examine)
                {
                    //如果关键词相同
                    if (infor.Modifier == item.Modifier)
                    {
                        //继续查找是否存在主索引相同的项目
                        foreach (var mod in infor.Informations)
                        {
                            if (mod.DisplayName == item.DisplayName)
                            {
                                //查看是否为删除操作
                                if (item.IsDelete == true)
                                {
                                    infor.Informations.Remove(mod);
                                    isAdd = true;
                                    break;
                                }
                                else
                                {
                                    mod.DisplayValue = item.DisplayValue;
                                    mod.DisplayName = item.DisplayName;
                                    mod.Link = item.Link;

                                    isAdd = true;
                                    break;
                                }
                            }
                        }
                        //没有找到 
                        if (isAdd == false)
                        {
                            //查看是否为删除操作
                            if (item.IsDelete == false)
                            {
                                //添加
                                infor.Informations.Add(new RelevancesKeyValueModel
                                {
                                    DisplayName = item.DisplayName,
                                    DisplayValue = item.DisplayValue,
                                    Link = item.Link,

                                });
                                isAdd = true;
                                break;
                            }
                        }
                    }
                }
                if (isAdd == false && item.IsDelete == false)
                {
                    //没有找到关键词 则新建关键词
                    var temp = new RelevancesViewModel
                    {
                        Modifier = item.Modifier,
                        Informations = new List<RelevancesKeyValueModel>()
                    };
                    temp.Informations.Add(new RelevancesKeyValueModel
                    {
                        DisplayName = item.DisplayName,
                        DisplayValue = item.DisplayValue,
                        Link = item.Link
                    });
                    relevances_examine.Add(temp);
                }

            }

            //json格式化
            model.EditOverview = _appHelper.GetJsonStringView(examine.Context);


            //判断是否是等待审核状态
            if (examine.IsPassed != null)
            {
                model.BeforeModel = relevances_examine;
                model.AfterModel = relevances;
            }
            else
            {
                model.BeforeModel = relevances;
                model.AfterModel = relevances_examine;
            }

            return true;
        }

        public async Task<bool> GetEditArticleMainPageExamineView(Models.ExaminedViewModel model, Examine examine)
        {
            model.Type = "文章";
            Article article = await _articleRepository.FirstOrDefaultAsync(s => s.Id == examine.ArticleId);
            if (article == null)
            {
                return false;
            }
            model.EntryId = article.Id;
            model.EntryName = article.Name;
            //判断是否是等待审核状态
            if (examine.IsPassed != null)
            {
                //序列化数据
                HtmlDiff.HtmlDiff htmlDiff = new HtmlDiff.HtmlDiff(examine.Context ?? "", article.MainPage ?? "");
                model.EditOverview = htmlDiff.Build().Replace("\r\n", "<br>");
                model.BeforeText = _appHelper.MarkdownToHtml(examine.Context);
                model.AfterText = _appHelper.MarkdownToHtml(article.MainPage);
            }
            else
            {
                //序列化数据
                HtmlDiff.HtmlDiff htmlDiff = new HtmlDiff.HtmlDiff(article.MainPage ?? "", examine.Context ?? "");
                model.EditOverview = htmlDiff.Build().Replace("\r\n", "<br>");
                model.BeforeText = _appHelper.MarkdownToHtml(article.MainPage);
                model.AfterText = _appHelper.MarkdownToHtml(examine.Context);
            }

            return true;
        }
        #endregion


        #region 评论

        public async Task<bool> GetPubulishCommentExamineView(Models.ExaminedViewModel model, Examine examine)
        {
            model.Type = "评论";
            Comment comment = await _commentRepository.FirstOrDefaultAsync(s => s.Id == examine.CommentId);
            if (comment == null)
            {
                return false;
            }
            model.EntryId = comment.Id;
            model.EntryName = "";
            //序列化数据
            CommentText commentText = null;
            using (TextReader str = new StringReader(examine.Context))
            {
                JsonSerializer serializer = new JsonSerializer();
                commentText = (CommentText)serializer.Deserialize(str, typeof(CommentText));
            }

            string result = $"评论时间<br>{commentText.CommentTime}<br>";
            result += $"评论内容<br>\n{commentText.Text}\n<br>";
            result += $"类型<br>{commentText.Type.GetDisplayName()}<br>";
            result += $"目标Id<br>{commentText.ObjectId}<br>";
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().UseSoftlineBreakAsHardlineBreak().Build();
            model.EditOverview = Markdown.ToHtml(result, pipeline);
            model.BeforeModel = commentText;
            model.AfterModel = commentText;

            return true;
        }

        #endregion


        #region 用户

        public bool GetUserMainPageExamineView(Models.ExaminedViewModel model, Examine examine)
        {
            model.Type = "用户";
            //判断是否是等待审核状态
            if (examine.IsPassed != null)
            {
                HtmlDiff.HtmlDiff htmlDiff = new HtmlDiff.HtmlDiff(examine.Context ?? "", examine.ApplicationUser.MainPageContext ?? "");
                model.EditOverview = htmlDiff.Build().Replace("\r\n", "<br>");
                model.BeforeText = _appHelper.MarkdownToHtml(examine.Context);
                model.AfterText = _appHelper.MarkdownToHtml(examine.ApplicationUser.MainPageContext);
            }
            else
            {
                HtmlDiff.HtmlDiff htmlDiff = new HtmlDiff.HtmlDiff(examine.ApplicationUser.MainPageContext ?? "", examine.Context ?? "");
                model.EditOverview = htmlDiff.Build().Replace("\r\n", "<br>");
                model.BeforeText = _appHelper.MarkdownToHtml(examine.ApplicationUser.MainPageContext);
                model.AfterText = _appHelper.MarkdownToHtml(examine.Context);
            }

            model.EntryId = 0;
            model.EntryName = examine.ApplicationUser.UserName;

            return true;
        }

        private UserMain InitExamineViewUserMain(ApplicationUser user)
        {
            UserMain userMainBefore = new UserMain
            {
                UserName = user.UserName,
                PersonalSignature = user.PersonalSignature,
                PhotoPath =_appHelper.GetImagePath( user.PhotoPath,"user.png"),
                BackgroundImage = _appHelper.GetImagePath(user.BackgroundImage, "userbackground.jpg")
            };
            return userMainBefore;
        }

        public async Task<bool> GetEditUserMainExamineView(Models.ExaminedViewModel model, Examine examine)
        {
            model.Type = "用户";
            ApplicationUser user = await _userRepository.FirstOrDefaultAsync(s => s.Id == examine.ApplicationUserId);
            if (user == null)
            {
                return false;
            }
            model.EntryId = 0;
            model.EntryName = examine.ApplicationUser.UserName;

            //序列化数据
            UserMain userMainBefore = InitExamineViewUserMain(user);

            //添加修改记录 
            await _userService.UpdateUserData(user, examine);

            //序列化数据
            UserMain userMain = InitExamineViewUserMain(user);

            //判断是否是等待审核状态
            if (examine.IsPassed != null)
            {
                HtmlDiff.HtmlDiff htmlDiff = new HtmlDiff.HtmlDiff(userMain.UserName ?? "", userMainBefore.UserName ?? "");
                model.EditOverview = "<h5>用户名</h5>" + htmlDiff.Build().Replace("\r\n", "<br>");

                htmlDiff = new HtmlDiff.HtmlDiff(userMain.PersonalSignature ?? "", userMainBefore.PersonalSignature ?? "");
                model.EditOverview += "<h5>个性签名</h5>" + htmlDiff.Build().Replace("\r\n", "<br>");

                htmlDiff = new HtmlDiff.HtmlDiff(userMain.Institute ?? "", userMainBefore.Institute ?? "");
                model.EditOverview += "<h5>学院</h5>" + htmlDiff.Build().Replace("\r\n", "<br>");

                htmlDiff = new HtmlDiff.HtmlDiff(userMain.StudentClass ?? "", userMainBefore.StudentClass ?? "");
                model.EditOverview += "<h5>班级</h5>" + htmlDiff.Build().Replace("\r\n", "<br>");

                htmlDiff = new HtmlDiff.HtmlDiff(userMain.StudentId ?? "", userMainBefore.StudentId ?? "");
                model.EditOverview += "<h5>学号</h5>" + htmlDiff.Build().Replace("\r\n", "<br>");

                htmlDiff = new HtmlDiff.HtmlDiff(userMain.StudentName ?? "", userMainBefore.StudentName ?? "");
                model.EditOverview += "<h5>真实姓名</h5>" + htmlDiff.Build().Replace("\r\n", "<br>");

                htmlDiff = new HtmlDiff.HtmlDiff(userMain.QQ ?? "", userMainBefore.QQ ?? "");
                model.EditOverview += "<h5>QQ</h5>" + htmlDiff.Build().Replace("\r\n", "<br>");

                htmlDiff = new HtmlDiff.HtmlDiff(userMain.WeChat ?? "", userMainBefore.WeChat ?? "");
                model.EditOverview += "<h5>微信</h5>" + htmlDiff.Build().Replace("\r\n", "<br>");

                htmlDiff = new HtmlDiff.HtmlDiff(userMain.PublicEmail ?? "", userMainBefore.PublicEmail ?? "");
                model.EditOverview += "<h5>公开的邮箱</h5>" + htmlDiff.Build().Replace("\r\n", "<br>");

                model.BeforeModel = userMain;
                model.AfterModel = userMainBefore;
            }
            else
            {
                HtmlDiff.HtmlDiff htmlDiff = new HtmlDiff.HtmlDiff(userMainBefore.UserName ?? "", userMain.UserName ?? "");
                model.EditOverview = "<h5>用户名</h5>" + htmlDiff.Build().Replace("\r\n", "<br>");

                htmlDiff = new HtmlDiff.HtmlDiff(userMainBefore.PersonalSignature ?? "", userMain.PersonalSignature ?? "");
                model.EditOverview += "<h5>个性签名</h5>" + htmlDiff.Build().Replace("\r\n", "<br>");

                htmlDiff = new HtmlDiff.HtmlDiff(userMainBefore.Institute ?? "", userMain.Institute ?? "");
                model.EditOverview += "<h5>学院</h5>" + htmlDiff.Build().Replace("\r\n", "<br>");

                htmlDiff = new HtmlDiff.HtmlDiff(userMainBefore.StudentClass ?? "", userMain.StudentClass ?? "");
                model.EditOverview += "<h5>班级</h5>" + htmlDiff.Build().Replace("\r\n", "<br>");

                htmlDiff = new HtmlDiff.HtmlDiff(userMainBefore.StudentId ?? "", userMain.StudentId ?? "");
                model.EditOverview += "<h5>学号</h5>" + htmlDiff.Build().Replace("\r\n", "<br>");

                htmlDiff = new HtmlDiff.HtmlDiff(userMainBefore.StudentName ?? "", userMain.StudentName ?? "");
                model.EditOverview += "<h5>真实姓名</h5>" + htmlDiff.Build().Replace("\r\n", "<br>");

                htmlDiff = new HtmlDiff.HtmlDiff(userMainBefore.QQ ?? "", userMain.QQ ?? "");
                model.EditOverview += "<h5>QQ</h5>" + htmlDiff.Build().Replace("\r\n", "<br>");

                htmlDiff = new HtmlDiff.HtmlDiff(userMainBefore.WeChat ?? "", userMain.WeChat ?? "");
                model.EditOverview += "<h5>微信</h5>" + htmlDiff.Build().Replace("\r\n", "<br>");

                htmlDiff = new HtmlDiff.HtmlDiff(userMainBefore.PublicEmail ?? "", userMain.PublicEmail ?? "");
                model.EditOverview += "<h5>公开的邮箱</h5>" + htmlDiff.Build().Replace("\r\n", "<br>");

                model.AfterModel = userMain;
                model.BeforeModel = userMainBefore;
            }

            return true;
        }

        #endregion


        #endregion

        #region 使审核记录真实作用在目标上


        #region 文章
        public async Task ExamineEditArticleMainAsync(Article article, ArticleMain examine)
        {
            _articleService.UpdateArticleDataMain(article, examine);

            await _articleRepository.UpdateAsync(article);
        }

        public async Task ExamineEditArticleRelevancesAsync(Article article, ArticleRelecancesModel examine)
        {
            _articleService.UpdateArticleDataRelevances(article, examine);
         

            await _articleRepository.UpdateAsync(article);
        }

        public async Task ExamineEditArticleMainPageAsync(Article article, string examine)
        {
            _articleService.UpdateArticleDataMainPage(article, examine);

            await _articleRepository.UpdateAsync(article);
        }
        #endregion

      
        #region 用户

        public async Task ExamineEditUserMainAsync(ApplicationUser user, UserMain examine)
        {
            await _userService.UpdateUserDataMain(user, examine);

            await _userRepository.UpdateAsync(user);
        }
        public async Task ExamineEditUserMainPageAsync(ApplicationUser user, string examine)
        {
            await _userService.UpdateUserDataMainPage(user, examine);

            await _userRepository.UpdateAsync(user);
        }


        #endregion



        #endregion

        #region 将审核记录添加到数据库


        public async Task UniversalEditArticleExaminedAsync(Article article, ApplicationUser user, bool isAdmin, string examineStr, Operation operation, string note)
        {
            if (isAdmin)
            {
                //添加到审核列表
                Examine examine = new Examine
                {
                    Operation = operation,
                    Context = examineStr,
                    IsPassed = true,
                    PassedAdminName = user.UserName,
                    Article = article,
                    ArticleId = article.Id,
                    PassedTime = DateTime.Now.ToCstTime(),
                    ApplyTime = DateTime.Now.ToCstTime(),
                    ApplicationUserId = user.Id,
                    ApplicationUser = user
                };
                await _examineRepository.InsertAsync(examine);
            }
            else
            {
                //查找是否在之前有审核
                //获取审核记录
                Examine examine = await GetUserArticleActiveExamineAsync(article.Id, user.Id, operation);
                if (examine != null)
                {
                    examine.Context = examineStr;
                    examine.ApplyTime = DateTime.Now.ToCstTime();
                    await _examineRepository.UpdateAsync(examine);
                }
                else
                {
                    examine = new Examine
                    {
                        Operation = operation,
                        Context = examineStr,
                        IsPassed = null,
                        PassedTime = null,
                        Article = article,
                        ArticleId = article.Id,
                        ApplyTime = DateTime.Now.ToCstTime(),
                        ApplicationUserId = user.Id,
                        ApplicationUser = user
                    };
                    //添加到审核列表
                    await _examineRepository.InsertAsync(examine);
                }
            }
        }

        public async Task<bool> UniversalCreateArticleExaminedAsync(Article article, ApplicationUser user, bool isAdmin, string examineStr, Operation operation, string note)
        {
            if (isAdmin)
            {
                //添加到审核列表
                Examine examine = new Examine
                {
                    Operation = operation,
                    Context = examineStr,
                    IsPassed = true,
                    Article = article,
                    ArticleId = article.Id,
                    PassedAdminName = user.UserName,
                    PassedTime = DateTime.Now.ToCstTime(),
                    ApplyTime = DateTime.Now.ToCstTime(),
                    ApplicationUserId = user.Id,
                    ApplicationUser = user,
                    Note = note
                };
                await _examineRepository.InsertAsync(examine);

            }
            else
            {
                //根据文章Id查找前置审核
                long examineId = -1;
                if (operation != Operation.EditArticleMain)
                {
                    Examine examine_1 = await GetUserArticleActiveExamineAsync(article.Id, user.Id, Operation.EditArticleMain);
                    if (examine_1 == null)
                    {
                        return false;
                    }
                    examineId = examine_1.Id;
                }

                //添加到审核列表
                Examine examine = new Examine
                {
                    Operation = operation,
                    Context = examineStr,
                    IsPassed = null,
                    Article = article,
                    ArticleId = article.Id,
                    PassedTime = null,
                    ApplyTime = DateTime.Now.ToCstTime(),
                    ApplicationUserId = user.Id,
                    ApplicationUser = user,
                    PrepositionExamineId = examineId,
                    Note = note
                };
                await _examineRepository.InsertAsync(examine);

            }
            return true;
        }


        public async Task UniversalEditUserExaminedAsync(ApplicationUser user, bool isAdmin, string examineStr, Operation operation, string note)
        {
            if (isAdmin)
            {
                //添加到审核列表
                Examine examine = new Examine
                {
                    Operation = operation,
                    Context = examineStr,
                    IsPassed = true,
                    PassedAdminName = user.UserName,
                    PassedTime = DateTime.Now.ToCstTime(),
                    ApplyTime = DateTime.Now.ToCstTime(),
                    ApplicationUserId = user.Id,
                    ApplicationUser = user,
                    Note = note
                };
                await _examineRepository.InsertAsync(examine);
            }
            else
            {
                //查找是否在之前有审核
                //获取审核记录
                Examine examine = await GetUserInforActiveExamineAsync( user.Id, operation);
                if (examine != null)
                {
                    examine.Context = examineStr;
                    examine.ApplyTime = DateTime.Now.ToCstTime();
                    await _examineRepository.UpdateAsync(examine);
                }
                else
                {
                    examine = new Examine
                    {
                        Operation = operation,
                        Context = examineStr,
                        IsPassed = null,
                        PassedTime = null,
                        ApplyTime = DateTime.Now.ToCstTime(),
                        ApplicationUserId = user.Id,
                        ApplicationUser = user,
                        Note = note
                    };
                    //添加到审核列表
                    await _examineRepository.InsertAsync(examine);
                }
            }
        }
        #endregion

        #region 批量导入词条文章时 创建审核记录

        public async Task AddBatchArticleExaminesAsync(Article model, ApplicationUser user, string note)
        {
            //第一步 处理主要信息

            //新建审核数据对象
            ArticleMain entryMain = new ArticleMain
            {
                Name = model.Name,
                BriefIntroduction = model.BriefIntroduction,
                MainPicture = model.MainPicture,
                BackgroundPicture = model.BackgroundPicture,
                SmallBackgroundPicture = model.SmallBackgroundPicture,
                Type = model.Type,
                OriginalAuthor = model.OriginalAuthor,
                OriginalLink = model.OriginalLink,
                PubishTime = model.PubishTime,
                RealNewsTime = model.RealNewsTime,
                DisplayName = model.DisplayName
            };
            //序列化
            string resulte = "";
            using (TextWriter text = new StringWriter())
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(text, entryMain);
                resulte = text.ToString();
            }
            //将空文章添加到数据库中 目的是为了获取索引
            Article article = new Article
            {
                Type = model.Type,
                CreateUser = user,
                CreateTime = model.CreateTime,
                LastEditTime = DateTime.Now.ToCstTime()
            };
            article = await _articleRepository.InsertAsync(article);
            await ExamineEditArticleMainAsync(article, entryMain);
            await UniversalCreateArticleExaminedAsync(article, user, true, resulte, Operation.EditArticleMain, note);

            //第二步 处理关联词条

            //转化为标准词条相关性列表格式
            List<ArticleRelevance> articleRelevance = model.Relevances?.ToList();
            //判断审核是否为空
            if (articleRelevance != null && articleRelevance.Count != 0)
            {
                //创建审核数据模型
                ArticleRelecancesModel examinedModel = new ArticleRelecancesModel
                {
                    Relevances = new List<ArticleRelevancesExaminedModel>()
                };
                foreach (var item in articleRelevance)
                {
                    examinedModel.Relevances.Add(new ArticleRelevancesExaminedModel
                    {
                        IsDelete = false,
                        DisplayName = item.DisplayName,
                        Modifier = item.Modifier

                    });
                }
                //序列化JSON
                resulte = "";
                using (TextWriter text = new StringWriter())
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(text, examinedModel);
                    resulte = text.ToString();
                }
                article = await _articleRepository.GetAll().Include(s => s.Relevances).FirstOrDefaultAsync(s => s.Id == article.Id);
                await ExamineEditArticleRelevancesAsync(article, examinedModel);
                await UniversalCreateArticleExaminedAsync(article, user, true, resulte, Operation.EditArticleRelevanes, note);
            }

            //第三步 添加正文

            //判断是否为空
            if (string.IsNullOrWhiteSpace(model.MainPage) == false)
            {
                await ExamineEditArticleMainPageAsync(article, model.MainPage);
                await UniversalCreateArticleExaminedAsync(article, user, true, model.MainPage, Operation.EditArticleMainPage, note);
            }
        }
        #endregion

        #region 获取用户待审核记录
        public async Task<Examine> GetUserArticleActiveExamineAsync(long articleId, string userId, Operation operation)
        {
            return await _examineRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(s => s.ArticleId == articleId && s.ApplicationUserId == userId && s.Operation == operation && s.IsPassed == null);
        }


        public async Task<Examine> GetUserInforActiveExamineAsync( string userId, Operation operation)
        {
            return await _examineRepository.FirstOrDefaultAsync(s =>s.ApplicationUserId == userId && s.Operation == operation && s.IsPassed == null);
        }

        #endregion
    }
}
