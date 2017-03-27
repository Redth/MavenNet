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
	public interface IMavenRepository
	{
		List<Models.Metadata> Metadata { get; }

		Task LoadMetadataAsync();

		Task<Models.Project> GetProjectAsync(string groupId, string artifactId);
		Task<Models.Project> GetProjectAsync(string groupId, string artifactId, string version);

		Task<Stream> LoadFileAsync(string path);
		Task<string> LoadTextFileAsync(string path);
		Task<IEnumerable<string>> GetFilesAsync(string path);
		Task<IEnumerable<string>> GetDirectoriesAsync(string path);
		string CombinePaths(params string[] paths);
	}

	public abstract class MavenRepository : IMavenRepository
	{
		public static IMavenRepository OpenUrl(string url)
		{
			return new UrlMavenRepository(url);
		}

		public static IMavenRepository OpenDirectory(string directoryPath)
		{
			return new DirectoryMavenRepository(directoryPath);
		}

		public abstract Task<Stream> LoadFileAsync(string path);
		public abstract Task<string> LoadTextFileAsync(string path);
		public abstract Task<IEnumerable<string>> GetFilesAsync(string path);
		public abstract Task<IEnumerable<string>> GetDirectoriesAsync(string path);
		public abstract string CombinePaths(params string[] paths);

		public List<Models.Metadata> Metadata { get; private set; } = new List<Models.Metadata>();

		protected virtual void BeforeLoadMetadata()
		{
		}

		public async Task LoadMetadataAsync()
		{
			BeforeLoadMetadata();

			var items = new List<Models.Metadata>();

			await recurseDir(string.Empty, items);

			Metadata.Clear();
			Metadata.AddRange(items);
		}

		async Task recurseDir(string path, List<Models.Metadata> items)
		{
			var files = await GetFilesAsync(path);

			// Look for maven-metadata.xml
			var metadataItem = files?.FirstOrDefault(f => f.Equals("maven-metadata.xml", StringComparison.OrdinalIgnoreCase));

			// If we found the maven-metadata.xml file, we are on an artifact folder
			// We can stop recursing subdirs at this point since we found artifact info
			if (!string.IsNullOrEmpty (metadataItem))
			{
				var itemStream = await LoadFileAsync (CombinePaths(path, metadataItem));

				var m = MavenMetadataParser.Parse (itemStream);

				if (m != null) {
					m.Path = path;
					items.Add (m);
				}
			}
			else
			{
				// If no maven-metadata.xml file was found let's try to keep recursing folders to find an artifact dir
				var dirs = await GetDirectoriesAsync(path);

				// Recurse over subdirs
				foreach (var dir in dirs)
				{
					var rPath = CombinePaths(path, dir);
					await recurseDir(rPath, items);
				}
			}
		}

		public Task<Models.Project> GetProjectAsync(string groupId, string artifactId)
		{
			return GetProjectAsync(groupId, artifactId, null);
		}

		public async Task<Models.Project> GetProjectAsync(string groupId, string artifactId, string version)
		{
			var metadata = Metadata?.FirstOrDefault(m => m.GroupId == groupId && m.ArtifactId == artifactId);

			if (metadata == null)
				return null;

			// Will store an actual version number that exists in the metadata here to fetch
			string validVersion = null;

			// Now we need to validate the requested version actually exists
			// If no version parameter was specified, we'll try and pick the most sane option from the metadata
			if (string.IsNullOrEmpty(version))
			{
				// Look at the Version tag, then Latest, then Release, then worst case grab the first from the list
				validVersion = metadata?.Version;
				if (string.IsNullOrEmpty(validVersion))
					validVersion = metadata?.Versioning?.Latest;
				if (string.IsNullOrEmpty(validVersion))
					validVersion = metadata?.Versioning?.Release;
				if (string.IsNullOrEmpty(validVersion))
					validVersion = metadata?.Versioning?.Versions?.FirstOrDefault();
			}
			else // Version was specified
			{
				// See if the specified version exists anywhere in the metadata
				if (version == metadata?.Version
					|| version == metadata?.Versioning?.Latest
					|| version == metadata?.Versioning?.Release
				    || (metadata?.Versioning?.Versions?.Contains(version) ?? false))
					validVersion = version;
			}

			if (string.IsNullOrEmpty(validVersion))
				return null;

			// POM file should always be in this format
			var pomFile = CombinePaths(metadata.Path, validVersion, $"{metadata.ArtifactId}-{validVersion}.pom");

			var stream = await LoadFileAsync(pomFile);

			var project = PomParser.Parse(stream);

			if (project != null)
			{
				project.Repository = this;
				project.Directory = CombinePaths(metadata.Path, validVersion);
				project.Path = pomFile;
			}


			return project;
		}
	}

	internal class DirectoryListing
	{
		public bool IsDirectory { get; set; }
		public string Name { get; set; }
	}
}
