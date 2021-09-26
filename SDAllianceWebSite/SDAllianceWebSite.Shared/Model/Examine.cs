using System;
using System.ComponentModel.DataAnnotations;

namespace SDAllianceWebSite.Shared.Model
{
    public class Examine
    {
        public long Id { get; set; }
        [StringLength(4000000)]
        public string Context { get; set; }

        /// <summary>
        /// 对数据的操作 0无 1用户主页
        /// </summary>
        public Operation Operation { get; set; }

        public bool? IsPassed { get; set; }

        public DateTime? PassedTime { get; set; }

        public DateTime ApplyTime { get; set; }
        /// <summary>
        /// 附加贡献值
        /// </summary>
        public int ContributionValue { get; set; } = 0;
        /// <summary>
        /// 提交审核时的附加说明
        /// </summary>
        public string Note { get; set; }
        /// <summary>
        /// 批注
        /// </summary>
        public string Comments { get; set; }
        /// <summary>
        /// 处理审核请求的管理员名称
        /// </summary>
        public string PassedAdminName { get; set; }

        public string ApplicationUserId { get; set; }

        public long? ArticleId { get; set; }

        public long? CommentId { get; set; }

        /// <summary>
        /// 前置审核Id
        /// </summary>
        public long? PrepositionExamineId { get; set; }

        public ApplicationUser ApplicationUser { get; set; }


        public Article Article { get; set; }

        public Comment Comment { get; set; }

    }
    public enum Operation
    {
        [Display(Name = "缺省")]
        None,
        [Display(Name = "修改个人主页")]
        UserMainPage,
        [Display(Name = "编辑个人信息")]
        EditUserMain,
        [Display(Name = "编辑文章主要信息")]
        EditArticleMain,
        [Display(Name = "编辑文章关联词条")]
        EditArticleRelevanes,
        [Display(Name = "编辑文章内容")]
        EditArticleMainPage,
        [Display(Name = "发表评论")]
        PubulishComment
    }
}
