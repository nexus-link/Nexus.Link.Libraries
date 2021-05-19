using System;

namespace Nexus.Link.Libraries.Web.AspNet.Annotations
{
    /// <summary>
    /// Use in conjunction with <see cref="SwaggerExtensions.EnableCapabilityGrouping(Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions)"/> to enable UI grouping of entities by capability.
    /// </summary>
    public class CapabilityGroupingAttribute : Attribute
    {
        public const string DescriptionDivder = " : ";

        private readonly string[] _parts;

        /// <summary>
        /// Each part will be joined with <see cref="DescriptionDivder"/> to visualize the hierarchy.
        ///
        /// <see cref="SwaggerExtensions.EnableCapabilityGrouping(Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions)"/>
        /// will use javascript DOM manipulation to group entities into capabilities.
        ///
        /// E.g. ("Consent management", "Consent") yields the tag "Consent management : Consent".
        /// </summary>
        public CapabilityGroupingAttribute(params string[] parts)
        {
            _parts = parts;
        }

        public string Description => string.Join(DescriptionDivder, _parts);
    }
}
