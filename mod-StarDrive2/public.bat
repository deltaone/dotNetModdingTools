@echo off
for %%* in (.) do set CurrentFolder=%%~nx*
del /q /a "..\%CurrentFolder%.public.zip"
..\7za.exe a -tzip -mx1 "..\%CurrentFolder%.public.zip" -xr!*.pdb -ir!"_mod" _mod.setup.exe _mod.core.dll _mod.core.public.ini >nul
..\7za.exe rn "..\%CurrentFolder%.public.zip" _mod.core.public.ini _mod.core.ini >nul
echo Done!
pause
