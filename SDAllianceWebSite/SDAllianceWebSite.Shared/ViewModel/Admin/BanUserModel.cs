using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDAllianceWebSite.Shared.ViewModel.Admin
{
   public  class BanUserModel
    {
        public string[] Ids { get; set; }

        public DateTime? UnsealTime { get; set; }
    }
}
