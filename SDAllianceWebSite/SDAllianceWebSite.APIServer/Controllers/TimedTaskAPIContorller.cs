﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SDAllianceWebSite.APIServer.Application.Articles;
using SDAllianceWebSite.APIServer.Application.Comments;
using SDAllianceWebSite.APIServer.Application.Helper;
using SDAllianceWebSite.APIServer.Application.Messages;
using SDAllianceWebSite.APIServer.Application.TimedTasks;
using SDAllianceWebSite.APIServer.Application.Users;
using SDAllianceWebSite.APIServer.DataReositories;
using SDAllianceWebSite.APIServer.ExamineX;
using SDAllianceWebSite.Shared.Model;
using SDAllianceWebSite.Shared.Models;
using SDAllianceWebSite.Shared.ViewModel.Admin;
using SDAllianceWebSite.Shared.ViewModel.Coments;
using SDAllianceWebSite.Shared.ViewModel.TimedTasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SDAllianceWebSite.APIServer.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ApiController]
    [Route("api/timedtasks/[action]")]

    public class TimedTaskAPIContorller : ControllerBase
    {
        private readonly IRepository<TimedTask, int> _timedTaskRepository;
        private readonly ITimedTaskService _timedTaskService;

        public TimedTaskAPIContorller(ITimedTaskService timedTaskService, IRepository<TimedTask, int> timedTaskRepository)
        {
            _timedTaskRepository = timedTaskRepository;
            _timedTaskService = timedTaskService;
        }


        [HttpGet]
        public async Task<ActionResult<ListTimedTasksInforViewModel>> ListTimedTasksAsync()
        {
            ListTimedTasksInforViewModel model = new ListTimedTasksInforViewModel
            {
                All = await _timedTaskRepository.CountAsync(),
                IsLastFail = await _timedTaskRepository.CountAsync(s=>s.IsLastFail==true),
                IsPasue = await _timedTaskRepository.CountAsync(s=>s.IsPause==true),
                IsRuning = await _timedTaskRepository.CountAsync(s=>s.IsRuning==true),
            };

            return model;
        }

        [HttpPost]
        public async Task<ActionResult<BootstrapBlazor.Components.QueryData<ListTimedTaskAloneModel>>> GetTimedTaskListAsync(TimedTasksPagesInfor input)
        {
            var dtos = await _timedTaskService.GetPaginatedResult(input.Options, input.SearchModel);

            return dtos;
        }

        [HttpPost]
        public async Task<ActionResult<Result>> UpdateTimedTaskDataAsync(ListTimedTaskAloneModel model)
        {
            //查找定时任务
            TimedTask timedTask= await _timedTaskRepository.FirstOrDefaultAsync(s => s.Id == model.Id);
            if (timedTask == null)
            {
                return new Result { Successful = false, Error = $"未找到Id：{model.Id}的定时任务" };
            }
            //检查数据合理性
            if (model.ExecuteType == TimedTaskExecuteType.EveryDay && model.EveryTime == null)
            {
                return new Result { Successful = false, Error = "每天固定时间执行的任务，其时间不能为空" };
            }
            if (model.ExecuteType == TimedTaskExecuteType.IntervalTime && model.IntervalTime <10)
            {
                return new Result { Successful = false, Error = "间隔固定时间的任务，间隔时间不能小于10分钟" };
            }
            if (model.Type == null)
            {
                model.Type = TimedTaskType.BackupArticle;
            }
            if (model.ExecuteType == null)
            {
                model.ExecuteType = TimedTaskExecuteType.IntervalTime;
            }
            //修改数据
            timedTask.Name = model.Name;
            timedTask.Type = model.Type.Value;
            timedTask.ExecuteType = model.ExecuteType.Value;
            timedTask.IntervalTime = model.IntervalTime;
            if (model.ExecuteType == TimedTaskExecuteType.EveryDay && string.IsNullOrWhiteSpace(model.EveryTime) == false)
            {
                try
                {
                    timedTask.EveryTime = DateTime.ParseExact(model.EveryTime, "yyyy-MM-dd HH:mm", null);
                }
                catch
                {
                    return new Result { Successful = false, Error = "固定时间格式不正确，应为 yyyy-MM-dd HH:mm" };
                }
            }
            timedTask.LastExecutedTime = model.LastExecutedTime;
            timedTask.Parameter = model.Parameter;
            timedTask.IsPause = model.IsPause;
            timedTask.IsRuning = model.IsRuning;
            timedTask.IsLastFail = model.IsLastFail;
            //保存
            await _timedTaskRepository.UpdateAsync(timedTask);

            return new Result { Successful = true };
        }

        [HttpPost]
        public async Task<ActionResult<Result>> AddTimedTaskAsync(ListTimedTaskAloneModel model)
        {
            //检查数据合理性
            if (model.ExecuteType == TimedTaskExecuteType.EveryDay && model.EveryTime == null)
            {
                return new Result { Successful = false, Error = "每天固定时间执行的任务，其时间不能为空" };
            }
            if (model.ExecuteType == TimedTaskExecuteType.IntervalTime && model.IntervalTime < 10)
            {
                return new Result { Successful = false, Error = "间隔固定时间的任务，间隔时间不能小于10分钟" };
            }
            if (model.Type == null)
            {
                model.Type = TimedTaskType.BackupArticle;
            }
            if (model.ExecuteType == null)
            {
                model.ExecuteType= TimedTaskExecuteType.IntervalTime;
            }
            //修改数据
            TimedTask timedTask = new TimedTask
            {
                Name = model.Name,
                Type = model.Type.Value,
                ExecuteType = model.ExecuteType.Value,
                IntervalTime = model.IntervalTime
            };
            if (model.ExecuteType==TimedTaskExecuteType.EveryDay&&string.IsNullOrWhiteSpace( model.EveryTime)==false)
            {
                try
                {
                    timedTask.EveryTime = DateTime.ParseExact(model.EveryTime, "yyyy-MM-dd HH:mm", null);
                }
                catch
                {
                    return new Result { Successful = false, Error = "固定时间格式不正确，应为 yyyy-MM-dd HH:mm" };
                }
            }
            timedTask.LastExecutedTime = model.LastExecutedTime;
            timedTask.Parameter = model.Parameter;
            timedTask.IsPause = model.IsPause;
            timedTask.IsRuning = model.IsRuning;
            timedTask.IsLastFail = model.IsLastFail;

            //保存
            await _timedTaskRepository.InsertAsync(timedTask);

            return new Result { Successful = true };
        }

        [HttpPost]
        public async Task<ActionResult<Result>> PauseTimedTaskAsync(PauseTimedTaskModel model)
        {
            await _timedTaskRepository.GetRangeUpdateTable().Where(s => model.Ids.Contains(s.Id)).Set(s => s.IsPause, b => model.IsPause).ExecuteAsync();

            return new Result { Successful = true };
        }

        [HttpPost]
        public async Task<ActionResult<Result>> DeleteTimedTaskAsync(DeleteTimedTaskModel model)
        {

            await _timedTaskRepository.DeleteRangeAsync(s => model.Ids.Contains(s.Id));
            return new Result { Successful = true };
        }

        [HttpPost]
        public async Task<ActionResult<Result>> RunTimedTaskAsync(RunTimedTaskModel model)
        {
            foreach (var item in model.Ids)
            {
                TimedTask timedTask = await _timedTaskRepository.FirstOrDefaultAsync(s => s.Id == item);
                if (timedTask != null)
                {
                    await _timedTaskService.RunTimedTask(timedTask);
                }
                else
                {
                    return new Result { Successful = false,Error="无法找到Id："+item+" 的定时任务" };
                }
            }

            return new Result { Successful = true };
        }

    }
}
