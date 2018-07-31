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
		public void Satisfy_Versions(string range, string version, bool satisfies)
		{
			var mvr = new MavenVersionRange(range);

			var actual = mvr.Satisfies(version);

			Assert.Equal(satisfies, actual);
		}
	}
}
