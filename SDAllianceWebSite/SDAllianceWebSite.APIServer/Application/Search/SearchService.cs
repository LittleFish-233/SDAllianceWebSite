using SDAllianceWebSite.Shared.Application.Dtos;
using SDAllianceWebSite.Shared.Application.Search.Dtos;
using SDAllianceWebSite.Shared.Model;
using SDAllianceWebSite.Shared.ViewModel.Home;
using SDAllianceWebSite.APIServer.Application.Helper;
using SDAllianceWebSite.APIServer.DataReositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace SDAllianceWebSite.APIServer.Application.Search
{
    public class SearchService : ISearchService
    {
        private readonly IRepository<Article, long> _articleRepository;
        private readonly IRepository<ApplicationUser, string> _userRepository;
        private readonly IAppHelper _appHelper;


        public SearchService( IRepository<Article, long> articleRepository, IRepository<ApplicationUser, string> userRepository, IAppHelper appHelper)
        {
            _articleRepository = articleRepository;
            _userRepository = userRepository;
            _appHelper = appHelper;
        }

        public async Task<PagedResultDto<SearchAloneModel>> GetPaginatedResult(GetSearchInput input)
        {
            IQueryable<Article> query2 = null;
            IEnumerable<ApplicationUser> query3 = null;

            //是否启用对应数据
            if (input.ScreeningConditions == "文章" || input.ScreeningConditions == "感想" || input.ScreeningConditions == "访谈" || input.ScreeningConditions == "技术"
                || input.ScreeningConditions == "动态" || input.ScreeningConditions == "评测" || input.ScreeningConditions == "公告"
                || input.ScreeningConditions == "杂谈")
            {
                query2 = _articleRepository.GetAll().AsNoTracking().Include(s=>s.CreateUser).Where(s => s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false);
            }
            else if (input.ScreeningConditions == "用户")
            {
                query3 =new List<ApplicationUser>(); // _userRepository.GetAll().AsNoTracking();
            }
            else
            {
                query2 = _articleRepository.GetAll().AsNoTracking().Include(s => s.CreateUser).Where(s => s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false);
                query3 = _userRepository.GetAll().AsNoTracking();
            }
            //根据数据集分别进行筛选
            int count1 = 0;
            int count2 = 0;
            int count3 = 0;
          
            if (query2 != null)
            {
                //判断是否是条件筛选
                if (!string.IsNullOrWhiteSpace(input.ScreeningConditions))
                {
                    switch (input.ScreeningConditions)
                    {
                        case "感想":
                            query2 = query2.Where(s => s.Type == ArticleType.Tought);
                            break;
                        case "访谈":
                            query2 = query2.Where(s => s.Type == ArticleType.Interview);
                            break;
                        case "攻略":
                            query2 = query2.Where(s => s.Type == ArticleType.Technology);
                            break;
                        case "动态":
                            query2 = query2.Where(s => s.Type == ArticleType.News);
                            break;
                        case "评测":
                            query2 = query2.Where(s => s.Type == ArticleType.Evaluation);
                            break;
                       case "公告":
                            query2 = query2.Where(s => s.Type == ArticleType.Notice);
                            break;
                        case "杂谈":
                            query2 = query2.Where(s => s.Type == ArticleType.None);
                            break;
                    }
                }
                //判断输入的查询名称是否为空
                if (!string.IsNullOrWhiteSpace(input.FilterText))
                {

                    query2 = query2.Where(s => s.Name.Contains(input.FilterText)
                      || s.DisplayName.Contains(input.FilterText)
                      || s.MainPage.Contains(input.FilterText)
                      || s.BriefIntroduction.Contains(input.FilterText));
                }
                count2 = query2.Count();
            }
            if (query3 != null)
            {
                //判断输入的查询名称是否为空
                if (!string.IsNullOrWhiteSpace(input.FilterText))
                {

                    query3 = query3.Where(s => s.UserName.Contains(input.FilterText)
                      || s.MainPageContext.Contains(input.FilterText)
                      || s.PersonalSignature.Contains(input.FilterText));
                }
                count3 = query3.Count();
            }
            //根据 每个数据 的条数最大值进行分页计算
            int count = 0;
            if (count1 > count)
            {
                count = count1;
            }
            if (count2 > count)
            {
                count = count2;
            }
            if (count3 > count)
            {
                count = count3;
            }

            //根据需求进行排序，然后进行分页逻辑的计算
            int realCount1 = 0;
            int realCount2 = 0;
            int realCount3 = 0;
           
            if ((input.CurrentPage - 1) * input.MaxResultCount < count2)
            {

                if ((input.CurrentPage - 1) * input.MaxResultCount + input.MaxResultCount < count2)
                {
                    //在数据范围内
                    realCount2 = input.MaxResultCount;
                    query2 = query2.OrderBy(input.Sorting).Skip((input.CurrentPage - 1) * input.MaxResultCount).Take(input.MaxResultCount);
                }
                else
                {
                    realCount2 = count2 - (input.CurrentPage - 1) * input.MaxResultCount;
                    query2 = query2.OrderBy(input.Sorting).Skip((input.CurrentPage - 1) * input.MaxResultCount).Take(realCount2);
                }
            }
            if ((input.CurrentPage - 1) * input.MaxResultCount < count3)
            {
                string sorting;
                //更改排序字段
                if (input.Sorting == "Name")
                {
                    sorting = "UserName";
                }
                else
                {
                    sorting = "Id";
                }
                if ((input.CurrentPage - 1) * input.MaxResultCount + input.MaxResultCount < count3)
                {
                    //在数据范围内
                    realCount3 = input.MaxResultCount;
                   // query3 = query3.OrderBy(sorting).Skip((input.CurrentPage - 1) * input.MaxResultCount).Take(input.MaxResultCount);
                }
                else
                {
                    realCount3 = count3 - (input.CurrentPage - 1) * input.MaxResultCount;
                   // query3 = query3.OrderBy(sorting).Skip((input.CurrentPage - 1) * input.MaxResultCount).Take(realCount3);
                }
            }

            //将结果转换为List集合 加载到内存中
            List<SearchAloneModel> models = new List<SearchAloneModel>();
            List<Article> models2 = null;
            List<ApplicationUser> models3 = null;
            count1 = 0;
            count2 = 0;
            count3 = 0;

            //依次加载
            if (realCount2 != 0)
            {
                models2 = await query2.AsNoTracking().ToListAsync();
                count2 = models2.Count;

            }

            if (realCount3 != 0)
            {
                models3 = new List<ApplicationUser>(); //await query3.AsNoTracking().ToListAsync();
                count3 = models3.Count;

            }

            //根据结果进行插值
            for (int i = 0; i < count; i++)
            {
                

                if (i < count2)
                {
                    var item = models2[i];
                    item.BriefIntroduction = _appHelper.GetStringAbbreviation(item.BriefIntroduction, 50);

                    models.Add(new SearchAloneModel
                    {
                        article = _appHelper.GetArticleInforTipViewModel(item)
                    });
                }
                if (i < count3)
                {
                    var item = models3[i];
                    item.PhotoPath = _appHelper.GetImagePath(item.PhotoPath, "user.png");

                    models.Add(new SearchAloneModel
                    {
                        user = new Shared.ViewModel.Search.UserInforTipViewModel
                        {
                            Id = item.Id,
                            PersonalSignature = item.PersonalSignature,
                           // Email = item.Email,
                            PhotoPath = item.PhotoPath,
                            UserName = item.UserName
                        }
                    });
                }
            }



            var dtos = new PagedResultDto<SearchAloneModel>
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
    }
}
