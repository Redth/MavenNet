using MavenNet.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;

namespace MavenNet
{
    public class AzureDevOpsRepository: MavenRepository
    {
        private readonly HttpClient _http;
        private readonly string _apiVersion= "api-version=6.0-preview.1";
        private readonly string _protocolType = "protocolType=maven";
        private readonly string _includeAllVersions = "includeAllVersions=true";
        private readonly Uri _pkgsUri;

        public AzureDevOpsRepository(string url)
        {
            _http = new HttpClient();
            BaseUri = new Uri(url);
        }
        public AzureDevOpsRepository(string organization, string feed, string personalAccessToken) 
            : this($"https://feeds.dev.azure.com/{organization}/_apis/packaging/Feeds/{feed}")
        {
            _pkgsUri = new Uri($"https://{organization}.pkgs.visualstudio.com/_apis/packaging/Feeds/{feed}");
            if (!string.IsNullOrWhiteSpace(personalAccessToken))
            {
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                  Convert.ToBase64String(
                      Encoding.ASCII.GetBytes(
                          string.Format("{0}:{1}", "", personalAccessToken?? string.Empty))));
            }
        }

        public Uri BaseUri { get; private set; }

        protected override char PathSeparator => '/';

        protected override string CombinePaths(params string[] parts) 
            => throw new NotImplementedException();

        protected override async Task<IEnumerable<Artifact>> GetArtifactsAsync(string groupId)
        {
			var artifacts = new List<Artifact>();

            var uriBuilder = new UriBuilder(BaseUri);
            uriBuilder.Path += "/packages";
            uriBuilder.Query += $"{_apiVersion}&{_protocolType}&{_includeAllVersions}&packageNameQuery={groupId}";

            var result = await _http.GetStringAsync(uriBuilder.Uri);
            var packages = JsonConvert.DeserializeObject<Models.DevOps.Packages>(result);

            foreach (var package in packages.value)
            {
                if (!package.groupId.Equals(groupId, StringComparison.OrdinalIgnoreCase))
                    continue;

                artifacts.Add(new Artifact(
                    package.artifactId,
                    package.groupId,
                    package.versions.Select(q => q.version).ToArray()
                    )
                );
            }

            return artifacts;
        }

        public override Task<Stream> OpenArtifactPomFile(string groupId, string artifactId, string version)
        {
            var path = $"/maven/{groupId}/{artifactId}/{version}/{artifactId}-{version}.pom/content";
            return OpenFileAsync(path);
        }

        public override Task<Stream> OpenArtifactLibraryFile(string groupId, string artifactId, string version, string packaging = "jar")
        {
            var path = $"/maven/{groupId}/{artifactId}/{version}/{artifactId}-{version}.{packaging}/content";
            return OpenFileAsync(path);
        }
        public override Task<Stream> OpenArtifactSourcesFile(string groupId, string artifactId, string version, string sourcesPostfix = "sources", string sourcesExtension = "jar")
        {
            var path = $"/maven/{groupId}/{artifactId}/{version}/{artifactId}-{version}-{sourcesPostfix}.{sourcesExtension}/content";
            return OpenFileAsync(path);
        }

        protected override Task<IEnumerable<string>> GetGroupIdsAsync()
        {
            throw new NotImplementedException();
        }

        protected override Task<Stream> OpenFileAsync(string path)
        {
            var uriBuilder = new UriBuilder(_pkgsUri);
            uriBuilder.Path += path;

            return _http.GetStreamAsync(uriBuilder.Uri);
        }
    }
}
