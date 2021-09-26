﻿using SDAllianceWebSite.Shared.Model;
using SDAllianceWebSite.Shared.Model.ExamineModel;
using SDAllianceWebSite.Shared.ViewModel;
using SDAllianceWebSite.Shared.ViewModel.Admin;
using SDAllianceWebSite.APIServer.Application.Articles;
using SDAllianceWebSite.APIServer.Application.Articles.Dtos;
using SDAllianceWebSite.APIServer.Application.Helper;
using SDAllianceWebSite.APIServer.Application.Users;
using SDAllianceWebSite.APIServer.Application.Users.Dtos;
using SDAllianceWebSite.APIServer.DataReositories;
using SDAllianceWebSite.Shared.Application.Dtos;
using SDAllianceWebSite.APIServer.ExamineX;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SDAllianceWebSite.Shared.Helper;
using SDAllianceWebSite.APIServer.Application.Comments;
using SDAllianceWebSite.APIServer.Application.Messages;
using SDAllianceWebSite.Shared.ExamineModel;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;
using SDAllianceWebSite.Shared.ViewModel.Batch;
using TencentCloud.Ckafka.V20190819.Models;
using SDAllianceWebSite.Shared.ViewModel.Accounts;

namespace SDAllianceWebSite.APIServer.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ApiController]
    [Route("api/batch/[action]")]
    public class BatchAPIController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<Article, long> _articleRepository;
        private readonly IRepository<FriendLink, int> _friendLinkRepository;
        private readonly IRepository<Carousel, int> _carouselRepository;
        private readonly IRepository<HistoryUser, int> _historyUserRepository;
        private readonly IAppHelper _appHelper;
        private readonly IExamineService _examineService;


        public BatchAPIController(IRepository<HistoryUser, int> historyUserRepository, IExamineService examineService,
        UserManager<ApplicationUser> userManager, IRepository<FriendLink, int> friendLinkRepository, IRepository<Carousel, int> carouselRepositor,
            IRepository<Article, long> articleRepository, IAppHelper appHelper)
        {
            this._userManager = userManager;
            _appHelper = appHelper;
            _articleRepository = articleRepository;
            _friendLinkRepository = friendLinkRepository;
            _carouselRepository = carouselRepositor;
            _historyUserRepository = historyUserRepository;
            _examineService = examineService;
        }

     

        [HttpPost]
        public async Task<ActionResult<Result>> UpdateArticleDataAsync(ListArticleAloneModel model)
        {
            //查找词条
            Article article = await _articleRepository.FirstOrDefaultAsync(s => s.Id == model.Id);
            if (article == null)
            {
                return new Result { Successful = false, Error = $"未找到Id：{model.Id}的文章" };
            }
            //修改数据
            article.Name = model.Name;
            article.DisplayName = model.DisplayName;
            article.Type = (ArticleType)model.Type;
            article.CreateTime = model.CreateTime;
            article.LastEditTime = model.LastEditTime;
            article.ReaderCount = model.ReaderCount;
            article.OriginalAuthor = model.OriginalAuthor;
            article.OriginalLink = model.OriginalLink;
            article.Priority = model.Priority;
            article.IsHidden = model.IsHidden;
            article.CanComment = model.CanComment;

            //保存
            await _articleRepository.UpdateAsync(article);
            return new Result { Successful = true };
        }

        [HttpPost]
        public async Task<ActionResult<Result>> UpdateArticleAuthorAsync(UpdateArticleAuthorModel model)
        {
            //查找文章和用户
            Article article = await _articleRepository.FirstOrDefaultAsync(s => s.Id == model.ArticleId);
            if(article==null)
            {
                return new Result { Successful = false, Error = "文章『Id：" + model.ArticleId + "』不存在" };
            }
            ApplicationUser user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return new Result { Successful = false, Error = "用户『Id：" + model.ArticleId + "』不存在" };
            }

            //修改文章作者
            article.CreateUser = user;
            article.CreateUserId = user.Id;
            await _articleRepository.UpdateAsync(article);
            //更新用户积分
            await _appHelper.UpdateUserIntegral(user);
            return new Result { Successful = true };
        }

  
        [HttpPost]
        public async Task<ActionResult<Result>> ImportArticleDataAsync(Article model)
        {
            //不进行完整数据检查 管理员负责确保数据无误 不建议批量导入数据
            if (model.Id != 0)
            {
                return new Result { Successful = false, Error = "导入数据时，不支持指定Id" };
            }
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                return new Result { Successful = false, Error = "名称不能为空" };
            }
            //不能重名
            if (await _articleRepository.CountAsync(s => s.Name == model.Name) > 0)
            {
                return new Result { Successful = false, Error = "文章名称名称『" + model.Name + "』重复，请尝试使用显示名称" };
            }
            //关联创建者
            //检查关联用户是否存在
            ApplicationUser user = null;
            if (string.IsNullOrWhiteSpace(model.CreateUserId) == false)
            {
                //判断是否为历史用户Id
                if (model.CreateUserId.Length < 6)
                {
                    var historyUser = await _historyUserRepository.FirstOrDefaultAsync(s => s.UserIdentity == model.CreateUserId);
                    if (historyUser != null)
                    {
                        //一定会用用户名进行关联
                        user = await _userManager.FindByNameAsync(historyUser.UserName);
                    }
                }
                else
                {
                    user = await _userManager.FindByIdAsync(model.CreateUserId);
                }
               
            }
            //不存在则使用导入者Id
            if (user == null)
            {
                user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            }
            model.CreateUser = user;
            model.CreateUserId = user.Id;
            //判断是否添加转载信息
            if(user.UserName==model.OriginalAuthor)
            {
                model.OriginalAuthor = "";
                model.OriginalLink = "";
            }
            //导入数据
            try
            {
                await _examineService.AddBatchArticleExaminesAsync(model, user,"批量导入历史数据");
                //更新用户积分
                await _appHelper.UpdateUserIntegral(user);
            }
            catch (Exception exc)
            {
                return new Result { Successful = false, Error = "为文章创建审核记录时发生异常『" + exc.Message + "』" };
            }

            return new Result { Successful = true };
        }

        [HttpPost]
        public async Task<ActionResult<Result>> ImportHistoryUserDataAsync(ImportUserModel model)
        {
            //无法进行数据检查 管理员负责确保数据无误 不建议批量导入数据
            if (await _userManager.FindByNameAsync(model.UserName) != null)
            {
                return new Result { Successful = false, Error = "该用户名" + model.UserName + "已存在，请更换名称后，尝试通知用户" };
            }
            if (await _userManager.FindByEmailAsync(model.Email) != null)
            {
                return new Result { Successful = false, Error = "该电子邮箱" + model.Email + "已存在，请更换电子邮箱后，尝试通知用户" };
            }
            if (string.IsNullOrWhiteSpace(model.UserName))
            {
                return new Result { Successful = false, Error = "用户名为必填项目" };
            }
            if (string.IsNullOrWhiteSpace(model.Email))
            {
                return new Result { Successful = false, Error = "电子邮箱为必填项目" };
            }
            if (string.IsNullOrWhiteSpace(model.Password))
            {
                return new Result { Successful = false, Error = "密码为必填项目" };
            }
            if (string.IsNullOrWhiteSpace(model.PasswordSalt))
            {
                return new Result { Successful = false, Error = "加密字符串为必填项目" };
            }

            //创建用户历史数据
            HistoryUser historyUser = new HistoryUser
            {
                UserName = model.UserName,
                Email = model.Email,
                LoginName = model.LoginName,
                Password = model.Password,
                PasswordSalt = model.PasswordSalt,
                UserIdentity=model.UserIdentity
            };
            //导入数据
            await _historyUserRepository.InsertAsync(historyUser);

            //创建当前用户数据
            ApplicationUser applicationUser = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                MainPageContext = model.MainPageContext,
                PersonalSignature = model.PersonalSignature,
                Birthday = model.Birthday,
                RegistTime = model.RegistTime,
                PhotoPath = model.PhotoPath,
                BackgroundImage = model.BackgroundImage,
                Integral = model.Integral,
                LearningValue = model.ContributionValue,
                OnlineTime = model.OnlineTime,
                LastOnlineTime = model.LastOnlineTime,
                UnsealTime = model.UnsealTime,
                CanComment = model.CanComment,
                EmailConfirmed = true
            };
            var result = await _userManager.CreateAsync(applicationUser, new Random().Next()+"A.A"+ new Random().Next());
            if(result.Succeeded==false)
            {
                return new Result { Successful =false, Error = result.Errors.First().Description };
            }
            
            await _userManager.AddToRoleAsync(applicationUser, "User");

            return new Result { Successful = true };

        }

        [HttpPost]
        public async Task<ActionResult<Result>> ImportFriendLinkDataAsync(FriendLink model)
        {
            //不进行完整数据检查 管理员负责确保数据无误 不建议批量导入数据

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                return new Result { Successful = false, Error = "名称不能为空" };
            }
            if (string.IsNullOrWhiteSpace(model.Link))
            {
                return new Result { Successful = false, Error = "链接不能为空" };
            }
         
            //导入数据
            await _friendLinkRepository.InsertAsync(model);

            return new Result { Successful = true };
        }

        [HttpPost]
        public async Task<ActionResult<Result>> ImportCarouselDataAsync(Carousel model)
        {
            //不进行完整数据检查 管理员负责确保数据无误 不建议批量导入数据

            if (string.IsNullOrWhiteSpace(model.Image))
            {
                return new Result { Successful = false, Error = "图片不能为空" };
            }
            if (string.IsNullOrWhiteSpace(model.Link))
            {
                return new Result { Successful = false, Error = "链接不能为空" };
            }

            //导入数据
            await _carouselRepository.InsertAsync(model);

            return new Result { Successful = true };
        }
    }
}
