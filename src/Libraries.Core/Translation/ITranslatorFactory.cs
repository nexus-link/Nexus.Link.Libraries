namespace Nexus.Link.Libraries.Core.Translation
{
    public interface ITranslatorFactory: ITranslationTargetClientName
    {
        ITranslator CreateTranslator();
    }
}