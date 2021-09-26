﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDAllianceWebSite.Shared.ViewModel.Accounts
{
    public class ChangePhoneNumberBeforeModel
    {
        [Display(Name = "身份验证密匙")]
        public string LoginKey { get; set; }
        [Phone(ErrorMessage = "请输入有效的手机号码")]
        [MaxLength(11, ErrorMessage = "手机号码的长度为11位")]
        [MinLength(11, ErrorMessage = "手机号码的长度为11位")]
        [Display(Name = "手机号码")]
        public string NewPhone { get; set; }
    }
}
