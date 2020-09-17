namespace Nexus.Link.Libraries.Core.Translation
{
    public interface ITranslationClientName
    {
        /// <summary>
        /// The name of the consumer or producer that is the translation target
        /// </summary>
        string TranslationClientName { get; }
    }
}