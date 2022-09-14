using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Misc;
using Shouldly;
using System;

namespace Nexus.Link.Libraries.Core.Tests.Misc
{
    [TestClass]
    public class StringExtensionsTests
    {

        [TestInitialize]
        public void Initialize()
        {
        }

        [DataTestMethod]
        [DataRow(null, null, null)]
        [DataRow("prefix", null, null)]
        [DataRow(null, "long string", "long string")]
        [DataRow("ef", "abcdef", "abcdef")]
        [DataRow("a", "abcdef", "bcdef")]
        [DataRow("ab", "abcdef", "cdef")]
        [DataRow("abcdef", "abcdef", "")]
        [DataRow("Central", "Person", "Person")]
        [DataRow("ab", "a", "a")]
        public void RemovePrefix_DefaultStringComparison(string prefix, string longString, string expectedResult)
        {
            var actualResult = longString.RemovePrefix(prefix);
            actualResult.ShouldBe(expectedResult);
        }

        [DataTestMethod]
        [DataRow("A", "abc", StringComparison.InvariantCulture, "abc")]
        [DataRow("a", "ABC", StringComparison.InvariantCulture, "ABC")]
        [DataRow("A", "abc", StringComparison.InvariantCultureIgnoreCase, "bc")]
        [DataRow("a", "ABC", StringComparison.InvariantCultureIgnoreCase, "BC")]
        [DataRow("Å", "åäö", StringComparison.InvariantCulture, "åäö")]
        [DataRow("å", "ÅÄÖ", StringComparison.InvariantCulture, "ÅÄÖ")]
        [DataRow("Å", "åäö", StringComparison.InvariantCultureIgnoreCase, "äö")]
        [DataRow("å", "ÅÄÖ", StringComparison.InvariantCultureIgnoreCase, "ÄÖ")]
        [DataRow("Å", "åäö", StringComparison.CurrentCulture, "åäö")]
        [DataRow("å", "ÅÄÖ", StringComparison.CurrentCulture, "ÅÄÖ")]
        [DataRow("Å", "åäö", StringComparison.CurrentCultureIgnoreCase, "äö")]
        [DataRow("å", "ÅÄÖ", StringComparison.CurrentCultureIgnoreCase, "ÄÖ")]
        public void RemovePrefix_StringComparisons(string prefix, string longString, StringComparison stringComparison, string expectedResult)
        {
            var actualResult = longString.RemovePrefix(prefix, stringComparison);
            actualResult.ShouldBe(expectedResult);
        }

        [DataTestMethod]
        [DataRow(null, null, null)]
        [DataRow(null, "suffix", null)]
        [DataRow("long string", null, "long string")]
        [DataRow("abcdef", "ab", "abcdef")]
        [DataRow("abcdef", "f", "abcde")]
        [DataRow("abcdef", "ef", "abcd")]
        [DataRow("abcdef", "abcdef", "")]
        [DataRow("a", "ab", "a")]
        public void RemoveSuffix_DefaultComparison(string longString, string suffix, string expectedResult)
        {
            var actualResult = longString.RemoveSuffix(suffix);
            actualResult.ShouldBe(expectedResult);
        }

        [DataTestMethod]
        [DataRow("a.b.c", '-', "a.b.c")]
        [DataRow("a.b.c", '.', "a.b")]
        [DataRow("a.b.last part - with no ellipsis", '.', "a.b")]
        public void RemoveLastSeparated(string longString, char separator, string expectedResult)
        {
            var actualResult = longString.RemoveLastSeparated(separator);
            actualResult.ShouldBe(expectedResult);
        }
    };
}
