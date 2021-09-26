using SDAllianceWebSite.Shared.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDAllianceWebSite.Shared.ViewModel.Favorites
{
    public class MoveFavoriteObjectsModel
    {
        public long CurrentFolderId { get; set; }

        public long[] FolderIds { get; set; }

        public List<KeyValuePair<FavoriteObjectType, long>> ObjectIds { get; set; }
    }
}
