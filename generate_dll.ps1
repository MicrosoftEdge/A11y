if([System.IntPtr]::Size -eq 8){ #64 bit
    & 'C:\Program Files (x86)\Microsoft SDKs\Windows\v8.1A\bin\NETFX 4.5.1 Tools\x64\TlbImp.exe' `
    C:\Windows\System32\UIAutomationCore.dll /out:Interop.UIAutomationCore.dll
}
else { #32 bit
    & 'C:\Program Files\Microsoft SDKs\Windows\v8.1A\bin\NETFX 4.5.1 Tools\TlbImp.exe' `
    C:\Windows\System32\UIAutomationCore.dll /out:Interop.UIAutomationCore.dll
}
