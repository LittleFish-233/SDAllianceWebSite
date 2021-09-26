using SDAllianceWebSite.Shared.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDAllianceWebSite.Shared.ViewModel.Accounts
{
    public class ThirdPartyLoginModel
    {
        public string Code { get; set; }

        public string ReturnUrl { get; set; }

        public bool IsSSR { get; set; }

        public ThirdPartyLoginType Type { get; set; }
    }
    public class ThirdPartyLoginResult
    {
        public ThirdPartyLoginResultType Code { get; set; }
        /// <summary>
        /// 错误描述
        /// </summary>
        public string Error { get; set; }
        /// <summary>
        /// 成功后的登入令牌
        /// </summary>
        public string Token { get; set; }
        /// <summary>
        /// 身份验证通过的令牌
        /// </summary>
        public string ThirdLoginKey { get; set; }
    }
    public enum ThirdPartyLoginResultType
    {
        Failed,
        LoginSuccessed,
        NoAssociatedAccount


    }

}
