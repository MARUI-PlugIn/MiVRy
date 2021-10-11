// Copyright Epic Games, Inc. All Rights Reserved.

#include "MiVRy.h"
#include "Core.h"
#include "Modules/ModuleManager.h"
#include "Interfaces/IPluginManager.h"
#include "Components/InputComponent.h"
#include "Engine.h"
#include "Camera/PlayerCameraManager.h"
#include "GestureRecognition.h"

#define LOCTEXT_NAMESPACE "FMiVRyModule"

void FMiVRyModule::StartupModule()
{
	FString BaseDir = IPluginManager::Get().FindPlugin("MiVRy")->GetBaseDir();
	FString LibraryPath;
#if PLATFORM_WINDOWS
#ifdef _WIN64
	LibraryPath = FPaths::Combine(*BaseDir, TEXT("Source/ThirdParty/MiVRy/Windows/x86_64/GestureRecognition_Win_x86_64.dll"));
#else
	LibraryPath = FPaths::Combine(*BaseDir, TEXT("Source/ThirdParty/MiVRy/Windows/x86_32/GestureRecognition_Win_x86_32.dll"));
#endif
#elif PLATFORM_ANDROID
	return;
#elif PLATFORM_LINUX
#if __x86_64__
    LibraryPath = FPaths::Combine(*BaseDir, TEXT("Source/ThirdParty/MiVRy/Linux/x86_64/libgesturerecognition.so"));
#else
	LibraryPath = FPaths::Combine(*BaseDir, TEXT("Source/ThirdParty/MiVRy/Linux/x86_32/libgesturerecognition.so"));
#endif
#elif PLATFORM_WINRT
#ifdef _WIN64
	LibraryPath = FPaths::Combine(*BaseDir, TEXT("Source/ThirdParty/MiVRy/Hololens/arm_64/GestureRecognition_UWP_ARM_64.dll"));
#else
	LibraryPath = FPaths::Combine(*BaseDir, TEXT("Source/ThirdParty/MiVRy/Hololens/x86_32/GestureRecognition_UWP_x86_32.dll"));
#endif
#endif

	this->LibraryHandle = !LibraryPath.IsEmpty() ? FPlatformProcess::GetDllHandle(*LibraryPath) : nullptr;
	if (this->LibraryHandle == nullptr) {
		FMessageDialog::Open(EAppMsgType::Ok, LOCTEXT("MiVRy Plugin Error", "Failed to load MiVRy library"));
		return;
	}
}

void FMiVRyModule::ShutdownModule()
{
	if (this->LibraryHandle != nullptr) {
		FPlatformProcess::FreeDllHandle(this->LibraryHandle);
		this->LibraryHandle = nullptr;
	}
}

#undef LOCTEXT_NAMESPACE
	
IMPLEMENT_MODULE(FMiVRyModule, MiVRy)
