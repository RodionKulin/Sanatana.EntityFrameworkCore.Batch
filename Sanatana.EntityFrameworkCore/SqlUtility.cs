using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.EntityFrameworkCore
{
    public static class SqlUtility
    {
        public static void SqlRowNumber(int page, int pageSize, out int numberStart, out int numberEnd)
        {
            if (pageSize < 1)
            {
                throw new Exception("Number of items per page must be greater then 0.");
            }

            if (page < 1)
            {
                page = 1;
            }
            numberStart = (page - 1) * pageSize + 1;
            numberEnd = numberStart + pageSize - 1;
        }

        public static int ToSkipNumber(int page, int pageSize)
        {
            if (pageSize < 1)
            {
                throw new Exception("Number of items per page must be greater then 0.");
            }

            if (page < 1)
            {
                page = 1;
            }

            int skip = (page - 1) * pageSize;
            return skip;
        }

        public static DateTime ToSmallDateTime(DateTime datetime)
        {
            DateTime smallDateTimeMin = new DateTime(1900, 1, 1, 12, 0, 0);
            DateTime smallDateTimeMax = new DateTime(2079, 6, 6, 11, 59, 59);

            if (datetime < smallDateTimeMin)
            {
                datetime = smallDateTimeMin;
            }
            else if (datetime > smallDateTimeMax)
            {
                datetime = smallDateTimeMax;
            }
            return datetime;
        }
        
        public static TimeSpan ToSqlTime(TimeSpan time)
        {
            TimeSpan max = new TimeSpan(0, 23, 59, 59, 999).Add(TimeSpan.FromTicks(9999));

            if (time < TimeSpan.FromSeconds(0))
            {
                time = TimeSpan.FromSeconds(0);
            }
            else if (time > max)
            {
                time = max;
            }
            return time;
        }
    }
}
