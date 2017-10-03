using System.Collections.Generic;
using System.Threading.Tasks;
using MavenNet.Models;

namespace MavenNet
{
	public interface IMavenRepository
	{
		Task<Project> GetProjectAsync(string groupId, string artifactId);
		Task<Project> GetProjectAsync(string groupId, string artifactId, string version);

		IList<Group> Groups { get; }

		Task Refresh();
	}
}
