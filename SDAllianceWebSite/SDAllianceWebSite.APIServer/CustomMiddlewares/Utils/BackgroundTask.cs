﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SDAllianceWebSite.APIServer.Application.Helper;
using SDAllianceWebSite.APIServer.Application.TimedTasks;
using SDAllianceWebSite.APIServer.DataReositories;
using SDAllianceWebSite.Shared.Helper;
using SDAllianceWebSite.Shared.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SDAllianceWebSite.APIServer.Application.BackgroundTasks
{
    public class BackgroundTask : BackgroundService
    {

        public IServiceProvider Services { get; }

        public BackgroundTask(IServiceProvider services)
        {
            Services = services;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await new TaskFactory().StartNew(() =>
                {
                    try
                    {
                        using var scope = Services.CreateScope();
                        var _timedTaskRepository =
                            scope.ServiceProvider
                                .GetRequiredService<IRepository<TimedTask, int>>();
                        var _timedTaskService =
                           scope.ServiceProvider
                               .GetRequiredService<ITimedTaskService>();

                        var taskList = _timedTaskRepository.GetAllList(s => s.IsRuning == false && s.IsPause == false);

                        foreach (var item in taskList)
                        {
                            switch (item.ExecuteType)
                            {
                                case TimedTaskExecuteType.IntervalTime:
                                    if (item.LastExecutedTime != null && item.LastExecutedTime.Value.AddMinutes(item.IntervalTime) >= DateTime.Now.ToCstTime())
                                    {
                                        continue;
                                    }
                                    break;
                                case TimedTaskExecuteType.EveryDay:
                                    if (item.LastExecutedTime != null && item.LastExecutedTime.Value.Date >= DateTime.Now.ToCstTime().Date)
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        if (item.EveryTime.Value.TimeOfDay >= DateTime.Now.ToCstTime().TimeOfDay)
                                        {
                                            continue;
                                        }
                                    }
                                    break;
                            }
                            _timedTaskService.RunTimedTask(item).Wait();
                        }
                    }
                    catch
                    {
                        //错误处理
                    }

                    //定时任务休眠
                    Thread.Sleep(60 * 1000);
                }, stoppingToken);
            }

        }

    }
}
