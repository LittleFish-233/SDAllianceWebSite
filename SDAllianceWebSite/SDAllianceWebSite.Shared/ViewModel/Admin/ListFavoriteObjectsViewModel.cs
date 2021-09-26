using BootstrapBlazor.Components;
using SDAllianceWebSite.Shared.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDAllianceWebSite.Shared.ViewModel.Admin
{
    public class ListFavoriteObjectsInforViewModel
    {
        public long EntryComments { get; set; }

        public long ArticleComments { get; set; }

        public long SpaceComments { get; set; }

        public long ParentComments { get; set; }

        public long ChildComments { get; set; }

        public long Hiddens { get; set; }
        public long All { get; set; }
    }

    public class ListFavoriteObjectsViewModel
    {
        public List<ListFavoriteObjectAloneModel> FavoriteObjects { get; set; }
    }
    public class ListFavoriteObjectAloneModel
    {
        [Display(Name = "Id")]
        public long Id { get; set; }
        [Display(Name = "类型")]
        public FavoriteObjectType Type { get; set; }
        [Display(Name = "名称")]
        public string Name { get; set; }
        [Display(Name = "目标Id")]
        public long ObjectId { get; set; }
        [Display(Name = "收藏时间")]
        public DateTime CreateTime { get; set; }
    }

    public class FavoriteObjectsPagesInfor
    {
        public QueryPageOptions Options { get; set; }
        public ListFavoriteObjectAloneModel SearchModel { get; set; }

        public long FavoriteFolderId { get; set; }
    }


}
