using SDAllianceWebSite.Shared.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SDAllianceWebSite.Shared.ViewModel.Articles
{
    public class CreateArticleViewModel
    {
        [Display(Name = "唯一名称")]
        [Required(ErrorMessage = "请填写唯一名称")]
        public string Name { get; set; }
        [Display(Name = "显示名称")]
        [Required(ErrorMessage = "请填写显示名称")]
        public string DisplayName { get; set; }
        [Display(Name = "文章简介")]
        [Required(ErrorMessage = "请填写文章简介")]
        public string BriefIntroduction { get; set; }
        //[Required(ErrorMessage = "请上传主图")]
        [Display(Name = "主图")]
        public string MainPicture { get; set; }

        [Display(Name = "背景图")]
        public string BackgroundPicture { get; set; }

        [Display(Name = "小背景图")]
        public string SmallBackgroundPicture { get; set; }


        public string MainPicturePath { get; set; }
        public string BackgroundPicturePath { get; set; }
        public string SmallBackgroundPicturePath { get; set; }

        [Display(Name = "类别")]
        [Required(ErrorMessage = "请选择类别")]
        public ArticleType Type { get; set; }

        [Display(Name = "正文")]
        [Required(ErrorMessage = "请输入正文")]
        public string Context { get; set; }

        [Display(Name = "原作者")]
        public string OriginalAuthor { get; set; }
        [Display(Name = "原文链接")]
        public string OriginalLink { get; set; }
        [Display(Name = "发布日期")]
        public DateTime PubishTime { get; set; }
        [Display(Name = "动态发生时间")]
        public DateTime? RealNewsTime { get; set; }

        public List<RelevancesModel> articles { get; set; }
        public List<RelevancesModel> others { get; set; }

        [Display(Name = "备注")]
        public string Note { get; set; }
    }
    public class RelevancesModel
    {
        [Display(Name = "唯一名称")]
        //[Required(ErrorMessage ="请输入显示名称")]
        public string DisplayName { get; set; }

        [Display(Name = "简介")]
        // [Required(ErrorMessage = "请输入显示值")]
        public string DisPlayValue { get; set; }

        [Display(Name = "链接")]
        // [Required(ErrorMessage = "请输入完整链接")]
        public string Link { get; set; }
    }

}
