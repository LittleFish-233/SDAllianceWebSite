﻿namespace SDAllianceWebSite.Shared.Model
{
    public class FriendLink
    {
        public int Id { get; set; }

        public string Image { get; set; }

        public string Link { get; set; }

        public string Name { get; set; }

        public int Priority { get; set; } = 0;

    }
}
