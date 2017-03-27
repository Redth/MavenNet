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

	public sealed class DirectoryMavenRepository : MavenRepository
	{
		public DirectoryMavenRepository(string directoryPath)
		{
			if (!Directory.Exists(directoryPath))
				Directory.CreateDirectory(directoryPath);

			BasePath = directoryPath;
		}

		public string BasePath { get; private set; }

		public override Task<Stream> LoadFileAsync(string path)
		{
			var fullPath = BasePath;

			if (!string.IsNullOrWhiteSpace (path))
				fullPath = Path.Combine(BasePath, path);

			return Task.FromResult((Stream)File.OpenRead(fullPath));
		}

		public override Task<string> LoadTextFileAsync(string path)
		{
			var fullPath = BasePath;

			if (!string.IsNullOrWhiteSpace(path))
				fullPath = Path.Combine(BasePath, path);

			using (var sr = new StreamReader(File.OpenRead(fullPath)))
				return sr.ReadToEndAsync();
		}


		public override Task<IEnumerable<string>> GetFilesAsync (string path)
		{
			var fullPath = BasePath;

			if (!string.IsNullOrWhiteSpace(path))
				fullPath = Path.Combine(BasePath, path);

			var dirInfo = new DirectoryInfo(fullPath);

			if (!dirInfo.Exists)
				return Task.FromResult(Enumerable.Empty<string>());

			return Task.FromResult(dirInfo.GetFiles().Select(f => f.Name));
		}

		public override Task<IEnumerable<string>> GetDirectoriesAsync(string path)
		{
			var fullPath = BasePath;

			if (!string.IsNullOrWhiteSpace(path))
				fullPath = Path.Combine(BasePath, path);

			var dirInfo = new DirectoryInfo(fullPath);

			if (!dirInfo.Exists)
				return Task.FromResult(Enumerable.Empty<string>());

			return Task.FromResult(dirInfo.GetDirectories().Select(f => f.Name));
		}

		public override string CombinePaths(params string[] paths)
		{
			return Path.Combine(paths);
		}
	}
	
}
