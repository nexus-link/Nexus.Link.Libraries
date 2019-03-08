using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Tests.Context;
using UT = Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Nexus.Link.Libraries.Core.Tests.Application
{
    [TestClass]
    public class TestConfigurationManagerAppSettings
    {
        [TestInitialize]
        public void Initialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(TestConfigurationManagerAppSettings));
        }

        [TestMethod]
        public void NoExceptionForMissingFile()
        {
            new ConfigurationManagerAppSettings().WithLocalSettingsSupport(new FileInfo("non-existing.json"));
        }

        [TestMethod]
        public void ReadSimpleSettingsFromJsonFile()
        {
            var getter = new ConfigurationManagerAppSettings().WithLocalSettingsSupport(new FileInfo("Application/appsettings.json"));
            UT.Assert.AreEqual("Bar", getter.GetAppSetting("Foo"));
        }

        [TestMethod]
        public void ReadNestedSettingFromJsonFile()
        {
            var getter = new ConfigurationManagerAppSettings().WithLocalSettingsSupport(new FileInfo("Application/appsettings.json"));
            UT.Assert.IsNull(getter.GetAppSetting("FruitsAndStars"), "Only stringy values should be supported");
        }
    }
}
