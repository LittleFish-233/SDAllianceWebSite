using SDAllianceWebSite.Shared.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDAllianceWebSite.Shared.ViewModel.Accounts
{
    public class AddThirdPartyLoginInforModel
    {
        public string LoginKey { get; set; }

        public string ThirdPartyKey { get; set; }

        public ThirdPartyLoginType Type { get; set; }
    }
}
