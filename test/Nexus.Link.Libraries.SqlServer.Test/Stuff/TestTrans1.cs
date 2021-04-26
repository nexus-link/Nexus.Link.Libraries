using System;
using Nexus.Link.Libraries.Core.Storage.Model;

namespace Nexus.Link.Libraries.SqlServer.Test.Stuff
{
    public class TestTrans1 : TestTrans1Create, IUniquelyIdentifiable<Guid>
    {
        public Guid Id { get; set; }
    }

    public class TestTrans1Create
    {
        public TestTrans1Create()
        {
            Field1 = Guid.NewGuid().ToString().ToLower();
            Field2 = Guid.NewGuid().ToString().ToLower();
        }

        public string Field1 { get; set; }
        public string Field2 { get; set; }
    }
}