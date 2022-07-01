using System;
using System.Linq;
using System.Threading.Tasks;

namespace Nexus.Link.Libraries.Core.Misc
{
    /// <summary>
    /// A helper class to do some common things with delegates.
    /// </summary>
    public class DelegateHelper
    {
        private readonly Delegate _delegateMethods;

        public DelegateHelper(Delegate delegateMethods)
        {
            _delegateMethods = delegateMethods;
        }

        /// <summary>
        /// Execute the delegates one by one.
        /// </summary>
        /// <param name="swallowExceptions">true means that all exceptions will be silently swallowed</param>
        /// <param name="arguments">The arguments that are passed to the delegate methods.</param>
        public void ExecuteDelegatesSequentially(bool swallowExceptions, params object[] arguments)
        {
            foreach (var delegateMethod in _delegateMethods.GetInvocationList())
            {
                try
                {
                    delegateMethod.DynamicInvoke(arguments);
                }
                catch (Exception)
                {
                    if (!swallowExceptions) throw;
                }
            }
        }

        /// <summary>
        /// Execute the delegates one by one.
        /// </summary>
        /// <param name="swallowExceptions">true means that all exceptions will be silently swallowed</param>
        /// <param name="arguments">The arguments that are passed to the delegate methods.</param>
        public async Task ExecuteDelegatesSequentiallyAsync(bool swallowExceptions, params object[] arguments)
        {
            foreach (var delegateMethod in _delegateMethods.GetInvocationList())
            {
                try
                {
                    await (Task)delegateMethod.DynamicInvoke(arguments);
                }
                catch (Exception)
                {
                    if (!swallowExceptions) throw;
                }
            }
        }

        /// <summary>
        /// Execute the delegates concurrently.
        /// </summary>
        /// <param name="swallowExceptions">true means that all exceptions will be silently swallowed</param>
        /// <param name="arguments">The arguments that are passed to the delegate methods.</param>
        public async Task ExecuteDelegatesConcurrentlyAsync(bool swallowExceptions, params object[] arguments)
        {
            var tasks = _delegateMethods.GetInvocationList()
                .Select(delegateMethod => (Task) delegateMethod.DynamicInvoke(arguments));
            try
            {
                await Task.WhenAll(tasks);
            }
            catch (Exception)
            {
                if (!swallowExceptions) throw;
            }
        }

        /// <summary>
        /// Execute the delegates one by one until one of them returns true.
        /// </summary>
        /// <param name="swallowExceptions">true means that all exceptions will be silently swallowed</param>
        /// <param name="arguments">The arguments that are passed to the delegate methods.</param>
        public bool ExecuteUntilTrue(bool swallowExceptions, params object[] arguments)
        {
            return ExecuteUntilTrue(swallowExceptions, b => b, false, arguments);
        }

        /// <summary>
        /// Execute the delegates one by one until the <paramref name="breakCondition"/> is met.
        /// </summary>
        /// <param name="swallowExceptions">true means that all exceptions will be silently swallowed</param>
        /// <param name="arguments">The arguments that are passed to the delegate methods.</param>
        /// <param name="resultWhenBreakConditionWasNotMet">If no method result meets the break condition, this value is returned.</param>
        /// <param name="breakCondition">If this method returns true, no more delegates will be called</param>
        /// <returns>The value that met the <paramref name="breakCondition"/>, or <paramref name="resultWhenBreakConditionWasNotMet"/>
        /// if no method meets the condition.</returns>
        public T ExecuteUntilTrue<T>(bool swallowExceptions, Func<T, bool> breakCondition, T resultWhenBreakConditionWasNotMet, params object[] arguments)
        {
            foreach (var delegateMethod in _delegateMethods.GetInvocationList())
            {
                try
                {
                    var result = (T)delegateMethod.DynamicInvoke(arguments);
                    if (breakCondition(result)) return result;
                }
                catch (Exception)
                {
                    if (!swallowExceptions) throw;
                }
            }

            return resultWhenBreakConditionWasNotMet;
        }

        /// <summary>
        /// Execute the delegates one by one until one of them returns true.
        /// </summary>
        /// <param name="swallowExceptions">true means that all exceptions will be silently swallowed</param>
        /// <param name="arguments">The arguments that are passed to the delegate methods.</param>
        public Task<bool> ExecuteSomeDelegatesAsync(bool swallowExceptions, params object[] arguments)
        {
            return ExecuteSomeDelegatesAsync(swallowExceptions, b => b, false, arguments);
        }
        

        /// <summary>
        /// Execute the delegates one by one until the <paramref name="breakCondition"/> is met.
        /// </summary>
        /// <param name="swallowExceptions">true means that all exceptions will be silently swallowed</param>
        /// <param name="arguments">The arguments that are passed to the delegate methods.</param>
        /// <param name="resultWhenBreakConditionWasNotMet">If no method result meets the break condition, this value is returned.</param>
        /// <param name="breakCondition">If this method returns true, no more delegates will be called</param>
        /// <returns>The value that met the <paramref name="breakCondition"/>, or <paramref name="resultWhenBreakConditionWasNotMet"/>
        /// if no method meets the condition.</returns>
        public async Task<T> ExecuteSomeDelegatesAsync<T>(bool swallowExceptions, Func<T, bool> breakCondition, T resultWhenBreakConditionWasNotMet, params object[] arguments)
        {
            foreach (var delegateMethod in _delegateMethods.GetInvocationList())
            {
                try
                {
                    var result = await (Task<T>)delegateMethod.DynamicInvoke(arguments);
                    if (breakCondition(result)) return result;
                }
                catch (Exception)
                {
                    if (!swallowExceptions) throw;
                }
            }

            return resultWhenBreakConditionWasNotMet;
        }
    }
}