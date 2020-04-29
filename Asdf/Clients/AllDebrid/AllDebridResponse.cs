namespace Asdf.Clients.AllDebrid
{
	public class AllDebridResponse<T>
	{
		public string status { get; set; }
		public AllDebridError error { get; set; }
		public T data { get; set; }
	}

	public class AllDebridError
	{
		public string code { get; set; }
		public string message { get; set; }
	}
}