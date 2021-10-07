using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.Libraries.Crud.Model
{
    /// <summary>
    /// What to search for and how to deliver it
    /// </summary>
    public class SearchDetails<TModel> : IValidatable
    {
        private const string WildCardZeroOrMoreCharacters = "<{wild-card-zero-or-more-characters}>";
        private const string WildCardOneCharacter = "<{wild-card-single-character}>";
        private object _orderBy;
        private object _where;
        private List<string> _wherePropertyNames;
        private List<string> _orderByPropertyNames;

        /// <summary>
        /// The properties of this optional object and their value will define the search criteria.
        /// The object should look like:
        /// {
        ///   PropertyName1 = Value1,
        ///   PropertyName2 = Value2,
        ///   ...
        /// }
        /// A null value means return all.
        /// </summary>
        /// <example>
        /// With new {FirstName = "Joe", ShoeSize = 42}, we will search for items where both conditions are met.
        /// </example>
        public object Where
        {
            get => _where;
            set => PrepareWhere(value);
        }

        /// <summary>
        /// The properties of this optional object and their bool values will define the sort order for the search result.
        /// {
        ///   SortFirstByThisName = TrueIfAscending,
        ///   SortThenByThisName = TrueIfAscending,
        ///   ...
        /// }
        /// A null value means that the order of items will be arbitrary.
        /// </summary>
        /// <example>
        /// With new {LastName = true, FirstName = false}, we will sort by LastName in ascending order and then by
        /// FirstName in descending order.
        /// </example>
        public object OrderBy
        {
            get => _orderBy;
            set => PrepareOrderBy(value);
        }

        /// <summary>
        /// THis constructor is intended for serialization. Use the constructor with parameters in other cases.
        /// </summary>
        public SearchDetails()
        {
            PrepareWhere(null);
            PrepareOrderBy(null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="where">
        /// The properties of this optional object and their value will define the search criteria.
        /// The object should look like:
        /// {
        ///   PropertyName1 = Value1,
        ///   PropertyName2 = Value2,
        ///   ...
        /// }
        /// A null value means return all.
        /// </param>
        /// <param name="orderBy">
        /// The properties of this optional object and their bool values will define the sort order for the search result.
        /// {
        ///   SortFirstByThisName = TrueIfAscending,
        ///   SortThenByThisName = TrueIfAscending,
        ///   ...
        /// }
        /// A null value means that the order of items will be arbitrary.
        /// </param>
        /// <example>
        /// With <paramref name="where"/> = new {FirstName = "Joe", ShoeSize = 42}, we will search for items where both conditions are met.
        /// With <paramref name="orderBy"/> = new {LastName = true, FirstName = false}, we will sort by LastName in ascending order and then by
        /// FirstName in descending order.
        /// </example>
        public SearchDetails(object where, object orderBy = null)
        {
            this.PrepareWhere(where);
            _where = where;
            PrepareOrderBy(orderBy);
            _orderBy = orderBy;
        }

        /// <summary>
        /// Convenience property. This is <see cref="WhereAsDictionary"/> as a <see cref="Dictionary{TKey,TValue}"/>.
        /// </summary>
        protected Dictionary<string, WhereCondition> WhereAsDictionary { get; private set; }

        public IEnumerable<string> WherePropertyNames => _wherePropertyNames;

        public WhereCondition GetWhereCondition(string key, string wildCardZeroOrMoreCharacters, string wildCardOneCharacter)
        {
            var whereCondition = WhereAsDictionary[key];
            if (!whereCondition.IsWildCard) return whereCondition;
            if (!(whereCondition.Object is string s)) return whereCondition;
            var replaced = s
                .Replace(WildCardZeroOrMoreCharacters, wildCardZeroOrMoreCharacters)
                .Replace(WildCardOneCharacter, wildCardOneCharacter);
            return new WhereCondition
            {
                IsWildCard = whereCondition.IsWildCard,
                Object = replaced
            };
        }

        public TModel GetWhereAsModel(string wildCardZeroOrMoreCharacters, string wildCardOneCharacter)
        {
            if (_where == null) return default;
            var whereAsJObject = JObject.FromObject(_where);
            foreach (var cancellationToken  in whereAsJObject.Children())
            {
                var property = cancellationToken  as JProperty;
                if (property == null) continue;
                var whereCondition =
                    GetWhereCondition(property.Name, wildCardZeroOrMoreCharacters, wildCardOneCharacter);
                if (!whereCondition.IsWildCard) continue;
                var currentValue = whereCondition.Object.ToString();
                if (currentValue == null) continue;
                var newValue = currentValue
                    .Replace(WildCardZeroOrMoreCharacters, wildCardZeroOrMoreCharacters)
                    .Replace(WildCardOneCharacter, wildCardOneCharacter);
                property.Value = newValue;
            }

            return whereAsJObject.ToObject<TModel>();
        }

        private void PrepareWhere(object value)
        {
            _wherePropertyNames = new List<string>();
            var sd = new Dictionary<string, WhereCondition>();
            if (value != null)
            {
                var modelType = typeof(TModel);
                var valueType = value.GetType();
                var where = JToken.FromObject(value);
                InternalContract.Require(@where.Type == JTokenType.Object,
                    $"Property {nameof(value)} must be an object with properties:\r{@where.ToString(Formatting.Indented)}");

                foreach (var cancellationToken  in where.Children())
                {
                    var property = cancellationToken  as JProperty;
                    InternalContract.Require(property != null,
                        $"Property {nameof(value)} must be an object with properties:\r{@where.ToString(Formatting.Indented)}");
                    if (property == null) continue;
                    var propertyInfo = modelType.GetProperty(property.Name);
                    InternalContract.RequireNotNull(propertyInfo, $"{nameof(value)}.{property.Name}",
                        $"Property {nameof(value)}.{property.Name} can't be found in type {modelType.Name}.");

                    InternalContract.Require(property.Value is JValue,
                        $"Property {nameof(value)}.{property.Name} must be a primitive type such as integer, string or boolean.");


                    var valueProperty = valueType.GetProperty(property.Name);
                    if (valueProperty == null) continue;
                    var v = valueProperty.GetValue(value);
                    string replaced = null;
                    var isWildCard = false;
                    if (v is string s)
                    {
                        (isWildCard, replaced) = ReplaceWildCard(s);
                    }
                    var whereProperty = new WhereCondition
                    {
                        IsWildCard = isWildCard,
                        Object = isWildCard ? replaced : v
                    };
                    _wherePropertyNames.Add(property.Name);
                    sd.Add(property.Name, whereProperty);
                }
            }

            WhereAsDictionary = sd;
            _where = value;
        }

        public static (bool, string) ReplaceWildCard(string value)
        {
            if (value == null) return (false, null);
            if (!value.Contains("*") && !value.Contains("?")) return (false, value);
            const string escapedAsterisk = "{escaped-asterisk}";
            const string escapedQuestionMark = "{escaped-question-mark}";
            const string escapedBackslash = "{escaped-backslash}";
            var filtered = value
                .Replace(@"\\", escapedBackslash)
                .Replace(@"\?", escapedQuestionMark)
                .Replace(@"\*", escapedAsterisk);
            var result = filtered
                .Replace("*", WildCardZeroOrMoreCharacters)
                .Replace("?", WildCardOneCharacter)
                .Replace(escapedBackslash, @"\\")
                .Replace(escapedQuestionMark, @"\?")
                .Replace(escapedAsterisk, @"\*");
            return (filtered.Contains("*") || filtered.Contains("?"), result);
        }

        /// <summary>
        /// Convenience property. This is <see cref="OrderByAsDictionary"/> as a <see cref="Dictionary{TKey,TValue}"/>.
        /// </summary>
        protected Dictionary<string, bool> OrderByAsDictionary { get; private set; }

        public IEnumerable<string> OrderByPropertyNames => _orderByPropertyNames;

        public bool IsAscending(string key) => OrderByAsDictionary[key];

        private void PrepareOrderBy(object value)
        {
            _orderByPropertyNames = new List<string>();
            var sd = new Dictionary<string, bool>();
            if (value != null)
            {
                InternalContract.RequireNotNull(value, nameof(value));
                var orderBy = JToken.FromObject(value);
                InternalContract.Require(orderBy.Type == JTokenType.Object,
                    $"Property {nameof(value)} must be an object with properties:\r{orderBy.ToString(Formatting.Indented)}");

                foreach (var cancellationToken  in orderBy.Children())
                {
                    var property = cancellationToken  as JProperty;
                    InternalContract.Require(property != null,
                        $"Property {nameof(value)} must be an object with properties:\r{orderBy.ToString(Formatting.Indented)}");
                    if (property == null) continue;
                    var propertyInfo = typeof(TModel).GetProperty(property.Name);
                    InternalContract.Require(propertyInfo != null,
                        $"Property {nameof(value)}.{property.Name} can't be found in type {typeof(TModel).FullName}.");
                    if (propertyInfo == null) continue;

                    InternalContract.Require(typeof(IComparable).IsAssignableFrom(propertyInfo.PropertyType),
                        $"The type of property {nameof(value)}.{property.Name} must implement {nameof(IComparable)}.");

                    InternalContract.Require(property.Value.Type == JTokenType.Boolean,
                        $"Property {nameof(value)}.{property.Name} must be a boolean.");

                    _orderByPropertyNames.Add(property.Name);
                    sd.Add(property.Name, (bool)property.Value);
                }
            }

            OrderByAsDictionary = sd;
            _orderBy = value;
        }

        /// <inheritdoc />
        public void Validate(string errorLocation, string propertyPath = "")
        {
            // The validation is done in the setter methods
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var whereAsString = "";
            if (_where != null)
            {
                var list = new List<string>();
                var where = JToken.FromObject(_where);
                var cancellationToken  = where.First;
                while (cancellationToken  != null)
                {
                    var property = cancellationToken  as JProperty;
                    if (property == null) continue;
                    list.Add($"  {property.Name} = {property.Value}");
                    cancellationToken  = cancellationToken .Next;
                }

                whereAsString = "\r" + string.Join(",\r", list) + "\r";
            }

            var orderByAsString = "";
            if (_orderBy != null)
            {
                var list = new List<string>();
                var orderBy = JToken.FromObject(_orderBy);
                var cancellationToken  = orderBy.First;
                while (cancellationToken  != null)
                {
                    var property = cancellationToken  as JProperty;
                    if (property == null) continue;
                    string ascendingOrDescending = (bool)property.Value ? "ascending" : "descending";
                    list.Add($"  {property.Name} = {ascendingOrDescending}");
                    cancellationToken  = cancellationToken .Next;
                }

                orderByAsString = "\r" + string.Join(",\r", list) + "\r";
            }

            var result = $"Where = {{{whereAsString}}}\rOrderBy = {{{orderByAsString}}}";
            return result;
        }
    }

    public class WhereCondition
    {
        public bool IsWildCard { get; set; }
        public object Object { get; set; }

    }
}