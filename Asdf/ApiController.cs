using Asdf.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Asdf
{
	[ApiController, Route("[controller]/[action]")]
	public class ApiController : ControllerBase
	{
		public const string Name = "Api";

		private readonly ShowsService _showsService;

		public ApiController(ShowsService showsService)
		{
			_showsService = showsService;
		}

		[HttpGet]
		public async Task<RedirectResult> Simkl(string code)
		{
			await _showsService.LoginAsync(code);
			return Redirect("~/");
		}
	}
}