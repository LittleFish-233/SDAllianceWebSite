﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDAllianceWebSite.Shared.Model
{
    public class FavoriteFolder
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public bool IsDefault { get; set; }

        public string MainImage { get; set; }

        public string BriefIntroduction { get; set; }

        public DateTime CreateTime { get; set; }

        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        public long Count { get; set; }

        public ICollection<FavoriteObject> FavoriteObjects { get; set; }
    }

    public class FavoriteObject
    {
        public long Id { get; set; }

        public FavoriteObjectType Type { get; set; }

        public DateTime CreateTime { get; set; }

        public long FavoriteFolderId { get; set; }
        public FavoriteFolder FavoriteFolder { get; set; }

        public long? ArticleId { get; set; }
        public Article Article { get; set; }
    }

    public enum FavoriteObjectType
    {
        [Display(Name = "文章")]
        Article
    }
}
