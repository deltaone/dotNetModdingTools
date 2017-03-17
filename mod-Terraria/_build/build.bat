@echo off

echo Compile _mod.plugins.dll ...

if exist  "%WinDir%\Microsoft.NET\Framework\v4.0.30319\csc.exe" (
	set csc="%WinDir%\Microsoft.NET\Framework\v4.0.30319\csc.exe"	
) else (
	echo Microsoft.NET Framework v4.0 not found ...
	timeout /t 5	
	exit
)

%csc% @_mod.plugins.rsp

echo Done!
timeout /t 5