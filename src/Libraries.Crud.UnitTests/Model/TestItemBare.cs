﻿using System;

namespace Nexus.Link.Libraries.Crud.UnitTests.Model
{
    /// <summary>
    /// A minimal storable item to be used in testing
    /// </summary>
    public partial class TestItemBare
    {
        /// <summary>
        /// The property to save.
        /// </summary>
        public string Value { get; set; }

    }

    public partial class TestItemBare : IItemForTesting
    {
        public const string ValueToMakeValidationFail = "THIS WILL FAIL";

        public static int Modulo { get; set; } = 3;

        public static int Count { get; set; } = 1;
        public virtual void InitializeWithDataForTesting(TypeOfTestDataEnum typeOfTestData)
        {
            switch (typeOfTestData)
            {
                case TypeOfTestDataEnum.Default:
                    Value = "Default";
                    break;
                case TypeOfTestDataEnum.ValidationFail:
                    Value = ValueToMakeValidationFail;
                    break;
                case TypeOfTestDataEnum.NullValue:
                    Value = null;
                    break;
                case TypeOfTestDataEnum.EmptyValue:
                    Value = "";
                    break;
                case TypeOfTestDataEnum.Variant1:
                    Value = "Variant1";
                    break;
                case TypeOfTestDataEnum.Variant2:
                    Value = "Variant2";
                    break;
                case TypeOfTestDataEnum.Guid:
                    Value = Guid.NewGuid().ToString();
                    break;
                case TypeOfTestDataEnum.Random:
                    Value = Guid.NewGuid().ToString();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(typeOfTestData), typeOfTestData, null);
            }

            switch (this)
            {
                case TestItemSort<Guid> sortItem:
                    sortItem.IncreasingNumber = Count;
                    sortItem.NumberModulo = Count % Modulo;
                    sortItem.DecreasingString = (short.MaxValue - Count).ToString();
                    Count++;
                    break;
                case TestItemSort<string> sortItem:
                    sortItem.IncreasingNumber = Count;
                    sortItem.NumberModulo = Count % Modulo;
                    sortItem.DecreasingString = (short.MaxValue - Count).ToString();
                    Count++;
                    break;
            }
        }

        public virtual void ChangeDataToNotEqualForTesting()
        {
            Value = Guid.NewGuid().ToString();
        }
    }

    #region override object
    public partial class TestItemBare
    {
        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (!(obj is TestItemBare person)) return false;
            if (!string.Equals(person.Value, Value)) return false;
            return true;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return Value.GetHashCode();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Value;
        }
    }
    #endregion
}
