using SDAllianceWebSite.Shared.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SDAllianceWebSite.Shared.Model
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(20000000)]
        public string MainPageContext { get; set; } = "### 哇，这里什么都没有呢";

        public string PersonalSignature { get; set; } = "哇，这里什么都没有呢";

        public DateTime? Birthday { get; set; }

        public DateTime RegistTime { get; set; }

        /// <summary>
        /// 学院
        /// </summary>
        public string Institute { get; set; }
        /// <summary>
        /// 班级
        /// </summary>
        public string StudentClass { get; set; }
        /// <summary>
        /// 学号
        /// </summary>
        public string StudentId { get; set; }
        /// <summary>
        /// 真实姓名
        /// </summary>
        public string StudentName { get; set; }

        public string QQ { get; set; }
        public string WeChat { get; set; }
        public string PublicEmail { get; set; }

        /// <summary>
        /// 用户头像
        /// </summary>
        public string PhotoPath { get; set; }
        /// <summary>
        /// 用户空间头图
        /// </summary>
        public string BackgroundImage { get; set; }

        /// <summary>
        /// 用户背景图 大屏幕
        /// </summary>
        public string MBgImage { get; set; }

        /// <summary>
        /// 用户背景图 小屏幕
        /// </summary>
        public string SBgImage { get; set; }


        public string SteamId { get; set; }
        /// <summary>
        /// 附加积分
        /// </summary>
        public int Integral { get; set; }
        /// <summary>
        /// 附加学习值
        /// </summary>
        public int LearningValue { get; set; } 
        /// <summary>
        /// 显示积分 = 附加 + 计算
        /// </summary>
        public int DisplayIntegral { get; set; } 
        /// <summary>
        /// 显示学习值 = 附加 + 计算
        /// </summary>
        public int DisplayLearningValue  { get; set; } 

        /// <summary>
        /// 在线时间 单位 秒
        /// </summary>
        public long OnlineTime { get; set; }
        /// <summary>
        /// 最后在线时间
        /// </summary>
        public DateTime LastOnlineTime { get; set; } 
        /// <summary>
        /// 解封时间
        /// </summary>
        public DateTime? UnsealTime { get; set; }
        /// <summary>
        /// 是否可以留言
        /// </summary>
        public bool CanComment { get; set; } = true;
        /// <summary>
        /// 是否公开收藏夹
        /// </summary>
        public bool IsShowFavotites { get; set; }

        /// <summary>
        /// 是否通过身份验证
        /// </summary>
        public bool IsPassedVerification { get; set; }
        /// <summary>
        /// 管理用户创建的文件
        /// </summary>
        public FileManager FileManager { get; set; }

        /// <summary>
        /// 管理用户空间的留言
        /// </summary>
        public UserSpaceCommentManager UserSpaceCommentManager { get; set; }

        public ICollection<SignInDay> SignInDays { get; set; }

        public ICollection<Examine> Examines { get; set; }

        public ICollection<Message> Messages { get; set; }

        public ICollection<UserOnlineInfor> UserOnlineInfors { get; set; }

        public ICollection<FavoriteFolder> FavoriteFolders { get; set; }

        public ICollection<ThirdPartyLoginInfor> ThirdPartyLoginInfors { get; set; }


    }
    public class SignInDay
    {
        public long Id { get; set; }

        public DateTime Time { get; set; }

        public string ApplicationUserId { get; set; }

        public ApplicationUser ApplicationUser { get; set; }

    }
}
