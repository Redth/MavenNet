using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using MavenNet.Models;

namespace MavenNet
{

	public sealed class GoogleMavenRepository : MavenRepository
	{
		HttpClient http;

		public GoogleMavenRepository()
		{
			http = new HttpClient();
			BaseUri = new Uri("https://dl.google.com/dl/android/maven2/");
		}

		public Uri BaseUri { get; private set; }

		Dictionary<string, string> htmlListingCache = new Dictionary<string, string>();


		protected async override Task<IEnumerable<string>> GetGroupIdsAsync()
		{
			var uriBuilder = new UriBuilder(BaseUri);
			uriBuilder.Path += "master-index.xml";
			using (var hs = await http.GetStreamAsync(uriBuilder.Uri).ConfigureAwait(false))
			{
				var x = XDocument.Load(hs);
				return x.XPathSelectElements("/*/*")?.Select(a => a.Name.ToString()) ?? Enumerable.Empty<string>();
			}
		}

		protected async override Task<IEnumerable<Artifact>> GetArtifactsAsync(string groupId)
		{
			var artifacts = new List<Artifact>();

			var uriBuilder = new UriBuilder(BaseUri);
			uriBuilder.Path += CombinePaths(groupId.Split('.')) + "/group-index.xml";

			using (var hs = await http.GetStreamAsync(uriBuilder.Uri).ConfigureAwait(false))
			{
				var x = XDocument.Load(hs);
				var artifactElems = x.XPathSelectElements("/*/*");

				// Check to see if we matched any elements
				if (artifactElems != null && artifactElems.Any()) {
					foreach (var ae in artifactElems) {
						// Extract the id and versions
						var id = ae.Name.ToString();
						var versions = ae.Attribute("versions")?.Value?.Split(',') ?? new string[]{};

						// Add them to our list if they are valid
						if (!string.IsNullOrEmpty(id) && versions != null && versions.Any())
							artifacts.Add(new Artifact(id, groupId, versions));
					}
				}
			}

			return artifacts;
		}

		protected override char PathSeparator
		{
			get { return '/'; }
		}

		protected override string CombinePaths(params string[] parts)
		{
			return string.Join("/", parts).Trim('/');
		}

		protected override Task<Stream> OpenFileAsync(string path)
		{
			var uriBuilder = new UriBuilder(BaseUri);
			uriBuilder.Path += path;

			return http.GetStreamAsync(uriBuilder.Uri);
		}
	}
}
