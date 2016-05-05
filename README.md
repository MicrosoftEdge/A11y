# A11y Automation Test Suite
An automated implementation of [html5accessibility.com](http://html5accessibility.com/)
for Microsoft Edge.

## Running
If you have Visual Studio, simply open A11y.sln and run.

Otherwise you can build and run in one step by calling run.ps1 within PowerShell.

### Building and running manually
It's also possible to run manually without Visual Studio or PowerShell.

First navigate to the project's root directory and install nuget:
``` powershell "(new-object net.webclient).DownloadFile('https://nuget.org/nuget.exe', 'nuget.exe')" ```

Then restore the packages:
``` nuget.exe restore A11y.sln ```

Finally find MSBuild.exe, which comes pre-installed with the .NET framework. It will be
in the C:\Windows\Microsoft.NET\Framework folder.
For example: C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe.
**Make sure to use the version installed on your computer.**

Use MSBuild to compile the solution: ```
C:\Windows\Microsoft.NET\Framework\<VERSION ON YOUR PC>\MSBuild.exe A11y.sln ```

Then just run the compiled program: ``` bin\Debug\Microsoft.Edge.A11y.exe ```

## Scores and reporting
For a full explanation of how scores are calculated, see
[html5accessibility.com](http://html5accessibility.com/).

After the tests have run, the results are printed to the console and saved in the root
directory of the project with the name "scores.csv".

## Testing your site
It's possible to use A11y to automate testing of your site as well. To understand how to do this view the [site testing documentation](https://github.com/MicrosoftEdge/A11y/tree/site_testing).

### Pass and failure conditions
The TestData.cs file contains the logic of the tests and an explanation of the built-in
tests. To add your own tests, add a TestData object for each test you want to run.

By default, elements are found by their control type, but you can pass in a custom method
of searching by adding a searchStrategy parameter.

In addition to the default tests, you can use the additionalRequirement paramter to
specify any other requirement that you want to verify. If you find yourself using the
same additionalRequirement for many of your tests, you may want to add another parameter
to the TestData constructor to simplify testing that requirement.

## Contributing
We want your feedback and your help! If you have any suggestions, file an issue and we
can figure out how to get your needs met.

If you'd like to submit code changes, the best thing to do is to file an issue first so
we can talk about whether the change would fit with the direction and purpose of the
project. Even if your changes don't fit with the general purpose of A11y, we'd love to
see you fork the project to do new things with it.

## Legal

You will need to complete a [Contributor License Agreement (CLA)](https://cla.microsoft.com/) before your pull request can be accepted. This agreement testifies that you are granting us permission to use the source code you are submitting, and that this work is being submitted under appropriate license that we can use it. The process is very simple as it just hooks into your Github account. Once we have received the signed CLA, we'll review the request. You will only need to do this once.
