using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Asdf
{
	public static class Extensions
	{
		public static Task<HttpResponseMessage> PostJsonAsync<T>(this HttpClient client, string url, T data)
		{
			return client.PostAsync(url, GetContent(data));
		}

		public static async Task<T> ReadAsJsonAsync<T>(this HttpResponseMessage response)
		{
			response.EnsureSuccessStatusCode();
			var data = await response.Content.ReadAsStringAsync();
			return JsonSerializer.Deserialize<T>(data);
		}

		private static HttpContent GetContent<T>(T data)
		{
			var serialized = JsonSerializer.Serialize(data);
			return new StringContent(serialized, Encoding.UTF8, MediaTypeNames.Application.Json);
		}
	}
}