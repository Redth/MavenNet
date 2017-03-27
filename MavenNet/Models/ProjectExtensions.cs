using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MavenNet.Models
{
	[Serializable]
	public class Artifact
	{
		[System.Xml.Serialization.XmlIgnore]
		public Project Project { get; set; }

		[System.Xml.Serialization.XmlIgnore]
		public string Path { get; set; }


		public Task<Stream> GetStreamAsync()
		{
			return Project.Repository.LoadFileAsync(Path);
		}
	}

	public partial class Project
	{
		[System.Xml.Serialization.XmlIgnore]
		public string Path { get; internal set; }

		[System.Xml.Serialization.XmlIgnore]
		public string Directory { get; internal set; }

		[System.Xml.Serialization.XmlIgnore]
		public IMavenRepository Repository { get; internal set; }

		public async Task<IEnumerable<Artifact>> GetArtifactsAsync()
		{
			var files = await Repository.GetFilesAsync(Directory);

			return files.Select(f => new Artifact {
				Path = Repository.CombinePaths(Path, f),
				Project = this
			});
		}
	}
}
