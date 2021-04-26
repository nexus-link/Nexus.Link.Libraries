using System.Data;

namespace Nexus.Link.Libraries.Crud.Interfaces
{
    public interface ICreateConnection
    {
        IDbConnection CreateConnection();
    }
}