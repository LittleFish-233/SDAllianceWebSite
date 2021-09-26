using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SDAllianceWebSite.APIServer.Application.Articles;
using SDAllianceWebSite.APIServer.Application.BackUpArchives;
using SDAllianceWebSite.APIServer.Application.Comments;
using SDAllianceWebSite.APIServer.Application.Helper;
using SDAllianceWebSite.APIServer.Application.Messages;
using SDAllianceWebSite.APIServer.Application.Users;
using SDAllianceWebSite.APIServer.DataReositories;
using SDAllianceWebSite.APIServer.ExamineX;
using SDAllianceWebSite.Shared.Model;
using SDAllianceWebSite.Shared.Models;
using SDAllianceWebSite.Shared.ViewModel.BackUpArchives;
using SDAllianceWebSite.Shared.ViewModel.TimedTasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SDAllianceWebSite.APIServer.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ApiController]
    [Route("api/backuparchives/[action]")]

    public class BackUpArchiveAPIController : ControllerBase
    {
        private readonly IRepository<BackUpArchive, long> _backUpArchiveRepository;
        private readonly IAppHelper _appHelper;

        private readonly IBackUpArchiveService _backUpArchiveService;

        public BackUpArchiveAPIController( IBackUpArchiveService backUpArchiveService,IRepository<BackUpArchive, long> backUpArchiveRepository,
            IAppHelper appHelper)
        {
            _appHelper = appHelper;
            _backUpArchiveRepository = backUpArchiveRepository;
            _backUpArchiveService = backUpArchiveService;
        }

        [HttpGet]
        public async Task<ActionResult<ListBackUpArchivesInforViewModel>> ListBackUpArchivesAsync()
        {
            ListBackUpArchivesInforViewModel model = new ListBackUpArchivesInforViewModel
            {
                All = await _backUpArchiveRepository.CountAsync(),
                IsLastFail = await _backUpArchiveRepository.CountAsync(s=>s.IsLastFail==true),
            };

            return model;
        }

        [HttpPost]
        public async Task<ActionResult<BootstrapBlazor.Components.QueryData<ListBackUpArchiveAloneModel>>> GetBackUpArchiveListAsync(BackUpArchivesPagesInfor input)
        {
            var dtos = await _backUpArchiveService.GetPaginatedResult(input.Options, input.SearchModel);

            return dtos;
        }

        [HttpPost]
        public async Task<ActionResult<Result>> RunBackUpArchiveAsync(RunBackUpArchiveModel model)
        {
            var backUpArchives = await _backUpArchiveRepository.GetAll().Where(s => model.Ids.Contains(s.Id)).ToListAsync();

            if (backUpArchives.Count == 0)
            {
                return new Result { Successful = false, Error = "无法找到Id：" + model.Ids.First().ToString() + " 的备份记录" };
            }
            foreach (var item in backUpArchives)
            {
                if (item.ArticleId != null && item.ArticleId > 0)
                {
                    await _backUpArchiveService.BackUpArticle(item);
                }
                else
                {
                    return new Result { Successful = false, Error = "关联词条或文章Id必须为有效值" };
                }
            }

            return new Result { Successful = true };
        }
    }
}
