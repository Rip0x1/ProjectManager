using System.Text;

namespace ProjectManagementSystem.API.Logging
{
    public sealed class FileLoggerProvider : ILoggerProvider
    {
        private readonly string _directory;
        private readonly object _sync = new();
        private StreamWriter? _writer;
        private string _currentDate = string.Empty;

        public FileLoggerProvider(string directory)
        {
            _directory = directory;
            Directory.CreateDirectory(_directory);
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new FileLogger(categoryName, this);
        }

        internal void Write(string message)
        {
            lock (_sync)
            {
                var date = DateTime.Now.ToString("yyyy-MM-dd");
                if (_writer == null || _currentDate != date)
                {
                    _writer?.Dispose();
                    _currentDate = date;
                    var path = Path.Combine(_directory, $"api-{date}.txt");
                    var stream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                    _writer = new StreamWriter(stream, Encoding.UTF8)
                    {
                        AutoFlush = true
                    };
                }

                _writer.WriteLine(message);
            }
        }

        public void Dispose()
        {
            lock (_sync)
            {
                _writer?.Dispose();
                _writer = null;
            }
        }
    }

    internal sealed class FileLogger : ILogger
    {
        private readonly string _category;
        private readonly FileLoggerProvider _provider;

        public FileLogger(string category, FileLoggerProvider provider)
        {
            _category = category;
            _provider = provider;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return NullScope.Instance;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{logLevel}] {_category}: {formatter(state, exception)}";
            if (exception != null)
            {
                line += Environment.NewLine + exception;
            }

            _provider.Write(line);
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();

            public void Dispose()
            {
            }
        }
    }
}
