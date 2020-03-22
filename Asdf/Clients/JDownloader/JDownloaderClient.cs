using Asdf.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Asdf.Clients.JDownloader
{
	public class JDownloaderClient : BaseClient
	{
		public const string ApiUrl = "https://api.jdownloader.org";

		private readonly ConfigService _config;
		private readonly JDownloaderState _state = new JDownloaderState();

		public JDownloaderClient(HttpClient client, ConfigService config) : base(client)
		{
			_config = config;
		}

		public async Task ConnectAsync()
		{
			_state.SetSecrets(_config.JDownloaderUserName, _config.JDownloaderPassword);

			var connect = await GetAsync<JDownloaderConnectResponse>($"/my/connect?email={_config.JDownloaderUserName}&appkey={_config.JDownloaderUserName}", _state.LoginSecret);
			_state.SetTokens(connect.sessiontoken, connect.regaintoken);

			var devices = await GetAsync<JDownloaderDevicesResponse>($"/my/listdevices?sessiontoken={_state.SessionToken}", _state.ServerEncryptionToken);
			_state.DeviceId = devices.list.First().id;

			var connections = await PostAsync<JDownloaderDeviceConnectionsResponse>("/device/getDirectConnectionInfos", _state.DeviceEncryptionToken);
			var connection = connections.infos.First();
			_state.ConnectionUrl = $"http://{connection.ip}:{connection.port}";
		}

		public Task GetPackagesAsync()
		{
			return PostAsync<IEnumerable<JDownloaderPackagesResponse>>("/downloadsV2/queryPackages", _state.DeviceEncryptionToken, new JDownloaderPackagesRequest());
		}

		public Task GetLinksAsync()
		{
			return PostAsync<IEnumerable<JDownloaderLinksResponse>>("/downloadsV2/queryLinks", _state.DeviceEncryptionToken, new JDownloaderLinksRequest());
		}

		private async Task<TResponse> PostAsync<TResponse>(string action, byte[] secret, object param = null)
		{
			var apiUrl = _state.ConnectionUrl ?? ApiUrl;
			var url = $"{apiUrl}/t_{_state.SessionToken}_{_state.DeviceId}{action}";

			var request = JsonSerializer.Serialize(new
			{
				apiVer = 1,
				rid = _state.RequestId,
				url = action,
				@params = param == null ? null : new[] { JsonSerializer.Serialize(param) }
			});
			var encrypted = Encrypt(request, secret);

			var response = await PostAsync(url, encrypted, "application/aesjson-jd");
			var data = await response.Content.ReadAsStringAsync();
			var decrypted = Decrypt(data, secret);
			if (!response.IsSuccessStatusCode)
			{
				var error = JsonSerializer.Deserialize<JDownloaderErrorResponse>(decrypted);
				throw new Exception($"{error.src}: {error.type} - {error.data}");
			}

			var result = JsonSerializer.Deserialize<JDownloaderResponse<TResponse>>(decrypted);
			return result.data;
		}

		private async Task<TResponse> GetAsync<TResponse>(string action, byte[] secret)
		{
			var url = ApiUrl + SignUrl($"{action}&rid={_state.RequestId}", secret);

			var response = await GetAsync(url);
			var data = await response.Content.ReadAsStringAsync();
			if (!response.IsSuccessStatusCode)
			{
				var error = JsonSerializer.Deserialize<JDownloaderErrorResponse>(data);
				throw new Exception($"{error.src}: {error.type} - {error.data}");
			}

			var decrypted = Decrypt(data, secret);
			return JsonSerializer.Deserialize<TResponse>(decrypted);
		}

		private string SignUrl(string url, byte[] key)
		{
			using var sha = new HMACSHA256(key);
			var bytes = Encoding.UTF8.GetBytes(url);
			var hash = sha.ComputeHash(bytes);
			var signature = BitConverter.ToString(hash).Replace("-", string.Empty);
			return $"{url}&signature={signature}";
		}

		private string Decrypt(string value, byte[] secret)
		{
			using var aes = Rijndael.Create();
			aes.Mode = CipherMode.CBC;
			aes.BlockSize = 128;

			using var decryptor = aes.CreateDecryptor(secret[16..], secret[..16]);
			using var ms = new MemoryStream(Convert.FromBase64String(value));
			using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
			using var sr = new StreamReader(cs);
			return sr.ReadToEnd();
		}

		private string Encrypt(string value, byte[] secret)
		{
			using var aes = Rijndael.Create();
			aes.Mode = CipherMode.CBC;
			aes.BlockSize = 128;

			using var encryptor = aes.CreateEncryptor(secret[16..], secret[..16]);
			using var ms = new MemoryStream();
			using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
			using (var sr = new StreamWriter(cs))
			{
				sr.Write(value);
			}
			return Convert.ToBase64String(ms.ToArray());
		}
	}
}