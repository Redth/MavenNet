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
		const string GOOGLE_MAVEN_REPO = "https://dl.google.com/dl/android/maven2/";

		[Test()]
		public async Task Test_Refresh_URL()
		{
			var repo = MavenRepository.FromUrl(ANDROID_THINGS_URL_REPO);
			await repo.Refresh();

			Assert.IsTrue(repo.Groups.Any());
		}

		[Test]
		public async Task Test_Project_URL()
		{
			var repo = MavenRepository.FromUrl(ANDROID_THINGS_URL_REPO);
			await repo.Refresh();

			var project = await repo.GetProjectAsync("com.google.android.things", "androidthings", "0.2-devpreview");

			Assert.IsTrue(project != null);
		}

		[Test]
		public async Task Test_GroupIds_Project_URL()
		{
			var repo = MavenRepository.FromUrl(ANDROID_THINGS_URL_REPO);
			await repo.Refresh("com.google.android.things");

			var project = await repo.GetProjectAsync("com.google.android.things", "androidthings", "0.2-devpreview");

			Assert.IsTrue(project != null);
		}

		[Category("LOCAL_ONLY")]
		[Test]
		public async Task Test_Refresh_FILE()
		{
			var repo = MavenRepository.FromDirectory(GPS_LOCAL_REPO);
			await repo.Refresh();

			Assert.IsTrue(repo.Groups.Any());
		}

		[Category("LOCAL_ONLY")]
		[Test]
		public async Task Test_Project_FILE()
		{
			var repo = MavenRepository.FromDirectory(GPS_LOCAL_REPO);
			await repo.Refresh();

			var project = await repo.GetProjectAsync("com.google.android.gms", "play-services-basement", "10.2.0");

			Assert.IsTrue(project != null);

			Assert.IsTrue(project.Dependencies?.Any());
		}

		[Category("LOCAL_ONLY")]
		[Test]
		public async Task Test_GroupIds_Project_FILE()
		{
			var repo = MavenRepository.FromDirectory(GPS_LOCAL_REPO);
			await repo.Refresh("com.google.android.gms");

			var project = await repo.GetProjectAsync("com.google.android.gms", "play-services-basement", "10.2.0");

			Assert.IsTrue(project != null);

			Assert.IsTrue(project.Dependencies?.Any());
		}

		[Test]
		public async Task Test_Refresh_GOOGLE()
		{
			var repo = MavenRepository.FromGoogle();
			await repo.Refresh();

			Assert.IsTrue(repo.Groups.Any());
		}

		[Test]
		public async Task Test_Project_GOOGLE()
		{
			var repo = MavenRepository.FromGoogle();
			await repo.Refresh();

			var project = await repo.GetProjectAsync("com.google.android.gms", "play-services-basement", "10.2.0");

			Assert.IsTrue(project != null);

			Assert.IsTrue(project.Dependencies?.Any());
		}

		[Test]
		public async Task Test_GroupIds_Project_GOOGLE()
		{
			var repo = MavenRepository.FromGoogle();
			await repo.Refresh("com.google.android.gms");

			var project = await repo.GetProjectAsync("com.google.android.gms", "play-services-basement", "10.2.0");

			Assert.IsTrue(project != null);

			Assert.IsTrue(project.Dependencies?.Any());
		}
	}
}
