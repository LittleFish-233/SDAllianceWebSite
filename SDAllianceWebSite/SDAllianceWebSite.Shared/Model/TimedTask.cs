using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDAllianceWebSite.Shared.Model
{
    public class TimedTask
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public TimedTaskType Type { get; set; }

        public TimedTaskExecuteType ExecuteType { get; set; }

        /// <summary>
        /// 间隔时间 单位 分钟
        /// </summary>
        public long IntervalTime { get; set; }
        /// <summary>
        /// 另一种定时方式 每天的固定时间
        /// </summary>
        public DateTime? EveryTime { get; set; }

        public DateTime? LastExecutedTime { get; set; }

        public string Parameter { get; set; }
        /// <summary>
        /// 是否暂停执行
        /// </summary>
        public bool IsPause { get; set; }
        /// <summary>
        /// 是否正在执行
        /// </summary>
        public bool IsRuning { get; set; }
        /// <summary>
        /// 上次是否失败
        /// </summary>
        public bool IsLastFail { get; set; }

    }

    public enum TimedTaskType
    {
        [Display(Name = "备份文章")]
        BackupArticle,
        [Display(Name = "更新网站地图")]
        UpdateSitemap
    }
    public enum TimedTaskExecuteType
    {
        [Display(Name = "间隔固定时间运行")]
        IntervalTime,
        [Display(Name = "每天运行")]
        EveryDay
    }
}
