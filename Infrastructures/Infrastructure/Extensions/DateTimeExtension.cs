using System;
using System.Collections.Generic;
using System.Globalization;

namespace Infrastructure.Extensions
{
    public static class DateTimeExtension
    {
        public static IEnumerable<(int Month, int Year)> MonthsBetween(DateTime startDate, DateTime endDate)
        {
            DateTime iterator;
            DateTime limit;

            if (endDate > startDate)
            {
                iterator = new DateTime(startDate.Year, startDate.Month, 1);
                limit = endDate;
            }
            else
            {
                iterator = new DateTime(endDate.Year, endDate.Month, 1);
                limit = startDate;
            }

            var dateTimeFormat = CultureInfo.CurrentCulture.DateTimeFormat;
            while (iterator <= limit)
            {
                yield return (
                    iterator.Month,
                    iterator.Year
                );

                iterator = iterator.AddMonths(1);
            }
        }
    }
}