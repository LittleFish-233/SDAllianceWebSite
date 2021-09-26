using SDAllianceWebSite.Shared.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDAllianceWebSite.Shared.ViewModel.Admin
{
    public class ExaminedNormalListModel
    {
        public long Id { get; set; }

        public ExaminedNormalListModelType Type { get; set; }

        public DateTime ApplyTime { get; set; }

        public DateTime? PassedTime { get; set; }

        public string RelatedId { get; set; }

        public string RelatedName { get; set; }

        public string UserId { get; set; }

        public string UserName { get; set; }

        public Operation Operation { get; set; }

        public bool? IsPassed { get; set; }
    }

    public enum ExaminedNormalListModelType
    {
        Article,
        User,
        Comment
    }
}
