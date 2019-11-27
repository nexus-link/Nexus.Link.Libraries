using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Core.Translation;
#pragma warning disable 659

namespace Nexus.Link.Libraries.Web.AspNet.Tests.InboundPipe.Support
{
    public class Foo : IUniquelyIdentifiable<string>
    {
        public const string IdConceptName = "foo.id";
        public const string ConsumerName = "consumer";
        public const string ProducerName = "producer";
        public const string ConsumerId1 = "consumer-1";
        public const string ProducerId1 = "producer-1";

        /// <inheritdoc />
        [TranslationConcept(IdConceptName)]
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