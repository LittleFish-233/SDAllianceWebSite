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
using SDAllianceWebSite.Shared.ViewModel.Favorites;
using TencentCloud.Ame.V20190916.Models;
using SDAllianceWebSite.Shared.ViewModel.Space;
using SDAllianceWebSite.Shared.ViewModel.Coments;
using SDAllianceWebSite.APIServer.Application.Favorites;
using SDAllianceWebSite.Shared.ViewModel.Search;

namespace SDAllianceWebSite.APIServer.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/Favorites/[action]")]
    public class FavoriteFolderAPIController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<Article, long> _articleRepository;
        private readonly IRepository<ApplicationUser, string> _userRepository;
        private readonly IRepository<FavoriteFolder, long> _favoriteFolderRepository;
        private readonly IRepository<FavoriteObject, long> _favoriteObjectRepository;
        private readonly IAppHelper _appHelper;
        private readonly IFavoriteFolderService _favoriteFolderService;
        private readonly IFavoriteObjectService _favoriteObjectService;

        public FavoriteFolderAPIController( IRepository<FavoriteFolder, long> favoriteFolderRepository,
        IRepository<ApplicationUser, string> userRepository,  IRepository<FavoriteObject, long> favoriteObjectRepository,
        UserManager<ApplicationUser> userManager, IFavoriteObjectService favoriteObjectService,
        IRepository<Article, long> articleRepository, IAppHelper appHelper,IFavoriteFolderService favoriteFolderService)
        {
            this._userManager = userManager;
            _appHelper = appHelper;
            _articleRepository = articleRepository;
            _userRepository = userRepository;
            _favoriteFolderRepository = favoriteFolderRepository;
            _favoriteObjectRepository = favoriteObjectRepository;
            _favoriteFolderService = favoriteFolderService;
            _favoriteObjectService = favoriteObjectService;
        }

        [HttpPost]
        public async Task<ActionResult<Result>> CreateFavoriteFolderAsync(CreateFavoriteFolderViewModel model)
        {
            //获取当前用户ID
            ApplicationUser user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //检查是否重名
            if (await _favoriteFolderRepository.GetAll().AnyAsync(s => s.Name == model.Name))
            {
                return new Result { Successful = false, Error = "已经存在名称为“" + model.Name + "”的收藏夹" };
            }
        

            await _favoriteFolderRepository.InsertAsync(new FavoriteFolder
            {
                Name = model.Name,
                IsDefault = model.IsDefault,
                BriefIntroduction = model.BriefIntroduction,
                ApplicationUser = user,
                ApplicationUserId = user.Id,
                CreateTime=DateTime.Now.ToCstTime()
            });

            return new Result { Successful = true };
        }

        [HttpPost]
        public async Task<ActionResult<Result>> AddFavoriteObjectAsync(AddFavoriteObjectViewModel model)
        {
            //获取当前用户ID
            ApplicationUser user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            var favoriteFolders = await _favoriteFolderRepository.GetAll().Where(s => model.FavoriteFolderIds.Contains(s.Id) && s.ApplicationUserId == user.Id).ToListAsync();
            if (favoriteFolders == null||favoriteFolders.Count==0)
            {
                return new Result { Successful = false, Error = "无法找到收藏夹" };
            }

            //查找对应对象
            if (model.Type == FavoriteObjectType.Article)
            {
                foreach (var temp in favoriteFolders)
                {
                    //查找是否已经添加
                    if (await _favoriteObjectRepository.GetAll().AnyAsync(s => s.FavoriteFolderId == temp.Id && s.ArticleId == model.ObjectId))
                    {
                        continue;
                    }

                    var item = await _articleRepository.FirstOrDefaultAsync(s => s.Id == model.ObjectId);
                    if (item == null)
                    {
                        return new Result { Successful = false, Error = "无法找到该文章" };

                    }
                    await _favoriteObjectRepository.InsertAsync(new FavoriteObject
                    {
                        FavoriteFolder = temp,
                        FavoriteFolderId = temp.Id,
                        Article = item,
                        ArticleId = item.Id,
                        Type = FavoriteObjectType.Article,
                        CreateTime = DateTime.Now.ToCstTime()
                    });
                }
            }

          


            //更新数目
            await _appHelper.UpdateFavoritesCountAsync(model.FavoriteFolderIds);

            return new Result { Successful = true };
        }

        /// <summary>
        /// 在词条页面调用 删除该词条的所有收藏
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<Result>> UnFavoriteObjectsAsync(UnFavoriteObjectsModel model)
        {
            //获取当前用户ID
            ApplicationUser user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //获取所有收藏夹Id
            var favoriteFolderIds = await _favoriteFolderRepository.GetAll().Where(s => s.ApplicationUserId == user.Id).Select(s => s.Id).ToArrayAsync();

            if(model.Type==FavoriteObjectType.Article)
            {
             
                await _favoriteObjectRepository.DeleteRangeAsync(s => s.ArticleId == model.ObjectId && s.Type == FavoriteObjectType.Article && favoriteFolderIds.Contains(s.FavoriteFolderId));
            }
            //更新数目
            await _appHelper.UpdateFavoritesCountAsync(favoriteFolderIds);
            return new Result { Successful = true };
        }

        
        private async Task<FavoriteFoldersViewModel> GetUserFavoriteFolders(string userId)
        {
            FavoriteFoldersViewModel model = new FavoriteFoldersViewModel
            {
                Favorites = new List<FavoriteFolderAloneModel>()
            };
            //获取当前用户ID
            ApplicationUser currentUser = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //判断是否为管理员
            bool isAdmin = false;
            if(currentUser!=null)
            {
                isAdmin=   await _userManager.IsInRoleAsync(currentUser, "Admin");
            }
         
            ApplicationUser user = await _userRepository.GetAll().Include(s => s.FavoriteFolders).FirstOrDefaultAsync(s => s.Id == userId);
            if (user == null)
            {
                return null;
            }

            //判断是否有权限获取
            if (isAdmin == false)
            {
                if (currentUser == null)
                {
                    if (user.IsShowFavotites == false)
                    {
                        return null;
                    }
                }
                else
                {
                    if (currentUser.Id != userId && user.IsShowFavotites == false)
                    {
                        return null;
                    }
                }
            }
          
            //如果没有收藏夹则创建默认收藏夹
            if (user.FavoriteFolders.Count == 0)
            {
                var favoritFolder = new FavoriteFolder
                {
                    ApplicationUser = user,
                    ApplicationUserId = user.Id,
                    Name = "默认收藏夹",
                    BriefIntroduction = "系统自动创建的默认收藏夹",
                    IsDefault = true,
                    CreateTime = DateTime.Now.ToCstTime()
                };
                user.FavoriteFolders.Add(favoritFolder);
                await _favoriteFolderRepository.InsertAsync(favoritFolder);
            }


            foreach (var item in user.FavoriteFolders)
            {
                model.Favorites.Add(new FavoriteFolderAloneModel
                {
                    Id = item.Id,
                    BriefIntroduction = item.BriefIntroduction,
                    IsDefault = item.IsDefault,
                    MainImage = _appHelper.GetImagePath(item.MainImage, "app.png"),
                    Count = item.Count,
                    CreateTime = item.CreateTime,
                    Name = item.Name
                });
            }

            return model;
        }

       [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<FavoriteFoldersViewModel>> GetUserFavoriteFoldersAsync(string id)
        {
            return await GetUserFavoriteFolders(id);
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<FavoriteFoldersViewModel>> GetUserFavoriteInforFromFolderIdAsync(string id)
        {
            //获取关联用户Id
            var currentUserId = (await _favoriteFolderRepository.GetAll().FirstOrDefaultAsync(s => s.Id.ToString() == id))?.ApplicationUserId;
            if(string.IsNullOrWhiteSpace(currentUserId))
            {
                return NotFound("未找到该收藏夹");
            }

            return await GetUserFavoriteFolders(currentUserId);
        }


        /// <summary>
        /// 在用户管理收藏词条页面调用 删除指定收藏
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<Result>> UserDeleteFavoriteObjectAsync(DeleteFavoriteObjectsModel model)
        {
            //获取当前用户ID
            ApplicationUser user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //判断是否为管理员
            bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            //判断是否为当前用户
            if(isAdmin==false&&await _favoriteFolderRepository.GetAll().AnyAsync(s=>s.Id==model.FavorieFolderId&&s.ApplicationUserId==user.Id)==false)
            {
                return new Result { Successful = false, Error = "访问被拒绝" };
            }

            await _favoriteObjectRepository.DeleteRangeAsync(s => s.FavoriteFolderId == model.FavorieFolderId && model.Ids.Contains(s.Id));

            //更新数目
            await _appHelper.UpdateFavoritesCountAsync(new long[] { model.FavorieFolderId });

            return new Result { Successful = true };
        }

        [HttpPost]
        public async Task<ActionResult<Result>> UserDeleteFavoriteFolderAsync(DeleteFavoriteFoldersModel model)
        {
            //获取当前用户ID
            ApplicationUser user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //判断是否为管理员
            bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            await _favoriteFolderRepository.DeleteRangeAsync(s => (isAdmin || s.ApplicationUserId == user.Id) && model.Ids.Contains(s.Id));

            return new Result { Successful = true };

        }

        [HttpPost]
        public async Task<ActionResult<Result>> UserSetFavoriteFolderDefaultAsync(SetDefaultFavoriteFolderModel model)
        {
            //获取当前用户ID
            ApplicationUser user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //判断是否为管理员
            bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            //判断是否为当前用户
            if (isAdmin == false && await _favoriteFolderRepository.GetAll().CountAsync(s => model.Ids.Contains(s.Id) && s.ApplicationUserId == user.Id) == model.Ids.Length)
            {
                return new Result { Successful = false, Error = "访问被拒绝" };
            }

            await _favoriteFolderRepository.GetRangeUpdateTable().Where(s => model.Ids.Contains(s.Id)).Set(s => s.IsDefault, b => model.IsDefault).ExecuteAsync();

            return new Result { Successful = true };
        }

        [HttpPost]
        public async Task<ActionResult<BootstrapBlazor.Components.QueryData<ListFavoriteFolderAloneModel>>> GetFavoriteFolderListAsync(FavoriteFoldersPagesInfor input)
        {
            //检查 目标用户id与当前用户id不一致 必须要求管理员身份
            //获取当前用户ID
            ApplicationUser user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //判断是否为管理员
            bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            //判断是否为当前用户
            if (isAdmin == false &&input.UserId!=user.Id)
            {
                return NotFound();
            }

            var dtos = await _favoriteFolderService.GetPaginatedResult(input.Options, input.SearchModel,input.UserId);

            return dtos;
        }

        [HttpPost]
        public async Task<ActionResult<BootstrapBlazor.Components.QueryData<ListFavoriteObjectAloneModel>>> GetFavoriteObjectListAsync(FavoriteObjectsPagesInfor input)
        {
            //获取当前用户ID
            ApplicationUser user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //判断是否为管理员
            bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            //判断是否为当前用户
            if (isAdmin == false && await _favoriteFolderRepository.GetAll().AnyAsync(s => s.Id == input.FavoriteFolderId && s.ApplicationUserId == user.Id) == false)
            {
                return NotFound();
            }

            var dtos = await _favoriteObjectService.GetPaginatedResult(input.Options, input.SearchModel,input.FavoriteFolderId);

            return dtos;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<PagedResultDto<FavoriteObjectAloneViewModel>> GetUserFavoriteObjectListAsync(PagedSortedAndFilterInput input)
        {
            ApplicationUser currentUser = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            var id=long.Parse(input.FilterText);
            var favoriteFolder = await _favoriteFolderRepository.GetAll().Include(s=>s.ApplicationUser).FirstOrDefaultAsync(s => s.Id == id);
            if(favoriteFolder==null||favoriteFolder.ApplicationUser==null)
            {
                return null;
            }
            ApplicationUser user = favoriteFolder.ApplicationUser;


            //判断是否为管理员
            bool isAdmin = false;
            if(currentUser!=null)
            {
                isAdmin=  await _userManager.IsInRoleAsync(currentUser, "Admin");
            }
          
            //判断是否有权限访问
            if (isAdmin == false)
            {
                if (currentUser == null)
                {
                    if (user.IsShowFavotites == false)
                    {
                        return new PagedResultDto<FavoriteObjectAloneViewModel>() { Data = new List<FavoriteObjectAloneViewModel>() };
                    }
                }
                else
                {
                    if(currentUser.Id!=user.Id&& user.IsShowFavotites == false)
                    {
                        return new PagedResultDto<FavoriteObjectAloneViewModel>() { Data = new List<FavoriteObjectAloneViewModel>() };
                    }
                }
            }

            return await _favoriteObjectService.GetPaginatedResult(input,id);
        }

        [HttpGet("{id}/{type}")]
        public async Task<ActionResult<IsObjectInUserFavoriteFolderResult>> IsObjectInUserFavoriteFolderAsync(long id,FavoriteObjectType type)
        {

            //获取当前用户ID
            ApplicationUser user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //获取所有收藏夹ID
            var favoriteFolderIds = await _favoriteFolderRepository.GetAll().Where(s => s.ApplicationUserId == user.Id).Select(s => s.Id).ToListAsync();
            //查找是否存在
            if(type==FavoriteObjectType.Article)
            {
                return new IsObjectInUserFavoriteFolderResult
                {
                    Result = await _favoriteObjectRepository.GetAll().AnyAsync(s => favoriteFolderIds.Contains(s.FavoriteFolderId) && s.ArticleId == id)
                };
            }

            return NotFound();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EditFavoriteFolderViewModel>> EditFavoriteFolderAsync(long id)
        {
            //获取当前用户ID
            ApplicationUser user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //判断是否为管理员
            bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            //获取收藏夹
            var folder = await _favoriteFolderRepository.GetAll().Include(s=>s.ApplicationUser).FirstOrDefaultAsync(s => s.Id == id);
            if(folder==null)
            {
                return NotFound();
            }
            //判断是否有权限编辑
            if (folder.ApplicationUserId != user.Id && isAdmin == false)
            {
                return NotFound();
            }

            EditFavoriteFolderViewModel model = new();
            model.Name = folder.Name;
            model.Id = folder.Id;
            model.BriefIntroduction = folder.BriefIntroduction;
            model.IsDefault = folder.IsDefault;
            model.MainImage = folder.MainImage;
            model.MainImagePath = _appHelper.GetImagePath(folder.MainImage, "app.png");
          
            return model;
        }

        [HttpPost]
        public async Task<ActionResult<Result>> EditFavoriteFolderAsync(EditFavoriteFolderViewModel model)
        {
            //获取当前用户ID
            ApplicationUser user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //判断是否为管理员
            bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            //获取收藏夹
            var folder = await _favoriteFolderRepository.GetAll().Include(s => s.ApplicationUser).FirstOrDefaultAsync(s => s.Id == model.Id);
            if (folder == null)
            {
                return NotFound();
            }
            //判断是否有权限编辑
            if (folder.ApplicationUserId != user.Id && isAdmin == false)
            {
                return NotFound();
            }

            folder.Name = model.Name;
            folder.Id = model.Id;
            folder.BriefIntroduction = model.BriefIntroduction;
            folder.IsDefault = model.IsDefault;
            folder.MainImage = model.MainImage;

            await _favoriteFolderRepository.UpdateAsync(folder);

            return new Result { Successful = true };
        }

        [HttpPost]
        public async Task<ActionResult<Result>> MoveFavoriteObjectsAsync(MoveFavoriteObjectsModel model)
        {
            //检查是否有权限移动
            //获取当前用户ID
            ApplicationUser user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //判断是否为管理员
            bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            //获取关联用户Id
            var currentUserId = (await _favoriteFolderRepository.GetAll().FirstOrDefaultAsync(s => s.Id == model.CurrentFolderId))?.ApplicationUserId;
             //判断是否有权限编辑
            if (currentUserId != user.Id && isAdmin == false)
            {
                return NotFound("没有权限编辑");
            }

            //获取收藏夹
            var folders = await _favoriteFolderRepository.GetAll().Include(s => s.ApplicationUser).Where(s => s.ApplicationUserId == currentUserId).ToListAsync();
            if (folders == null||folders.Count==0)
            {
                return NotFound("未找到收藏夹");
            }
            var folderIds = folders.Select(s => s.Id);

            //清除选定目标的关联收藏夹
            var articles = model.ObjectIds.Where(s => s.Key == FavoriteObjectType.Article).Select(s => s.Value);
            await _favoriteObjectRepository.DeleteRangeAsync(s => folderIds.Contains(s.FavoriteFolderId) && s.Type == FavoriteObjectType.Article && articles.Contains((long)s.ArticleId));

            //添加到新收藏夹
            foreach (var item in model.FolderIds)
            {
                foreach(var infor in model.ObjectIds)
                {
                    await _favoriteObjectRepository.InsertAsync(new FavoriteObject
                    {
                        Type = infor.Key,
                        ArticleId = infor.Key == FavoriteObjectType.Article ? infor.Value : null,
                        CreateTime = DateTime.Now.ToCstTime(),
                        FavoriteFolderId = item
                    });
                }
            }

            //更新收藏夹数
            await _appHelper.UpdateFavoritesCountAsync(folderIds.ToArray());

            return new Result { Successful = true };
        }

    }
}
