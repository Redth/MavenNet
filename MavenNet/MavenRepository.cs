using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MavenNet.Models;

namespace MavenNet
{
    public abstract class MavenRepository : IMavenRepository
    {
        public static GoogleMavenRepository FromGoogle()
        {
            return new GoogleMavenRepository();
        }

        public static MavenCentralRepository FromMavenCentral()
        {
            return new MavenCentralRepository();
        }

        public static FileBasedMavenRepository FromUrl(string url)
        {
            return new UrlMavenRepository(url);
        }
        public static AzureDevOpsRepository FromAzureDevOps(string organization, string feed, string PAT)
        {
            return new AzureDevOpsRepository(organization, feed, PAT);
        }

        public static FileBasedMavenRepository FromDirectory(string directoryPath)
        {
            return new DirectoryMavenRepository(directoryPath);
        }

        public IList<Group> Groups { get; private set; } = new List<Group>();


        protected abstract char PathSeparator { get; }

        protected abstract Task<Stream> OpenFileAsync(string path);
        protected abstract Task<IEnumerable<string>> GetGroupIdsAsync();
        protected abstract Task<IEnumerable<Artifact>> GetArtifactsAsync(string groupId);
        protected abstract string CombinePaths(params string[] parts);

        public virtual async Task Refresh()
        {
            var groupIds = await GetGroupIdsAsync().ConfigureAwait(false);

            await Refresh(groupIds.ToArray());
        }

        public virtual async Task Refresh(params string[] groupIds)
        {
            Groups.Clear();

            foreach (var groupId in groupIds)
            {

                var g = new Group(groupId);

                var artifacts = await GetArtifactsAsync(groupId).ConfigureAwait(false);

                // Set a reference to this repository implementation
                foreach (var a in artifacts)
                {
                    a.Repository = this;
                    g.Artifacts.Add(a);
                }

                Groups.Add(g);
            }
        }

        public virtual Task Refresh(params Group[] groups)
        {
            Groups.Clear();

            foreach (var group in groups)
            {
                foreach (var a in group.Artifacts)
                    a.Repository = this;

                Groups.Add(group);
            }

            return Task.CompletedTask;
        }

        public virtual Task<Stream> OpenArtifactPomFile(string groupId, string artifactId, string version)
        {
            var path = CombinePaths(CombinePaths(groupId.Split('.')), artifactId, version, artifactId + "-" + version + ".pom");

            return OpenFileAsync(path);
        }

        public virtual Task<Stream> OpenArtifactLibraryFile(string groupId, string artifactId, string version, string packaging = "jar")
        {
            var path = CombinePaths(CombinePaths(groupId.Split('.')), artifactId, version, artifactId + "-" + version + "." + packaging.ToLowerInvariant().TrimStart('.'));

            return OpenFileAsync(path);
        }

        public virtual Task<Stream> OpenArtifactSourcesFile(string groupId, string artifactId, string version, string sourcesPostfix = "sources", string sourcesExtension = "jar")
        {
            var path = CombinePaths(CombinePaths(groupId.Split('.')), artifactId, version, artifactId + "-" + version + $"-{sourcesPostfix}.{sourcesExtension.TrimStart('.')}");

            return OpenFileAsync(path);
        }

        public Task<Stream> OpenMavenMetadataFile(string groupId, string artifactId)
        {
            var path = CombinePaths(CombinePaths(groupId.Split('.')), artifactId, "maven-metadata.xml");

            return OpenFileAsync(path);
        }

        public Task<Project> GetProjectAsync(string groupId, string artifactId)
        {
            return GetProjectAsync(groupId, artifactId, null);
        }

        public async Task<Project> GetProjectAsync(string groupId, string artifactId, string version)
        {
            var group = Groups?.FirstOrDefault(g => g.Id == groupId);
            if (group == null)
                throw new KeyNotFoundException($"No group found for groupId: `{groupId}`");

            var artifact = group.Artifacts?.FirstOrDefault(a => a.GroupId == groupId && a.Id == artifactId);
            if (artifact == null)
                throw new KeyNotFoundException($"No artifact found for artifactId: `{artifactId}`");

            var hasVersion = artifact.Versions?.Any(v => v == version) ?? false;

            if (!hasVersion)
                throw new KeyNotFoundException($"No version for artifact `{artifactId}` with version: `{version}`");

            return PomParser.Parse(await artifact.OpenPomFile(version).ConfigureAwait(false));
        }
    }
}
