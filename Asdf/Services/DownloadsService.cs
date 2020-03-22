using System.Collections.Generic;

namespace Asdf.Services
{
	public class DownloadsService
	{
		//private readonly JDownloaderHandler _jdownloaderHandler = new JDownloaderHandler("ad9ae644-3f37-44ad-aca4-f83e833b3a42");
		private readonly ConfigService _config;

		public DownloadsService(ConfigService config)
		{
			_config = config;
		}

		public bool Login()
		{
			if (_config.JDownloaderUserName == null || _config.JDownloaderPassword == null)
			{
				return false;
			}
			return true;
		}

		public bool Login(string userName, string password)
		{
			_config.JDownloaderUserName = userName;
			_config.JDownloaderPassword = password;
			_config.Save();
			return Login();
		}

		public IEnumerable<DownloadModel> GetItems()
		{
			yield break;
		}
	}
}