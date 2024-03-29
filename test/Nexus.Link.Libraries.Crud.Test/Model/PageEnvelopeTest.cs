﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Crud.Model;

namespace Nexus.Link.Libraries.Crud.Test.Model
{
    [TestClass]
    public class SearchDetailsTests
    {

        [DataTestMethod]
        [DataRow(null, false)]
        [DataRow("", false)]
        [DataRow("A?", true)]
        [DataRow("A\\?", false)]
        [DataRow(@"A\?", false)]
        [DataRow(@"A\?\?", false)]
        [DataRow(@"A\\?", true)]
        [DataRow(@"A\\\?", false)]
        [DataRow(@"A\\\\?", true)]
        [DataRow(@"A\??", true)]
        [DataRow(@"A??", true)]
        [DataRow("A*", true)]
        [DataRow("A\\*", false)]
        [DataRow(@"A\*", false)]
        [DataRow(@"A\*\*", false)]
        [DataRow(@"A\\*", true)]
        [DataRow(@"A\\\*", false)]
        [DataRow(@"A\\\\*", true)]
        [DataRow(@"A\**", true)]
        [DataRow(@"A**", true)]
        [DataRow(@"A\*?", true)]
        [DataRow(@"A\?*", true)]

        public void TestIsWildCard(string searchFor, bool expectedIsWildCard)
        {
            var isWildCard = SearchDetails<string>.ReplaceWildCard(searchFor).Item1;
            Assert.AreEqual(expectedIsWildCard, isWildCard);
        }

    }
}
