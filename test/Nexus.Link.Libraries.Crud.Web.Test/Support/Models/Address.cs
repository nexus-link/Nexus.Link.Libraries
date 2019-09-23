using System;
using Nexus.Link.Libraries.Core.Storage.Model;

namespace Nexus.Link.Libraries.Crud.Web.Test.Support.Models
{
    public class Address : IUniquelyIdentifiable<Guid>
    {
        /// <inheritdoc />
        public Guid Id { get; set; }

        public string Street { get; set; }

        public Guid? PersonId { get; set; }
        public string City { get; set; }

        /// <inheritdoc />
        public override bool Equals(object o)
        {
            if (!(o is Address address)) return false;
            return Equals(address);
        }

        protected bool Equals(Address other)
        {
            return string.Equals(Street, other.Street) && string.Equals(City, other.City);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                // ReSharper disable NonReadonlyMemberInGetHashCode
                return ((Street != null ? Street.GetHashCode() : 0) * 397) ^ (City != null ? City.GetHashCode() : 0);
                // ReSharper restore NonReadonlyMemberInGetHashCode
            }
        }
    }
}
