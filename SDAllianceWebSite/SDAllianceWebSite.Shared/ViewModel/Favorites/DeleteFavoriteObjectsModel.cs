using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDAllianceWebSite.Shared.ViewModel.Favorites
{
    public class DeleteFavoriteObjectsModel
    {
        public long FavorieFolderId { get; set; }

        public long[] Ids { get; set; }
    }
}
