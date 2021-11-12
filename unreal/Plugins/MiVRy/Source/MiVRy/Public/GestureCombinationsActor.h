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
	UPROPERTY(EditAnywhere, Category = "Gesture Combinations")
		int NumberOfParts = 2;
	
	/**
	* Whether GestureDatabase files to be loaded/saved by this actor are using the Unity coordinate system.
	* Set to true if you want to load GestureDatabase files created with Unity apps (for example
	* the Unity-based GestureManager) or save GestureDatabase files for later use in Unity.
	* This internally switches the coordinate system (z-up -> y-up) and scales
	* the world (centimeters -> meters).
	* Regarding the Unreal VR world scale, see: World Settings -> World To Meters.
	*/
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Gesture Combinations")
		bool UnityCombatibilityMode = false;

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
	* Query whether a gesture performance (gesture motion, stroke) was started and is currently ongoing.
	* \return   True if a gesture motion (stroke) was started and is ongoing, false if not.
	*/
	bool isStrokeStarted(int part);

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
	* @param similarity How similar the performed gesture is to the recorded gestures on a scale from zero to one.
	* @return The ID of the gesture combination, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Identify Gesture Combination"))
	int identifyGestureCombination(float& similarity);

	/**
	* Continuous gesture identification.
	* @param HMD_Location The current position of the headset.
	* @param HMD_Rotation The current rotation of the headset.
	* @param Similarity How similar the performed gesture motion is compared to recorded gesture samples (0~1).
	* @return The gesture combination ID identified, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Continuous Identify"))
	int contdIdentify(const FVector& HMD_Location, const FRotator& HMD_Rotation, float& Similarity);

	/**
	* Continuous gesture identification.
	* @param HMD_Location The current position of the headset.
	* @param HMD_Rotation The current rotation of the headset.
	* @param Similarity How similar the performed gesture motion is compared to recorded gesture samples (0~1).
	* @return The gesture combination ID identified, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Continuous Identify (Quaternion Rotation)"))
	int contdIdentifyQ(const FVector& HMD_Location, const FQuat& HMD_Rotation, float& Similarity);

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
	* @param to_gesture_index The ID of the gesture into which to copy.
	* @param mirror_x Whether to mirror the samples about the x-axis.
	* @param mirror_y Whether to mirror the samples about the y-axis.
	* @param mirror_z Whether to mirror the samples about the z-axis.
	* @return Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Copy Gesture"))
	int copyGesture(int from_part, int from_gesture_index, int to_part, int to_gesture_index, bool mirror_x = false, bool mirror_y = false, bool mirror_z = false);

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
	* Get the number of recorded samples of a registered gesture.
	* @param part The combination part or hand side index.
	* @param index The gesture ID.
	* @return The number of sampled which have been recorded, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Get Gesture Number Of Samples"))
	int getGestureNumberOfSamples(int part, int index);

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
	* @param HMD_Location The headset location at the time of the recording.
	* @param HMD_Rotation The headset rotation at the time of the recording.
	* @param Locations The positional data points of the sample.
	* @param Rotations The rotational data points of the sample.
	* @return Zero on success, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Get Gesture Sample Stroke"))
	int getGestureSampleStroke(int part, int gesture_index, int sample_index, bool processed, FVector& HMD_Location, FRotator& HMD_Rotation, TArray<FVector>& Locations, TArray<FRotator>& Rotations);

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
	* Import recorded training data from file.
	* @param path Path of the file to import.
	* @return Zero on success, an error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Combinations", meta = (DisplayName = "Import from GestureDatabase File"))
	int importFromFile(const FFilePath& path);

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
	* Delegate for training callbacks.
	* @param Source The GestureCombinationsActor from which the callback originated.
	* @param Performance The current gesture recognition performance (0~1).
	*/
	DECLARE_DYNAMIC_MULTICAST_DELEGATE_TwoParams(FTrainingCallbackDelegate, AGestureCombinationsActor*, Source, float, Performance);

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

	struct TrainingCallbackMetadata {
		AGestureCombinationsActor* actor;
		FTrainingCallbackDelegate* delegate;
	};
protected:
	TrainingCallbackMetadata TrainingUpdateMetadata;
	TrainingCallbackMetadata TrainingFinishMetadata;
	static void TrainingCallbackFunction(double performance, TrainingCallbackMetadata* metadata);
};
