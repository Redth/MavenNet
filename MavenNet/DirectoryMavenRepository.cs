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

	public sealed class DirectoryMavenRepository : FileBasedMavenRepository
	{
		public DirectoryMavenRepository(string directoryPath)
		{
			if (!Directory.Exists(directoryPath))
				Directory.CreateDirectory(directoryPath);

			BasePath = directoryPath;
		}

		public string BasePath { get; private set; }

		protected override Task<Stream> OpenFileAsync(string path)
		{
			var fullPath = CombinePaths(BasePath, path);

			return Task.FromResult((Stream)File.OpenRead(fullPath));
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

		protected override char PathSeparator
		{
			get { return Path.DirectorySeparatorChar; }
		}

		protected override string CombinePaths(params string[] parts)
		{
			return Path.Combine(parts);
		}
	}
	
}
