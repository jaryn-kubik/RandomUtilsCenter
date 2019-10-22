using System.Collections.Generic;

namespace Asdf.Services
{
	public class DebridModel
	{
		public long Id { get; set; }
		public string Name { get; set; }

		public DebridStatus StatusType { get; set; }
		public string Status { get; set; }

		public long SizeTotal { get; set; }
		public long SizeCurrent { get; set; }

		public IEnumerable<DebridFileModel> Files { get; set; }
	}

	public class DebridFileModel
	{
		public string Name { get; set; }
		public string Link { get; set; }
		public long Size { get; set; }
	}

	public enum DebridStatus
	{
		Error,
		Finished,
		Processing
	}
}