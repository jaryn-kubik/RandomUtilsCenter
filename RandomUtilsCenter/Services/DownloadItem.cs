namespace RandomUtilsCenter.Services
{
	public class DownloadItem
	{
		public string FileName { get; set; }
		public long Size { get; set; }
		public long Position { get; set; }
		public bool Done { get; set; }

		public long SpeedTotal { get; set; }
		public long SpeedCurrent { get; set; }
	}
}