using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Core.Misc.Models;

namespace Nexus.Link.Libraries.Core.Tests.Misc
{
    [TestClass]
    public class CircuitBreakerTests
    {
        private Mock<ICoolDownStrategy> _coolDownStrategyMock;

        [TestInitialize]
        public void Initialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(CircuitBreakerTests));
            _coolDownStrategyMock = new Mock<ICoolDownStrategy>();
            _coolDownStrategyMock
                .Setup(strategy => strategy.Reset());
            _coolDownStrategyMock
                .Setup(strategy => strategy.Next());
        }

        [TestMethod]
        public async Task Handles_Success()
        {
            var circuitBreaker = new CircuitBreaker(_coolDownStrategyMock.Object);
            await UseCircuitBreakerAsync(circuitBreaker, null, () => Task.CompletedTask);
            // No circuit break after success
            await UseCircuitBreakerAsync(circuitBreaker, null, () => Task.CompletedTask);
        }

        [TestMethod]
        public async Task Exception_Is_Rethrown_But_Doesnt_Break()
        {
            var circuitBreaker = new CircuitBreaker(_coolDownStrategyMock.Object);
            var expectedException = new ApplicationException("Fail");
            await UseCircuitBreakerAsync(circuitBreaker, expectedException, () => throw expectedException);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNull(circuitBreaker.FirstFailureAt);
        }

        [TestMethod]
        public async Task InnerException_Is_Rethrown_And_Breaks()
        {
            var circuitBreaker = new CircuitBreaker(_coolDownStrategyMock.Object);
            var expectedException = new ApplicationException("Fail");
            await UseCircuitBreakerAsync(circuitBreaker, expectedException, () => throw new CircuitBreakerException(expectedException));
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(circuitBreaker.FirstFailureAt);
        }

        [TestMethod]
        public async Task Breaks_Circuit_After_Failure()
        {
            var circuitBreaker = new CircuitBreaker(_coolDownStrategyMock.Object);
            
            var expectedException = new ApplicationException("Fail");
            await UseCircuitBreakerAsync(circuitBreaker, expectedException, () => throw new CircuitBreakerException(expectedException));

            // Break circuit
            await UseCircuitBreakerAsync(circuitBreaker, expectedException, () => Task.CompletedTask);
        }

        [TestMethod]
        public async Task Allows_Only_One_Contender_After_Failure()
        {
            var circuitBreaker = new CircuitBreaker(_coolDownStrategyMock.Object);
            
            var expectedException = new ApplicationException("Fail");
            await UseCircuitBreakerAsync(circuitBreaker, expectedException, () => throw new CircuitBreakerException(expectedException));
            _coolDownStrategyMock
                .SetupGet(strategy => strategy.HasCooledDown)
                .Returns(true);

            // Contender
            var task1 = UseCircuitBreakerAsync(circuitBreaker, expectedException, async () =>
            {
                await Task.Delay(10);
            });
            // This one should fail, because contender is running.
            await UseCircuitBreakerAsync(circuitBreaker, expectedException, () => throw new ArgumentException("Should not be thrown."));
            await task1;
        }

        [TestMethod]
        public async Task Breaks_Circuit_Fast_For_Many_Parallell()
        {
        }
        

        private async Task UseCircuitBreakerAsync(CircuitBreaker circuitBreaker, ApplicationException expectedException, Func<Task> actionAsync)
        {
            try
            {
                await circuitBreaker.ExecuteOrThrowAsync(actionAsync);
                if (expectedException != null) {
                    Microsoft.VisualStudio.TestTools.UnitTesting.Assert.Fail($"No exception thrown, expected exception {expectedException.GetType().FullName}.");
                }
            }
            catch (Exception e)
            {
                if (expectedException == null)
                {
                    Microsoft.VisualStudio.TestTools.UnitTesting.Assert.Fail(
                        $"Did not expect an exception, but {e.GetType().FullName} was thrown: {e.Message}");
                }
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsInstanceOfType(e, expectedException.GetType(), $"Expected exception of type {expectedException.GetType().FullName}, but got {e.GetType().FullName}: {e.Message}");
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(expectedException.Message, e.Message, $"Expected message {expectedException.Message}, got {e.Message}");
            }
        }
    }
}