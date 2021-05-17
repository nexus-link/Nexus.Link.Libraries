using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
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
            var where = details.Where == null ? null : JObject.FromObject(details.Where);
            var orderBy = details.OrderBy == null ? null : JObject.FromObject(details.OrderBy);

            var filtered = items.Where(item => IsMatch(item, where));
            var sorted = Sort(filtered, orderBy);
            return sorted;
        }

        public static IEnumerable<TModel> Sort<TModel>(IEnumerable<TModel> items, JToken order)
        {
            if (order == null) return items;
            var orderBy = new List<KeyValuePair<string, bool>>();
            var token = order.First;
            while (token != null)
            {
                var property = token as JProperty;
                if (property == null) continue;
                var ascending = (bool)property.Value;
                orderBy.Add(new KeyValuePair<string, bool>(property.Name, ascending));
                token = token.Next;
            }

            var list = items.ToList();
            list.Sort((item1, item2) => Compare(item1, item2, orderBy));
            return list;
        }

        public static int Compare<TModel>(TModel firstItem, TModel secondItem, List<KeyValuePair<string, bool>> orderBy)
        {
            foreach (var sortParameter in orderBy)
            {
                var property = typeof(TModel).GetProperty(sortParameter.Key);
                if (property == null) continue;

                var revertFactor = sortParameter.Value ? 1 : -1;
                var value1 = property.GetValue(firstItem) as IComparable;
                var value2 = property.GetValue(secondItem) as IComparable;

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

        public static bool IsMatch<TModel>(TModel item, JObject condition)
        {
            if (condition == null) return true;
            var itemAsJson = JObject.FromObject(item);
            var conditionToken = condition.First;
            while (conditionToken != null)
            {
                var conditionProperty = conditionToken as JProperty;
                if (conditionProperty == null) continue;
                var itemValue = itemAsJson.GetValue(conditionProperty.Name);
                if (itemValue == null) continue;
                if (itemValue.Type != conditionProperty.Value.Type) return false;
                if (itemValue.ToString() != conditionProperty?.Value.ToString()) return false;
                conditionToken = conditionToken.Next;
            }
            return true;
        }

        public static List<string> OrderByAsList<TModel>(SearchDetails<TModel> details)
        {
            var orderList = new List<string>();
            if (details.OrderBy != null)
            {
                var json = JObject.FromObject(details.OrderBy);
                var token = json.First;
                while (token != null)
                {
                    if (!(token is JProperty conditionProperty)) continue;
                    var ascending = (bool) conditionProperty.Value;

                    var ascOrDesc = @ascending ? "ASC" : "DESC";
                    orderList.Add($"[{conditionProperty.Name}] {ascOrDesc}");
                    token = token.Next;
                }
            }

            return orderList;
        }

        public static List<string> WhereAsList<TModel>(SearchDetails<TModel> details)
        {
            var whereList = new List<string>();
            if (details.Where != null)
            {
                var json = JObject.FromObject(details.Where);
                var token = json.First;
                while (token != null)
                {
                    if (!(token is JProperty conditionProperty)) continue;
                    whereList.Add($"[{conditionProperty.Name}] = @{conditionProperty.Name}");
                    token = token.Next;
                }
            }

            return whereList;
        }
    }
}