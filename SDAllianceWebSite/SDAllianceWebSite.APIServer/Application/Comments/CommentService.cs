﻿using SDAllianceWebSite.APIServer.Application.Comments.Dtos;
using SDAllianceWebSite.APIServer.DataReositories;
using SDAllianceWebSite.Shared.Application.Dtos;
using SDAllianceWebSite.Shared.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using SDAllianceWebSite.Shared.ViewModel.Coments;
using SDAllianceWebSite.APIServer.Application.Helper;
using BootstrapBlazor.Components;
using SDAllianceWebSite.Shared.ViewModel.Admin;
using System.Collections.Concurrent;
using Markdig;

namespace SDAllianceWebSite.APIServer.Application.Comments
{
    public class CommentService : ICommentService
    {
        private readonly IRepository<Comment, long> _commentRepository;
        private readonly IRepository<UserSpaceCommentManager, long> _userSpaceCommentManagerRepository;
        private static readonly ConcurrentDictionary<Type, Func<IEnumerable<Comment>, string, SortOrder, IEnumerable<Comment>>> SortLambdaCache = new();

        private readonly IAppHelper _appHelper;

        public CommentService(IAppHelper appHelper,IRepository<Comment, long> commentRepository, IRepository<UserSpaceCommentManager, long> userSpaceCommentManagerRepository)
        {
            _commentRepository = commentRepository;
            _userSpaceCommentManagerRepository = userSpaceCommentManagerRepository;
            _appHelper = appHelper;
        }
        public async Task<PagedResultDto<CommentViewModel>> GetPaginatedResult(GetCommentInput input, CommentType type, string Id,string rankName,string ascriptionUserId)
        {
            //筛选出符合类型的所有评论

            IQueryable<Comment> query = _commentRepository.GetAll().AsNoTracking().Where(s => s.ParentCodeNavigation == null);
            long tempId = 0;
            if (type != CommentType.CommentUser)
            {
                tempId = long.Parse(Id);
            }

            switch (type)
            {
                case CommentType.CommentArticle:
                    query = query.Where(s => s.Type == type && s.ArticleId == tempId);
                    break;
                case CommentType.CommentUser:
                    //
                    var userSpace = await _userSpaceCommentManagerRepository.FirstOrDefaultAsync(s => s.ApplicationUserId == Id);
                    if (userSpace != null)
                    {
                        query = query.Where(s => s.Type == type && s.UserSpaceCommentManagerId == userSpace.Id);
                    }
                    else
                    {
                        return new PagedResultDto<CommentViewModel>() { Data = new List<CommentViewModel>(), TotalCount = 0 };
                    }
                    break;
            }


            //统计查询数据的总条数 不包括子评论
            var count = query.Count();
            //根据需求进行排序，然后进行分页逻辑的计算
            query = query.OrderBy("Priority desc").ThenBy(input.Sorting).Skip((input.CurrentPage - 1) * input.MaxResultCount).Take(input.MaxResultCount);
            //将结果转换为List集合 加载到内存中
            long[] tempQuery = await query.Select(s=>s.Id).ToArrayAsync();

            List<CommentViewModel> models = new List<CommentViewModel>();
            foreach (var item in tempQuery)
            {
                models.Add( await CombinationDataAsync(item, rankName,  ascriptionUserId));
            }

            var dtos = new PagedResultDto<CommentViewModel>
            {
                TotalCount = count,
                CurrentPage = input.CurrentPage,
                MaxResultCount = input.MaxResultCount,
                Data = models,
                FilterText = input.FilterText,
                Sorting = input.Sorting,
                ScreeningConditions = input.ScreeningConditions
            };

            return dtos;
        }

        /// <summary>
        /// 递归复制数据到视图模型
        /// </summary>
        /// <param name="index">评论</param>
        /// <param name="rankName">归属者头衔</param>
        /// <param name="ascriptionUserId">归属者Id</param>
        /// <returns></returns>
        private async Task<CommentViewModel> CombinationDataAsync(long index,string rankName, string ascriptionUserId)
        {
            //获取评论
            Comment comment = await _commentRepository.GetAll().AsNoTracking().Include(s=>s.InverseParentCodeNavigation).Include(s => s.ApplicationUser).FirstOrDefaultAsync(s => s.Id == index);
            CommentViewModel model = new CommentViewModel
            {
                ApplicationUserId = comment.ApplicationUserId,
                ArticleId = comment.ArticleId,
                UserSpaceCommentManagerId = comment.UserSpaceCommentManagerId,
                IsHidden = comment.IsHidden,
                CommentTime = comment.CommentTime,
                Type = comment.Type,
                Id = comment.Id,
                UserName = comment.ApplicationUser.UserName,
                UserImage = _appHelper.GetImagePath(comment.ApplicationUser.PhotoPath, "user.png"),
                InverseParentCodeNavigation = new List<CommentViewModel>()
            };
            //提前将MarkDown语法转为Html
            if (comment.IsHidden == false)
            {
                var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().UseSoftlineBreakAsHardlineBreak().Build();
                model.Text = Markdig.Markdown.ToHtml(comment.Text??"", pipeline);
            }
            else
            {
                model.Text = "该评论被隐藏";
            }


            if (comment.ApplicationUserId==ascriptionUserId)
            {
                model.Rank += rankName;
            }

            //递归初始化子评论
            foreach (var item in comment.InverseParentCodeNavigation)
            {
                model.InverseParentCodeNavigation.Add(await CombinationDataAsync(item.Id,  rankName,  ascriptionUserId));
            }

            return model;

        }

        public async Task<QueryData<ListCommentAloneModel>> GetPaginatedResult(QueryPageOptions options, ListCommentAloneModel searchModel,CommentType type=CommentType.None,string objectId="")
        {
            IEnumerable<Comment> items = _commentRepository.GetAll().AsNoTracking().Include(s=>s.ParentCodeNavigation);
            //判断评论列表类型
            long tempId = 0;
            if(type!=CommentType.None&&type!=CommentType.CommentUser)
            {
                tempId = long.Parse(objectId);
            }
            switch(type)
            {
                case CommentType.CommentArticle:
                    items = items.Where(s => s.ArticleId == tempId);
                    break;
                case CommentType.CommentUser:
                    var space = await _userSpaceCommentManagerRepository.FirstOrDefaultAsync(s => s.ApplicationUserId == objectId);
                    if(space==null)
                    {
                        return new QueryData<ListCommentAloneModel>();
                    }
                    items = items.Where(s => s.UserSpaceCommentManagerId == space.Id);
                    break;
            }
            // 处理高级搜索
            if (!string.IsNullOrWhiteSpace(searchModel.Text))
            {
                items = items.Where(item => item.Text?.Contains(searchModel.Text, StringComparison.OrdinalIgnoreCase) ?? false);
            }

            if (!string.IsNullOrWhiteSpace(searchModel.ApplicationUserId))
            {
                items = items.Where(item => item.ApplicationUserId?.Contains(searchModel.ApplicationUserId, StringComparison.OrdinalIgnoreCase) ?? false);
            }

            if (!string.IsNullOrWhiteSpace(searchModel.ArticleId?.ToString()))
            {
                items = items.Where(item => item.ArticleId.ToString()?.Contains(searchModel.ArticleId.ToString(), StringComparison.OrdinalIgnoreCase) ?? false);
            }
            if (searchModel.Type != null)
            {
                items = items.Where(item => item.Type == searchModel.Type);
            }
            /*   if (!string.IsNullOrWhiteSpace(searchModel.ParentCodeNavigationId?.ToString()))
               {
                   items = items.Where(item => item.ParentCodeNavigationId.ToString()?.Contains(searchModel.ParentCodeNavigationId.ToString(), StringComparison.OrdinalIgnoreCase) ?? false);
               }*/

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
                    items = items.Where(item => (item.Text?.Contains(options.SearchText) ?? false)
                                 || (item.ApplicationUserId?.Contains(options.SearchText) ?? false)
                                 || (item.ArticleId.ToString()?.Contains(options.SearchText) ?? false)
                                /* || (item.ParentCodeNavigationId.ToString()?.Contains(options.SearchText) ?? false)*/);
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
                var invoker = SortLambdaCache.GetOrAdd(typeof(Comment), key => LambdaExtensions.GetSortLambda<Comment>().Compile());
                items = invoker(items, options.SortName, options.SortOrder);
                isSorted = true;
            }

            // 设置记录总数
            var total = items.LongCount();

            // 内存分页
            items = items.Skip((options.PageIndex - 1) * options.PageItems).Take(options.PageItems).ToList();

            //复制数据
            List<ListCommentAloneModel> resultItems = new List<ListCommentAloneModel>();
            foreach (var item in items)
            {
                resultItems.Add(new ListCommentAloneModel
                {
                    Id = item.Id,
                    Type = item.Type,
                    CommentTime = item.CommentTime,
                    Text = item.Text,
                    ApplicationUserId = item.ApplicationUserId,
                    ParentCodeNavigationId = item.ParentCodeNavigation?.Id,
                    ArticleId = item.ArticleId,
                    UserSpaceCommentManagerId = item.UserSpaceCommentManagerId,
                    IsHidden = item.IsHidden,
                    Priority=item.Priority
                });
            }

            return new QueryData<ListCommentAloneModel>()
            {
                Items = resultItems,
                TotalCount = total,
                IsSorted = isSorted,
                // IsFiltered = isFiltered
            };
        }

    }


}
