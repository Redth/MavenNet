using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MavenNet.Models
{
	[Serializable]
	public class Artifact
	{
		public Artifact(string id, string groupId, params string[] versions)
		{
			Id = id;
			GroupId = groupId;
			Versions = versions;
		}

		public string GroupId { get; private set; }
		public string Id { get; private set; }
		public IList<string> Versions { get; private set; }

		public MavenRepository Repository { get; internal set; }

		public async Task<Models.Metadata> GetMetadata ()
		{
			using (var s = await Repository.OpenMavenMetadataFile(GroupId, Id).ConfigureAwait(false))
				return MavenMetadataParser.Parse(s);
		}

		public Task<Stream> OpenPomFile(string version)
		{
			return Repository.OpenArtifactPomFile(GroupId, Id, version);
		}

		public Task<Stream> OpenLibraryFile(string version, string packaging = "jar")
		{
			return Repository.OpenArtifactLibraryFile(GroupId, Id, version, packaging);
		}
	}
}
