using NUnit.Framework;
using System.Linq;
using System;
using System.Threading.Tasks;

namespace MavenNet.Tests
{
	[TestFixture()]
	public class Test
	{
		const string GPS_LOCAL_REPO = "/Users/redth/Library/Developer/Xamarin/android-sdk-macosx/extras/google/m2repository/";
		const string ANDROID_THINGS_URL_REPO = "https://google.bintray.com/androidthings";

		[Test()]
		public async Task Test_Load_Metadata_URL()
		{
			var repo = MavenRepository.OpenUrl(ANDROID_THINGS_URL_REPO);
			await repo.LoadMetadataAsync();

			Assert.IsTrue(repo.Metadata.Any());
		}

		[Test]
		public async Task Test_Load_Project_Model_URL()
		{
			var repo = MavenRepository.OpenUrl(ANDROID_THINGS_URL_REPO);
			await repo.LoadMetadataAsync();

			var project = await repo.GetProjectAsync("com.google.android.things", "androidthings", "0.2-devpreview");

			Assert.IsTrue(project != null);
		}

		[Category ("LOCAL_ONLY")]
		[Test]
		public async Task Test_Load_Metadata_FILE()
		{
			var repo = MavenRepository.OpenDirectory(GPS_LOCAL_REPO);
			await repo.LoadMetadataAsync();

			Assert.IsTrue(repo.Metadata.Any());
		}

		[Category("LOCAL_ONLY")]
		[Test]
		public async Task Test_Load_Project_Model_FILE()
		{
			var repo = MavenRepository.OpenDirectory(GPS_LOCAL_REPO);
			await repo.LoadMetadataAsync();

			var project = await repo.GetProjectAsync("com.google.android.gms", "play-services-basement", "10.2.0");

			Assert.IsTrue(project != null);

			Assert.IsTrue(project.Dependencies?.Any());
		}


		[Category("LOCAL_ONLY")]
		[Test]
		public async Task Test_Load_Project_Artifacts_FILE()
		{
			var repo = MavenRepository.OpenDirectory(GPS_LOCAL_REPO);
			await repo.LoadMetadataAsync();

			var project = await repo.GetProjectAsync("com.google.android.gms", "play-services-basement", "10.2.0");

			Assert.IsTrue(project != null);

			var artifacts = await project.GetArtifactsAsync();

			Assert.IsTrue(artifacts.Any());
		}
	}
}
