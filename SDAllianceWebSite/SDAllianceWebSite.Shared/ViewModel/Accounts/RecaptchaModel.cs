using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDAllianceWebSite.Shared.ViewModel.Accounts
{
    public class RecaptchaPostModel
    {
        public string secret { get; set; }
        public string response { get; set; }
        public string remoteip { get; set; }
    }
    public class RecaptchaResponseModel
    {
        public bool success { get; set; }
        public string challenge_ts { get; set; }
        public string hostname { get; set; }
    }
}
