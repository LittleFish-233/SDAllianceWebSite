using SDAllianceWebSite.Shared.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDAllianceWebSite.Shared.ViewModel.Accounts
{
    public class ThirdPartyLoginTempModel
    {
        public string ThirdLoginKey { get; set; }

        public ThirdPartyLoginType Type { get; set; }
    }
}
