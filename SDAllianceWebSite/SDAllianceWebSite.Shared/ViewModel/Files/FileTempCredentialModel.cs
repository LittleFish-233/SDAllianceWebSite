using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDAllianceWebSite.Shared.ViewModel.Files
{
    public class FileTempCredentialModel
    {
        public string tmpSecretId { get; set; }
        public string tmpSecretKey { get; set; }
        public string sessionToken { get; set; }
        public string startTime { get; set; }
        public string expiredTime { get; set; }
    }
}
