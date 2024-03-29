﻿namespace Nexus.Link.Libraries.Crud.UnitTests.Model
{
    /// <summary>
    /// Methods needed for automatic testing of persistent storage implementations.
    /// </summary>
    public interface IItemForTesting
    {
        /// <summary>
        /// Fills all mandatory fields  with valid data.
        /// </summary>
        /// <param name="typeOfTestData">Decides what kind of data to fill with, <see cref="TypeOfTestDataEnum"/>.</param>
        /// <returns>The item itself ("this").</returns>
        void InitializeWithDataForTesting(TypeOfTestDataEnum typeOfTestData);

        /// <summary>
        /// Changes the information in a way that would make the item not equal to the state before the changes. 
        /// </summary>
        /// <returns>The item itself ("this").</returns>
        void ChangeDataToNotEqualForTesting();
    }
}
