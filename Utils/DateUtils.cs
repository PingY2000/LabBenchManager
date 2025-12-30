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
        /// 例如：
        /// - 今年：[12/11, 12/12, 12/13, 12/15] -> "12/11–13, 12/15"
        /// - 非今年：[2023/12/11, 2023/12/12] -> "2023/12/11–12"
        /// - 跨年：[2024/12/30, 2024/12/31, 2025/1/1] -> "2024/12/30–2025/01/01"
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
            var currentYear = DateTime.Now.Year; // 🔑 获取当前年份

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
                    // 🔑 单个日期：今年显示 MM/dd，非今年显示 yyyy/MM/dd
                    formattedRange = rangeStart.Year == currentYear
                        ? rangeStart.ToString("MM/dd")
                        : rangeStart.ToString("yyyy/MM/dd");
                }
                else if (rangeStart.Year == rangeEnd.Year && rangeStart.Month == rangeEnd.Month)
                {
                    // 🔑 同年同月
                    if (rangeStart.Year == currentYear)
                    {
                        // 今年：MM/dd–dd
                        formattedRange = $"{rangeStart:MM/dd}–{rangeEnd:dd}";
                    }
                    else
                    {
                        // 非今年：yyyy/MM/dd–dd
                        formattedRange = $"{rangeStart:yyyy/MM/dd}–{rangeEnd:dd}";
                    }
                }
                else if (rangeStart.Year == rangeEnd.Year)
                {
                    // 🔑 同年不同月
                    if (rangeStart.Year == currentYear)
                    {
                        // 今年：MM/dd–MM/dd
                        formattedRange = $"{rangeStart:MM/dd}–{rangeEnd:MM/dd}";
                    }
                    else
                    {
                        // 非今年：yyyy/MM/dd–MM/dd
                        formattedRange = $"{rangeStart:yyyy/MM/dd}–{rangeEnd:MM/dd}";
                    }
                }
                else
                {
                    // 🔑 跨年：yyyy/MM/dd–yyyy/MM/dd（始终显示年份）
                    formattedRange = $"{rangeStart:yyyy/MM/dd}–{rangeEnd:yyyy/MM/dd}";
                }

                ranges.Add(formattedRange);
                i = j + 1;
            }

            return string.Join(", ", ranges);
        }
    }
}