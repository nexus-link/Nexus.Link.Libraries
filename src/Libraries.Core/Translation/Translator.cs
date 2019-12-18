using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Crud.Model;
using Nexus.Link.Libraries.Core.Json;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Core.Storage.Model;

namespace Nexus.Link.Libraries.Core.Translation
{
    /// <summary>
    /// A convenience class for translations.
    /// </summary>
    public class Translator : ITranslator
    {
        private readonly string _clientName;
        private readonly ITranslatorService _service;
        private readonly HashSet<string> _conceptValues;
        private IDictionary<string, string> _translations;
        private readonly Regex _conceptValueAndNothingElseInStringRegex;
        private readonly Regex _conceptValueRegex;

        /// <summary>
        /// Decorate a value
        /// </summary>
        public static string Decorate(string conceptName, string clientName, string value) => value == null ? null :
            $"({conceptName}!~{clientName}!{value})";

        /// <summary>
        /// A translator for a specific <paramref name="clientName"/> that will use the <paramref name="service"/> for the actual translations.
        /// </summary>
        public Translator(string clientName, ITranslatorService service)
        {
            InternalContract.RequireNotNullOrWhiteSpace(clientName, nameof(clientName));
            InternalContract.RequireNotNull(service, nameof(service));
            _clientName = clientName;
            _service = service;
            _conceptValues = new HashSet<string>();
            _translations = new Dictionary<string, string>();
            _conceptValueRegex = new Regex(@"\(([^!]+)!([^!]+)!(.+)\)", RegexOptions.Compiled);
            _conceptValueAndNothingElseInStringRegex = new Regex("\"" + @"(\(([^!]+)!([^!]+)!(?:(?!\)" + "\"" + @").)+\))" + "\"", RegexOptions.Compiled);
        }

        /// <inheritdoc/>
        public string Decorate(string conceptName, string value)
        {
            if (value == null) return null;
            return IsDecorated(value) ? value : Decorate(conceptName, _clientName, value);
        }

        /// <inheritdoc/>
        public SlaveToMasterId<string> Decorate(string masterIdConceptName, string slaveIdConceptName, SlaveToMasterId<string> id)
        {
            if (id == null) return null;
            id.MasterId = Decorate(masterIdConceptName, id.MasterId);
            id.SlaveId = Decorate(masterIdConceptName, id.SlaveId);
            return id;
        }

        /// <inheritdoc />
        public T Decorate<T>(T item)
        {
            return (T)Decorate(item, typeof(T));
        }

        /// <inheritdoc />
        public object Decorate(object item, Type type)
        {
            if (item == null) return null;
            DecorateClassOrCollection(item, 0);
            return item;
        }

        /// <inheritdoc />
        [Obsolete("Use the method Decorate<T>(T). Obsolete since 2019-12-13.")]
        public IEnumerable<T> Decorate<T>(IEnumerable<T> items)
        {
            return items?.Select(Decorate);
        }

        /// <inheritdoc />
        [Obsolete("Use the method Decorate(object, Type). Obsolete since 2019-12-13.")]
        public IEnumerable<object> Decorate(IEnumerable<object> items, Type type)
        {
            return items?.Select(i => Decorate(i, type));
        }

        /// <inheritdoc/>
        [Obsolete("Use the method Decorate<T>(T). Obsolete since 2019-11-21.")]
        public TModel DecorateItem<TModel>(TModel item)
        {
            return Decorate(item);
        }

        /// <inheritdoc/>
        [Obsolete("Use the method Decorate<T>(T). Obsolete since 2019-11-21.")]
        public IEnumerable<TModel> DecorateItems<TModel>(IEnumerable<TModel> items)
        {
            if (items == null) return null;
            var array = items as TModel[] ?? items.ToArray();
            foreach (var item in array)
            {
                DecorateItem(item);
            }

            return array;
        }

        /// <inheritdoc/>
        [Obsolete("Use the method Decorate<T>(T). Obsolete since 2019-11-21.")]
        public PageEnvelope<TModel> DecoratePage<TModel>(PageEnvelope<TModel> page)
        {
            if (page == null) return null;
            page.Data = DecorateItems(page.Data);
            return page;
        }

        /// <inheritdoc/>
        public ITranslator Add<T>(T item)
        {
            if (item == null) return this;
            var jsonString = JsonConvert.SerializeObject(item);
            foreach (Match match in _conceptValueAndNothingElseInStringRegex.Matches(jsonString))
            {
                FulcrumAssert.IsGreaterThanOrEqualTo(1, match.Groups.Count, null, "Expected match to have at least one group.");
                var conceptPath = match.Groups[1].ToString();
                _conceptValues.Add(conceptPath);
            }
            // TODO: Find all decorated strings and add them to the translation batch.
            return this;
        }

        /// <inheritdoc/>
        public ITranslator AddSubStrings(string s)
        {
            foreach (Match match in _conceptValueRegex.Matches(s))
            {
                var conceptPath = match.Groups[0].ToString();
                _conceptValues.Add(conceptPath);
            }
            // TODO: Find all decorated strings and add them to the translation batch.
            return this;
        }

        /// <inheritdoc/>
        public async Task ExecuteAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            _translations = await _service.TranslateAsync(_conceptValues, _clientName, cancellationToken);
        }

        /// <inheritdoc/>
        public T Translate<T>(T item)
        {
            return (T)Translate(item, typeof(T));
        }

        /// <inheritdoc />
        public object Translate(object item, Type type)
        {
            if (item == null) return null;
            var json = JsonConvert.SerializeObject(item);
            foreach (var conceptValue in _conceptValues)
            {
                if (!_translations.ContainsKey(conceptValue)) continue;
                json = json.Replace(conceptValue, _translations[conceptValue]);
            }
            var translatedItem = JsonConvert.DeserializeObject(json, type);
            FulcrumAssert.AreEqual(translatedItem.GetType(), type);
            return translatedItem;
        }

        #region private methods
        private void DecorateClassOrCollection(object o, int depth)
        {
            if (depth > 20)
            {
                var objectAsJson = JsonConvert.SerializeObject(o, Formatting.Indented);
                Log.LogWarning($"Will not decorate object deeper than level {depth - 1}.\r Remaining levels to decorate: {objectAsJson}");
                return;
            }
            try
            {
                switch (o)
                {
                    case null:
                        return;
                    case ICollection collection:
                        {
                            foreach (var item in collection)
                            {
                                // Recursive call
                                DecorateClassOrCollection(item, depth + 1);
                            }

                            return;
                        }
                }

                var objectType = o.GetType();
                if (!objectType.IsClass)
                {
                    return;
                }

                var properties = objectType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                foreach (var property in properties)
                {
                    DecorateClassProperty(o, property, depth + 1);
                }
            }
            catch (Exception e)
            {
                if (depth > 0) throw;
                var objectAsJson = JsonConvert.SerializeObject(o, Formatting.Indented);
                Log.LogWarning($"Could not decorate object. Parts of the object may have been decorated. \rObject: {objectAsJson}\rException: {e.Message}", e);
            }
        }

        private void DecorateClassProperty(object o, PropertyInfo property, int depth)
        {
            var conceptAttribute = GetConceptAttribute(property);
            if (conceptAttribute != null)
            {
                DecoratePropertyWithConceptName(conceptAttribute.ConceptName, o, property);
            }
            else if (property.CanRead)
            {
                DecorateClassOrCollection(property.GetValue(o), depth);
            }
        }

        public static TranslationConceptAttribute GetConceptAttribute(PropertyInfo property)
        {
            return (TranslationConceptAttribute)property?.GetCustomAttributes(false).FirstOrDefault(a => a is TranslationConceptAttribute);
        }

        private bool IsDecorated(string value)
        {
            return ConceptValue.TryParse(value, out _);
        }

        private void DecoratePropertyWithConceptName(string conceptName, object o, PropertyInfo property)
        {
            if (!property.CanRead)
            {
                Log.LogWarning($"Can't decorate property {property.Name} (concept {conceptName}), because the property is not readable.");
                return;
            }
            var currentValue = property.GetValue(o);
            switch (currentValue)
            {
                case null:
                    break;
                case string s:
                    {
                        if (!property.CanWrite)
                        {
                            Log.LogWarning($"Can't decorate property {property.Name} (concept {conceptName}), because the property is not writable.");
                            return;
                        }
                        var newValue = Decorate(conceptName, s);
                        property.SetValue(o, newValue);
                        break;
                    }
                case string[] stringArray:
                    for (var i = 0; i < stringArray.Length; i++)
                    {
                        stringArray[i] = Decorate(conceptName, stringArray[i]);
                    }
                    break;
                case IList<string> stringList:
                    var length = stringList.Count;
                    for (var i = 0; i < length; i++)
                    {
                        stringList[i] = Decorate(conceptName, stringList[i]);
                    }
                    break;
                default:
                    Log.LogWarning(
                        $"{typeof(Translator).FullName} can't decorate the property {property.Name}" + 
                        $" because its type ({property.PropertyType.Name}) is currently not supported." + 
                        " The types that are supported are: string, string[] and IList<string>.");
                    break;
            }
        }

        #endregion
    }
}