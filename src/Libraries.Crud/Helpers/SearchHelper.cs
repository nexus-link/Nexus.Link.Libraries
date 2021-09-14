using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Crud.Model;

namespace Nexus.Link.Libraries.Crud.Helpers
{
    public static class SearchHelper
    {
        public static IEnumerable<TModel> FilterAndSort<TModel>(IEnumerable<TModel> items, SearchDetails<TModel> details)
        {
            InternalContract.RequireNotNull(details, nameof(details));
            InternalContract.RequireValidated(details, nameof(details));
            var filtered = items.Where(item => IsMatch(item, details));
            var sorted = Sort(filtered, details);
            return sorted;
        }

        public static IEnumerable<TModel> Sort<TModel>(IEnumerable<TModel> items,  SearchDetails<TModel> details)
        {
            var list = items.ToList();
            list.Sort((item1, item2) => Compare(item1, item2, details));
            return list;
        }

        public static int Compare<TModel>(TModel firstItem, TModel secondItem,  SearchDetails<TModel> details)
        {
            var modelType = typeof(TModel);
            foreach (var propertyName in details.OrderByPropertyNames)
            {
                var propertyInfo = modelType.GetProperty(propertyName);
                if (propertyInfo == null) continue;
                var revertFactor = details.IsAscending(propertyName) ? 1 : -1;
                var value1 = propertyInfo.GetValue(firstItem) as IComparable;
                var value2 = propertyInfo.GetValue(secondItem) as IComparable;

                if (value1 == null)
                {
                    if (value2 == null) continue;
                    return revertFactor;
                }

                if (value2 == null) return -revertFactor;
                var result = value1.CompareTo(value2);
                if (result != 0) return result * revertFactor;
            }

            return 0;
        }

        public static bool IsMatch<TModel>(TModel item, SearchDetails<TModel> details)
        {
            if (!details.WherePropertyNames.Any()) return true;
            
            var modelType = typeof(TModel);
            foreach (var propertyName in details.WherePropertyNames)
            {
                var propertyInfo = modelType.GetProperty(propertyName);
                if (propertyInfo == null) continue;
                var itemValue = propertyInfo.GetValue(item);
                var whereCondition = details.GetWhereCondition(propertyName, ".*", ".");
                if (whereCondition.Object == null) return itemValue == null;
                if (itemValue == null) return false; 
                if (itemValue.GetType() != whereCondition.Object.GetType()) return false;
                if (whereCondition.IsWildCard)
                {
                    var regexp = new Regex((string) whereCondition.Object);
                    if (!regexp.IsMatch(itemValue.ToString())) return false;
                }
                else
                {
                    if (itemValue.ToString() != whereCondition.Object.ToString()) return false;
                }
            }

            return true;
        }
    }
}