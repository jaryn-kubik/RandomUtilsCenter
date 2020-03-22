namespace Asdf.Clients.Tmdb
{
	public class TmdbEpisodeResponse : TmdbResponse
	{
		public string air_date { get; set; }
		public int season_number { get; set; }
		public int episode_number { get; set; }
		public string name { get; set; }
	}
}