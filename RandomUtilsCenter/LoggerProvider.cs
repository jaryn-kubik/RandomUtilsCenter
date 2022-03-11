using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.IO;

namespace RandomUtilsCenter
{
	public class LoggerProvider : ILoggerProvider
	{
		private readonly ConcurrentDictionary<string, Logger> _loggers = new ConcurrentDictionary<string, Logger>();
		private readonly object _syncRoot = new object();
		private StreamWriter _stream;

		public LoggerProvider()
		{
			var path = Path.Combine(Path.GetDirectoryName(typeof(Startup).Assembly.Location), ".log.txt");
			if (File.Exists(path) && new FileInfo(path).Length > 10 * 1000 * 1000)
			{
				File.Delete(path);
			}
			_stream = new StreamWriter(path, true) { AutoFlush = true };
		}

		public ILogger CreateLogger(string categoryName)
		{
			return _loggers.GetOrAdd(categoryName, x => new Logger(_syncRoot, x, _stream));
		}

		public void Dispose()
		{
			foreach (var logger in _loggers)
			{
				logger.Value.Dispose();
			}
			_loggers.Clear();
			_stream?.Dispose();
			_stream = null;
		}
	}

	public class Logger : ILogger, IDisposable
	{
		private readonly object _syncRoot;
		private readonly string _categoryName;
		private StreamWriter _stream;

		public Logger(object syncRoot, string categoryName, StreamWriter stream)
		{
			_syncRoot = syncRoot;
			_categoryName = categoryName;
			_stream = stream;
		}

		public void Dispose()
		{
			_stream = null;
		}

		public IDisposable BeginScope<TState>(TState state)
		{
			return null;
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			return true;
		}

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			lock (_syncRoot)
			{
				var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
				var level = logLevel.ToString().ToUpperInvariant().PadRight(11);
				var category = _categoryName.Substring(Math.Max(0, _categoryName.Length - 60)).PadRight(60);
				var msg = formatter(state, exception);
				_stream.WriteLine("{0}  {1}  [{2}] {3}", timestamp, level, category, msg);
			}
		}
	}
}