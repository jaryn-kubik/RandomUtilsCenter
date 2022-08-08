using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.IO;

namespace RandomUtilsCenter.Server.Helpers
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
}