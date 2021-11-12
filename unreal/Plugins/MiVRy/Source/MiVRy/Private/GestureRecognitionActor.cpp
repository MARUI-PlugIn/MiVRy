/*
 * MiVRy - VR gesture recognition library plug-in for Unreal.
 * Version 1.20
 * Copyright (c) 2021 MARUI-PlugIn (inc.)
 *
 * MiVRy is licensed under a Creative Commons Attribution-NonCommercial 4.0 International License
 * ( http://creativecommons.org/licenses/by-nc/4.0/ )
 *
 * This software is free to use for non-commercial purposes.
 * You may use this software in part or in full for any project
 * that does not pursue financial gain, including free software
 * and projects completed for evaluation or educational purposes only.
 * Any use for commercial purposes is prohibited.
 * You may not sell or rent any software that includes
 * this software in part or in full, either in it's original form
 * or in altered form.
 * If you wish to use this software in a commercial application,
 * please contact us at support@marui-plugin.com to obtain
 * a commercial license.
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
	}
}

void AGestureRecognitionActor::Tick(float DeltaTime)
{
	Super::Tick(DeltaTime);
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
	if (this->UnityCombatibilityMode) {
		p[0] = HMD_Position.Y * 0.01f; // Unity.X = right = Unreal.Y
		p[1] = HMD_Position.Z * 0.01f; // Unity.Y = up    = Unreal.Z
		p[2] = HMD_Position.X * 0.01f; // Unity.Z = front = Unreal.X
		q[0] = HMD_Rotation.Y;
		q[1] = HMD_Rotation.Z;
		q[2] = HMD_Rotation.X;
		q[3] = HMD_Rotation.W;
	} else {
		p[0] = HMD_Position.X;
		p[1] = HMD_Position.Y;
		p[2] = HMD_Position.Z;
		q[0] = HMD_Rotation.X;
		q[1] = HMD_Rotation.Y;
		q[2] = HMD_Rotation.Z;
		q[3] = HMD_Rotation.W;
	}
	return this->gro->startStroke(p, q, RecordAsSample);
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
	if (this->UnityCombatibilityMode) {
		p[0] = HandPosition.Y * 0.01f; // Unity.X = right = Unreal.Y
		p[1] = HandPosition.Z * 0.01f; // Unity.Y = up    = Unreal.Z
		p[2] = HandPosition.X * 0.01f; // Unity.Z = front = Unreal.X
		q[0] = HandRotation.Y;
		q[1] = HandRotation.Z;
		q[2] = HandRotation.X;
		q[3] = HandRotation.W;
	} else {
		p[0] = HandPosition.X;
		p[1] = HandPosition.Y;
		p[2] = HandPosition.Z;
		q[0] = HandRotation.X;
		q[1] = HandRotation.Y;
		q[2] = HandRotation.Z;
		q[3] = HandRotation.W;
	}
	return this->gro->contdStrokeQ(p, q);
	return 0;
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
	if (this->UnityCombatibilityMode) {
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
	if (this->UnityCombatibilityMode) {
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
	if (this->UnityCombatibilityMode) {
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
	double hmd_p[3];
	double hmd_q[4];
	if (this->UnityCombatibilityMode) {
		hmd_p[0] = HMD_Position.Y * 0.01f; // Unity.X = right = Unreal.Y
		hmd_p[1] = HMD_Position.Z * 0.01f; // Unity.Y = up    = Unreal.Z
		hmd_p[2] = HMD_Position.X * 0.01f; // Unity.Z = front = Unreal.X
		hmd_q[0] = quat.Y;
		hmd_q[1] = quat.Z;
		hmd_q[2] = quat.X;
		hmd_q[3] = quat.W;
	} else {
		hmd_p[0] = HMD_Position.X;
		hmd_p[1] = HMD_Position.Y;
		hmd_p[2] = HMD_Position.Z;
		hmd_q[0] = quat.X;
		hmd_q[1] = quat.Y;
		hmd_q[2] = quat.Z;
		hmd_q[3] = quat.W;
	}
	double similarity = -1;
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
	if (this->UnityCombatibilityMode) {
		hmd_p[0] = HMD_Position.Y * 0.01f; // Unity.X = right = Unreal.Y
		hmd_p[1] = HMD_Position.Z * 0.01f; // Unity.Y = up    = Unreal.Z
		hmd_p[2] = HMD_Position.X * 0.01f; // Unity.Z = front = Unreal.X
		hmd_q[0] = quat.Y;
		hmd_q[1] = quat.Z;
		hmd_q[2] = quat.X;
		hmd_q[3] = quat.W;
	} else {
		hmd_p[0] = HMD_Position.X;
		hmd_p[1] = HMD_Position.Y;
		hmd_p[2] = HMD_Position.Z;
		hmd_q[0] = quat.X;
		hmd_q[1] = quat.Y;
		hmd_q[2] = quat.Z;
		hmd_q[3] = quat.W;
	}
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
	if (this->UnityCombatibilityMode) {
		hmd_p[0] = HMD_Position.Y * 0.01f; // Unity.X = right = Unreal.Y
		hmd_p[1] = HMD_Position.Z * 0.01f; // Unity.Y = up    = Unreal.Z
		hmd_p[2] = HMD_Position.X * 0.01f; // Unity.Z = front = Unreal.X
		hmd_q[0] = HMD_Quaternion.Y;
		hmd_q[1] = HMD_Quaternion.Z;
		hmd_q[2] = HMD_Quaternion.X;
		hmd_q[3] = HMD_Quaternion.W;
	} else {
		hmd_p[0] = HMD_Position.X;
		hmd_p[1] = HMD_Position.Y;
		hmd_p[2] = HMD_Position.Z;
		hmd_q[0] = HMD_Quaternion.X;
		hmd_q[1] = HMD_Quaternion.Y;
		hmd_q[2] = HMD_Quaternion.Z;
		hmd_q[3] = HMD_Quaternion.W;
	}
	int ret = this->gro->contdRecord(hmd_p, hmd_q);
	return ret;
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
	const char* name_str = TCHAR_TO_ANSI(*name);
	return this->gro->createGesture(name_str, nullptr);
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
	const char* name_str = TCHAR_TO_ANSI(*name);
	return this->gro->setGestureName(index, name_str);
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

int AGestureRecognitionActor::getGestureSampleStroke(int gesture_index, int sample_index, bool processed, FVector& HMD_Location, FRotator& HMD_Rotation, TArray<FVector>& Locations, TArray<FRotator>& Rotations)
{
	if (!this->gro) {
		return -99;
	}
	const int sample_len = this->gro->getGestureSampleLength(gesture_index, sample_index, processed);
	if (sample_len <= 0) {
		return sample_len;
	}
	double hmd_p[3];
	double hmd_q[4];
	double* p = new double[3 * sample_len];
	double* q = new double[4 * sample_len];
	int ret = this->gro->getGestureSampleStroke(gesture_index, sample_index, processed, hmd_p, hmd_q, (double(*)[3])p, (double(*)[4])q, sample_len);
	if (ret <= 0) {
		delete[] p;
		delete[] q;
		Locations.Empty(0);
		Rotations.Empty(0);
		return ret;
	}
	const int len = (ret < sample_len) ? ret : sample_len;
	Locations.SetNum(len);
	Rotations.SetNum(len);
	FQuat quat;
	if (this->UnityCombatibilityMode) {
		HMD_Location.X = (float)hmd_p[2] * 100.0f; // Unreal.X = front = Unity.Z
		HMD_Location.Y = (float)hmd_p[0] * 100.0f; // Unreal.Y = right = Unity.X
		HMD_Location.Z = (float)hmd_p[1] * 100.0f; // Unreal.Z = up    = Unity.Y
		quat.X = (float)hmd_q[2]; // Unreal.X = front = Unity.Z
		quat.Y = (float)hmd_q[0]; // Unreal.Y = right = Unity.X
		quat.Z = (float)hmd_q[1]; // Unreal.Z = up    = Unity.Y
		quat.W = (float)hmd_q[3];
		HMD_Rotation = quat.Rotator();
		for (int i = 0; i < len; i++) {
			Locations[i].X = (float)p[3 * i + 2] * 100.0f; // Unreal.X = front = Unity.Z
			Locations[i].Y = (float)p[3 * i + 0] * 100.0f; // Unreal.Y = right = Unity.X
			Locations[i].Z = (float)p[3 * i + 1] * 100.0f; // Unreal.Z = up    = Unity.Y
			quat.X = (float)q[4 * i + 2]; // Unreal.X = front = Unity.Z
			quat.Y = (float)q[4 * i + 0]; // Unreal.Y = right = Unity.X
			quat.Z = (float)q[4 * i + 1]; // Unreal.Z = up    = Unity.Y
			quat.W = (float)q[4 * i + 3];
			Rotations[i] = quat.Rotator();
		}
	} else {
		HMD_Location.X = (float)hmd_p[0];
		HMD_Location.Y = (float)hmd_p[1];
		HMD_Location.Z = (float)hmd_p[2];
		quat.X = (float)hmd_q[0];
		quat.Y = (float)hmd_q[1];
		quat.Z = (float)hmd_q[2];
		quat.W = (float)hmd_q[3];
		HMD_Rotation = quat.Rotator();
		for (int i = 0; i < len; i++) {
			Locations[i].X = (float)p[3 * i + 0];
			Locations[i].Y = (float)p[3 * i + 1];
			Locations[i].Z = (float)p[3 * i + 2];
			quat.X = (float)q[4 * i + 0];
			quat.Y = (float)q[4 * i + 1];
			quat.Z = (float)q[4 * i + 2];
			quat.W = (float)q[4 * i + 3];
			Rotations[i] = quat.Rotator();
		}
	}
	delete[] p;
	delete[] q;
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
	FQuat quat;
	if (this->UnityCombatibilityMode) {
		GestureLocation.X = (float)hmd_p[2] * 100.0f; // Unreal.X = front = Unity.Z
		GestureLocation.Y = (float)hmd_p[0] * 100.0f; // Unreal.Y = right = Unity.X
		GestureLocation.Z = (float)hmd_p[1] * 100.0f; // Unreal.Z = up    = Unity.Y
		quat.X = (float)hmd_q[2]; // Unreal.X = front = Unity.Z
		quat.Y = (float)hmd_q[0]; // Unreal.Y = right = Unity.X
		quat.Z = (float)hmd_q[1]; // Unreal.Z = up    = Unity.Y
		quat.W = (float)hmd_q[3];
		GestureRotation = quat.Rotator();
		GestureScale = (float)scale * 100.0f;
	} else {
		GestureLocation.X = (float)hmd_p[0];
		GestureLocation.Y = (float)hmd_p[1];
		GestureLocation.Z = (float)hmd_p[2];
		quat.X = (float)hmd_q[0];
		quat.Y = (float)hmd_q[1];
		quat.Z = (float)hmd_q[2];
		quat.W = (float)hmd_q[3];
		GestureRotation = quat.Rotator();
		GestureScale = (float)scale;
	}
	const int len = (ret < mean_len) ? ret : mean_len;
	Locations.SetNum(len);
	Rotations.SetNum(len);
	for (int i = 0; i < len; i++) {
		Locations[i].X = (float)p[3 * i + 0];
		Locations[i].Y = (float)p[3 * i + 1];
		Locations[i].Z = (float)p[3 * i + 2];
		quat.X = (float)q[4 * i + 0];
		quat.Y = (float)q[4 * i + 1];
		quat.Z = (float)q[4 * i + 2];
		quat.W = (float)q[4 * i + 3];
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
	FString path_str = path.FilePath;
	if (FPaths::IsRelative(path_str)) {
		path_str = FPaths::Combine(FPaths::ProjectDir(), path_str);
	}
	return this->gro->loadFromFile(TCHAR_TO_ANSI(*path_str), nullptr);
}

int AGestureRecognitionActor::importFromFile(const FFilePath& path)
{
	if (!this->gro) {
		return -99;
	}
	FString path_str = path.FilePath;
	if (FPaths::IsRelative(path_str)) {
		path_str = FPaths::Combine(FPaths::ProjectDir(), path_str);
	}
	return this->gro->importFromFile(TCHAR_TO_ANSI(*path_str), nullptr);
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

void AGestureRecognitionActor::TrainingCallbackFunction(double performance, TrainingCallbackMetadata* metadata)
{
	if (!metadata || !metadata->delegate || !metadata->actor) {
		return;
	}
	metadata->delegate->Broadcast(metadata->actor, performance);
};
