// Copyright Epic Games, Inc. All Rights Reserved.

using System;
using System.IO;
using UnrealBuildTool;
using Tools.DotNETCommon;

public class MiVRy : ModuleRules
{
	public MiVRy(ReadOnlyTargetRules Target) : base(Target)
	{
		PCHUsage = ModuleRules.PCHUsageMode.UseExplicitOrSharedPCHs;
		// PrecompileForTargets = PrecompileTargetsType.None;
		// bPrecompile = false;
		// bUsePrecompiled = false;

		PublicIncludePaths.AddRange(
			new string[] {
				// ...
			}
			);
				
		
		PrivateIncludePaths.AddRange(
			new string[] {
				Path.Combine(ModuleDirectory, "Private")
			}
			);
			
		
		PublicDependencyModuleNames.AddRange(
			new string[]
			{
				"Core",
				"Projects",
				"HeadMountedDisplay"
			}
			);
			
		
		PrivateDependencyModuleNames.AddRange(
			new string[]
			{
				"CoreUObject",
				"Engine"
			}
			);
		
		
		DynamicallyLoadedModuleNames.AddRange(
			new string[]
			{
				//
			}
			);

		if (Target.Platform == UnrealTargetPlatform.Win64) {
			PublicAdditionalLibraries.Add("$(PluginDir)/Source/ThirdParty/MiVRy/Windows/x86_64/GestureRecognition_Win_x86_64.lib");
			RuntimeDependencies.Add("$(PluginDir)/Source/ThirdParty/MiVRy/Windows/x86_64/GestureRecognition_Win_x86_64.dll");
			PublicDelayLoadDLLs.Add("GestureRecognition_Win_x86_64.dll");
		} else if (Target.Platform == UnrealTargetPlatform.Win32) {
			PublicAdditionalLibraries.Add("$(PluginDir)/Source/ThirdParty/MiVRy/Windows/x86_32/GestureRecognition_Win_x86_32.lib");
			RuntimeDependencies.Add("$(PluginDir)/Source/ThirdParty/MiVRy/Windows/x86_32/GestureRecognition_Win_x86_32.dll");
			PublicDelayLoadDLLs.Add("GestureRecognition_Win_x86_32.dll");
		} else if (Target.Platform == UnrealTargetPlatform.Android) {
			IAndroidToolChain ToolChain = AndroidExports.CreateToolChain(Target.ProjectFile);
            var Architectures = ToolChain.GetAllArchitectures();
            var ArchitectureDetected = false;
            foreach(var arch in Architectures) {
                if (arch.Contains("armv7")) {
                    Log.TraceInformation("[MiVRy] Adding MiVRy library for armeabi-v7a.");
                    PublicAdditionalLibraries.Add("$(PluginDir)/Source/ThirdParty/MiVRy/Android/arm_32/libmivry.so");
                    ArchitectureDetected = true;
                    continue;
                }
                if (arch.Contains("arm64")) {
                    Log.TraceInformation("[MiVRy] Adding MiVRy library for arm64-v8a.");
                    PublicAdditionalLibraries.Add("$(PluginDir)/Source/ThirdParty/MiVRy/Android/arm_64/libmivry.so");
                    ArchitectureDetected = true;
                    continue;
                }
                Log.TraceInformation(string.Format("arch = {0}",arch));
            }
			if (ArchitectureDetected == false) {
                Log.TraceInformation("[MiVRy] WARNING: Could not detect target architecture. Defaulting to arm64-v8a.");
                PublicAdditionalLibraries.Add("$(PluginDir)/Source/ThirdParty/MiVRy/Android/arm_64/libmivry.so");
            }
            string PluginPath = Utils.MakePathRelativeTo(ModuleDirectory, Target.RelativeEnginePath);
            AdditionalPropertiesForReceipt.Add("AndroidPlugin", Path.Combine(PluginPath, "MiVRy_APL.xml"));
		} else if (Target.Platform == UnrealTargetPlatform.HoloLens) {
			RuntimeDependencies.Add("$(PluginDir)/Source/ThirdParty/MiVRy/Hololens/arm_64/GestureRecognition_UWP_ARM_64.dll");
			PublicDelayLoadDLLs.Add("GestureRecognition_UWP_ARM_64.dll");
		} else if (Target.Platform == UnrealTargetPlatform.Linux) {
			PublicRuntimeLibraryPaths.Add("$(PluginDir)/Source/ThirdParty/MiVRy/Linux/x86_32/");
			RuntimeDependencies.Add("$(PluginDir)/Source/ThirdParty/MiVRy/Linux/x86_32/libgesturerecognition.so");
		} else if (Target.Platform == UnrealTargetPlatform.LinuxAArch64) {
			PublicRuntimeLibraryPaths.Add("$(PluginDir)/Source/ThirdParty/MiVRy/Linux/x86_64/");
			RuntimeDependencies.Add("$(PluginDir)/Source/ThirdParty/MiVRy/Linux/x86_64/libgesturerecognition.so");
		}
	}
}
