﻿using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.Libraries.Core.MultiTenant.Model
{
    /// <summary>
    /// Information about a tenant in the Fulcrum multi tenant runtime.
    /// </summary>
    public class Tenant : IValidatable
    {
        private static readonly string Namespace = typeof(Tenant).Namespace;

        /// <summary>
        /// Constructor
        /// </summary>
        public Tenant(string organization, string environment)
        {
            InternalContract.RequireNotNullOrWhiteSpace(organization, nameof(organization));
            InternalContract.RequireNotNullOrWhiteSpace(environment, nameof(environment));
            Organization = organization?.ToLower();
            Environment = environment?.ToLower();
            Validate($"{Namespace}: 80BAC4F7-6369-4ACD-A34F-413A20E24C27");
        }

        /// <summary>
        /// Empty constructor. Use the overload <see cref="Tenant(string,string)"/>, the empty constructor should only be used for deserialization. 
        /// </summary>
        public Tenant()
        {
        }

        /// <summary>
        /// A unique lowercase abbreviation or acronym for the organization, e.g. "sef" for Svensk Elitfotboll
        /// </summary>
        public string Organization { get; set; }
        /// <summary>
        /// A lowercase name of the organization environment, e.g. "local", "dev", "tst", "ver", "int", "prd", "production", etc.
        /// </summary>
        public string Environment { get; set; }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Organization} ({Environment})";
        }

        /// <inheritdoc />
        public string ToLogString() => ToString();

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is Tenant tenant && ToString().Equals(tenant.ToString());
        }

        /// <inheritdoc />
        public void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsNotNullOrWhiteSpace(Organization, nameof(Organization), errorLocation);
            FulcrumValidate.IsNotNullOrWhiteSpace(Environment, nameof(Environment), errorLocation);
            FulcrumValidate.AreEqual(Organization.ToLower(), Organization, nameof(Organization), errorLocation);
            FulcrumValidate.AreEqual(Environment.ToLower(), Environment, nameof(Environment), errorLocation);
        }
    }
}
