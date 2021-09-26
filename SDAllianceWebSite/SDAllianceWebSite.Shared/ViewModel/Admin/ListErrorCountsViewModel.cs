using BootstrapBlazor.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDAllianceWebSite.Shared.ViewModel.Admin
{
    public class ListErrorCountsInforViewModel
    {
        public long All { get; set; }
    }
    public class ListErrorCountsViewModel
    {
        public List<ListErrorCountAloneModel> ErrorCounts { get; set; }
    }
    public class ListErrorCountAloneModel
    {
        [Display(Name = "Id")]
        public long Id { get; set; }
        [Display(Name = "唯一标识")]
        public string Text { get; set; }

        [Display(Name = "时间")]
        public DateTime LastUpdateTime { get; set; }

    
    }

    public class ErrorCountsPagesInfor
    {
        public QueryPageOptions Options { get; set; }
        public ListErrorCountAloneModel SearchModel { get; set; }

        public string Text { get; set; }
    }
}
