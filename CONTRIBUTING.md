# Contributing Guide

First, thanks for your contribution! 🎉

We gladly accept issues and pull requests to this repository. Please note the following general guidelines and advice -

## Issues
When submitting an issue, please include the following in addition to an explanation of the issue:
- Version of the library you're using.
- The runtime and platform version you're using (i.e., .NET 4.6.1 or .NET Core 2.1).
- Any stack trace or diagnostic logs you may have that demonstrate the issue.

## Pull Requests
Before making a pull request for a feature, please open an issue in order to gain consensus on the work you're doing.

In addition, ensure that:
- Test coverage has been added, where appropriate.
- The CHANGELOG and README have been updated.
- If you are making a breaking change, indicate it in the CHANGELOG.

## Releases
To make a release, commit a tag to master of the format `vmajor.minor.patch` or `vmajor.minor.patch-alpha/beta`. CircleCI should automatically build and publish the resulting artifact.