using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDAllianceWebSite.Shared.ViewModel.Articles
{
    public class ArticleAsyncInforViewModel
    {
        /// <summary>
        /// 是否有权限编辑
        /// </summary>
        public bool Authority { get; set; }

        /// <summary>
        /// 是否已经点赞
        /// </summary>
        public bool IsThumbsUp { get; set; }
    }
}
