﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDAllianceWebSite.Shared.ViewModel.Accounts
{
    public class ChangeUserPasswordModel
    {
        public string UserName { get; set; }
         
        [Display(Name = "身份验证密匙")]
        public string LoginKey { get; set; }

        [Required(ErrorMessage = "请输入密码")]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string Password { get; set; }

        [Required(ErrorMessage = "请输入密码")]
        [Display(Name = "确认密码")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "两次输入的密码不一致，请重新输入")]
        public string ConfirmPassword { get; set; }
    }
}
