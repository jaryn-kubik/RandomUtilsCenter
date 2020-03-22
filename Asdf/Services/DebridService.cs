using Asdf.Clients.AllDebrid;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Asdf.Services
{
	public class DebridService
	{
		private static readonly List<DebridModel> _items = new List<DebridModel>();
		private readonly AllDebridClient _client;

		public DebridService(AllDebridClient client)
		{
			_client = client;
		}

		public Task<string> LoginAsync()
		{
			return _client.LoginAsync();
		}

		public async Task<IEnumerable<DebridModel>> GetItemsAsync(bool force = false)
		{
			if (force)
			{
				_items.Clear();
			}

			if (_items.Count > 0)
			{
				return _items;
			}

			var items = await _client.GetTorrentsAsync();
			var result = items.torrents?.Values?.Select(x => new DebridModel
			{
				Id = x.id,
				Name = x.filename,
				SizeCurrent = x.statusCode < 3 ? x.downloaded : x.uploaded,
				SizeTotal = x.size,
				Status = x.status,
				StatusType = x.statusCode < 4 ? DebridStatus.Processing : x.statusCode == 4 ? DebridStatus.Finished : DebridStatus.Error,
				Files = x.links?.Select(y => new DebridFileModel
				{
					Name = y.filename,
					Link = y.link,
					Size = y.size
				})?.OrderBy(y => y.Name).ThenByDescending(y => y.Size)
			});

			if (result != null)
			{
				_items.AddRange(result.OrderBy(x => x.StatusType).ThenBy(x => x.Name));
			}
			return _items;
		}

		public Task DeleteAsync(DebridModel item)
		{
			return _client.DeleteAsync(item.Id);
		}
	}
}