using System;
using Nexus.Link.Libraries.Crud.UnitTests.Model;

namespace Nexus.Link.Libraries.SqlServer.Test.Stuff
{
    public class Storage
    {
        public Storage(CrudSql<TestTrans1Create, TestTrans1> testTrans1, CrudSql<TestTrans2Create, TestTrans2> testTrans2)
        {
            TestTrans1 = testTrans1;
            TestTrans2 = testTrans2;
        }

        public CrudSql<TestTrans1Create, TestTrans1> TestTrans1 { get; }
        public CrudSql<TestTrans2Create, TestTrans2> TestTrans2 { get; }
    }
}