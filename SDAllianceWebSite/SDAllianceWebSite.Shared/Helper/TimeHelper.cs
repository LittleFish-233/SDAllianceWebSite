using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace SDAllianceWebSite.Shared.Helper
{
    public class TimeUtil
    {
        public static DateTime GetCstDateTime()
        {
            Instant now = SystemClock.Instance.GetCurrentInstant();
            var shanghaiZone = DateTimeZoneProviders.Tzdb["Asia/Shanghai"];
            return now.InZone(shanghaiZone).ToDateTimeUnspecified();
        }

    }
    public static class DateTimeExtentions
    {
        public static DateTime ToCstTime(this DateTime time)
        {
            return TimeUtil.GetCstDateTime();
        }
    }
}
