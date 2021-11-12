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

#pragma once

#include "MiVRyUtil.h"
#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "MotionControllerComponent.h"
#include "MiVRyActor.generated.h"

class IGestureRecognition;
class IGestureCombinations;

USTRUCT(BlueprintType)
struct FMiVRyGesturePart
{
    GENERATED_BODY()

	/**
	* Which side (hand) performed this gesture part.
	*/
	UPROPERTY(BlueprintReadWrite, Category = "MiVRy")
    GestureRecognition_Side Side = GestureRecognition_Side::Left;

	/**
	* The location where the gesture part was performed.
	*/
	UPROPERTY(BlueprintReadWrite, Category = "MiVRy")
	FVector Position = FVector::ZeroVector;

	/**
	* The orientation/direction in whicht the gesture part was performed.
	*/
	UPROPERTY(BlueprintReadWrite, Category = "MiVRy")
	FRotator Rotation = FRotator{0,0,0};

	/**
	* The size / scale at which the gesture part was performed.
	*/
	UPROPERTY(BlueprintReadWrite, Category = "MiVRy")
	float Scale = 0;

	/**
	* Primary axis along which the gesture was performed.
	* For example, a figure eight ('8') starting from the top
	* will have the primary direction "downwards".
	*/
	UPROPERTY(BlueprintReadWrite, Category = "MiVRy")
	FVector PrimaryDirection = FVector::ForwardVector;

	/**
	* Secondary axis along which the gesture was performed.
	* For example, a figure eight ('8')
	* will have the secondary direction "right".
	*/
	UPROPERTY(BlueprintReadWrite, Category = "MiVRy")
	FVector SecondaryDirection = FVector::RightVector;

	/**
	* The gesture ID (index) of this part.
	*/
	UPROPERTY(BlueprintReadWrite, Category = "MiVRy")
	int PartGestureID = -1;
};


/**
* High-level gesture recognition actor for quick and easy use of
* GestureDatabase files.
*/
UCLASS(ClassGroup = GestureRecognition, hideCategories = (Object, LOD, Physics, Collision, Cooking, Actor, Rendering))
class MIVRY_API AMiVRyActor : public AActor
{
	GENERATED_BODY()
	
public:
	/**
	* File from which to load pre-recorded gestures.
	*/
	UPROPERTY(EditAnywhere, Category = "MiVRy", meta = (RelativePath))
		FFilePath GestureDatabaseFile;

	/**
	* Whether the GestureDatabase file has been created with Unity,
	* for example with the Unity-based "GestureManager" app.
	* This internally switches the coordinate system (z-up -> y-up) and scales
	* the world (centimeters -> meters).
	* Regarding the Unreal VR world scale, see: World Settings -> World To Meters.
	*/
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "MiVRy")
		bool UnityGestureDatabaseFile = false;

	/**
	* Motion controller to use as left hand. (Optinal).
	* Use either this or LeftHandActor to specify the source
	* for the location/rotation of the left hand.
	* If neither is set, XRSystem->GetMotionControllerData is used.
	*/
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "MiVRy")
		UMotionControllerComponent* LeftMotionController = nullptr;

	/**
	* Actor to use as left hand. (Optinal).
	* Use either this or LeftMotionController to specify the source
	* for the location/rotation of the left hand.
	* If neither is set, XRSystem->GetMotionControllerData is used.
	*/
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "MiVRy")
		AActor* LeftHandActor = nullptr;

	/**
	* Motion controller to use as right hand. (Optinal).
	* Use either this or RightHandActor to specify the source
	* for the location/rotation of the right hand.
	* If neither is set, XRSystem->GetMotionControllerData is used.
	*/
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "MiVRy")
		UMotionControllerComponent* RightMotionController = nullptr;

	/**
	* Actor to use as right hand. (Optinal).
	* Use either this or RightMotionController to specify the source
	* for the location/rotation of the right hand.
	* If neither is set, XRSystem->GetMotionControllerData is used.
	*/
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "MiVRy")
		AActor* RightHandActor = nullptr;

	/**
	* Name of the input to use as trigger which starts/stops the
	* gesturing. (Optional).
	* See the Project Settings' Input page. If you don't set this
	* name, you have to call startGesturing/stopGesturing functions 
	* on this actor manually.
	*/
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "MiVRy")
		FName LeftTriggerInput;

	/**
	* Name of the input to use as trigger which starts/stops the
	* gesturing. (Optional).
	* See the Project Settings' Input page. If you don't set this
	* name, you have to call startGesturing/stopGesturing functions 
	* on this actor manually.
	*/
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "MiVRy")
		FName RightTriggerInput;

	/**
	* Start a gesture motion.
	* @param Result Whether the function succeded or failed.
	* @param ErrorCode Error code if the function failed, 0 if it succeeded.
	* @param side Which hand (left or right) is starting the gesturing.
	*/
	UFUNCTION(BlueprintCallable, Category = "MiVRy", Meta=(DisplayName="Start Gesturing", ExpandEnumAsExecs="Result"))
		void startGesturing(GestureRecognition_Result& Result, int& ErrorCode, GestureRecognition_Side side = GestureRecognition_Side::Left);

	/**
	* End a gesture motion.
	* The gesture will be identified if no other hand is currently gesturing.
	* @param Result The current status of gesture identification.
	* @param side Which hand (left or right) finished gesturing.
	*/
	UFUNCTION(BlueprintCallable, Category = "MiVRy", Meta = (DisplayName = "Stop Gesturing", ExpandEnumAsExecs = "Result"))
		void stopGesturing(GestureRecognition_Identification& Result, GestureRecognition_Side side = GestureRecognition_Side::Left);

	/**
	* Identify the gesture (or gesture combination) of the last completed gesture performance.
	* @param Result Whether there is information on the last identified gesture.
	* @param GestureID The ID of the identified gesture or gesture combination. Error code on failure.
	* @param GestureName The name of the identified gesture. (Empty string on failure).
	* @param Similarity How similar the gesture performance was to recorded samples (0~1).
	* @param GestureParts Information about the individual parts (strokes) of which the gesture consisted.
	*/
	UFUNCTION(BlueprintCallable, Category = "MiVRy", Meta = (DisplayName = "Get Identified Gesture Info", ExpandEnumAsExecs = "Result"))
		void getIdentifiedGestureInfo(GestureRecognition_Result& Result, int& GestureID, FString& GestureName, float& Similarity, TArray<FMiVRyGesturePart>& GestureParts);

	/**
	* Retrieve a 3d gesture path that is the representative average of a gesture part.
	* The coordinate system used is a standardized gesture coordinate system, where the x-axis
	* is primary direction of the gesture, and the y-axis is the secondary direction of the gesture.
	* The coordinate system is normalized so that the standard deviation of points' distances from the origin is one.
	* @param Result Whether sample could be retrieved or not.
	* @param Locations The locations of the gesture mean stroke data points, in the gesture's own coordinate system.
	* @param Rotations The rotations of the gesture mean stroke data points, in the gesture's own coordinate system.
	* @param GestureLocation The overall position relative to (ie: "as seen from") the headset.
	* @param GestureRotation The overall rotation relative to (ie: "as seen from") the headset.
	* @param GestureScale The overall scale of the average gesture.
	* @return The number of data points in the mean, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "MiVRy", meta = (DisplayName = "Get Gesture Part Average Path", ExpandEnumAsExecs = "Result"))
		void getGesturePartAveragePath(const FMiVRyGesturePart& part, GestureRecognition_Result& Result, TArray<FVector>& Locations, TArray<FRotator>& Rotations, FVector& GestureLocation, FRotator& GestureRotation, float& GestureScale);


	DECLARE_DYNAMIC_MULTICAST_DELEGATE_SixParams(FGestureIdentificationCallbackDelegate,
		AMiVRyActor*, Source,
		GestureRecognition_Identification, Result,
		int, GestureID,
		FString, GestureName,
		float, Similarity,
		const TArray<FMiVRyGesturePart>&, GestureParts
	);

	/**
	* Delegate to be called during training when a gesture was identified.
	*/
	UPROPERTY(BlueprintAssignable, Category = "Gesture Identification Events")
		FGestureIdentificationCallbackDelegate OnGestureIdentifiedDelegate;

	AMiVRyActor(); //!< Constructor.
	virtual ~AMiVRyActor(); //!< Destructor.
	virtual void Tick(float DeltaTime) override; //!< Called every frame.
protected:
	virtual void BeginPlay() override; //!< Called when the game starts or when spawned.
	
	IGestureRecognition* gro = nullptr; //!< The GestureRecognition object in use (if any).
	IGestureCombinations* gco = nullptr; //!< The GestureCombinations object in use (if any).
	bool side_active[2] = { false, false }; //!< Which hand (side) is currently gesturing (if any).
	TArray<FMiVRyGesturePart> parts; //!< Temporary storage for gesture parts.
	int gesture_id = -1; //!< Last identified gesture ID, or error code.
	double similarity = -1.0; //!< Last identified gesture's similarity.

	void LeftTriggerInputPressed();
	void LeftTriggerInputReleased();
	void RightTriggerInputPressed();
	void RightTriggerInputReleased();
};
