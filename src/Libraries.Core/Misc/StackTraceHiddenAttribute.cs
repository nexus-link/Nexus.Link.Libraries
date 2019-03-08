using System;
using Nexus.Link.Libraries.Core.Error.Logic;

namespace Nexus.Link.Libraries.Core.Misc
{
    /// <summary>
    /// Attribute to hide methods from <see cref="FulcrumException"/> stack traces.
    /// Add this to methods that you don't want to be part of the stack trace.
    /// Typically used for FulcrumAssert methods.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class StackTraceHiddenAttribute : Attribute
    {
    }
}