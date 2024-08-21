using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace MavenNet
{
	public class MavenVersionRange
	{
		public MavenVersionRange(string value)
		{
			Value = value;
		}

		public string Value { get; set; }

		public bool Satisfies(string version)
		{
			var nugetVersion = default(NuGet.Versioning.NuGetVersion);
			
			try
			{
				nugetVersion = NuGet.Versioning.NuGetVersion.Parse(version);
			}
			catch (Exception e)
			{
				if (e is System.ArgumentException && e.Message.Contains(" is not a valid version string."))
				{
					try
					{
						int idx = version.LastIndexOf('.');
						// creating valid SemVer from invalid Maven
						string semver = version.Substring(0, idx);
						nugetVersion = NuGet.Versioning.NuGetVersion.Parse(semver);
						string release_textual = version.Substring(idx + 1, version.Length - idx - 1);
						try
						{
							int release_numeric = int.Parse(release_textual, NumberStyles.HexNumber);
							nugetVersion = NuGet.Versioning.NuGetVersion.Parse($"{semver}.{release_numeric}");
						}
						catch (Exception exception)
						{
							nugetVersion = NuGet.Versioning.NuGetVersion.Parse($"{semver}+{release_textual}");
						}
					}
					catch (Exception ei)
					{
						Console.WriteLine(ei);
						throw;
					}
				}
			}

			var v = Regex.Replace(Value, "\\s", "");

			if (string.IsNullOrEmpty(v))
				return false;

			var versionGroups = new List<string>();

			// Do a pass over the range string to parse out version groups
			// eg: (1.0],(1.1,]
			var isInGroup = false;
			var currentGroup = string.Empty;
			foreach (var c in v) {
				if (c == '(' || c == '[')
				{
					currentGroup += c;
					isInGroup = true;
				}
				else if (c == ')' || c == ']' || (!isInGroup && c == ','))
				{
					// Don't add the , separating groups
					if (isInGroup)
						currentGroup += c;

					isInGroup = false;
					if (!string.IsNullOrEmpty(currentGroup))
						versionGroups.Add(currentGroup);
					currentGroup = string.Empty;
				}
				else
				{
					currentGroup += c;
				}
			}

			if (!string.IsNullOrEmpty(currentGroup))
				versionGroups.Add(currentGroup);

			var anyGroupMatch = false;

			foreach (var group in versionGroups)
			{
				var nvr = NuGet.Versioning.VersionRange.Parse(group);
				if (nvr.Satisfies(nugetVersion))
					anyGroupMatch = true;
			}

			return anyGroupMatch;
		}
	}
}
