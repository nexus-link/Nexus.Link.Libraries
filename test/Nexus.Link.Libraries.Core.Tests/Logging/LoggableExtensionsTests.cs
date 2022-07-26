using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Logging;
using Shouldly;
using UT = Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Nexus.Link.Libraries.Core.Tests.Logging
{
    [TestClass]
    public class LoggableExtensionsTests
    {

        [TestInitialize]
        public void Initialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(LoggableExtensionsTests).FullName);
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        }

        [DataTestMethod]
        [DataRow(0, 0, 0, 0, 1, "1ms")]
        [DataRow(0, 0, 0, 0, 57, "57ms")]
        [DataRow(0, 0, 0, 0, 999, "999ms")]
        [DataRow(0, 0, 0, 0, 1000, "1.00s")]
        [DataRow(0, 0, 0, 0, 1009, "1.01s")]
        [DataRow(0, 0, 0, 1, 994, "1.99s")]
        [DataRow(0, 0, 0, 1, 995, "2.0s")]
        [DataRow(0, 0, 0, 2, 0, "2.0s")]
        [DataRow(0, 0, 0, 9, 949, "9.9s")]
        [DataRow(0, 0, 0, 9, 950, "10.0s")]
        [DataRow(0, 0, 0, 10, 0, "10.0s")]
        [DataRow(0, 0, 0, 29, 999, "29.5s")]
        [DataRow(0, 0, 0, 30, 000, "30s")]
        [DataRow(0, 0, 0, 89, 999, "90s")]
        [DataRow(0, 0, 0, 90, 0, "1min 30s")]
        [DataRow(0, 0, 0, 91, 0, "1min 31s")]
        [DataRow(0, 0, 9, 59, 994, "9min 59s")]
        [DataRow(0, 0, 9, 59, 995, "9min 59s")]
        [DataRow(0, 0, 10, 0, 0, "10.0min")]
        [DataRow(0, 0, 29, 59, 999, "29.5min")]
        [DataRow(0, 0, 30, 0, 0, "30min")]
        [DataRow(0, 0, 89, 59, 999, "90min")]
        [DataRow(0, 0, 90, 0, 0, "1h 30min")]
        [DataRow(0, 11, 59, 59, 999, "11h 59min")]
        [DataRow(0, 12, 0, 0, 0, "12.0h")]
        [DataRow(0, 12, 31, 0, 0, "12.5h")]
        [DataRow(0, 23, 59, 59, 999, "23.5h")]
        [DataRow(0, 24, 0, 0, 0, "1 days 0h")]
        [DataRow(6, 23, 59, 59, 999, "6 days 23h")]
        [DataRow(7, 0, 0, 0, 0, "7.0 days")]
        [DataRow(13, 23, 59, 59, 999, "13.5 days")]
        [DataRow(14, 0, 0, 0, 0, "14 days")]
        [DataRow(1000, 23, 59, 59, 999, "1001 days")]
        public void ToLogString_TimeSpan(int days, int hours, int minutes, int seconds, int milliseconds, string expectedString)
        {
            var timeSpan = new TimeSpan(days, hours, minutes, seconds, milliseconds);
            var actualString = timeSpan.ToLogString();
            actualString.ShouldBe(expectedString);
        }
    }
}
