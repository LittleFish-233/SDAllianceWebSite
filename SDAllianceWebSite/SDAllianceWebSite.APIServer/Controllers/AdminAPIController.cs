using SDAllianceWebSite.Shared.Model;
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
using SDAllianceWebSite.Shared.Models;
using SDAllianceWebSite.APIServer.Application.Files;
using SDAllianceWebSite.APIServer.Application.ErrorCounts;
using SDAllianceWebSite.APIServer.Application.Favorites;

namespace SDAllianceWebSite.APIServer.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ApiController]
    [Route("api/admin/[action]")]
    public class AdminAPIController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IRepository<Examine, long> _examineRepository;
        private readonly IRepository<Article, long> _articleRepository;
        private readonly IRepository<FriendLink, int> _friendLinkRepository;
        private readonly IRepository<Carousel, int> _carouselRepository;
        private readonly IRepository<UserFile, int> _userFileRepository;
        private readonly IRepository<Message, long> _messageRepository;
        private readonly IRepository<Comment, long> _commentRepository;
        private readonly IRepository<ThumbsUp, long> _thumbsUpRepository;
        private readonly IRepository<BackUpArchive, long> _backUpArchiveRepository;
        private readonly IRepository<BackUpArchiveDetail, long> _backUpArchiveDetailRepository;
        private readonly IRepository<UserOnlineInfor, long> _userOnlineInforRepository;
        private readonly IRepository<ApplicationUser, string> _userRepository;
        private readonly IRepository<SignInDay, long> _signInDayRepository;
        private readonly IRepository<ErrorCount, long> _errorCountRepository;
        private readonly IRepository<FavoriteFolder, long> _favoriteFolderRepository;
        private readonly IRepository<FavoriteObject, long> _favoriteObjectRepository;
        private readonly IUserService _userService;
        private readonly IAppHelper _appHelper;
        private readonly IExamineService _examineService;
        private readonly IArticleService _articleService;
        private readonly ICommentService _commentService;
        private readonly IMessageService _messageService;
        private readonly IFileService _fileService;
        private readonly IErrorCountService _errorCountService;
        private readonly IFavoriteFolderService _favoriteFolderService;

        public AdminAPIController(IRepository<UserOnlineInfor, long> userOnlineInforRepository, IRepository<UserFile, int> userFileRepository, IRepository<FavoriteObject, long> favoriteObjectRepository,
        IFileService fileService, IRepository<SignInDay, long> signInDayRepository, IRepository<ErrorCount, long> errorCountRepository, IRepository<BackUpArchiveDetail, long> backUpArchiveDetailRepository,
        IRepository<ThumbsUp, long> thumbsUpRepository, IRepository<BackUpArchive, long> backUpArchiveRepository,
        IRepository<ApplicationUser, string> userRepository,IMessageService messageService, ICommentService commentService,IRepository<Comment, long> commentRepository,
        IRepository<Message, long> messageRepository, IErrorCountService errorCountService, IRepository<FavoriteFolder, long> favoriteFolderRepository,
        UserManager<ApplicationUser> userManager, IRepository<FriendLink, int> friendLinkRepository, IRepository<Carousel, int> carouselRepositor,
            IArticleService articleService, IUserService userService, RoleManager<IdentityRole> roleManager, IExamineService examineService,
            IRepository<Article, long> articleRepository, IAppHelper appHelper, IFavoriteFolderService favoriteFolderService,
        IWebHostEnvironment webHostEnvironment, IRepository<Examine, long> examineRepository)
        {
            this._userManager = userManager;
            _examineRepository = examineRepository;
            _appHelper = appHelper;
            _articleRepository = articleRepository;
            _examineService = examineService;
            _roleManager = roleManager;
            _userService = userService;
            _articleService = articleService;
            _friendLinkRepository = friendLinkRepository;
            _carouselRepository = carouselRepositor;
            _messageRepository = messageRepository;
            _commentRepository = commentRepository;
            _commentService = commentService;
            _messageService = messageService;
            _userRepository = userRepository;
            _userFileRepository = userFileRepository;
            _userOnlineInforRepository = userOnlineInforRepository;
            _thumbsUpRepository = thumbsUpRepository;
            _backUpArchiveRepository = backUpArchiveRepository;
            _fileService = fileService;
            _signInDayRepository = signInDayRepository;
            _errorCountService = errorCountService;
            _errorCountRepository = errorCountRepository;
            _favoriteFolderRepository = favoriteFolderRepository;
            _favoriteFolderService = favoriteFolderService;
            _backUpArchiveDetailRepository = backUpArchiveDetailRepository;
            _favoriteObjectRepository = favoriteObjectRepository;
        }

        [HttpGet]
        public async Task<ActionResult<ListUsersInforViewModel>> ListUsersAsync()
        {
            ListUsersInforViewModel model = new ListUsersInforViewModel
            {
                UserCount = await _userManager.Users.CountAsync()
            };
            return model;
        }

        [HttpPost]
        public async Task<ActionResult<BootstrapBlazor.Components.QueryData<ListUserAloneModel>>> GetUserListAsync(UsersPagesInfor input)
        {
            var dtos = await _userService.GetPaginatedResult(input.Options, input.SearchModel);

            return dtos;
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<EditUserViewModel>> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            var userClaims = await _userManager.GetClaimsAsync(user);

            var model = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                PersonalSignature = user.PersonalSignature,
                MainPageContext = user.MainPageContext,
                Integral = user.Integral,
                LearningValue = user.LearningValue,
                Claims = userClaims.Select(c => c.Value).ToList(),
                PhotoPath = _appHelper.GetImagePath(user.PhotoPath, "user.png"),
                BackgroundPath = _appHelper.GetImagePath(user.BackgroundImage, "user.png"),
                PhotoName = user.PhotoPath,
                BackgroundName = user.BackgroundImage,
                CanComment = user.CanComment,
                Roles = new List<UserRolesModel>(),
               Birthday=user.Birthday,
               IsShowFavotites=user.IsShowFavotites,
               IsPassedVerification=user.IsPassedVerification,
                Institute = user.Institute,
                StudentClass = user.StudentClass,
                StudentId = user.StudentId,
                StudentName = user.StudentName,
                QQ = user.QQ,
                WeChat = user.WeChat,
                PublicEmail = user.PublicEmail,
            };
            //获取用户角色
            var allRoles = _roleManager.Roles;
            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var item in allRoles)
            {
                bool isSelected = false;
                foreach (var infor in userRoles)
                {
                    if (item.Name == infor)
                    {
                        isSelected = true;
                        break;
                    }
                }
                model.Roles.Add(new UserRolesModel { Name = item.Name, IsSelected = isSelected });
            }
          
            return model;
        }
        [HttpPost]
        public async Task<ActionResult<Result>> EditUser(EditUserViewModel model)
        {
            //获取当前用户ID
            ApplicationUser user_admin = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            var user = await _userManager.FindByIdAsync(model.Id);

            if (user == null)
            {
                return new Result { Error = "当前用户不存在，可能在编辑过程中被删除", Successful = false };
            }

            if (await _userManager.IsInRoleAsync(user, "SuperAdmin")&& await _userManager.IsInRoleAsync(user_admin, "SuperAdmin")==false)
            {
                return new Result { Error = "你没有权限修改超级管理员的信息", Successful = false };
            }

            user.Email = model.Email;
            user.UserName = model.UserName;
            user.PersonalSignature = model.PersonalSignature;
            user.MainPageContext = model.MainPageContext;
            user.Birthday = model.Birthday;
            user.Integral = model.Integral;
            user.LearningValue = model.LearningValue;
            user.PhotoPath = model.PhotoName;
            user.CanComment = model.CanComment;
            user.BackgroundImage = model.BackgroundName;
            user.IsPassedVerification = model.IsPassedVerification;
            user.Institute = model.Institute;
            user.StudentClass = model.StudentClass;
            user.StudentId = model.StudentId;
            user.StudentName = model.StudentName;
            user.WeChat = model.WeChat;
            user.PublicEmail = model.PublicEmail;
            user.QQ = model.QQ;

            if (await _userManager.IsInRoleAsync(user_admin, "SuperAdmin"))
            {
                //处理用户角色
                var userRoles = await _userManager.GetRolesAsync(user);
                foreach (var item in model.Roles)
                {
                    bool isAdd = false;
                    foreach (var infor in userRoles)
                    {
                        if (item.Name == infor)
                        {
                            if (item.IsSelected == false)
                            {
                                await _userManager.RemoveFromRoleAsync(user, infor);
                                isAdd = true;
                                break;
                            }
                        }
                    }
                    if (isAdd == false && item.IsSelected == true)
                    {
                        await _userManager.AddToRoleAsync(user, item.Name);
                    }
                }
            }

            //更新数据
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return new Result { Successful = true };
            }

            return new Result { Successful = false, Error = result.Errors.ToList()[0].Description };
        }

        [HttpGet]
        public async Task<ActionResult<GetArticleCountModel>> ListArticlesAsync()
        {
            GetArticleCountModel model = new GetArticleCountModel
            {
                All = await _articleRepository.CountAsync(),
                Toughts = await _articleRepository.CountAsync(x => x.Type == ArticleType.Tought),
                Interviews = await _articleRepository.CountAsync(x => x.Type == ArticleType.Interview),
                Technologies = await _articleRepository.CountAsync(x => x.Type == ArticleType.Technology),
                News = await _articleRepository.CountAsync(x => x.Type == ArticleType.News),
                Hiddens = await _articleRepository.CountAsync(x => x.IsHidden == true)
            };

            return model;
        }

        [HttpPost]
        public async Task<ActionResult<BootstrapBlazor.Components.QueryData<ListArticleAloneModel>>> GetArticleListAsync(ArticlesPagesInfor input)
        {
            var dtos = await _articleService.GetPaginatedResult(input.Options, input.SearchModel);

            return dtos;
        }

        [HttpGet]
        public async Task<ActionResult<ListExaminesInforViewModel>> ListExaminesAsync()
        {
            ListExaminesInforViewModel model = new();
            DateTime tempDateTimeNow = DateTime.Now.ToCstTime();
            model.All = await _examineRepository.CountAsync();
            model.Passed = await _examineRepository.CountAsync(x => x.IsPassed == true && x.PassedTime != null && x.PassedTime.Value.Date == tempDateTimeNow.Date);
            model.Unpassed = await _examineRepository.CountAsync(x => x.IsPassed == false && x.PassedTime != null && x.PassedTime.Value.Date == tempDateTimeNow.Date);
            model.Examining = await _examineRepository.CountAsync(x => x.IsPassed == null);


            return model;
        }
        [HttpPost]
        public async Task<ActionResult<BootstrapBlazor.Components.QueryData<ListExamineAloneModel>>> GetExamineListAsync(ExaminesPagesInfor input)
        {
            var dtos = await _examineService.GetPaginatedResult(input.Options, input.SearchModel);

            return dtos;
        }

        [HttpGet]
        public async Task<ActionResult<ListCommentsInforViewModel>> ListCommentsAsync()
        {
            ListCommentsInforViewModel model = new ListCommentsInforViewModel
            {
                All = await _commentRepository.LongCountAsync(),
                ArticleComments = await _commentRepository.LongCountAsync(s => s.Type == CommentType.CommentArticle),
                SpaceComments = await _commentRepository.LongCountAsync(s => s.Type == CommentType.CommentUser),
                ParentComments = await _commentRepository.LongCountAsync(s => s.Type != CommentType.ReplyComment),
                ChildComments = await _commentRepository.LongCountAsync(s => s.Type == CommentType.ReplyComment),
                Hiddens = await _commentRepository.CountAsync(x => x.IsHidden == true)
            };

            return model;
        }

        [HttpPost]
        public async Task<ActionResult<BootstrapBlazor.Components.QueryData<ListCommentAloneModel>>> GetCommentListAsync(CommentsPagesInfor input)
        {
            var dtos = await _commentService.GetPaginatedResult(input.Options, input.SearchModel);

            return dtos;
        }

        [HttpGet]
        public async Task<ActionResult<ListMessagesInforViewModel>> ListMessagesAsync()
        {
            ListMessagesInforViewModel model = new ListMessagesInforViewModel
            {
                All = await _messageRepository.LongCountAsync(),
                ReadedCount = await _messageRepository.LongCountAsync(s => s.IsReaded == true),
                NotReadedCount = await _messageRepository.LongCountAsync(s => s.IsReaded == false)
            };

            return model;
        }

        [HttpPost]
        public async Task<ActionResult<BootstrapBlazor.Components.QueryData<ListMessageAloneModel>>> GetMessageListAsync(MessagesPagesInfor input)
        {
            var dtos = await _messageService.GetPaginatedResult(input.Options, input.SearchModel);

            return dtos;
        }

        [HttpGet]
        public async Task<ActionResult<ListFilesInforViewModel>> ListFilesAsync()
        {
            ListFilesInforViewModel model = new ListFilesInforViewModel
            {
                All = await _userFileRepository.CountAsync()
            };

            return model;
        }

        [HttpPost]
        public async Task<ActionResult<BootstrapBlazor.Components.QueryData<ListFileAloneModel>>> GetFileListAsync(FilesPagesInfor input)
        {
            var dtos = await _fileService.GetPaginatedResult(input.Options, input.SearchModel);

            return dtos;
        }

        [HttpGet]
        public async Task<ActionResult<ListErrorCountsInforViewModel>> ListErrorCountsAsync()
        {
            ListErrorCountsInforViewModel model = new ListErrorCountsInforViewModel
            {
                All = await _errorCountRepository.LongCountAsync()
            };

            return model;
        }

        [HttpPost]
        public async Task<ActionResult<BootstrapBlazor.Components.QueryData<ListErrorCountAloneModel>>> GetErrorCountListAsync(ErrorCountsPagesInfor input)
        {
            var dtos = await _errorCountService.GetPaginatedResult(input.Options, input.SearchModel);

            return dtos;
        }

        [HttpGet]
        public async Task<ActionResult<ListFavoriteFoldersInforViewModel>> ListFavoriteFoldersAsync()
        {
            ListFavoriteFoldersInforViewModel model = new ListFavoriteFoldersInforViewModel
            {
                All = await _favoriteFolderRepository.LongCountAsync(),
                Defaults=await _favoriteFolderRepository.LongCountAsync(s=>s.IsDefault==true)
            };

            return model;
        }

        [HttpPost]
        public async Task<ActionResult<BootstrapBlazor.Components.QueryData<ListFavoriteFolderAloneModel>>> GetFavoriteFolderListAsync(FavoriteFoldersPagesInfor input)
        {
            var dtos = await _favoriteFolderService.GetPaginatedResult(input.Options, input.SearchModel, null);

            return dtos;
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        [HttpGet]
        public async Task<ActionResult<Models.ExaminedViewModel>> ExaminedAsync(int id = -1)
        {
            //获取审核单
            Examine examine = null;
            bool IsContinued = false;

            if (id == -1)
            {
                try
                {
                    examine = await _examineRepository.GetAll()
                        .Include(s => s.ApplicationUser)
                        .FirstOrDefaultAsync(x => x.IsPassed == null);
                    if (examine == null)
                    {
                        return NotFound();
                    }
                    IsContinued = true;
                }
                catch
                {
                    return NotFound();
                }
            }
            else
            {
                examine = await _examineRepository.GetAll()
                        .Include(s => s.ApplicationUser)
                        .FirstOrDefaultAsync(x => x.Id == id);
            }

            if (examine == null)
            {
                return NotFound();
            }
            //对应赋值
            Models.ExaminedViewModel model = new Models.ExaminedViewModel
            {
                Id = examine.Id,
                ApplicationUserId = examine.ApplicationUserId,
                ApplyTime = examine.ApplyTime,
                Operation = examine.Operation,
                ApplicationUserName = examine.ApplicationUser.UserName,
                Comments = examine.Comments,
                IsPassed = false,
                IsContinued = IsContinued,
                IsExamined = false,
                PassedAdminName = examine.PassedAdminName,
                Note = examine.Note
            };
            //判断是否有前置审核
            if (examine.PrepositionExamineId != null)
            {
                Examine temp = await _examineRepository.FirstOrDefaultAsync(s => s.Id == examine.PrepositionExamineId && s.IsPassed == null);
                if (temp == null)
                {
                    model.PrepositionExamineId = -1;
                }
                else
                {
                    model.PrepositionExamineId = (int)examine.PrepositionExamineId;
                }
            }
            //判断是否是等待审核状态
            if (examine.IsPassed != null)
            {
                model.PassedTime = (DateTime)examine.PassedTime;
                model.IsExamined = true;
                model.IsPassed = (bool)examine.IsPassed;
            }
            try
            {
                if (await _examineService.GetExamineView(model, examine) == false)
                {
                    return NotFound();
                }

            }
            catch (Exception exc)
            {
                return NotFound("生成审核视图出错");
            }
            return model;
        }
        [HttpPost]
        public async Task<ActionResult<Result>> ExaminedAsync(Models.ExaminedViewModel model)
        {
            Examine examine = await _examineRepository.FirstOrDefaultAsync(x => x.Id == model.Id);
            if (examine == null)
            {
                return NotFound();
            }
            if (examine.IsPassed != null)
            {
                return new Result { Successful = false, Error = "该记录已经被被审核，不能修改审核状态" };
            }
            ApplicationUser user = await _userManager.Users.SingleAsync(x => x.Id == examine.ApplicationUserId);
            if (user == null)
            {
                return NotFound();
            }
            //判断是否有前置审核
            if (examine.PrepositionExamineId != null)
            {
                Examine temp = await _examineRepository.FirstOrDefaultAsync(s => s.Id == examine.PrepositionExamineId && s.IsPassed == null);
                if (temp != null)
                {
                    return new Result { Successful = false, Error = $"该审核有一个前置审核,请先对前置审核进行审核，ID{examine.PrepositionExamineId}" };
                }
            }
            Article article = null;
            Comment comment = null;
            //获取当前管理员ID
            ApplicationUser userAdmin = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            //判断是否通过
            if (model.IsPassed == true)
            {

                //根据操作类别进行操作
                switch (examine.Operation)
                {
                    case Operation.UserMainPage:
                        await _examineService.ExamineEditUserMainPageAsync(user, examine.Context);
                        break;
                    case Operation.EditUserMain:
                       
                        //序列化数据
                        UserMain userMain = null;
                        using (TextReader str = new StringReader(examine.Context))
                        {
                            JsonSerializer serializer = new JsonSerializer();
                            userMain = (UserMain)serializer.Deserialize(str, typeof(UserMain));
                        }

                        await _examineService.ExamineEditUserMainAsync(user, userMain);
                        break;
                  
                    case Operation.EditArticleMain:

                        article = await _articleRepository.FirstOrDefaultAsync(s => s.Id == examine.ArticleId);
                        if (article == null)
                        {
                            return NotFound();
                        }
                        //序列化数据
                        ArticleMain articleMain = null;
                        using (TextReader str = new StringReader(examine.Context))
                        {
                            JsonSerializer serializer = new JsonSerializer();
                            articleMain = (ArticleMain)serializer.Deserialize(str, typeof(ArticleMain));
                        }

                        await _examineService.ExamineEditArticleMainAsync(article, articleMain);
                        break;
                    case Operation.EditArticleRelevanes:

                        article = await _articleRepository.GetAll().Include(s => s.Relevances).FirstOrDefaultAsync(s => s.Id == examine.ArticleId);
                        if (article == null)
                        {
                            return NotFound();
                        }

                        ArticleRelecancesModel articleRelecancesModel = null;
                        using (TextReader str = new StringReader(examine.Context))
                        {
                            JsonSerializer serializer = new JsonSerializer();
                            articleRelecancesModel = (ArticleRelecancesModel)serializer.Deserialize(str, typeof(ArticleRelecancesModel));
                        }
                        await _examineService.ExamineEditArticleRelevancesAsync(article, articleRelecancesModel);
                        break;
                    case Operation.EditArticleMainPage:

                        article = await _articleRepository.FirstOrDefaultAsync(s => s.Id == examine.ArticleId);
                        if (article == null)
                        {
                            return NotFound();
                        }
                        await _examineService.ExamineEditArticleMainPageAsync(article, examine.Context);
                        break;
                 
                    case Operation.PubulishComment:
                        comment = await _commentRepository.FirstOrDefaultAsync(s => s.Id == examine.CommentId);
                        if (comment == null)
                        {
                            return NotFound();
                        }
                        CommentText commentText = null;
                        using (TextReader str = new StringReader(examine.Context))
                        {
                            JsonSerializer serializer = new JsonSerializer();
                            commentText = (CommentText)serializer.Deserialize(str, typeof(CommentText));
                        }
                        await _appHelper.ExaminePublishCommentTextAsync(comment, commentText);
                        break;
                }

                //修改审核状态
                examine.Comments = model.Comments;
                examine.PassedAdminName = userAdmin.UserName;
                examine.PassedTime = DateTime.Now.ToCstTime();
                examine.IsPassed = true;
                examine.ContributionValue = model.ContributionValue;
                examine =await _examineRepository.UpdateAsync(examine);

                //更新用户积分
                await _appHelper.UpdateUserIntegral(user);
            }
            else
            {
                article = await _articleRepository.FirstOrDefaultAsync(s => s.Id == examine.ArticleId);
                comment = await _commentRepository.FirstOrDefaultAsync(s => s.Id == examine.CommentId);
                //修改审核状态
                examine.Comments = model.Comments;
                examine.PassedAdminName = userAdmin.UserName;
                examine.PassedTime = DateTime.Now.ToCstTime();
                examine.IsPassed = false;
                examine =await _examineRepository.UpdateAsync(examine);
                //修改以其为前置审核的审核状态
                if (await _examineRepository.CountAsync(s => s.IsPassed == null && s.PrepositionExamineId == examine.Id) != 0)
                {
                    var temp = _examineRepository.GetAll().Where(s => s.IsPassed == null && s.PrepositionExamineId == examine.Id);
                    foreach (var item in temp)
                    {
                        item.IsPassed = false;
                        item.Comments = "其前置审核被驳回";
                        item.PassedTime = DateTime.Now.ToCstTime();
                        item.PassedAdminName = userAdmin.UserName;
                        await _examineRepository.UpdateAsync(item);
                    }
                }
            }
            //给用户发送通知
            if (article != null)
            {
                if (examine.PrepositionExamineId == null || examine.PrepositionExamineId <= 0)
                {
                    //查找是否有以此为前置审核的审核 如果有 则代表第一次创建
                    if (await _examineRepository.CountAsync(s => s.IsPassed == null && s.PrepositionExamineId == examine.Id) != 0)
                    {
                        await _messageRepository.InsertAsync(new Message
                        {
                            Title = (examine.IsPassed ?? false) ? "文章发布成功提醒" : "文章发布驳回提醒",
                            PostTime = DateTime.Now.ToCstTime(),
                            Image = "default/logo.png",
                            Rank = "系统",
                            Text = "你的文章『" + article.Name + "』被管理员『" + userAdmin.UserName + "』" + ((examine.IsPassed ?? false) ? "审核通过，已经成功发布，感谢你为软件开发联盟做出的贡献" : "驳回") + (string.IsNullOrWhiteSpace(examine.Comments) ? "" : "，批注『" + examine.Comments + "』"),
                            Link = "articles/index/" + examine.Article.Id,
                            LinkTitle = article.Name,
                            Type = (examine.IsPassed ?? false) ? MessageType.ExaminePassed : MessageType.ExamineUnPassed,
                            ApplicationUser = user,
                            ApplicationUserId = user.Id
                        });
                    }
                    else
                    {
                        await _messageRepository.InsertAsync(new Message
                        {
                            Title = (examine.IsPassed ?? false) ? "编辑通过提醒" : "编辑驳回提醒",
                            PostTime = DateTime.Now.ToCstTime(),
                            Image = "default/logo.png",
                            Rank = "系统",
                            Text = "你对文章『" + article.Name + "』的『" + examine.Operation.GetDisplayName() + "』操作被管理员『" + userAdmin.UserName + "』" + ((examine.IsPassed ?? false) ? "通过，感谢你为软件开发联盟做出的贡献" : "驳回") + (string.IsNullOrWhiteSpace(examine.Comments) ? "" : "，批注『" + examine.Comments + "』"),
                            Link = "articles/index/" + article.Id,
                            LinkTitle = article.Name,
                            Type = (examine.IsPassed ?? false) ? MessageType.ExaminePassed : MessageType.ExamineUnPassed,
                            ApplicationUser = user,
                            ApplicationUserId = user.Id
                        });
                    }
                }

            }
            else if(comment!=null)
            {
                await _messageRepository.InsertAsync(new Message
                {
                    Title = (examine.IsPassed ?? false) ? "评论审核通过提醒" : "评论驳回提醒",
                    PostTime = DateTime.Now.ToCstTime(),
                    Image = "default/logo.png",
                    Rank = "系统",
                    Text = "你的评论『\n" + _appHelper.GetStringAbbreviation(comment.Text,20) + "\n』被管理员『" + userAdmin.UserName + "』" + ((examine.IsPassed ?? false) ? "通过，感谢你为软件开发联盟做出的贡献" : "驳回") + (string.IsNullOrWhiteSpace(examine.Comments) ? "" : "，批注『" + examine.Comments + "』"),
                    Link = "",
                    LinkTitle = "",
                    Type = (examine.IsPassed ?? false) ? MessageType.ExaminePassed : MessageType.ExamineUnPassed,
                    ApplicationUser = user,
                    ApplicationUserId = user.Id
                });
            }
            else
            {
                await _messageRepository.InsertAsync(new Message
                {
                    Title = (examine.IsPassed ?? false) ? "编辑通过提醒" : "编辑驳回提醒",
                    PostTime = DateTime.Now.ToCstTime(),
                    Image = "default/logo.png",
                    Rank = "系统",
                    Text = "你的『" + examine.Operation.GetDisplayName() + "』操作被管理员『" + userAdmin.UserName + "』" + ((examine.IsPassed ?? false) ? "通过，感谢你为软件开发联盟做出的贡献" : "驳回") + (string.IsNullOrWhiteSpace(examine.Comments) ? "" : "，批注『" + examine.Comments + "』"),
                    Link = "home/examined/" + examine.Id,
                    LinkTitle = "第" + examine.Id + "条审核记录",
                    Type = (examine.IsPassed ?? false) ? MessageType.ExaminePassed : MessageType.ExamineUnPassed,
                    ApplicationUser = user,
                    ApplicationUserId = user.Id
                });

            }

            return new Result { Successful = true };
        }

        [HttpGet]
        public async Task<ActionResult<ManageHomeViewModel>> ManageHomeAsync()
        {
            ManageHomeViewModel model = new ManageHomeViewModel
            {
                Links = await _friendLinkRepository.GetAll().OrderByDescending(s => s.Priority).ToListAsync()
            };
            foreach (var item in model.Links)
            {
                item.Image = _appHelper.GetImagePath(item.Image, "");
            }
            model.Carousels = await _carouselRepository.GetAll().OrderByDescending(s => s.Priority).ToListAsync();
            foreach (var item in model.Carousels)
            {
                item.Image = _appHelper.GetImagePath(item.Image, "");
            }


            model.AppImage = _appHelper.GetImagePath("", "app.png");
            model.UserImage = _appHelper.GetImagePath("", "user.png");
            model.UserBackgroundImage = _appHelper.GetImagePath("", "userbackground.jpg");
            model.CertificateImage = _appHelper.GetImagePath("", "certificate.png");
            model.BackgroundImage = _appHelper.GetImagePath("", "background.png");

            return model;
        }

        [HttpGet]
        public async Task<ActionResult<EditCarouselsViewModel>> EditCarouselsAsync()
        {
            //根据类别生成首个视图模型
            EditCarouselsViewModel model = new EditCarouselsViewModel
            {
                Carousels = new List<CarouselModel>()
            };
            List<Carousel> carousels = await _carouselRepository.GetAll().OrderByDescending(s => s.Priority).ToListAsync();
            foreach (var item in carousels)
            {
                model.Carousels.Add(new CarouselModel
                {
                    Link = item.Link,
                    Priority=item.Priority,
                    ImagePath = _appHelper.GetImagePath(item.Image, "")
                });

            }

            return model;
        }
        [HttpPost]
        public async Task<ActionResult<Result>> EditCarouselsAsync(EditCarouselsViewModel model)
        {
            //先把删除当前所有图片
            List<Carousel> carousels = await _carouselRepository.GetAll().ToListAsync();
            foreach (var item in carousels)
            {
                //_appHelper.DeleteImage(item.Image);
                await _carouselRepository.DeleteAsync(item);
            }
            //循环添加视图中的图片
            if (model.Carousels != null)
            {
                foreach (var item in model.Carousels)
                {
                    await _carouselRepository.InsertAsync(new Carousel
                    {
                        Image = item.ImagePath == "background.png" ? "" : item.ImagePath,
                        Link = item.Link,
                        Priority=item.Priority
                    });
                }
            }
            return new Result { Successful = true };
        }

        [HttpGet]
        public async Task<ActionResult<EditFriendLinksViewModel>> EditFriendLinksAsync()
        {
            //根据类别生成首个视图模型
            EditFriendLinksViewModel model = new EditFriendLinksViewModel
            {
                FriendLinks = new List<FriendLinkModel>()
            };
            List<FriendLink> friendLinks = await _friendLinkRepository.GetAll().OrderByDescending(s => s.Priority).ToListAsync();
            foreach (var item in friendLinks)
            {
                model.FriendLinks.Add(new FriendLinkModel
                {                  
                    Link = item.Link,
                    Name = item.Name,
                    Priority=item.Priority,
                    ImagePath = _appHelper.GetImagePath(item.Image, "app.png")
                });
            }
            return model;
        }

        [HttpPost]
        public async Task<ActionResult<Result>> EditFriendLinksAsync(EditFriendLinksViewModel model)
        {

            //先把删除当前所有图片
            List<FriendLink> friendLinks = await _friendLinkRepository.GetAll().ToListAsync();
            foreach (var item in friendLinks)
            {
                //_appHelper.DeleteImage(item.Image);
                await _friendLinkRepository.DeleteAsync(item);
            }
            //循环添加视图中的图片
            if (model.FriendLinks != null)
            {
                foreach (var item in model.FriendLinks)
                {
                    await _friendLinkRepository.InsertAsync(new FriendLink
                    {
                        Image = item.ImagePath == "background.png" ? "" : item.ImagePath,
                        Name = item.Name,
                        Link = item.Link,
                        Priority=item.Priority
                    });
                }
            }
            return new Result { Successful = true };
        }

        /// <summary>
        /// 临时脚本
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<Result>> TempFunction()
        {
            await _appHelper.DeleteAllBackupInfor();
            return new Result { Successful = true };
        }
        /// <summary>
        /// 获取网站数据概览
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<OverviewDataViewModel>> GetOverviewData()
        {
            try
            {
                DateTime tempDateTimeNow = DateTime.Now.ToCstTime();
                //获取数据
                OverviewDataViewModel model = new OverviewDataViewModel
                {
                    TotalRegisterCount = await _userRepository.CountAsync(),
                    YesterdayRegisterCount = await _userRepository.CountAsync(s => s.RegistTime.Date == tempDateTimeNow.AddDays(-1).Date),
                    TodayRegisterCount = await _userRepository.CountAsync(s => s.RegistTime.Date == tempDateTimeNow.Date),
                    TodayOnlineCount = await _userRepository.CountAsync(s => s.LastOnlineTime.Date == tempDateTimeNow.Date),

                    TotalArticleCount = await _articleRepository.LongCountAsync(),
                    YesterdayCreateArticleCount = await _articleRepository.LongCountAsync(s => s.CreateTime.Date == tempDateTimeNow.AddDays(-1).Date),
                    YesterdayEditArticleCount = await _examineRepository.LongCountAsync(s => s.IsPassed == true && s.PassedTime != null && s.PassedTime.Value.Date == tempDateTimeNow.AddDays(-1).Date && (s.Operation == Operation.EditArticleMain || s.Operation == Operation.EditArticleRelevanes || s.Operation == Operation.EditArticleMainPage)),
                    TodayCreateArticleCount = await _articleRepository.LongCountAsync(s => s.CreateTime.Date == tempDateTimeNow.Date),
                    TodayEditArticleCount = await _articleRepository.LongCountAsync(s => s.LastEditTime.Date == tempDateTimeNow.Date),

                    TotalExamineCount = await _examineRepository.LongCountAsync(),
                    YesterdayTotalExamineCount = await _examineRepository.LongCountAsync(s => s.ApplyTime.Date == tempDateTimeNow.AddDays(-1).Date),
                    TodayTotalExamineCount = await _examineRepository.LongCountAsync(s => s.ApplyTime.Date == tempDateTimeNow.Date),
                    TotalExaminingCount = await _examineRepository.LongCountAsync(s => s.IsPassed == null),

                    TotalCommentCount = await _commentRepository.LongCountAsync(),
                    YesterdayCommentCount = await _commentRepository.LongCountAsync(s => s.CommentTime.Date == tempDateTimeNow.AddDays(-1).Date),
                    TodayCommentCount = await _commentRepository.LongCountAsync(s => s.CommentTime.Date == tempDateTimeNow.Date),

                    TotalMessageCount = await _messageRepository.LongCountAsync(),
                    YesterdayMessageCount = await _messageRepository.LongCountAsync(s => s.PostTime.Date == tempDateTimeNow.AddDays(-1).Date),
                    TodayMessageCount = await _messageRepository.LongCountAsync(s => s.PostTime.Date == tempDateTimeNow.Date),

                    TotalFileCount = await _userFileRepository.CountAsync(),
                    TotalFileSpace = await _userFileRepository.GetAll().SumAsync(s => s.FileSize) ?? 0,
                    YesterdayFileCount = await _userFileRepository.CountAsync(s => s.UploadTime.Date == tempDateTimeNow.AddDays(-1).Date),
                    YesterdayFileSpace = await _userFileRepository.GetAll().Where(s => s.UploadTime.Date == tempDateTimeNow.AddDays(-1).Date).SumAsync(s => s.FileSize) ?? 0,
                    TodayFileCount = await _userFileRepository.CountAsync(s => s.UploadTime.Date == tempDateTimeNow.Date),
                    TodayFileSpace = await _userFileRepository.GetAll().Where(s => s.UploadTime.Date == tempDateTimeNow.Date).SumAsync(s => s.FileSize) ?? 0,
                };

                return model;
            }
            catch (Exception exc)
            {
                return BadRequest(exc.Message);
            }

        }

        /// <summary>
        /// 图表天数
        /// </summary>
        public const int MaxCountLineDay = 30;

        /// <summary>
        /// 获取用户图表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<BootstrapBlazor.Components.ChartDataSource>> GetUserCountLineAsync()
        {
            DateTime tempDateTimeNow = DateTime.Now.ToCstTime();

            //获取数据
            var registerCounts = await _userRepository.GetAll().Where(s => s.RegistTime.Date > tempDateTimeNow.Date.AddDays(-MaxCountLineDay))
               // 先进行了时间字段变更为String字段，切只保留到天
               // 采用拼接的方式
               .Select(n => new { Time = n.RegistTime.Date })
               // 分类
               .GroupBy(n => n.Time)
               // 返回汇总样式
               .Select(n => new CountLineModel { Time = n.Key, Count = n.Count() })
               .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
               .ToListAsync();

            var onlineCounts = await _userOnlineInforRepository.GetAll().Where(s => s.Date.Date > tempDateTimeNow.Date.AddDays(-MaxCountLineDay))
             // 先进行了时间字段变更为String字段，切只保留到天
             // 采用拼接的方式
             .Select(n => new { Time = n.Date.Date })
             // 分类
             .GroupBy(n => n.Time)
             // 返回汇总样式
             .Select(n => new CountLineModel { Time = n.Key, Count = n.Count() })
             .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
             .ToListAsync();


            var SignIns = await _signInDayRepository.GetAll().Where(s => s.Time.Date > tempDateTimeNow.Date.AddDays(-MaxCountLineDay))
             // 先进行了时间字段变更为String字段，切只保留到天
             // 采用拼接的方式
             .Select(n => new { Time = n.Time.Date })
             // 分类
             .GroupBy(n => n.Time)
             // 返回汇总样式
             .Select(n => new CountLineModel { Time = n.Key, Count = n.Count() })
             .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
             .ToListAsync();

            var temp = _appHelper.GetCountLine(new Dictionary<string, List<CountLineModel>> { ["注册"] = registerCounts, ["在线"] = onlineCounts, ["签到"] = SignIns }, "日期", "数目", "用户");
            return temp;
        }

        /// <summary>
        /// 获取文章图表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<BootstrapBlazor.Components.ChartDataSource>> GetArticleCountLineAsync()
        {
            DateTime tempDateTimeNow = DateTime.Now.ToCstTime();
            //获取数据
            var createCounts = await _articleRepository.GetAll().Where(s => s.CreateTime.Date > tempDateTimeNow.Date.AddDays(-MaxCountLineDay))
               // 先进行了时间字段变更为String字段，切只保留到天
               // 采用拼接的方式
               .Select(n => new { Time = n.CreateTime.Date })
               // 分类
               .GroupBy(n => n.Time)
               // 返回汇总样式
               .Select(n => new CountLineModel { Time = n.Key, Count = n.Count() })
               .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
               .ToListAsync();

            var editCounts = await _examineRepository.GetAll().Where(s => (s.Operation == Operation.EditArticleMain || s.Operation == Operation.EditArticleRelevanes || s.Operation == Operation.EditArticleMainPage)
                                                                           && s.IsPassed == true && s.ApplyTime.Date > tempDateTimeNow.Date.AddDays(-MaxCountLineDay))
               // 先进行了时间字段变更为String字段，切只保留到天
               // 采用拼接的方式
               .Select(n => new { Time = n.ApplyTime.Date })
               // 分类
               .GroupBy(n => n.Time)
               // 返回汇总样式
               .Select(n => new CountLineModel { Time = n.Key, Count = n.Count() })
               .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
               .ToListAsync();
            var thumsupCounts = await _thumbsUpRepository.GetAll().Where(s => s.ThumbsUpTime.Date > tempDateTimeNow.Date.AddDays(-MaxCountLineDay))
               // 先进行了时间字段变更为String字段，切只保留到天
               // 采用拼接的方式
               .Select(n => new { Time = n.ThumbsUpTime.Date })
               // 分类
               .GroupBy(n => n.Time)
               // 返回汇总样式
               .Select(n => new CountLineModel { Time = n.Key, Count = n.Count() })
               .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
               .ToListAsync();

            var commentCounts = await _commentRepository.GetAll().Where(s => s.Type == CommentType.CommentArticle && s.CommentTime.Date > tempDateTimeNow.Date.AddDays(-MaxCountLineDay))
              // 先进行了时间字段变更为String字段，切只保留到天
              // 采用拼接的方式
              .Select(n => new { Time = n.CommentTime.Date })
              // 分类
              .GroupBy(n => n.Time)
              // 返回汇总样式
              .Select(n => new CountLineModel { Time = n.Key, Count = n.Count() })
              .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
              .ToListAsync();

            var favoriteCounts = await _favoriteObjectRepository.GetAll().Where(s => s.Type == FavoriteObjectType.Article && s.CreateTime.Date > tempDateTimeNow.Date.AddDays(-MaxCountLineDay))
               // 先进行了时间字段变更为String字段，切只保留到天
               // 采用拼接的方式
               .Select(n => new { Time = n.CreateTime.Date })
               // 分类
               .GroupBy(n => n.Time)
               // 返回汇总样式
               .Select(n => new CountLineModel { Time = n.Key, Count = n.Count() })
               .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
               .ToListAsync();

            var temp = _appHelper.GetCountLine(new Dictionary<string, List<CountLineModel>> { ["发表"] = createCounts, ["编辑"] = editCounts, ["点赞"] = thumsupCounts,["评论"] = commentCounts, ["收藏"] = favoriteCounts }, "日期", "数目", "文章");
            return temp;
        }

        /// <summary>
        /// 获取审核图表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<BootstrapBlazor.Components.ChartDataSource>> GetExamineCountLineAsync()
        {
            DateTime tempDateTimeNow = DateTime.Now.ToCstTime();
            //获取数据
            var applyCounts = await _examineRepository.GetAll().Where(s =>s.ApplyTime.Date > tempDateTimeNow.Date.AddDays(-MaxCountLineDay))
               // 先进行了时间字段变更为String字段，切只保留到天
               // 采用拼接的方式
               .Select(n => new { Time = n.ApplyTime.Date })
               // 分类
               .GroupBy(n => n.Time)
               // 返回汇总样式
               .Select(n => new CountLineModel { Time = n.Key, Count = n.Count() })
               .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
               .ToListAsync();

            var passCounts = await _examineRepository.GetAll().Where(s => s.PassedTime != null && s.PassedTime.Value.Date > tempDateTimeNow.Date.AddDays(-MaxCountLineDay))
               // 先进行了时间字段变更为String字段，切只保留到天
               // 采用拼接的方式
               .Select(n => new { Time = n.PassedTime.Value.Date })
               // 分类
               .GroupBy(n => n.Time)
               // 返回汇总样式
               .Select(n => new CountLineModel { Time = n.Key, Count = n.Count() })
               .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
               .ToListAsync();

            var temp = _appHelper.GetCountLine(new Dictionary<string, List<CountLineModel>> { ["申请"] = applyCounts, ["处理"] = passCounts }, "日期", "数目", "审核");
            return temp;
        }

        /// <summary>
        /// 获取评论图表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<BootstrapBlazor.Components.ChartDataSource>> GetCommentCountLineAsync()
        {
            DateTime tempDateTimeNow = DateTime.Now.ToCstTime();
            //获取数据
            var publishCounts = await _commentRepository.GetAll().Where(s => s.CommentTime.Date > tempDateTimeNow.Date.AddDays(-MaxCountLineDay))
               // 先进行了时间字段变更为String字段，切只保留到天
               // 采用拼接的方式
               .Select(n => new { Time = n.CommentTime.Date })
               // 分类
               .GroupBy(n => n.Time)
               // 返回汇总样式
               .Select(n => new CountLineModel { Time = n.Key, Count = n.Count() })
               .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
               .ToListAsync();

            var temp = _appHelper.GetCountLine(new Dictionary<string, List<CountLineModel>> { ["发表"] = publishCounts }, "日期", "数目", "评论");
            return temp;
        }


        /// <summary>
        /// 获取消息图表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<BootstrapBlazor.Components.ChartDataSource>> GetMessageCountLineAsync()
        {
            DateTime tempDateTimeNow = DateTime.Now.ToCstTime();
            //获取数据
            var postCounts = await _messageRepository.GetAll().Where(s => s.PostTime.Date > tempDateTimeNow.Date.AddDays(-MaxCountLineDay))
               // 先进行了时间字段变更为String字段，切只保留到天
               // 采用拼接的方式
               .Select(n => new { Time = n.PostTime.Date })
               // 分类
               .GroupBy(n => n.Time)
               // 返回汇总样式
               .Select(n => new CountLineModel { Time = n.Key, Count = n.Count() })
               .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
               .ToListAsync();

            var temp = _appHelper.GetCountLine(new Dictionary<string, List<CountLineModel>> { ["发送"] = postCounts }, "日期", "数目", "消息");
            return temp;
        }

        /// <summary>
        /// 获取文件图表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<BootstrapBlazor.Components.ChartDataSource>> GetFileCountLineAsync()
        {
            DateTime tempDateTimeNow = DateTime.Now.ToCstTime();
            //获取数据
            var Counts = await _userFileRepository.GetAll().Where(s => s.UploadTime.Date > tempDateTimeNow.Date.AddDays(-MaxCountLineDay))
               // 先进行了时间字段变更为String字段，切只保留到天
               // 采用拼接的方式
               .Select(n => new { Time = n.UploadTime.Date })
               // 分类
               .GroupBy(n => n.Time)
               // 返回汇总样式
               .Select(n => new CountLineModel { Time = n.Key, Count = n.Count() })
               .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
               .ToListAsync();

            var Spaces = await _userFileRepository.GetAll().Where(s => s.UploadTime.Date > tempDateTimeNow.Date.AddDays(-MaxCountLineDay))
               // 先进行了时间字段变更为String字段，切只保留到天
               // 采用拼接的方式
               .Select(n => new { Time = n.UploadTime.Date,Space=((double)n.FileSize)/(1024*1024) })
               // 分类
               .GroupBy(n => n.Time)
               // 返回汇总样式
               .Select(n => new CountLineModel { Time = n.Key, Count = n.Sum(s=>s.Space) })
               .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
               .ToListAsync();

            var temp = _appHelper.GetCountLine(new Dictionary<string, List<CountLineModel>> { ["数目"] = Counts, ["大小 MB"] = Spaces }, "日期", "", "文件");
            return temp;
        }

        /// <summary>
        /// 获取备份图表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<BootstrapBlazor.Components.ChartDataSource>> GetBackUpArchiveCountLineAsync()
        {
            DateTime tempDateTimeNow = DateTime.Now.ToCstTime();
            //获取数据
            var success = await _backUpArchiveDetailRepository.GetAll().Where(s => s.IsFail == false && s.BackUpTime.Date > tempDateTimeNow.Date.AddDays(-MaxCountLineDay))
               // 先进行了时间字段变更为String字段，切只保留到天
               // 采用拼接的方式
               .Select(n => new { Time = n.BackUpTime.Date })
               // 分类
               .GroupBy(n => n.Time)
               // 返回汇总样式
               .Select(n => new CountLineModel { Time = n.Key, Count = n.Count() })
               .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
               .ToListAsync();

            var times = await _backUpArchiveDetailRepository.GetAll().Where(s => s.BackUpTime.Date > tempDateTimeNow.Date.AddDays(-MaxCountLineDay))
               // 先进行了时间字段变更为String字段，切只保留到天
               // 采用拼接的方式
               .Select(n => new { Time = n.BackUpTime.Date, n.TimeUsed })
               // 分类
               .GroupBy(n => n.Time)
               // 返回汇总样式
               .Select(n => new CountLineModel { Time = n.Key, Count = n.Average(s => s.TimeUsed) })
               .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
               .ToListAsync();

            var errors = await _backUpArchiveDetailRepository.GetAll().Where(s => s.IsFail == true && s.BackUpTime.Date > tempDateTimeNow.Date.AddDays(-MaxCountLineDay))
               // 先进行了时间字段变更为String字段，切只保留到天
               // 采用拼接的方式
               .Select(n => new { Time = n.BackUpTime.Date })
               // 分类
               .GroupBy(n => n.Time)
               // 返回汇总样式
               .Select(n => new CountLineModel { Time = n.Key, Count = n.Count() })
               .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
               .ToListAsync();

            var temp = _appHelper.GetCountLine(new Dictionary<string, List<CountLineModel>> { ["成功"] = success,  ["错误"] = errors ,["用时 秒"] = times}, "日期", "", "备份");
            return temp;
        }
    }
}
