namespace Asdf.Clients.Simkl
{
	public class SimklAuthResponse : SimklResponse
	{
		public string access_token { get; set; }
		public string token_type { get; set; }
		public string scope { get; set; }
	}
}