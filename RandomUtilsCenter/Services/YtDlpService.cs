using Python.Runtime;
using System;
using System.IO;

namespace RandomUtilsCenter.Services
{
	public class YtDlpService
	{
		public YtDlpService()
		{
			var pathBase = Path.GetDirectoryName(typeof(YtDlpService).Assembly.Location);
			var pathPython = Path.Combine(pathBase, "python");
			Runtime.PythonDLL = Path.Combine(pathPython, "python310.dll");
			var path = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process);
			Environment.SetEnvironmentVariable("PATH", $"{pathPython};{path}", EnvironmentVariableTarget.Process);
			PythonEngine.Initialize();

			using (Py.GIL())
			{
				dynamic sys = Py.Import("sys");
				sys.path.append(Path.Combine(pathBase, "yt-dlp"));
			}
		}
	}
}