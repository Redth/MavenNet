using System;
using Xunit;

namespace MavenNet.Tests
{
	public class MavenVersioningTests
	{
		[Theory]
		[InlineData("26.1.0", "28.0.0-beta01", true)]
		[InlineData("28.0.0", "28.0.0-beta01", false)]
		[InlineData("[1.0,2.0]", "1.0", true)]
		[InlineData("[1.0,2.0]", "1.5", true)]
		[InlineData("[1.0,2.0]", "2.0", true)]
		[InlineData("[1.0,2.0]", "2.1", false)]
		[InlineData("[1.0,2.0]", "0.9", false)]
		[InlineData("(,1.0],[1.2,)", "0.9", true)]
		[InlineData("(,1.0],[1.2,)", "1.0", true)]
		[InlineData("(,1.0],[1.2,)", "1.2", true)]
		[InlineData("(,1.0],[1.2,)", "1.3", true)]
		[InlineData("(,1.0],[1.2,)", "1.1", false)]
		[InlineData("(,1.0],[2.2,)", "1.0.1.262e11d", false)]
		[InlineData("(,0.8],[2.2,)", "0.11.1.647c3c2", false)]
		[InlineData("(,0.8],[2.2,)", "0.11.1.3c786d2", false)]
		[InlineData("(,0.8],[2.2,)", "0.10.1.2e8fe11", false)]
		[InlineData("(,0.8],[2.2,)", "0.10.1.e92f734", false)]
		[InlineData("(,0.8],[2.2,)", "0.10.1.d22d4de", false)]
		[InlineData("(,0.8],[2.2,)", "0.9.3.545a756", false)]
		[InlineData("(,0.8],[2.2,)", "0.9.3.a319893", false)]
		public void Satisfy_Versions(string range, string version, bool satisfies)
		{
			var mvr = new MavenVersionRange(range);

			var actual = mvr.Satisfies(version);

			Assert.Equal(satisfies, actual);
		}
	}
}
