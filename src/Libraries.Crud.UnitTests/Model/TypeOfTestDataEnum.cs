namespace Nexus.Link.Libraries.Crud.UnitTests.Model
{
    /// <summary>
    /// Enumeration for the different kinds of data that we expect from a testable class.
    /// </summary>
    public enum TypeOfTestDataEnum
    {
        /// <summary>
        ///  A fixed set of data with "normal" values.
        /// </summary>
        Default,
        /// <summary>
        ///  Data that makes the validation fail.
        /// </summary>
        ValidationFail,
        /// <summary>
        ///  A fixed set of data, not the same data as <see cref="Variant2"/>.
        /// </summary>
        Variant1,
        /// <summary>
        ///  A fixed set of data, not the same data as <see cref="Variant1"/>.
        /// </summary>
        Variant2,
        /// <summary>
        /// A random set of data, shouldn't be Equal to any other instance.
        /// </summary>
        Random,
        /// <summary>
        /// A new Guid as string.
        /// </summary>
        Guid,
        /// <summary>
        /// A null value
        /// </summary>
        NullValue,
        /// <summary>
        /// An empty value
        /// </summary>
        EmptyValue
    };
}