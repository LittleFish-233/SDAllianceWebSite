﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDAllianceWebSite.Shared.ViewModel.Articles
{
    public class EditArticleCanCommentModel
    {
        public long[] Ids { get; set; }

        public bool CanComment { get; set; }
    }
}
