namespace Nexus.Link.Libraries.Core.Platform.AsyncProcesses
{
    public interface ILockable
    {
        LockWithTimeout LockWithTimeout { get; }
    }
}