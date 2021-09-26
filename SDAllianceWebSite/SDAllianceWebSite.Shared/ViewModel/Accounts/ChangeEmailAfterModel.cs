﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDAllianceWebSite.Shared.ViewModel.Accounts
{
    public class ChangeEmailAfterModel
    {
        [Required(ErrorMessage = "请输入验证码")]
        [Display(Name = "验证码")]
        public string NumToken { get; set; }

        [Display(Name = "身份验证密匙")]
        public string LoginKey { get; set; }
        [Display(Name = "新邮箱")]
        [EmailAddress(ErrorMessage = "请输入有效的电子邮箱")]
        public string NewEmail { get; set; }

    }
}
