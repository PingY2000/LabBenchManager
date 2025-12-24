// Utils/DateUtils.cs

using System;
using System.Collections.Generic;
using System.Linq;

namespace LabBenchManager.Utils
{
    public static class DateUtils
    {
        /// <summary>
        /// 将日期列表格式化为包含连续日期范围的紧凑字符串。
        /// 例如：[12/11, 12/12, 12/13, 12/15] -> "12/11–13, 12/15"
        /// 跨月示例：[12/30, 12/31, 1/1] -> "12/30–01/01"
        /// </summary>
        /// <param name="dates">日期列表（无需预先排序）</param>
        /// <returns>格式化后的紧凑日期字符串</returns>
        public static string FormatDateListWithRanges(List<DateTime> dates)
        {
            if (dates == null || !dates.Any())
            {
                return "(未排期)";
            }

            var sortedDates = dates.OrderBy(d => d).ToList();
            var ranges = new List<string>();

            int i = 0;
            while (i < sortedDates.Count)
            {
                var rangeStart = sortedDates[i];
                int j = i;

                // 查找连续日期的结束点
                while (j + 1 < sortedDates.Count && sortedDates[j + 1].Date == sortedDates[j].Date.AddDays(1))
                {
                    j++;
                }

                var rangeEnd = sortedDates[j];

                string formattedRange;
                if (rangeStart.Date == rangeEnd.Date)
                {
                    // 单个日期：MM/dd
                    formattedRange = rangeStart.ToString("MM/dd");
                }
                else if (rangeStart.Year == rangeEnd.Year && rangeStart.Month == rangeEnd.Month)
                {
                    // 同年同月：MM/dd–dd
                    formattedRange = $"{rangeStart:MM/dd}–{rangeEnd:dd}";
                }
                else
                {
                    // 跨月或跨年：MM/dd–MM/dd
                    formattedRange = $"{rangeStart:MM/dd}–{rangeEnd:MM/dd}";
                }

                ranges.Add(formattedRange);
                i = j + 1;
            }

            return string.Join(", ", ranges);
        }

    }
}