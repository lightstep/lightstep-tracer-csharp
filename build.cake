#tool "xunit.runner.console"
#tool "nuget:?package=GitVersion.CommandLine"

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var debugConfiguration = Argument("configuration", "Debug");
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

		MSBuild(solutionFile, settings => settings
			.SetConfiguration(debugConfiguration)
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
		GitVersion(new GitVersionSettings { UpdateAssemblyInfo = true });
		MSBuild(solutionFile, settings => settings
			.SetConfiguration(configuration)
			.WithTarget("Rebuild")
			.SetVerbosity(Verbosity.Minimal));
    });

Task("Test")
	.IsDependentOn("Build")
    .Does(() =>
	{
		XUnit2(string.Format("./test/**/bin/Release/**/*.Tests.dll", configuration), new XUnit2Settings {
			XmlReport = true,
			OutputDirectory = buildDir
		});
});

Task("Default")
	.IsDependentOn("Test");

RunTarget(target);