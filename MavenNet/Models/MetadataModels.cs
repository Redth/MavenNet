using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace MavenNet.Models
{
	[Serializable]
	[XmlRoot (ElementName="metadata")]
	public partial class Metadata
	{
		public string Path { get; set; }

		[XmlElement (ElementName="groupId")]
		public string GroupId { get; set; }

		[XmlElement (ElementName="artifactId")]
		public string ArtifactId { get; set; }

		[XmlElement (ElementName="version")]
		public string Version { get; set; }

		[XmlElement (ElementName="versioning")]
		public Versioning Versioning { get; set; }

		[XmlArrayAttribute("plugins")]
		[XmlArrayItemAttribute("plugin")]
		public List<MetadataPlugin> Plugins { get; set; }

		[XmlIgnore]
		public List<string> AllVersions
		{
			get
			{
				var list = new List<string>();
				if (!string.IsNullOrEmpty(Version))
					list.Add(Version);
				if (!string.IsNullOrEmpty(Versioning?.Latest))
					list.Add(Versioning.Latest);
				if (!string.IsNullOrEmpty(Versioning?.Release))
					list.Add(Versioning.Release);
				if (Versioning?.Versions?.Any() ?? false)
					list.AddRange(Versioning.Versions);

				return list.Distinct ().OrderBy(v => v).ToList ();
			}
		}
	}

	[Serializable]
	[XmlRoot (ElementName="versioning")]
	public class Versioning
	{
		[XmlElement (ElementName="latest")]
		public string Latest { get; set; }

		[XmlElement (ElementName="release")]
		public string Release { get; set; }

		[XmlElement (ElementName="lastUpdated")]
		public string LastUpdated { get; set; }

		[XmlElement (ElementName="snapshot")]
		public Snapshot Snapshot { get; set; }

		[XmlArrayAttribute("versions")]
		[XmlArrayItemAttribute("version", DataType = "string")]
		public List<string> Versions { get; set; }

		[XmlArrayAttribute("snapshotVersions")]
		[XmlArrayItemAttribute("snapshotVersion")]
		public List<SnapshotVersion> SnapshotVersions { get; set; }
	}

	[Serializable]
	[XmlRoot (ElementName="snapshot")]
	public class Snapshot
	{
		[XmlElement (ElementName="timestamp")]
		public string Timestamp { get; set; }

		[XmlElement (ElementName="buildNumber")]
		public int BuildNumber { get; set; }

		[XmlElement (ElementName="localCopy")]
		public bool LocalCopy { get; set; }
	}

	[Serializable]
	[XmlRoot (ElementName="snapshotVersion")]
	public class SnapshotVersion
	{
		[XmlElement (ElementName="classifier")]
		public string Classifier { get; set; }

		[XmlElement (ElementName="extension")]
		public string Extension { get; set; }

		[XmlElement (ElementName="value")]
		public string Value { get; set; }

		[XmlElement (ElementName="updated")]
		public string Updated { get; set; }
	}

	[Serializable]
	[XmlRoot (ElementName="plugin")]
	public class MetadataPlugin
	{
		[XmlElement (ElementName="name")]
		public string Name { get; set; }

		[XmlElement (ElementName="prefix")]
		public string Prefix { get; set; }

		[XmlElement (ElementName="artifactId")]
		public string ArtifactId { get; set; }
	}
}
