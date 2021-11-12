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
	* Whether GestureDatabase files to be loaded/saved by this actor are using the Unity coordinate system.
	* Set to true if you want to load GestureDatabase files created with Unity apps (for example
	* the Unity-based GestureManager) or save GestureDatabase files for later use in Unity.
	* This internally switches the coordinate system (z-up -> y-up) and scales
	* the world (centimeters -> meters).
	* Regarding the Unreal VR world scale, see: World Settings -> World To Meters.
	*/
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Gesture Recognition")
	bool UnityCombatibilityMode = false;

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
	* \return   True if a gesture motion (stroke) was started and is ongoing, false if not.
	*/
	bool isStrokeStarted();
	
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
	* Get the number of recorded samples of a registered gesture.
	* @param index The Gesture ID / index for which to query the number of samples.
	* @return The number of samples, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Get Number Of Gesture Samples"))
	int getGestureNumberOfSamples(int index);

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
	* @param HMD_Location The location of the VR headset when the gesture was recorded.
	* @param HMD_Rotation The rotation of the VR headset when the gesture was recorded.
	* @param Locations The locations of the recorded data points.
	* @param Rotations The rotations of the recorded data points.
	* @return The number of data points in the sample recording, a negative error code on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Get Gesture Sample Stroke"))
	int getGestureSampleStroke(int gesture_index, int sample_index, bool processed, FVector& HMD_Location, FRotator& HMD_Rotation, TArray<FVector>& Locations, TArray<FRotator>& Rotations);

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
	* Save the neural network and recorded training data to file.
	* @param path Where to save the gesture database file.
	* @return Zero on success, false on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Save Gestures to GestureDatabase File"))
	int saveToFile(const FFilePath& path);

	/**
	* Load the neural network and recorded training data from file.
	* @param The gesture database file to load.
	* @return Zero on success, false on failure.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Load GestureDatabase File"))
	int loadFromFile(const FFilePath& path);

	/**
	* Import recorded gestures from file.
	* @param path The file from which to import gestures.
	*/
	UFUNCTION(BlueprintCallable, Category = "Gesture Recognition", meta = (DisplayName = "Import Gestures From File"))
	int importFromFile(const FFilePath& path);

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
	
	// Training Callback Functionality
	// UFUNCTION(BlueprintImplementableEvent, Category = "Gesture Recognition")
	// 	void OnTrainingUpdate(float performance);
	// 
	// DECLARE_EVENT_TwoParams(AGestureRecognitionActor, FTrainingCallbackEvent, AGestureRecognitionActor*, float);
	// FTrainingCallbackEvent& OnTrainingUpdateEvent() { return TrainingUpdateEvent; }
	// FTrainingCallbackEvent TrainingUpdateEvent;

	DECLARE_DYNAMIC_MULTICAST_DELEGATE_TwoParams(FTrainingCallbackDelegate, AGestureRecognitionActor*, Source, float, Performance);

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

	struct TrainingCallbackMetadata {
		AGestureRecognitionActor* actor;
		FTrainingCallbackDelegate* delegate;
	};
protected:
	TrainingCallbackMetadata TrainingUpdateMetadata;
	TrainingCallbackMetadata TrainingFinishMetadata;
	static void TrainingCallbackFunction(double performance, TrainingCallbackMetadata* metadata);
};
