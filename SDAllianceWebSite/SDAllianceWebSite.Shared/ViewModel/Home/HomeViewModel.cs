using SDAllianceWebSite.Shared.Model;
using SDAllianceWebSite.Shared.ViewModel.Articles;
using System.Collections.Generic;

namespace SDAllianceWebSite.Shared.ViewModel.Home
{
    public class HomeViewModel
    {
        public List<Carousel> Carousels { get; set; }

        public List<FriendLink> FriendLinks { get; set; }
    }
}
