using BootstrapBlazor.Components;
using Microsoft.EntityFrameworkCore;
using SDAllianceWebSite.APIServer.Application.Helper;
using SDAllianceWebSite.APIServer.DataReositories;
using SDAllianceWebSite.Shared.Application.Dtos;
using SDAllianceWebSite.Shared.Model;
using SDAllianceWebSite.Shared.ViewModel.Admin;
using SDAllianceWebSite.Shared.ViewModel.Search;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Dynamic.Core;
using SDAllianceWebSite.Shared.ViewModel.Favorites;

namespace SDAllianceWebSite.APIServer.Application.Favorites
{
    public class FavoriteObjectService:IFavoriteObjectService
    {
        private readonly IRepository<FavoriteObject, long> _favoriteObjectRepository;
        private readonly IRepository<Article, long> _articleRepository;
        private static readonly ConcurrentDictionary<Type, Func<IEnumerable<FavoriteObject>, string, SortOrder, IEnumerable<FavoriteObject>>> SortLambdaCache = new();

        private readonly IAppHelper _appHelper;

        public FavoriteObjectService(IAppHelper appHelper, IRepository<FavoriteObject, long> favoriteObjectRepository, IRepository<Article, long> articleRepository)
        {
            _favoriteObjectRepository = favoriteObjectRepository;
            _appHelper = appHelper;
            _articleRepository = articleRepository;
        }

        public async Task<QueryData<ListFavoriteObjectAloneModel>> GetPaginatedResult(QueryPageOptions options, ListFavoriteObjectAloneModel searchModel, long favoriteFolderId =0)
        {
            IEnumerable<FavoriteObject> items;

            //是否限定用户
            if (favoriteFolderId != 0)
            {
                items = _favoriteObjectRepository.GetAll().AsNoTracking().Where(s => s.FavoriteFolderId == favoriteFolderId);
            }
            else
            {
                items = _favoriteObjectRepository.GetAll().AsNoTracking();
            }

         

            // 排序
            var isSorted = false;
            if (!string.IsNullOrWhiteSpace(options.SortName))
            {
                // 外部未进行排序，内部自动进行排序处理
                var invoker = SortLambdaCache.GetOrAdd(typeof(FavoriteObject), key => LambdaExtensions.GetSortLambda<FavoriteObject>().Compile());
                items = invoker(items, options.SortName, options.SortOrder);
                isSorted = true;
            }

            // 设置记录总数
            var total = items.LongCount();

            // 内存分页
            items = items.Skip((options.PageIndex - 1) * options.PageItems).Take(options.PageItems).ToList();

            //获取名称
            var articleIds = items.Where(s => s.Type == FavoriteObjectType.Article).Select(s => s.ArticleId).ToList();

            var article_ = await _articleRepository.GetAll().Where(s => articleIds.Contains(s.Id)).Select(s => new KeyValuePair<long, string>(s.Id, s.DisplayName)).ToListAsync();


            //复制数据
            List<ListFavoriteObjectAloneModel> resultItems = new List<ListFavoriteObjectAloneModel>();
            foreach (var item in items)
            {
                resultItems.Add(new ListFavoriteObjectAloneModel
                {
                    Id = item.Id,
                    Name =  article_.FirstOrDefault(s => s.Key == item.ArticleId).Value,
                    Type=item.Type,
                    ObjectId=(long) item.ArticleId,
                    CreateTime = item.CreateTime
                });
            }

            return new QueryData<ListFavoriteObjectAloneModel>()
            {
                Items = resultItems,
                TotalCount = total,
                IsSorted = isSorted,
                // IsFiltered = isFiltered
            };
        }

        public async Task<PagedResultDto<FavoriteObjectAloneViewModel>> GetPaginatedResult(PagedSortedAndFilterInput input,long favoriteFolderId)
        {
            IQueryable<FavoriteObject> query;
            //筛选
            if (favoriteFolderId!=0)
            {
                query = _favoriteObjectRepository.GetAll().Where(s => s.FavoriteFolderId == favoriteFolderId).AsNoTracking();

            }
            else
            {
                query = _favoriteObjectRepository.GetAll().AsNoTracking();
            }
          
            //统计查询数据的总条数
            var count = query.Count();
            //根据需求进行排序，然后进行分页逻辑的计算
            //这个特殊方法中当前页数解释为起始位
            query = query.OrderBy(input.Sorting).Skip(input.CurrentPage).Take(input.MaxResultCount);

            //将结果转换为List集合 加载到内存中
            List<FavoriteObject> models = null;
            if (count != 0)
            {
                models = await query.AsNoTracking().Include(s => s.Article).ThenInclude(s=>s.CreateUser).ToListAsync();
            }
            else
            {
                models = new List<FavoriteObject>();
            }

            var dtos = new List<FavoriteObjectAloneViewModel>();
            foreach (var item in models)
            {
                if (item.Type == FavoriteObjectType.Article)
                {
                    dtos.Add(new FavoriteObjectAloneViewModel
                    {
                        article = _appHelper.GetArticleInforTipViewModel(item.Article)
                    });
                }
               
            }

            var dtos_ = new PagedResultDto<FavoriteObjectAloneViewModel>
            {
                TotalCount = count,
                CurrentPage = input.CurrentPage,
                MaxResultCount = input.MaxResultCount,
                Data = dtos,
                FilterText = input.FilterText,
                Sorting = input.Sorting,
                ScreeningConditions = input.ScreeningConditions
            };
            return dtos_;
        }

    }
}
