using System;
using System.IO;
using System.Xml.Serialization;

namespace MavenNet
{
	static class PomParser
	{
		public static Models.Project Parse(Stream stream)
		{
			Models.Project result = null;

			var serializer = new XmlSerializer(typeof(Models.Project));

			using (var sr = new StreamReader(stream))
				result = (Models.Project)serializer.Deserialize(sr);

			return result;
		}
	}
}
