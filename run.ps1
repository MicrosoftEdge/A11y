if(-Not (Test-Path .\nuget.exe)){
    (New-Object net.WebClient).DownloadFile('https://nuget.org/nuget.exe', 'nuget.exe')
}

nuget.exe restore A11y.sln

$msbuild = Get-Item C:\Windows\Microsoft.NET\Framework\v4*\MSBuild\
& $msbuild .\A11y.sln

.\bin\Debug\Microsoft.Edge.A11y.exe
