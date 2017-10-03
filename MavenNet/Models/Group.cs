using System.Collections.Generic;

namespace MavenNet.Models
{
	public class Group
	{
		public Group(string id)
		{
			Id = id;
		}

		public string Id { get; private set; }

		public IList<Artifact> Artifacts { get; internal set; } = new List<Artifact>();
	}
}
