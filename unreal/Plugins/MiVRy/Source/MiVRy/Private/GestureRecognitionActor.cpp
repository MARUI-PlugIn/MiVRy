/*
 * MiVRy - VR gesture recognition library plug-in for Unreal.
 * Version 2.9
 * Copyright (c) 2023 MARUI-PlugIn (inc.)
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

#include "GestureRecognitionActor.h"
#include "Misc/MessageDialog.h"
#include "GestureRecognition.h"
#include "Async/Async.h"

AGestureRecognitionActor::AGestureRecognitionActor()
{
	PrimaryActorTick.bCanEverTick = true;

	TrainingUpdateMetadata.actor = this;
	TrainingUpdateMetadata.delegate = &this->OnTrainingUpdateDelegate;
	TrainingFinishMetadata.actor = this;
	TrainingFinishMetadata.delegate = &this->OnTrainingFinishDelegate;
	LoadingFinishMetadata.actor = this;
	LoadingFinishMetadata.delegate = &this->OnLoadingFinishDelegate;
}

AGestureRecognitionActor::~AGestureRecognitionActor()
{
	if (this->gro != nullptr) {
		delete this->gro;
		this->gro = nullptr;
	}
}

void AGestureRecognitionActor::BeginPlay()
{
	Super::BeginPlay();

	this->gro = (IGestureRecognition*)GestureRecognition_create();
	if (this->gro == nullptr) {
		FMessageDialog::Open(EAppMsgType::Ok, FText::FromString("GestureRecognition::create returned null"));
		return;
	}
	switch (this->MivryCoordinateSystem) {
	case GestureRecognition_CoordinateSystem::Unreal_OculusVR:
	case GestureRecognition_CoordinateSystem::Unreal_OpenXR:
	case GestureRecognition_CoordinateSystem::Unreal_SteamVR:
	default:
		this->gro->rotationalFrameOfReference.rotationOrder = IGestureRecognition::RotationOrder::ZYX;
		break;
	case GestureRecognition_CoordinateSystem::Unity_OculusVR:
	case GestureRecognition_CoordinateSystem::Unity_OpenXR:
	case GestureRecognition_CoordinateSystem::Unity_SteamVR:
		this->gro->rotationalFrameOfReference.rotationOrder = IGestureRecognition::RotationOrder::YXZ;
	}
	if (this->LicenseName.IsEmpty() == false) {
		auto license_name = StringCast<ANSICHAR>(*this->LicenseName);
		auto license_key = StringCast<ANSICHAR>(*this->LicenseKey);
		int ret = this->gro->activateLicense(license_name.Get(), license_key.Get());
		if (ret != 0) {
			FMessageDialog::Open(EAppMsgType::Ok, FText::FromString(FString("Failed to activate license: ") + UMiVRyUtil::errorCodeToString(ret)));
		}
	}
}

void AGestureRecognitionActor::Tick(float DeltaTime)
{
	Super::Tick(DeltaTime);
}

int AGestureRecognitionActor::activateLicense(const FString& license_name, const FString& license_key)
{
	if (this->gro == nullptr) {
		return -99;
	}
	auto license_name_str = StringCast<ANSICHAR>(*license_name);
	auto license_key_str = StringCast<ANSICHAR>(*license_key);
	return this->gro->activateLicense(license_name_str.Get(), license_key_str.Get());
}

int AGestureRecognitionActor::activateLicenseFile(const FString& license_file_path)
{
	if (this->gro == nullptr) {
		return -99;
	}
	auto license_file_path_str = StringCast<ANSICHAR>(*license_file_path);
	return this->gro->activateLicenseFile(license_file_path_str.Get());
}

int AGestureRecognitionActor::startStroke(const FVector& HMD_Position, const FRotator& HMD_Rotation, int RecordAsSample)
{
	return this->startStrokeQ(HMD_Position, HMD_Rotation.Quaternion(), RecordAsSample);
}

int AGestureRecognitionActor::startStrokeQ(const FVector& HMD_Position, const FQuat& HMD_Rotation, int RecordAsSample)
{
	if (this->gro == nullptr) {
		return -99;
	}
	double p[3];
	double q[4];
	UMiVRyUtil::convertInput(HMD_Position, HMD_Rotation, GestureRecognition_DeviceType::Headset, this->UnrealVRPlugin, this->MivryCoordinateSystem, p, q);
	return this->gro->startStroke(p, q, RecordAsSample);
}

int AGestureRecognitionActor::updateHeadPosition(const FVector& HMD_Position, const FRotator& HMD_Rotation)
{
	return this->updateHeadPositionQ(HMD_Position, HMD_Rotation.Quaternion());
}

int AGestureRecognitionActor::updateHeadPositionQ(const FVector& HMD_Position, const FQuat& HMD_Rotation)
{
	if (this->gro == nullptr) {
		return -99;
	}
	double p[3];
	double q[4];
	UMiVRyUtil::convertInput(HMD_Position, HMD_Rotation, GestureRecognition_DeviceType::Headset, this->UnrealVRPlugin, this->MivryCoordinateSystem, p, q);
	return this->gro->updateHeadPositionQ(p, q);
}

int AGestureRecognitionActor::contdStroke(const FVector& HandPosition, const FRotator& HandRotation)
{
	return this->contdStrokeQ(HandPosition, HandRotation.Quaternion());
}

int AGestureRecognitionActor::contdStrokeQ(const FVector& HandPosition, const FQuat& HandRotation)
{
	if (!this->gro) {
		return -99;
	}
	double p[3];
	double q[4];
	UMiVRyUtil::convertInput(HandPosition, HandRotation, GestureRecognition_DeviceType::Controller, this->UnrealVRPlugin, this->MivryCoordinateSystem, p, q);
	return this->gro->contdStrokeQ(p, q);
}

int AGestureRecognitionActor::endStroke(float& Similarity, FVector& GesturePosition, FRotator& GestureRotation, float& GestureScale)
{
	FQuat q;
	int gesture_id = this->endStrokeQ(Similarity, GesturePosition, q, GestureScale);
	GestureRotation = q.Rotator();
	return gesture_id;
}

int AGestureRecognitionActor::endStrokeQ(float& Similarity, FVector& GesturePosition, FQuat& GestureRotation, float& GestureScale)
{
	if (!this->gro) {
		return -99;
	}
	double similarity = -1;
	double pos[3] = {0,0,0};
	double scale = 1;
	double dir0[3] = {1,0,0};
	double dir1[3] = {0,1,0};
	double dir2[3] = {0,0,1};
	int gesture_id = this->gro->endStrokeAndGetSimilarity(&similarity, pos, &scale, dir0, dir1, dir2);
	Similarity = similarity;
	FRotationMatrix rm(FRotator(0, 0, 0));
	switch (this->MivryCoordinateSystem) {
	case GestureRecognition_CoordinateSystem::Unity_OculusVR:
	case GestureRecognition_CoordinateSystem::Unity_OpenXR:
	case GestureRecognition_CoordinateSystem::Unity_SteamVR:
		GesturePosition.X = float(pos[2]) * 100.0f; // Unreal.X = front = Unity.Z
		GesturePosition.Y = float(pos[0]) * 100.0f; // Unreal.Y = right = Unity.X
		GesturePosition.Z = float(pos[1]) * 100.0f; // Unreal.Z = up    = Unity.Y
		GestureScale = float(scale) * 100.0f;
		rm.M[0][0] = (float)dir0[2]; // Unreal.X = front = Unity.Z
		rm.M[0][1] = (float)dir0[0]; // Unreal.Y = right = Unity.X
		rm.M[0][2] = (float)dir0[1]; // Unreal.Z = up    = Unity.Y
		rm.M[1][0] = (float)dir1[2];
		rm.M[1][1] = (float)dir1[0];
		rm.M[1][2] = (float)dir1[1];
		rm.M[2][0] = (float)dir2[2];
		rm.M[2][1] = (float)dir2[0];
		rm.M[2][2] = (float)dir2[1];
		GestureRotation = rm.ToQuat();
		break;
	case GestureRecognition_CoordinateSystem::Unreal_OculusVR:
	case GestureRecognition_CoordinateSystem::Unreal_OpenXR:
	case GestureRecognition_CoordinateSystem::Unreal_SteamVR:
	default:
		GesturePosition.X = float(pos[0]);
		GesturePosition.Y = float(pos[1]);
		GesturePosition.Z = float(pos[2]);
		GestureScale = float(scale);
		rm.M[0][0] = (float)dir0[0];
		rm.M[0][1] = (float)dir0[1];
		rm.M[0][2] = (float)dir0[2];
		rm.M[1][0] = (float)dir1[0];
		rm.M[1][1] = (float)dir1[1];
		rm.M[1][2] = (float)dir1[2];
		rm.M[2][0] = (float)dir2[0];
		rm.M[2][1] = (float)dir2[1];
		rm.M[2][2] = (float)dir2[2];
		GestureRotation = rm.ToQuat();
	}
	return gesture_id;
}


int AGestureRecognitionActor::endStrokeAndGetAllProbabilities(TArray<float>& Probabilities, FVector& GesturePosition, FRotator& GestureRotation, float& GestureScale)
{
	if (!this->gro) {
		return -99;
	}
	FQuat quat;
	int ret = this->endStrokeAndGetAllProbabilitiesQ(Probabilities, GesturePosition, quat, GestureScale);
	GestureRotation = quat.Rotator();
	return ret;
}

int AGestureRecognitionActor::endStrokeAndGetAllProbabilitiesQ(TArray<float>& Probabilities, FVector& GesturePosition, FQuat& GestureRotation, float& GestureScale)
{
	if (!this->gro) {
		return -99;
	}
	const int num_gestures = this->gro->numberOfGestures();
	if (num_gestures <= 0) {
		return IGestureRecognition::Error_NoGestures;
	}
	double* p = new double[num_gestures];
	for (int i = 0; i < num_gestures; i++) {
		p[i] = -1;
	}
	int n = num_gestures;
	double pos[3] = {0,0,0};
	double scale = 1;
	double dir0[3] = {1,0,0};
	double dir1[3] = {0,1,0};
	double dir2[3] = {0,0,1};
	int ret = this->gro->endStrokeAndGetAllProbabilities(p, &n, pos, &scale, dir0, dir1, dir2);
	if (ret != 0) {
		Probabilities.SetNum(0);
		return ret;
	}
	Probabilities.SetNum(n);
	for (int i = 0; i < n; i++) {
		Probabilities[i] = (float)p[i];
	}
	delete[] p;
	FRotationMatrix rm(FRotator(0, 0, 0));
	switch (this->MivryCoordinateSystem) {
	case GestureRecognition_CoordinateSystem::Unity_OculusVR:
	case GestureRecognition_CoordinateSystem::Unity_OpenXR:
	case GestureRecognition_CoordinateSystem::Unity_SteamVR:
		GesturePosition.X = float(pos[2]) * 100.0f; // Unreal.X = front = Unity.Z
		GesturePosition.Y = float(pos[0]) * 100.0f; // Unreal.Y = right = Unity.X
		GesturePosition.Z = float(pos[1]) * 100.0f; // Unreal.Z = up    = Unity.Y
		GestureScale = float(scale) * 100.0f;
		rm.M[0][0] = (float)dir0[2]; // Unreal.X = front = Unity.Z
		rm.M[0][1] = (float)dir0[0]; // Unreal.Y = right = Unity.X
		rm.M[0][2] = (float)dir0[1]; // Unreal.Z = up    = Unity.Y
		rm.M[1][0] = (float)dir1[2];
		rm.M[1][1] = (float)dir1[0];
		rm.M[1][2] = (float)dir1[1];
		rm.M[2][0] = (float)dir2[2];
		rm.M[2][1] = (float)dir2[0];
		rm.M[2][2] = (float)dir2[1];
		GestureRotation = rm.ToQuat();
		break;
	case GestureRecognition_CoordinateSystem::Unreal_OculusVR:
	case GestureRecognition_CoordinateSystem::Unreal_OpenXR:
	case GestureRecognition_CoordinateSystem::Unreal_SteamVR:
	default:
		GesturePosition.X = float(pos[0]);
		GesturePosition.Y = float(pos[1]);
		GesturePosition.Z = float(pos[2]);
		GestureScale = float(scale);
		rm.M[0][0] = (float)dir0[0];
		rm.M[0][1] = (float)dir0[1];
		rm.M[0][2] = (float)dir0[2];
		rm.M[1][0] = (float)dir1[0];
		rm.M[1][1] = (float)dir1[1];
		rm.M[1][2] = (float)dir1[2];
		rm.M[2][0] = (float)dir2[0];
		rm.M[2][1] = (float)dir2[1];
		rm.M[2][2] = (float)dir2[2];
		GestureRotation = rm.ToQuat();
	}
	return 0;
}

int AGestureRecognitionActor::endStrokeAndGetAllProbabilitiesAndSimilarities(TArray<float>& Probabilities, TArray<float>& Similarities, FVector& GesturePosition, FRotator& GestureRotation, float& GestureScale)
{
	if (!this->gro) {
		return -99;
	}
	FQuat quat;
	int ret = this->endStrokeAndGetAllProbabilitiesAndSimilaritiesQ(Probabilities, Similarities, GesturePosition, quat, GestureScale);
	GestureRotation = quat.Rotator();
	return ret;
}

int AGestureRecognitionActor::endStrokeAndGetAllProbabilitiesAndSimilaritiesQ(TArray<float>& Probabilities, TArray<float>& Similarities, FVector& GesturePosition, FQuat& GestureRotation, float& GestureScale)
{
	if (!this->gro) {
		return -99;
	}
	const int num_gestures = this->gro->numberOfGestures();
	if (num_gestures <= 0) {
		return IGestureRecognition::Error_NoGestures;
	}
	int n = num_gestures;
	double* p = new double[num_gestures];
	double* s = new double[num_gestures];
	for (int i = 0; i < num_gestures; i++) {
		p[i] = -1;
		s[i] = -1;
	}
	double pos[3] = {0,0,0};
	double scale = 1;
	double dir0[3] = {1,0,0};
	double dir1[3] = {0,1,0};
	double dir2[3] = {0,0,1};
	int ret = this->gro->endStrokeAndGetAllProbabilitiesAndSimilarities(p, s, &n, pos, &scale, dir0, dir1, dir2);
	if (ret != 0) {
		Probabilities.SetNum(0);
		Similarities.SetNum(0);
		return ret;
	}
	Probabilities.SetNum(n);
	Similarities.SetNum(n);
	for (int i = 0; i < n; i++) {
		Probabilities[i] = (float)p[i];
		Similarities[i] = (float)s[i];
	}
	delete[] p;
	delete[] s;
	FRotationMatrix rm(FRotator(0, 0, 0));
	switch (this->MivryCoordinateSystem) {
	case GestureRecognition_CoordinateSystem::Unity_OculusVR:
	case GestureRecognition_CoordinateSystem::Unity_OpenXR:
	case GestureRecognition_CoordinateSystem::Unity_SteamVR:
		GesturePosition.X = float(pos[2]) * 100.0f; // Unreal.X = front = Unity.Z
		GesturePosition.Y = float(pos[0]) * 100.0f; // Unreal.Y = right = Unity.X
		GesturePosition.Z = float(pos[1]) * 100.0f; // Unreal.Z = up    = Unity.Y
		GestureScale = float(scale) * 100.0f;
		rm.M[0][0] = (float)dir0[2]; // Unreal.X = front = Unity.Z
		rm.M[0][1] = (float)dir0[0]; // Unreal.Y = right = Unity.X
		rm.M[0][2] = (float)dir0[1]; // Unreal.Z = up    = Unity.Y
		rm.M[1][0] = (float)dir1[2];
		rm.M[1][1] = (float)dir1[0];
		rm.M[1][2] = (float)dir1[1];
		rm.M[2][0] = (float)dir2[2];
		rm.M[2][1] = (float)dir2[0];
		rm.M[2][2] = (float)dir2[1];
		GestureRotation = rm.ToQuat();
		break;
	case GestureRecognition_CoordinateSystem::Unreal_OculusVR:
	case GestureRecognition_CoordinateSystem::Unreal_OpenXR:
	case GestureRecognition_CoordinateSystem::Unreal_SteamVR:
	default:
		GesturePosition.X = float(pos[0]);
		GesturePosition.Y = float(pos[1]);
		GesturePosition.Z = float(pos[2]);
		GestureScale = float(scale);
		rm.M[0][0] = (float)dir0[0];
		rm.M[0][1] = (float)dir0[1];
		rm.M[0][2] = (float)dir0[2];
		rm.M[1][0] = (float)dir1[0];
		rm.M[1][1] = (float)dir1[1];
		rm.M[1][2] = (float)dir1[2];
		rm.M[2][0] = (float)dir2[0];
		rm.M[2][1] = (float)dir2[1];
		rm.M[2][2] = (float)dir2[2];
		GestureRotation = rm.ToQuat();
	}
	return 0;
}

bool AGestureRecognitionActor::isStrokeStarted()
{
	if (!this->gro) {
		return false;
	}
	return this->gro->isStrokeStarted();
}

int AGestureRecognitionActor::pruneStroke(int num, int ms)
{
	if (!this->gro) {
		return -99;
	}
	return this->gro->pruneStroke(num, ms);
}

int AGestureRecognitionActor::cancelStroke()
{
	if (!this->gro) {
		return -99;
	}
	return this->gro->cancelStroke();
}

int AGestureRecognitionActor::contdIdentify(const FVector& HMD_Position, const FRotator& HMD_Rotation, float& Similarity)
{
	if (!this->gro) {
		return -99;
	}
	FQuat quat = HMD_Rotation.Quaternion();
	double similarity = -1;
	double hmd_p[3];
	double hmd_q[4];
	UMiVRyUtil::convertInput(HMD_Position, quat, GestureRecognition_DeviceType::Headset, this->UnrealVRPlugin, this->MivryCoordinateSystem, hmd_p, hmd_q);
	int ret = this->gro->contdIdentify(hmd_p, hmd_q, &similarity);
	Similarity = (float)similarity;
	return ret;
}

int AGestureRecognitionActor::contdIdentifyAndGetAllProbabilitiesAndSimilarities(const FVector& HMD_Position, const FRotator& HMD_Rotation, TArray<float>& Probabilities, TArray<float>& Similarities)
{
	if (!this->gro) {
		return -99;
	}
	const int num_gestures = this->gro->numberOfGestures();
	if (num_gestures <= 0) {
		return IGestureRecognition::Error_NoGestures;
	}
	int n = num_gestures;
	double* p = new double[num_gestures];
	double* s = new double[num_gestures];
	FQuat quat = HMD_Rotation.Quaternion();
	double hmd_p[3];
	double hmd_q[4];
	UMiVRyUtil::convertInput(HMD_Position, quat, GestureRecognition_DeviceType::Headset, this->UnrealVRPlugin, this->MivryCoordinateSystem, hmd_p, hmd_q);
	int ret = this->gro->contdIdentifyAndGetAllProbabilitiesAndSimilarities(hmd_p, hmd_q, p, s, &n);
	if (ret != 0) {
		delete[] p;
		delete[] s;
		return ret;
	}
	Probabilities.SetNum(n);
	Similarities.SetNum(n);
	for (int i = 0; i < n; i++) {
		Probabilities[i] = (float)p[i];
		Similarities[i] = (float)s[i];
	}
	delete[] p;
	delete[] s;
	return 0;
}

int AGestureRecognitionActor::contdRecord(const FVector& HMD_Position, const FRotator& HMD_Rotation)
{
	if (!this->gro) {
		return -99;
	}
	FQuat HMD_Quaternion = HMD_Rotation.Quaternion();
	double hmd_p[3];
	double hmd_q[4];
	UMiVRyUtil::convertInput(HMD_Position, HMD_Quaternion, GestureRecognition_DeviceType::Headset, this->UnrealVRPlugin, this->MivryCoordinateSystem, hmd_p, hmd_q);
	return this->gro->contdRecord(hmd_p, hmd_q);
}

int AGestureRecognitionActor::GetContinuousGestureIdentificationPeriod()
{
	if (!this->gro) {
		return -99;
	}
	return (int)this->gro->contdIdentificationPeriod;
}

int AGestureRecognitionActor::SetContinuousGestureIdentificationPeriod(int PeriodInMs)
{
	if (!this->gro) {
		return -99;
	}
	this->gro->contdIdentificationPeriod = (unsigned int)PeriodInMs;
	return 0;
}

int AGestureRecognitionActor::GetContinuousGestureIdentificationSmoothing()
{
	if (!this->gro) {
		return -99;
	}
	return (int)this->gro->contdIdentificationSmoothing;
}

int AGestureRecognitionActor::SetContinuousGestureIdentificationSmoothing(int NumberOfSamples)
{
	if (!this->gro) {
		return -99;
	}
	this->gro->contdIdentificationSmoothing = (unsigned int)NumberOfSamples;
	return 0;
}

int AGestureRecognitionActor::numberOfGestures()
{
	if (!this->gro) {
		return -99;
	}
	return this->gro->numberOfGestures();
}

int AGestureRecognitionActor::deleteGesture(int index)
{
	if (!this->gro) {
		return -99;
	}
	return this->gro->deleteGesture(index);
}

int AGestureRecognitionActor::deleteAllGestures()
{
	if (!this->gro) {
		return -99;
	}
	return this->gro->deleteAllGestures();
}

int AGestureRecognitionActor::createGesture(const FString& name)
{
	if (!this->gro) {
		return -99;
	}
	return this->gro->createGesture(TCHAR_TO_ANSI(*name), nullptr);
}

float AGestureRecognitionActor::recognitionScore()
{
	if (!this->gro) {
		return -99;
	}
	return (float)this->gro->recognitionScore();
}

FString AGestureRecognitionActor::getGestureName(int index)
{
	if (!this->gro) {
		return "";
	}
	return this->gro->getGestureName(index);
}

int AGestureRecognitionActor::setGestureName(int index, const FString& name)
{
	if (!this->gro) {
		return -99;
	}
	return this->gro->setGestureName(index, TCHAR_TO_ANSI(*name));
}

int AGestureRecognitionActor::setGestureMetadata(int index, const TArray<uint8>& metadata)
{
	if (!this->gro) {
		return -99;
	}
	IGestureRecognition::DefaultMetadata* dmo = (IGestureRecognition::DefaultMetadata*)this->gro->getGestureMetadata(index);
	if (dmo == nullptr) {
		dmo = IGestureRecognition::defaultMetadataCreatorFunction();
		this->gro->setGestureMetadata(index, dmo);
	}
	return dmo->setData(metadata.GetData(), metadata.Num());
}

int AGestureRecognitionActor::setGestureMetadataAsString(int index, const FString& metadata)
{
	if (!this->gro) {
		return -99;
	}
	IGestureRecognition::DefaultMetadata* dmo = (IGestureRecognition::DefaultMetadata*)this->gro->getGestureMetadata(index);
	if (dmo == nullptr) {
		dmo = IGestureRecognition::defaultMetadataCreatorFunction();
		this->gro->setGestureMetadata(index, dmo);
	}
	return dmo->setData(TCHAR_TO_ANSI(*metadata), metadata.Len());
}

int AGestureRecognitionActor::getGestureMetadata(int index, TArray<uint8>& metadata)
{
	metadata.SetNum(0);
	if (!this->gro) {
		return -99;
	}
	IGestureRecognition::DefaultMetadata* dmo = (IGestureRecognition::DefaultMetadata*)this->gro->getGestureMetadata(index);
	if (dmo == nullptr) {
		return 0;
	}
	const int size = dmo->getSize();
	if (size < 0) {
		return size;
	}
	metadata.SetNum(size);
	return dmo->getData(metadata.GetData(), size);
}

int AGestureRecognitionActor::getGestureMetadataAsString(int index, FString& metadata)
{
	metadata = "";
	if (!this->gro) {
		return -99;
	}
	IGestureRecognition::DefaultMetadata* dmo = (IGestureRecognition::DefaultMetadata*)this->gro->getGestureMetadata(index);
	if (dmo == nullptr) {
		return 0;
	}
	const int size = dmo->getSize();
	if (size < 0) {
		return 0;
	}
	char* buf = new char[size + 1];
	int ret = dmo->getData(buf, size);
	if (ret < 0) {
		delete[] buf;
		return ret;
	}
	buf[size] = '\n';
	metadata = FString(buf);
	delete[] buf;
	return ret;
}

bool AGestureRecognitionActor::getGestureEnabled(int index)
{
	if (!this->gro) {
		return false;
	}
	return this->gro->getGestureEnabled(index);
}

int AGestureRecognitionActor::setGestureEnabled(int index, bool enabled)
{
	if (!this->gro) {
		return -99;
	}
	return this->gro->setGestureEnabled(index, enabled);
}

int AGestureRecognitionActor::getGestureNumberOfSamples(int index)
{
	if (!this->gro) {
		return -99;
	}
	return this->gro->getGestureNumberOfSamples(index);
}

int AGestureRecognitionActor::getGestureSampleType(int gesture_index, int sample_index) const
{
	if (!this->gro) {
		return -99;
	}
	return this->gro->getGestureSampleType(gesture_index, sample_index);
}

int AGestureRecognitionActor::setGestureSampleType(int gesture_index, int sample_index, int type)
{
	if (!this->gro) {
		return -99;
	}
	return this->gro->setGestureSampleType(gesture_index, sample_index, type);
}

int AGestureRecognitionActor::getGestureSampleLength(int gesture_index, int sample_index, bool processed)
{
	if (!this->gro) {
		return -99;
	}
	return this->gro->getGestureSampleLength(gesture_index, sample_index, processed);
}

int AGestureRecognitionActor::getGestureSampleStroke(int gesture_index, int sample_index, bool processed, TArray<FVector>& Locations, TArray<FRotator>& Rotations, TArray<FVector>& HMD_Locations, TArray<FRotator>& HMD_Rotations)
{
	if (!this->gro) {
		return -99;
	}
	const int sample_len = this->gro->getGestureSampleLength(gesture_index, sample_index, processed);
	if (sample_len <= 0) {
		return sample_len;
	}
	double* p = new double[3 * sample_len];
	double* q = new double[4 * sample_len];
	double* hmd_p = new double[3 * sample_len];
	double* hmd_q = new double[4 * sample_len];
	int ret = this->gro->getGestureSampleStroke(gesture_index, sample_index, processed, sample_len, (double(*)[3])p, (double(*)[4])q, (double(*)[3])hmd_p, (double(*)[4])hmd_q);
	if (ret <= 0) {
		delete[] p;
		delete[] q;
		Locations.Empty(0);
		Rotations.Empty(0);
		HMD_Locations.Empty(0);
		HMD_Rotations.Empty(0);
		return ret;
	}
	FQuat quat;
	const int len = (ret < sample_len) ? ret : sample_len;
	Locations.SetNum(len);
	Rotations.SetNum(len);
	HMD_Locations.SetNum(len);
	HMD_Rotations.SetNum(len);
	for (int i = 0; i < len; i++) {
		UMiVRyUtil::convertOutput(this->UnrealVRPlugin, this->MivryCoordinateSystem, &p[i*3], &q[i*4], GestureRecognition_DeviceType::Controller, Locations[i], quat);
		Rotations[i] = quat.Rotator();
		UMiVRyUtil::convertOutput(this->UnrealVRPlugin, this->MivryCoordinateSystem, &hmd_p[i*3], &hmd_q[i*4], GestureRecognition_DeviceType::Headset, HMD_Locations[i], quat);
		HMD_Rotations[i] = quat.Rotator();
	}
	delete[] p;
	delete[] q;
	delete[] hmd_p;
	delete[] hmd_q;
	return len;
}

int AGestureRecognitionActor::getGestureMeanLength(int gesture_index)
{
	if (!this->gro) {
		return -99;
	}
	return this->gro->getGestureMeanLength(gesture_index);
}

int AGestureRecognitionActor::getGestureMeanStroke(int gesture_index, TArray<FVector>& Locations, TArray<FRotator>& Rotations, FVector& GestureLocation, FRotator& GestureRotation, float& GestureScale)
{
	if (!this->gro) {
		return -99;
	}
	const int mean_len = this->gro->getGestureMeanLength(gesture_index);
	if (mean_len <= 0) {
		return mean_len;
	}
	double* p = new double[3 * mean_len];
	double* q = new double[4 * mean_len];
	double stroke_p[3];
	double stroke_q[4];
	double scale;
	const int ret = this->gro->getGestureMeanStroke(gesture_index, (double(*)[3])p, (double(*)[4])q, mean_len, stroke_p, stroke_q, &scale);
	if (ret <= 0) {
		delete[] p;
		delete[] q;
		Locations.Empty(0);
		Rotations.Empty(0);
		return ret;
	}
	switch (this->MivryCoordinateSystem) {
	case GestureRecognition_CoordinateSystem::Unity_OculusVR:
	case GestureRecognition_CoordinateSystem::Unity_OpenXR:
	case GestureRecognition_CoordinateSystem::Unity_SteamVR:
		GestureLocation = FVector((float)stroke_p[2], (float)stroke_p[0], (float)stroke_p[1]);
		GestureRotation = FQuat((float)stroke_q[2], (float)stroke_q[0], (float)stroke_q[1], (float)stroke_q[3]).Rotator();
		GestureScale = (float)scale * 100.0f;
		break;
	case GestureRecognition_CoordinateSystem::Unreal_OculusVR:
	case GestureRecognition_CoordinateSystem::Unreal_OpenXR:
	case GestureRecognition_CoordinateSystem::Unreal_SteamVR:
	default:
		GestureLocation = FVector((float)stroke_p[0], (float)stroke_p[1], (float)stroke_p[2]);
		GestureRotation = FQuat((float)stroke_q[0], (float)stroke_q[1], (float)stroke_q[2], (float)stroke_q[3]).Rotator();
		GestureScale = (float)scale;
	}
	const int len = (ret < mean_len) ? ret : mean_len;
	Locations.SetNum(len);
	Rotations.SetNum(len);
	FQuat quat;
	for (int i = 0; i < len; i++) {
		UMiVRyUtil::convertOutput(this->UnrealVRPlugin, this->MivryCoordinateSystem, &p[3 * i], &q[4 * i], GestureRecognition_DeviceType::Controller, Locations[i], quat);
		Rotations[i] = quat.Rotator();
	}
	delete[] p;
	delete[] q;
	return len;
}

int AGestureRecognitionActor::deleteGestureSample(int gesture_index, int sample_index)
{
	if (!this->gro) {
		return -99;
	}
	return this->gro->deleteGestureSample(gesture_index, sample_index);
}

int AGestureRecognitionActor::deleteAllGestureSamples(int gesture_index)
{
	if (!this->gro) {
		return -99;
	}
	return this->gro->deleteAllGestureSamples(gesture_index);
}

int AGestureRecognitionActor::setMetadata(const TArray<uint8>& metadata)
{
	if (!this->gro) {
		return -99;
	}
	IGestureRecognition::DefaultMetadata* dmo = (IGestureRecognition::DefaultMetadata*)this->gro->getMetadata();
	if (dmo == nullptr) {
		dmo = IGestureRecognition::defaultMetadataCreatorFunction();
		this->gro->setMetadata(dmo);
	}
	return dmo->setData(metadata.GetData(), metadata.Num());
}

int AGestureRecognitionActor::setMetadataAsString(const FString& metadata)
{
	if (!this->gro) {
		return -99;
	}
	IGestureRecognition::DefaultMetadata* dmo = (IGestureRecognition::DefaultMetadata*)this->gro->getMetadata();
	if (dmo == nullptr) {
		dmo = IGestureRecognition::defaultMetadataCreatorFunction();
		this->gro->setMetadata(dmo);
	}
	return dmo->setData(TCHAR_TO_ANSI(*metadata), metadata.Len());
}

int AGestureRecognitionActor::getMetadata(TArray<uint8>& metadata)
{
	metadata.SetNum(0);
	if (!this->gro) {
		return -99;
	}
	IGestureRecognition::DefaultMetadata* dmo = (IGestureRecognition::DefaultMetadata*)this->gro->getMetadata();
	if (dmo == nullptr) {
		return 0;
	}
	const int size = dmo->getSize();
	if (size < 0) {
		return size;
	}
	metadata.SetNum(size);
	return dmo->getData(metadata.GetData(), size);
}

int AGestureRecognitionActor::getMetadataAsString(FString& metadata)
{
	metadata = "";
	if (!this->gro) {
		return -99;
	}
	IGestureRecognition::DefaultMetadata* dmo = (IGestureRecognition::DefaultMetadata*)this->gro->getMetadata();
	if (dmo == nullptr) {
		return 0;
	}
	const int size = dmo->getSize();
	if (size < 0) {
		return 0;
	}
	char* buf = new char[size + 1];
	int ret = dmo->getData(buf, size);
	if (ret < 0) {
		delete[] buf;
		return ret;
	}
	buf[size] = '\n';
	metadata = FString(buf);
	delete[] buf;
	return ret;
}

int AGestureRecognitionActor::saveToFile(const FFilePath& path)
{
	if (!this->gro) {
		return -99;
	}
	FString path_str = path.FilePath;
	if (FPaths::IsRelative(path_str)) {
		path_str = FPaths::Combine(FPaths::ProjectDir(), path_str);
	}
	return this->gro->saveToFile(TCHAR_TO_ANSI(*path_str));
}


int AGestureRecognitionActor::loadFromFile(const FFilePath& path)
{
	if (!this->gro) {
		return -99;
	}
	FString path_str;
	GestureRecognition_Result result;
	UMiVRyUtil::findFile(path.FilePath, result, path_str);
	if (result == GestureRecognition_Result::Then) {
		return this->gro->loadFromFile(TCHAR_TO_ANSI(*path_str), nullptr);
	}
	TArray<uint8> buffer;
	UMiVRyUtil::readFileToBuffer(path.FilePath, result, buffer);
	if (result == GestureRecognition_Result::Then) {
		return this->gro->loadFromBuffer((const char*)buffer.GetData(), buffer.Num(), nullptr);
	}
	return -3;
}

int AGestureRecognitionActor::loadFromBuffer(const TArray<uint8>& buffer)
{
	if (!this->gro) {
		return -99;
	}
	return this->gro->loadFromBuffer((const char*)buffer.GetData(), buffer.Num(), nullptr);
}

int AGestureRecognitionActor::importFromFile(const FFilePath& path)
{
	if (!this->gro) {
		return -99;
	}
	FString path_str;
	GestureRecognition_Result result;
	UMiVRyUtil::findFile(path.FilePath, result, path_str);
	if (result == GestureRecognition_Result::Then) {
		return this->gro->importFromFile(TCHAR_TO_ANSI(*path_str), nullptr);
	}
	TArray<uint8> buffer;
	UMiVRyUtil::readFileToBuffer(path.FilePath, result, buffer);
	if (result == GestureRecognition_Result::Then) {
		return this->gro->importFromBuffer((const char*)buffer.GetData(), buffer.Num(), nullptr);
	}
	return -3;
}

int AGestureRecognitionActor::importFromBuffer(const TArray<uint8>& buffer)
{
	if (!this->gro) {
		return -99;
	}
	return this->gro->importFromBuffer((const char*)buffer.GetData(), buffer.Num(), nullptr);
}

int AGestureRecognitionActor::saveToFileAsync(const FFilePath& path)
{
	if (!this->gro)
		return -99;
	int ret;
	ret = this->gro->setSavingUpdateCallbackFunction((IGestureRecognition::SavingCallbackFunction*)&SavingCallbackFunction);
	if (ret != 0)
		return ret;
	ret = this->gro->setSavingFinishCallbackFunction((IGestureRecognition::SavingCallbackFunction*)&SavingCallbackFunction);
	if (ret != 0)
		return ret;
	ret = this->gro->setSavingUpdateCallbackMetadata(&this->SavingUpdateMetadata);
	if (ret != 0)
		return ret;
	ret = this->gro->setSavingFinishCallbackMetadata(&this->SavingFinishMetadata);
	if (ret != 0)
		return ret;
	FString path_str = path.FilePath;
	if (FPaths::IsRelative(path_str)) {
		path_str = FPaths::Combine(FPaths::ProjectDir(), path_str);
	}
	return this->gro->saveToFileAsync(TCHAR_TO_ANSI(*path_str));
}

bool AGestureRecognitionActor::isSaving()
{
	if (!this->gro)
		return false;
	return this->gro->isSaving();
}

int AGestureRecognitionActor::cancelSaving()
{
	if (!this->gro)
		return -99;
	return this->gro->cancelSaving();
}

int AGestureRecognitionActor::loadFromFileAsync(const FFilePath& path)
{
	if (!this->gro)
		return -99;
	int ret;
	ret = this->gro->setLoadingUpdateCallbackFunction((IGestureRecognition::LoadingCallbackFunction*)&LoadingCallbackFunction);
	if (ret != 0)
		return ret;
	ret = this->gro->setLoadingFinishCallbackFunction((IGestureRecognition::LoadingCallbackFunction*)&LoadingCallbackFunction);
	if (ret != 0)
		return ret;
	ret = this->gro->setLoadingUpdateCallbackMetadata(&this->LoadingUpdateMetadata);
	if (ret != 0)
		return ret;
	ret = this->gro->setLoadingFinishCallbackMetadata(&this->LoadingFinishMetadata);
	if (ret != 0)
		return ret;
	FString path_str;
	GestureRecognition_Result result;
	UMiVRyUtil::findFile(path.FilePath, result, path_str);
	if (result == GestureRecognition_Result::Then) {
		return this->gro->loadFromFileAsync(TCHAR_TO_ANSI(*path_str), nullptr);
	}
	TArray<uint8> buffer;
	UMiVRyUtil::readFileToBuffer(path.FilePath, result, buffer);
	if (result == GestureRecognition_Result::Then) {
		return this->gro->loadFromBufferAsync((const char*)buffer.GetData(), buffer.Num(), nullptr);
	}
	return -3;
}

int AGestureRecognitionActor::loadFromBufferAsync(const TArray<uint8>& buffer)
{
	if (!this->gro)
		return -99;
	int ret;
	ret = this->gro->setLoadingUpdateCallbackFunction((IGestureRecognition::LoadingCallbackFunction*)&LoadingCallbackFunction);
	if (ret != 0)
		return ret;
	ret = this->gro->setLoadingFinishCallbackFunction((IGestureRecognition::LoadingCallbackFunction*)&LoadingCallbackFunction);
	if (ret != 0)
		return ret;
	ret = this->gro->setLoadingUpdateCallbackMetadata(&this->LoadingUpdateMetadata);
	if (ret != 0)
		return ret;
	ret = this->gro->setLoadingFinishCallbackMetadata(&this->LoadingFinishMetadata);
	if (ret != 0)
		return ret;
	return this->gro->loadFromBufferAsync((const char*)buffer.GetData(), buffer.Num(), nullptr);
}

bool AGestureRecognitionActor::isLoading()
{
	if (!this->gro)
		return false;
	return this->gro->isLoading();
}

int AGestureRecognitionActor::cancelLoading()
{
	if (!this->gro)
		return -99;
	return this->gro->cancelLoading();
}


int AGestureRecognitionActor::startTraining()
{
	if (this->gro == nullptr) {
		return -99;
	}
	this->gro->trainingUpdateCallback = (IGestureRecognition::TrainingCallbackFunction*)&TrainingCallbackFunction;
	this->gro->trainingFinishCallback = (IGestureRecognition::TrainingCallbackFunction*)&TrainingCallbackFunction;
	this->gro->trainingUpdateCallbackMetadata = &this->TrainingUpdateMetadata;
	this->gro->trainingFinishCallbackMetadata = &this->TrainingFinishMetadata;
	return this->gro->startTraining();
}

bool AGestureRecognitionActor::isTraining()
{
	if (!this->gro) {
		return false;
	}
	return this->gro->isTraining();
}

void AGestureRecognitionActor::stopTraining()
{
	if (!this->gro) {
		return;
	}
	this->gro->stopTraining();
}

int AGestureRecognitionActor::getMaxTrainingTime()
{
	if (!this->gro) {
		return -99;
	}
	return (int)this->gro->maxTrainingTime;
}

void AGestureRecognitionActor::setMaxTrainingTime(int t)
{
	if (!this->gro) {
		return;
	}
	this->gro->maxTrainingTime = (unsigned long)t;
}

int AGestureRecognitionActor::setUpdateHeadPositionPolicy(GestureRecognition_UpdateHeadPositionPolicy p)
{
	if (!this->gro) {
		return -99;
	}
	return this->gro->setUpdateHeadPositionPolicy((IGestureRecognition::UpdateHeadPositionPolicy)p);
}

GestureRecognition_UpdateHeadPositionPolicy AGestureRecognitionActor::getUpdateHeadPositionPolicy()
{
	if (!this->gro) {
		return (GestureRecognition_UpdateHeadPositionPolicy)-99;
	}
	return (GestureRecognition_UpdateHeadPositionPolicy)this->gro->getUpdateHeadPositionPolicy();
}

GestureRecognition_FrameOfReference AGestureRecognitionActor::getRotationalFrameOfReferenceRoll()
{
	if (!this->gro) {
		return (GestureRecognition_FrameOfReference)-1;
	}
    switch (this->MivryCoordinateSystem) {
        case GestureRecognition_CoordinateSystem::Unity_OpenXR:
        case GestureRecognition_CoordinateSystem::Unity_OculusVR:
        case GestureRecognition_CoordinateSystem::Unity_SteamVR:
            return (GestureRecognition_FrameOfReference)this->gro->rotationalFrameOfReference.z;
		case GestureRecognition_CoordinateSystem::Unreal_OculusVR:
		case GestureRecognition_CoordinateSystem::Unreal_OpenXR:
		case GestureRecognition_CoordinateSystem::Unreal_SteamVR:
		default:
            return (GestureRecognition_FrameOfReference)this->gro->rotationalFrameOfReference.x;
    }
}

void AGestureRecognitionActor::setRotationalFrameOfReferenceRoll(GestureRecognition_FrameOfReference i)
{
	if (!this->gro) {
		return;
	}
    switch (this->MivryCoordinateSystem) {
        case GestureRecognition_CoordinateSystem::Unity_OpenXR:
        case GestureRecognition_CoordinateSystem::Unity_OculusVR:
        case GestureRecognition_CoordinateSystem::Unity_SteamVR:
            this->gro->rotationalFrameOfReference.z = IGestureRecognition::FrameOfReference(i);
            break;
		case GestureRecognition_CoordinateSystem::Unreal_OculusVR:
		case GestureRecognition_CoordinateSystem::Unreal_OpenXR:
		case GestureRecognition_CoordinateSystem::Unreal_SteamVR:
		default:
            this->gro->rotationalFrameOfReference.x = IGestureRecognition::FrameOfReference(i);
    }
}

GestureRecognition_FrameOfReference AGestureRecognitionActor::getRotationalFrameOfReferencePitch()
{
	if (!this->gro) {
		return (GestureRecognition_FrameOfReference)-1;
	}
    switch (this->MivryCoordinateSystem) {
        case GestureRecognition_CoordinateSystem::Unity_OpenXR:
        case GestureRecognition_CoordinateSystem::Unity_OculusVR:
        case GestureRecognition_CoordinateSystem::Unity_SteamVR:
            return (GestureRecognition_FrameOfReference)this->gro->rotationalFrameOfReference.x;
		case GestureRecognition_CoordinateSystem::Unreal_OculusVR:
		case GestureRecognition_CoordinateSystem::Unreal_OpenXR:
		case GestureRecognition_CoordinateSystem::Unreal_SteamVR:
		default:
            return (GestureRecognition_FrameOfReference)this->gro->rotationalFrameOfReference.y;
    }
}

void AGestureRecognitionActor::setRotationalFrameOfReferencePitch(GestureRecognition_FrameOfReference i)
{
	if (!this->gro) {
		return;
	}
    switch (this->MivryCoordinateSystem) {
        case GestureRecognition_CoordinateSystem::Unity_OpenXR:
        case GestureRecognition_CoordinateSystem::Unity_OculusVR:
        case GestureRecognition_CoordinateSystem::Unity_SteamVR:
            this->gro->rotationalFrameOfReference.x = IGestureRecognition::FrameOfReference(i);
            break;
		case GestureRecognition_CoordinateSystem::Unreal_OculusVR:
		case GestureRecognition_CoordinateSystem::Unreal_OpenXR:
		case GestureRecognition_CoordinateSystem::Unreal_SteamVR:
		default:
            this->gro->rotationalFrameOfReference.y = IGestureRecognition::FrameOfReference(i);
    }
}

GestureRecognition_FrameOfReference AGestureRecognitionActor::getRotationalFrameOfReferenceYaw()
{
	if (!this->gro) {
		return (GestureRecognition_FrameOfReference)-1;
	}
    switch (this->MivryCoordinateSystem) {
        case GestureRecognition_CoordinateSystem::Unity_OpenXR:
        case GestureRecognition_CoordinateSystem::Unity_OculusVR:
        case GestureRecognition_CoordinateSystem::Unity_SteamVR:
            return (GestureRecognition_FrameOfReference)this->gro->rotationalFrameOfReference.y;
		case GestureRecognition_CoordinateSystem::Unreal_OculusVR:
		case GestureRecognition_CoordinateSystem::Unreal_OpenXR:
		case GestureRecognition_CoordinateSystem::Unreal_SteamVR:
		default:
            return (GestureRecognition_FrameOfReference)this->gro->rotationalFrameOfReference.z;
    }
}

void AGestureRecognitionActor::setRotationalFrameOfReferenceYaw(GestureRecognition_FrameOfReference i)
{
	if (!this->gro) {
		return;
	}
    switch (this->MivryCoordinateSystem) {
        case GestureRecognition_CoordinateSystem::Unity_OpenXR:
        case GestureRecognition_CoordinateSystem::Unity_OculusVR:
        case GestureRecognition_CoordinateSystem::Unity_SteamVR:
            this->gro->rotationalFrameOfReference.y = IGestureRecognition::FrameOfReference(i);
            break;
		case GestureRecognition_CoordinateSystem::Unreal_OculusVR:
		case GestureRecognition_CoordinateSystem::Unreal_OpenXR:
		case GestureRecognition_CoordinateSystem::Unreal_SteamVR:
		default:
            this->gro->rotationalFrameOfReference.z = IGestureRecognition::FrameOfReference(i);
    }
}

GestureRecognition_RotationOrder AGestureRecognitionActor::getRotationalFrameOfReferenceRotationOrder()
{
	if (!this->gro) {
		return (GestureRecognition_RotationOrder)-1;
	}
	return (GestureRecognition_RotationOrder)this->gro->rotationalFrameOfReference.rotationOrder;
}

void AGestureRecognitionActor::setRotationalFrameOfReferenceRotationOrder(GestureRecognition_RotationOrder i)
{
	if (!this->gro) {
		return;
	}
	this->gro->rotationalFrameOfReference.rotationOrder = IGestureRecognition::RotationOrder(i);
}

void AGestureRecognitionActor::TrainingCallbackFunction(double performance, TrainingCallbackMetadata* metadata)
{
	if (!metadata || !metadata->delegate || !metadata->actor) {
		return;
	}
	metadata->delegate->Broadcast(metadata->actor, performance);
}

void AGestureRecognitionActor::LoadingCallbackFunction(int result, LoadingCallbackMetadata* metadata)
{
	if (!metadata || !metadata->delegate || !metadata->actor) {
		return;
	}
	metadata->delegate->Broadcast(metadata->actor, result);
}

void AGestureRecognitionActor::SavingCallbackFunction(int result, SavingCallbackMetadata* metadata)
{
	if (!metadata || !metadata->delegate || !metadata->actor) {
		return;
	}
	metadata->delegate->Broadcast(metadata->actor, result);
}
