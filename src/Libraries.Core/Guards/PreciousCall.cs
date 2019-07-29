using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Misc;

namespace Nexus.Link.Libraries.Core.Guards
{
    [SuppressMessage("ReSharper", "ExplicitCallerInfoArgument")]
    public class PreciousCall
    {
        private readonly CodeLocation _caller;
        private CodeLocation _me;

        [SuppressMessage("ReSharper", "ExplicitCallerInfoArgument")]
        public PreciousCall(CodeLocation caller,
            [CallerLineNumber] int lineNumber = 0,
            [CallerFilePath] string filePath = "",
            [CallerMemberName] string memberName = "")
        {
            _caller = caller;
            _me = new CodeLocation(memberName, filePath, lineNumber);
        }

        public TValue Trace<TGuard, TValue>(TGuard guard, Func<TGuard, TValue> func)
        {
            var hasException = false;
            TValue result = default;
            try
            {
                // TODO: Trace entering the method
                return result = func(guard);
            }
            catch (Exception e)
            {
                hasException = true;
                LogException(e);
                throw;
            }
            finally
            {
                if (!hasException) LogResult(result);
            }
        }

        private void LogResult<T>(T result)
        {
            Log.LogVerbose($"Returning result: {result}", null, _me.MemberName, _me.FilePath, _me.LineNumber);
        }

        private void LogException(Exception e)
        {
            switch (e)
            {
                case null:
                    return;
                case FulcrumException fulcrumException when fulcrumException.HasBeenLogged:
                    Log.LogError($"{_me.MemberName} failed with exception {fulcrumException.GetType().Name}: {e.Message}", e, _me.MemberName, _me.FilePath, _me.LineNumber);
                    break;
            }

            Log.LogError($"{_me.MemberName} failed with exception {e.GetType().FullName}: {e.Message}", e, _me.MemberName, _me.FilePath, _me.LineNumber);
        }
    }
}
