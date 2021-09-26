using BootstrapBlazor.Components;
using Microsoft.EntityFrameworkCore;
using SDAllianceWebSite.APIServer.Application.Helper;
using SDAllianceWebSite.APIServer.DataReositories;
using SDAllianceWebSite.Shared.Model;
using SDAllianceWebSite.Shared.ViewModel.Admin;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SDAllianceWebSite.APIServer.Application.Favorites
{
    public class FavoriteFolderService:IFavoriteFolderService
    {
        private readonly IRepository<FavoriteFolder, long> _favoriteFolderRepository;
        private static readonly ConcurrentDictionary<Type, Func<IEnumerable<FavoriteFolder>, string, SortOrder, IEnumerable<FavoriteFolder>>> SortLambdaCache = new();

        private readonly IAppHelper _appHelper;

        public FavoriteFolderService(IAppHelper appHelper, IRepository<FavoriteFolder, long> favoriteFolderRepository)
        {
            _favoriteFolderRepository = favoriteFolderRepository;
            _appHelper = appHelper;
        }

        public async Task<QueryData<ListFavoriteFolderAloneModel>> GetPaginatedResult(QueryPageOptions options, ListFavoriteFolderAloneModel searchModel,string userId="")
        {
            IEnumerable<FavoriteFolder> items;

            //是否限定用户
            if (string.IsNullOrWhiteSpace(userId) == false)
            {
                items = _favoriteFolderRepository.GetAll().AsNoTracking().Where(s => s.ApplicationUserId == userId);
            }
            else
            {
                items = _favoriteFolderRepository.GetAll().AsNoTracking();
            }

            // 处理高级搜索
            if (!string.IsNullOrWhiteSpace(searchModel.Name))
            {
                items = items.Where(item => item.Name?.Contains(searchModel.Name, StringComparison.OrdinalIgnoreCase) ?? false);
            }

            if (!string.IsNullOrWhiteSpace(searchModel.BriefIntroduction))
            {
                items = items.Where(item => item.BriefIntroduction?.Contains(searchModel.BriefIntroduction, StringComparison.OrdinalIgnoreCase) ?? false);
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
                    items = items.Where(item => (item.Name?.Contains(options.SearchText) ?? false)
                                 || (item.BriefIntroduction?.Contains(options.SearchText) ?? false)
                                 || (item.ApplicationUserId.ToString()?.Contains(options.SearchText) ?? false));
                }
            }
         
            // 排序
            var isSorted = false;
            if (!string.IsNullOrWhiteSpace(options.SortName))
            {
                // 外部未进行排序，内部自动进行排序处理
                var invoker = SortLambdaCache.GetOrAdd(typeof(FavoriteFolder), key => LambdaExtensions.GetSortLambda<FavoriteFolder>().Compile());
                items = invoker(items, options.SortName, options.SortOrder);
                isSorted = true;
            }

            // 设置记录总数
            var total = items.LongCount();

            // 内存分页
            items = items.Skip((options.PageIndex - 1) * options.PageItems).Take(options.PageItems).ToList();

            //复制数据
            List<ListFavoriteFolderAloneModel> resultItems = new List<ListFavoriteFolderAloneModel>();
            foreach (var item in items)
            {
                resultItems.Add(new ListFavoriteFolderAloneModel
                {
                    Id = item.Id,
                    Name=item.Name,
                    IsDefault=item.IsDefault,
                    BriefIntroduction=item.BriefIntroduction,
                    Count=item.Count,
                    CreateTime=item.CreateTime,
                    UserId=item.ApplicationUserId
                });
            }

            return new QueryData<ListFavoriteFolderAloneModel>()
            {
                Items = resultItems,
                TotalCount = total,
                IsSorted = isSorted,
                // IsFiltered = isFiltered
            };
        }
    }
}
