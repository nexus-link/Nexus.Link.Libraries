using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;

namespace Nexus.Link.Libraries.Crud.Model
{
    /// <summary>
    /// What to search for and how to deliver it
    /// </summary>
    public class SearchDetails<TModel> : IValidatable
    {
        private object _orderBy;
        private object _where;

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
            set
            {
                WhereAsSortedDictionary = ParseWhere(value);
                _where = value;
            }
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
            set
            {
                OrderByAsSortedDictionary = ParseOrderBy(value);
                _orderBy = value;
            }
        }

        /// <summary>
        /// THis constructor is intended for serialization. Use the constructor with parameters in other cases.
        /// </summary>
        public SearchDetails()
        {
            WhereAsSortedDictionary = new SortedDictionary<string, object>();
            OrderByAsSortedDictionary = new SortedDictionary<string, bool>();
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
            this.WhereAsSortedDictionary = ParseWhere(where);
            _where = where;
            OrderByAsSortedDictionary = ParseOrderBy(orderBy);
            _orderBy = orderBy;
        }

        /// <summary>
        /// Convenience property. This is <see cref="WhereAsSortedDictionary"/> as a <see cref="TModel"/> object.
        /// </summary>
        public TModel WhereAsModel => WhereAsJObject == null ? default : WhereAsJObject.ToObject<TModel>();

        /// <summary>
        /// Convenience property. This is <see cref="WhereAsSortedDictionary"/> as a <see cref="JObject"/>.
        /// </summary>
        public JObject WhereAsJObject => _where == null ? null : JObject.FromObject(_where);

        /// <summary>
        /// Convenience property. This is <see cref="WhereAsSortedDictionary"/> as a <see cref="SortedDictionary{TKey,TValue}"/>.
        /// </summary>
        public SortedDictionary<string, object> WhereAsSortedDictionary { get; private set; }

        private static SortedDictionary<string, object> ParseWhere(object value)
        {
            var sd = new SortedDictionary<string, object>();
            if (value != null)
            {
                var modelType = typeof(TModel);
                var valueType = value.GetType();
                var where = JToken.FromObject(value);
                InternalContract.Require(@where.Type == JTokenType.Object,
                    $"Property {nameof(value)} must be an object with properties:\r{@where.ToString(Formatting.Indented)}");

                var token = @where.First;
                while (token != null)
                {
                    var property = token as JProperty;
                    InternalContract.Require(property != null,
                        $"Property {nameof(value)} must be an object with properties:\r{@where.ToString(Formatting.Indented)}");
                    if (property != null)
                    {
                        var propertyInfo = modelType.GetProperty(property.Name);
                        InternalContract.RequireNotNull(propertyInfo, $"{nameof(value)}.{property.Name}",
                            $"Property {nameof(value)}.{property.Name} can't be found in type {modelType.Name}.");

                        InternalContract.Require(property.Value is JValue,
                            $"Property {nameof(value)}.{property.Name} must be a primitive type such as integer, string or boolean.");


                        var valueProperty = valueType.GetProperty(property.Name);
                        if (valueProperty != null)
                        {
                            sd.Add(property.Name, valueProperty.GetValue(value));
                        }
                    }

                    token = token.Next;
                }
            }

            return sd;
        }

        /// <summary>
        /// Convenience property. This is <see cref="OrderByAsSortedDictionary"/> as a <see cref="SortedDictionary{TKey,TValue}"/>.
        /// </summary>
        public SortedDictionary<string, bool> OrderByAsSortedDictionary { get; private set; }

        private static SortedDictionary<string, bool> ParseOrderBy(object value)
        {
            var sd = new SortedDictionary<string, bool>();
            if (value != null)
            {
                InternalContract.RequireNotNull(value, nameof(value));
                var orderBy = JToken.FromObject(value);
                InternalContract.Require(orderBy.Type == JTokenType.Object,
                    $"Property {nameof(value)} must be an object with properties:\r{orderBy.ToString(Formatting.Indented)}");

                var token = orderBy.First;
                while (token != null)
                {
                    var property = token as JProperty;
                    InternalContract.Require(property != null,
                        $"Property {nameof(value)} must be an object with properties:\r{orderBy.ToString(Formatting.Indented)}");

                    var propertyInfo = typeof(TModel).GetProperty(property.Name);
                    InternalContract.Require(propertyInfo != null,
                        $"Property {nameof(value)}.{property.Name} can't be found in type {typeof(TModel).FullName}.");

                    InternalContract.Require(typeof(IComparable).IsAssignableFrom(propertyInfo.PropertyType),
                        $"The type of property {nameof(value)}.{property.Name} must implement {nameof(IComparable)}.");

                    InternalContract.Require(property.Value.Type == JTokenType.Boolean,
                        $"Property {nameof(value)}.{property.Name} must be a boolean.");

                    sd.Add(property.Name, (bool) property.Value);
                    token = token.Next;
                }
            }

            return sd;
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
                var token = where.First;
                while (token != null)
                {
                    var property = token as JProperty;
                    if (property == null) continue;
                    list.Add($"  {property.Name} = {property.Value}");
                    token = token.Next;
                }

                whereAsString = "\r" + string.Join(",\r", list) + "\r";
            }

            var orderByAsString = "";
            if (_orderBy != null)
            {
                var list = new List<string>();
                var orderBy = JToken.FromObject(_orderBy);
                var token = orderBy.First;
                while (token != null)
                {
                    var property = token as JProperty;
                    if (property == null) continue;
                    string ascendingOrDescending = (bool)property.Value ? "ascending" : "descending";
                    list.Add($"  {property.Name} = {ascendingOrDescending}");
                    token = token.Next;
                }

                orderByAsString = "\r" + string.Join(",\r", list) + "\r";
            }

            var result = $"Where = {{{whereAsString}}}\rOrderBy = {{{orderByAsString}}}";
            return result;
        }
    }
}