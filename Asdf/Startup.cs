using Asdf.Clients.AllDebrid;
using Asdf.Clients.Simkl;
using Asdf.Clients.Tmdb;
using Asdf.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using System;

namespace Asdf
{
	public static class Startup
	{
		public static void Main(string[] args)
		{
			var app = GetApplication(args);
			app.UseDeveloperExceptionPage();
			app.UseStaticFiles();
			app.UseRouting();

			app.MapControllers();
			app.MapBlazorHub();
			app.MapFallbackToPage("/_Host");

			app.Run();
		}

		private static WebApplication GetApplication(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			builder.WebHost.ConfigureKestrel(y => y.ListenLocalhost(1337, z => z.UseHttps()));

			builder.Logging.AddProvider(new LoggerProvider());
			builder.Logging.SetMinimumLevel(LogLevel.Debug);
			builder.Logging.AddFilter("Microsoft", LogLevel.Information);
			builder.Logging.AddFilter("System", LogLevel.Information);

			builder.Services.AddHttpContextAccessor();
			builder.Services.AddControllers();
			builder.Services.AddRazorPages();
			builder.Services.AddServerSideBlazor(x => x.DetailedErrors = true);
			ConfigureServices(builder.Services);

			return builder.Build();
		}

		private static void ConfigureServices(IServiceCollection services)
		{
			services.AddMudServices();

			services.AddSingleton(ConfigService.Load());

			services.AddScoped<ShowsService>();
			services.AddHttpClient<SimklClient>(x => x.BaseAddress = new Uri(SimklClient.ApiUrl));
			services.AddHttpClient<TmdbClient>(x => x.BaseAddress = new Uri(TmdbClient.ApiUrl));

			services.AddScoped<DebridService>();
			services.AddHttpClient<AllDebridClient>(x => x.BaseAddress = new Uri(AllDebridClient.ApiUrl));

			services.AddSingleton<HtmlWatcherService>();
			services.AddHostedService(x => x.GetRequiredService<HtmlWatcherService>());

			services.AddSingleton<PingService>();
			services.AddHostedService(x => x.GetRequiredService<PingService>());
		}
	}
}