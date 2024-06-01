using Microsoft.Extensions.Logging;
using Python.Runtime;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RandomUtilsCenter.Services
{
    public class YtDlpService
    {
        private readonly ILogger<YtDlpService> _logger;
        private readonly CircuitsService _circuitsService;
        private readonly IntPtr _threadState;

        public YtDlpService(ILogger<YtDlpService> logger, CircuitsService circuitsService, ClipboardService clipboard)
        {
            _logger = logger;
            _circuitsService = circuitsService;
            clipboard.Register(OnClipboardAsync);
        }

        private Task OnClipboardAsync(string text)
        {
            /*if (_circuitsService.Any && text.StartsWith("https://"))
			{
				var extractor = GetExtractor(text);
				if (extractor != "Generic")
				{
					Utils.ShowToast(extractor, text);
					Download(text);
				}
			}*/
            return Task.CompletedTask;
        }

        public async Task UpdateAsync(CancellationToken cancellationToken)
        {
            /*var startInfo = new ProcessStartInfo("python.exe", "-m pip install -U yt-dlp") { CreateNoWindow = true };
			await Process.Start(startInfo).WaitForExitAsync(cancellationToken);*/
        }

        public void Initialize()
        {
            /*Runtime.PythonDLL = "python310.dll";
			PythonEngine.Initialize();
			_threadState = PythonEngine.BeginAllowThreads();

			Call<bool>(x => x.Init(new YtDlpLogger(_logger).ToPython()));*/
        }

        public void Shutdown()
        {
            /*PythonEngine.EndAllowThreads(_threadState);
			PythonEngine.Shutdown();*/
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