﻿using Xunit;
using System.Linq;
using System;
using System.Threading.Tasks;

namespace MavenNet.Tests
{
	public class Test
	{
		const string ANDROID_THINGS_URL_REPO = "https://google.bintray.com/androidthings";

		[Fact]
		public async Task Test_Refresh_URL()
		{
			var repo = MavenRepository.FromUrl(ANDROID_THINGS_URL_REPO);
			await repo.Refresh();

			Assert.True(repo.Groups.Any());
		}

		[Fact]
		public async Task Test_Project_URL()
		{
			var repo = MavenRepository.FromUrl(ANDROID_THINGS_URL_REPO);
			await repo.Refresh();

			var project = await repo.GetProjectAsync("com.google.android.things", "androidthings", "0.2-devpreview");

			Assert.True(project != null);
		}

		[Fact]
		public async Task Test_GroupIds_Project_URL()
		{
			var repo = MavenRepository.FromUrl(ANDROID_THINGS_URL_REPO);
			await repo.Refresh("com.google.android.things");

			var project = await repo.GetProjectAsync("com.google.android.things", "androidthings", "0.2-devpreview");

			Assert.True(project != null);
		}


		[Fact]
		public async Task Test_GroupIds_Project_URL_Maven()
		{
			var repo = MavenRepository.FromMavenCentral();
			await repo.Refresh("com.facebook.android");

			var project = await repo.GetProjectAsync("com.facebook.android", "facebook-android-sdk", "4.33.0");

			Assert.True(project != null);
			Assert.NotEmpty(project.Dependencies);
		}

		[Fact]
		public async Task Test_Refresh_GOOGLE()
		{
			var repo = MavenRepository.FromGoogle();
			await repo.Refresh();

			Assert.True(repo.Groups.Any());
		}

		[Fact]
		public async Task Test_Project_GOOGLE()
		{
			var repo = MavenRepository.FromGoogle();
			await repo.Refresh();

			var project = await repo.GetProjectAsync("com.google.android.gms", "play-services-basement", "10.2.0");

			Assert.True(project != null);

			Assert.True(project.Dependencies?.Any());
		}

		[Fact]
		public async Task Test_GroupIds_Project_GOOGLE()
		{
			var repo = MavenRepository.FromGoogle();
			await repo.Refresh("com.google.android.gms");

			var project = await repo.GetProjectAsync("com.google.android.gms", "play-services-basement", "10.2.0");

			Assert.True(project != null);

			Assert.True(project.Dependencies?.Any());
		}

		[Fact]
		public async Task Test_GroupIds_Project_MAVENCENTRAL()
		{
			var repo = MavenRepository.FromMavenCentral();
			await repo.Refresh("org.bouncycastle");

			var project = await repo.GetProjectAsync("org.bouncycastle", "bcpkix-jdk15on", "1.57");

			Assert.True(project != null);

			Assert.True(project.Dependencies?.Any());
		}

        [Fact]
        public async Task Test_GroupIds_Project_MAVENCENTRAL_1()
        {
            var repo = MavenRepository.FromMavenCentral();
            MavenRepository.HttpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/111.0.0.0 Safari/537.36");
            await repo.Refresh("com.google.accompanist");

            var project = await repo.GetProjectAsync("com.google.accompanist", "accompanist-appcompat-theme", "0.30.1");

            Assert.True(project != null);

            Assert.True(project.Dependencies?.Any());
        }

        [Fact]
        public async Task Test_GroupIds_Project_MAVENCENTRAL_2()
        {
            var repo = MavenRepository.FromMavenCentral();
            MavenRepository.HttpClient.DefaultRequestHeaders.Clear();
            await repo.Refresh("com.google.accompanist");

            try
            {
                var project = await repo.GetProjectAsync("com.google.accompanist", "accompanist-appcompat-theme", "0.30.1");
            }
            finally
            {
            }
        }
    }
}
