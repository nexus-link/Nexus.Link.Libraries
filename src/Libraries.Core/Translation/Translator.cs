﻿using System;
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
            return (T) Decorate(item, typeof(T));
        }

        /// <inheritdoc />
        public object Decorate(object item, Type type)
        {
            if (item == null) return null;
            DecorateInternal(item);
            return item;
        }

        /// <inheritdoc />
        public IEnumerable<T> Decorate<T>(IEnumerable<T> items)
        {
            return items?.Select(Decorate);
        }

        /// <inheritdoc />
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
           return (T) Translate(item, typeof(T));
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
        private void DecorateInternal(object o)
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
                        DecorateInternal(item);
                    }
                    return;
                }
            }

            var objectType = o.GetType();
            if (objectType.IsPrimitive)
            {
                return;
            }
            foreach (var property in objectType.GetProperties())
            {
                DecorateProperty(o, property);
            }
        }

        private void DecorateProperty(object o, PropertyInfo property)
        {
            if (property.PropertyType.IsPrimitive)
            {
                return;
            }
            var conceptAttribute = GetConceptAttribute(property);
            if (conceptAttribute != null)
            {
                DecorateWithReflection(conceptAttribute.ConceptName, o, property);
            }
            else
            {
                DecorateInternal(property.GetValue(o));
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

        private void DecorateWithReflection(string conceptName, object o, PropertyInfo property)
        {
            var currentValue = property.GetValue(o);
            switch (currentValue)
            {
                case null:
                    break;
                case ICollection<string> strings:
                {
                    var newValue = strings.Select(s => Decorate(conceptName, s));
                    switch (strings)
                    {
                        case string[] _:
                            property.SetValue(o, newValue.ToArray());
                            break;
                        case List<string> _:
                            property.SetValue(o, newValue.ToList());
                            break;
                        default:
                            FulcrumAssert.Fail(
                                $"Failed to decorate a collection of strings; no translation method for collections of type {currentValue.GetType().FullName}:" + 
                                $" Currently a collection can only have the {nameof(TranslationConceptAttribute)} attribute if it is of type  {typeof(string[]).Name} or {typeof(List<string>).Name}, which was not true for property {property.Name} ({property.PropertyType.Name}).");
                            break;
                    }
                    break;
                }
                case string s:
                {
                    var newValue = Decorate(conceptName, s);
                    property.SetValue(o, newValue);
                    break;
                }
            }
        }

        #endregion
    }
}