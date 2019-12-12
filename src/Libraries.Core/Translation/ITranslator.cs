using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Crud.Model;
using Nexus.Link.Libraries.Core.Storage.Model;

namespace Nexus.Link.Libraries.Core.Translation
{
    /// <summary>
    /// Core methods for translations
    /// </summary>
    public interface ITranslator
    {
        /// <summary>
        /// Add all concept values in the <paramref name="item"/> to the list of values to be translated.
        /// </summary>
        ITranslator Add<T>(T item);

        /// <summary>
        /// Add all concept values in the string <paramref name="s"/> to the list of values to be translated.
        /// </summary>
        ITranslator AddSubStrings(string s);

        /// <summary>
        /// Decorate the <paramref name="value"/> into a concept value path.
        /// </summary>
        string Decorate(string conceptName, string value);

        /// <summary>
        /// Decorate the <paramref name="id"/> with concept value paths.
        /// </summary>
        SlaveToMasterId<string> Decorate(string masterIdConceptName, string slaveIdConceptName, SlaveToMasterId<string> id);

        /// <summary>
        /// Decorate the <paramref name="item"/> so that concept values are set to concept value paths.
        /// </summary>
        [Obsolete("Use the method Decorate<T>(T). Obsolete since 2019-11-21.")]
        TModel DecorateItem<TModel>(TModel item);

        /// <summary>
        /// Decorate the <paramref name="item"/> so that concept values are set to concept value paths.
        /// </summary>
        T Decorate<T>(T item);

        /// <summary>
        /// Decorate the <paramref name="items"/> so that concept values are set to concept value paths.
        /// </summary>
        IEnumerable<T> Decorate<T>(IEnumerable<T> items);

        /// <summary>
        /// Decorate the <paramref name="item"/> so that concept values are set to concept value paths.
        /// </summary>
        object Decorate(object item, Type type);

        /// <summary>
        /// Decorate the <paramref name="items"/> so that concept values are set to concept value paths.
        /// </summary>
        IEnumerable<object> Decorate(IEnumerable<object> items, Type type);

        /// <summary>
        ///  Decorate the <paramref name="items"/> so that concept values are set to concept value paths.
        /// </summary>
        [Obsolete("Use the method Decorate<T>(T). Obsolete since 2019-11-21.")]
        IEnumerable<TModel> DecorateItems<TModel>(IEnumerable<TModel> items);

        /// <summary>
        /// Decorate the <paramref name="page"/> so that concept values are set to concept value paths.
        /// </summary>
        [Obsolete("Use the method Decorate<T>(T). Obsolete since 2019-11-21.")]
        PageEnvelope<TModel> DecoratePage<TModel>(PageEnvelope<TModel> page);

        /// <summary>
        /// Do the actual translation.
        /// </summary>
        /// <returns></returns>
        Task ExecuteAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Find all concept values in the <paramref name="item"/>, translate them and return the result.
        /// </summary>
        /// <remarks>Convenience function for <see cref="Translate"/></remarks>
        T Translate<T>(T item);

        /// <summary>
        /// Find all concept values in the <paramref name="item"/>, translate them and return the result.
        /// </summary>
        object Translate(object item, Type type);
    }
}