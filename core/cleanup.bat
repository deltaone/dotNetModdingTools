@echo off

rd /s /q src\_mod.core\obj
rd /s /q src\_mod.core-test\obj
rd /s /q src\_mod.injector\obj
rd /s /q src\_mod.install\obj
rd /s /q src\_mod.uninstall\obj
rd /s /q src\_mod.injector-test\obj

cd bin

del /Q *.config
del /Q *.vshost.exe
del /Q *.manifest
del /Q *.pdb
del /Q *.xml
del /Q *.suo
del /Q *.bak
del /Q *.modded
del /Q *.log

cd ..
cd ..

for /F "delims=" %%I in ('"dir /B /AD | findstr /B "mod-*""') do (	
	echo clean: %%I ...
	rd /s /q "%%I\_mods\obj"	
)

timeout /t 5
