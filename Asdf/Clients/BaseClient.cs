using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Asdf.Clients
{
	public abstract class BaseClient
	{
		private readonly HttpClient _client;

		protected BaseClient(HttpClient client)
		{
			_client = client;
		}

		protected HttpRequestHeaders Headers => _client.DefaultRequestHeaders;

		protected Task<HttpResponseMessage> GetAsync(string url)
		{
			return _client.GetAsync(url);
		}

		protected Task<HttpResponseMessage> PostAsync(string url)
		{
			return _client.PostAsync(url, null);
		}

		protected Task<HttpResponseMessage> PostJsonAsync<T>(string url, T data)
		{
			var serialized = JsonSerializer.Serialize(data);
			var content = new StringContent(serialized, Encoding.UTF8, MediaTypeNames.Application.Json);
			return _client.PostAsync(url, content);
		}

		protected async Task<T> ReadAsJsonAsync<T>(HttpResponseMessage response)
		{
			response.EnsureSuccessStatusCode();
			var data = await response.Content.ReadAsStringAsync();
			return JsonSerializer.Deserialize<T>(data);
		}
	}
}