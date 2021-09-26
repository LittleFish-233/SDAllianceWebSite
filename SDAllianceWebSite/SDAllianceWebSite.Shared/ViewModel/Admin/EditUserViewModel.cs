using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SDAllianceWebSite.Shared.ViewModel.Admin
{
    public class EditUserViewModel
    {
        public string Id { get; set; }
        [Required]
        [Display(Name = "用户名")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "请输入电子邮箱")]
        [EmailAddress]
        [Display(Name = "注册的电子邮箱")]
        public string Email { get; set; }

        [Display(Name = "主页")]
        public string MainPageContext { get; set; }

        [Display(Name = "个性签名")]
        public string PersonalSignature { get; set; }
        [Display(Name = "生日")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? Birthday { get; set; }

        public string PhotoName { get; set; }
        public string PhotoPath { get; set; }

        public string BackgroundName { get; set; }
        public string BackgroundPath { get; set; }


        public List<string> Claims { get; set; }

        public IList<UserRolesModel> Roles { get; set; }
        [Display(Name = "基础积分")]
        public int Integral { get; set; }
        [Display(Name = "基础学习值值")]
        public int LearningValue { get; set; }
        [Display(Name = "是否开启空间留言")]
        public bool CanComment { get; set; }
        [Display(Name = "是否公开收藏夹")]
        public bool IsShowFavotites { get; set; }
        [Display(Name = "是否通过身份验证")]
        public bool IsPassedVerification { get; set; }


        [Display(Name = "学院")]
        public string Institute { get; set; }
        [Display(Name = "班级")]
        public string StudentClass { get; set; }
        [Display(Name = "学号")]
        public string StudentId { get; set; }
        [Display(Name = "真实姓名")]
        public string StudentName { get; set; }
        [Display(Name = "QQ")]
        public string QQ { get; set; }
        [Display(Name = "微信")]
        public string WeChat { get; set; }
        [Display(Name = "公开的邮箱")]
        public string PublicEmail { get; set; }


        public EditUserViewModel()
        {
            Claims = new List<string>();
            Roles = new List<UserRolesModel>();
        }
    }
    public class UserRolesModel
    {
        public string Name { get; set; }

        public bool IsSelected { get; set; }
    }
}
