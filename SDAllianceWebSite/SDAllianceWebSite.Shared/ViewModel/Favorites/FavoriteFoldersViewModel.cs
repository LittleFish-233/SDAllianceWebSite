using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDAllianceWebSite.Shared.ViewModel.Favorites
{
    public class FavoriteFoldersViewModel
    {
        public List<FavoriteFolderAloneModel> Favorites { get; set; }
    }
    public class FavoriteFolderAloneModel
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public bool IsDefault { get; set; }

        public string MainImage { get; set; }

        public string BriefIntroduction { get; set; }

        public DateTime CreateTime { get; set; }

        public long Count { get; set; }
    }

}
