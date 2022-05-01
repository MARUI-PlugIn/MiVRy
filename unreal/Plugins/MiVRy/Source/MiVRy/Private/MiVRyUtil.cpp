/*
 * MiVRy - VR gesture recognition library plug-in for Unreal.
 * Version 2.0
 * Copyright (c) 2022 MARUI-PlugIn (inc.)
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
            return "Invalid parameter(s) provided to function.";
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

void UMiVRyUtil::convertInput(const FVector& location, const FQuat& rotation, GestureRecognition_DeviceType device_type, GestureRecognition_CoordinateSystem coord_sys, double p[3], double q[4])
{
	if (coord_sys == GestureRecognition_CoordinateSystem::Unreal) {
		p[0] = location.X;
		p[1] = location.Y;
		p[2] = location.Z;
		q[0] = rotation.X;
		q[1] = rotation.Y;
		q[2] = rotation.Z;
		q[3] = rotation.W;
		return;
	}
	FQuat quaternion = FQuat(-0.5f, -0.5f, -0.5f, 0.5f) * rotation;
	switch (device_type) {
	case GestureRecognition_DeviceType::Headset:
		quaternion = quaternion * FQuat(0.5f, 0.5f, 0.5f, 0.5f);
		break;
	case GestureRecognition_DeviceType::Controller:
		switch (coord_sys) {
		case GestureRecognition_CoordinateSystem::UnityOpenXR:
		case GestureRecognition_CoordinateSystem::UnitySteamVR:
			quaternion = quaternion * FQuat(0, 0, 0.7071068f, 0.7071068f);
			break;
		case GestureRecognition_CoordinateSystem::UnityOculusVR:
			quaternion = quaternion * FQuat(0.5f, 0.5f, 0.5f, 0.5f);
			break;
		}
		break;
	}
	p[0] = location.Y * 0.01; // Unity.X = right = Unreal.Y
	p[1] = location.Z * 0.01; // Unity.Y = up    = Unreal.Z
	p[2] = location.X * 0.01; // Unity.Z = front = Unreal.X
	q[0] = quaternion.X;
	q[1] = quaternion.Y;
	q[2] = quaternion.Z;
	q[3] = quaternion.W;
}


void UMiVRyUtil::convertOutput(GestureRecognition_CoordinateSystem coord_sys, const double p[3], const double q[4], GestureRecognition_DeviceType device_type, FVector& location, FQuat& rotation)
{
	if (coord_sys == GestureRecognition_CoordinateSystem::Unreal) {
		location.X = (float)p[0];
		location.Y = (float)p[1];
		location.Z = (float)p[2];
		rotation.X = (float)q[0];
		rotation.Y = (float)q[1];
		rotation.Z = (float)q[2];
		rotation.W = (float)q[3];
		return;
	}
	location.X = 100.0f * (float)p[2]; // Unreal.X = front = Unity.Z
	location.Y = 100.0f * (float)p[0]; // Unreal.Y = right = Unity.X
	location.Z = 100.0f * (float)p[1]; // Unreal.Z = up    = Unity.Y
	rotation.X = (float)q[0]; // in Unity coordinates
	rotation.Y = (float)q[1];
	rotation.Z = (float)q[2];
	rotation.W = (float)q[3];
	rotation.Normalize();
	rotation = FQuat(0.5f, 0.5f, 0.5f, 0.5f) * rotation;
	switch (device_type) {
	case GestureRecognition_DeviceType::Headset:
		rotation = rotation * FQuat(-0.5f, -0.5f, -0.5f, 0.5f);
		break;
	case GestureRecognition_DeviceType::Controller:
		switch (coord_sys) {
		case GestureRecognition_CoordinateSystem::UnityOpenXR:
		case GestureRecognition_CoordinateSystem::UnitySteamVR:
			rotation = rotation * FQuat(0, 0, -0.7071068f, 0.7071068f);
			break;
		case GestureRecognition_CoordinateSystem::UnityOculusVR:
			rotation = rotation * FQuat(-0.5f, -0.5f, -0.5f, 0.5f);
			break;
		}
		break;
	}
}
