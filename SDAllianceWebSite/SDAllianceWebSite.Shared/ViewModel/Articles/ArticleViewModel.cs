﻿using SDAllianceWebSite.Shared.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SDAllianceWebSite.Shared.ViewModel.Articles
{
    public class ArticleViewModel
    {
        public long Id { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 最后编辑人
        /// </summary>
        public string LastEditUserName { get; set; }

        /// <summary>
        /// 创建文章的用户名
        /// </summary>
        public string CreateUserName { get; set; }


        public string UserId { get; set; }

       /// <summary>
       /// 创建文章日期
       /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 最后编辑日期
        /// </summary>
        public DateTime EditDate { get; set; }
        /// <summary>
        /// 主图
        /// </summary>
        public string MainPicture { get; set; }
        /// <summary>
        /// 背景图
        /// </summary>
        public string BackgroundPicture { get; set; }
        /// <summary>
        /// 小背景图
        /// </summary>
        public string SmallBackgroundPicture { get; set; }
        /// <summary>
        /// 是否有权限编辑
        /// </summary>
        public bool Authority { get; set; }
        /// <summary>
        /// 是否处于编辑状态
        /// </summary>
        public bool IsEdit { get; set; }
        /// <summary>
        /// 是否已经点赞
        /// </summary>
        public bool IsThumbsUp { get; set; }
        /// <summary>
        /// 是否处于隐藏状态
        /// </summary>
        public bool IsHidden { get; set; }

        public bool CanComment { get; set; }

        public string OriginalAuthor { get; set; }
        public string OriginalLink { get; set; }
        public DateTime? PubishTime { get; set; }

        public int DisambigId { get; set; }

        public string DisambigName { get; set; }


        public EditState MainState { get; set; }
        public EditState MainPageState { get; set; }
        public EditState RelevancesState { get; set; }


        /// <summary>
        /// 阅读数
        /// </summary>
        public int ReaderCount { get; set; }
        /// <summary>
        /// 点赞数
        /// </summary>
        public int ThumbsUpCount { get; set; }
        /// <summary>
        /// 评论数
        /// </summary>
        public long CommentCount { get; set; }



        /// <summary>
        /// 类型
        /// </summary>
        public ArticleType Type { get; set; }


        /// <summary>
        /// 主页
        /// </summary>
        public string MainPage { get; set; }

        /// <summary>
        /// 相关性列表
        /// </summary>
        public List<RelevancesViewModel> Relevances { get; set; }

    }

    public enum EditState
    {
        [Display(Name = "缺省")]
        None,
        [Display(Name = "正常")]
        Normal,
        [Display(Name = "预览")]
        Preview,
        [Display(Name = "锁定")]
        locked

    }
    public class RelevancesViewModel
    {
        public string Modifier { get; set; }
        public List<RelevancesKeyValueModel> Informations { get; set; }
    }
    public class RelevancesKeyValueModel
    {
        public string Image { get; set; }
        public string DisplayName { get; set; }
        public string DisplayValue { get; set; }
        public string Link { get; set; }

        public int Id { get; set; }
    }
}
