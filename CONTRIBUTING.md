# Contributing Guide

First, thanks for your contribution! 🎉

We gladly accept issues and pull requests to this repository. If this is a security-related issue, please email us directly at infosec@lightstep.com. 

Please note the following general guidelines and advice -

## Issues
When submitting an issue, please include the following in addition to an explanation of the issue:
- Version of the library you're using.
- The runtime and platform version you're using (i.e., .NET 4.6.1 or .NET Core 2.1).
- Any stack trace or diagnostic logs you may have that demonstrate the issue.

## Pull Requests
Before making a pull request for a feature, please open an issue in order to gain consensus on the work you're doing.

All pull requests should be rebased against `master`, and all tests should pass before a PR will be merged.

In addition, ensure that:
- Test coverage has been added, where appropriate.
- The CHANGELOG and README have been updated.
- If you are making a breaking change, indicate it in the CHANGELOG.

## Releases
To make a release, commit a tag to master of the format `vmajor.minor.patch` or `vmajor.minor.patch-alpha/beta`. CircleCI should automatically build and publish the resulting artifact.

## Developing

This library is intended for cross-platform usage, as well as cross-platform development. Please ensure that any dependencies added or changed fully support cross-platform .NET via Mono and .NET Core.

_Development Dependencies_
- .NET Framework 4.5+ (On MacOS/Linux, you need Mono 5.16, stable channel). **If on MacOS, install Mono via its installer and not Homebrew**
- .NET Core 2.1+
- Cake (see _Local Builds_)
- PostSharp (Windows only, for `LightStep.CSharpAspectTestApp`)

_Local Builds_

We use [Cake](https://cakebuild.net/) as a build tool. Run `dotnet tool install -g Cake.Tool` to make Cake globally available, then run `dotnet cake build.cake` to run tests. This requires .NET Core 2.1+.

You should be able to use any C# development environment, such as [Visual Studio Code](https://code.visualstudio.com/) with the C# extension, [Visual Studio 2017](https://visualstudio.microsoft.com/), or [JetBrains Rider](https://www.jetbrains.com/rider/). 