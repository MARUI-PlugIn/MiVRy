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

#include "MiVRyActor.h"
#include "Interfaces/IPluginManager.h"
#include "Misc/MessageDialog.h"
#include "Misc/Paths.h"
#include "Engine/GameInstance.h"
#include "IXRTrackingSystem.h"
#include "GestureRecognition.h"
#include "GestureCombinations.h"
#include "Misc/FileHelper.h"

// Sets default values
AMiVRyActor::AMiVRyActor()
{
	PrimaryActorTick.bCanEverTick = true;
	this->bBlockInput = 0;
	this->AutoReceiveInput = EAutoReceiveInput::Player0;
	// auto plugin = IPluginManager::Get().FindPlugin("MiVRy");
	// if (!plugin.IsValid()) {
	// 	FMessageDialog::Open(EAppMsgType::Ok, FText::FromString("MiVRy Actor could not access plugin"));
	// 	return;
	// }
}


AMiVRyActor::~AMiVRyActor()
{
	if (this->gro) {
		GestureRecognition_delete(this->gro);
		this->gro = nullptr;
	}
	if (this->gco) {
		GestureCombinations_delete(this->gco);
		this->gco = nullptr;
	}
}

void AMiVRyActor::BeginPlay()
{
	Super::BeginPlay();

	if (this->InputComponent != nullptr) {
		if (!this->LeftTriggerInput.IsNone() && this->LeftTriggerInput != "") {
			auto& BindingPressed = this->InputComponent->BindAction(this->LeftTriggerInput, IE_Pressed, this, &AMiVRyActor::LeftTriggerInputPressed);
			if (BindingPressed.IsValid()) {
				BindingPressed.bConsumeInput = 0;
			} else {
				UE_LOG(LogTemp, Warning, TEXT("[MiVRyActor] Failed to bind input %s"), *LeftTriggerInput.ToString());
			}
			auto& BindingReleased = this->InputComponent->BindAction(this->LeftTriggerInput, IE_Released, this, &AMiVRyActor::LeftTriggerInputReleased);
			if (BindingReleased.IsValid()) {
				BindingReleased.bConsumeInput = 0;
			} else {
				UE_LOG(LogTemp, Warning, TEXT("[MiVRyActor] Failed to bind input %s"), *LeftTriggerInput.ToString());
			}
		}
		if (!this->RightTriggerInput.IsNone() && this->RightTriggerInput != "") {
			auto& BindingPressed = this->InputComponent->BindAction(this->RightTriggerInput, IE_Pressed, this, &AMiVRyActor::RightTriggerInputPressed);
			if (BindingPressed.IsValid()) {
				BindingPressed.bConsumeInput = 0;
			} else {
				UE_LOG(LogTemp, Warning, TEXT("[MiVRyActor] Failed to bind input %s"), *RightTriggerInput.ToString());
			}
			auto& BindingReleased = this->InputComponent->BindAction(this->RightTriggerInput, IE_Released, this, &AMiVRyActor::RightTriggerInputReleased);
			if (BindingReleased.IsValid()) {
				BindingReleased.bConsumeInput = 0;
			} else {
				UE_LOG(LogTemp, Warning, TEXT("[MiVRyActor] Failed to bind input %s"), *RightTriggerInput.ToString());
			}
		}
	} else if (!this->LeftTriggerInput.IsNone() || !this->RightTriggerInput.IsNone()) {
		UE_LOG(LogTemp, Warning, TEXT("[MiVRyActor] Trigger Inputs are set but the actor has no InputComponend. Auto Receive Input setting missing?"));
	}

	FString path = this->GestureDatabaseFile.FilePath;
	TArray64<uint8> file_contents;
	if (FFileHelper::LoadFileToArray(file_contents, *path)) {
		UE_LOG(LogTemp, Display, TEXT("[MiVRyActor] Found gesture database file at '%s'"), *path);
	} else {
		UE_LOG(LogTemp, Display, TEXT("[MiVRyActor] Could not find gesture database file at '%s'"), *path);
		if (FPaths::IsRelative(path)) {
			FString relpath = FPaths::Combine(FPaths::ProjectDir(), path);
			if (FFileHelper::LoadFileToArray(file_contents, *relpath)) {
				UE_LOG(LogTemp, Display, TEXT("[MiVRyActor] Found gesture database file at '%s'"), *relpath);
			} else {
				UE_LOG(LogTemp, Display, TEXT("[MiVRyActor] Could not find gesture database file at '%s'"), *relpath);
				FString abspath = FPaths::ConvertRelativePathToFull(relpath);
				if (FFileHelper::LoadFileToArray(file_contents, *abspath)) {
					UE_LOG(LogTemp, Display, TEXT("[MiVRyActor] Found gesture database file at '%s'"), *abspath);
				} else {
					UE_LOG(LogTemp, Display, TEXT("[MiVRyActor] Could not find gesture database file at '%s'"), *abspath);
					UE_LOG(LogTemp, Error, TEXT("[MiVRyActor] Could not find gesture database file anywhere. Not packaged?"));
					return;
				}
			}
		} else {
			UE_LOG(LogTemp, Error, TEXT("[MiVRyActor] Could not find gesture database file anywhere. Not packaged?"));
			return;
		}
	}

	// Try one-part/one-hand gesture recognition object
	this->gro = (IGestureRecognition*)GestureRecognition_create();
	if (this->gro == nullptr) {
		UE_LOG(LogTemp, Error, TEXT("[MiVRyActor] Failed to create GestureRecognition object"));
		return;
	}
	int ret = this->gro->loadFromBuffer((const char*)file_contents.GetData(), file_contents.Num(), nullptr);
	if (ret == 0) {
		UE_LOG(LogTemp, Display, TEXT("[MiVRyActor] Successfully loaded gesture database file."));
		return; // successfully loaded
	} // else: failed to load
	delete this->gro;
	this->gro = nullptr;

	// try multi-part gesture combinations object
	this->gco = (IGestureCombinations*)GestureCombinations_create(0);
	if (this->gco == nullptr) {
		UE_LOG(LogTemp, Error, TEXT("[MiVRyActor] Failed to create GestureCombinations object"));
		return;
	}
	ret = this->gco->loadFromBuffer((const char*)file_contents.GetData(), file_contents.Num(), nullptr);
	if (ret == 0) {
		UE_LOG(LogTemp, Display, TEXT("[MiVRyActor] Successfully loaded gesture database file."));
		return; // successfully loaded
	} // else: failed to load
	delete this->gco;
	this->gco = nullptr;

	FString error_str = UMiVRyUtil::errorCodeToString(ret);
	UE_LOG(LogTemp, Error, TEXT("[MiVRyActor] Failed to load gesture database file %s: %s"), *this->GestureDatabaseFile.FilePath, *error_str);
}

void AMiVRyActor::Tick(float DeltaTime)
{
	Super::Tick(DeltaTime);

	UMotionControllerComponent* motion_controllers[2];
	motion_controllers[(uint8)GestureRecognition_Side::Left ] = this->LeftMotionController;
	motion_controllers[(uint8)GestureRecognition_Side::Right] = this->RightMotionController;
	AActor* hand_actors[2];
	hand_actors[(uint8)GestureRecognition_Side::Left ] = this->LeftHandActor;
	hand_actors[(uint8)GestureRecognition_Side::Right] = this->RightHandActor;
	EControllerHand controller_hand[2];
	controller_hand[(uint8)GestureRecognition_Side::Left ] = EControllerHand::Left;
	controller_hand[(uint8)GestureRecognition_Side::Right] = EControllerHand::Right;

	for (int side = 1; side >= 0; side--) {
		if (!side_active[side])
			continue;
		FVector location;
		FRotator rotation;
		UMotionControllerComponent* motion_controller = motion_controllers[side];
		AActor* hand_actor = hand_actors[side];
		if (motion_controller) {
			// motion_controller->Activate(true);
			location = motion_controller->GetComponentLocation();
			rotation = motion_controller->GetComponentRotation();
		} else if (hand_actor) {
			location = hand_actor->GetActorLocation();
			rotation = hand_actor->GetActorRotation();
		} else {
			if (GEngine == nullptr || !GEngine->XRSystem.IsValid()) {
				continue;
			}
			FXRMotionControllerData data;
			GEngine->XRSystem->GetMotionControllerData(nullptr, controller_hand[side], data);
			if (data.GripPosition.IsZero()) {
				UE_LOG(LogTemp, Warning, TEXT("[MiVRyActor] unable to track %s controller (device name: '%s')"),
					(side == (uint8)GestureRecognition_Side::Left ? TEXT("left") : TEXT("right")), *data.DeviceName.ToString());
				continue;
			}
			location = data.GripPosition;
			rotation = data.GripRotation.Rotator();
		}
		FQuat quaternion = rotation.Quaternion();
		double p[3];
		double q[4];
		if (this->UnityGestureDatabaseFile) {
			p[0] = location.Y * 0.01f; // Unity.X = right = Unreal.Y
			p[1] = location.Z * 0.01f; // Unity.Y = up    = Unreal.Z
			p[2] = location.X * 0.01f; // Unity.Z = front = Unreal.X
			q[0] = quaternion.Y;
			q[1] = quaternion.Z;
			q[2] = quaternion.X;
			q[3] = quaternion.W;
		} else {
			p[0] = location.X;
			p[1] = location.Y;
			p[2] = location.Z;
			q[0] = quaternion.X;
			q[1] = quaternion.Y;
			q[2] = quaternion.Z;
			q[3] = quaternion.W;
		}
		if (this->gro) {
			int ret = this->gro->contdStrokeQ(p, q);
			if (ret != 0)
				UE_LOG(LogTemp, Warning, TEXT("[MiVRyActor] GestureRecognition::contdStroke() failed with %i"), ret);
		} else if (this->gco) {
			int ret = this->gco->contdStrokeQ(side, p, q);
			if (ret != 0)
				UE_LOG(LogTemp, Warning, TEXT("[MiVRyActor] GestureCombinations::contdStroke() failed with %i"), ret);
		} else {
			UE_LOG(LogTemp, Warning, TEXT("[MiVRyActor.Tick] GestureRecognition object was not created. Failed to load database file?"));
		}
	}
}

void AMiVRyActor::startGesturing(GestureRecognition_Result& Result, int& ErrorCode, GestureRecognition_Side side)
{
	APlayerCameraManager* camManager = GetWorld()->GetFirstPlayerController()->PlayerCameraManager;
	if (camManager == nullptr) {
		Result = GestureRecognition_Result::Error;
		UE_LOG(LogTemp, Warning, TEXT("[MiVRyActor.startGesturing] Could not get camera position."));
		return;
	}
	FVector location = camManager->GetCameraLocation();
	FRotator rotation = camManager->GetCameraRotation();
	FQuat quaternion = rotation.Quaternion();
	double p[3];
	double q[4];
	if (this->UnityGestureDatabaseFile) {
		p[0] = location.Y * 0.01f; // Unity.X = right = Unreal.Y
		p[1] = location.Z * 0.01f; // Unity.Y = up    = Unreal.Z
		p[2] = location.X * 0.01f; // Unity.Z = front = Unreal.X
		q[0] = quaternion.Y;
		q[1] = quaternion.Z;
		q[2] = quaternion.X;
		q[3] = quaternion.W;
	} else {
		p[0] = location.X;
		p[1] = location.Y;
		p[2] = location.Z;
		q[0] = quaternion.X;
		q[1] = quaternion.Y;
		q[2] = quaternion.Z;
		q[3] = quaternion.W;
	}
	if (this->gro) {
		ErrorCode = this->gro->startStroke(p, q, -1);
		if (ErrorCode != 0) {
			Result = GestureRecognition_Result::Error;
			return;
		}
		Result = GestureRecognition_Result::Then;
		this->side_active[(uint8)side] = true;
		return;
	}
	if (this->gco) {
		ErrorCode = this->gco->startStroke((int)side, p, q, -1);
		if (ErrorCode != 0) {
			Result = GestureRecognition_Result::Error;
			return;
		}
		this->side_active[(uint8)side] = true;
		Result = GestureRecognition_Result::Then;
		return;
	}
	UE_LOG(LogTemp, Warning, TEXT("[MiVRyActor.startGesturing] GestureRecognition object was not created. Failed to load database file?"));
	ErrorCode = -1;
	Result = GestureRecognition_Result::Error;
}

void AMiVRyActor::stopGesturing(GestureRecognition_Identification& Result, GestureRecognition_Side side)
{
	this->side_active[(uint8)side] = false;
	if (this->gro) {
		double position[3];
		double scale;
		double dir0[3];
		double dir1[3];
		double dir2[3];
		this->gesture_id = this->gro->endStrokeAndGetSimilarity(&this->similarity, position, &scale, dir0, dir1, dir2);
		if (this->gesture_id < 0) {
			UE_LOG(LogTemp, Warning, TEXT("[MiVRyActor] Identification failed with %i"), this->gesture_id);
			Result = GestureRecognition_Identification::FailedToIdentify;
			return;
		}
		this->parts.SetNum(1);
		FMiVRyGesturePart& part = this->parts[0];
		part.Side = side;
		part.PartGestureID = this->gesture_id;
		if (this->UnityGestureDatabaseFile) {
			part.Position.X = float(position[2]) * 100.0f; // Unreal.X = front = Unity.Z
			part.Position.Y = float(position[0]) * 100.0f; // Unreal.Y = right = Unity.X
			part.Position.Z = float(position[1]) * 100.0f; // Unreal.Z = up    = Unity.Y
			part.Scale = float(scale) * 100.0f;
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
			part.Rotation = rm.Rotator();
			part.PrimaryDirection.X = float(dir0[2]); // Unreal.X = front = Unity.Z
			part.PrimaryDirection.Y = float(dir0[0]); // Unreal.Y = right = Unity.X
			part.PrimaryDirection.Z = float(dir0[1]); // Unreal.Z = up    = Unity.Y
			part.SecondaryDirection.X = float(dir1[2]);
			part.SecondaryDirection.Y = float(dir1[0]);
			part.SecondaryDirection.Z = float(dir1[1]);
		} else {
			part.Position.X = float(position[0]);
			part.Position.Y = float(position[1]);
			part.Position.Z = float(position[2]);
			part.Scale = float(scale);
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
			part.Rotation = rm.Rotator();
			part.PrimaryDirection.X = float(dir0[0]);
			part.PrimaryDirection.Y = float(dir0[1]);
			part.PrimaryDirection.Z = float(dir0[2]);
			part.SecondaryDirection.X = float(dir1[0]);
			part.SecondaryDirection.Y = float(dir1[1]);
			part.SecondaryDirection.Z = float(dir1[2]);
		}
		Result = GestureRecognition_Identification::GestureIdentified;
		return;
	}
	if (this->gco) {
		double position[3];
		double scale;
		double dir0[3];
		double dir1[3];
		double dir2[3];
		int ret = this->gco->endStroke((int)side, position, &scale, dir0, dir1, dir2);
		if (ret < 0) {
			Result = GestureRecognition_Identification::FailedToIdentify;
			UE_LOG(LogTemp, Warning, TEXT("[MiVRyActor] GestureCombinations::endStroke() failed with %i"), Result);
			return;
		}
		FMiVRyGesturePart* part = nullptr;
		for (int i = this->parts.Num() - 1; i >= 0; i--) {
			if (this->parts[i].Side == side) {
				part = &this->parts[i];
			}
		}
		if (part == nullptr) {
			const int32 i = this->parts.Add(FMiVRyGesturePart());
			part = &this->parts[i];
			part->Side = side;
		}
		if (this->UnityGestureDatabaseFile) {
			part->Position.X = float(position[2]) * 100.0f; // Unreal.X = front = Unity.Z
			part->Position.Y = float(position[0]) * 100.0f; // Unreal.Y = right = Unity.X
			part->Position.Z = float(position[1]) * 100.0f; // Unreal.Z = up    = Unity.Y
			part->Scale = float(scale) * 100.0f;
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
			part->Rotation = rm.Rotator();
			part->PrimaryDirection.X = float(dir0[2]);
			part->PrimaryDirection.Y = float(dir0[0]);
			part->PrimaryDirection.Z = float(dir0[1]);
			part->SecondaryDirection.X = float(dir1[2]);
			part->SecondaryDirection.Y = float(dir1[0]);
			part->SecondaryDirection.Z = float(dir1[1]);
		} else {
			part->Position.X = float(position[0]);
			part->Position.Y = float(position[1]);
			part->Position.Z = float(position[2]);
			part->Scale = float(scale);
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
			part->Rotation = rm.Rotator();
			part->PrimaryDirection.X = float(dir0[0]);
			part->PrimaryDirection.Y = float(dir0[1]);
			part->PrimaryDirection.Z = float(dir0[2]);
			part->SecondaryDirection.X = float(dir1[0]);
			part->SecondaryDirection.Y = float(dir1[1]);
			part->SecondaryDirection.Z = float(dir1[2]);
		}
		if (this->side_active[1 - (uint8)side]) {
			Result = GestureRecognition_Identification::WaitingForOtherHand;
			// UE_LOG(LogTemp, Display, TEXT("[MiVRyActor] waiting for other hand on side %i"), (uint8)side);
			return;
		}
		this->gesture_id = this->gco->identifyGestureCombination(&this->similarity);
		if (this->gesture_id < 0) {
			Result = GestureRecognition_Identification::FailedToIdentify;
			for (int i = 0; i < parts.Num(); i++) {
				parts[i].PartGestureID = -1;
			}
			return;
		}
		for (int i = 0; i < parts.Num(); i++) {
			parts[i].PartGestureID = this->gco->getCombinationPartGesture(this->gesture_id, (int)parts[i].Side);
		}
		Result = GestureRecognition_Identification::GestureIdentified;
		return;
	}
	UE_LOG(LogTemp, Warning, TEXT("[MiVRyActor.stopGesturing] GestureRecognition object was not created. Failed to load database file?"));
	Result = GestureRecognition_Identification::FailedToIdentify;
}

void AMiVRyActor::getIdentifiedGestureInfo(GestureRecognition_Result& Result, int& GestureID, FString& GestureName, float& Similarity, TArray<FMiVRyGesturePart>& GestureParts)
{
	GestureID = this->gesture_id;
	Similarity = (float)this->similarity;
	GestureParts = this->parts;
	if (GestureID < 0) {
		Result = GestureRecognition_Result::Error;
		return;
	}
	if (this->gro) {
		GestureName = this->gro->getGestureName(GestureID);
		Result = GestureRecognition_Result::Then;
	} else if (this->gco) {
		GestureName = this->gco->getGestureCombinationName(GestureID);
		Result = GestureRecognition_Result::Then;
	} else {
		UE_LOG(LogTemp, Warning, TEXT("[MiVRyActor.getIdentifiedGestureInfo] GestureRecognition object was not created. Failed to load database file?"));
		GestureName = "";
		Result = GestureRecognition_Result::Error;
	}
}


void AMiVRyActor::getGesturePartAveragePath(const FMiVRyGesturePart& part, GestureRecognition_Result& Result, TArray<FVector>& Locations, TArray<FRotator>& Rotations, FVector& GestureLocation, FRotator& GestureRotation, float& GestureScale)
{
	Locations.Empty(0);
	Rotations.Empty(0);
	GestureLocation = FVector::ZeroVector;
	GestureRotation = FRotator::ZeroRotator;
	GestureScale = 1.0f;
	Result = GestureRecognition_Result::Error;
	if (this->gro) {
		const int mean_len = this->gro->getGestureMeanLength(part.PartGestureID);
		if (mean_len <= 0) {
			return;
		}
		double* p = new double[3 * mean_len];
		double* q = new double[4 * mean_len];
		double hmd_p[3];
		double hmd_q[4];
		double scale;
		const int ret = this->gro->getGestureMeanStroke(part.PartGestureID, (double(*)[3])p, (double(*)[4])q, mean_len, hmd_p, hmd_q, &scale);
		if (ret <= 0) {
			delete[] p;
			delete[] q;
			return;
		}
		FQuat quat;
		if (this->UnityGestureDatabaseFile) {
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
		Result = GestureRecognition_Result::Then;
		return;
	} else if (this->gco) {
		int mean_len = this->gco->getGestureMeanLength((int)part.Side, part.PartGestureID);
		if (mean_len <= 0)
			return;
		double* p = new double[3 * mean_len];
		double* q = new double[4 * mean_len];
		double hmd_p[3];
		double hmd_q[4];
		double scale;
		int ret = this->gco->getGestureMeanStroke((int)part.Side, part.PartGestureID, (double(*)[3])p, (double(*)[4])q, mean_len, hmd_p, hmd_q, &scale);
		if (ret < 0) {
			delete[] p;
			delete[] q;
			return;
		}
		FQuat quat;
		if (this->UnityGestureDatabaseFile) {
			GestureLocation.X = (float)hmd_p[2] * 100.0f; // Unreal.X = front = Unity.Z
			GestureLocation.Y = (float)hmd_p[0] * 100.0f; // Unreal.Y = right = Unity.X
			GestureLocation.Z = (float)hmd_p[1] * 100.0f; // Unreal.Z = up    = Unity.Y
			quat.X = (float)hmd_q[2];
			quat.Y = (float)hmd_q[0];
			quat.Z = (float)hmd_q[1];
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
		Result = GestureRecognition_Result::Then;
		return;
	} else {
		UE_LOG(LogTemp, Warning, TEXT("[MiVRyActor.getGesturePartStroke] GestureRecognition object was not created. Failed to load database file?"));
	}
}


void AMiVRyActor::LeftTriggerInputPressed()
{
	GestureRecognition_Result Result;
	int ErrorCode;
	this->startGesturing(Result, ErrorCode, GestureRecognition_Side::Left);
}

void AMiVRyActor::LeftTriggerInputReleased()
{
	GestureRecognition_Identification Result;
	this->stopGesturing(Result, GestureRecognition_Side::Left);
	if (Result == GestureRecognition_Identification::WaitingForOtherHand) {
		return;
	}
	if (Result == GestureRecognition_Identification::FailedToIdentify) {
		this->OnGestureIdentifiedDelegate.Broadcast(this, Result, -1, "", -1, TArray<FMiVRyGesturePart>());
		return;
	}
	// else: GestureIdentified
	GestureRecognition_Result Result2;
	int GestureID = -1;
	FString GestureName = "";
	float Similarity = -1;
	TArray<FMiVRyGesturePart> GestureParts;
	this->getIdentifiedGestureInfo(Result2, GestureID, GestureName, Similarity, GestureParts);
	this->OnGestureIdentifiedDelegate.Broadcast(
		this,
		(Result2 == GestureRecognition_Result::Error) ? GestureRecognition_Identification::FailedToIdentify : GestureRecognition_Identification::GestureIdentified,
		GestureID, GestureName, Similarity, GestureParts);
}

void AMiVRyActor::RightTriggerInputPressed()
{
	GestureRecognition_Result Result;
	int ErrorCode;
	this->startGesturing(Result, ErrorCode, GestureRecognition_Side::Right);
}

void AMiVRyActor::RightTriggerInputReleased()
{
	GestureRecognition_Identification Result;
	this->stopGesturing(Result, GestureRecognition_Side::Right);
	if (Result == GestureRecognition_Identification::WaitingForOtherHand) {
		return;
	}
	if (Result == GestureRecognition_Identification::FailedToIdentify) {
		this->OnGestureIdentifiedDelegate.Broadcast(this, Result, -1, "", -1, TArray<FMiVRyGesturePart>());
		return;
	}
	// else: GestureIdentified
	GestureRecognition_Result Result2;
	int GestureID = -1;
	FString GestureName = "";
	float Similarity = -1;
	TArray<FMiVRyGesturePart> GestureParts;
	this->getIdentifiedGestureInfo(Result2, GestureID, GestureName, Similarity, GestureParts);
	this->OnGestureIdentifiedDelegate.Broadcast(
		this,
		(Result2 == GestureRecognition_Result::Error) ? GestureRecognition_Identification::FailedToIdentify : GestureRecognition_Identification::GestureIdentified,
		GestureID, GestureName, Similarity, GestureParts);
}
