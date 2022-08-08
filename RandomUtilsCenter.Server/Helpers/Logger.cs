using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace RandomUtilsCenter.Server.Helpers
{
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