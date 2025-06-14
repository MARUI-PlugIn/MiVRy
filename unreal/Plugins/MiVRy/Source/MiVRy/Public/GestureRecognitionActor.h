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
#include "GestureRecognitionActor.generated.h"

class IGestureRecognition;

UCLASS(ClassGroup = GestureRecognition, hideCategories = (Object, LOD, Physics, Collision, Cooking, Actor, Rendering, Input))
class MIVRY_API AGestureRecognitionActor : public AActor
{
	GENERATED_BODY()
private:
	IGestureRecognition* gro = nullptr; //!< Gesture Recognition object.

public:	
	// Sets default values for this actor's properties
	AGestureRecognitionActor();
	virtual ~AGestureRecognitionActor();

protected:
	// Called when the game starts or when spawned
	virtual void BeginPlay() override;

public:
	// Called every frame
	virtual void Tick(float DeltaTime) override;

	/**
    * Which VR plug-in you're using in your project.
    * By default, UE5 uses OpenXR plug-in as other plug-ins have been deprecated.
    * However, in UE4, several plug-ins were available including "OculusVR" which used a different coordinate
    * system for the controller.
	*/
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Gesture Recognition", meta = (DisplayName = "Unreal VR Plugin"))
	GestureRecognition_VRPlugin UnrealVRPlugin = GestureRecognition_VRPlugin::OpenXR;
	
	/**
	* Which coordinate system is used inside the MiVRy AI.
	* If you recorded your gestures in another coordinate system (for example: in Unity with the Gesture Manager VR App),
	* select the correct coorinate system here to automatically convery between your project and the gesture recordings in the
	* MiVRy gesture database file.
	* Regarding the Unreal VR world scale, see: World Settings -> World To Meters.
	*/
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Gesture Recognition", meta = (DisplayName = "MiVRy Coordinate System"))
	GestureRecognition_CoordinateSystem MivryCoordinateSystem = GestureRecognition_CoordinateSystem::Unreal_OpenXR;

	/**
	* License ID (name) of your MiVRy license.
	* Leave emtpy for free version.
	*/
	UPROPERTY(EditAnywhere, Category = "Gesture Recognition")
	FString LicenseName;

	/**
	* License Key of your MiVRy license.
	* Leave emtpy for free version.
	*/
	UPROPERTY(EditAnywhere, Category = "Gesture Recognition")
	FString LicenseKey;

	/**
	* Provide a license to enable additional functionality.
	* @param license_name   The license name text.
	* @param license_key    The license key text.
	* @return   Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Activate License"))
	int activateLicense(const FString& license_name, const FString& license_key);

	/**
	* Provide a license file to enable additional functionality.
	* @param license_file_path	The file path to the license file.
	* @return   Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Activate License File"))
	int activateLicenseFile(const FString& license_file_path);

	/**
	* Check if a license was activated to enable additional functionality.
	* @return   One if a license was activated, zero if not.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Get License Status"))
	int getLicenseStatus() const;

	/**
	* Start new gesture motion.
	* @param HMD_Position Current position of the headset.
	* @param HMD_Rotation Current orientation/rotation of the headset.
	* @param RecordAsSample Gesture ID if you want to record a sample for a gesture, -1 to identify a gesture.
	* @return Zero on success, an error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Start Stroke"))
	int startStroke(const FVector& HMD_Position, const FRotator& HMD_Rotation, int RecordAsSample=-1);
	
	/**
	* Start new gesture motion, using a Quaternion as rotation data.
	* @param HMD_Position Current position of the headset.
	* @param HMD_Rotation Current orientation/rotation of the headset.
	* @param RecordAsSample Gesture ID if you want to record a sample for a gesture, -1 to identify a gesture.
	* @return Zero on success, an error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Start Stroke (Quaternion Rotation)"))
	int startStrokeQ(const FVector& HMD_Position, const FQuat& HMD_Rotation, int RecordAsSample=-1);

	/**
	* Update the current position of the HMD/headset during a gesture performance (stroke).
	* @param  HMD_Position		Current position of the headset.
	* @param  HMD_Rotation      Current orientation/rotation of the headset.
	* @return                   Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Update Head Position"))
	int updateHeadPosition(const FVector& HMD_Position, const FRotator& HMD_Rotation);

	/**
	* Update the current position of the HMD/headset during a gesture performance (stroke).
	* @param  HMD_Position		Current position of the headset.
	* @param  HMD_Rotation      Current orientation/rotation of the headset.
	* @return                   Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Update Head Position (Quaternion Rotation)"))
	int updateHeadPositionQ(const FVector& HMD_Position, const FQuat& HMD_Rotation);

	/**
	* Continue to perform a gesture (motion update).
	* @param HandPosition Current position of the hand.
	* @param HandRotation Current orientation/rotation of the hand.
	* @return Zero on success, an error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Continue Stroke"))
	int contdStroke(const FVector& HandPosition, const FRotator& HandRotation=FRotator(0,0,0));
	
	/**
	* Continue to perform a gesture (motion update), using a Quaternion as rotation data.
	* @param HandPosition Current position of the hand.
	* @param HandRotation Current orientation/rotation of the hand.
	* @return Zero on success, an error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Continue Stroke (Quaternion Rotation)"))
	int contdStrokeQ(const FVector& HandPosition, const FQuat& HandRotation);
	
	/**
	* End a gesture motion and identify gesture.
	* If the gesture was started as recording a new sample (sample-recording-mode), then
    * the gesture will be added to the reference examples for this gesture.
    * If the gesture (stroke) was started in identification mode, the gesture recognition
    * library will attempt to identify the gesture.
	* @param Similarity Similarity between the performed stroke and the identified gesture (0~1).
	* @param GesturePosition The position where the gesture was performed.
	* @param GestureRotation The direction/rotation in which the gesture was performed.
	* @param GestureScale The scale (size) at which the gesture was performed.
	* @return The ID of the gesture identified, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "End Stroke"))
	int endStroke(float& Similarity, FVector& GesturePosition, FRotator& GestureRotation, float& GestureScale);

	/**
	* End a gesture motion and identify gesture.
	* If the gesture was started as recording a new sample (sample-recording-mode), then
    * the gesture will be added to the reference examples for this gesture.
    * If the gesture (stroke) was started in identification mode, the gesture recognition
    * library will attempt to identify the gesture.
	* @param Similarity Similarity between the performed stroke and the identified gesture (0~1).
	* @param GesturePosition The position where the gesture was performed.
	* @param GestureRotation The direction/rotation in which the gesture was performed.
	* @param GestureScale The scale (size) at which the gesture was performed.
	* @return The ID of the gesture identified, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "End Stroke (Quaternion Rotation)"))
	int endStrokeQ(float& Similarity, FVector& GesturePosition, FQuat& GestureRotation, float& GestureScale);

	/**
	* End gesture motion and get probabilities for all registered gestures.
	* @param Probabilities Probability estimates for each registered gesture that the performed motion was this gesture.
	* @param GesturePosition The position where the gesture was performed.
	* @param GestureRotation The direction/rotation in which the gesture was performed.
	* @param GestureScale The scale (size) at which the gesture was performed.
	* @return Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "End Stroke and get all Probabilities"))
	int endStrokeAndGetAllProbabilities(TArray<float>& Probabilities, FVector& GesturePosition, FRotator& GestureRotation, float& GestureScale);

	/**
	* End gesture motion and get probabilities for all registered gestures.
	* @param Probabilities Probability estimates for each registered gesture that the performed motion was this gesture.
	* @param GesturePosition The position where the gesture was performed.
	* @param GestureRotation The direction/rotation in which the gesture was performed.
	* @param GestureScale The scale (size) at which the gesture was performed.
	* @return Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "End Stroke and get all Probabilities"))
	int endStrokeAndGetAllProbabilitiesQ(TArray<float>& Probabilities, FVector& GesturePosition, FQuat& GestureRotation, float& GestureScale);
	
	/**
	* End gesture motion and get probabilities for all registered gestures
	* as well as similarity values for all registered gestures.
	* @param Probabilities Probability estimates for each registered gesture that the performed motion was this gesture.
	* @param Similarities Similarity of the performed gesture motion with each registered gesture (0~1).
	* @param GesturePosition The position where the gesture was performed.
	* @param GestureRotation The direction/rotation in which the gesture was performed.
	* @param GestureScale The scale (size) at which the gesture was performed.
	* @return Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "End Stroke and get all Probabilities and Similarities"))
	int endStrokeAndGetAllProbabilitiesAndSimilarities(TArray<float>& Probabilities, TArray<float>& Similarities, FVector& GesturePosition, FRotator& GestureRotation, float& GestureScale);
	
	/**
	* End gesture motion and get probabilities for all registered gestures
	* as well as similarity values for all registered gestures.
	* @param Probabilities Probability estimates for each registered gesture that the performed motion was this gesture.
	* @param Similarities Similarity of the performed gesture motion with each registered gesture (0~1).
	* @param GesturePosition The position where the gesture was performed.
	* @param GestureRotation The direction/rotation in which the gesture was performed.
	* @param GestureScale The scale (size) at which the gesture was performed.
	* @return Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "End Stroke and get all Probabilities and Similarities"))
	int endStrokeAndGetAllProbabilitiesAndSimilaritiesQ(TArray<float>& Probabilities, TArray<float>& Similarities, FVector& GesturePosition, FQuat& GestureRotation, float& GestureScale);

	/**
	* Query whether a gesture performance (gesture motion, stroke) was started and is currently ongoing.
	* @return   True if a gesture motion (stroke) was started and is ongoing, false if not.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Is Stroke Started"))
	bool isStrokeStarted();

    /**
    * Prune currently performed gesture motion by discarding older tracking data points.
    * @param    num             Number of tracking data points to retain. -1 for no numeric limit.
    * @param    ms              Time frame (in milliseconds) of tracking data points to retain. -1 for no time limit.
    * @return   Number of retained tracking data points, a negative error code on failure.
    */
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Prune Stroke"))
    int pruneStroke(int num, int ms);
	
	/**
	* Cancel a started gesture motion.
	* @return Zero on success, an error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Cancel Stroke"))
	int cancelStroke();

	/**
	* Continuous gesture identification.
	* Call this regularily while performing a gesture to identify continuous gestures.
	* @param HMD_Position Current position of the headset.
	* @param HMD_Rotation Current orientation/rotation of the headset.
	* @param Similarity Similarity between the performed stroke and the identified gesture (0~1).
	* @return The Gesture ID/index of the identified gesture, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Continuous Identify Gesture"))
	int contdIdentify(const FVector& HMD_Position, const FRotator& HMD_Rotation, float& Similarity);

	/**
	* Continuous gesture identification.
	* Call this regularily while performing a gesture to identify continuous gestures.
	* @param HMD_Position Current position of the headset.
	* @param HMD_Rotation Current orientation/rotation of the headset.
	* @param Probabilities Probability estimates for each registered gesture that the performed motion was this gesture.
	* @param Similarities Similarity of the performed gesture motion with each registered gesture (0~1).
	* @return Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Continuous Identify Gesture and get all Probabilities and Similarities"))
	int contdIdentifyAndGetAllProbabilitiesAndSimilarities(const FVector& HMD_Position, const FRotator& HMD_Rotation, TArray<float>& Probabilities, TArray<float>& Similarities);

	/**
	* Continuous gesture recording.
	* Call this regularily while performing a gesture to record continuous gesture samples.
	* @param HMD_Position Current position of the headset.
	* @param HMD_Rotation Current orientation/rotation of the headset.
	* @return Zero on success, an error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Continuous Record Gesture"))
	int contdRecord(const FVector& HMD_Position, const FRotator& HMD_Rotation);

	/**
	* Get time frame in milliseconds for continuous gesture identification.
	* @return The time frame in milliseconds for continuous gesture identification, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Get Continuous Gesture Identification Period"))
	int GetContinuousGestureIdentificationPeriod();

	/**
	* Set time frame in milliseconds for continuous gesture identification.
	* @param PeriodInMs The time frame in milliseconds for continuous gesture identification.
	* @return Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Set Continuous Gesture Identification Period"))
	int SetContinuousGestureIdentificationPeriod(int PeriodInMs);

	/**
	* The number of samples to use for smoothing continuous gesture identification results.
	* @return The number of samples to use for smoothing continuous gesture identification, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Get Continuous Gesture Identification Smoothing"))
	int GetContinuousGestureIdentificationSmoothing();

	/**
	* The number of samples to use for smoothing continuous gesture identification results.
	* @param NumberOfSamples The number of samples to use for smoothing continuous gesture identification.
	* @return Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Get Continuous Gesture Identification Smoothing"))
	int SetContinuousGestureIdentificationSmoothing(int NumberOfSamples);

	/**
	* Query the number of currently registered gestures.
	* @return The number of gestures currently registered in the library, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Get Number of Gestures"))
	int numberOfGestures();

	/**
	* Delete currently registered gesture.
	* @param index ID of the gesture to delete.
	* @return Zero on success, an error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Delete Gesture"))
	int deleteGesture(int index);

	/**
	* Delete recorded gestures.
	* @return Zero on success, an error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Delete All Gestures"))
	int deleteAllGestures();

	/**
	* Create new gesture.
	* @param name Name of the new gesture.
	* @return The ID of the newly created gesture, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Create New Gesture"))
	int createGesture(const FString& name);

	/**
	* Get the gesture recognition score of the current neural network - 
	* i.e. how well the AI can distinguish the recorded gestures - 
	* on a scale from zero (unable to identify any gestures) to one
	* (correctly identifies all gestures).
	* @return The gesture recogniion score (0~1).
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Get Recognition Score"))
	float recognitionScore();

	/**
	* Get the name of a registered gesture.
	* @param index The ID of the gesture.
	* @return The name of the gesture.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Get Gesture Name"))
	FString getGestureName(int index);

	/**
	* Update the name of a gesture.
	* @param index The ID of the gesture.
	* @param name The new name for the gesture.
	* @return Zero on success, an error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Set Gesture Name"))
	int setGestureName(int index, const FString& name);

	/**
	* Set a Metadata object of a registered gesture.
	* @param    index           The gesture ID of the gesture whose metadata to get.
	* @param    metadata        The new metadata data.
	* @return                   Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Set Gesture Metadata"))
	int setGestureMetadata(int index, const TArray<uint8>& metadata);

	/**
	* Set a Metadata object of a registered gesture to a text string.
	* @param    index           The gesture ID of the gesture whose metadata to get.
	* @param    metadata        The new metadata string.
	* @return                   Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Set Gesture Metadata as String"))
	int setGestureMetadataAsString(int index, const FString& metadata);

	/**
	* Get the current metadata of a registered gesture.
	* @param    index           The gesture ID of the gesture whose metadata to get.
	* @param    metadata        The metadata registered for this IGestureRecognition object.
	* @return                   Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Get Gesture Metadata"))
	int getGestureMetadata(int index, TArray<uint8>& metadata);

	/**
	* Get the current metadata of a registered gesture, assuming it's a text string.
	* @param    index           The gesture ID of the gesture whose metadata to get.
	* @param    metadata        The metadata registered for this GestureRecognition object as a string.
	* @return                   Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Get Gesture Metadata as String"))
	int getGestureMetadataAsString(int index, FString& metadata);

	/**
	* Get whether a registered gesture is enabled or disabled.
	* A disabled gesture is not used in training and identification, but will retain its recorded samples.
	* @param    index           The gesture ID of the gesture whose status to get.
	* @return                   True if the gesture is enabled, false if disabled.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Get Gesture Enabled"))
	bool getGestureEnabled(int index);

	/**
	* Enable/disable a registered gesture.
	* A disabled gesture is not used in training and identification, but will retain its recorded samples.
	* @param    index           The gesture ID of the gesture whose status to set.
	* @param    enabled         Whether the gesture is supposed to be enabled (default) or disabled.
	* @return                   Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Set Gesture Enabled"))
	int setGestureEnabled(int index, bool enabled);

	/**
	* Get the number of recorded samples of a registered gesture.
	* @param index The Gesture ID / index for which to query the number of samples.
	* @return The number of samples, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Get Number Of Gesture Samples"))
	int getGestureNumberOfSamples(int index);

	/**
	* Get the type of a previously recorded sample stroke.
	* 0 = Standard.
	* 1 = Continuous.
	* @param   gesture_index   The zero-based index (ID) of the gesture from where to retrieve the number of data points.
	* @param   sample_index    The zero-based index (ID) of the sample for which to retrieve the number of data points.
	* @return  The type (ID) of the recorded sample stroke, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Get Gesture Sample Type"))
	int getGestureSampleType(int gesture_index, int sample_index) const;

	/**
	* Set the type of a previously recorded sample stroke.
	* 0 = Standard.
	* 1 = Continuous.
	* @param   gesture_index   The zero-based index (ID) of the gesture from where to retrieve the number of data points.
	* @param   sample_index    The zero-based index (ID) of the sample for which to retrieve the number of data points.
	* @param   type            The type ID to set for the sample stroke.
	* @return  The type (ID) of the recorded sample stroke, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Set Gesture Sample Type"))
	int setGestureSampleType(int gesture_index, int sample_index, int type);

	/**
	* Get the number of data points a sample has.
	* @param gesture_index The ID/index of the Gesture for which to query a sample.
	* @param sample_index The ID/index of the sample whose length to query.
	* @param processed Whether to get the raw length of the sample recording (false) or the length after pre-processing (true).
	* @return The number of data points in the sample recording, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Get Gesture Sample Length"))
	int getGestureSampleLength(int gesture_index, int sample_index, bool processed);

	/**
	* Retrieve a sample recording data set.
	* @param gesture_index The ID/index of the Gesture for which to query a sample.
	* @param sample_index The ID/index of the sample whose length to query.
	* @param processed Whether to get the raw length of the sample recording (false) or the length after pre-processing (true).
	* @param Locations The locations of the recorded data points.
	* @param Rotations The rotations of the recorded data points.
	* @param HMD_Locations The location of the VR headset when the gesture was recorded.
	* @param HMD_Rotations The rotation of the VR headset when the gesture was recorded.
	* @return The number of data points in the sample recording, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Get Gesture Sample Stroke"))
	int getGestureSampleStroke(int gesture_index, int sample_index, bool processed, TArray<FVector>& Locations, TArray<FRotator>& Rotations, TArray<FVector>& HMD_Locations, TArray<FRotator>& HMD_Rotations);

	/**
	* Get the number of samples of the gesture mean (average over samples).
	* @param gesture_index The ID/index of the Gesture for which to query.
	* @return The number of data points of the mean gesture stroke, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Get Gesture Mean Length"))
	int getGestureMeanLength(int gesture_index);

	/**
	* Retrieve a gesture mean (average over samples).
	* The coordinate system used is a standardized gesture coordinate system, where the x-axis
	* is primary direction of the gesture, and the y-axis is the secondary direction of the gesture.
	* The coordinate system is normalized so that the standard deviation of points' distances from the origin is one.
	* @param gesture_index The ID (index) of the gesture for which to retrieve the average stroke.
	* @param Locations The locations of the gesture mean stroke data points, in the gesture's own coordinate system.
	* @param Rotations The rotations of the gesture mean stroke data points, in the gesture's own coordinate system.
	* @param GestureLocation The overall position relative to (ie: "as seen from") the headset.
	* @param GestureRotation The overall rotation relative to (ie: "as seen from") the headset.
	* @param GestureScale The overall scale of the average gesture.
	* @return The number of data points in the mean, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Get Gesture Mean Stroke"))
	int getGestureMeanStroke(int gesture_index, TArray<FVector>& Locations, TArray<FRotator>& Rotations, FVector& GestureLocation, FRotator& GestureRotation, float& GestureScale);

	/**
	* Delete a gesture sample recording from the set.
	* @param gesture_index The ID/index of the Gesture from which to delete a sample.
	* @param sample_index The ID/index of the sample to delete.
	* @return Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Delete Gesture Sample"))
	int deleteGestureSample(int gesture_index, int sample_index);

	/**
	* Delete all gesture sample recordings from the set.
	* @param gesture_index The ID/index of the Gesture from which to delete all sample.
	* @return Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Delete All Gesture Samples"))
	int deleteAllGestureSamples(int gesture_index);

	/**
	* Set a Metadata object of this GestureRecognition object.
	* @param    metadata        The new metadata data.
	* @return                   Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Set Metadata"))
	int setMetadata(const TArray<uint8>& metadata);

	/**
	* Set a Metadata object of this GestureRecognition object to a text string.
	* @param    metadata        The new metadata string.
	* @return                   Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Set Metadata as String"))
	int setMetadataAsString(const FString& metadata);

	/**
	* Get the current metadata of this GestureRecognition object.
	* @param    metadata        The metadata registered for this IGestureRecognition object.
	* @return                   Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Get Metadata"))
	int getMetadata(TArray<uint8>& metadata);

	/**
	* Get the current metadata of this GestureRecognition object, assuming it's a text string.
	* @param    metadata        The metadata registered for this GestureRecognition object as a string.
	* @return                   Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Get Metadata as String"))
	int getMetadataAsString(FString& metadata);

	/**
	* Save the neural network and recorded training data to file.
	* @param path Where to save the gesture database file.
	* @return Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Save Gestures to GestureDatabase File"))
	int saveToFile(const FFilePath& path);

	/**
	* Load the neural network and recorded training data from file.
	* @param path Path to the gesture database file to load.
	* @return Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Load GestureDatabase File"))
	int loadFromFile(const FFilePath& path);

	/**
	* Load the neural network and recorded training data from buffer.
	* @param buffer The gesture database to load.
	* @return Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Load GestureDatabase Buffer"))
	int loadFromBuffer(const TArray<uint8>& buffer);

	/**
	* Import recorded gestures from file.
	* @param path The file from which to import gestures.
	* @return Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Import Gestures From File"))
	int importFromFile(const FFilePath& path);

	/**
	* Import recorded gestures from buffer.
	* @param buffer The gesture database from which to import gestures.
	* @return Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Import Gestures From Buffer"))
	int importFromBuffer(const TArray<uint8>& buffer);

	/**
	* Save the neural network and recorded training data to file, asynchronously.
	* The function will return immediately, while the saving process will continue in the background.
	* Use 'isSaving' to check if the saving process is still ongoing.
	* You can use 'OnTrainingFinishDelegate' to receive a notification when saving finishes.
	* @param path Path where to save the gesture database file.
	* @return Zero on success, a negative error code on failure. Note that this only relates to *starting* the saving process.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Save GestureDatabase File Async"))
	int saveToFileAsync(const FFilePath& path);

	/**
	* Whether the Neural Network is currently saving to a file or buffer.
	* @return   True if the AI is currently saving, false if not.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Is Saving"))
	bool isSaving();

	/**
	* Cancel a currently running saving process.
	* @return   Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Cancel Saving"))
	int cancelSaving();

	/**
	* Load the neural network and recorded training data from file, asynchronously.
	* The function will return immediately, while the loading process will continue in the background.
	* Use 'isLoading' to check if the loading process is still ongoing.
	* You can use 'OnTrainingFinishDelegate' to receive a notification when loading finishes.
	* @param path Path to the gesture database file to load.
	* @return Zero on success, a negative error code on failure. Note that this only relates to *starting* the loading process.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Load GestureDatabase File Async"))
	int loadFromFileAsync(const FFilePath& path);

	/**
	* Load the neural network and recorded training data buffer, asynchronously.
	* The function will return immediately, while the loading process will continue in the background.
	* Use 'isLoading' to check if the loading process is still ongoing.
	* You can use 'OnTrainingFinishDelegate' to receive a notification when loading finishes.
	* @param buffer The gesture database to load.
	* @return Zero on success, a negative error code on failure. Note that this only relates to *starting* the loading process.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Load GestureDatabase Buffer Async"))
	int loadFromBufferAsync(const TArray<uint8>& buffer);

	/**
	* Whether the Neural Network is currently loading from a file or buffer.
	* @return   True if the AI is currently loading, false if not.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Is Loading"))
	bool isLoading();

	/**
	* Cancel a currently running loading process.
	* @return   Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Cancel Loading"))
	int cancelLoading();

	/**
	* Start train the Neural Network based on the the currently collected data.
	* @return Zero on success, an error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Start Training"))
	int startTraining();

	/**
	* Whether the Neural Network is currently training.
	* @return True if the Neural Network is currently training, false if not.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Is Training"))
	bool isTraining();

	/**
	* Stop the training process (last best result will be used).
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Stop Training"))
	void stopTraining();

	/**
	* Get maximum training time in seconds.
	* @return The maximum training time in seconds to use.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Get Max Training Time"))
	int getMaxTrainingTime();

	/**
	* Set maximum training time in seconds.
	* @param t The maximum training time in seconds to use.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Set Max Training Time"))
	void setMaxTrainingTime(int t);

	/**
	* Change the current policy on whether the AI should consider changes in head position during the gesturing.
	* This will change whether the data provided via calls to "updateHeadPosition" functions will be used,
	* so you also need to provide the changing head position via "updateHeadPosition" for this to have any effect.
	* The data provided via "updateHeadPosition" is stored regardless of the policy, but is only used if the policy
	* set by this function requires it.
	* @param  p     The policy on whether to use (and how to use) the data provided by "updateHeadPosition" during a gesture motion.
	* @return       Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Set Update Head Position Policy"))
	int setUpdateHeadPositionPolicy(GestureRecognition_UpdateHeadPositionPolicy p);

	/**
	* Query the current policy on whether the AI should consider changes in head position during the gesturing.
	* @return       The current policy on whether to use (and how to use) the data provided by "updateHeadPosition" during a gesture motion.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Get Update Head Position Policy"))
	GestureRecognition_UpdateHeadPositionPolicy getUpdateHeadPositionPolicy();

	/**
	* Get whether the tilting rotation of the users head (also called "roll" or "bank",
	* tilting the head to the site without changing the view direction) should be considered
	* when recording and performing gestures.
	* @return The frame of reference currently in use.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Get Rotational Frame Of Reference for Roll"))
	GestureRecognition_FrameOfReference getRotationalFrameOfReferenceRoll();

	/**
	* Set whether the tilting rotation of the users head (also called "roll" or "bank",
	* tilting the head to the site without changing the view direction) should be considered
	* when recording and performing gestures.
	* @param FrameOfReference The frame of reference to use.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Set Rotational Frame Of Reference for Roll"))
	void setRotationalFrameOfReferenceRoll(GestureRecognition_FrameOfReference FrameOfReference);

	/**
	* Get whether the vertical rotation of the users head (commonly called "pitch", looking up or down)
	* should be considered when recording and performing gestures.
	* @return The frame of reference currently in use.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Get Rotational Frame Of Reference for Pitch"))
	GestureRecognition_FrameOfReference getRotationalFrameOfReferencePitch();

	/**
	* Set whether the vertical rotation of the users head (commonly called "pitch", looking up or down)
	* should be considered when recording and performing gestures.
	* @param FrameOfReference The frame of reference to use.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Set Rotational Frame Of Reference for Pitch"))
	void setRotationalFrameOfReferencePitch(GestureRecognition_FrameOfReference FrameOfReference);

	/**
	* Get whether the horizontal rotation of the users head (commonly called "pan" or "yaw",
	* looking left or right) should be considered when recording and performing gestures.
	* @return The frame of reference currently in use.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Get Rotational Frame Of Reference for Yaw"))
	GestureRecognition_FrameOfReference getRotationalFrameOfReferenceYaw();

	/**
	* Set whether the horizontal rotation of the users head (commonly called "pan" or "yaw",
	* looking left or right) should be considered when recording and performing gestures.
	* @param FrameOfReference The frame of reference to use.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Set Rotational Frame Of Reference for Yaw"))
	void setRotationalFrameOfReferenceYaw(GestureRecognition_FrameOfReference FrameOfReference);

	/**
	* Get the order of rotations used when interpreting the Rotational Frame of Reference.
	* @return The rotation order currently in use.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Get Rotation Order for Rotational Frame Of Reference"))
	GestureRecognition_RotationOrder getRotationalFrameOfReferenceRotationOrder();

	/**
	* Set the order of rotations used when interpreting the Rotational Frame of Reference.
	* @param RotationOrder The rotation order to use.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Set Rotation Order for Rotational Frame Of Reference"))
	void setRotationalFrameOfReferenceRotationOrder(GestureRecognition_RotationOrder RotationOrder);

	/**
	* Delegate for training callbacks.
	* @param Source The GestureRecognitionActor from which the callback originated.
	* @param Performance The current gesture recognition performance (0~1).
	*/
	DECLARE_DYNAMIC_MULTICAST_DELEGATE_TwoParams(FTrainingCallbackDelegate, AGestureRecognitionActor*, Source, float, Performance);

	/**
	* Delegate for loading callbacks.
	* @param Source The GestureRecognitionActor from which the callback originated.
	* @param Status during loading: the percentage of loading progress; on finish: the result of the loading process (zero on success, a negative error code on failure).
	*/
	DECLARE_DYNAMIC_MULTICAST_DELEGATE_TwoParams(FLoadingCallbackDelegate, AGestureRecognitionActor*, Source, int, Status);

	/**
	* Delegate for saving callbacks.
	* @param Source The GestureRecognitionActor from which the callback originated.
	* @param Status during saving: the percentage of saving progress; on finish: the result of the saving process (zero on success, a negative error code on failure).
	*/
	DECLARE_DYNAMIC_MULTICAST_DELEGATE_TwoParams(FSavingCallbackDelegate, AGestureRecognitionActor*, Source, int, Status);

	/**
	* Delegate to be called during training when the recognition performance was increased.
	*/
	UPROPERTY(BlueprintAssignable, Category = "GestureRecognition Training Events")
		FTrainingCallbackDelegate OnTrainingUpdateDelegate;

	/**
	* Delegate to be called when training was finished (or stopped).
	*/
	UPROPERTY(BlueprintAssignable, Category = "GestureRecognition Training Events")
		FTrainingCallbackDelegate OnTrainingFinishDelegate;

	/**
	* Delegate to be called (repeatedly) during loading.
	*/
	UPROPERTY(BlueprintAssignable, Category = "GestureRecognition Loading Events")
		FLoadingCallbackDelegate OnLoadingUpdateDelegate;

	/**
	* Delegate to be called when loading was finished (or canceled).
	*/
	UPROPERTY(BlueprintAssignable, Category = "GestureRecognition Loading Events")
		FLoadingCallbackDelegate OnLoadingFinishDelegate;

	/**
	* Delegate to be called (repeatedly) during saving.
	*/
	UPROPERTY(BlueprintAssignable, Category = "GestureRecognition Saving Events")
		FSavingCallbackDelegate OnSavingUpdateDelegate;

	/**
	* Delegate to be called when saving was finished (or canceled).
	*/
	UPROPERTY(BlueprintAssignable, Category = "GestureRecognition Saving Events")
		FSavingCallbackDelegate OnSavingFinishDelegate;

	struct TrainingCallbackMetadata {
		AGestureRecognitionActor* actor;
		FTrainingCallbackDelegate* delegate;
	};
	struct LoadingCallbackMetadata {
		AGestureRecognitionActor* actor;
		FLoadingCallbackDelegate* delegate;
	};
	struct SavingCallbackMetadata {
		AGestureRecognitionActor* actor;
		FSavingCallbackDelegate* delegate;
	};
protected:
	TrainingCallbackMetadata TrainingUpdateMetadata;
	TrainingCallbackMetadata TrainingFinishMetadata;
	LoadingCallbackMetadata  LoadingUpdateMetadata;
	LoadingCallbackMetadata  LoadingFinishMetadata;
	SavingCallbackMetadata  SavingUpdateMetadata;
	SavingCallbackMetadata  SavingFinishMetadata;
	static void TrainingCallbackFunction(double performance, TrainingCallbackMetadata* metadata);
	static void LoadingCallbackFunction(int result, LoadingCallbackMetadata* metadata);
	static void SavingCallbackFunction(int result, SavingCallbackMetadata* metadata);
};
