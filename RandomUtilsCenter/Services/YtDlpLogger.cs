using Microsoft.Extensions.Logging;

namespace RandomUtilsCenter.Services
{
	public class YtDlpLogger
	{
		private readonly ILogger<YtDlpService> _logger;

		public YtDlpLogger(ILogger<YtDlpService> logger)
		{
			_logger = logger;
		}

		public void debug(string msg)
		{
			_logger.LogTrace(msg);
		}

		public void warning(string msg)
		{
			_logger.LogWarning(msg);
		}

		public void error(string msg)
		{
			_logger.LogError(msg);
		}
	}
}