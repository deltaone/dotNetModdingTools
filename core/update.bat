@echo off

cd ..

for /F "delims=" %%I in ('"dir /B /AD | findstr /B "mod-*""') do (	
	echo update: %%I ...
	copy core\bin\_mod.install.exe %%I\
	copy core\bin\_mod.uninstall.exe %%I\
	copy core\bin\_mod.injector.exe %%I\_mods\managed\
	copy core\bin\_mod.core.dll %%I\_mods\managed\	
	copy core\refs\cecil\Mono.Cecil.dll %%I\_mods\managed\
	copy core\refs\cecil\Mono.Cecil.Rocks.dll %%I\_mods\managed\
)

echo Done!
timeout /t 5
