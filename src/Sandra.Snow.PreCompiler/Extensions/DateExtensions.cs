namespace Sandra.Snow.PreCompiler.Extensions
{
    using System;

    public static class DateExtensions
    {
        public static DateTime AsYearDate(this DateTime date)
        {
            return new DateTime(date.Year, 1, 1);
        }

        public static DateTime AsMonthDate(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1);
        }
    }
}