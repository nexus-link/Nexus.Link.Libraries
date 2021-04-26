using System;
using Nexus.Link.Libraries.Core.Storage.Model;

namespace Nexus.Link.Libraries.SqlServer.Test.Stuff
{
    public class TestTrans2 : TestTrans2Create, IUniquelyIdentifiable<Guid>
    {
        public Guid Id { get; set; }
    }

    public class TestTrans2Create
    {
        public TestTrans2Create()
        {
            Field1 = Guid.NewGuid().ToString().ToLower();
            Field2 = Guid.NewGuid().ToString().ToLower();
        }

        public string Field1 { get; set; }
        public string Field2 { get; set; }
    }
}