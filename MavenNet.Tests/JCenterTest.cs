using Xunit;
using System.Linq;
using System;
using System.Threading.Tasks;

namespace MavenNet.Tests
{
	public class JcenterTest
	{
		const string JCENTER_URL_REPO = "https://dl.bintray.com/kommunicate/Kommunicate-Android-Chat-SDK/";

		[Fact]
		public async Task Test_Refresh_URL()
		{
			var repo = MavenRepository.FromUrl(JCENTER_URL_REPO);
			await repo.Refresh();

			Assert.True(repo.Groups.Any());
		}

		[Fact]
		public async Task Test_Project_URL()
		{
			var repo = MavenRepository.FromUrl(JCENTER_URL_REPO);
			await repo.Refresh();

			var project = await repo.GetProjectAsync("io.kommunicate.sdk", "kommunicateui", "2.0.5");

			Assert.True(project != null);
		}

		[Fact]
		public async Task Test_GroupIds_Project_URL()
		{
			var repo = MavenRepository.FromUrl(JCENTER_URL_REPO);
			await repo.Refresh("io.kommunicate.sdk");

			var project = await repo.GetProjectAsync("io.kommunicate.sdk", "kommunicateui", "2.0.5");

			Assert.True(project != null);
		}
	}
}
