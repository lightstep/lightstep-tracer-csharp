#tool "xunit.runner.console"

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var buildDir = Directory("./build");
var distDir = Directory("./dist");
var solutionFile = GetFiles("./*.sln").First();
var solution = new Lazy<SolutionParserResult>(() => ParseSolution(solutionFile));


Task("Clean")
	.IsDependentOn("Clean-Outputs")
	.Does(() => 
	{
		MSBuild(solutionFile, settings => settings
			.SetConfiguration(configuration)
			.WithTarget("Clean")
			.SetVerbosity(Verbosity.Minimal));
	});

Task("Clean-Outputs")
	.Does(() => 
	{
		CleanDirectory(buildDir);
		CleanDirectory(distDir);
	});

Task("Build")
	.IsDependentOn("Clean-Outputs")
    .Does(() =>
	{
		NuGetRestore(solutionFile);

		MSBuild(solutionFile, settings => settings
			.SetConfiguration(configuration)
			.WithTarget("Rebuild")
			.SetVerbosity(Verbosity.Minimal));
    });

Task("Test")
	.IsDependentOn("Build")
    .Does(() =>
	{
		XUnit2(string.Format("./test/**/bin/{0}/*.Tests.dll", configuration), new XUnit2Settings {
			XmlReport = true,
			OutputDirectory = buildDir
		});
});

Task("Default")
	.IsDependentOn("Build");

RunTarget(target);