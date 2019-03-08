using System;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Storage.Model;

namespace Nexus.Link.Libraries.Core.Tests.Support
{
    public class PersonStorableItem : StorableItem, INameable
    {
        public PersonStorableItem(string givenName, string surname)
        {
            GivenName = givenName;
            Surname = surname;
        }

        /// <summary>
        /// The given name (western "first name") for the person.
        /// </summary>
        public string GivenName { get; set; }

        /// <summary>
        /// The surname (western "last name") for the person.
        /// </summary>
        public string Surname { get; set; }

        /// <inheritdoc />
        public string Name => $"{GivenName} {Surname}";

        public override bool Equals(object obj)
        {
            if (!(obj is PersonStorableItem person)) return false;
            if (!Equals(person.Id, Id)) return false;
            if (!string.Equals(person.Etag, Etag, StringComparison.OrdinalIgnoreCase)) return false;
            if (!string.Equals(person.GivenName, GivenName, StringComparison.OrdinalIgnoreCase)) return false;
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (!string.Equals(person.Surname, Surname, StringComparison.OrdinalIgnoreCase)) return false;
            return true;
        }

        public override int GetHashCode()
        {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return Id.GetHashCode();
        }

        /// <inheritdoc />
        public override void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsNotNullOrWhiteSpace(GivenName, nameof(GivenName), errorLocation);
            FulcrumValidate.IsNotNullOrWhiteSpace(Surname, nameof(Surname), errorLocation);
        }
    }
}
