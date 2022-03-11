namespace RandomUtilsCenter.Clients.Tmdb
{
	public class TmdbShowResponse : TmdbResponse
	{
		public string name { get; set; }
		public TmdbEpisodeResponse next_episode_to_air { get; set; }
	}
}