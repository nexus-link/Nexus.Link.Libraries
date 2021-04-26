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
        public async Task Handles_Success()
        {
            foreach (var circuitBreaker in _circuitBreakersUnderTest)
            {
                await ValidateCircuitBreakerUsageAsync(circuitBreaker, (t) => Task.CompletedTask);
                // No circuit break after success
                await ValidateCircuitBreakerUsageAsync(circuitBreaker, (t) => Task.CompletedTask);
            }
        }

        [TestMethod]
        public async Task Exception_Is_Rethrown_But_Doesnt_Break()
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
        public async Task InnerException_Is_Rethrown_And_Breaks()
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
        public async Task Breaks_Circuit_After_Failure()
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
        public async Task No_Contender_During_CoolDown()
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
                await ValidateCircuitBreakerUsageAsync(circuitBreaker, (t) => Task.Delay(1, t), expectedException);
            }
        }

        [TestMethod]
        public async Task Parallel_Success_Followed_By_Fail_Overrides_CoolDown()
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

        [TestMethod]
        public async Task Parallel_Fail_Followed_By_Success_Deactivates_Circuit_Break()
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

                // Trigger fail followed by success
                blockFail = false;
                await task2;
                blockSuccess = false;
                await task1;

                // All should execute in parallel, as we ended with a success
                var block1 = true;
                var task3 = ValidateCircuitBreakerUsageAsync(circuitBreaker, async (t) =>
                {
                    // ReSharper disable once AccessToModifiedClosure
                    while (block1) await Task.Delay(1, t);
                });
                await ValidateCircuitBreakerUsageAsync(circuitBreaker, (t) => Task.Delay(1, t));
                block1 = false;
                await task3;
            }
        }

        [TestMethod]
        public async Task Allows_Only_One_Contender_After_Failure()
        {
            foreach (var circuitBreaker in _circuitBreakersUnderTest)
            {

                var expectedException = new ApplicationException("Fail");

                // Fail
                await ValidateCircuitBreakerUsageAsync(circuitBreaker,
                    (t) => throw new CircuitBreakerException(expectedException), expectedException);

                _coolDownStrategyMock
                    .SetupGet(strategy => strategy.HasCooledDown)
                    .Returns(true);

                // Contender
                var block = true;
                var task1 = ValidateCircuitBreakerUsageAsync(circuitBreaker, async (t) =>
                {
                    // ReSharper disable once AccessToModifiedClosure
                    while (block) await Task.Delay(1, t);
                });

                // This one should fail, because contender is running.
                await ValidateCircuitBreakerUsageAsync(circuitBreaker,
                    (t) => throw new ArgumentException("Should not be thrown."), expectedException);
                block = false;
                await task1;
            }
        }

        [TestMethod]
        public async Task No_Contender_While_Others_Are_Executing()
        {
            foreach (var circuitBreaker in _circuitBreakersUnderTest)
            {
                bool block = true;

                var expectedException = new ApplicationException("Fail");

                // Blocking execution
                var task1 = ValidateCircuitBreakerUsageAsync(circuitBreaker, async (t) =>
                {
                    // ReSharper disable once AccessToModifiedClosure
                    while (block) await Task.Delay(1, t);
                });

                // Fail
                await ValidateCircuitBreakerUsageAsync(circuitBreaker,
                    (t) => throw new CircuitBreakerException(expectedException), expectedException);

                _coolDownStrategyMock
                    .SetupGet(strategy => strategy.HasCooledDown)
                    .Returns(true);

                // This one should fail, because no contender can run as long as earlier requests are still running.
                await ValidateCircuitBreakerUsageAsync(circuitBreaker, (t) => Task.Delay(1, t), expectedException);
                block = false;
                await task1;

                // Allow contender through
                await ValidateCircuitBreakerUsageAsync(circuitBreaker, (t) => Task.Delay(1, t));
            }
        }

        [TestMethod]
        public async Task Breaks_Circuit_Fast_For_Many_Parallel()
        {
            foreach (var circuitBreaker in _circuitBreakersUnderTest)
            {
                var count = 0;

                var expectedException = new ApplicationException("Fail");
                await ValidateCircuitBreakerUsageAsync(circuitBreaker,
                    (t) => throw new CircuitBreakerException(expectedException), expectedException);
                _coolDownStrategyMock
                    .SetupGet(strategy => strategy.HasCooledDown)
                    .Returns(() => count == 0);

                var tasks = new ConcurrentBag<Task>();

                var block = true;
                // Massive parallel calling "attack", only one contender should be let through
                for (var i = 0; i < 1000; i++)
                {
                    var task = ValidateCircuitBreakerUsageAsync(circuitBreaker, async (t) =>
                    {
                        count++;
                        // ReSharper disable once AccessToModifiedClosure
                        while (block) await Task.Delay(1, t);
                        throw new CircuitBreakerException(expectedException);
                    }, expectedException);
                    tasks.Add(task);
                }

                block = false;
                await Task.WhenAll(tasks);
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(1, count);
            }
        }
        

        private async Task ValidateCircuitBreakerUsageAsync(ICircuitBreaker circuitBreaker, Func<CancellationToken, Task> actionAsync, ApplicationException expectedException = null, CancellationToken cancellationToken = default)
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
    }
}