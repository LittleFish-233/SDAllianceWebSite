using SDAllianceWebSite.Shared.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDAllianceWebSite.Shared.ViewModel.Articles
{
    public class RevokeExamineModel
    {
        public long Id { get; set; }

        public Operation ExamineType { get; set; }
    }
}
