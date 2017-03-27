# MavenNet
A .NET Client for inspecting and interacting with Maven Repositories


## Huh? What? Why?
Some Android/Java libraries use a system kind of like NuGet, called Maven.  Most .NET developers would not, could not, should not care about this.

I had a particular need to be able to work with information and artifacts in Maven repositories to track dependency changes and updates.  Being a .NET developer, I decided it would be useful to have a .NET API for this.

## Usage

You can open a repository from a url or a directory path:

```csharp
// Open by URL
var repo = MavenRepository.OpenUrl ("https://mavenrepohost.com/somerepo");

// Open by Directory
var repo = MavenRepository.OpenDirectory ("/path/to/somerepo");
```

After you open a repository, you will want to load the metadata to be able to access a list of package information:

```csharp
await repo.LoadMetadataAsync ();
```

Once you've loaded metadata, you can access it via the metadata property:

```csharp
foreach (var item in repo.Metadata) {

	Console.WriteLine ($"{item.GroupId} - {item.ArtifactId}");

	foreach (var version in item.AllVersions) {
		Console.WriteLine ($"\t{version}");
	}
}
```

You can access the Project Object Model (.pom XML file details) as well:

```csharp
var project = await repo.GetProjectAsync ("com.some.package", "artifact-id", "1.2.3");

foreach (var dep in project.Dependencies) {
	Console.WriteLine ($"{dep} - {dep.ArtifactId} - {dep.Version}");
}
```
