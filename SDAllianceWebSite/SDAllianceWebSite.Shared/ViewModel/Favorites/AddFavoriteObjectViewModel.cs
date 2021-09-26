using SDAllianceWebSite.Shared.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDAllianceWebSite.Shared.ViewModel.Favorites
{
    public class AddFavoriteObjectViewModel
    {
        public long[] FavoriteFolderIds { get; set;}

        public FavoriteObjectType Type { get; set; }

        public long ObjectId { get; set; }
    }
}
