using System;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;

namespace Nexus.Link.Libraries.Core.Guards
{
    public class GuardCondition<T>
    {
        internal readonly GuardConfiguration<T> Config;
        internal bool IsNegationActive { get; private set; }

        public GuardCondition(GuardConfiguration<T> config)
        {
            Config = config;
        }

        internal T Value => Config == null ? default : Value;

        internal string TheValueAsStringOrTheWordNull() => Value == null ? "null" : Value.ToString();

        internal Type ValueTypeOrGenericType() => Value == null ? typeof(T) : Value.GetType();

        public string MaybeTheWordNot => IsNegationActive ? "not " : "";

        public GuardCondition<T> Not
        {
            get
            {
                IsNegationActive = !IsNegationActive;
                return this;
            }
        }
    }
}