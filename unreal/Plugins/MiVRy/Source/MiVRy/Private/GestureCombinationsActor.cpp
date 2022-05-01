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


#include "GestureCombinationsActor.h"
#include "GestureCombinations.h"
#include "Misc/MessageDialog.h"
#include "Misc/Paths.h"

AGestureCombinationsActor::AGestureCombinationsActor()
{
	PrimaryActorTick.bCanEverTick = true;

	TrainingUpdateMetadata.actor = this;
	TrainingUpdateMetadata.delegate = &this->OnTrainingUpdateDelegate;
	TrainingFinishMetadata.actor = this;
	TrainingFinishMetadata.delegate = &this->OnTrainingFinishDelegate;
	LoadingFinishMetadata.actor = this;
	LoadingFinishMetadata.delegate = &this->OnLoadingFinishDelegate;
}


AGestureCombinationsActor::~AGestureCombinationsActor()
{
	if (this->gco) {
		delete this->gco;
		this->gco = nullptr;
	}
}


// Called when the game starts or when spawned
void AGestureCombinationsActor::BeginPlay()
{
	Super::BeginPlay();

	this->gco = (IGestureCombinations*)GestureCombinations_create(this->NumberOfParts);
	if (this->gco == nullptr) {
		FMessageDialog::Open(EAppMsgType::Ok, FText::FromString("GestureCombinations::create returned null"));
	} else if (this->LicenseName.IsEmpty() == false) {
		const char* license_name = TCHAR_TO_ANSI(*this->LicenseName);
		const char* license_key = TCHAR_TO_ANSI(*this->LicenseKey);
		int ret = this->gco->activateLicense(license_name, license_key);
		if (ret != 0) {
			FMessageDialog::Open(EAppMsgType::Ok, FText::FromString(FString("Failed to activate license: ") + UMiVRyUtil::errorCodeToString(ret)));
		}
	}
}

// Called every frame
void AGestureCombinationsActor::Tick(float DeltaTime)
{
	Super::Tick(DeltaTime);
}


int AGestureCombinationsActor::startStroke(int part, const FVector& HMD_Position, const FRotator& HMD_Rotation, int record_as_sample)
{
	FQuat q = HMD_Rotation.Quaternion();
	return this->startStrokeQ(part, HMD_Position, q, record_as_sample);
}


int AGestureCombinationsActor::startStrokeQ(int part, const FVector& HMD_Position, const FQuat& HMD_Rotation, int record_as_sample)
{
	if (!this->gco)
		return -99;
	double p[3];
	double q[4];
	UMiVRyUtil::convertInput(HMD_Position, HMD_Rotation, GestureRecognition_DeviceType::Headset, this->CoordinateSystem, p, q);
	return this->gco->startStroke(part, p, q, record_as_sample);
}

int AGestureCombinationsActor::updateHeadPosition(const FVector& HMD_Position, const FRotator& HMD_Rotation)
{
	return this->updateHeadPositionQ(HMD_Position, HMD_Rotation.Quaternion());
}

int AGestureCombinationsActor::updateHeadPositionQ(const FVector& HMD_Position, const FQuat& HMD_Rotation)
{
	if (this->gco == nullptr) {
		return -99;
	}
	double p[3];
	double q[4];
	UMiVRyUtil::convertInput(HMD_Position, HMD_Rotation, GestureRecognition_DeviceType::Headset, this->CoordinateSystem, p, q);
	return this->gco->updateHeadPositionQ(p, q);
}

int AGestureCombinationsActor::contdStroke(int part, const FVector& HandPosition, const FRotator& HandRotation)
{
	FQuat q = HandRotation.Quaternion();
	return this->contdStrokeQ(part, HandPosition, q);
}

int AGestureCombinationsActor::contdStrokeQ(int part, const FVector& HandPosition, const FQuat& HandRotation)
{
	if (!this->gco)
		return -99;
	double p[3];
	double q[4];
	UMiVRyUtil::convertInput(HandPosition, HandRotation, GestureRecognition_DeviceType::Controller, this->CoordinateSystem, p, q);
	return this->gco->contdStrokeQ(part, p, q);
}

int AGestureCombinationsActor::endStroke(int part, FVector& GesturePosition, FRotator& GestureRotation, float& GestureScale)
{
	FQuat q(0, 0, 0, 1);
	int ret = this->endStrokeQ(part, GesturePosition, q, GestureScale);
	GestureRotation = q.Rotator();
	return ret;
}

int AGestureCombinationsActor::endStrokeQ(int part, FVector& GesturePosition, FQuat& GestureRotation, float& GestureScale)
{
	if (!this->gco)
		return -99;
	double pos[3] = {0,0,0};
	double scale = 1;
	double dir0[3] = {1,0,0};
	double dir1[3] = {0,1,0};
	double dir2[3] = {0,0,1};
	int ret = this->gco->endStroke(part, pos, &scale, dir0, dir1, dir2);
	if (this->CoordinateSystem != GestureRecognition_CoordinateSystem::Unreal) {
		GesturePosition.X = float(pos[2]) * 100.0f; // Unreal.X = front = Unity.Z
		GesturePosition.Y = float(pos[0]) * 100.0f; // Unreal.Y = right = Unity.X
		GesturePosition.Z = float(pos[1]) * 100.0f; // Unreal.Z = up    = Unity.Y
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
		GestureScale = float(scale) * 100.0f;
	} else {
		GesturePosition.X = pos[0];
		GesturePosition.Y = pos[1];
		GesturePosition.Z = pos[2];
		FRotationMatrix rm(FRotator(0, 0, 0));
		rm.M[0][0] = (float)dir0[0]; // dir0 = main direction = forward = x-axis
		rm.M[0][1] = (float)dir0[1];
		rm.M[0][2] = (float)dir0[2];
		rm.M[1][0] = (float)dir1[0];
		rm.M[1][1] = (float)dir1[1];
		rm.M[1][2] = (float)dir1[2];
		rm.M[2][0] = (float)dir2[0];
		rm.M[2][1] = (float)dir2[1];
		rm.M[2][2] = (float)dir2[2];
		GestureRotation = rm.ToQuat();
		GestureScale = float(scale);
	}
	return ret;
}

int AGestureCombinationsActor::getPartProbabilitiesAndSimilarities(int part, TArray<float>& probabilities, TArray<float>& similarities)
{
	probabilities.SetNum(0);
	similarities.SetNum(0);
	if (!this->gco)
		return -99;
	int n = this->gco->numberOfGestures(part);
	if (n <= 0) {
		return IGestureRecognition::Error_NoGestures;
	}
	double* p = new double[n];
	double* s = new double[n];
	int ret = this->gco->getPartProbabilitiesAndSimilarities(part, p, s, &n);
	if (ret == 0) {
		probabilities.SetNum(n);
		similarities.SetNum(n);
		for (int i = n-1; i >= 0; i--) {
			probabilities[i] = (float)p[i];
			similarities[i] = (float)s[i];
		}
	}
	delete[] p;
	delete[] s;
	return ret;
}

bool AGestureCombinationsActor::isStrokeStarted(int part)
{
	if (!this->gco)
		return false;
	return this->gco->isStrokeStarted(part);
}


int AGestureCombinationsActor::cancelStroke(int part)
{
	if (!this->gco)
		return -99;
	return this->gco->cancelStroke(part);
}


int AGestureCombinationsActor::identifyGestureCombination(float& probability, float& similarity, TArray<float>& parts_probabilities, TArray<float>& parts_similarities)
{
	probability = -1;
	similarity = -1;
	parts_probabilities.SetNum(0);
	parts_similarities.SetNum(0);
	if (!this->gco)
		return -99;
	double p = 0;
	double s = 0;
	const int n = this->gco->numberOfParts();
	if (n <= 0) {
		return -99;
	}
	double* parts_p = new double[n];
	double* parts_s = new double[n];
	parts_probabilities.SetNum(n);
	parts_similarities.SetNum(n);
	int ret = this->gco->identifyGestureCombination(&p, &s, parts_p, parts_s);
	probability = (float)p;
	similarity = (float)s;
	for (int i = n - 1; i >= 0; i--) {
		parts_probabilities[i] = (float)parts_p[i];
		parts_similarities[i] = (float)parts_s[i];
	}
	delete[] parts_p;
	delete[] parts_s;
	return ret;
}


int AGestureCombinationsActor::contdIdentify(const FVector& HMD_Location, const FRotator& HMD_Rotation, float& Similarity, TArray<float>& PartsProbabilities, TArray<float>& PartsSimilarities)
{
	if (!this->gco)
		return -99;
	FQuat quaternion = HMD_Rotation.Quaternion();
	return this->contdIdentifyQ(HMD_Location, quaternion, Similarity, PartsProbabilities, PartsSimilarities);
}

int AGestureCombinationsActor::contdIdentifyQ(const FVector& HMD_Location, const FQuat& HMD_Rotation, float& Similarity, TArray<float>& PartsProbabilities, TArray<float>& PartsSimilarities)
{
	if (!this->gco)
		return -99;
	double hmd_p[3];
	double hmd_q[4];
	UMiVRyUtil::convertInput(HMD_Location, HMD_Rotation, GestureRecognition_DeviceType::Headset, this->CoordinateSystem, hmd_p, hmd_q);
	double similarity = -1;
	const int num_parts = this->gco->numberOfParts();
	double* parts_probabilities = new double[num_parts];
	double* parts_similarities = new double[num_parts];
	int ret = this->gco->contdIdentify(hmd_p, hmd_q, &similarity, parts_probabilities, parts_similarities);
	Similarity = (float)similarity;
	PartsProbabilities.SetNum(num_parts);
	PartsSimilarities.SetNum(num_parts);
	for (int part = num_parts-1; part >= 0; part--) {
		PartsProbabilities[part] = (float)parts_probabilities[part];
		PartsSimilarities[part] = (float)parts_similarities[part];
	}
	delete[] parts_probabilities;
	delete[] parts_similarities;
	return ret;
}

int AGestureCombinationsActor::contdRecord(const FVector& HMD_Location, const FRotator& HMD_Rotation)
{
	if (!this->gco)
		return -99;
	FQuat quaternion = HMD_Rotation.Quaternion();
	return this->contdRecordQ(HMD_Location, quaternion);
}

int AGestureCombinationsActor::contdRecordQ(const FVector& HMD_Location, const FQuat& HMD_Rotation)
{
	if (!this->gco)
		return -99;
	double hmd_p[3];
	double hmd_q[4];
	UMiVRyUtil::convertInput(HMD_Location, HMD_Rotation, GestureRecognition_DeviceType::Headset, this->CoordinateSystem, hmd_p, hmd_q);
	return this->gco->contdRecord(hmd_p, hmd_q);
}

int AGestureCombinationsActor::getContdIdentificationPeriod(int part)
{
	if (!this->gco)
		return -99;
	return this->gco->getContdIdentificationPeriod(part);
}

int AGestureCombinationsActor::setContdIdentificationPeriod(int part, int ms)
{
	if (!this->gco)
		return -99;
	return this->gco->setContdIdentificationPeriod(part, ms);
}

int AGestureCombinationsActor::getContdIdentificationSmoothing(int part)
{
	if (!this->gco)
		return -99;
	return this->gco->getContdIdentificationSmoothing(part);
}

int AGestureCombinationsActor::setContdIdentificationSmoothing(int part, int samples)
{
	if (!this->gco)
		return -99;
	return this->gco->setContdIdentificationSmoothing(part, samples);
}

int AGestureCombinationsActor::numberOfGestures(int part)
{
	if (!this->gco)
		return -99;
	return this->gco->numberOfGestures(part);
}


int AGestureCombinationsActor::deleteGesture(int part, int index)
{
	if (!this->gco)
		return -99;
	return this->gco->deleteGesture(part, index);
}


int AGestureCombinationsActor::deleteAllGestures(int part)
{
	if (!this->gco)
		return -99;
	return this->gco->deleteAllGestures(part);
}


int AGestureCombinationsActor::createGesture(int part, const FString& name)
{
	if (!this->gco)
		return -99;
	const char* name_str = TCHAR_TO_ANSI(*name);
	return this->gco->createGesture(part, name_str, nullptr);
}


int AGestureCombinationsActor::copyGesture(int from_part, int from_gesture_index, int to_part, int to_gesture_index, bool mirror_x, bool mirror_y, bool mirror_z)
{
	if (!this->gco)
		return -99;
	return this->gco->copyGesture(from_part, from_gesture_index, to_part, to_gesture_index, mirror_x, mirror_y, mirror_z);
}


float AGestureCombinationsActor::gestureRecognitionScore(int part, bool all_samples)
{
	if (!this->gco)
		return -99;
	return (float)this->gco->gestureRecognitionScore(part, all_samples);
}


FString AGestureCombinationsActor::getGestureName(int part, int index)
{
	if (!this->gco)
		return "";
	return this->gco->getGestureName(part, index);
}

int AGestureCombinationsActor::getGestureNumberOfSamples(int part, int index)
{
	if (!this->gco)
		return -99;
	return this->gco->getGestureNumberOfSamples(part, index);
}

int AGestureCombinationsActor::getGestureSampleLength(int part, int gesture_index, int sample_index, bool processed)
{
	if (!this->gco)
		return -99;
	return this->gco->getGestureSampleLength(part, gesture_index, sample_index, processed);
}

int AGestureCombinationsActor::getGestureSampleStroke(int part, int gesture_index, int sample_index, bool processed, FVector& HMD_Location, FRotator& HMD_Rotation, TArray<FVector>& Locations, TArray<FRotator>& Rotations)
{
	if (!this->gco)
		return -99;
	
	int sample_len = this->gco->getGestureSampleLength(part, gesture_index, sample_index, processed);
	if (sample_len <= 0)
		return sample_len;
	double hmd_p[3];
	double hmd_q[4];
	double* p = new double[3 * sample_len];
	double* q = new double[4 * sample_len];
	int ret = this->gco->getGestureSampleStroke(part, gesture_index, sample_index, processed, hmd_p, hmd_q, (double(*)[3])p, (double(*)[4])q, sample_len);
	if (ret < 0) {
		delete[] p;
		delete[] q;
		return ret;
	}
	const int len = (sample_len < ret) ? sample_len : ret;
	Locations.SetNum(len);
	Rotations.SetNum(len);
	FQuat quat;
	UMiVRyUtil::convertOutput(this->CoordinateSystem, hmd_p, hmd_q, GestureRecognition_DeviceType::Headset, HMD_Location, quat);
	HMD_Rotation = quat.Rotator();
	for (int i = 0; i < len; i++) {
		Locations[i].X = (float)p[3 * i + 0]; // x = primart axis
		Locations[i].Y = (float)p[3 * i + 1]; // y = secondary axis
		Locations[i].Z = (float)p[3 * i + 2]; // z = least-significant axis
		quat.X = (float)q[4 * i + 0];
		quat.Y = (float)q[4 * i + 1];
		quat.Z = (float)q[4 * i + 2];
		quat.W = (float)q[4 * i + 3];
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

int AGestureCombinationsActor::getGestureMeanLength(int part, int gesture_index)
{
	if (!this->gco)
		return -99;
	return this->gco->getGestureMeanLength(part, gesture_index);
}

int AGestureCombinationsActor::getGestureMeanStroke(int part, int gesture_index, TArray<FVector>& Locations, TArray<FRotator>& Rotations, FVector& GestureLocation, FRotator& GestureRotation, float& GestureScale)
{
	if (!this->gco)
		return -99;

	int mean_len = this->gco->getGestureMeanLength(part, gesture_index);
	if (mean_len <= 0)
		return mean_len;
	double* p = new double[3 * mean_len];
	double* q = new double[4 * mean_len];
	double hmd_p[3];
	double hmd_q[4];
	double scale;
	int ret = this->gco->getGestureMeanStroke(part, gesture_index, (double(*)[3])p, (double(*)[4])q, mean_len, hmd_p, hmd_q, &scale);
	if (ret < 0) {
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

int AGestureCombinationsActor::deleteGestureSample(int part, int gesture_index, int sample_index)
{
	if (!this->gco)
		return -99;
	return this->gco->deleteGestureSample(part, gesture_index, sample_index);
}

int AGestureCombinationsActor::deleteAllGestureSamples(int part, int gesture_index)
{
	if (!this->gco)
		return -99;
	return this->gco->deleteAllGestureSamples(part, gesture_index);
}

int AGestureCombinationsActor::setGestureName(int part, int index, const FString& name)
{
	if (!this->gco)
		return false;
	const char* name_str = TCHAR_TO_ANSI(*name);
	return this->gco->setGestureName(part, index, name_str);
}

int AGestureCombinationsActor::saveToFile(const FFilePath& path)
{
	if (!this->gco)
		return -99;
	FString path_str = path.FilePath;
	if (FPaths::IsRelative(path_str)) {
		path_str = FPaths::Combine(FPaths::ProjectDir(), path_str);
	}
	return this->gco->saveToFile(TCHAR_TO_ANSI(*path_str));
}

int AGestureCombinationsActor::loadFromFile(const FFilePath& path)
{
	if (!this->gco)
		return -99;
	FString path_str;
	GestureRecognition_Result result;
	UMiVRyUtil::findFile(path.FilePath, result, path_str);
	if (result == GestureRecognition_Result::Then) {
		return this->gco->loadFromFile(TCHAR_TO_ANSI(*path_str));
	}
	TArray<uint8> buffer;
	UMiVRyUtil::readFileToBuffer(path.FilePath, result, buffer);
	if (result == GestureRecognition_Result::Then) {
		return this->gco->loadFromBuffer((const char*)buffer.GetData(), buffer.Num(), nullptr);
	}
	return -3;
}

int AGestureCombinationsActor::loadFromBuffer(const TArray<uint8>& buffer)
{
	if (!this->gco)
		return -99;
	return this->gco->loadFromBuffer((const char*)buffer.GetData(), buffer.Num(), nullptr);
}

int AGestureCombinationsActor::importFromFile(const FFilePath& path)
{
	if (!this->gco)
		return -99;
	FString path_str;
	GestureRecognition_Result result;
	UMiVRyUtil::findFile(path.FilePath, result, path_str);
	if (result == GestureRecognition_Result::Then) {
		return this->gco->importFromFile(TCHAR_TO_ANSI(*path_str));
	}
	TArray<uint8> buffer;
	UMiVRyUtil::readFileToBuffer(path.FilePath, result, buffer);
	if (result == GestureRecognition_Result::Then) {
		return this->gco->importFromBuffer((const char*)buffer.GetData(), buffer.Num(), nullptr);
	}
	return -3;
}

int AGestureCombinationsActor::importFromBuffer(const TArray<uint8>& buffer)
{
	if (!this->gco)
		return -99;
	return this->gco->importFromBuffer((const char*)buffer.GetData(), buffer.Num(), nullptr);
}

int AGestureCombinationsActor::numberOfGestureCombinations()
{
	if (!this->gco)
		return -99;
	return this->gco->numberOfGestureCombinations();
}

int AGestureCombinationsActor::deleteGestureCombination(int index)
{
	if (!this->gco)
		return -99;
	return this->gco->deleteGestureCombination(index);
}

int AGestureCombinationsActor::deleteAllGestureCombinations()
{
	if (!this->gco)
		return -99;
	return this->gco->deleteAllGestureCombinations();
}

int AGestureCombinationsActor::createGestureCombination(const FString& name)
{
	if (!this->gco)
		return -99;
	const char* name_str = TCHAR_TO_ANSI(*name);
	return this->gco->createGestureCombination(name_str);
}

int AGestureCombinationsActor::setCombinationPartGesture(int combination_index, int part, int gesture_index)
{
	if (!this->gco)
		return -99;
	return this->gco->setCombinationPartGesture(combination_index, part, gesture_index);
}

int AGestureCombinationsActor::getCombinationPartGesture(int combination_index, int part)
{
	if (!this->gco)
		return -99;
	return this->gco->getCombinationPartGesture(combination_index, part);
}

FString AGestureCombinationsActor::getGestureCombinationName(int index)
{
	if (!this->gco)
		return "";
	return this->gco->getGestureCombinationName(index);
}

int AGestureCombinationsActor::setGestureCombinationName(int index, const FString& name)
{
	if (!this->gco)
		return -99;
	const char* name_str = TCHAR_TO_ANSI(*name);
	return this->gco->setGestureCombinationName(index, name_str);
}

int AGestureCombinationsActor::loadFromFileAsync(const FFilePath& path)
{
	if (!this->gco)
		return -99;
	int ret;
	ret = this->gco->setLoadingUpdateCallbackFunction((IGestureRecognition::LoadingCallbackFunction*)&LoadingCallbackFunction);
	if (ret != 0)
		return ret;
	ret = this->gco->setLoadingFinishCallbackFunction((IGestureRecognition::LoadingCallbackFunction*)&LoadingCallbackFunction);
	if (ret != 0)
		return ret;
	ret = this->gco->setLoadingUpdateCallbackMetadata(&this->LoadingUpdateMetadata);
	if (ret != 0)
		return ret;
	ret = this->gco->setLoadingFinishCallbackMetadata(&this->LoadingFinishMetadata);
	if (ret != 0)
		return ret;
	FString path_str;
	GestureRecognition_Result result;
	UMiVRyUtil::findFile(path.FilePath, result, path_str);
	if (result == GestureRecognition_Result::Then) {
		return this->gco->loadFromFileAsync(TCHAR_TO_ANSI(*path_str), nullptr);
	}
	TArray<uint8> buffer;
	UMiVRyUtil::readFileToBuffer(path.FilePath, result, buffer);
	if (result == GestureRecognition_Result::Then) {
		return this->gco->loadFromBufferAsync((const char*)buffer.GetData(), buffer.Num(), nullptr);
	}
	return -3;
	
}

int AGestureCombinationsActor::loadFromBufferAsync(const TArray<uint8>& buffer)
{
	if (!this->gco)
		return -99;
	int ret;
	ret = this->gco->setLoadingUpdateCallbackFunction((IGestureRecognition::LoadingCallbackFunction*)&LoadingCallbackFunction);
	if (ret != 0)
		return ret;
	ret = this->gco->setLoadingFinishCallbackFunction((IGestureRecognition::LoadingCallbackFunction*)&LoadingCallbackFunction);
	if (ret != 0)
		return ret;
	ret = this->gco->setLoadingUpdateCallbackMetadata(&this->LoadingUpdateMetadata);
	if (ret != 0)
		return ret;
	ret = this->gco->setLoadingFinishCallbackMetadata(&this->LoadingFinishMetadata);
	if (ret != 0)
		return ret;
	return this->gco->loadFromBufferAsync((const char*)buffer.GetData(), buffer.Num(), nullptr);
}

bool AGestureCombinationsActor::isLoading()
{
	if (!this->gco)
		return false;
	return this->gco->isLoading();
}

int AGestureCombinationsActor::cancelLoading()
{
	if (!this->gco)
		return -99;
	return this->gco->cancelLoading();
}

int AGestureCombinationsActor::startTraining()
{
	if (!this->gco)
		return -99;
	this->gco->trainingUpdateCallback = (IGestureRecognition::TrainingCallbackFunction*)&TrainingCallbackFunction;
	this->gco->trainingFinishCallback = (IGestureRecognition::TrainingCallbackFunction*)&TrainingCallbackFunction;
	this->gco->trainingUpdateCallbackMetadata = &this->TrainingUpdateMetadata;
	this->gco->trainingFinishCallbackMetadata = &this->TrainingFinishMetadata;
	return this->gco->startTraining();
}

bool AGestureCombinationsActor::isTraining()
{
	if (!this->gco)
		return false;
	return this->gco->isTraining();
}

void AGestureCombinationsActor::stopTraining()
{
	if (!this->gco)
		return;
	this->gco->stopTraining();
}


float AGestureCombinationsActor::recognitionScore()
{
	if (!this->gco)
		return -99;
	return (float)this->gco->recognitionScore();
}


int AGestureCombinationsActor::getMaxTrainingTime()
{
	if (!this->gco)
		return -99;
	return (int)this->gco->getMaxTrainingTime();
}

void AGestureCombinationsActor::setMaxTrainingTime(int t)
{
	if (!this->gco)
		return;
	this->gco->setMaxTrainingTime((unsigned long)t);
}

GestureRecognition_FrameOfReference AGestureCombinationsActor::getRotationalFrameOfReferenceRoll()
{
	if (!this->gco) {
		return (GestureRecognition_FrameOfReference)-1;
	}
	return (GestureRecognition_FrameOfReference)this->gco->rotationalFrameOfReference.x;
}

void AGestureCombinationsActor::setRotationalFrameOfReferenceRoll(GestureRecognition_FrameOfReference i)
{
	if (!this->gco) {
		return;
	}
	this->gco->rotationalFrameOfReference.x = IGestureRecognition::FrameOfReference(i);
}

GestureRecognition_FrameOfReference AGestureCombinationsActor::getRotationalFrameOfReferencePitch()
{
	if (!this->gco) {
		return (GestureRecognition_FrameOfReference)-1;
	}
	return (GestureRecognition_FrameOfReference)this->gco->rotationalFrameOfReference.y;
}

void AGestureCombinationsActor::setRotationalFrameOfReferencePitch(GestureRecognition_FrameOfReference i)
{
	if (!this->gco) {
		return;
	}
	this->gco->rotationalFrameOfReference.y = IGestureRecognition::FrameOfReference(i);
}

GestureRecognition_FrameOfReference AGestureCombinationsActor::getRotationalFrameOfReferenceYaw()
{
	if (!this->gco) {
		return (GestureRecognition_FrameOfReference)-1;
	}
	return (GestureRecognition_FrameOfReference)this->gco->rotationalFrameOfReference.z;
}

void AGestureCombinationsActor::setRotationalFrameOfReferenceYaw(GestureRecognition_FrameOfReference i)
{
	if (!this->gco) {
		return;
	}
	this->gco->rotationalFrameOfReference.z = IGestureRecognition::FrameOfReference(i);
}

void AGestureCombinationsActor::TrainingCallbackFunction(double performance, TrainingCallbackMetadata* metadata) {
	if (!metadata || !metadata->delegate || !metadata->actor)
		return;
	metadata->delegate->Broadcast(metadata->actor, performance);
};

void AGestureCombinationsActor::LoadingCallbackFunction(int result, LoadingCallbackMetadata* metadata)
{
	if (!metadata || !metadata->delegate || !metadata->actor) {
		return;
	}
	metadata->delegate->Broadcast(metadata->actor, result);
};

