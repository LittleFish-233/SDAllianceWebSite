using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SDAllianceWebSite.Shared.Model
{
    public class RegisterModel
    {
        [Required(ErrorMessage = "请输入电子邮箱地址")]
        [EmailAddress(ErrorMessage ="电子邮件的格式不正确")]
        [Display(Name = "邮箱地址")]
        //  [VaildEmailDomain(allowedDomain: "qq.com", ErrorMessage = "电子邮件的后缀必须是qq.com")]
        public string Email { get; set; }

        [Required(ErrorMessage = "请输入密码")]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string Password { get; set; }

        [Required(ErrorMessage = "请输入密码")]
        [Display(Name = "确认密码")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "两次输入的密码不一致，请重新输入")]
        public string ConfirmPassword { get; set; }
        [Display(Name = "用户名")]
        [Required(ErrorMessage = "请输入用户名")]
        [MaxLength(20,ErrorMessage ="用户名必须少于20字符")]
        public string Name { get; set; }
        [Display(Name ="学号")]
        [Required(ErrorMessage ="请输入学号")]
        public string StudentId { get; set; }

        public string Token { get; set; }
        public string randstr { get; set; }

    }
    public class RegisterResult
    {
        public bool Successful { get; set; }
        public IEnumerable<string> Errors { get; set; }
    }
}
