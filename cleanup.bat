@echo off

rd /s /q core\obj
rd /s /q core\bin

for %%X in (.pdb,.vshost.exe,.manifest,.log,.suo,.csproj.user) do del /s /q /a ".\*%%X"

for /F "delims=" %%I in ('"dir /B /AD | findstr /B "mod-*""') do (	
	echo cleanup: %%I ...
	for %%X in (_mod.core,_mod.plugins,_mod.setup,obj) do rd /s /q "%%I\_build\%%X"
	for %%X in (_mod.core.dll,_mod.setup.exe,_mod.plugins.dll) do del /s /q /a "%%I\%%X"
	rem for %%X in (.pdb,.vshost.exe,.manifest,.log,.suo) do del /s /q /a "%%I\*%%X"
)

echo Done!
pause
