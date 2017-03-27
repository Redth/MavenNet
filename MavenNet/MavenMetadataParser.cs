using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace MavenNet
{
	static class MavenMetadataParser
	{
		public static Models.Metadata Parse(Stream stream)
		{
			Models.Metadata result = null;

			var serializer = new XmlSerializer(typeof(Models.Metadata));

			using (var sr = new StreamReader (stream))
				result = (Models.Metadata)serializer.Deserialize (sr);

			return result;
		}
	}
}
