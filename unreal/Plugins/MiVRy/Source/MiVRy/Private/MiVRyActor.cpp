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

#include "MiVRyActor.h"
#include "Interfaces/IPluginManager.h"
#include "Misc/MessageDialog.h"
#include "Misc/Paths.h"
#include "Engine.h"
#include "Engine/GameInstance.h"
#include "Misc/FileHelper.h"
#include "Camera/PlayerCameraManager.h"
#include "IXRTrackingSystem.h"
#include "EnhancedInputComponent.h"
#include "GestureRecognition.h"
#include "GestureCombinations.h"

void FMiVRyGesturePart::parse(const double pos[3], double scale, const double dir0[3], const double dir1[3], const double dir2[3], GestureRecognition_CoordinateSystem coordsys)
{
	FRotationMatrix rm(FRotator(0, 0, 0));
	switch (coordsys) {
	case GestureRecognition_CoordinateSystem::Unity_OculusVR:
	case GestureRecognition_CoordinateSystem::Unity_OpenXR:
	case GestureRecognition_CoordinateSystem::Unity_SteamVR:
		this->Position.X = float(pos[2]) * 100.0f; // Unreal.X = front = Unity.Z
		this->Position.Y = float(pos[0]) * 100.0f; // Unreal.Y = right = Unity.X
		this->Position.Z = float(pos[1]) * 100.0f; // Unreal.Z = up    = Unity.Y
		this->Scale = float(scale) * 100.0f;
		rm.M[0][0] = (float)dir0[2]; // Unreal.X = front = Unity.Z
		rm.M[0][1] = (float)dir0[0]; // Unreal.Y = right = Unity.X
		rm.M[0][2] = (float)dir0[1]; // Unreal.Z = up    = Unity.Y
		rm.M[1][0] = (float)dir1[2];
		rm.M[1][1] = (float)dir1[0];
		rm.M[1][2] = (float)dir1[1];
		rm.M[2][0] = (float)dir2[2];
		rm.M[2][1] = (float)dir2[0];
		rm.M[2][2] = (float)dir2[1];
		this->Rotation = rm.Rotator();
		this->PrimaryDirection.X = float(dir0[2]); // Unreal.X = front = Unity.Z
		this->PrimaryDirection.Y = float(dir0[0]); // Unreal.Y = right = Unity.X
		this->PrimaryDirection.Z = float(dir0[1]); // Unreal.Z = up    = Unity.Y
		this->SecondaryDirection.X = float(dir1[2]);
		this->SecondaryDirection.Y = float(dir1[0]);
		this->SecondaryDirection.Z = float(dir1[1]);
		break;
	case GestureRecognition_CoordinateSystem::Unreal_OculusVR:
	case GestureRecognition_CoordinateSystem::Unreal_OpenXR:
	case GestureRecognition_CoordinateSystem::Unreal_SteamVR:
	default:
		this->Position.X = float(pos[0]);
		this->Position.Y = float(pos[1]);
		this->Position.Z = float(pos[2]);
		this->Scale = float(scale);
		rm.M[0][0] = (float)dir0[0];
		rm.M[0][1] = (float)dir0[1];
		rm.M[0][2] = (float)dir0[2];
		rm.M[1][0] = (float)dir1[0];
		rm.M[1][1] = (float)dir1[1];
		rm.M[1][2] = (float)dir1[2];
		rm.M[2][0] = (float)dir2[0];
		rm.M[2][1] = (float)dir2[1];
		rm.M[2][2] = (float)dir2[2];
		this->Rotation = rm.Rotator();
		this->PrimaryDirection.X = float(dir0[0]);
		this->PrimaryDirection.Y = float(dir0[1]);
		this->PrimaryDirection.Z = float(dir0[2]);
		this->SecondaryDirection.X = float(dir1[0]);
		this->SecondaryDirection.Y = float(dir1[1]);
		this->SecondaryDirection.Z = float(dir1[2]);
	}
}

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

	this->SetupPlayerInputComponent(this->InputComponent);

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
		if (this->LicenseName.IsEmpty() == false) {
			auto license_name = StringCast<ANSICHAR>(*this->LicenseName);
			auto license_key = StringCast<ANSICHAR>(*this->LicenseKey);
			ret = this->gro->activateLicense(license_name.Get(), license_key.Get());
			if (ret == 0) {
				UE_LOG(LogTemp, Display, TEXT("[MiVRyActor] Successfully activated license."));
			} else {
				const FString errorString = UMiVRyUtil::errorCodeToString(ret);
				UE_LOG(LogTemp, Error, TEXT("[MiVRyActor] Failed to activate license: %s"), *errorString);
			}
		} else if (this->LicenseFilePath.FilePath.IsEmpty() == false) {
			auto license_file_path = StringCast<ANSICHAR>(*this->LicenseFilePath.FilePath);
			ret = this->gro->activateLicenseFile(license_file_path.Get());
			if (ret == 0) {
				UE_LOG(LogTemp, Display, TEXT("[MiVRyActor] Successfully activated license."));
			} else {
				const FString errorString = UMiVRyUtil::errorCodeToString(ret);
				UE_LOG(LogTemp, Error, TEXT("[MiVRyActor] Failed to activate license: %s"), *errorString);
			}
		}
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
		if (this->LicenseName.IsEmpty() == false) {
			auto license_name = StringCast<ANSICHAR>(*this->LicenseName);
			auto license_key = StringCast<ANSICHAR>(*this->LicenseKey);
			ret = this->gco->activateLicense(license_name.Get(), license_key.Get());
			if (ret == 0) {
				UE_LOG(LogTemp, Display, TEXT("[MiVRyActor] Successfully activated license."));
			} else {
				const FString errorString = UMiVRyUtil::errorCodeToString(ret);
				UE_LOG(LogTemp, Error, TEXT("[MiVRyActor] Failed to activate license: %s"), *errorString);
			}
		} else if (this->LicenseFilePath.FilePath.IsEmpty() == false) {
			auto license_file_path = StringCast<ANSICHAR>(*this->LicenseFilePath.FilePath);
			ret = this->gro->activateLicenseFile(license_file_path.Get());
			if (ret == 0) {
				UE_LOG(LogTemp, Display, TEXT("[MiVRyActor] Successfully activated license."));
			} else {
				const FString errorString = UMiVRyUtil::errorCodeToString(ret);
				UE_LOG(LogTemp, Error, TEXT("[MiVRyActor] Failed to activate license: %s"), *errorString);
			}
		}
		return; // successfully loaded
	} // else: failed to load
	delete this->gco;
	this->gco = nullptr;

	FString error_str = UMiVRyUtil::errorCodeToString(ret);
	UE_LOG(LogTemp, Error, TEXT("[MiVRyActor] Failed to load gesture database file %s: %s"), *this->GestureDatabaseFile.FilePath, *error_str);
}

void AMiVRyActor::SetupPlayerInputComponent(class UInputComponent* inputComponent)
{
	if (inputComponent == nullptr) {
		if (!this->LeftTriggerInput.IsNone() || !this->RightTriggerInput.IsNone()) {
			UE_LOG(LogTemp, Warning, TEXT("[MiVRyActor] Trigger Inputs are set but the actor has no InputComponend. Auto Receive Input setting missing?"));
		}
		if (this->LeftTriggerInputAction.IsValid() || this->RightTriggerInputAction.IsValid()) {
			UE_LOG(LogTemp, Warning, TEXT("[MiVRyActor] Trigger Inputs are set but the actor has no InputComponend. Auto Receive Input setting missing?"));
		}
		return;
	}
	if (!this->LeftTriggerInput.IsNone() && this->LeftTriggerInput != "") {
		auto& BindingPressed = inputComponent->BindAction(this->LeftTriggerInput, IE_Pressed, this, &AMiVRyActor::LeftTriggerInputPressed);
		if (BindingPressed.IsValid()) {
			BindingPressed.bConsumeInput = 0;
		} else {
			UE_LOG(LogTemp, Warning, TEXT("[MiVRyActor] Failed to bind input %s"), *LeftTriggerInput.ToString());
		}
		auto& BindingReleased = inputComponent->BindAction(this->LeftTriggerInput, IE_Released, this, &AMiVRyActor::LeftTriggerInputReleased);
		if (BindingReleased.IsValid()) {
			BindingReleased.bConsumeInput = 0;
		} else {
			UE_LOG(LogTemp, Warning, TEXT("[MiVRyActor] Failed to bind input %s"), *LeftTriggerInput.ToString());
		}
	}
	if (!this->RightTriggerInput.IsNone() && this->RightTriggerInput != "") {
		auto& BindingPressed = inputComponent->BindAction(this->RightTriggerInput, IE_Pressed, this, &AMiVRyActor::RightTriggerInputPressed);
		if (BindingPressed.IsValid()) {
			BindingPressed.bConsumeInput = 0;
		} else {
			UE_LOG(LogTemp, Warning, TEXT("[MiVRyActor] Failed to bind input %s"), *RightTriggerInput.ToString());
		}
		auto& BindingReleased = inputComponent->BindAction(this->RightTriggerInput, IE_Released, this, &AMiVRyActor::RightTriggerInputReleased);
		if (BindingReleased.IsValid()) {
			BindingReleased.bConsumeInput = 0;
		} else {
			UE_LOG(LogTemp, Warning, TEXT("[MiVRyActor] Failed to bind input %s"), *RightTriggerInput.ToString());
		}
	}
	UEnhancedInputComponent* enhancedInputComponent = Cast<UEnhancedInputComponent>(inputComponent);
	if (enhancedInputComponent == nullptr) {
		return;
	}
	if (this->LeftTriggerInputAction.IsValid()) {
		enhancedInputComponent->BindAction(this->LeftTriggerInputAction.Get(), ETriggerEvent::Triggered, this, &AMiVRyActor::EnhancedInputLeftTrigger);
	}
	if (this->RightTriggerInputAction.IsValid()) {
		enhancedInputComponent->BindAction(this->RightTriggerInputAction.Get(), ETriggerEvent::Triggered, this, &AMiVRyActor::EnhancedInputRightTrigger);
	}
}

void AMiVRyActor::Tick(float DeltaTime)
{
	Super::Tick(DeltaTime);

	USceneComponent* motion_controllers[2];
	motion_controllers[(uint8)GestureRecognition_Side::Left ] = this->LeftMotionController.Get();
	motion_controllers[(uint8)GestureRecognition_Side::Right] = this->RightMotionController.Get();
	AActor* hand_actors[2];
	hand_actors[(uint8)GestureRecognition_Side::Left ] = this->LeftHandActor.Get();
	hand_actors[(uint8)GestureRecognition_Side::Right] = this->RightHandActor.Get();
	EControllerHand controller_hand[2];
	controller_hand[(uint8)GestureRecognition_Side::Left ] = EControllerHand::Left;
	controller_hand[(uint8)GestureRecognition_Side::Right] = EControllerHand::Right;

	for (int side = 1; side >= 0; side--) {
		if (!side_active[side] && this->ContinuousGestureRecognition != GestureRecognition_ContinuousIdentification::Always) {
			continue;
		}
		FVector location;
		FRotator rotation;
		USceneComponent* motion_controller = motion_controllers[side];
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
		UMiVRyUtil::convertInput(location, quaternion, GestureRecognition_DeviceType::Controller, this->UnrealVRPlugin, this->MivryCoordinateSystem, p, q);
		double hmd_p[3];
		double hmd_q[4];
		if (this->CompensateHeadMotion) {
			APlayerCameraManager* camManager = GetWorld()->GetFirstPlayerController()->PlayerCameraManager;
			if (camManager == nullptr) {
				UE_LOG(LogTemp, Warning, TEXT("[MiVRyActor.Tick] Could not get camera position."));
				return;
			}
			FVector hmd_location = camManager->GetCameraLocation();
			FRotator hmd_rotation = camManager->GetCameraRotation();
			FQuat hmd_quaternion = hmd_rotation.Quaternion();
			UMiVRyUtil::convertInput(hmd_location, hmd_quaternion, GestureRecognition_DeviceType::Headset, this->UnrealVRPlugin, this->MivryCoordinateSystem, hmd_p, hmd_q);
		}
		int ret;
		if (this->gro) {
			if (this->CompensateHeadMotion) {
				ret = this->gro->updateHeadPositionQ(hmd_p, hmd_q);
				if (ret != 0) {
					UE_LOG(LogTemp, Warning, TEXT("[MiVRyActor] GestureRecognition::updateHeadPositionQ() failed with %i"), ret);
				}
			}
			ret = this->gro->contdStrokeQ(p, q);
			if (ret != 0) {
				UE_LOG(LogTemp, Warning, TEXT("[MiVRyActor] GestureRecognition::contdStroke() failed with %i"), ret);
			}
			if (this->ContinuousGestureRecognition != GestureRecognition_ContinuousIdentification::Off) {
				// this->gesture_id = this->gro->contdIdentify(hmd_p, hmd_q, &this->similarity);
				double pos[3];
				double scale;
				double dir0[3];
				double dir1[3];
				double dir2[3];
				this->gesture_id = this->gro->contdIdentifyAndGetStroke(hmd_p, hmd_q, &this->similarity, pos, &scale, dir0, dir1, dir2);
				if (this->gesture_id < 0) {
					UE_LOG(LogTemp, Warning, TEXT("[MiVRyActor] Continuous identification failed with %i"), this->gesture_id);
				} else {
					this->parts.SetNum(1);
					FMiVRyGesturePart& part = this->parts[0];
					part.Side = (GestureRecognition_Side)side;
					part.parse(pos, scale, dir0, dir1, dir2, this->MivryCoordinateSystem);
					this->OnGestureIdentifiedDelegate.Broadcast(
						this,
						GestureRecognition_Identification::GestureIdentified,
						this->gesture_id,
						this->gro->getGestureName(this->gesture_id),
						this->similarity,
						this->parts
					);
				}
			}
		} else if (this->gco) {
			if (this->CompensateHeadMotion) {
				ret = this->gco->updateHeadPositionQ(hmd_p, hmd_q);
				if (ret != 0) {
					UE_LOG(LogTemp, Warning, TEXT("[MiVRyActor] GestureCombinations::updateHeadPositionQ() failed with %i"), ret);
				}
			}
			this->gco->setContdIdentificationPeriod((int)side, this->ContinuousGesturePeriod);
			this->gco->setContdIdentificationSmoothing((int)side, this->ContinuousGestureSmoothing);
			ret = this->gco->contdStrokeQ(side, p, q);
			if (ret != 0) {
				UE_LOG(LogTemp, Warning, TEXT("[MiVRyActor] GestureCombinations::contdStroke() failed with %i"), ret);
			}
			if (this->ContinuousGestureRecognition != GestureRecognition_ContinuousIdentification::Off) {
				this->gesture_id = this->gco->contdIdentify(hmd_p, hmd_q, &this->similarity);
				if (this->gesture_id < 0) {
					UE_LOG(LogTemp, Warning, TEXT("[MiVRyActor] Continuous identification failed with %i"), this->gesture_id);
				} else {
					const int numParts = this->gco->numberOfParts();
					this->parts.SetNum(numParts);
					for (int i = numParts - 1; i >= 0; i--) {
						FMiVRyGesturePart& part = this->parts[i];
						part.Side = (GestureRecognition_Side)i;
						double pos[3];
						double scale;
						double dir0[3];
						double dir1[3];
						double dir2[3];
						this->gco->contdIdentifyGetLastStrokeInfo(i, pos, &scale, dir0, dir1, dir2);
						part.parse(pos, scale, dir0, dir1, dir2, this->MivryCoordinateSystem);
					}
					this->OnGestureIdentifiedDelegate.Broadcast(
						this,
						GestureRecognition_Identification::GestureIdentified,
						this->gesture_id,
						this->gco->getGestureCombinationName(this->gesture_id),
						this->similarity,
						this->parts
					);
				}
			}
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
	UMiVRyUtil::convertInput(location, quaternion, GestureRecognition_DeviceType::Headset, this->UnrealVRPlugin, this->MivryCoordinateSystem, p, q);
	if (this->gro) {
		this->gro->contdIdentificationPeriod = this->ContinuousGesturePeriod;
		this->gro->contdIdentificationSmoothing = this->ContinuousGestureSmoothing;
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
	ErrorCode = -99;
	Result = GestureRecognition_Result::Error;
}

bool AMiVRyActor::IsGesturing(GestureRecognition_Side side)
{
	return this->side_active[(uint8)side];
}

void AMiVRyActor::stopGesturing(GestureRecognition_Identification& Result, GestureRecognition_Side side)
{
	this->side_active[(uint8)side] = false;
	if (this->gro) {
		double pos[3];
		double scale;
		double dir0[3];
		double dir1[3];
		double dir2[3];
		this->gesture_id = this->gro->endStrokeAndGetSimilarity(&this->similarity, pos, &scale, dir0, dir1, dir2);
		if (this->gesture_id < 0) {
			UE_LOG(LogTemp, Warning, TEXT("[MiVRyActor] Identification failed with %i"), this->gesture_id);
			Result = GestureRecognition_Identification::FailedToIdentify;
			this->parts.SetNum(1);
			FMiVRyGesturePart& part = this->parts[0];
			part.Side = side;
			part.PartGestureID = -1;
			this->OnGestureIdentifiedDelegate.Broadcast(this, Result, -1, "", -1, TArray<FMiVRyGesturePart>());
			return;
		}
		this->parts.SetNum(1);
		FMiVRyGesturePart& part = this->parts[0];
		part.Side = side;
		part.PartGestureID = this->gesture_id;
		part.parse(pos, scale, dir0, dir1, dir2, this->MivryCoordinateSystem);
		Result = GestureRecognition_Identification::GestureIdentified;

		GestureRecognition_Result Result2;
		int GestureID = -1;
		FString GestureName = "";
		float Similarity = -1;
		TArray<FMiVRyGesturePart> GestureParts;
		this->getIdentifiedGestureInfo(Result2, GestureID, GestureName, Similarity, GestureParts);
		this->OnGestureIdentifiedDelegate.Broadcast(
			this,
			GestureRecognition_Identification::GestureIdentified,
			GestureID, GestureName, Similarity, GestureParts);

		return;
	}
	if (this->gco) {
		double pos[3];
		double scale;
		double dir0[3];
		double dir1[3];
		double dir2[3];
		int ret = this->gco->endStroke((int)side, pos, &scale, dir0, dir1, dir2);
		if (ret < 0) {
			Result = GestureRecognition_Identification::FailedToIdentify;
			UE_LOG(LogTemp, Warning, TEXT("[MiVRyActor] GestureCombinations::endStroke() failed with %i"), ret);
			return;
		}
		FMiVRyGesturePart* part = nullptr;
		for (int i = this->parts.Num() - 1; i >= 0; i--) {
			if (this->parts[i].Side == side) {
				part = &this->parts[i];
				break;
			}
		}
		if (part == nullptr) {
			const int32 i = this->parts.Add(FMiVRyGesturePart());
			part = &this->parts[i];
			part->Side = side;
		}
		part->parse(pos, scale, dir0, dir1, dir2, this->MivryCoordinateSystem);
		if (this->side_active[1 - (uint8)side]) {
			Result = GestureRecognition_Identification::WaitingForOtherHand;
			return;
		}
		this->gesture_id = this->gco->identifyGestureCombination(nullptr, &this->similarity);
		if (this->gesture_id < 0) {
			Result = GestureRecognition_Identification::FailedToIdentify;
			UE_LOG(LogTemp, Warning, TEXT("[MiVRyActor] GestureCombinations::identifyGestureCombination() failed with %i"), this->gesture_id);
			for (int i = 0; i < parts.Num(); i++) {
				parts[i].PartGestureID = -1;
			}
			this->OnGestureIdentifiedDelegate.Broadcast(this, Result, -1, "", -1, TArray<FMiVRyGesturePart>());
			return;
		}
		for (int i = 0; i < parts.Num(); i++) {
			parts[i].PartGestureID = this->gco->getCombinationPartGesture(this->gesture_id, (int)parts[i].Side);
		}
		Result = GestureRecognition_Identification::GestureIdentified;
		GestureRecognition_Result Result2;
		int GestureID = -1;
		FString GestureName = "";
		float Similarity = -1;
		TArray<FMiVRyGesturePart> GestureParts;
		this->getIdentifiedGestureInfo(Result2, GestureID, GestureName, Similarity, GestureParts);
		this->OnGestureIdentifiedDelegate.Broadcast(
			this,
			GestureRecognition_Identification::GestureIdentified,
			GestureID, GestureName, Similarity, GestureParts
		);
		return;
	}
	UE_LOG(LogTemp, Warning, TEXT("[MiVRyActor.stopGesturing] GestureRecognition object was not created. Failed to load database file?"));
	Result = GestureRecognition_Identification::FailedToIdentify;
}

void AMiVRyActor::getIdentifiedGestureInfo(GestureRecognition_Result& Result, int& GestureID, FString& GestureName, float& Similarity, TArray<FMiVRyGesturePart>& GestureParts) const
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
		UMiVRyUtil::convertOutput(this->UnrealVRPlugin, this->MivryCoordinateSystem, hmd_p, hmd_q, GestureRecognition_DeviceType::Headset, GestureLocation, quat);
		GestureRotation = quat.Rotator();
		GestureScale = (float)scale;
		switch (this->MivryCoordinateSystem) {
		case GestureRecognition_CoordinateSystem::Unity_OculusVR:
		case GestureRecognition_CoordinateSystem::Unity_OpenXR:
		case GestureRecognition_CoordinateSystem::Unity_SteamVR:
			GestureScale *= 100.0f;
		}
		const int len = (ret < mean_len) ? ret : mean_len;
		Locations.SetNum(len);
		Rotations.SetNum(len);
		for (int i = 0; i < len; i++) {
			Locations[i].X = (float)p[3 * i + 0]; // x = primart axis
			Locations[i].Y = (float)p[3 * i + 1]; // y = secondary axis
			Locations[i].Z = (float)p[3 * i + 2]; // z = least-significant axis
			quat.X = (float)q[4 * i + 0];
			quat.Y = (float)q[4 * i + 1];
			quat.Z = (float)q[4 * i + 2];
			quat.W = (float)q[4 * i + 3];
			quat.Normalize();
			switch (this->MivryCoordinateSystem) {
			case GestureRecognition_CoordinateSystem::Unity_OculusVR:
			case GestureRecognition_CoordinateSystem::Unity_OpenXR:
			case GestureRecognition_CoordinateSystem::Unity_SteamVR:
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
		UMiVRyUtil::convertOutput(this->UnrealVRPlugin, this->MivryCoordinateSystem, hmd_p, hmd_q, GestureRecognition_DeviceType::Headset, GestureLocation, quat);
		GestureRotation = quat.Rotator();
		GestureScale = (float)scale;
		switch (this->MivryCoordinateSystem) {
		case GestureRecognition_CoordinateSystem::Unity_OculusVR:
		case GestureRecognition_CoordinateSystem::Unity_OpenXR:
		case GestureRecognition_CoordinateSystem::Unity_SteamVR:
			GestureScale *= 100.0f;
		}
		const int len = (ret < mean_len) ? ret : mean_len;
		Locations.SetNum(len);
		Rotations.SetNum(len);
		for (int i = 0; i < len; i++) {
			Locations[i].X = (float)p[3 * i + 0]; // x = primart axis
			Locations[i].Y = (float)p[3 * i + 1]; // y = secondary axis
			Locations[i].Z = (float)p[3 * i + 2]; // z = least-significant axis
			quat.X = (float)q[4 * i + 0];
			quat.Y = (float)q[4 * i + 1];
			quat.Z = (float)q[4 * i + 2];
			quat.W = (float)q[4 * i + 3];
			quat.Normalize();
			switch (this->MivryCoordinateSystem) {
			case GestureRecognition_CoordinateSystem::Unity_OculusVR:
			case GestureRecognition_CoordinateSystem::Unity_OpenXR:
			case GestureRecognition_CoordinateSystem::Unity_SteamVR:
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
}

void AMiVRyActor::EnhancedInputLeftTrigger(const FInputActionValue& Value)
{
	float v = EnhancedInputTriggerValue(Value);
	UE_LOG(LogTemp, Warning, TEXT("EnhancedInputLeftTrigger %f"), v);
	if (this->side_active[(uint8)GestureRecognition_Side::Left]) {
		// currently gesturing
		if (v < this->LeftTriggerInputThreshold) {
			this->LeftTriggerInputReleased();
		}
	} else {
		// currently not gesturing
		if (v >= this->LeftTriggerInputThreshold) {
			this->LeftTriggerInputPressed();
		}
	}
}

void AMiVRyActor::EnhancedInputRightTrigger(const FInputActionValue& Value)
{
	float v = EnhancedInputTriggerValue(Value);
	UE_LOG(LogTemp, Warning, TEXT("EnhancedInputRightTrigger %f"), v);
	if (this->side_active[(uint8)GestureRecognition_Side::Right]) {
		// currently gesturing
		if (v < this->RightTriggerInputThreshold) {
			this->RightTriggerInputReleased();
		}
	} else {
		// currently not gesturing
		if (v >= this->RightTriggerInputThreshold) {
			this->RightTriggerInputPressed();
		}
	}
}

float AMiVRyActor::EnhancedInputTriggerValue(const FInputActionValue& Value)
{
	const EInputActionValueType valueType = Value.GetValueType();
	switch (valueType) {
		case EInputActionValueType::Boolean:
			return Value.Get<bool>() ? 1.0f : 2.0f;
			break;
		case EInputActionValueType::Axis1D:
			return Value.Get<FInputActionValue::Axis1D>(); // FInputActionValue::Axis1D == float
			break;
		case EInputActionValueType::Axis2D:
			return Value.Get<FInputActionValue::Axis2D>().Size(); // FInputActionValue::Axis2D == FVector2D
			break;
		case EInputActionValueType::Axis3D:
			return Value.Get<FInputActionValue::Axis3D>().Size(); // FInputActionValue::Axis3D == FVector
			break;
		default:
			UE_LOG(LogTemp, Warning, TEXT("[MiVRyActor.EnhancedInputLeftTrigger] Unkown value type."));
			return 0.0;
	}
}
