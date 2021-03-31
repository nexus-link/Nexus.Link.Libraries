using System;
using System.Collections.Concurrent;
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
            await ValidateCircuitBreakerUsageAsync(circuitBreaker, () => Task.CompletedTask);
            // No circuit break after success
            await ValidateCircuitBreakerUsageAsync(circuitBreaker, () => Task.CompletedTask);
        }

        [TestMethod]
        public async Task Exception_Is_Rethrown_But_Doesnt_Break()
        {
            var circuitBreaker = new CircuitBreaker(_coolDownStrategyMock.Object);
            var expectedException = new ApplicationException("Fail");
            await ValidateCircuitBreakerUsageAsync(circuitBreaker, () => throw expectedException, expectedException);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNull(circuitBreaker.FirstFailureAt);
        }

        [TestMethod]
        public async Task InnerException_Is_Rethrown_And_Breaks()
        {
            var circuitBreaker = new CircuitBreaker(_coolDownStrategyMock.Object);
            var expectedException = new ApplicationException("Fail");
            await ValidateCircuitBreakerUsageAsync(circuitBreaker, () => throw new CircuitBreakerException(expectedException), expectedException);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(circuitBreaker.FirstFailureAt);
        }

        [TestMethod]
        public async Task Breaks_Circuit_After_Failure()
        {
            var circuitBreaker = new CircuitBreaker(_coolDownStrategyMock.Object);
            
            var expectedException = new ApplicationException("Fail");
            await ValidateCircuitBreakerUsageAsync(circuitBreaker, () => throw new CircuitBreakerException(expectedException), expectedException);

            // Break circuit
            await ValidateCircuitBreakerUsageAsync(circuitBreaker, () => Task.CompletedTask, expectedException);
        }

        [TestMethod]
        public async Task Allows_Only_One_Contender_After_Failure()
        {
            var circuitBreaker = new CircuitBreaker(_coolDownStrategyMock.Object);
            
            var expectedException = new ApplicationException("Fail");

            // Fail
            await ValidateCircuitBreakerUsageAsync(circuitBreaker, () => throw new CircuitBreakerException(expectedException), expectedException);

            _coolDownStrategyMock
                .SetupGet(strategy => strategy.HasCooledDown)
                .Returns(true);

            // Contender
            var task1 = ValidateCircuitBreakerUsageAsync(circuitBreaker, () => Task.Delay(10));

            // This one should fail, because contender is running.
            await ValidateCircuitBreakerUsageAsync(circuitBreaker, () => throw new ArgumentException("Should not be thrown."), expectedException);
            await task1;
        }

        [TestMethod]
        public async Task No_Contender_While_Others_Are_Executing()
        {
            var circuitBreaker = new CircuitBreaker(_coolDownStrategyMock.Object);
            bool block = true;
            
            var expectedException = new ApplicationException("Fail");

            // Blocking execution
            var task1 = ValidateCircuitBreakerUsageAsync(circuitBreaker, async () =>
            {
                // ReSharper disable once AccessToModifiedClosure
                while (block) await Task.Delay(1);
            });

            // Fail
            await ValidateCircuitBreakerUsageAsync(circuitBreaker, () => throw new CircuitBreakerException(expectedException), expectedException);

            _coolDownStrategyMock
                .SetupGet(strategy => strategy.HasCooledDown)
                .Returns(true);

            // This one should fail, because no contender can run as long as earlier requests are still running.
            await ValidateCircuitBreakerUsageAsync(circuitBreaker, () => Task.Delay(1), expectedException);
            block = false;
            await task1;

            // Allow contender through
            await ValidateCircuitBreakerUsageAsync(circuitBreaker, () => Task.Delay(1));
        }

        [TestMethod]
        public async Task Breaks_Circuit_Fast_For_Many_Parallel()
        {
            var count = 0;
            var circuitBreaker = new CircuitBreaker(_coolDownStrategyMock.Object);
            
            var expectedException = new ApplicationException("Fail");
            await ValidateCircuitBreakerUsageAsync(circuitBreaker, () => throw new CircuitBreakerException(expectedException), expectedException);
            _coolDownStrategyMock
                .SetupGet(strategy => strategy.HasCooledDown)
                .Returns(() => count == 0);

            var tasks = new ConcurrentBag<Task>();

            for (var i = 0; i < 1000; i++)
            {
                var task = ValidateCircuitBreakerUsageAsync(circuitBreaker, async () =>
                {
                    count++;
                    await Task.Delay(10);
                    throw new CircuitBreakerException(expectedException);
                }, expectedException);
                tasks.Add(task);
            }
            
            await Task.WhenAll(tasks);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(1, count);
        }
        

        private async Task ValidateCircuitBreakerUsageAsync(CircuitBreaker circuitBreaker, Func<Task> actionAsync, ApplicationException expectedException = null)
        {
            var exceptionThrown = false;
            try
            {
                await circuitBreaker.ExecuteOrThrowAsync(actionAsync);

            }
            catch (Exception e)
            {
                exceptionThrown = true;
                if (expectedException == null)
                {
                    Microsoft.VisualStudio.TestTools.UnitTesting.Assert.Fail(
                        $"Did not expect an exception, but {e.GetType().FullName} was thrown: {e.Message}");
                }
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsInstanceOfType(e, expectedException.GetType(), $"Expected exception of type {expectedException.GetType().FullName}, but got {e.GetType().FullName}: {e.Message}");
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(expectedException.Message, e.Message, $"Expected message {expectedException.Message}, got {e.Message}");
            }
            if (expectedException != null) {
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(exceptionThrown, $"No exception thrown, expected exception {expectedException.GetType().FullName}.");
            }
        }
    }
}