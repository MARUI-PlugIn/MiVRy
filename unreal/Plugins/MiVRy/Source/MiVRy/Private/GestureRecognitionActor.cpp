/*
 * MiVRy - VR gesture recognition library plug-in for Unreal.
 * Version 2.5
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
	if (this->CoordinateSystem == GestureRecognition_CoordinateSystem::Unreal) {
		this->gro->rotationalFrameOfReference.rotationOrder = IGestureRecognition::RotationOrder::ZYX;
	} else { // Unity
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
	UMiVRyUtil::convertInput(HMD_Position, HMD_Rotation, GestureRecognition_DeviceType::Headset, this->CoordinateSystem, p, q);
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
	UMiVRyUtil::convertInput(HMD_Position, HMD_Rotation, GestureRecognition_DeviceType::Headset, this->CoordinateSystem, p, q);
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
	UMiVRyUtil::convertInput(HandPosition, HandRotation, GestureRecognition_DeviceType::Controller, this->CoordinateSystem, p, q);
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
	if (this->CoordinateSystem != GestureRecognition_CoordinateSystem::Unreal) {
		GesturePosition.X = float(pos[2]) * 100.0f; // Unreal.X = front = Unity.Z
		GesturePosition.Y = float(pos[0]) * 100.0f; // Unreal.Y = right = Unity.X
		GesturePosition.Z = float(pos[1]) * 100.0f; // Unreal.Z = up    = Unity.Y
		GestureScale = float(scale) * 100.0f;
		FRotationMatrix rm(FRotator(0, 0, 0));
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
	} else {
		GesturePosition.X = float(pos[0]);
		GesturePosition.Y = float(pos[1]);
		GesturePosition.Z = float(pos[2]);
		GestureScale = float(scale);
		FRotationMatrix rm(FRotator(0,0,0));
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
	if (this->CoordinateSystem != GestureRecognition_CoordinateSystem::Unreal) {
		GesturePosition.X = float(pos[2]) * 100.0f; // Unreal.X = front = Unity.Z
		GesturePosition.Y = float(pos[0]) * 100.0f; // Unreal.Y = right = Unity.X
		GesturePosition.Z = float(pos[1]) * 100.0f; // Unreal.Z = up    = Unity.Y
		GestureScale = float(scale) * 100.0f;
		FRotationMatrix rm(FRotator(0, 0, 0));
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
	} else {
		GesturePosition.X = float(pos[0]);
		GesturePosition.Y = float(pos[1]);
		GesturePosition.Z = float(pos[2]);
		GestureScale = float(scale);
		FRotationMatrix rm(FRotator(0, 0, 0));
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
	if (this->CoordinateSystem != GestureRecognition_CoordinateSystem::Unreal) {
		GesturePosition.X = float(pos[2]) * 100.0f; // Unreal.X = front = Unity.Z
		GesturePosition.Y = float(pos[0]) * 100.0f; // Unreal.Y = right = Unity.X
		GesturePosition.Z = float(pos[1]) * 100.0f; // Unreal.Z = up    = Unity.Y
		GestureScale = float(scale) * 100.0f;
		FRotationMatrix rm(FRotator(0, 0, 0));
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
	} else {
		GesturePosition.X = float(pos[0]);
		GesturePosition.Y = float(pos[1]);
		GesturePosition.Z = float(pos[2]);
		GestureScale = float(scale);
		FRotationMatrix rm(FRotator(0, 0, 0));
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
	UMiVRyUtil::convertInput(HMD_Position, quat, GestureRecognition_DeviceType::Headset, this->CoordinateSystem, hmd_p, hmd_q);
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
	UMiVRyUtil::convertInput(HMD_Position, quat, GestureRecognition_DeviceType::Headset, this->CoordinateSystem, hmd_p, hmd_q);
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
	UMiVRyUtil::convertInput(HMD_Position, HMD_Quaternion, GestureRecognition_DeviceType::Headset, this->CoordinateSystem, hmd_p, hmd_q);
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

int AGestureRecognitionActor::getGestureNumberOfSamples(int index)
{
	if (!this->gro) {
		return -99;
	}
	return this->gro->getGestureNumberOfSamples(index);
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
		UMiVRyUtil::convertOutput(this->CoordinateSystem, &p[i*3], &q[i*4], GestureRecognition_DeviceType::Controller, Locations[i], quat);
		Rotations[i] = quat.Rotator();
		UMiVRyUtil::convertOutput(this->CoordinateSystem, &hmd_p[i*3], &hmd_q[i*4], GestureRecognition_DeviceType::Headset, HMD_Locations[i], quat);
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
	double hmd_p[3];
	double hmd_q[4];
	double scale;
	const int ret = this->gro->getGestureMeanStroke(gesture_index, (double(*)[3])p, (double(*)[4])q, mean_len, hmd_p, hmd_q, &scale);
	if (ret <= 0) {
		delete[] p;
		delete[] q;
		Locations.Empty(0);
		Rotations.Empty(0);
		return ret;
	}
	const int len = (ret < mean_len) ? ret : mean_len;
	Locations.SetNum(len);
	Rotations.SetNum(len);
	FQuat quat;
	UMiVRyUtil::convertOutput(this->CoordinateSystem, hmd_p, hmd_q, GestureRecognition_DeviceType::Headset, GestureLocation, quat);
	GestureRotation = quat.Rotator();
	GestureScale = (float)scale;
	if (this->CoordinateSystem != GestureRecognition_CoordinateSystem::Unreal) {
		GestureScale *= 100.0f;
	}
	for (int i = 0; i < len; i++) {
		Locations[i].X = (float)p[3 * i + 0]; // x = primart axis
		Locations[i].Y = (float)p[3 * i + 1]; // y = secondary axis
		Locations[i].Z = (float)p[3 * i + 2]; // z = least-significant axis
		quat.X = (float)q[4 * i + 0];
		quat.Y = (float)q[4 * i + 1];
		quat.Z = (float)q[4 * i + 2];
		quat.W = (float)q[4 * i + 3];
		quat.Normalize();
		if (this->CoordinateSystem != GestureRecognition_CoordinateSystem::Unreal) {
			const FVector xaxis = quat.GetAxisX();
			const FVector yaxis = quat.GetAxisY();
			const FVector zaxis = quat.GetAxisZ();
			FRotationMatrix m(FRotator::ZeroRotator);
			m.M[0][0] = -yaxis.Z;
			m.M[0][1] = -yaxis.X;
			m.M[0][2] = -yaxis.Y;
			m.M[1][0] = xaxis.Z;
			m.M[1][1] = xaxis.X;
			m.M[1][2] = xaxis.Y;
			m.M[2][0] = zaxis.Z;
			m.M[2][1] = zaxis.X;
			m.M[2][2] = zaxis.Y;
			quat = m.ToQuat();
		}
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

GestureRecognition_FrameOfReference AGestureRecognitionActor::getRotationalFrameOfReferenceRoll()
{
	if (!this->gro) {
		return (GestureRecognition_FrameOfReference)-1;
	}
	return (GestureRecognition_FrameOfReference)this->gro->rotationalFrameOfReference.x;
}

void AGestureRecognitionActor::setRotationalFrameOfReferenceRoll(GestureRecognition_FrameOfReference i)
{
	if (!this->gro) {
		return;
	}
	this->gro->rotationalFrameOfReference.x = IGestureRecognition::FrameOfReference(i);
}

GestureRecognition_FrameOfReference AGestureRecognitionActor::getRotationalFrameOfReferencePitch()
{
	if (!this->gro) {
		return (GestureRecognition_FrameOfReference)-1;
	}
	return (GestureRecognition_FrameOfReference)this->gro->rotationalFrameOfReference.y;
}

void AGestureRecognitionActor::setRotationalFrameOfReferencePitch(GestureRecognition_FrameOfReference i)
{
	if (!this->gro) {
		return;
	}
	this->gro->rotationalFrameOfReference.y = IGestureRecognition::FrameOfReference(i);
}

GestureRecognition_FrameOfReference AGestureRecognitionActor::getRotationalFrameOfReferenceYaw()
{
	if (!this->gro) {
		return (GestureRecognition_FrameOfReference)-1;
	}
	return (GestureRecognition_FrameOfReference)this->gro->rotationalFrameOfReference.z;
}

void AGestureRecognitionActor::setRotationalFrameOfReferenceYaw(GestureRecognition_FrameOfReference i)
{
	if (!this->gro) {
		return;
	}
	this->gro->rotationalFrameOfReference.z = IGestureRecognition::FrameOfReference(i);
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
