﻿using System;

namespace Nexus.Link.Libraries.Core.Translation
{
    /// <summary>
    /// Information about a translation concept
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
    public class TranslationConceptAttribute : Attribute
    {
        /// <summary>
        /// The name of the concept
        /// </summary>
        public string ConceptName { get; }

        /// <summary>
        /// Add information about a translation concept.
        /// </summary>
        /// <param name="conceptName">The <see cref="ConceptName"/>.</param>
        public TranslationConceptAttribute(string conceptName)
        {
            ConceptName = conceptName;
        }
    }
}