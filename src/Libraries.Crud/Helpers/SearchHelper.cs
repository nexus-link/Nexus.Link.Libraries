using System;
using System.Collections.Generic;
using System.Linq;
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
            var filtered = items.Where(item => IsMatch(item, details.WhereAsSortedDictionary));
            var sorted = Sort(filtered, details.OrderByAsSortedDictionary);
            return sorted;
        }

        public static IEnumerable<TModel> Sort<TModel>(IEnumerable<TModel> items, SortedDictionary<string, bool> orderBy)
        {
            var list = items.ToList();
            list.Sort((item1, item2) => Compare(item1, item2, orderBy));
            return list;
        }

        public static int Compare<TModel>(TModel firstItem, TModel secondItem, SortedDictionary<string, bool> orderBy)
        {
            var modelType = typeof(TModel);
            foreach (var keyValuePair in orderBy)
            {
                var propertyInfo = modelType.GetProperty(keyValuePair.Key);
                if (propertyInfo == null) continue;
                var revertFactor = keyValuePair.Value ? 1 : -1;
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

        public static bool IsMatch<TModel>(TModel item, SortedDictionary<string, object> condition)
        {
            if (condition == null || !condition.Any()) return true;
            
            var modelType = typeof(TModel);
            foreach (var keyValuePair in condition)
            {
                var propertyInfo = modelType.GetProperty(keyValuePair.Key);
                if (propertyInfo == null) continue;
                var itemValue = propertyInfo.GetValue(item);
                if (itemValue == null) continue;
                if (itemValue.GetType() != keyValuePair.Value.GetType()) return false;
                if (itemValue.ToString() != keyValuePair.Value.ToString()) return false;
            }
            return true;
        }
    }
}