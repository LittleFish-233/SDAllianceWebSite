using SDAllianceWebSite.Shared.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDAllianceWebSite.Shared.ViewModel.Coments
{
    public class PublishCommentModel
    {
        public CommentType Type { get; set; }

        public string ObjectId { get; set; }

        public string Text { get; set; }
    }
}
