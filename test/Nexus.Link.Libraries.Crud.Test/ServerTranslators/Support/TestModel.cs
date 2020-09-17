using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Core.Translation;

#pragma warning disable 659

namespace Nexus.Link.Libraries.Crud.Test.ServerTranslators.Support
{
    /// <inheritdoc cref="TestModelCreate" />
    public class TestModel : TestModelCreate, IUniquelyIdentifiable<string>, IOptimisticConcurrencyControlByETag
    {
        public const string IdConceptName = "testmodel.id";

        /// <inheritdoc />
        [TranslationConcept("testmodel.id")]
        public string Id { get; set; }

        /// <inheritdoc />
        public string Etag { get; set; }

        public static string DecoratedId(string name, string id) => $"({IdConceptName}!~{name}!{id})";

        public string DecoratedId(string name) => DecoratedId(name, Id);

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (!(obj is TestModel testModel)) return false;
            return Equals(Id, testModel.Id) && base.Equals(obj);
        }
    }
}