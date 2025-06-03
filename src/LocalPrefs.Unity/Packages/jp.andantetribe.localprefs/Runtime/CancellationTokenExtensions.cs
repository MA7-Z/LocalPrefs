#nullable enable

using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace AndanteTribe.IO.Unity
{
    internal static class CancellationTokenExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CancellationTokenRegistration RegisterWithoutCaptureExecutionContext(in this CancellationToken cancellationToken, Action callback)
        {
            var restoreFlow = false;
            if (!ExecutionContext.IsFlowSuppressed())
            {
                ExecutionContext.SuppressFlow();
                restoreFlow = true;
            }

            try
            {
                return cancellationToken.Register(callback, false);
            }
            finally
            {
                if (restoreFlow)
                {
                    ExecutionContext.RestoreFlow();
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CancellationTokenRegistration RegisterWithoutCaptureExecutionContext(in this CancellationToken cancellationToken, Action<object> callback, object state)
        {
            var restoreFlow = false;
            if (!ExecutionContext.IsFlowSuppressed())
            {
                ExecutionContext.SuppressFlow();
                restoreFlow = true;
            }

            try
            {
                return cancellationToken.Register(callback, state, false);
            }
            finally
            {
                if (restoreFlow)
                {
                    ExecutionContext.RestoreFlow();
                }
            }
        }
    }
}