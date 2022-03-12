using Microsoft.Extensions.Logging;
using Python.Runtime;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace RandomUtilsCenter.Services
{
	public class YtDlpService
	{
		private readonly ILogger<YtDlpService> _logger;

		public YtDlpService(ILogger<YtDlpService> logger)
		{
			_logger = logger;
		}

		public async Task UpdateAsync(CancellationToken cancellationToken)
		{
			var pica = Environment.GetEnvironmentVariable("path");
			await Process.Start("python.exe", "-m pip install -U yt-dlp").WaitForExitAsync(cancellationToken);
		}

		public void Initialize()
		{
			Runtime.PythonDLL = "python310.dll";
			PythonEngine.Initialize();

			var logger = new YtDlpLogger(_logger).ToPython();
			Call<bool>(x => x.Init(logger));
		}

		public void Shutdown()
		{
			PythonEngine.Shutdown();
		}

		private static T Call<T>(Func<dynamic, T> action)
		{
			using (Py.GIL())
			{
				try
				{
					dynamic ytDlp = Py.Import("yt-dlp-wrapper");
					return action(ytDlp);
				}
				catch (Exception ex)
				{
					Utils.ShowMessage("Error - YtDlpService", $"{ex}");
					throw;
				}
			}
		}

		private string GetExtractor(string url) => Call<string>(x => x.GetExtractor(url));
		private int Download(string url) => Call<int>(x => x.Download(url));
	}
}