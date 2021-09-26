﻿using BootstrapBlazor.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDAllianceWebSite.Shared.ViewModel.Admin
{
    public class ListDisambigsInforViewModel
    {
        public long Hiddens { get; set; }
        public long All { get; set; }
    }

    public class ListDisambigsViewModel
    {
        public List<ListDisambigAloneModel> Disambigs { get; set; }
    }
    public class ListDisambigAloneModel
    {
        [Display(Name = "Id")]
        public int Id { get; set; }
        [Display(Name = "名称")]
        public string Name { get; set; }
        [Display(Name = "简介")]
        public string BriefIntroduction { get; set; }

        [Display(Name = "是否隐藏")]
        public bool IsHidden { get; set; }
    }

    public class DisambigsPagesInfor
    {
        public QueryPageOptions Options { get; set; }
        public ListDisambigAloneModel SearchModel { get; set; }

        public string ObjectId { get; set; }
    }
}
