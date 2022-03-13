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
		private IntPtr _threadState;

		public YtDlpService(ILogger<YtDlpService> logger, ClipboardService clipboard)
		{
			_logger = logger;
			clipboard.Register(OnClipboardAsync);
		}

		private Task OnClipboardAsync(string text)
		{
			text = text?.Trim() ?? string.Empty;
			if (text.StartsWith("https://"))
			{
				var extractor = GetExtractor(text);
				if (extractor != "Generic")
				{
					Utils.ShowToast(extractor, text);
					Download(text);
				}
			}
			return Task.CompletedTask;
		}

		public async Task UpdateAsync(CancellationToken cancellationToken)
		{
			await Process.Start("python.exe", "-m pip install -U yt-dlp").WaitForExitAsync(cancellationToken);
		}

		public void Initialize()
		{
			Runtime.PythonDLL = "python310.dll";
			PythonEngine.Initialize();
			_threadState = PythonEngine.BeginAllowThreads();

			Call<bool>(x => x.Init(new YtDlpLogger(_logger).ToPython()));
		}

		public void Shutdown()
		{
			PythonEngine.EndAllowThreads(_threadState);
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

		private static string GetExtractor(string url) => Call<string>(x => x.GetExtractor(url));
		private static int Download(string url) => Call<int>(x => x.Download(url));
	}
}