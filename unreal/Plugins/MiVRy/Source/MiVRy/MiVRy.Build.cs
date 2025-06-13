/*
 * MiVRy - VR gesture recognition library plug-in for Unreal.
 * Version 2.12
 * Copyright (c) 2025 MARUI-PlugIn (inc.)
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO,
 * THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
 * PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
 * EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 * PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY
 * OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
 * OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.IO;
using UnrealBuildTool;
// using Tools.DotNETCommon;

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
				// "InputCore",
                "EnhancedInput",
				"HeadMountedDisplay",
				"AIModule"
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
		} else if (Target.Platform == UnrealTargetPlatform.Android) {
            /*
			IAndroidToolChain ToolChain = AndroidExports.CreateToolChain(Target.ProjectFile);
            var Architectures = ToolChain.GetAllArchitectures();
            var ArchitectureDetected = false;
            foreach(var arch in Architectures) {
                if (arch.Contains("armv7")) {
                    // Log.TraceInformation("[MiVRy] Adding MiVRy library for armeabi-v7a.");
                    PublicAdditionalLibraries.Add("$(PluginDir)/Source/ThirdParty/MiVRy/Android/arm_32/libmivry.so");
                    ArchitectureDetected = true;
                    continue;
                }
                if (arch.Contains("arm64")) {
                    // Log.TraceInformation("[MiVRy] Adding MiVRy library for arm64-v8a.");
                    PublicAdditionalLibraries.Add("$(PluginDir)/Source/ThirdParty/MiVRy/Android/arm_64/libmivry.so");
                    ArchitectureDetected = true;
                    continue;
                }
                // Log.TraceInformation(string.Format("arch = {0}",arch));
            }
			if (ArchitectureDetected == false) {
                // Log.TraceInformation("[MiVRy] WARNING: Could not detect target architecture. Defaulting to arm64-v8a.");
                PublicAdditionalLibraries.Add("$(PluginDir)/Source/ThirdParty/MiVRy/Android/arm_64/libmivry.so");
            }
            */
            if (Target.Architecture == UnrealArch.Arm64) {
				PublicAdditionalLibraries.Add("$(PluginDir)/Source/ThirdParty/MiVRy/Android/arm_64/libmivry.so");
			} else if (Target.Architecture == UnrealArch.X64) {
				PublicAdditionalLibraries.Add("$(PluginDir)/Source/ThirdParty/MiVRy/Android/x86_64/libmivry.so");
			} else {
				if (Target.Architectures.Contains(UnrealArch.Arm64)) {
					PublicAdditionalLibraries.Add("$(PluginDir)/Source/ThirdParty/MiVRy/Android/arm_64/libmivry.so");
				} else if (Target.Architectures.Contains(UnrealArch.X64)) {
					PublicAdditionalLibraries.Add("$(PluginDir)/Source/ThirdParty/MiVRy/Android/x86_64/libmivry.so");
				} else {
                    // Log.TraceInformation("[MiVRy] WARNING: Could not detect target architecture. Defaulting to arm64-v8a.");
					PublicAdditionalLibraries.Add("$(PluginDir)/Source/ThirdParty/MiVRy/Android/arm_64/libmivry.so");
				}
            }
            string PluginPath = Utils.MakePathRelativeTo(ModuleDirectory, Target.RelativeEnginePath);
            AdditionalPropertiesForReceipt.Add("AndroidPlugin", Path.Combine(PluginPath, "MiVRy_APL.xml"));
		} else if (Target.Platform == UnrealTargetPlatform.Linux) {
			PublicRuntimeLibraryPaths.Add("$(PluginDir)/Source/ThirdParty/MiVRy/Linux/x86_64/");
			RuntimeDependencies.Add("$(PluginDir)/Source/ThirdParty/MiVRy/Linux/x86_64/libgesturerecognition.so");
		} else if (Target.Platform == UnrealTargetPlatform.LinuxArm64) {
			PublicRuntimeLibraryPaths.Add("$(PluginDir)/Source/ThirdParty/MiVRy/Linux/arm_64/");
			RuntimeDependencies.Add("$(PluginDir)/Source/ThirdParty/MiVRy/Linux/arm_64/libgesturerecognition.so");
		} else if (Target.Platform == UnrealTargetPlatform.Mac) {
			PublicAdditionalLibraries.Add("$(PluginDir)/Source/ThirdParty/MiVRy/MacOS/libgesturerecognition.dylib");
			RuntimeDependencies.Add("$(PluginDir)/Source/ThirdParty/MiVRy/MacOS/libgesturerecognition.dylib");
		}
	}
}
