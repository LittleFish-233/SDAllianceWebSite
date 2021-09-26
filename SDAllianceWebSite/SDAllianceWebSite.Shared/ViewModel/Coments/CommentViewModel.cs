﻿
using SDAllianceWebSite.Shared.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDAllianceWebSite.Shared.ViewModel.Coments
{
    public class CommentViewModel
    {
        public long Id { get; set; }

        public bool IsHidden { get; set; }

        public DateTime CommentTime { get; set; }

        public CommentType Type { get; set; }

        public string Rank { get; set; }

        public string Text { get; set; }

        /// <summary>
        /// 发表评论的用户
        /// </summary>
        public string ApplicationUserId { get; set; }

        public string UserImage { get; set; }

        public string UserName { get; set; }


        public long? ArticleId { get; set; }


        public long? UserSpaceCommentManagerId { get; set; }

        /// <summary>
        /// 子评论
        /// </summary>
        public List<CommentViewModel> InverseParentCodeNavigation { get; set; }


    }
}
