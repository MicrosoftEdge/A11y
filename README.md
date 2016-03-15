# A11y Automation Test Suite
An automated implementation of [html5accessibility.com](http://html5accessibility.com/)
for Microsoft Edge.

## Running
If you have Visual Studio, simply open A11y.sln and run.

### Building and running without Visual Studio
It's also possible to run without installing Visual Studio using MSBuild.

First navigate to the project's root directory and install nuget:
```
powershell "(new-object net.webclient).DownloadFile('https://nuget.org/nuget.exe', 'nuget.exe')"
```

Then restore the packages:
```
nuget.exe restore A11y.sln
```

Finally find MSBuild.exe, which comes pre-installed with the .NET framework. It will be found
in the Framework folder of C:\Windows\Microsoft.NET. For example:
C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe. **Make sure to use the version installed on your computer.**

Use MSBuild to compile the solution:
```
C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe A11y.sln
```

Then just run the compiled program:
```
bin\Debug\Microsoft.Edge.A11y.exe
```

Here's all those together. Just navigate to the root directory and paste:
```
powershell "(new-object net.webclient).DownloadFile('https://nuget.org/nuget.exe', 'nuget.exe')"
nuget.exe restore A11y.sln
C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe A11y.sln
bin\Debug\Microsoft.Edge.A11y.exe
```

## Scores and reporting
For a full explanation of how scores are calculated, see [html5accessibility.com](http://html5accessibility.com/).

After the tests have run, the results are printed to the console and saved in the root directory
of the project with the name "scores.csv".
