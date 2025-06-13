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
#include "GestureCombinationsActor.generated.h"

class IGestureCombinations;

UCLASS(ClassGroup = GestureRecognition, hideCategories = (Object, LOD, Physics, Collision, Cooking, Actor, Rendering, Input))
class MIVRY_API AGestureCombinationsActor : public AActor
{
	GENERATED_BODY()
private:
	IGestureCombinations* gco = nullptr; //!< Gesture Combinations object.

public:	
	// Sets default values for this actor's properties
	AGestureCombinationsActor();
	virtual ~AGestureCombinationsActor();

protected:
	// Called when the game starts or when spawned
	virtual void BeginPlay() override;

public:
	virtual void Tick(float DeltaTime) override;

	/**
	* The number of gesture parts (1 for one-handed gestures, 2 for two-handed gestures, 2+ for multi-part gestures).
	*/
	UPROPERTY(EditAnywhere, BlueprintReadWrite, BlueprintSetter = setNumberOfParts, Category = "Gesture Combinations")
		int NumberOfParts = 2;

	/**
	* Set the number of parts on this GestureCombinationsActor.
	* [WARNING] Changing the number of parts erases all internal data, including recorded gestures.
	* @param The number of gesture parts (1 for one-handed gestures, 2 for two-handed gestures, 2+ for multi-part gestures).
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Activate License"))
		void setNumberOfParts(int NewNumberOfParts);
	
	/**
    * Which VR plugin you're using in your project.
    * By default, UE5 uses OpenXR plugin as other plug-ins have been deprecated.
    * However, in UE4 several plugins were available including "OculusVR" which used a different coordinate
    * system for the controller.
	*/
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Gesture Combinations", meta = (DisplayName = "Unreal VR Plugin"))
		GestureRecognition_VRPlugin UnrealVRPlugin = GestureRecognition_VRPlugin::OpenXR;
	
	/**
	* Which coordinate system is used inside the MiVRy AI.
	* If you recorded your gestures in another coordinate system (for example: in Unity with the Gesture Manager VR App),
	* select the correct coorinate system here to automatically convery between your project and the gesture recordings in the
	* MiVRy gesture database file.
    * Regarding the Unreal VR world scale, see: World Settings -> World To Meters.
	*/
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Gesture Combinations", meta = (DisplayName = "MiVRy Coordinate System"))
		GestureRecognition_CoordinateSystem MivryCoordinateSystem = GestureRecognition_CoordinateSystem::Unreal_OpenXR;

	/**
	* License ID (name) of your MiVRy license.
	* Leave emtpy for free version.
	*/
	UPROPERTY(EditAnywhere, Category = "Gesture Combinations")
		FString LicenseName;

	/**
	* License Key of your MiVRy license.
	* Leave emtpy for free version.
	*/
	UPROPERTY(EditAnywhere, Category = "Gesture Combinations")
		FString LicenseKey;

	/**
	* Provide a license to enable additional functionality.
	* @param license_name   The license name text.
	* @param license_key    The license key text.
	* @return   Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Activate License"))
	int activateLicense(const FString& license_name, const FString& license_key);

	/**
	* Provide a license file to enable additional functionality.
	* @param license_file_path	The file path to the license file.
	* @return   Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Activate License File"))
	int activateLicenseFile(const FString& license_file_path);

	/**
	* Check if a license was activated to enable additional functionality.
	* @return   One if a license was activated, zero if not.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Get License Status"))
	int getLicenseStatus() const;

	/**
    * Get whether a subgestures / parts / hand is currently used (enabled) in this multi-gesture object.
    * @param  part              The sub-gesture index (or side).
    * @return                   True if the part is used/enabled, false if it was disabled.
    */
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Get Part Enabled"))
    bool getPartEnabled(int part);

    /**
    * Set whether a subgestures / parts / hand is currently used (enabled) in this multi-gesture object.
    * @param  part              The sub-gesture index (or side).
    * @param  enabled           Whether the sub-gesture part (or side) should be used or disabled.
    * @return                   Zero on success, a negative error code on failure.
    */
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Set Part Enabled"))
    int setPartEnabled(int part, bool enabled);

	/**
	* Start new gesture motion.
	* @param part The combination part or hand side index.
	* @param HMD_Position The current position of the VR headset.
	* @param HMD_Rotation The current rotation/orientation of the VR headset.
	* @param record_as_sample Gesture ID if you want to record a sample for a gesture, -1 to identify a gesture.
	* @return Zero on success, an error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Start Stroke"))
	int startStroke(int part, const FVector& HMD_Position, const FRotator& HMD_Rotation, int record_as_sample=-1);
	
	/**
	* Start new gesture motion.
	* @param part The combination part or hand side index.
	* @param HMD_Position The current position of the VR headset.
	* @param HMD_Rotation The current rotation/orientation of the VR headset.
	* @param record_as_sample Gesture ID if you want to record a sample for a gesture, -1 to identify a gesture.
	* @return Zero on success, an error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Start Stroke (Quaternion Rotation)"))
	int startStrokeQ(int part, const FVector& HMD_Position, const FQuat& HMD_Rotation, int record_as_sample=-1);

	/**
	* Update the current position of the HMD/headset during a gesture performance (stroke).
	* @param  HMD_Position		Current position of the headset.
	* @param  HMD_Rotation      Current orientation/rotation of the headset.
	* @return                   Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Update Head Position"))
	int updateHeadPosition(const FVector& HMD_Position, const FRotator& HMD_Rotation);

	/**
	* Update the current position of the HMD/headset during a gesture performance (stroke).
	* @param  HMD_Position		Current position of the headset.
	* @param  HMD_Rotation      Current orientation/rotation of the headset.
	* @return                   Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Update Head Position (Quaternion Rotation)"))
	int updateHeadPositionQ(const FVector& HMD_Position, const FQuat& HMD_Rotation);

	/**
	* Continue gesture motion, providing new hand data.
	* @param part The combination part or hand side index.
	* @param HandPosition The current position of the hand performing the gesture.
	* @param HandRotation The current rotation/orientation of the hand performing the gesture.
	* @return Zero on success, an error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Continue Stroke"))
	int contdStroke(int part, const FVector& HandPosition, const FRotator& HandRotation);
	
	/**
	* Continue gesture motion, providing new hand data.
	* @param part The combination part or hand side index.
	* @param HandPosition The current position of the hand performing the gesture.
	* @param HandRotation The current rotation/orientation of the hand performing the gesture.
	* @return Zero on success, an error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Continue Stroke (Quaternion Rotation)"))
	int contdStrokeQ(int part, const FVector& HandPosition, const FQuat& HandRotation);

	/**
	* End a gesture motion for one hand or part of the combination.
	* After all gesture motions for all combination parts / hands are finished,
	* use identifyGestureCombination() to identify the whole combination.
	* @param part The combination part or hand side index.
	* @param GesturePosition The location at which the gesture was performed.
	* @param GestureRotation The orientation/direction at which the gesture was performed.
	* @param GestureScale The size / scale at which the gesture was performed.
	* @return Zero on success, an error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "End Stroke"))
	int endStroke(int part, FVector& GesturePosition, FRotator& GestureRotation, float& GestureScale);
	
	/**
	* End a gesture motion for one hand or part of the combination.
	* After all gesture motions for all combination parts / hands are finished,
	* use identifyGestureCombination() to identify the whole combination.
	* @param part The combination part or hand index.
	* @param GesturePosition The location at which the gesture was performed.
	* @param GestureRotation The orientation/direction at which the gesture was performed.
	* @param GestureScale The size / scale at which the gesture was performed.
	* @return Zero on success, an error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "End Stroke (Quaternion Rotation)"))
	int endStrokeQ(int part, FVector& GesturePosition, FQuat& GestureRotation, float& GestureScale);

	/**
	* Get all the probabilities and similarities (for each registered gesture) of the last gesture performance.
	* @param    part            The sub-gesture index (or side) of the gesture motion.
	* @param    probabilities   Array to receive the propability values (0~1) for each registered gesture.
	* @param    similarities    Array to receive the similarity values (0~1) for each registered gesture.
	* @return                   Zero on success, or a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Get all Probabilities and Similarities (last stroke)"))
	int getPartProbabilitiesAndSimilarities(int part, TArray<float>& probabilities, TArray<float>& similarities);

	/**
	* Query whether a gesture performance (gesture motion, stroke) was started and is currently ongoing.
	* @return   True if a gesture motion (stroke) was started and is ongoing, false if not.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Is Stroke Started"))
	bool isStrokeStarted(int part);

	/**
	* Prune the current stroke (gesture motion).
	* @param    part            The sub-gesture index (or side) of the gesture motion.
	* @param    num             The number of tracking data points to retain. -1 for no numeric limit.
	* @param    ms              The time duration of tracking data to retain (in milliseconds). -1 for no time limit.
	* @return   The number of retained tracking data points, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Prune Stroke"))
	int pruneStroke(int part, int num, int ms);

	/**
	* Cancel a gesture motion for one hand or part of the combination.
	* @param part The hand side or part index for which to cancel the gesture.
	* @return Zero on success, an error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Cancel Stroke"))
	int cancelStroke(int part);

	/**
	* Identify the last performed gesture combination.
	* Use this *after* startStroke()/contdStroke()/endStroke() has been completed
	* for all combination parts/hands.
	* @param probability The probability (0~1) expressing how likely the performed gesture motion was actually the identified gesture.
	* @param similarity How similar the performed gesture is to the recorded gestures on a scale from zero to one.
	* @param parts_probabilities The probability (0~1) of the gesture performance for each part/side (array must be numberOfParts() in length).
	* @param parts_similarities The similarity (0~1) of the gesture performance for each part/side (array must be numberOfParts() in length).
	* @return The ID of the gesture combination, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Identify Gesture Combination"))
	int identifyGestureCombination(float& probability, float& similarity, TArray<float>& parts_probabilities, TArray<float>& parts_similarities);

	/**
	* Continuous gesture identification.
	* @param HMD_Location The current position of the headset.
	* @param HMD_Rotation The current rotation of the headset.
	* @param Similarity How similar the performed gesture motion is compared to recorded gesture samples (0~1).
	* @param PartsProbabilities The probability values (scale from 0 to 1) for each of the combination parts (eg. hands, sides).
    * @param PartsSimilarities The similarity values (scale from 0 to 1) for each of the combination parts (eg. hands, sides).
	* @return The gesture combination ID identified, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Continuous Identify"))
	int contdIdentify(const FVector& HMD_Location, const FRotator& HMD_Rotation, float& Similarity, TArray<float>& PartsProbabilities, TArray<float>& PartsSimilarities);

	/**
	* Continuous gesture identification.
	* @param HMD_Location The current position of the headset.
	* @param HMD_Rotation The current rotation of the headset.
	* @param Similarity How similar the performed gesture motion is compared to recorded gesture samples (0~1).
	* @param PartsProbabilities The probability values (scale from 0 to 1) for each of the combination parts (eg. hands, sides).
    * @param PartsSimilarities The similarity values (scale from 0 to 1) for each of the combination parts (eg. hands, sides).
	* @return The gesture combination ID identified, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Continuous Identify (Quaternion Rotation)"))
	int contdIdentifyQ(const FVector& HMD_Location, const FQuat& HMD_Rotation, float& Similarity, TArray<float>& PartsProbabilities, TArray<float>& PartsSimilarities);

	/**
	* Continuous gesture recording.
	* @param HMD_Location The current position of the headset.
	* @param HMD_Rotation The current rotation of the headset.
	* @return Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Continuous Record"))
	int contdRecord(const FVector& HMD_Location, const FRotator& HMD_Rotation);

	/**
	* Continuous gesture recording.
	* @param HMD_Location The current position of the headset.
	* @param HMD_Rotation The current rotation of the headset.
	* @return Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Continuous Record (Quaternion Rotation)"))
	int contdRecordQ(const FVector& HMD_Location, const FQuat& HMD_Rotation);

	/**
	* Get time frame for continuous gesture identification in milliseconds.
	* @param part The hand side index or gesture part ID for which to retrieve the Identification Period.
	* @return The Identification Period in milliseconds, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Get Continuous Identification Period"))
	int getContdIdentificationPeriod(int part); 

	/**
	* Set time frame for continuous gesture identification in milliseconds.
	* @param part The hand side index or gesture part ID for which to get the Identification Period.
	* @param ms The Identification Period in milliseconds.
	* @return Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Set Continuous Identification Period"))
	int setContdIdentificationPeriod(int part, int ms); 

	/**
	* Get smoothing for continuous gesture identification in number of samples.
	* @param part The hand side index or gesture part ID for which to retrieve the smoothing.
	* @return The number of smoothing samples, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Get Continuous Identification Smoothing"))
	int getContdIdentificationSmoothing(int part); 

	/**
	* Set smoothing for continuous gesture identification in number of samples.
	* @param part The hand side index or gesture part ID for which to set the smoothing.
	* @param samples The number of smoothing samples.
	* @return Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Get Continuous Identification Smoothing"))
	int setContdIdentificationSmoothing(int part, int samples); 

	/**
	* Get the number of gestures currently recorded in the i's system.
	* @param part The combination part for which to get the number of gestures.
	* @return The number of gestures or a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Get Number of Gestures"))
	int numberOfGestures(int part);

	/**
	* Delete the recorded gesture with the specified index.
	* @param part The combination part or hand side index for which delete a gesture.
	* @param index The ID of the gesture to delete.
	* @return Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Delete Gesture"))
	int deleteGesture(int part, int index);

	/**
	* Delete recorded gestures.
	* @param part The combination part or hand side index for which to delete the gestures.
	* @return Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Delete All Gestures"))
	int deleteAllGestures(int part);

	/**
	* Create new gesture.
	* @param part The combination part or hand side index for which create a gestures.
	* @param name The name of the new gesture.
	* @return The ID of the newly created gesture, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Create Gesture"))
	int createGesture(int part, const FString& name);

	/**
	* Copy gesture from one part/side to another.
	* @param from_part The combination part or hand side index from which to copy.
	* @param from_gesture_index The ID of the gesture to copy.
	* @param to_part The combination part or hand side index to which to copy.
	* @param to_gesture_index The ID of the gesture into which to copy. -1 to create a new gesture.
	* @param mirror Whether to mirror the samples (left/right).
	* @return Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Copy Gesture"))
	int copyGesture(int from_part, int from_gesture_index, int to_part, int to_gesture_index=-1, bool mirror=false);

	/**
	* Get the gesture recognition score of the current neural network (0~1).
	* @param part The combination part of hand side index for which to retrieve the score.
	* @param all_samples Whether to calculate the performance using all recorded samples, not just a separated test set.
	* @return The gesture gecognition score (0~1), -1 if an error occurred.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Get Gesture Recognition Score"))
	float gestureRecognitionScore(int part, bool all_samples = false);

	/**
	* Get the name of a registered gesture.
	* @param part The combination part or hand side ID from which to query a gesture name.
	* @param index The gesture ID of the gesture whose name to query.
	* @return The name of the gesture, an empty string on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Get Gesture Name"))
	FString getGestureName(int part, int index);
	
	/**
	* Get the current metadata of a registered gesture.
	* @param    part        The combination part or hand side ID from which to query a gesture name.
	* @param    index       The gesture ID of the gesture whose metadata to get.
	* @param    metadata    The metadata registered for this IGestureRecognition object.
	* @return               Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Get Gesture Metadata"))
	int getGestureMetadata(int part, int index, TArray<uint8>& metadata);

	/**
	* Get the current metadata of a registered gesture, assuming it's a text string.
	* @param    part        The combination part or hand side ID from which to query a gesture name.
	* @param    index       The gesture ID of the gesture whose metadata to get.
	* @param    metadata    The metadata registered for this GestureRecognition object as a string.
	* @return               Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Get Gesture Metadata as String"))
	int getGestureMetadataAsString(int part, int index, FString& metadata);

	/**
	* Get whether a registered gesture is enabled or disabled.
	* A disabled gesture is not used in training and identification, but will retain its recorded samples.
	* @param    part        The combination part or hand side ID from which to query a gesture name.
	* @param    index       The gesture ID of the gesture whose status to get.
	* @return               True if the gesture is enabled, false if disabled.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Get Gesture Enabled"))
	bool getGestureEnabled(int part, int index);

	/**
	* Get the number of recorded samples of a registered gesture.
	* @param part The combination part or hand side index.
	* @param index The gesture ID.
	* @return The number of sampled which have been recorded, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Get Gesture Number Of Samples"))
	int getGestureNumberOfSamples(int part, int index);
	
    /**
    * Get the type of a previously recorded sample stroke.
    * 0 = Standard.
    * 1 = Continuous.
    * @param   part            The sub-gesture index (or side).
    * @param   gesture_index   The zero-based index (ID) of the gesture from where to retrieve the number of data points.
    * @param   sample_index    The zero-based index (ID) of the sample for which to retrieve the number of data points.
    * @return  The type (ID) of the recorded sample stroke, a negative error code on failure.
    */
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Get Gesture Sample Type"))
    int getGestureSampleType(int part, int gesture_index, int sample_index) const;

    /**
    * Set the type of a previously recorded sample stroke.
    * 0 = Standard.
    * 1 = Continuous.
    * @param   part            The sub-gesture index (or side).
    * @param   gesture_index   The zero-based index (ID) of the gesture from where to retrieve the number of data points.
    * @param   sample_index    The zero-based index (ID) of the sample for which to retrieve the number of data points.
    * @param   type            The type ID to set for the sample stroke.
    * @return  The type (ID) of the recorded sample stroke, a negative error code on failure.
    */
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Set Gesture Sample Type"))
    int setGestureSampleType(int part, int gesture_index, int sample_index, int type);

	/**
	* Get the number of data points a sample has.
	* @param part The combination part or hand side index.
	* @param index The gesture ID.
	* @param sample_index The index number (zero-based) of the recorded sample.
	* @param processed Whether to retrieve the length of the raw recording (false) or processed sample (true).
	* @return The number of data points in the recording.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Get Gesture Sample Length"))
	int getGestureSampleLength(int part, int gesture_index, int sample_index, bool processed);

	/**
	* Retrieve a sample stroke.
	* @param part The combination part or hand side index.
	* @param index The gesture ID.
	* @param sample_index The index number (zero-based) of the recorded sample.
	* @param processed Whether to retrieve the raw recording (false) or processed sample (true).
	* @param Locations The positional data points of the sample.
	* @param Rotations The rotational data points of the sample.
	* @param HMD_Locations The headset location at the time of the recording.
	* @param HMD_Rotations The headset rotation at the time of the recording.
	* @return Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Get Gesture Sample Stroke"))
	int getGestureSampleStroke(int part, int gesture_index, int sample_index, bool processed, TArray<FVector>& Locations, TArray<FRotator>& Rotations, TArray<FVector>& HMD_Locations, TArray<FRotator>& HMD_Rotations);

	/**
	* Get the number of samples of the gesture mean (average over samples).
	* @param part The combination part or hand side index.
	* @param index The gesture ID.
	* @return The number of samples in the gesture recording mean.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Get Gesture Mean Length"))
	int getGestureMeanLength(int part, int gesture_index);

	/**
	* Retrieve a gesture mean (average over samples).
	* The coordinate system used is a standardized gesture coordinate system, where the x-axis
	* is primary direction of the gesture, and the y-axis is the secondary direction of the gesture.
	* The coordinate system is normalized so that the standard deviation of points' distances from the origin is one.
	* @param part The combination part or hand side index.
	* @param gesture_index The gesture ID.
	* @param Locations The positional data points of the mean, in the gesture's own coordinate system.
	* @param Rotations The rotational data points of the mean, in the gesture's own coordinate system.
	* @param GestureLocation The overall position relative to (ie: "as seen from") the headset.
	* @param GestureRotation The overall rotation relative to (ie: "as seen from") the headset.
	* @param GestureScale The overall scale of the average gesture.
	* @return Zero on success, an error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Get Gesture Mean Stroke"))
	int getGestureMeanStroke(int part, int gesture_index, TArray<FVector>& Locations, TArray<FRotator>& Rotations, FVector& GestureLocation, FRotator& GestureRotation, float& GestureScale);

	/**
	* Delete a gesture sample recording of a gesture.
	* @param part The combination part or hand side index.
	* @param gesture_index The gesture ID.
	* @param sample_index The index number (zero-based) of the recorded sample.
	* @return Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Delete Gesture Sample"))
	int deleteGestureSample(int part, int gesture_index, int sample_index);

	/**
	* Delete all gesture sample recordings of a gesture.
	* @param part The combination part or hand side index.
	* @param gesture_index The gesture ID.
	* @return Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Delete All Gesture Samples"))
	int deleteAllGestureSamples(int part, int gesture_index);

	/**
	* Set the name of a registered gesture.
	* @param part The combination part or hand side index.
	* @param index The gesture ID.
	* @param name The new name for the gesture.
	* @return Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Set Gesture Name"))
	int setGestureName(int part, int index, const FString& name);

	/**
	* Set a Metadata object of a registered gesture.
	* @param    part        The combination part or hand side index.
	* @param    index       The gesture ID of the gesture whose metadata to get.
	* @param    metadata    The new metadata data.
	* @return               Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Set Gesture Metadata"))
	int setGestureMetadata(int part, int index, const TArray<uint8>& metadata);

	/**
	* Set a Metadata object of a registered gesture to a text string.
	* @param    part        The combination part or hand side index.
	* @param    index       The gesture ID of the gesture whose metadata to get.
	* @param    metadata    The new metadata string.
	* @return               Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Set Gesture Metadata as String"))
	int setGestureMetadataAsString(int part, int index, const FString& metadata);

	/**
	* Enable/disable a registered gesture.
	* A disabled gesture is not used in training and identification, but will retain its recorded samples.
	* @param    part        The combination part or hand side index.
	* @param    index       The gesture ID of the gesture whose status to set.
	* @param    enabled     Whether the gesture is supposed to be enabled (default) or disabled.
	* @return               Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Set Gesture Enabled"))
	int setGestureEnabled(int part, int index, bool enabled);

	/**
	* Save the neural network and recorded training data to file.
	* @param path Path of the file where to save the gestures and AI.
	* @return Zero on success, an error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Save to GestureDatabase File"))
	int saveToFile(const FFilePath& path);

	/**
	* Load Gesture Combinations from file.
	* @param path Path of the file to load.
	* @return Zero on success, an error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Load GestureDatabase File"))
	int loadFromFile(const FFilePath& path);

	/**
	* Load Gesture Combinations from buffer.
	* @param buffer Array containing the Gesture Database data.
	* @return Zero on success, an error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Load GestureDatabase from Buffer"))
	int loadFromBuffer(const TArray<uint8>& buffer);

	/**
	* Import recorded training data from file.
	* @param path Path of the file to import.
	* @return Zero on success, an error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Import from GestureDatabase File"))
	int importFromFile(const FFilePath& path);

	/**
	* Import recorded training data from buffer.
	* @param buffer Array containing the Gesture Database data.
	* @return Zero on success, an error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Import from GestureDatabase Buffer"))
	int importFromBuffer(const TArray<uint8>& buffer);

	/**
	* Get the number of registered gesture combinations.
	* @return Number of available gesture combinations, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Get Number Of Gesture Combinations"))
	int numberOfGestureCombinations();

	/**
	* Delete the gesture combination with the specified index.
	* @param index The ID/index of the gesture combination to delete.
	* @return Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Delete Gesture Combination"))
	int deleteGestureCombination(int index);

	/**
	* Delete recorded gestures.
	* @return Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Delete All Gesture Combinations"))
	int deleteAllGestureCombinations();

	/**
	* Create new gesture combination.
	* @param name The name of the new gesture combination.
	* @return The ID of the new gesture combination, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Create Gesture Combination"))
	int  createGestureCombination(const FString& name);

	/**
	* Set which gesture this multi-gesture expects for this part.
	* @param combination_index The gesture combination ID for which to set the part's gesture.
	* @param part The combination part or hand side index.
	* @param gesture_index The gesture ID.
	* @return Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Set Combination Part Gesture"))
	int setCombinationPartGesture(int combination_index, int part, int gesture_index);

	/**
	* Get which gesture this multi-gesture expects for this part.
	* @param combination_index The gesture combination ID for which to get the part's gesture.
	* @param part The combination part or hand side index.
	* @return The ID of the gesture set for this combination and part.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Get Combination Part Gesture"))
	int  getCombinationPartGesture(int combination_index, int part);

	/**
	* Get the name of a registered multi-gesture.
	* @param index The ID of the gesture combination.
	* @return The current name of the gesture combination.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Get Gesture Combination Name"))
	FString getGestureCombinationName(int index);

	/**
	* Set the name of a registered multi-gesture.
	* @param index The ID of the gesture combination.
	* @param name The new name for the gesture combination.
	* @return Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Set Gesture Combination Name"))
	int setGestureCombinationName(int index, const FString& name);

	/**
	* Get the Metadata of a registered multi-gesture combination.
	* @param	index       The gesture combination ID whose Metadata to get.
	* @param    metadata    The Metadata registered with the multi-gesture combination.
	* @return				Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Get Gesture Combination Metadata"))
	int getGestureCombinationMetadata(int index, TArray<uint8>& metadata);

	/**
	* Get the Metadata of a registered multi-gesture combination, assuming it's a text string.
	* @param    index       The gesture combination ID whose Metadata to get.
	* @param    metadata	The Metadata registered with the multi-gesture combination.
	* @return				Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Get Gesture Combination Metadata as String"))
	int getGestureCombinationMetadataAsString(int index, FString& metadata);

	/**
	* Set the Metadata of a registered multi-gesture combination.
	* @param    index       The gesture combination ID whose metadata to set.
	* @param    metadata    The new Metadata for the multi-gesture combination.
	* @return               Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Set Gesture Combination Metadata"))
	int setGestureCombinationMetadata(int index, const TArray<uint8>& metadata);

	/**
	* Set the Metadata of a registered multi-gesture combination as a text string.
	* @param    index       The gesture combination ID whose metadata to set.
	* @param    metadata    The new Metadata for the multi-gesture combination.
	* @return               Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Set Gesture Combination Metadata as String"))
	int setGestureCombinationMetadataAsString(int index, const FString& metadata);

	/**
	* Set Metadata for this GestureCombinations object.
	* @param    metadata    The new Metadata.
	* @return               Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Set Metadata"))
	int setMetadata(const TArray<uint8>& metadata);

	/**
	* Set Metadata for this GestureCombinations object as a text string.
	* @param    metadata    The new Metadata.
	* @return               Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Set Metadata as String"))
	int setMetadataAsString(const FString& metadata);

	/**
	* Get the current metadata of this GestureCombinations object.
	* @param    metadata	The metadata registered for this GestureCombinations.
	* @return               Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Get Metadata"))
	int getMetadata(TArray<uint8>& metadata);

	/**
	* Get the current metadata of this GestureCombinations object, assuming it's a text string.
	* @param    metadata	The metadata registered for this GestureCombinations as string.
	* @return               Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Get Metadata as String"))
	int getMetadataAsString(FString& metadata);

	/**
	* Save the neural network and recorded training data to file, asynchronously.
	* The function will return immediately, while the saving process will continue in the background.
	* Use 'isSaving' to check if the saving process is still ongoing.
	* You can use 'OnTrainingFinishDelegate' to receive a notification when saving finishes.
	* @param path Path where to save the gesture database file.
	* @return Zero on success, a negative error code on failure. Note that this only relates to *starting* the saving process.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Save GestureDatabase File Async"))
	int saveToFileAsync(const FFilePath& path);

	/**
	* Whether the Neural Network is currently saving to a file or buffer.
	* @return   True if the AI is currently saving, false if not.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Is Saving"))
	bool isSaving();

	/**
	* Cancel a currently running saving process.
	* @return   Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Cancel Saving"))
	int cancelSaving();

	/**
	* Load the neural network and recorded training data from file, asynchronously.
	* The function will return immediately, while the loading process will continue in the background.
	* Use 'isLoading' to check if the loading process is still ongoing.
	* You can use 'OnTrainingFinishDelegate' to receive a notification when loading finishes.
	* @param path Path to the gesture database file to load.
	* @return Zero on success, a negative error code on failure. Note that this only relates to *starting* the loading process.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Load GestureDatabase File Async"))
	int loadFromFileAsync(const FFilePath& path);

	/**
	* Load the neural network and recorded training data buffer, asynchronously.
	* The function will return immediately, while the loading process will continue in the background.
	* Use 'isLoading' to check if the loading process is still ongoing.
	* You can use 'OnTrainingFinishDelegate' to receive a notification when loading finishes.
	* @param buffer The gesture database to load.
	* @return Zero on success, a negative error code on failure. Note that this only relates to *starting* the loading process.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Load GestureDatabase Buffer Async"))
	int loadFromBufferAsync(const TArray<uint8>& buffer);

	/**
	* Whether the Neural Network is currently loading from a file or buffer.
	* @return True if the AI is currently loading, false if not.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Is Loading"))
	bool isLoading();

	/**
	* Cancel a currently running loading process.
	* @return Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Cancel Loading"))
	int cancelLoading();

	/**
	* Start train the Neural Network based on the the currently collected data.
	* @return Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Start Training"))
	int startTraining(); 

	/**
	* Whether the Neural Network is currently training.
	* @return True on success, false on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Is Training"))
	bool isTraining();

	/**
	* Stop the training process (last best result will be used).
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Stop Training"))
	void stopTraining();

	/**
	* Get the gesture recognition score of the current neural network (0~1).
	* @return The recognition score of the neural network (0~1).
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Get Recognition Score"))
	float recognitionScore();

	/**
	* Set the maximum time for training in seconds.
	* @return The current maximum training time in seconds.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Get Max Training Time"))
	int getMaxTrainingTime();

	/**
	* Get the current maximum training time in seconds.
	* @param t The desired maximum training time in seconds.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Set Max Training Time"))
	void setMaxTrainingTime(int t);

    /**
    * Get the number of maximum parallel training threads.
    * Zero or a negative number are interpreted as an unlimited number of parallel threads.
    * @return                   The number of maximum parallel training threads.
    */
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Set Max Training Threads"))
    int getMaxTrainingThreads();

    /**
    * Set the number of maximum parallel training threads.
    * Zero or a negative number are interpreted as an unlimited number of parallel threads.
    * @param    n               The number of maximum parallel training threads.
    */
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Set Max Training Threads"))
    void setMaxTrainingThreads(int n);

	/**
	* Change the current policy on whether the AI should consider changes in head position during the gesturing.
	* This will change whether the data provided via calls to "updateHeadPosition" functions will be used,
	* so you also need to provide the changing head position via "updateHeadPosition" for this to have any effect.
	* The data provided via "updateHeadPosition" is stored regardless of the policy, but is only used if the policy
	* set by this function requires it.
	* @param  part  The sub-gesture index (or side).
	* @param  p     The policy on whether to use (and how to use) the data provided by "updateHeadPosition" during a gesture motion.
	* @return       Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Set Update Head Position Policy"))
	int setUpdateHeadPositionPolicy(int part, GestureRecognition_UpdateHeadPositionPolicy p);

	/**
	* Query the current policy on whether the AI should consider changes in head position during the gesturing.
	* @param   part The sub-gesture index (or side).
	* @return       The current policy on whether to use (and how to use) the data provided by "updateHeadPosition" during a gesture motion.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Get Update Head Position Policy"))
	GestureRecognition_UpdateHeadPositionPolicy getUpdateHeadPositionPolicy(int part);

	/**
	* Get whether the tilting rotation of the users head (also called "roll" or "bank",
	* tilting the head to the site without changing the view direction) should be considered
	* when recording and performing gestures.
	* @return The frame of reference currently in use.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Get Rotational Frame Of Reference for Roll"))
	GestureRecognition_FrameOfReference getRotationalFrameOfReferenceRoll();

	/**
	* Set whether the tilting rotation of the users head (also called "roll" or "bank",
	* tilting the head to the site without changing the view direction) should be considered
	* when recording and performing gestures.
	* @param FrameOfReference The frame of reference to use.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Set Rotational Frame Of Reference for Roll"))
	void setRotationalFrameOfReferenceRoll(GestureRecognition_FrameOfReference FrameOfReference);

	/**
	* Get whether the vertical rotation of the users head (commonly called "pitch", looking up or down)
	* should be considered when recording and performing gestures.
	* @return The frame of reference currently in use.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Get Rotational Frame Of Reference for Pitch"))
	GestureRecognition_FrameOfReference getRotationalFrameOfReferencePitch();

	/**
	* Set whether the vertical rotation of the users head (commonly called "pitch", looking up or down)
	* should be considered when recording and performing gestures.
	* @param FrameOfReference The frame of reference to use.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Set Rotational Frame Of Reference for Pitch"))
	void setRotationalFrameOfReferencePitch(GestureRecognition_FrameOfReference FrameOfReference);

	/**
	* Get whether the horizontal rotation of the users head (commonly called "pan" or "yaw",
	* looking left or right) should be considered when recording and performing gestures.
	* @return The frame of reference currently in use.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Get Rotational Frame Of Reference for Yaw"))
	GestureRecognition_FrameOfReference getRotationalFrameOfReferenceYaw();

	/**
	* Set whether the horizontal rotation of the users head (commonly called "pan" or "yaw",
	* looking left or right) should be considered when recording and performing gestures.
	* @param FrameOfReference The frame of reference to use.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Set Rotational Frame Of Reference for Yaw"))
	void setRotationalFrameOfReferenceYaw(GestureRecognition_FrameOfReference FrameOfReference);

	/**
	* Get the order of rotations used when interpreting the Rotational Frame of Reference.
	* @return The rotation order currently in use.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Get Rotation Order for Rotational Frame Of Reference"))
		GestureRecognition_RotationOrder getRotationalFrameOfReferenceRotationOrder();

	/**
	* Set the order of rotations used when interpreting the Rotational Frame of Reference.
	* @param RotationOrder The rotation order to use.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Set Rotation Order for Rotational Frame Of Reference"))
		void setRotationalFrameOfReferenceRotationOrder(GestureRecognition_RotationOrder RotationOrder);

	/**
	* Delegate for training callbacks.
	* @param Source The GestureCombinationsActor from which the callback originated.
	* @param Performance The current gesture recognition performance (0~1).
	*/
	DECLARE_DYNAMIC_MULTICAST_DELEGATE_TwoParams(FTrainingCallbackDelegate, AGestureCombinationsActor*, Source, float, Performance);

	/**
	* Delegate for loading callbacks.
	* @param Source The GestureCombinationsActor from which the callback originated.
	* @param Status During loading: the percentage of loading progress; on finish: the result of the loading process (zero on success, a negative error code on failure).
	*/
	DECLARE_DYNAMIC_MULTICAST_DELEGATE_TwoParams(FLoadingCallbackDelegate, AGestureCombinationsActor*, Source, int, Status);

	/**
	* Delegate for saving callbacks.
	* @param Source The GestureCombinationsActor from which the callback originated.
	* @param Status During saving: the percentage of saving progress; on finish: the result of the saving process (zero on success, a negative error code on failure).
	*/
	DECLARE_DYNAMIC_MULTICAST_DELEGATE_TwoParams(FSavingCallbackDelegate, AGestureCombinationsActor*, Source, int, Status);

	/**
	* Delegate to be called during training when the recognition performance was increased.
	*/
	UPROPERTY(BlueprintAssignable, Category = "GestureCombinations Training Events")
		FTrainingCallbackDelegate OnTrainingUpdateDelegate;

	/**
	* Delegate to be called when training was finished (or stopped).
	*/
	UPROPERTY(BlueprintAssignable, Category = "GestureCombinations Training Events")
		FTrainingCallbackDelegate OnTrainingFinishDelegate;

	/**
	* Delegate to be called (repeatedly) during loading.
	*/
	UPROPERTY(BlueprintAssignable, Category = "GestureCombinations Loading Events")
		FLoadingCallbackDelegate OnLoadingUpdateDelegate;

	/**
	* Delegate to be called when loading was finished (or canceled).
	*/
	UPROPERTY(BlueprintAssignable, Category = "GestureCombinations Loading Events")
		FLoadingCallbackDelegate OnLoadingFinishDelegate;

	/**
	* Delegate to be called (repeatedly) during saving.
	*/
	UPROPERTY(BlueprintAssignable, Category = "GestureCombinations Saving Events")
		FSavingCallbackDelegate OnSavingUpdateDelegate;

	/**
	* Delegate to be called when saving was finished (or canceled).
	*/
	UPROPERTY(BlueprintAssignable, Category = "GestureCombinations Saving Events")
		FSavingCallbackDelegate OnSavingFinishDelegate;

	struct TrainingCallbackMetadata {
		AGestureCombinationsActor* actor;
		FTrainingCallbackDelegate* delegate;
	};
	struct LoadingCallbackMetadata {
		AGestureCombinationsActor* actor;
		FLoadingCallbackDelegate* delegate;
	};
	struct SavingCallbackMetadata {
		AGestureCombinationsActor* actor;
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
