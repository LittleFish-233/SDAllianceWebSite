﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDAllianceWebSite.Shared.ViewModel.Accounts
{
    public class SecondAuthenticationModel
    {
        [Required(ErrorMessage = "请输入验证码")]
        [Display(Name = "验证码")]
        public string NumToken { get; set; }
    }

    public class UserAuthenticationTypeModel
    {
        public bool IsOnEmail { get; set; }

        public bool IsOnPhone { get; set; }
    }

    public class CheckSecondAuthenticationModel
    {
        public string LoginKey { get; set; }

        public string UserId { get; set; }
    }

    public enum SecondAuthenticationType
    {
        None,
        Phone,
        Email
    }
}
