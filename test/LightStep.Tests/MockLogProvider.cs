using System;
using System.Collections.Generic;
using LightStep.Logging;
using LightStep.Logging.LogProviders;

namespace LightStep.Tests
{
    internal class MockLogProvider : ILogProvider
    {
        public Logger GetLogger(string name)
        {
            return Log;
        }

        public IDisposable OpenNestedContext(string message)
        {
            return new DisposableAction(() => { });
        }

        public IDisposable OpenMappedContext(string key, object value, bool destructure = false)
        {
            return new DisposableAction(() => { });
        }

        private static readonly object _lock = new object();

        private static readonly List<Tuple<LogLevel, string, Exception>> Logs
            = new List<Tuple<LogLevel, string, Exception>>();

        private static bool Log(LogLevel logLevel, Func<string> messageFunc, Exception exception,
            params object[] formatParameters)
        {
            string message = null;
            if (messageFunc != null)
            {
                message = messageFunc();
                if (formatParameters != null)
                {
                    message =
                        LogMessageFormatter.FormatStructuredMessage(message,
                            formatParameters,
                            out _);
                }
            }

            if (message != null || exception != null)
            {
                lock (_lock)
                {
                    Logs.Add(Tuple.Create(logLevel, message, exception));
                }
            }

            return true;
        }

        public static Tuple<LogLevel, string, Exception>[] PurgeAndFetchLogs()
        {
            lock (_lock)
            {
                var result = Logs.ToArray();
                Logs.Clear();
                return result;
            }
        }

        public class UseMockLogProviderScope : IDisposable
        {
            private readonly ILogProvider _oldLogProvider;

            public UseMockLogProviderScope()
            {
                _oldLogProvider = LogProvider.CurrentLogProvider;
                LogProvider.SetCurrentLogProvider(new MockLogProvider());
            }

            public void Dispose()
            {
                LogProvider.SetCurrentLogProvider(_oldLogProvider);
            }
        }
    }
}