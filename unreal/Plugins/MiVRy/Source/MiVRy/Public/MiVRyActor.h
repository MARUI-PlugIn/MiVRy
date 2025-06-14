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

#pragma once

#include "MiVRyUtil.h"
#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "InputAction.h"
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

	void parse(const double pos[3], double scale, const double dir0[3], const double dir1[3], const double dir2[3], GestureRecognition_CoordinateSystem coordsys);
};

/**
* Index/ID of the (hand) side (0=left, 1=right).
*/
UENUM(BlueprintType)
enum class GestureRecognition_ContinuousIdentification : uint8
{
	Off = 0 UMETA(DisplayName = "Off"),
	WhilePressingTrigger = 1 UMETA(DisplayName = "While Pressing Trigger"),
	Always = 2 UMETA(DisplayName = "Always"),
	// TriggerToggles = 3 UMETA(DisplayName = "Trigger Toggles On/Off"),
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
	* Which VR plug-in you're using in your project.
	* By default, UE5 uses OpenXR plug-in as other plug-ins have been deprecated.
	* However, in UE4, several plug-ins were available including "OculusVR" which used a different coordinate
	* system for the controller.
	*/
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "MiVRy", meta = (DisplayName = "Unreal VR Plugin"))
		GestureRecognition_VRPlugin UnrealVRPlugin = GestureRecognition_VRPlugin::OpenXR;

	/**
	* Which coordinate system is used inside the MiVRy AI.
	* If you recorded your gestures in another coordinate system (for example: in Unity with the Gesture Manager VR App),
	* select the correct coorinate system here to automatically convery between your project and the gesture recordings in the
	* MiVRy gesture database file.
	* Regarding the Unreal VR world scale, see: World Settings -> World To Meters.
	*/
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "MiVRy", meta = (DisplayName = "MiVRy Coordinate System"))
		GestureRecognition_CoordinateSystem MivryCoordinateSystem = GestureRecognition_CoordinateSystem::Unreal_OpenXR;

	/**
	* Actor component (eg. MotionControllerComponent) to use as left hand. (Optional).
	* Use either this or LeftHandActor to specify the source
	* for the location/rotation of the left hand.
	* If neither is set, XRSystem->GetMotionControllerData is used.
	*/
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "MiVRy")
		TSoftObjectPtr<USceneComponent> LeftMotionController = nullptr;

	/**
	* Actor to use as left hand. (Optional).
	* Use either this or LeftMotionController to specify the source
	* for the location/rotation of the left hand.
	* If neither is set, XRSystem->GetMotionControllerData is used.
	*/
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "MiVRy")
		TSoftObjectPtr<AActor> LeftHandActor = nullptr;

	/**
	* Actor component (eg. MotionControllerComponent) to use as right hand. (Optional).
	* Use either this or RightHandActor to specify the source
	* for the location/rotation of the right hand.
	* If neither is set, XRSystem->GetMotionControllerData is used.
	*/
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "MiVRy")
		TSoftObjectPtr<USceneComponent> RightMotionController = nullptr;

	/**
	* Actor to use as right hand. (Optional).
	* Use either this or RightMotionController to specify the source
	* for the location/rotation of the right hand.
	* If neither is set, XRSystem->GetMotionControllerData is used.
	*/
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "MiVRy")
		TSoftObjectPtr<AActor> RightHandActor = nullptr;

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
	* The Input Action which indicates whether the user wishes to
	* start/stop gesturing. When the value of the associated button (or axis)
	* is above the LeftTriggerInputThreshold gesturing starts,
	* when it falls below the threshold gesturing stops and MiVRy
	* will identify the performed gesture.
	*/
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "MiVRy")
		TSoftObjectPtr<UInputAction> LeftTriggerInputAction = nullptr;

	/**
	* The Input Action which indicates whether the user wishes to
	* start/stop gesturing. When the value of the associated button (or axis)
	* is above the RightTriggerInputThreshold gesturing starts,
	* when it falls below the threshold gesturing stops and MiVRy
	* will identify the performed gesture.
	*/
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "MiVRy")
		TSoftObjectPtr<UInputAction> RightTriggerInputAction = nullptr;

	/**
	* The threshold value above which the associated Input Action value must rise
	* to be considered "pressed" (or "triggered").
	*/
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "MiVRy")
		float LeftTriggerInputThreshold = 0.85f;

	/**
	* The threshold value above which the associated Input Action value must rise
	* to be considered "pressed" (or "triggered").
	*/
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "MiVRy")
		float RightTriggerInputThreshold = 0.85f;

	/**
	* Whether gestures should be identified continuously during gesturing,
	* instead of after a gesture motion is finished.
	* NOTE: Continous gesture recognition is running *while* the trigger is pressed.
	* So if you want to XXX
	*/
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "MiVRy")
		GestureRecognition_ContinuousIdentification ContinuousGestureRecognition = GestureRecognition_ContinuousIdentification::Off;

	/**
	* Time frame (in milliseconds) that continuous gestures are expected to be.
	*/
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "MiVRy")
		int ContinuousGesturePeriod = 1000;

	/**
	* The number of samples to use for smoothing continuous gesture identification results.
	*/
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "MiVRy")
		int ContinuousGestureSmoothing = 3;

	/**
	* Whether or not to compensate head motions during gesturing
	* by continuously updating the current head position/rotation.
	*/
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "MiVRy")
		bool CompensateHeadMotion = false;

	/**
	* License ID (name) of your MiVRy license.
	* Leave emtpy for free version.
	*/
	UPROPERTY(EditAnywhere, Category = "MiVRy")
		FString LicenseName;

	/**
	* License Key of your MiVRy license.
	* Leave emtpy for free version.
	*/
	UPROPERTY(EditAnywhere, Category = "MiVRy")
		FString LicenseKey;

	/**
	* Provide a license file to enable additional functionality.
	* Alternative to specifying LicenseName and LicenseKey directly above.
	* Leave emtpy for free version.
	*/
	UPROPERTY(EditAnywhere, Category = "MiVRy", meta = (RelativePath))
		FFilePath LicenseFilePath;

	/**
	* Start a gesture motion.
	* @param Result Whether the function succeded or failed.
	* @param ErrorCode Error code if the function failed, 0 if it succeeded.
	* @param side Which hand (left or right) is starting the gesturing.
	*/
	UFUNCTION(BlueprintCallable, Category = "MiVRy", Meta=(DisplayName="Start Gesturing", ExpandEnumAsExecs="Result"))
		void startGesturing(GestureRecognition_Result& Result, int& ErrorCode, GestureRecognition_Side side = GestureRecognition_Side::Left);

	UFUNCTION(BlueprintCallable, Category = "MiVRy", Meta = (DisplayName = "Is Gesturing"))
		bool IsGesturing(GestureRecognition_Side side = GestureRecognition_Side::Left);

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
		void getIdentifiedGestureInfo(GestureRecognition_Result& Result, int& GestureID, FString& GestureName, float& Similarity, TArray<FMiVRyGesturePart>& GestureParts) const;

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
	void SetupPlayerInputComponent(class UInputComponent* InputComponent); //!< Bind the input component actions.
	
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

	void EnhancedInputLeftTrigger(const FInputActionValue& Value);
	void EnhancedInputRightTrigger(const FInputActionValue& Value);
	float EnhancedInputTriggerValue(const FInputActionValue& Value);
};
