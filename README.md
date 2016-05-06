# dotNetModdingTools
dotNetModdingTools is a universal platform for hooking managed function calls, you can easily create mods for .Net games/applications (including Unity3d / XNA and etc).

# Description
Installed hooks allow overriding hooked function's return values, essentially granting complete control over managed code execution in a targeted game/application.

# Features
 * Easily distribute your mods, in simple case - one .cs source file
 * Mods compile on user side through install utility
 * Intercept execution on begin/end of target function 
 * Allow overriding/analyse hooked function return value
 * Ease install/deinstall modifications for updating game/application
 * For example modded StarDrive2 / Terraria, look into code ... 

# Requirements
 * Microsoft.NET Framework v3.5 (for references)
 * Microsoft.NET Framework v4.0 (for compiler)
 
# Tips
 * You can use this bat file for update mod and run game, place this into ./core folder (don't forget to fix paths):
``` 
@echo off
set exe=C:\GAMES\StarDrive 2\SD2.exe
set dst=C:\GAMES\StarDrive 2\SD2_Data\Managed\
cd bin
_mod.injector.exe
copy Assembly-CSharp.dll.modded "%dst%\Assembly-CSharp.dll"
copy _mod.core.dll "%dst%\_mod.core.dll"
copy _mod.modules.dll "%dst%\_mod.modules.dll"
copy *.ini "%dst%"
cd ..
"%exe%" 
timeout /t 5 
```
 * When you have more than one mod, please disable non active mods projects in Visual Studio. Look here http://stackoverflow.com/questions/734573/preventing-visual-studio-from-building-all-projects-when-debugging-one ...

# Thanks 
 * UnityHook - https://github.com/HearthSim/UnityHook
 * Infinest Terraria hacks - http://www.mpgh.net/forum/showthread.php?t=752309 
 
# Technical notes
```
Hook method must be marked with attribute
	[RuntimeHook("assemby to patch", "type", "method", bool hookOnBegin)]
	if hookOnBegin = false - hook injected on end of function (by default hookOnBegin = true)
	on ex. [RuntimeHook("Assembly-CSharp.dll", "Planet", "CalculatePollution")]

Hook prototype
    [RuntimeHook("Terraria.exe", "Terraria.Player", "Update", false)]
    public static object Hook(object rv, object obj, params object[] args)
    {
		return(null);
	}
	
	
Return null from hook function for continue executing or return not null for immediate return from intercepted function
	injected code on void function:
		object[] args = new object[0];
		object obj = QuickSave.Save(methodof(Planet..cctor()).MethodHandle, null, args);
		if (obj != null)
		{
			return;
		}
	with intercepted argumrnts and int return type
		object[] array = new object[1];
		array[0] = GrossProduction; // function argument
		object obj = QuickSave.Load(methodof(Planet.CalculatePollution(float)).MethodHandle, this, array);
		if (obj != null)
		{
			return (int)((int)obj);
		}	
		
		
Incertept constuctor, names see into ILSpy ...
	[RuntimeHook("Assembly-CSharp.dll", "Planet", ".cctor")]
	[RuntimeHook("main.exe", "Core.ConfigFile", ".ctor", false)]        	
	
	
Get/Set private variable's value
	http://stackoverflow.com/questions/95910/find-a-private-field-with-reflection

	Get private variable's value using Reflection:
		var _barVariable = typeof(Foo).GetField("_bar", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(objectForFooClass);

	Set value for private variable using Reflection:
		typeof(Foo).GetField("_bar", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(objectForFoocClass, "newValue");
		
		
Execute hook function once after execute intercepted function
	Hook class variable:
		private bool intercept = true;
	Hook class body:	
		if (!intercept) return (null);
		// Perform the real call
		intercept = false;
		// public method		
		Network.BattlePayConfig battlePayConfigResponse = Network.GetBattlePayConfigResponse();
		return battlePayConfigResponse;
		// non public method
		MethodInfo method = this_.GetType().GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance);
		method.Invoke(this_, new object[] { });

		
Get caller info from RuntimeMethodHandle		
	var method = MethodBase.GetMethodFromHandle(rmh);
	var typeName = method.DeclaringType.FullName;
	var methodName = method.Name;
		
```

# mod-StarDrive2

# mod-Terraria
 * How to install: 
```
	1. Copy content of folder mod-Terraria to Terraria game folder
	2. execute _mod.install.exe
```
 * How to use:
```
	F - teleport to cursor
	ALT+1-0 - store teleport position
	1-0 - teleport to stored position
	Enter - open console - type command and press CTRL+Enter for executing command
		.craft - lets you craft anything
		.invsave - saves your current inventory to a file in the Terraria folder
		.invload - loads the inventory from the given file and overwrites your current one
		.flare - flashlight on mouse cursor (hold ALT for activation)
		.torch - light on player (invisible torch)
		.range - infinite building range (hold ALT for activation)
		.ruler - building grid
		.meteor - force spawn meteor
		.bloodmoon - starting bloodmoon event
		.invasion - starting invasion // Main.player[i].statLifeMax >= 200
		.eclipse - starting eclipse event
```	
	
# Required 3rd-party Binaries

To the mod-StarDrive2\sd2_data\managed directory, you must add the following 3rd-party binaries; these can be found in the {GameName}_Data/Managed folder of the game in question.

    UnityEngine.dll
    Assembly-CSharp.dll
    Assembly-CSharp-firstpass.dll

To the mod-Terraria directory, you must add terraria.exe from your Terraria gamefolder.
