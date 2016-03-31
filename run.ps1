$msbuild = Get-Item C:\Windows\Microsoft.NET\Framework\v4*\MSBuild\
& $msbuild .\A11y.sln

.\bin\Debug\Microsoft.Edge.A11y.exe
