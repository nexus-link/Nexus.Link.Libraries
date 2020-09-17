using System;
using Nexus.Link.Libraries.Core.Storage.Model;

namespace Nexus.Link.Libraries.Crud.Web.Test.Support.Models
{
    public class Person : IUniquelyIdentifiable<Guid>
    {
        public string GivenName { get; set; }
        public string Surname { get; set; }

        /// <inheritdoc />
        public Guid Id { get; set; }

        public Guid? ParentId { get; set; }

        /// <inheritdoc />
        public override bool Equals(object o)
        {
            if (!(o is Person person)) return false;
            return Equals(person);
        }

        protected bool Equals(Person other)
        {
            return string.Equals(GivenName, other.GivenName) && string.Equals(Surname, other.Surname);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                // ReSharper disable NonReadonlyMemberInGetHashCode
                return ((GivenName != null ? GivenName.GetHashCode() : 0) * 397) ^ (Surname != null ? Surname.GetHashCode() : 0);
                // ReSharper restore NonReadonlyMemberInGetHashCode
            }
        }
    }
}
