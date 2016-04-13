if(-Not (Test-Path .\nuget.exe)){
    (New-Object net.WebClient).DownloadFile('https://nuget.org/nuget.exe', 'nuget.exe')
}

if(-Not (Test-Path .\Interop.UIAutomationCore.dll)){
    Invoke-Expression -Command generate_dll.ps1
}

nuget.exe restore A11y.sln -Verbosity quiet

$msbuild = Get-Item C:\Windows\Microsoft.NET\Framework\v4*\MSBuild\
& $msbuild .\A11y.sln /v:q /nologo

.\bin\Debug\Microsoft.Edge.A11y.exe
