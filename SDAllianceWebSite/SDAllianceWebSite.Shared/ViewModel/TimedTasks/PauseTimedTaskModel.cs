using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDAllianceWebSite.Shared.ViewModel.TimedTasks
{
    public class PauseTimedTaskModel
    {
        public int[] Ids { get; set; }

        public bool IsPause { get; set; }
    }
}
