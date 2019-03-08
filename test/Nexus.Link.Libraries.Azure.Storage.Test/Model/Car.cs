using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Storage.Model;

namespace Nexus.Link.Libraries.Azure.Storage.Test.Model
{
    public class Car : IUniquelyIdentifiable<string>, IValidatable, IOptimisticConcurrencyControlByETag
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsNotNullOrWhiteSpace(Name, nameof(Name), errorLocation);
            if (Id != null || Etag != null)
            {
                FulcrumValidate.IsNotNullOrWhiteSpace(Id, nameof(Id), errorLocation);
                FulcrumValidate.IsNotNullOrWhiteSpace(Etag, nameof(Etag), errorLocation);
            }
        }

        public string Etag { get; set; }
    }
}
