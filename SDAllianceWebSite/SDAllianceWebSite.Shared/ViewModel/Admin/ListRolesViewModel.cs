﻿using BootstrapBlazor.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDAllianceWebSite.Shared.ViewModel.Admin
{
    public class ListRolesInforViewModel
    {
    }

    public class ListRolesViewModel
    {
        public List<ListRoleAloneModel> Roles { get; set; }
    }
    public class ListRoleAloneModel
    {
        [Display(Name = "Id")]
        public string Id { get; set; }
        [Display(Name = "角色名")]
        public string Name { get; set; }
    }

    public class RolesPagesInfor
    {
        public QueryPageOptions Options { get; set; }
        public ListRoleAloneModel SearchModel { get; set; }

        public string Text { get; set; }
    }
}
