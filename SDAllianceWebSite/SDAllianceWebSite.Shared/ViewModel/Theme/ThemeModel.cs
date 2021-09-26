using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDAllianceWebSite.Shared.ViewModel.Theme
{
    public class ThemeModel
    {
        public bool UseTabSet { get; set; } = false;

        public string Theme { get; set; } = "color3";

        public bool IsOpen { get; set; }

        public bool IsFixedHeader { get; set; } = true;

        public bool IsFixedFooter { get; set; } = false;

        public bool IsFullSide { get; set; } = true;

        public bool ShowFooter { get; set; } = true;

        public bool IsDark { get; set; } = false;

        public bool IsDebug { get; set; } = false;

        public bool IsOnMouse { get; set; } = true;

        public bool IsOnBgImage { get; set; } = false;
    }
}
