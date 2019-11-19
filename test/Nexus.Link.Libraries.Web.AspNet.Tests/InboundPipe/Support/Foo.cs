using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Core.Translation;
#pragma warning disable 659

namespace Nexus.Link.Libraries.Web.AspNet.Tests.InboundPipe.Support
{
    internal class Foo : IUniquelyIdentifiable<string>
    {
        /// <inheritdoc />
        [TranslationConcept("foo.id")]
        public string Id { get; set; }

        public string Name { get; set; }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (!(obj is Foo other)) return false;
            return other.Id == Id && other.Name == Name;
        }
    }
}