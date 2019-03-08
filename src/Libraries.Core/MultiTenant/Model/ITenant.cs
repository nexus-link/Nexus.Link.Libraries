﻿using System;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Logging;

namespace Nexus.Link.Libraries.Core.MultiTenant.Model
{
    /// <summary>
    /// Information about a tenant in the Fulcrum multi tenant runtime.
    /// </summary>
    [Obsolete("Use Tenant.", true)]
    public interface ITenant : IValidatable, ILoggable
    {
        /// <summary>
        /// A unique lowercase abbreviation or acronym for the organization, e.g. "sef" for Svensk Elitfotboll
        /// </summary>
        string Organization { get; }
        /// <summary>
        /// A lowercase ascii name for the organization environment, e.g. "local", "dev", "test", "ver", "integration-test", "prd", "production", etc.
        /// </summary>
        string Environment { get; }
    }
}