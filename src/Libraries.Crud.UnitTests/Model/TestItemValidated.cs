using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.Libraries.Crud.UnitTests.Model
{
    /// <summary>
    /// A minimal storable item that implements <see cref="IValidatable"/> to be used in testing
    /// </summary>
    public class TestItemValidated<TId> : TestItemId<TId>, IValidatable
    {
        public void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsTrue(Value != TestItemBare.ValueToMakeValidationFail, errorLocation, 
                $"The value must not be {TestItemBare.ValueToMakeValidationFail}");
        }
    }
}
