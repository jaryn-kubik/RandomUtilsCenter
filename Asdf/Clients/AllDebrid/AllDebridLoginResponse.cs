namespace Asdf.Clients.AllDebrid
{
	public class AllDebridLoginResponse
	{
		public string pin { get; set; }
		public string user_url { get; set; }
		public string check_url { get; set; }
	}

	public class AllDebridLoginCheckResponse
	{
		public string apikey { get; set; }
	}
}