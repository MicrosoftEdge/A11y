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
It's possible to use A11y to automate testing of your site as well. A sample project is
included on the site_testing branch.

### Test files
The sample test page is included in this repo on the site_testing branch. It's possible
(and better) to have your sites in another location. Just change the constructor call to
to the TestStrategy class passing in the base URL where your test files are located.
