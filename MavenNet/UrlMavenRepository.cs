using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MavenNet.Models;

namespace MavenNet
{

	public sealed class UrlMavenRepository : MavenRepository
	{
		HttpClient http;

		public UrlMavenRepository(string url)
		{
			http = new HttpClient();
			BaseUri = new Uri(url);
		}

		public Uri BaseUri { get; private set; }

		public override Task<Stream> LoadFileAsync(string path)
		{
			var fullUri = BaseUri;

			if (!string.IsNullOrWhiteSpace(path))
			{
				var uriBuilder = new UriBuilder(BaseUri);
				uriBuilder.Path += "/" + path.TrimStart('/');
				fullUri = uriBuilder.Uri;
			}

			return http.GetStreamAsync(fullUri);
		}

		public override Task<string> LoadTextFileAsync(string path)
		{
			var fullUri = BaseUri;

			if (!string.IsNullOrWhiteSpace(path))
			{
				var uriBuilder = new UriBuilder(BaseUri);
				uriBuilder.Path += "/" + path.TrimStart('/');
				fullUri = uriBuilder.Uri;
			}

			return http.GetStringAsync(fullUri);
		}

		Dictionary<string, string> htmlListingCache = new Dictionary<string, string>();

		protected override void BeforeLoadMetadata()
		{
			base.BeforeLoadMetadata();

			htmlListingCache.Clear();
		}

		public override async Task<IEnumerable<string>> GetDirectoriesAsync(string path)
		{
			const string rxPattern = @"<a\s+href\s?=\s?""(?<dir>.*?)"">";

			var list = new List<string>();
			if (!htmlListingCache.ContainsKey(path))
				htmlListingCache.Add(path, await LoadTextFileAsync(path));
			var html = htmlListingCache[path];

			var matches = Regex.Matches(html, rxPattern);

			foreach (Match m in matches)
			{
				var dir = m?.Groups?["dir"]?.Value;

				if (string.IsNullOrEmpty(dir) || !dir.EndsWith ("/", StringComparison.OrdinalIgnoreCase))
					continue;

				list.Add(dir.Trim('/'));
			}

			return list;
		}

		public override async Task<IEnumerable<string>> GetFilesAsync(string path)
		{
			const string rxPattern = @"<a\s+href\s?=\s?""(?<dir>.*?)"">";

			var list = new List<string>();
			if (!htmlListingCache.ContainsKey(path))
				htmlListingCache.Add(path, await LoadTextFileAsync(path));
			var html = htmlListingCache[path];

			var matches = Regex.Matches(html, rxPattern);

			foreach (Match m in matches)
			{
				var dir = m?.Groups?["dir"]?.Value;

				if (string.IsNullOrEmpty(dir) || dir.EndsWith ("/", StringComparison.OrdinalIgnoreCase))
					continue;

				list.Add(dir.Trim('/'));
			}

			return list;
		}

		public override string CombinePaths(params string[] paths)
		{
			var result = new StringBuilder();

			var index = 0;

			foreach (var p in paths)
			{
				if (index <= 0)
					result.Append(p.TrimEnd('/'));
				else
					result.Append("/" + p.Trim('/'));
				index++;
			}

			return result.ToString();
		}
	}
	
}
