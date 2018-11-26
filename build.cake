#tool "xunit.runner.console"

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var debugConfiguration = Argument("configuration", "Debug");
var buildDir = Directory("./build");
var distDir = Directory("./dist");
var solution = "./LightStep.sln";
var lightStepAssemblyInfoFile = "./src/LightStep/Properties/AssemblyInfo.cs";		
var version = EnvironmentVariable("CIRCLE_TAG") ?? "v0.0.0";
version = version.TrimStart('v');
var buildNo = String.IsNullOrWhiteSpace(EnvironmentVariable("CIRCLE_BUILD_NUM")) ? "local" : EnvironmentVariable("CIRCLE_BUILD_NUM");
var semVersion = string.Concat(version + "-" + buildNo);
var nuGetApiKey = EnvironmentVariable("NuGet");

Task("Clean")
    .Does( ()=> 
{
    CleanDirectory(buildDir);
	CleanDirectory(distDir);
    CleanDirectories("./**/obj/*.*");
    CleanDirectories($"./**/bin/{configuration}/*.*");
	CleanDirectories($"./**/bin/{debugConfiguration}/*.*");
});

Task("Restore")
    .IsDependentOn("Clean")
    .Does( ()=> 
{
    DotNetCoreRestore(solution);
});

Task("Build")
	.IsDependentOn("Restore")
    .Does(() =>
	{
		CreateAssemblyInfo(lightStepAssemblyInfoFile, new AssemblyInfoSettings {
			Product = "LightStep",
			Version = version,
			FileVersion = version,
			InformationalVersion = semVersion,
			Copyright = string.Format("Copyright (c) LightStep 2018 - {0}", DateTime.Now.Year)
		});
		var assemblyInfo = ParseAssemblyInfo(lightStepAssemblyInfoFile);
		Information("Version: {0}", assemblyInfo.AssemblyVersion);
		Information("File version: {0}", assemblyInfo.AssemblyFileVersion);
		Information("Informational version: {0}", assemblyInfo.AssemblyInformationalVersion);
		MSBuild(solution, settings => settings
			.SetConfiguration(configuration)
			.WithTarget("Rebuild")
			.WithProperty("Version", assemblyInfo.AssemblyInformationalVersion)
			.SetVerbosity(Verbosity.Minimal));
    });

Task("Test")
	.IsDependentOn("Build")
    .Does(() =>
	{
		var projects = GetFiles("./test/**/*.csproj");
		
        foreach(var project in projects)
        {
			DotNetCoreTest(project.FullPath, new DotNetCoreTestSettings {
				Logger = "xunit;LogFilePath=../../build/test_results.xml"
			});
        }
 
});

Task("Publish")
    .IsDependentOn("Test")
	.WithCriteria(() => EnvironmentVariable("CI") == "true")
    .Does(() =>
    {
		var nupkg = GetFiles("./src/LightStep/bin/Release/*.nupkg").First();
		DotNetCoreNuGetPush(nupkg.FullPath, new DotNetCoreNuGetPushSettings {
			ApiKey = nuGetApiKey
		});
    });

Task("Default")
	.IsDependentOn("Test");

RunTarget(target);