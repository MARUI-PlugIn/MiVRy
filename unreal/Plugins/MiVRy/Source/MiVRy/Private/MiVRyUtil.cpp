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
#include "Core.h"
#include <fstream>

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

void UMiVRyUtil::toUnityCoords(const FVector& location, const FQuat& quaternion, bool is_controller, double m[4][4])
{
    const FVector xaxis = quaternion.GetAxisX();
	const FVector yaxis = quaternion.GetAxisY();
	const FVector zaxis = quaternion.GetAxisZ();
	if (!is_controller) {
		m[0][0] = yaxis.Y;
		m[0][1] = yaxis.Z;
		m[0][2] = yaxis.X;
		m[0][3] = 0.0;
		m[1][0] = zaxis.Y;
		m[1][1] = zaxis.Z;
		m[1][2] = zaxis.X;
		m[1][3] = 0.0;
		m[2][0] = xaxis.Y;
		m[2][1] = xaxis.Z;
		m[2][2] = xaxis.X;
		m[2][3] = 0.0;
	} else {
		m[0][0] = yaxis.Y;
		m[0][1] = yaxis.Z;
		m[0][2] = yaxis.X;
		m[0][3] = 0.0;
		m[1][0] = -xaxis.Y;
		m[1][1] = -xaxis.Z;
		m[1][2] = -xaxis.X;
		m[1][3] = 0.0;
		m[2][0] = zaxis.Y;
		m[2][1] = zaxis.Z;
		m[2][2] = zaxis.X;
		m[2][3] = 0.0;
	}
	m[3][0] = location.Y * 0.01; // Unity.X = right = Unreal.Y
	m[3][1] = location.Z * 0.01; // Unity.Y = up    = Unreal.Z
	m[3][2] = location.X * 0.01; // Unity.Z = front = Unreal.X
	m[3][3] = 1.0;
}


void UMiVRyUtil::fromUnityCoords(const double p[3], const double q[4], bool is_controller, FVector& location, FQuat& quaternion)
{
	location.X = 100.0f * (float)p[2]; // Unreal.X = front = Unity.Z
	location.Y = 100.0f * (float)p[0]; // Unreal.Y = right = Unity.X
	location.Z = 100.0f * (float)p[1]; // Unreal.Z = up    = Unity.Y
	quaternion.X = (float)q[0]; // in Unity coordinates
	quaternion.Y = (float)q[1];
	quaternion.Z = (float)q[2];
	quaternion.W = (float)q[3];
	quaternion.Normalize();
	const FVector xaxis = quaternion.GetAxisX();
	const FVector yaxis = quaternion.GetAxisY();
	const FVector zaxis = quaternion.GetAxisZ();
	FMatrix m = FMatrix::Identity;
	if (is_controller) {
		m.M[0][0] = -yaxis.Z;
		m.M[0][1] = -yaxis.X;
		m.M[0][2] = -yaxis.Y;
		m.M[1][0] = xaxis.Z;
		m.M[1][1] = xaxis.X;
		m.M[1][2] = xaxis.Y;
		m.M[2][0] = zaxis.Z;
		m.M[2][1] = zaxis.X;
		m.M[2][2] = zaxis.Y;
	} else {
		m.M[0][0] = zaxis.Z;
		m.M[0][1] = zaxis.X;
		m.M[0][2] = zaxis.Y;
		m.M[1][0] = xaxis.Z;
		m.M[1][1] = xaxis.X;
		m.M[1][2] = xaxis.Y;
		m.M[2][0] = yaxis.Z;
		m.M[2][1] = yaxis.X;
		m.M[2][2] = yaxis.Y;
	}
	quaternion = m.ToQuat();
}
