using SDAllianceWebSite.Shared.Model;
using SDAllianceWebSite.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDAllianceWebSite.Shared.ViewModel.Accounts
{
    public class PostVerificationCodeModel
    {
        public string UserName { get; set; }

        public string LoginKey { get; set; }

        public string Mail { get; set; }

        public SMSType SMSType { get; set; }

        public SendType SendType { get; set; }

        public string Token { get; set; }
        public string Randstr { get; set; }

    }
}
