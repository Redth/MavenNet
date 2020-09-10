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
	public sealed class UrlMavenRepository : FileBasedMavenRepository
	{
		HttpClient http;

		public UrlMavenRepository(string url)
		{
			http = new HttpClient();
			BaseUri = new Uri(url);
		}

		public Uri BaseUri { get; private set; }

		protected override Task<Stream> OpenFileAsync(string path)
		{
			var uriBuilder = new UriBuilder(BaseUri);
			uriBuilder.Path += "/" + path.TrimStart('/');

			return http.GetStreamAsync(uriBuilder.Uri);
		}

		Dictionary<string, string> htmlListingCache = new Dictionary<string, string>();

		public override Task Refresh()
		{
			htmlListingCache.Clear();

			return base.Refresh();
		}

		public override async Task<IEnumerable<string>> GetDirectoriesAsync(string path)
		{
			const string rxPattern = @"<a.*\s+href\s?=\s?""(?<dir>.*?)""";

			var list = new List<string>();
			//var html = htmlListingCache?[path] ?? await LoadTextFileAsync(path).ConfigureAwait (false);

			//if (!htmlListingCache.ContainsKey(path))
			//htmlListingCache.Add(path, html);

			var html = await LoadTextFileAsync(path);

			var matches = Regex.Matches(html, rxPattern, RegexOptions.Multiline);

			foreach (Match m in matches)
			{
				var dir = m?.Groups?["dir"]?.Value;

				if (string.IsNullOrEmpty(dir) || !dir.EndsWith ("/", StringComparison.OrdinalIgnoreCase))
					continue;

				list.Add(dir.Trim('/').Trim(':'));
			}

			return list;
		}

		public override async Task<IEnumerable<string>> GetFilesAsync(string path)
		{
			const string rxPattern = @"<a.*\s+href\s?=\s?""(?<dir>.+?)""";

			var list = new List<string>();
			string html = null;

			if (!htmlListingCache.ContainsKey(path)) {
				try
				{
					html = await LoadTextFileAsync(path).ConfigureAwait(false);
				} catch {}

				if (!string.IsNullOrEmpty(html))
					htmlListingCache.Add(path, html);
			}

			var matches = Regex.Matches(html ?? string.Empty, rxPattern);

			foreach (Match m in matches)
			{
				var dir = m?.Groups?["dir"]?.Value;

				if (string.IsNullOrEmpty(dir) || dir.EndsWith ("/", StringComparison.OrdinalIgnoreCase))
					continue;

				list.Add(dir.Trim('/'));
			}

			return list;
		}

		protected override char PathSeparator
		{
			get { return '/'; }
		}

		protected override string CombinePaths(params string[] parts)
		{
			var result = new StringBuilder();

			var index = 0;

			foreach (var p in parts)
			{
				if (index <= 0)
					result.Append(p.TrimEnd('/'));
				else
					result.Append("/" + p.Trim('/'));
				index++;
			}

			return result.ToString();
		}

		Task<string> LoadTextFileAsync(string path)
		{
			var fullUri = BaseUri;

			if (!string.IsNullOrWhiteSpace(path))
			{
				var uriBuilder = new UriBuilder(BaseUri);
				uriBuilder.Path += (uriBuilder.Path.EndsWith("/") ? "" : "/") + path.TrimStart('/');
				fullUri = uriBuilder.Uri;
			}

			return http.GetStringAsync(fullUri);
		}
	}
	
}
