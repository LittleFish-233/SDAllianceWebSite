using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SDAllianceWebSite.Shared.Model
{
    public class ErrorCount
    {
        public long Id { get; set; }

        public string Text { get; set; }

        public int Count { get; set; }

        public DateTime LastUpdateTime { get; set; }
    }
}
