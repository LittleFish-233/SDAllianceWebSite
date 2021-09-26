using BootstrapBlazor.Components;
using SDAllianceWebSite.Shared.Model;
using SDAllianceWebSite.Shared.ViewModel.Admin;
using SDAllianceWebSite.Shared.ViewModel.TimedTasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SDAllianceWebSite.APIServer.Application.TimedTasks
{
    public interface ITimedTaskService
    {
        Task<QueryData<ListTimedTaskAloneModel>> GetPaginatedResult(QueryPageOptions options, ListTimedTaskAloneModel searchModel);

        /// <summary>
        /// 运行定时任务
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        Task RunTimedTask(TimedTask item);
    }
}
