
using SDAllianceWebSite.Shared.Application.Search.Dtos;
using SDAllianceWebSite.Shared.Model;
using SDAllianceWebSite.Shared.ViewModel;
using SDAllianceWebSite.Shared.ViewModel.Articles;
using SDAllianceWebSite.Shared.ViewModel.Home;
using SDAllianceWebSite.APIServer.Application.Helper;
using SDAllianceWebSite.APIServer.Application.Search;
using SDAllianceWebSite.APIServer.DataReositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using SDAllianceWebSite.Shared.Helper;
using System.Runtime.InteropServices;
using System.Net.Http;

namespace SDAllianceWebSite.APIServer.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/home/[action]")]
    public class HomeAPIController : ControllerBase
    {
        private readonly ISearchService _searchService;
        private readonly IRepository<Article, long> _articleRepository;
        private readonly IRepository<Carousel, int> _carouselRepository;
        private readonly IRepository<FriendLink, int> _friendLinkRepository;
        private readonly IAppHelper _appHelper;

        public HomeAPIController( IRepository<Carousel, int> carouselRepository, ISearchService searchService, IRepository<FriendLink, int> friendLinkRepository,IRepository<Article, long> articleRepository, IAppHelper appHelper)
        {
            _searchService = searchService;
            _articleRepository = articleRepository;
            _appHelper = appHelper;
            _carouselRepository = carouselRepository;
            _friendLinkRepository = friendLinkRepository;
        }
    

        /// <summary>
        /// 获取友情链接 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<List<FriendLink>>> GetHomeFriendLinksViewAsync()
        {
            List<FriendLink> model = new List<FriendLink>();

            //获取友情置顶词条 根据优先级排序
            var entry_result4 = await _friendLinkRepository.GetAll().AsNoTracking().OrderBy("Priority desc")
                .Where(s =>  s.Name != null && s.Name != "" ).Take(12).ToListAsync();
            if (entry_result4 != null)
            {
                foreach (var item in entry_result4)
                {
                    model.Add(new FriendLink
                    {
                        Id = item.Id,
                        Image = _appHelper.GetImagePath(item.Image, "app.png"),
                        Link = item.Link,
                        Name = item.Name
                        //DisPlayValue = _appHelper.GetStringAbbreviation(item.BriefIntroduction, 20)
                    });
                }

            }
            return model;
        }
        /// <summary>
        /// 获取通知
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<List<ArticleHomeAloneViewModel>>> GetHomeNoticesViewAsync()
        {
            List<ArticleHomeAloneViewModel> model = new List<ArticleHomeAloneViewModel>();

            //获取公告
            List<Article> articles = await _articleRepository.GetAll().Where(s => s.IsHidden != true).AsNoTracking().OrderByDescending(s=>s.Priority).ThenByDescending(s=>s.PubishTime)
                .Where(s => s.Type == ArticleType.Notice && s.IsHidden != true).Take(12).ToListAsync();
            foreach (var item in articles)
            {
                model.Add(new ArticleHomeAloneViewModel
                {
                    Id = item.Id,
                    Image = _appHelper.GetImagePath(item.MainPicture, "certificate.png"),
                    DisPlayName = item.DisplayName ?? item.Name,
                    CommentCount = item.CommentCount,
                    ReadCount = item.ReaderCount,
                    //DisPlayValue = _appHelper.GetStringAbbreviation(item.BriefIntroduction, 20)
                });
            }
            return model;

        }
        /// <summary>
        /// 获取最近发布的文章
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<List<ArticleHomeAloneViewModel>>> GetHomeArticlesViewAsync()
        {
            List<ArticleHomeAloneViewModel> model = new List<ArticleHomeAloneViewModel>();

            //获取近期发布的文章
            var article_result2 = await _articleRepository.GetAll().Where(s=>s.IsHidden!=true&&s.Type!=ArticleType.Notice).AsNoTracking().OrderByDescending(s=>s.PubishTime).ThenByDescending(s=>s.Id)
                .Where(s => s.Name != null && s.Name != "").Take(12).ToListAsync();
            if (article_result2 != null)
            {
                foreach (var item in article_result2)
                {
                    model.Add(new ArticleHomeAloneViewModel
                    {
                        Id = item.Id,
                        Image = _appHelper.GetImagePath(item.MainPicture, "certificate.png"),
                        DisPlayName = item.DisplayName ?? item.Name,
                        CommentCount = item.CommentCount,
                        ReadCount = item.ReaderCount,
                        //DisPlayValue = _appHelper.GetStringAbbreviation(item.BriefIntroduction, 20)
                    });
                }
            }
            return model;

        }
        /// <summary>
        /// 获取轮播图
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<List<Carousel>>> GetHomeCarouselsViewAsync()
        {
            List<Carousel> model= await _carouselRepository.GetAll().AsNoTracking().OrderByDescending(s=>s.Priority).ToListAsync();
            foreach (var item in model)
            {
                item.Image = _appHelper.GetImagePath(item.Image, "");
            }

            return model;
        }
        /// <summary>
        /// 搜索
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<SearchViewModel>> SearchAsync(GetSearchInput input)
        {
            SearchViewModel model = new SearchViewModel();
            var dtos = await _searchService.GetPaginatedResult(input);
            dtos.Data = dtos.Data.ToList();

            model.pagedResultDto = dtos;

            return model;
        }

        /// <summary>
        /// 获取搜索提示
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<string>>> GetSearchTipListAsync()
        {
            return await _articleRepository.GetAll().Where(s=>s.IsHidden!=true).AsNoTracking().Select(s => s.Name).Where(s => string.IsNullOrWhiteSpace(s)==false).ToArrayAsync();
        }
    }
}
