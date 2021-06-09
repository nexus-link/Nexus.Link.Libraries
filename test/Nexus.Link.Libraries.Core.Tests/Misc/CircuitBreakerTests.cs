using System;
using System.Collections.Concurrent;
using System.Threading;
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
        private ICircuitBreaker[] _circuitBreakersUnderTest;

        [TestInitialize]
        public void Initialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(CircuitBreakerTests));
            _coolDownStrategyMock = new Mock<ICoolDownStrategy>();
            _coolDownStrategyMock
                .Setup(strategy => strategy.Reset());
            _coolDownStrategyMock
                .Setup(strategy => strategy.StartNextCoolDownPeriod());
            var circuitBreakerOptions = new CircuitBreakerWithThrottlingOptions
            {
                CoolDownStrategy = _coolDownStrategyMock.Object,
                CancelConcurrentRequestsWhenOneFails = false,
                ThrottlingCoolDownStrategy = _coolDownStrategyMock.Object,
                ConcurrencyThresholdForChokingResolved = 100
            };
            _circuitBreakersUnderTest = new ICircuitBreaker[]
            {
                new CircuitBreaker(circuitBreakerOptions),
                new CircuitBreakerWithThrottling(circuitBreakerOptions)
            };
        }

        [TestMethod]
        public async Task Handles_Success_Async()
        {
            foreach (var circuitBreaker in _circuitBreakersUnderTest)
            {
                await ValidateCircuitBreakerUsageAsync(circuitBreaker, (t) => Task.CompletedTask);
                // No circuit break after success
                await ValidateCircuitBreakerUsageAsync(circuitBreaker, (t) => Task.CompletedTask);
            }
        }

        [TestMethod]
        public void Handles_Success_Sync()
        {
            foreach (var circuitBreaker in _circuitBreakersUnderTest)
            {
                ValidateCircuitBreakerUsage(circuitBreaker, () => { });
                // No circuit break after success
                ValidateCircuitBreakerUsage(circuitBreaker, () => { });
            }
        }

        [TestMethod]
        public async Task Exception_Is_Rethrown_But_Doesnt_Break_Async()
        {
            foreach (var circuitBreaker in _circuitBreakersUnderTest)
            {
                var expectedException = new ApplicationException("Fail");
                await ValidateCircuitBreakerUsageAsync(circuitBreaker, (t) => throw expectedException,
                    expectedException);
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNull(circuitBreaker.FirstFailureAt);
            }
        }

        [TestMethod]
        public void Exception_Is_Rethrown_But_Doesnt_Break_Sync()
        {
            foreach (var circuitBreaker in _circuitBreakersUnderTest)
            {
                var expectedException = new ApplicationException("Fail");
                ValidateCircuitBreakerUsage(circuitBreaker, () => throw expectedException,
                    expectedException);
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNull(circuitBreaker.FirstFailureAt);
            }
        }

        [TestMethod]
        public async Task InnerException_Is_Rethrown_And_Breaks_Async()
        {
            foreach (var circuitBreaker in _circuitBreakersUnderTest)
            {
                var expectedException = new ApplicationException("Fail");
                await ValidateCircuitBreakerUsageAsync(circuitBreaker,
                    (t) => throw new CircuitBreakerException(expectedException), expectedException);
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(circuitBreaker.FirstFailureAt);
            }
        }

        [TestMethod]
        public void InnerException_Is_Rethrown_And_Breaks_Sync()
        {
            foreach (var circuitBreaker in _circuitBreakersUnderTest)
            {
                var expectedException = new ApplicationException("Fail");
                ValidateCircuitBreakerUsage(circuitBreaker,
                    () => throw new CircuitBreakerException(expectedException), expectedException);
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(circuitBreaker.FirstFailureAt);
            }
        }

        [TestMethod]
        public async Task Breaks_Circuit_After_Failure_Async()
        {
            foreach (var circuitBreaker in _circuitBreakersUnderTest)
            {

                var expectedException = new ApplicationException("Fail");
                await ValidateCircuitBreakerUsageAsync(circuitBreaker,
                    (t) => throw new CircuitBreakerException(expectedException), expectedException);

                // Break circuit
                await ValidateCircuitBreakerUsageAsync(circuitBreaker, (t) => Task.CompletedTask, expectedException);
            }
        }

        [TestMethod]
        public void Breaks_Circuit_After_Failure_Sync()
        {
            foreach (var circuitBreaker in _circuitBreakersUnderTest)
            {

                var expectedException = new ApplicationException("Fail");
                ValidateCircuitBreakerUsage(circuitBreaker,
                    () => throw new CircuitBreakerException(expectedException), expectedException);

                // Break circuit
                ValidateCircuitBreakerUsage(circuitBreaker, () => { }, expectedException);
            }
        }

        [TestMethod]
        public async Task No_Contender_During_CoolDown_Async()
        {
            foreach (var circuitBreaker in _circuitBreakersUnderTest)
            {

                var expectedException = new ApplicationException("Fail");

                // Fail
                await ValidateCircuitBreakerUsageAsync(circuitBreaker,
                    (t) => throw new CircuitBreakerException(expectedException), expectedException);

                _coolDownStrategyMock
                    .SetupGet(strategy => strategy.HasCooledDown)
                    .Returns(false);

                // Contender should not execute, due to cool down
                await ValidateCircuitBreakerUsageAsync(circuitBreaker, (t) => throw new ApplicationException("This should not be executed"), expectedException);
            }
        }

        [TestMethod]
        public void No_Contender_During_CoolDown_Sync()
        {
            foreach (var circuitBreaker in _circuitBreakersUnderTest)
            {

                var expectedException = new ApplicationException("Fail");

                // Fail
                ValidateCircuitBreakerUsage(circuitBreaker,
                    () => throw new CircuitBreakerException(expectedException), expectedException);

                _coolDownStrategyMock
                    .SetupGet(strategy => strategy.HasCooledDown)
                    .Returns(false);

                // Contender should not execute, due to cool down
                ValidateCircuitBreakerUsage(circuitBreaker, () => throw new ApplicationException("This should not be executed"), expectedException);
            }
        }

        [TestMethod]
        public async Task Parallel_Success_Followed_By_Fail_Overrides_CoolDown_Async()
        {
            foreach (var circuitBreaker in _circuitBreakersUnderTest)
            {

                var expectedException1 = new ApplicationException("Fail 1");

                var expectedException2 = new ApplicationException("Fail 2");

                // Parallel success
                var blockSuccess = true;
                var task1 = ValidateCircuitBreakerUsageAsync(circuitBreaker, async (t) =>
                {
                    // ReSharper disable once AccessToModifiedClosure
                    while (blockSuccess) await Task.Delay(1, t);
                });

                // Parallel fail
                var blockFail = true;
                var task2 = ValidateCircuitBreakerUsageAsync(circuitBreaker, async (t) =>
                {
                    // ReSharper disable once AccessToModifiedClosure
                    while (blockFail) await Task.Delay(1, t);
                    throw new CircuitBreakerException(expectedException2);
                }, expectedException2);

                // Fail
                await ValidateCircuitBreakerUsageAsync(circuitBreaker,
                    (t) => throw new CircuitBreakerException(expectedException1), expectedException1);

                // Trigger success followed by fail
                blockSuccess = false;
                await task1;
                blockFail = false;
                await task2;

                _coolDownStrategyMock
                    .SetupGet(strategy => strategy.HasCooledDown)
                    .Returns(false);

                // Contender should execute, due to cool down is ignored (because of concurrent request succeeded)
                var blockContender = true;
                var task3 = ValidateCircuitBreakerUsageAsync(circuitBreaker, async (t) =>
                {
                    // ReSharper disable once AccessToModifiedClosure
                    while (blockContender) await Task.Delay(1, t);
                });

                // This one should fail, because contender is running.
                await ValidateCircuitBreakerUsageAsync(circuitBreaker,
                    (t) => throw new ArgumentException("Should not be thrown."), expectedException2);
                blockContender = false;
                await task3;
            }
        }
        
        private async Task ValidateCircuitBreakerUsageAsync(ICircuitBreaker circuitBreaker, Func<CancellationToken, Task> actionAsync, Exception expectedException = null, CancellationToken cancellationToken = default)
        {
            var exceptionThrown = false;
            try
            {
                await circuitBreaker.ExecuteOrThrowAsync(actionAsync, cancellationToken);
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
        
        private void ValidateCircuitBreakerUsage(ICircuitBreaker circuitBreaker, Action action, Exception expectedException = null)
        {
            var exceptionThrown = false;
            try
            {
                circuitBreaker.ExecuteOrThrow(action);
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