using SDAllianceWebSite.Shared.Model;
using SDAllianceWebSite.Shared.ViewModel.Space;
using SDAllianceWebSite.APIServer.Application.Helper;
using SDAllianceWebSite.APIServer.DataReositories;
using SDAllianceWebSite.APIServer.ExamineX;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using SDAllianceWebSite.APIServer.Application.Messages;
using SDAllianceWebSite.APIServer.Application.Users.Dtos;
using SDAllianceWebSite.Shared.Application.Dtos;
using SDAllianceWebSite.Shared.ViewModel.Admin;
using SDAllianceWebSite.Shared.ViewModel.Coments;
using Markdig;
using SDAllianceWebSite.Shared.Helper;
using SDAllianceWebSite.APIServer.Application.Users;
using SDAllianceWebSite.Shared.ExamineModel;
using System.IO;
using Newtonsoft.Json;

namespace SDAllianceWebSite.APIServer.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/space/[action]")]
    public class SpaceAPIController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<Examine, long> _examineRepository;
        private readonly IRepository<ApplicationUser, long> _userRepository;
        private readonly IRepository<Message, int> _messageRepository;
        private readonly IRepository<SignInDay, long> _signInDayRepository;
        private readonly IRepository<Article, long> _articleRepository;
        private readonly IExamineService _examineService;
        private readonly IMessageService _messageService;
        private readonly IUserService _userService;
        private readonly IAppHelper _appHelper;

        public SpaceAPIController(IRepository<Message, int> messageRepository,IMessageService messageService,IAppHelper appHelper, IRepository<ApplicationUser, long> userRepository,
        UserManager<ApplicationUser> userManager, IRepository<SignInDay, long> signInDayRepository, IRepository<Article, long> articleRepository, IUserService userService,
        IRepository<Examine, long> examineRepository, IExamineService examineService)
        {
            _examineRepository = examineRepository;
            _examineService = examineService;
            _appHelper = appHelper;
            this._userManager = userManager;
            _messageService = messageService;
            _messageRepository = messageRepository;
            _userRepository = userRepository;
            _signInDayRepository = signInDayRepository;
            _articleRepository = articleRepository;
            _userService = userService;
        }
        /// <summary>
        /// 通过Id获取用户的真实数据 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<UserInforViewModel>> GetUserDataAsync(string id)
        {
            //判断是否为当前登入用户
            ApplicationUser user_ = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            ApplicationUser user = await _userRepository.GetAll().AsNoTracking().Where(s => s.Id == id)
                .Select(s => new ApplicationUser
                {
                    Id = s.Id,
                    BackgroundImage = s.BackgroundImage,
                    PhotoPath = s.PhotoPath,
                    UserName = s.UserName,
                    PersonalSignature = s.PersonalSignature
                }).FirstOrDefaultAsync();
            if (user == null)
            {
                return NotFound();
            }

            //判断是否为当前用户 是则加载待审核信息
            if (user_ != null && user_.Id == user.Id)
            {
                var examines = await _examineRepository.GetAll().Where(s => s.ApplicationUserId == user.Id && s.IsPassed == null)
               .Select(n => new Examine { Operation = n.Operation, Context = n.Context })
               .ToListAsync();

                var examine1 = examines.Find(s => s.Operation == Operation.UserMainPage && s.IsPassed == null);
                if (examine1 != null)
                {
                    await _userService.UpdateUserData(user, examine1);
                }
                examine1 = examines.Find(s => s.Operation == Operation.EditUserMain && s.IsPassed == null);
                if (examine1 != null)
                {
                    await _userService.UpdateUserData(user, examine1);
                }
            }
            UserInforViewModel model = new UserInforViewModel
            {
                Id=user.Id,
                PersonalSignature = user.PersonalSignature,
                UserName = user.UserName,
                PhotoPath = _appHelper.GetImagePath(user.PhotoPath, "user.png"),
                BackgroundImage = _appHelper.GetImagePath(user.BackgroundImage, "userbackground.jpg")
            };

            return model;

        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        [HttpGet]
        public async Task<ActionResult<PersonalSpaceViewModel>> GetUserViewAsync(string id = "")
        {
            ApplicationUser user = null;

            if (id == "")
            {
                //获取当前登入用户
                //获取当前用户ID
                user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
                id = user.Id;
            }

            //获取当前用户ID
            try
            {
                user = await _userManager.Users
                                          .Include(x => x.SignInDays)
                                          .Include(s => s.FileManager)
                                          .SingleAsync(x => x.Id == id);
            }
            catch
            {
                return NotFound();
            }

            if (user == null)
            {
                //未找到用户
                return NotFound();
            }

            //判断是否为当前登入用户
            bool isCurrentUser = false;
            ApplicationUser user_ = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            if (user_ != null && user_.Id == user.Id)
            {
                isCurrentUser = true;
            }

            //拉取审核数据
            var examines = await _examineRepository.GetAll().Where(s => s.ApplicationUserId == user.Id)
                .Select(n => new Examine { IsPassed = n.IsPassed, Operation = n.Operation,Context=n.Context,  ArticleId = n.ArticleId,ApplyTime = n.ApplyTime, Id = n.Id })
                .ToListAsync();
            //如果是当前用户 则加载待审核的用户信息
            if (isCurrentUser == true)
            {
                var examine1 = examines.Find(s => s.Operation == Operation.UserMainPage && s.IsPassed == null);
                if (examine1 != null)
                {
                    await _userService.UpdateUserData(user, examine1);
                }
                examine1 = examines.Find(s => s.Operation == Operation.EditUserMain && s.IsPassed == null);
                if (examine1 != null)
                {
                    await _userService.UpdateUserData(user, examine1);
                }
            }

            PersonalSpaceViewModel model = new PersonalSpaceViewModel
            {
                EditCountList = new List<KeyValuePair<DateTime, int>>(),
                Name = user.UserName,
                Email = user.Email,
                PersonalSignature = user.PersonalSignature,
                MainPageContext = user.MainPageContext,
                Id = user.Id,
                IsCurrentUser = isCurrentUser,
                Integral = user.DisplayIntegral,
                LearningValue = user.DisplayLearningValue,
                PhotoPath = _appHelper.GetImagePath(user.PhotoPath, "user.png"),
                SBgImage = _appHelper.GetImagePath(user.SBgImage, ""),
                MBgImage = _appHelper.GetImagePath(user.MBgImage, ""),
                BackgroundImagePath = _appHelper.GetImagePath(user.BackgroundImage, "userbackground.jpg"),
                OnlineTime = ((double)user.OnlineTime / 60 / 60),
                LastOnlineTime = user.LastOnlineTime,
                CanComment = user.CanComment,
                RegisteTime = user.RegistTime,
                Birthday = user.Birthday,
                IsShowFavorites = user.IsShowFavotites,
                Institute = user.Institute,
                StudentClass = user.StudentClass,
                StudentId = user.StudentId,
                StudentName = user.StudentName,
                QQ = user.QQ,
                WeChat = user.WeChat,
                PublicEmail = user.PublicEmail
            };


            //计算成功率和失败率和待审核率
            model.PassedExamineCount = examines.Count(s => s.IsPassed == true);
            model.UnpassedExamineCount = examines.Count(s => s.IsPassed == false);
            model.PassingExamineCount = examines.Count(s => s.IsPassed == null);
            //计算各个部分编辑数目
            model.CreateArticleNum = _articleRepository.Count(s => s.CreateUserId == user.Id);
            model.TotalExamine = examines.Count;
            if (examines.Count != 0)
            {
                var temp = examines.Max(s => s.ApplyTime);
                model.LastEditTime = temp;
            }

            //计算可用空间
            if (user.FileManager != null)
            {
                model.UsedFilesSpace = user.FileManager.UsedSize;
                model.TotalFilesSpace = user.FileManager.TotalSize;
            }
            else
            {
                model.UsedFilesSpace = 0;
                model.TotalFilesSpace = 500 * 1024 * 1024;
            }

            if (await _userManager.IsInRoleAsync(user, "Admin") == true)
            {
                model.Role = "Admin";
            }
            else
            {
                model.Role = "User";
            }

            //计算编辑数据 获取过去30天的词条编辑数量
            //从30天前开始倒数 遇到第一个开始添加
            int MaxCountLineDay = DateTime.Now.DayOfYear;
            DateTime tempDateTimeNow = DateTime.Now.ToCstTime();
            var editCounts = await _examineRepository.GetAll().Where(s => s.ApplicationUserId == user.Id && s.ApplyTime.Date > tempDateTimeNow.Date.AddDays(-MaxCountLineDay))
                   // 先进行了时间字段变更为String字段，切只保留到天
                   // 采用拼接的方式
                   .Select(n => new { Time = n.ApplyTime.Date })
                  // 分类
                  .GroupBy(n => n.Time)
                  // 返回汇总样式
                  .Select(n => new KeyValuePair<DateTime, int>(n.Key, n.Count()))
                   .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
                  .ToListAsync();
            model.EditCountList = editCounts;

            //计算连续签到天数和今天是否签到
            model.IsSignIn = false;
            model.SignInDays = 0;
            if (user.SignInDays != null)
            {


                model.SignInDaysList = user.SignInDays.Select(s => new KeyValuePair<DateTime, int>(s.Time.Date, 1)).ToList();
                if (user.SignInDays.Any(s => s.Time.Date == DateTime.Now.ToCstTime().Date))
                {
                    model.IsSignIn = true;
                    while (user.SignInDays.Any(s => s.Time.Date == DateTime.Now.ToCstTime().AddDays(-model.SignInDays).Date))
                    {
                        model.SignInDays++;
                    }
                }
                else
                {
                    if (user.SignInDays.Any(s => s.Time.Date == DateTime.Now.ToCstTime().Date.AddDays(-1)))
                    {
                        while (user.SignInDays.Any(s => s.Time.Date == DateTime.Now.ToCstTime().AddDays(-model.SignInDays - 1).Date))
                        {
                            model.SignInDays++;
                        }
                    }
                }
            }

            //是否有审核的主页
            var examine = examines.FirstOrDefault(s => s.Operation == Operation.UserMainPage && s.IsPassed == null);
            if (examine != null)
            {
                model.MainPageContext = await _examineRepository.GetAll().Where(s => s.Id == examine.Id).Select(s => s.Context).FirstOrDefaultAsync();
            }

            //提前将MarkDown语法转为Html
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().UseSoftlineBreakAsHardlineBreak().Build();
            model.MainPageContext = Markdig.Markdown.ToHtml(model.MainPageContext ?? "", pipeline);

            return model;
        }

        [AllowAnonymous]
        [HttpGet("{id}/{currentPage}/{MaxResultCount}")]
        public async Task<ActionResult<IEnumerable<ExaminedNormalListModel>>> GetUserEditRecordAsync(string id, int currentPage, int MaxResultCount)
        {
            ApplicationUser user = null;
            //获取当前用户ID
            try
            {
                user = await _userManager.Users
                                          .Include(s => s.Examines)
                                          .SingleAsync(x => x.Id == id);
            }
            catch
            {
                return NotFound();
            }
            if (user == null)
            {
                //未找到用户
                return NotFound();
            }

            GetExamineInput input = new GetExamineInput
            {
                CurrentPage = currentPage,
                MaxResultCount = MaxResultCount,
                Sorting = "Id desc",
                ScreeningConditions = "全部"
            };
            var dtos = await _examineService.GetPaginatedResult(input, user.Id);
           
            return dtos.Data;
        }


        [HttpGet("{currentPage}/{MaxResultCount}")]
        public async Task<ActionResult<PagedResultDto<Message>>> GetUserMessageAsync(int currentPage, int MaxResultCount)
        {
            //获取当前用户ID
            ApplicationUser user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            GetMessageInput input = new GetMessageInput
            {
                CurrentPage = currentPage,
                MaxResultCount = MaxResultCount,
                ScreeningConditions = "全部"
            };
            var dtos = await _messageService.GetPaginatedResult(input, user.Id);

            //需要清除环回引用
            foreach (var item in dtos.Data)
            {
                if (item.ApplicationUser != null)
                {
                    item.ApplicationUser = null;
                }
                //获取图片链接
                item.Image = _appHelper.GetImagePath(item.Image, "user.png");
            }
            return dtos;
        }

        [HttpGet]
        public async Task<ActionResult<Result>> SignInAsync()
        {
            //获取当前用户ID
            ApplicationUser user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            DateTime dateTime = DateTime.Now.ToCstTime();
            //计算连续签到天数和今天是否签到
            if (await _signInDayRepository.GetAll().AnyAsync(s=>s.ApplicationUserId==user.Id&&s.Time.Date== dateTime.Date) ==false)
            {
               await _signInDayRepository.InsertAsync(new SignInDay
                {
                    ApplicationUser = user,
                    ApplicationUserId = user.Id,
                    Time = DateTime.Now.ToCstTime()
                });
            }

            //更新用户积分
            await _appHelper.UpdateUserIntegral(user);

            return new Result { Successful = true };

        }

        [HttpGet]
        public async Task<ActionResult<EditUserMainPageViewModel>> EditMainPageAsync()
        {
            //获取当前用户ID
            ApplicationUser user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            user = await _userManager.Users
                         .SingleAsync(x => x.Id == user.Id);


            //判断用户是否有待审核的主页编辑记录
            Examine examine = await _examineService.GetUserInforActiveExamineAsync( user.Id, Operation.UserMainPage);

            if (examine != null)
            {
                await _userService.UpdateUserData(user, examine);
            }
            EditUserMainPageViewModel model = new EditUserMainPageViewModel
            {
                Id = user.Id,
                MainPage = user.MainPageContext
            };

            return model;
        }

        [HttpPost]
        public async Task<ActionResult<Result>> EditMainPageAsync(EditUserMainPageViewModel model)
        {
            //获取当前用户ID
            ApplicationUser user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //检查是否超过编辑上限
            if (await _examineRepository.CountAsync(s => s.ApplicationUserId == user.Id && s.IsPassed == null) > ToolHelper.MaxEditorCount)
            {
                return new Result { Successful = false, Error = "当前已超过最大待审核编辑数目，请等待审核通过后继续编辑，长时间未更新请联系管理员" };
            }
            //判断是否是管理员
            if (await _userManager.IsInRoleAsync(user, "Admin") == true)
            {
                await _examineService.ExamineEditUserMainPageAsync(user, model.MainPage);
                await _examineService.UniversalEditUserExaminedAsync(user, true, model.MainPage, Operation.UserMainPage, "");
            }
            else
            {
                await _examineService.UniversalEditUserExaminedAsync(user, false, model.MainPage, Operation.UserMainPage, "");
            }

            return new Result { Successful = true };

        }

        [HttpGet]
        public async Task<ActionResult<EditUserDataViewModel>> EditUserDataAsync()
        {
            //获取当前用户ID
            ApplicationUser user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            user = await _userRepository.GetAll().Include(s => s.ThirdPartyLoginInfors).FirstOrDefaultAsync(s => s.Id == user.Id);

            //判断用户是否有待审核的主页编辑记录
            Examine examine = await _examineService.GetUserInforActiveExamineAsync(user.Id, Operation.EditUserMain);
            if (examine != null)
            {
                await _userService.UpdateUserData(user, examine);
            }


            EditUserDataViewModel model = new();
            model.Email = ToolHelper.GetxxxString(user.Email);
            model.Phone = ToolHelper.GetxxxString(user.PhoneNumber);
            model.UserName = user.UserName;
            model.PhotoPath = _appHelper.GetImagePath(user.PhotoPath, "user.png");
            model.BackgroundPath = _appHelper.GetImagePath(user.BackgroundImage, "userbackground.jpg");
            model.BackgroundName = user.BackgroundImage;
            model.MBgImagePath = _appHelper.GetImagePath(user.MBgImage, "background.png");
            model.MBgImageName = user.MBgImage;
            model.SBgImagePath = _appHelper.GetImagePath(user.SBgImage, "background.png");
            model.SBgImageName = user.SBgImage;
            model.PhotoName = user.PhotoPath;
            model.Birthday = user.Birthday;
            model.PersonalSignature = user.PersonalSignature;
            model.CanComment = user.CanComment;
            model.SteamId = user.SteamId;
            model.Id = user.Id;
            model.IsShowFavorites = user.IsShowFavotites;
            model.GithubAccountName = user.ThirdPartyLoginInfors.FirstOrDefault(s => s.Type == ThirdPartyLoginType.GitHub)?.Name;
            model.MicrosoftAccountName = user.ThirdPartyLoginInfors.FirstOrDefault(s => s.Type == ThirdPartyLoginType.Microsoft)?.Name;
            model.GiteeAccountName = user.ThirdPartyLoginInfors.FirstOrDefault(s => s.Type == ThirdPartyLoginType.Gitee)?.Name;

            model.Institute = user.Institute;
            model.StudentClass = user.StudentClass;
            model.StudentId = user.StudentId;
            model.StudentName = user.StudentName;
            model.QQ = user.QQ;
            model.WeChat = user.WeChat;
            model.PublicEmail = user.PublicEmail;
            return model;
        }

        [HttpPost]
        public async Task<ActionResult<Result>> EditUserDataAsync(EditUserDataViewModel model)
        {
            //获取ip
            var ip = this.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(ip))
            {
                ip = this.Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }

            //获取当前用户ID
            ApplicationUser user = await _appHelper.GetAPICurrentUserAsync(HttpContext);



            user.Birthday = model.Birthday;
            user.MBgImage = model.MBgImageName;
            user.SBgImage = model.SBgImageName;
            user.CanComment = model.CanComment;
            //user.IsShowFavotites = model.IsShowFavorites;

            var result = await _userManager.UpdateAsync(user);


            //敏感数据 判断是否修改
            if (user.UserName != model.UserName || user.PersonalSignature != model.PersonalSignature
                || user.PhotoPath != model.PhotoName || user.BackgroundImage != model.BackgroundName
                 || user.Institute != model.Institute || user.StudentClass != model.StudentClass
                  || user.StudentId != model.StudentId || user.StudentName != model.StudentName
                   || user.QQ != model.QQ || user.WeChat != model.WeChat || user.PublicEmail != model.PublicEmail)
            {
                //添加修改记录
                //新建审核数据对象
                UserMain userMain = new UserMain
                {
                    UserName = model.UserName,
                    PersonalSignature = model.PersonalSignature,
                    PhotoPath = model.PhotoPath,
                    BackgroundImage = model.BackgroundName,
                    Institute = model.Institute,
                    StudentClass = model.StudentClass,
                    StudentId = model.StudentId,
                    StudentName = model.StudentName,
                    QQ = model.QQ,
                    WeChat = model.WeChat,
                    PublicEmail = model.PublicEmail,
                };
                //序列化
                string resulte = "";
                using (TextWriter text = new StringWriter())
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(text, userMain);
                    resulte = text.ToString();
                }
                //判断是否是管理员
                if (await _userManager.IsInRoleAsync(user, "Admin") == true)
                {
                    await _examineService.ExamineEditUserMainAsync(user, userMain);
                    await _examineService.UniversalEditUserExaminedAsync(user, true, resulte, Operation.EditUserMain, "");
                }
                else
                {
                    await _examineService.UniversalEditUserExaminedAsync(user, false, resulte, Operation.EditUserMain, "");
                }
            }

            if (result.Succeeded)
            {
                return new Result { Successful = true };
            }
            else
            {
                return new Result { Successful = false, Error = result.Errors.ToList()[0].Description };

            }
        }

        [HttpGet]
        public async Task<ActionResult<Result>> ReadedAllMessagesAsync()
        {
            //获取当前用户ID
            ApplicationUser user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            await _messageRepository.GetRangeUpdateTable().Where(s => s.ApplicationUserId == user.Id).Set(s => s.IsReaded, b => true).ExecuteAsync();

            return new Result { Successful = true };
        }


        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<ActionResult<Result>> EditMessageIsReadedAsync(EditMessageIsReadedModel model)
        {
            await _messageRepository.GetRangeUpdateTable().Where(s => model.Ids.Contains(s.Id)).Set(s => s.IsReaded, b => model.IsReaded).ExecuteAsync();

            return new Result { Successful = true };
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<ActionResult<Result>> DeleteMessagesAsync(DeleteMessagesModel model)
        {
            await _messageRepository.DeleteRangeAsync(s => model.Ids.Contains(s.Id));

            return new Result { Successful = true };
        }

        [HttpPost]
        public async Task<ActionResult<Result>> UserEditMessageIsReadedAsync(EditMessageIsReadedModel model)
        {
            //获取当前用户ID
            ApplicationUser user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //判断是否为管理员
            bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            await _messageRepository.GetRangeUpdateTable().Where(s => (true || s.ApplicationUserId == user.Id) && model.Ids.Contains(s.Id)).Set(s => s.IsReaded, b => model.IsReaded).ExecuteAsync();

            return new Result { Successful = true };
        }


        [HttpPost]
        public async Task<ActionResult<BootstrapBlazor.Components.QueryData<ListMessageAloneModel>>> GetMessagesListNormalAsync(MessagesPagesInfor input)
        {  //获取当前用户ID
            ApplicationUser user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //检查是否为当前用户Id
            if (input.SearchModel.ApplicationUserId != user.Id && await _userManager.IsInRoleAsync(user, "Admin") == false)
            {
                return BadRequest("你没有权限查看此用户的消息列表");
            }
            var dtos = await _messageService.GetPaginatedResult(input.Options, input.SearchModel);

            return dtos;
        }

        [HttpPost]
        public async Task<ActionResult<Result>> UserDeleteMessagesAsync(DeleteMessagesModel model)
        {
            //获取当前用户ID
            ApplicationUser user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //判断是否为管理员
            bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            await _messageRepository.DeleteRangeAsync(s => (isAdmin || s.ApplicationUserId == user.Id) && model.Ids.Contains(s.Id));

            return new Result { Successful = true };
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<ActionResult<Result>> PostMessagesAsync(ListMessageAloneModel model)
        {
            await _messageRepository.InsertAsync(new Message
            {
                Type = (MessageType)model.Type,
                PostTime=model.PostTime??DateTime.Now.ToCstTime(),
                Title = model.Title,
                Text = model.Text,
                ApplicationUserId = model.ApplicationUserId,
                Rank = model.Rank,
                Link = model.Link,
                AdditionalInfor = model.AdditionalInfor,
                LinkTitle = model.LinkTitle,
                Image = model.Image,
                IsReaded = model.IsReaded
            });
            return new Result { Successful = true };
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserUnReadedMessagesModel>> GetUserUnReadedMessagesAsync(string id)
        {
            return new UserUnReadedMessagesModel
            {
                Count =await _messageRepository.CountAsync(s => s.ApplicationUserId == id && s.IsReaded == false)
            };
        }
    }
}
