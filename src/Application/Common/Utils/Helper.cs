using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmAPI.Application.Common.Utils
{
    static class Helper
    {
        /// <summary>
        /// Funcion to get the valid until date for the enrollment.
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static DateTime GetUntilValidTime(IConfiguration config)
        {
            int monthsToAdd = config.GetValue<int>("EnrollmentSettings:ValidMonths");

            DateTime baseDate = DateTime.Today.AddMonths(monthsToAdd);
            DateTime finalDate = new DateTime(baseDate.Year, baseDate.Month, baseDate.Day, 23, 59, 59);

            return finalDate;
        }
    }
}
