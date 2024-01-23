/*
 * MiVRy - VR gesture recognition library plug-in for Unreal.
 * Version 2.10
 * Copyright (c) 2024 MARUI-PlugIn (inc.)
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

#include "MiVRyUtil.h"
#include "GestureRecognition.h"
#include "Core.h"
#include <fstream>

FString UMiVRyUtil::versionString()
{
	return FString(IGestureRecognition::getVersionString());
}

FString UMiVRyUtil::errorCodeToString(int errorCode)
{
    switch (errorCode)
    {
        case 0:
            return "Function executed successfully.";
        case -1:
            return "No gesture (or combination) matches.";
        case -2:
            return "Invalid index provided to function.";
        case -3:
            return "Invalid file path provided to function.";
        case -4:
            return "Path to an invalid file provided to function.";
        case -5:
            return "Calculations failed due to numeric instability(too small or too large numbers).";
        case -6:
            return "The internal state of the AI was corrupted.";
        case -7:
            return "Available data(number of samples etc) is insufficient for this operation.";
        case -8:
            return "The operation could not be performed because the AI is currently training.";
        case -9:
            return "No gestures registered.";
        case -10:
            return "The neural network is inconsistent - re-training might solve the issue.";
        case -11:
            return "File or object exists and can't be overwritten.";
		case -12:
			return "Gesture performance (gesture motion, stroke) was not started yet (missing startStroke()).";
		case -13:
			return "Gesture performance (gesture motion, stroke) was not finished yet (missing endStroke()).";
		case -14:
			return "The gesture recognition/combinations object is internally corrupted or inconsistent.";
		case -15:
			return "The operation could not be performed because the AI is loading a gesture database file.";
		case -16:
			return "The provided license key is not valid or the operation is not permitted under the current license.";
		case -17:
			return "The operation could not be performed because the AI is currently being saved to a database file.";
		case -18:
			return "Invalid parameter(s) provided to function.";
		case -19:
			return "Input/output failure.";
		case -99:
			return "Gesture Recognition object was not created.";
    }
    return "Unknown error.";
}

void UMiVRyUtil::findFile(const FString& Path, GestureRecognition_Result& Result, FString& FoundPath)
{
	IFileManager& fm = IFileManager::Get();
	std::ifstream ifs;
	FoundPath = Path;
	ifs.open(TCHAR_TO_ANSI(*FoundPath), std::ios_base::in | std::ios_base::binary);
	if (ifs.is_open()) {
		Result = GestureRecognition_Result::Then;
		ifs.close();
		return;
	}
	FoundPath = FPaths::ConvertRelativePathToFull(FoundPath);
	ifs.open(TCHAR_TO_ANSI(*FoundPath), std::ios_base::in | std::ios_base::binary);
	if (ifs.is_open()) {
		Result = GestureRecognition_Result::Then;
		ifs.close();
		return;
	}
	FoundPath = fm.ConvertToAbsolutePathForExternalAppForRead(*FoundPath);
	ifs.open(TCHAR_TO_ANSI(*FoundPath), std::ios_base::in | std::ios_base::binary);
	if (ifs.is_open()) {
		Result = GestureRecognition_Result::Then;
		ifs.close();
		return;
	}

	FoundPath = FPaths::Combine(FPaths::ProjectDir(), Path);
	ifs.open(TCHAR_TO_ANSI(*FoundPath), std::ios_base::in | std::ios_base::binary);
	if (ifs.is_open()) {
		Result = GestureRecognition_Result::Then;
		ifs.close();
		return;
	}
	FoundPath = FPaths::ConvertRelativePathToFull(FoundPath);
	ifs.open(TCHAR_TO_ANSI(*FoundPath), std::ios_base::in | std::ios_base::binary);
	if (ifs.is_open()) {
		Result = GestureRecognition_Result::Then;
		ifs.close();
		return;
	}
	FoundPath = fm.ConvertToAbsolutePathForExternalAppForRead(*FoundPath);
	ifs.open(TCHAR_TO_ANSI(*FoundPath), std::ios_base::in | std::ios_base::binary);
	if (ifs.is_open()) {
		Result = GestureRecognition_Result::Then;
		ifs.close();
		return;
	}

	FoundPath = FPaths::Combine(FPaths::ProjectContentDir(), Path);
	ifs.open(TCHAR_TO_ANSI(*FoundPath), std::ios_base::in | std::ios_base::binary);
	if (ifs.is_open()) {
		Result = GestureRecognition_Result::Then;
		ifs.close();
		return;
	}
	FoundPath = FPaths::ConvertRelativePathToFull(FoundPath);
	ifs.open(TCHAR_TO_ANSI(*FoundPath), std::ios_base::in | std::ios_base::binary);
	if (ifs.is_open()) {
		Result = GestureRecognition_Result::Then;
		ifs.close();
		return;
	}
	FoundPath = fm.ConvertToAbsolutePathForExternalAppForRead(*FoundPath);
	ifs.open(TCHAR_TO_ANSI(*FoundPath), std::ios_base::in | std::ios_base::binary);
	if (ifs.is_open()) {
		Result = GestureRecognition_Result::Then;
		ifs.close();
		return;
	}

	FoundPath = FPaths::Combine(FPaths::ProjectPluginsDir(), Path);
	ifs.open(TCHAR_TO_ANSI(*FoundPath), std::ios_base::in | std::ios_base::binary);
	if (ifs.is_open()) {
		Result = GestureRecognition_Result::Then;
		ifs.close();
		return;
	}
	FoundPath = FPaths::ConvertRelativePathToFull(FoundPath);
	ifs.open(TCHAR_TO_ANSI(*FoundPath), std::ios_base::in | std::ios_base::binary);
	if (ifs.is_open()) {
		Result = GestureRecognition_Result::Then;
		ifs.close();
		return;
	}
	FoundPath = fm.ConvertToAbsolutePathForExternalAppForRead(*FoundPath);
	ifs.open(TCHAR_TO_ANSI(*FoundPath), std::ios_base::in | std::ios_base::binary);
	if (ifs.is_open()) {
		Result = GestureRecognition_Result::Then;
		ifs.close();
		return;
	}

	FoundPath = FPaths::Combine(FPaths::ProjectPersistentDownloadDir(), Path);
	ifs.open(TCHAR_TO_ANSI(*FoundPath), std::ios_base::in | std::ios_base::binary);
	if (ifs.is_open()) {
		Result = GestureRecognition_Result::Then;
		ifs.close();
		return;
	}
	FoundPath = FPaths::ConvertRelativePathToFull(FoundPath);
	ifs.open(TCHAR_TO_ANSI(*FoundPath), std::ios_base::in | std::ios_base::binary);
	if (ifs.is_open()) {
		Result = GestureRecognition_Result::Then;
		ifs.close();
		return;
	}
	FoundPath = fm.ConvertToAbsolutePathForExternalAppForRead(*FoundPath);
	ifs.open(TCHAR_TO_ANSI(*FoundPath), std::ios_base::in | std::ios_base::binary);
	if (ifs.is_open()) {
		Result = GestureRecognition_Result::Then;
		ifs.close();
		return;
	}

	FoundPath = FPaths::Combine(FPaths::ProjectUserDir(), Path);
	ifs.open(TCHAR_TO_ANSI(*FoundPath), std::ios_base::in | std::ios_base::binary);
	if (ifs.is_open()) {
		Result = GestureRecognition_Result::Then;
		ifs.close();
		return;
	}
	FoundPath = FPaths::ConvertRelativePathToFull(FoundPath);
	ifs.open(TCHAR_TO_ANSI(*FoundPath), std::ios_base::in | std::ios_base::binary);
	if (ifs.is_open()) {
		Result = GestureRecognition_Result::Then;
		ifs.close();
		return;
	}
	FoundPath = fm.ConvertToAbsolutePathForExternalAppForRead(*FoundPath);
	ifs.open(TCHAR_TO_ANSI(*FoundPath), std::ios_base::in | std::ios_base::binary);
	if (ifs.is_open()) {
		Result = GestureRecognition_Result::Then;
		ifs.close();
		return;
	}
	FoundPath = Path;
	Result = GestureRecognition_Result::Error;
}

void UMiVRyUtil::readFileToBuffer(const FString& path, GestureRecognition_Result& result, TArray<uint8>& data)
{
	if (FFileHelper::LoadFileToArray(data, *path)) {
		result = GestureRecognition_Result::Then;
		return;
	}
	if (!FPaths::IsRelative(path)) {
		result = GestureRecognition_Result::Error;
		return;
	}
	FString relpath = FPaths::Combine(FPaths::ProjectDir(), path);
	if (FFileHelper::LoadFileToArray(data, *relpath)) {
		result = GestureRecognition_Result::Then;
		return;
	}
	FString abspath = FPaths::ConvertRelativePathToFull(relpath);
	if (FFileHelper::LoadFileToArray(data, *abspath)) {
		result = GestureRecognition_Result::Then;
		return;
	}
	result = GestureRecognition_Result::Error;
}

const FQuat RotateYp55(0, 0.4617486f,  0, 0.8870108f); // Controller rotation: OpenXR -> OculusVR
const FQuat RotateYm55(0, -0.4617486f, 0, 0.8870108f); // Controller rotation: OculusVR -> OpenXR
const FQuat RotateYp25(0, 0.2164396f,  0, 0.976296f ); // Controller rotation: SteamVR -> OculusVR
const FQuat RotateYm25(0, -0.2164396f, 0, 0.976296f ); // Controller rotation: OculusVR -> SteamVR
const FQuat RotateYp20(0, 0.258819f,   0, 0.9659258f); // Controller rotation: OpenXR -> SteamVR
const FQuat RotateYm20(0, -0.258819f,  0, 0.9659258f); // Controller rotation: SteamVR -> OpenXR
const FQuat RotateXZY( 0.5f,  0.5f,       0.5f,       0.5f);
const FQuat RotateXYZ(-0.5f, -0.5f,      -0.5f,       0.5f);

void UMiVRyUtil::convertInput(const FVector& location, const FQuat& rotation_in, GestureRecognition_DeviceType device_type, GestureRecognition_VRPlugin vr_plugin, GestureRecognition_CoordinateSystem coord_sys, double p[3], double q[4])
{
	FQuat rotation = rotation_in;
    if (vr_plugin == GestureRecognition_VRPlugin::OculusVR) {
        switch (coord_sys) {
		case GestureRecognition_CoordinateSystem::Unreal_OculusVR:
		case GestureRecognition_CoordinateSystem::Unreal_OpenXR:
		case GestureRecognition_CoordinateSystem::Unreal_SteamVR:
            p[0] = location.X;
            p[1] = location.Y;
            p[2] = location.Z;
			if (device_type == GestureRecognition_DeviceType::Controller) {
				if (coord_sys == GestureRecognition_CoordinateSystem::Unreal_OpenXR) {
					rotation = rotation * RotateYm55; // Unreal_OculusVR -> Unreal_OpenXR
				} else if (coord_sys == GestureRecognition_CoordinateSystem::Unreal_SteamVR) {
					rotation = rotation * RotateYm25; // Unreal_OculusVR -> Unreal_SteamVR
				}
			}
			q[0] = rotation.X;
			q[1] = rotation.Y;
			q[2] = rotation.Z;
			q[3] = rotation.W;
            return;
        }
        // else: one of the Unity coordinate systems
		rotation = RotateXYZ * rotation;
        switch (device_type) {
        case GestureRecognition_DeviceType::Headset:
			rotation = rotation * RotateXZY;
            break;
        case GestureRecognition_DeviceType::Controller:
            switch (coord_sys) {
            case GestureRecognition_CoordinateSystem::Unity_OpenXR:
				rotation = rotation * RotateYm55 * RotateXZY; // Unreal_OculusVR -> Unity_OpenXR
				break;
            case GestureRecognition_CoordinateSystem::Unity_SteamVR:
				rotation = rotation * RotateYm25 * RotateXZY; // Unreal_OculusVR -> Unity_SteamVR
                break;
            case GestureRecognition_CoordinateSystem::Unity_OculusVR:
				rotation = rotation * RotateXZY; // Unreal_OculusVR -> Unreal_OculusVR
                break;
            }
            break;
        }
        p[0] = location.Y * 0.01; // Unity.X = right = Unreal.Y
        p[1] = location.Z * 0.01; // Unity.Y = up    = Unreal.Z
        p[2] = location.X * 0.01; // Unity.Z = front = Unreal.X
        q[0] = rotation.X;
        q[1] = rotation.Y;
        q[2] = rotation.Z;
        q[3] = rotation.W;
        return;
    } else
	if (vr_plugin == GestureRecognition_VRPlugin::SteamVR) {
		switch (coord_sys) {
		case GestureRecognition_CoordinateSystem::Unreal_SteamVR:
		case GestureRecognition_CoordinateSystem::Unreal_OculusVR:
		case GestureRecognition_CoordinateSystem::Unreal_OpenXR:
            p[0] = location.X;
            p[1] = location.Y;
            p[2] = location.Z;
			if (device_type == GestureRecognition_DeviceType::Controller) {
				if (coord_sys == GestureRecognition_CoordinateSystem::Unreal_OpenXR) {
					rotation = rotation * RotateYm20; // Unreal_SteamVR -> Unreal_OpenXR
				} else if (coord_sys == GestureRecognition_CoordinateSystem::Unreal_OculusVR) {
					rotation = rotation * RotateYp25; // Unreal_SteamVR -> Unreal_OculusVR
				}
			}
			q[0] = rotation.X;
			q[1] = rotation.Y;
			q[2] = rotation.Z;
			q[3] = rotation.W;
            return;
        }
        // else: one of the Unity coordinate systems
		rotation = RotateXYZ * rotation;
        switch (device_type) {
        case GestureRecognition_DeviceType::Headset:
			rotation = rotation * RotateXZY;
            break;
        case GestureRecognition_DeviceType::Controller:
            switch (coord_sys) {
            case GestureRecognition_CoordinateSystem::Unity_OpenXR:
				rotation = rotation * RotateYm20 * RotateXZY; // Unreal_SteamVR -> Unity_OpenXR
				break;
            case GestureRecognition_CoordinateSystem::Unity_SteamVR:
				rotation = rotation * RotateXZY; // Unreal_SteamVR -> Unity_SteamVR
                break;
            case GestureRecognition_CoordinateSystem::Unity_OculusVR:
				rotation = rotation * RotateYp25 * RotateXZY; // Unreal_SteamVR -> Unity_OculusVR
                break;
            }
            break;
        }
        p[0] = location.Y * 0.01; // Unity.X = right = Unreal.Y
        p[1] = location.Z * 0.01; // Unity.Y = up    = Unreal.Z
        p[2] = location.X * 0.01; // Unity.Z = front = Unreal.X
        q[0] = rotation.X;
        q[1] = rotation.Y;
        q[2] = rotation.Z;
        q[3] = rotation.W;
        return;
	} else { // (vr_plugin == GestureRecognition_VRPlugin::Unreal_OpenXR)
		switch (coord_sys) {
		case GestureRecognition_CoordinateSystem::Unreal_OpenXR:
		case GestureRecognition_CoordinateSystem::Unreal_SteamVR:
		case GestureRecognition_CoordinateSystem::Unreal_OculusVR:
            p[0] = location.X;
            p[1] = location.Y;
            p[2] = location.Z;
			if (device_type == GestureRecognition_DeviceType::Controller) {
				if (coord_sys == GestureRecognition_CoordinateSystem::Unreal_SteamVR) {
					rotation = rotation * RotateYp20; // Unreal_OpenXR -> Unreal_SteamVR
				} else if (coord_sys == GestureRecognition_CoordinateSystem::Unreal_OculusVR) {
					rotation = rotation * RotateYp55; // Unreal_OpenXR -> Unreal_OculusVR
				}
			}
			q[0] = rotation.X;
			q[1] = rotation.Y;
			q[2] = rotation.Z;
			q[3] = rotation.W;
            return;
        }
        // else: one of the Unity coordinate systems
		rotation = RotateXYZ * rotation;
        switch (device_type) {
        case GestureRecognition_DeviceType::Headset:
			rotation = rotation * RotateXZY;
            break;
        case GestureRecognition_DeviceType::Controller:
            switch (coord_sys) {
            case GestureRecognition_CoordinateSystem::Unity_OpenXR:
				rotation = rotation * RotateXZY; // Unreal_OpenXR -> Unity_OpenXR
				break;
            case GestureRecognition_CoordinateSystem::Unity_SteamVR:
				rotation = rotation * RotateYp20 * RotateXZY; // Unreal_OpenXR -> Unity_SteamVR
                break;
            case GestureRecognition_CoordinateSystem::Unity_OculusVR:
				rotation = rotation * RotateYp55 * RotateXZY; // Unreal_OpenXR -> Unity_OculusVR
                break;
            }
            break;
        }
        p[0] = location.Y * 0.01; // Unity.X = right = Unreal.Y
        p[1] = location.Z * 0.01; // Unity.Y = up    = Unreal.Z
        p[2] = location.X * 0.01; // Unity.Z = front = Unreal.X
        q[0] = rotation.X;
        q[1] = rotation.Y;
        q[2] = rotation.Z;
        q[3] = rotation.W;
        return;
	}
}


void UMiVRyUtil::convertOutput(GestureRecognition_VRPlugin vr_plugin, GestureRecognition_CoordinateSystem coord_sys, const double p[3], const double q[4], GestureRecognition_DeviceType device_type, FVector& location, FQuat& rotation)
{
    if (vr_plugin == GestureRecognition_VRPlugin::OculusVR) {
        switch (coord_sys) {
		case GestureRecognition_CoordinateSystem::Unreal_OculusVR:
		case GestureRecognition_CoordinateSystem::Unreal_OpenXR:
		case GestureRecognition_CoordinateSystem::Unreal_SteamVR:
            location.X = (float)p[0];
            location.Y = (float)p[1];
            location.Z = (float)p[2];
            rotation.X = (float)q[0];
            rotation.Y = (float)q[1];
            rotation.Z = (float)q[2];
            rotation.W = (float)q[3];
            if (device_type == GestureRecognition_DeviceType::Controller) {
				if (coord_sys == GestureRecognition_CoordinateSystem::Unreal_OpenXR) {
					rotation = rotation * RotateYp55; // Unreal_OpenXR -> Unreal_OculusVR
				} else if (coord_sys == GestureRecognition_CoordinateSystem::Unreal_SteamVR) {
					rotation = rotation * RotateYp25; // Unreal_SteamVR -> Unreal_OculusVR
				}
            }
            return;
        }
        // else: one of the Unity coordinate systems
        location.X = 100.0f * (float)p[2]; // Unreal.X = front = Unity.Z
        location.Y = 100.0f * (float)p[0]; // Unreal.Y = right = Unity.X
        location.Z = 100.0f * (float)p[1]; // Unreal.Z = up    = Unity.Y
        rotation.X = (float)q[0]; // in Unity coordinates
        rotation.Y = (float)q[1];
        rotation.Z = (float)q[2];
        rotation.W = (float)q[3];
        rotation.Normalize();
        rotation = RotateXZY * rotation;
        switch (device_type) {
        case GestureRecognition_DeviceType::Headset:
            rotation = rotation * RotateXYZ;
            break;
        case GestureRecognition_DeviceType::Controller:
            switch (coord_sys) {
            case GestureRecognition_CoordinateSystem::Unity_OpenXR:
				rotation = rotation * RotateXYZ * RotateYp55; // Unity_OpenXR -> Unreal_OculusVR
				break;
            case GestureRecognition_CoordinateSystem::Unity_SteamVR:
                rotation = rotation * RotateXYZ * RotateYp25; // Unity_SteamVR -> Unreal_OculusVR
                break;
            case GestureRecognition_CoordinateSystem::Unity_OculusVR:
                rotation = rotation * RotateXYZ; // Unity_OculusVR -> Unreal_OculusVR
                break;
            }
            break;
        }
        return;
    } else
	if (vr_plugin == GestureRecognition_VRPlugin::SteamVR) {
		switch (coord_sys) {
		case GestureRecognition_CoordinateSystem::Unreal_OculusVR:
		case GestureRecognition_CoordinateSystem::Unreal_OpenXR:
		case GestureRecognition_CoordinateSystem::Unreal_SteamVR:
            location.X = (float)p[0];
            location.Y = (float)p[1];
            location.Z = (float)p[2];
            rotation.X = (float)q[0];
            rotation.Y = (float)q[1];
            rotation.Z = (float)q[2];
            rotation.W = (float)q[3];
            if (device_type == GestureRecognition_DeviceType::Controller) {
				if (coord_sys == GestureRecognition_CoordinateSystem::Unreal_OpenXR) {
					rotation = rotation * RotateYp20; // Unreal_OpenXR -> Unreal_SteamVR
				} else if (coord_sys == GestureRecognition_CoordinateSystem::Unreal_OculusVR) {
					rotation = rotation * RotateYm25; // Unreal_OculusVR -> Unreal_SteamVR
				}
            }
            return;
        }
        // else: one of the Unity coordinate systems
        location.X = 100.0f * (float)p[2]; // Unreal.X = front = Unity.Z
        location.Y = 100.0f * (float)p[0]; // Unreal.Y = right = Unity.X
        location.Z = 100.0f * (float)p[1]; // Unreal.Z = up    = Unity.Y
        rotation.X = (float)q[0]; // in Unity coordinates
        rotation.Y = (float)q[1];
        rotation.Z = (float)q[2];
        rotation.W = (float)q[3];
        rotation.Normalize();
        rotation = RotateXZY * rotation;
        switch (device_type) {
        case GestureRecognition_DeviceType::Headset:
            rotation = rotation * RotateXYZ;
            break;
        case GestureRecognition_DeviceType::Controller:
            switch (coord_sys) {
            case GestureRecognition_CoordinateSystem::Unity_OpenXR:
				rotation = rotation * RotateXYZ * RotateYp20; // Unity_OpenXR -> Unreal_SteamVR
				break;
            case GestureRecognition_CoordinateSystem::Unity_SteamVR:
                rotation = rotation * RotateXYZ; // Unity_SteamVR -> Unreal_SteamVR
                break;
            case GestureRecognition_CoordinateSystem::Unity_OculusVR:
                rotation = rotation * RotateXYZ * RotateYm25; // Unity_OculusVR -> Unreal_SteamVR
                break;
            }
            break;
        }
        return;
	} else { // vr_plugin == GestureRecognition_VRPlugin::OpenXR
		switch (coord_sys) {
		case GestureRecognition_CoordinateSystem::Unreal_OculusVR:
		case GestureRecognition_CoordinateSystem::Unreal_OpenXR:
		case GestureRecognition_CoordinateSystem::Unreal_SteamVR:
            location.X = (float)p[0];
            location.Y = (float)p[1];
            location.Z = (float)p[2];
            rotation.X = (float)q[0];
            rotation.Y = (float)q[1];
            rotation.Z = (float)q[2];
            rotation.W = (float)q[3];
            if (device_type == GestureRecognition_DeviceType::Controller) {
				if (coord_sys == GestureRecognition_CoordinateSystem::Unreal_SteamVR) {
					rotation = rotation * RotateYm20; // Unreal_SteamVR -> Unreal_OpenXR
				} else if (coord_sys == GestureRecognition_CoordinateSystem::Unreal_OculusVR) {
					rotation = rotation * RotateYm55; // Unreal_OculusVR -> Unreal_OpenXR
				}
            }
            return;
        }
        // else: one of the Unity coordinate systems
        location.X = 100.0f * (float)p[2]; // Unreal.X = front = Unity.Z
        location.Y = 100.0f * (float)p[0]; // Unreal.Y = right = Unity.X
        location.Z = 100.0f * (float)p[1]; // Unreal.Z = up    = Unity.Y
        rotation.X = (float)q[0]; // in Unity coordinates
        rotation.Y = (float)q[1];
        rotation.Z = (float)q[2];
        rotation.W = (float)q[3];
        rotation.Normalize();
        rotation = RotateXZY * rotation;
        switch (device_type) {
        case GestureRecognition_DeviceType::Headset:
            rotation = rotation * RotateXYZ;
            break;
        case GestureRecognition_DeviceType::Controller:
            switch (coord_sys) {
            case GestureRecognition_CoordinateSystem::Unity_OpenXR:
				rotation = rotation * RotateXYZ; // Unity_OpenXR -> Unreal_OpenXR
				break;
            case GestureRecognition_CoordinateSystem::Unity_SteamVR:
                rotation = rotation * RotateXYZ * RotateYm20; // Unity_SteamVR -> Unreal_OpenXR
                break;
            case GestureRecognition_CoordinateSystem::Unity_OculusVR:
                rotation = rotation * RotateXYZ * RotateYm55; // Unity_OculusVR -> Unreal_OpenXR
                break;
            }
            break;
        }
        return;
	}
}
