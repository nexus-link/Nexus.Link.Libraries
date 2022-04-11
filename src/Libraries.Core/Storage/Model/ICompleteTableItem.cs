namespace Nexus.Link.Libraries.Core.Storage.Model
{
    /// <summary>
    /// The interfaces we expect from a database table item.
    /// </summary>
    public interface ICompleteTableItem : ITableItem, ITimeStamped, IRecordVersion
    {
    }
}