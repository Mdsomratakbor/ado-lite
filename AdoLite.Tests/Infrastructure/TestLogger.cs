using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace AdoLite.Tests.Infrastructure
{
    /// <summary>
    /// Simple in-memory logger to assert log output in tests without a real sink.
    /// </summary>
    public class TestLogger<T> : ILogger<T>, IDisposable
    {
        private readonly List<LogEntry> _entries = new();

        public IReadOnlyList<LogEntry> Entries => _entries;

        public IDisposable BeginScope<TState>(TState state) => this;

        public void Dispose()
        {
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            _entries.Add(new LogEntry
            {
                Level = logLevel,
                Message = formatter?.Invoke(state, exception) ?? state?.ToString(),
                Exception = exception
            });
        }

        public class LogEntry
        {
            public LogLevel Level { get; set; }
            public string Message { get; set; }
            public Exception Exception { get; set; }
        }
    }
}
