using BootstrapBlazor.Components;
using Microsoft.EntityFrameworkCore;
using NETCore.MailKit.Core;
using SDAllianceWebSite.APIServer.Application.BackUpArchives;
using SDAllianceWebSite.APIServer.DataReositories;
using SDAllianceWebSite.Shared.Helper;
using SDAllianceWebSite.Shared.Model;
using SDAllianceWebSite.Shared.ViewModel.Admin;
using SDAllianceWebSite.Shared.ViewModel.TimedTasks;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SDAllianceWebSite.APIServer.Application.TimedTasks
{
    public class TimedTaskService:ITimedTaskService
    {
        private readonly IRepository<TimedTask, int> _timedTaskRepository;
        private readonly IBackUpArchiveService _backUpArchiveService;

        private static readonly ConcurrentDictionary<Type, Func<IEnumerable<TimedTask>, string, SortOrder, IEnumerable<TimedTask>>> SortLambdaCacheApplicationUser = new();

        public TimedTaskService(IRepository<TimedTask, int> timedTaskRepository,  IBackUpArchiveService backUpArchiveService)
        {
            _timedTaskRepository = timedTaskRepository;
            _backUpArchiveService = backUpArchiveService;
        }

        public Task<QueryData<ListTimedTaskAloneModel>> GetPaginatedResult(QueryPageOptions options, ListTimedTaskAloneModel searchModel)
        {
            IEnumerable<TimedTask> items = _timedTaskRepository.GetAll().AsNoTracking();
            // 处理高级搜索
            if (!string.IsNullOrWhiteSpace(searchModel.Name))
            {
                items = items.Where(item => item.Name?.Contains(searchModel.Name, StringComparison.OrdinalIgnoreCase) ?? false);
            }
            if (searchModel.Type != null)
            {
                items = items.Where(item => item.Type == searchModel.Type);
            }
            if (searchModel.ExecuteType != null)
            {
                items = items.Where(item => item.ExecuteType == searchModel.ExecuteType);
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
                    items = items.Where(item => (item.Name?.Contains(options.SearchText) ?? false));
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
                var invoker = SortLambdaCacheApplicationUser.GetOrAdd(typeof(TimedTask), key => LambdaExtensions.GetSortLambda<TimedTask>().Compile());
                items = invoker(items, options.SortName, options.SortOrder);
                isSorted = true;
            }

            // 设置记录总数
            var total = items.Count();

            // 内存分页
            items = items.Skip((options.PageIndex - 1) * options.PageItems).Take(options.PageItems).ToList();

            //复制数据
            List<ListTimedTaskAloneModel> resultItems = new List<ListTimedTaskAloneModel>();
            foreach (var item in items)
            {
                resultItems.Add(new ListTimedTaskAloneModel
                {
                    Id = item.Id,
                    Name = item.Name,
                    Type = item.Type,
                    ExecuteType = item.ExecuteType,
                    IntervalTime = item.IntervalTime,
                    EveryTime = item.EveryTime?.ToString("yyyy-MM-dd HH:mm"),
                    LastExecutedTime = item.LastExecutedTime,
                    Parameter = item.Parameter,
                    IsPause = item.IsPause,
                    IsRuning=item.IsRuning,
                    IsLastFail=item.IsLastFail
                });
            }

            return Task.FromResult(new QueryData<ListTimedTaskAloneModel>()
            {
                Items = resultItems,
                TotalCount = total,
                IsSorted = isSorted,
                // IsFiltered = isFiltered
            });
        }

        public async Task RunTimedTask(TimedTask item)
        {
            if (item.IsRuning == true || item.IsPause == true)
            {
                return;
            }
            try
            {

                //更新执行状态
                item = await _timedTaskRepository.FirstOrDefaultAsync(s => s.Id == item.Id);
                if (item != null)
                {
                    item.IsRuning = true;
                    await _timedTaskRepository.UpdateAsync(item);
                }
                //根据不同类型任务进行调用
                switch (item.Type)
                {
                   
                    case TimedTaskType.BackupArticle:
                        int maxNum = 10;
                        try
                        {
                            if (string.IsNullOrWhiteSpace(item.Parameter) == false)
                            {
                                maxNum = int.Parse(item.Parameter);
                            }
                        }
                        catch { }
                        await _backUpArchiveService.BackUpAllArticles(maxNum);
                        break;
               
                    case TimedTaskType.UpdateSitemap:
                        await _backUpArchiveService.UpdateSitemap();
                        break;
                }
                //记录执行时间
                item = await _timedTaskRepository.FirstOrDefaultAsync(s => s.Id == item.Id);
                if (item != null)
                {
                    item.IsLastFail = false;
                    item.IsRuning = false;
                    item.LastExecutedTime = DateTime.Now.ToCstTime();
                    await _timedTaskRepository.UpdateAsync(item);
                }
            }
            catch
            {
                item = await _timedTaskRepository.FirstOrDefaultAsync(s => s.Id == item.Id);
                if (item != null)
                {
                    item.IsLastFail = false;
                    item.IsRuning = false;
                    item.IsLastFail = true;
                    item.LastExecutedTime = DateTime.Now.ToCstTime();
                    await _timedTaskRepository.UpdateAsync(item);
                }
            }
            finally
            {

            }
        }

    }
}
