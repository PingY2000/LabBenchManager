// Utils/DateUtils.cs

namespace LabBenchManager.Utils
{
    public static class DateUtils
    {
        /// <summary>
        /// 将日期列表格式化为包含连续日期范围的紧凑字符串。
        /// 例如：[1, 2, 3, 5] -> "01-01~01-03, 01-05"
        /// </summary>
        /// <param name="dates">已排序的日期列表</param>
        /// <returns>格式化后的字符串</returns>
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
                while (j + 1 < sortedDates.Count && sortedDates[j + 1] == sortedDates[j].AddDays(1))
                {
                    j++;
                }

                var rangeEnd = sortedDates[j];

                if (rangeStart == rangeEnd)
                {
                    // 单个日期
                    ranges.Add(rangeStart.ToString("MM-dd"));
                }
                else
                {
                    // 连续日期范围
                    ranges.Add($"{rangeStart:MM-dd}~{rangeEnd:MM-dd}");
                }

                i = j + 1; // 从下一个非连续日期开始新的查找
            }

            return string.Join(", ", ranges);
        }
    }
}