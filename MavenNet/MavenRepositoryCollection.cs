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
	public class MavenRepositoryCollection
	{
		public MavenRepositoryCollection(params IMavenRepository[] repositories)
		{
			Repositories = repositories;
		}

		public IMavenRepository[] Repositories { get; private set; }

		public Task<Project> GetProjectAsync(string groupId, string artifactId)
		{
			return GetProjectAsync (groupId, artifactId, null);
		}

		public async Task<Project> GetProjectAsync(string groupId, string artifactId, string version)
		{
			foreach (var r in Repositories)
			{
				var p = await r.GetProjectAsync(groupId, artifactId, version).ConfigureAwait(false);

				if (p != null)
					return p;
			}

			return null;
		}

		public async Task Refresh()
		{
			foreach (var r in Repositories)
				await r.Refresh().ConfigureAwait(false);
		}
	}
}
