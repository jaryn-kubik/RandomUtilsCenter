namespace Asdf.Clients.AllDebrid
{
	public class AllDebridLoginResponse : AllDebridResponse
	{
		public string pin { get; set; }
		public string user_url { get; set; }
		public string check_url { get; set; }
	}

	public class AllDebridLoginCheckResponse : AllDebridResponse
	{
		public string token { get; set; }
	}
}