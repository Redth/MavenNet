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
	public abstract class FileBasedMavenRepository : MavenRepository
	{
		public abstract Task<IEnumerable<string>> GetFilesAsync(string path);
		public abstract Task<IEnumerable<string>> GetDirectoriesAsync(string path);

		protected override async Task<IEnumerable<string>> GetGroupIdsAsync()
		{
			var groupIds = new List<string>();

			await recurseDir(string.Empty, groupIds).ConfigureAwait(false);

			return groupIds;
		}

		protected override async Task<IEnumerable<Artifact>> GetArtifactsAsync(string groupId)
		{
			var artifacts = new List<Artifact>();

			var groupDir = CombinePaths(groupId.Split('.'));

			var artifactDirs = await GetDirectoriesAsync(groupDir).ConfigureAwait(false);

			foreach (var artifactDir in artifactDirs) {
				
				var versions = await GetDirectoriesAsync(CombinePaths(groupDir, artifactDir)).ConfigureAwait(false);

				if (!string.IsNullOrEmpty(artifactDir) && versions != null && versions.Any())
					artifacts.Add(new Artifact(artifactDir, groupId, versions.ToArray()));
			}

			return artifacts;
		}

		async Task recurseDir(string path, List<string> groupIds)
		{
			// If this path was a group, what would the id be calculated as.
			// We are recursing to look for the folder below the group folder, which will contain a maven-metadata.xml file
			// So the group will actually be the parent of this folder, so we calculate one level up
			var groupId = path.Replace('/', '.').Replace('\\', '.').Trim('.');
			if (groupId.Contains('.'))
				groupId = groupId.Substring(0, groupId.LastIndexOf('.'));

			// See if this group was already detected and exit our recursion if so
			if (!string.IsNullOrEmpty(groupId) && groupIds.Any(gid => gid.Equals(groupId, StringComparison.OrdinalIgnoreCase)))
				return;

			// If we got this far, we aren't trying to process a duplicate group name, so continue looking for maven-metadata.xml
			var files = await GetFilesAsync(path).ConfigureAwait(false);

			// Look for maven-metadata.xml
			var metadataItem = files?.FirstOrDefault(f => f.Equals("maven-metadata.xml", StringComparison.OrdinalIgnoreCase));

			// If we found the maven-metadata.xml file, we are on an artifact folder
			// We can stop recursing subdirs at this point since we found artifact info
			if (!string.IsNullOrEmpty (metadataItem))
			{
				groupIds.Add(groupId);
			}
			else
			{
				// If no maven-metadata.xml file was found let's try to keep recursing folders to find an artifact dir
				var dirs = await GetDirectoriesAsync(path).ConfigureAwait(false);

				// Recurse over subdirs
				foreach (var dir in dirs)
				{
					var rPath = CombinePaths(path, dir);
					await recurseDir(rPath, groupIds).ConfigureAwait(false);
				}
			}
		}
	}

	internal class DirectoryListing
	{
		public bool IsDirectory { get; set; }
		public string Name { get; set; }
	}
}
