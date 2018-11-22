#tool "xunit.runner.console"

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var debugConfiguration = Argument("configuration", "Debug");
var buildDir = Directory("./build");
var distDir = Directory("./dist");
var solutionFile = GetFiles("./*.sln").First();
var solution = new Lazy<SolutionParserResult>(() => ParseSolution(solutionFile));
var lightStepAssemblyInfoFile = "./src/LightStep/Properties/AssemblyInfo.cs";		
var version = EnvironmentVariable("CIRCLE_TAG") ?? "0.0.0";
var buildNo = EnvironmentVariable("CIRCLE_BUILD_NUM") ?? "local";
var semVersion = string.Concat(version + "-" + buildNo);

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