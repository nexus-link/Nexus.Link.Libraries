using Microsoft.VisualStudio.TestTools.UnitTesting;
using UT = Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Nexus.Link.Libraries.Core.Tests.Translate
{
    [TestClass]
    public class TestConceptValue
    {
        [TestMethod]
        public void ParseContext()
        {
            var conceptValue = Translation.ConceptValue.Parse("(concept!context!value)");
            UT.Assert.AreEqual("context", conceptValue.ContextName);
            UT.Assert.AreEqual("value", conceptValue.Value);
            UT.Assert.IsNull(conceptValue.ClientName);
        }

        [TestMethod]
        public void ParseClient()
        {
            var conceptValue = Translation.ConceptValue.Parse("(concept!~client!value)");
            UT.Assert.AreEqual("concept", conceptValue.ConceptName);
            UT.Assert.AreEqual("client", conceptValue.ClientName);
            UT.Assert.AreEqual("value", conceptValue.Value);
            UT.Assert.IsNull(conceptValue.ContextName);
        }

        [TestMethod]
        public void ContextToPath()
        {
            var conceptValue = new Translation.ConceptValue
            {
                ConceptName = "concept",
                ContextName = "context",
                Value = "value"
            };
            var path = conceptValue.ToPath();
            UT.Assert.AreEqual("(concept!context!value)", path);
        }

        [TestMethod]
        public void ClientToPath()
        {
            var conceptValue = new Translation.ConceptValue
            {
                ConceptName = "concept",
                ClientName = "client",
                Value = "value"
            };
            var path = conceptValue.ToPath();
            UT.Assert.AreEqual("(concept!~client!value)", path);
        }
    }
}
