using System;
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
        /// The object {FirstName = "Joe", ShoeSize = 42} will search for items where both conditions are met.
        /// </example>
        public object Where { get; set; }

        /// <summary>
        /// The properties of this optional object and their bool values will define the sort order for the search result.
        /// {
        ///   SortFirstByThisName = TrueIfAscending,
        ///   SortThenByThisName = TrueIfAscending,
        ///   ...
        /// }
        /// </summary>
        /// A null value means that the order of items will be arbitrary.
        /// <example>
        /// The object {LastName = true, FirstName = false} will sort by LastName in ascending order and then by
        /// FirstName in descending order.
        /// </example>
        public object OrderBy { get; set; }

        /// <inheritdoc />
        public void Validate(string errorLocation, string propertyPath = "")
        {
            if (Where != null)
            {
                var where = JToken.FromObject(Where);
                FulcrumValidate.IsTrue(where.Type == JTokenType.Object, errorLocation,
                    $"Property {propertyPath}.{nameof(Where)} must be an object with properties:\r{where.ToString(Formatting.Indented)}");

                var token = where.First;
                while (token != null)
                {
                    var property = token as JProperty;
                    FulcrumValidate.IsTrue(property != null, errorLocation,
                        $"Property {propertyPath}.{nameof(Where)} must be an object with properties:\r{where.ToString(Formatting.Indented)}");
                    if (property == null) continue;
                    FulcrumValidate.IsTrue(typeof(TModel).GetProperty(property.Name) != null, errorLocation,
                        $"Property {propertyPath}.{nameof(Where)}.{property.Name} can't be found in type {typeof(TModel).FullName}.");

                    FulcrumValidate.IsTrue(property.Value is JValue, errorLocation,
                        $"Property {propertyPath}.{nameof(Where)}.{property.Name} must be a primitive type such as integer, string or boolean.");
                    token = token.Next;
                }
            }

            if (OrderBy != null)
            {
                var orderBy = JToken.FromObject(OrderBy);
                FulcrumValidate.IsTrue(orderBy.Type == JTokenType.Object, errorLocation,
                    $"Property {propertyPath}.{nameof(OrderBy)} must be an object with properties:\r{orderBy.ToString(Formatting.Indented)}");

                var token = orderBy.First;
                while (token != null)
                {
                    var property = token as JProperty;
                    FulcrumValidate.IsTrue(property != null, errorLocation,
                        $"Property {propertyPath}.{nameof(OrderBy)} must be an object with properties:\r{orderBy.ToString(Formatting.Indented)}");

                    var propertyInfo = typeof(TModel).GetProperty(property.Name);
                    FulcrumValidate.IsTrue(propertyInfo != null, errorLocation,
                        $"Property {propertyPath}.{nameof(OrderBy)}.{property.Name} can't be found in type {typeof(TModel).FullName}.");
                    FulcrumValidate.IsTrue(typeof(IComparable).IsAssignableFrom(propertyInfo.PropertyType), errorLocation,
                        $"The type of property {propertyPath}.{nameof(OrderBy)}.{property.Name} must implement {nameof(IComparable)}.");

                    FulcrumValidate.IsTrue(property.Value.Type == JTokenType.Boolean, errorLocation,
                    $"Property {propertyPath}.{nameof(OrderBy)}.{property.Name} must be a boolean.");
                    token = token.Next;
                }
            }
        }
    }
}