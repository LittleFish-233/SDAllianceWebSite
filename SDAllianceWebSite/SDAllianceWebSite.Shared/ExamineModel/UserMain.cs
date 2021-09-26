using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDAllianceWebSite.Shared.ExamineModel
{
    public class UserMain
    {
        /// <summary>
        /// 用户头像
        /// </summary>
        public string PhotoPath { get; set; }
        /// <summary>
        /// 用户空间头图
        /// </summary>
        public string BackgroundImage { get; set; }
        /// <summary>
        /// 个性签名
        /// </summary>
        public string PersonalSignature { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }
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
    }
}
