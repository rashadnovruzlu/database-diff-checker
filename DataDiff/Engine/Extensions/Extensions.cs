using DataDiff.Engine.Enums;
using DataDiff.Engine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataDiff.Engine.Extensions
{
    public static class Extensions
    {
        public static ColumnDataTypes GetColumnType(this object obj)
        {
            if (obj is null)
                return ColumnDataTypes.None;

            string columnType = obj.GetType().ToString();

            if (columnType.Contains("int", StringComparison.OrdinalIgnoreCase) ||
                columnType.Contains("date", StringComparison.OrdinalIgnoreCase) ||
                columnType.Contains("bool", StringComparison.OrdinalIgnoreCase))
            {
                return ColumnDataTypes.Number;
            }
            else
            {
                return ColumnDataTypes.Character;
            }
        }

        public static IEnumerable<T> SkipFirst<T>(this IEnumerable<T> source)
        {
            bool skipped = false;
            foreach (var item in source)
            {
                if (skipped)
                    yield return item;
                else
                    skipped = true;
            }
        }


    }

}
