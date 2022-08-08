using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RandomUtilsCenter.Server.Helpers;
using RandomUtilsCenter.Server.Ping;

namespace RandomUtilsCenter.Server
{
	public static class Startup
	{
		public static void Main(string[] args)
		{
			var app = GetApplication(args);
			app.UseDeveloperExceptionPage();
			app.UseStaticFiles();
			app.MapHub<PingHub>("/Ping");
			app.Run();
		}

		private static WebApplication GetApplication(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			builder.WebHost.ConfigureKestrel(y => y.ListenLocalhost(7332, z => z.UseHttps()));

			builder.Logging.AddProvider(new LoggerProvider());
			builder.Logging.SetMinimumLevel(LogLevel.Debug);
			builder.Logging.AddFilter("Microsoft", LogLevel.Information);
			builder.Logging.AddFilter("System", LogLevel.Information);

			builder.Services.AddHttpContextAccessor();
			builder.Services.AddControllers();
			builder.Services.AddRazorPages();
			builder.Services.AddServerSideBlazor(x => x.DetailedErrors = true);
			ConfigureServices(builder.Services);

			var app = builder.Build();
			//app.Services.GetRequiredService<DownloadsService>();
			return app;
		}

		private static void ConfigureServices(IServiceCollection services)
		{
			services.AddSingleton<PingService>();
			services.AddHostedService(x => x.GetRequiredService<PingService>());

			/*services.AddSingleton<HtmlWatcherService>();
			services.AddHostedService(x => x.GetRequiredService<HtmlWatcherService>());
			
			services.AddSingleton<ClipboardService>();
			services.AddHostedService(x => x.GetRequiredService<ClipboardService>());

			services.AddSingleton<TorrentsService>();
			services.AddSingleton<DebridService>();
			services.AddSingleton<DownloadsService>();

			services.AddSingleton<YtDlpService>();

			services.AddHostedService<InitHostedService>();*/
		}
	}
}