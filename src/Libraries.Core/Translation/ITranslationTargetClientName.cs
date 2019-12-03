namespace Nexus.Link.Libraries.Core.Translation
{
    public interface ITranslationTargetClientName
    {
        /// <summary>
        /// The name of the consumer or producer that is the translation target
        /// </summary>
        string TargetClientName { get; }
    }
}