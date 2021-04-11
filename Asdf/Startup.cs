using Asdf.Clients.AllDebrid;
using Asdf.Clients.JDownloader;
using Asdf.Clients.Simkl;
using Asdf.Clients.Tmdb;
using Asdf.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

namespace Asdf
{
	public class Startup
	{
		public static void Main(string[] args)
		{
			Host.CreateDefaultBuilder(args)
				.ConfigureLogging(x =>
				{
					x.AddProvider(new LoggerProvider());
					x.SetMinimumLevel(LogLevel.Debug);
					x.AddFilter("System", LogLevel.Information);
					x.AddFilter("Microsoft", LogLevel.Information);
				})
				.ConfigureWebHostDefaults(x =>
				{
					x.UseStartup<Startup>();
					x.ConfigureKestrel(y => y.ListenLocalhost(1337, z => z.UseHttps()));
				})
				.Build()
				.Run();
		}

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddHttpContextAccessor();
			services.AddControllers();
			services.AddRazorPages();
			services.AddServerSideBlazor(x => x.DetailedErrors = true);

			services.AddSingleton(ConfigService.Load());

			services.AddHostedService<JDownloaderService>();
			services.AddScoped<DownloadsService>();
			services.AddHttpClient<JDownloaderClient>(x => x.BaseAddress = new Uri(JDownloaderClient.ApiUrl));

			services.AddScoped<ShowsService>();
			services.AddHttpClient<SimklClient>(x => x.BaseAddress = new Uri(SimklClient.ApiUrl));
			services.AddHttpClient<TmdbClient>(x => x.BaseAddress = new Uri(TmdbClient.ApiUrl));

			services.AddScoped<DebridService>();
			services.AddHttpClient<AllDebridClient>(x => x.BaseAddress = new Uri(AllDebridClient.ApiUrl));

			services.AddSingleton<HtmlWatcherService>();
			services.AddHostedService(x => x.GetRequiredService<HtmlWatcherService>());
		}

		public void Configure(IApplicationBuilder app)
		{
			app.UseDeveloperExceptionPage();
			app.UseStaticFiles();
			app.UseRouting();
			app.UseEndpoints(x =>
			{
				x.MapControllers();
				x.MapBlazorHub();
				x.MapFallbackToPage("/_Host");
			});
		}
	}
}