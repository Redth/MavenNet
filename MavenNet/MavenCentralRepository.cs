using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MavenNet.Models;
using Newtonsoft.Json.Linq;

namespace MavenNet
{
    public class MavenCentralRepository : MavenRepository
    {
        protected override char PathSeparator
            => '/';

        protected override string CombinePaths(params string[] parts)
        {
            return string.Join(new string(PathSeparator, 1), parts);
        }

        static readonly HttpClient http = new HttpClient();

        protected override async Task<IEnumerable<Artifact>> GetArtifactsAsync(string groupId)
        {
            var artifacts = new List<Artifact>();

            var url = $"http://search.maven.org/solrsearch/select?q=g:%22{groupId}%22&rows=100&wt=json";

            var data = await http.GetStringAsync(url);

            var json = JObject.Parse(data);

            var docs = json["response"]["docs"] as JArray;

            if (docs != null)
            {
                foreach (var doc in docs)
                {
                    var gid = doc.Value<string>("g");
                    var aid = doc.Value<string>("a");

                    try
                    {
                        using (var s = await OpenMavenMetadataFile(gid, aid).ConfigureAwait(false))
                        {
                            var metadata = MavenMetadataParser.Parse(s);

                            artifacts.Add(new Artifact(aid, gid, metadata.AllVersions.ToArray()));
                        }
                    }
                    catch { }
                }
            }

            return artifacts;
        }

        // Optimization that avoids the Maven search, which also avoids the 100 row limit
        protected override async Task<IEnumerable<Artifact>> GetArtifactsAsync(string groupId, params string[] artifactIds)
        {
            var artifacts = new List<Artifact>();

            foreach (var aid in artifactIds) {
                try {
                    using (var s = await OpenMavenMetadataFile(groupId, aid).ConfigureAwait(false)) {
                        var metadata = MavenMetadataParser.Parse(s);

                        artifacts.Add(new Artifact(aid, groupId, metadata.AllVersions.ToArray()));
                    }
                } catch { }
            }

            return artifacts;
        }

        protected override Task<IEnumerable<string>> GetGroupIdsAsync()
        {
            return Task.FromResult<IEnumerable<string>>(new List<string>());
        }

        protected override Task<Stream> OpenFileAsync(string path)
        {
            var url = $"https://repo1.maven.org/maven2/{path}";

            return http.GetStreamAsync(url);
        }
    }
}
