using System;
namespace MavenNet.Models
{
	public partial class Dependency
	{
		public bool Satisfies(string version)
		{
			var mavenVersionRange = new MavenVersionRange(this.Version);
			return mavenVersionRange.Satisfies(version);
		}
	}
}
