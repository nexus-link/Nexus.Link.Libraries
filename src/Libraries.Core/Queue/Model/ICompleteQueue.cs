namespace Nexus.Link.Libraries.Core.Queue.Model
{
    /// <summary>
    /// A queue that has complete queue functionality.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICompleteQueue<T>: IReadableQueue<T>, IPeekableQueue<T>, IWritableQueue<T>, ICountableQueue
    {
    }
}
