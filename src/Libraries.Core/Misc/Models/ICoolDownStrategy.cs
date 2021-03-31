using System;

namespace Nexus.Link.Libraries.Core.Misc.Models
{
    public interface ICoolDownStrategy
    {
        DateTimeOffset LastFailAt { get; }
        DateTimeOffset NextTryAt { get; }
        bool HasCooledDown { get; }
        void Reset();
        void StartNextCoolDownPeriod();
    }
}