/*
 * MiVRy GestureRecognition - 3D gesture recognition library.
 * Version 2.6
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
 * 
 * --------------------------------------------------------------------------
 * 
 * # HOW TO USE:
 * 
 * (1) Place the GestureRecognition.h
 * as well as the .dll and/or .lib files appropriate for your platform
 * in your project. 
 * 
 * 
 * (2) Create a new Gesture recognition object and register the gestures that you want to identify later.
 * <code>
 * IGestureRecognition* gr = new GestureRecognition();
 * int myFirstGesture = gr->createGesture("my first gesture");
 * int mySecondGesture = gr->createGesture("my second gesture");
 * </code>
 * 
 * 
 * (3) Record a number of samples for each gesture by calling startStroke(), contdStroke() and endStroke()
 * for your registered gestures, each time inputting the headset and controller transformation.
 * <code>
 * double hmd_p[3] = <your headset or reference coordinate system position>;
 * double hmd_q[4] = <your headset or reference coordinate system rotation as x,y,z,w quaternions>;
 * gr->startStroke(hmd_p, hmd_q, myFirstGesture);
 * 
 * // repeat the following while performing the gesture with your controller:
 * double p[3] = <your VR controller position in space>;
 * double q[4] = <your VR controller rotation in space as x,y,z,w quaternions>;
 * gr->contdStrokeQ(p,q);
 * // ^ repead while performing the gesture with your controller.
 * 
 * gr->endStroke();
 * </code>
 * Repeat this multiple times for each gesture you want to identify.
 * We recommend recording at least 20 samples for each gesture.
 * 
 * 
 * (4) Start the training process by calling startTraining().
 * You can optionally register callback functions to receive updates on the learning progress
 * by calling setTrainingUpdateCallback() and setTrainingFinishCallback().
 * <code>
 * gr->setMaxTrainingTime(10); // Set training time to 10 seconds.
 * gr->startTraining();
 * </code>
 * You can stop the training process by calling stopTraining().
 * After training, you can check the gesture identification performance by calling recognitionScore()
 * (a value of 1 means 100% correct recognition).
 * 
 * 
 * (5) Now you can identify new gestures performed by the user in the same way
 * as you were recording samples:
 * <code>
 * double hmd_p[3] = <your headset or reference coordinate system position>;
 * double hmd_q[4] = <your headset or reference coordinate system rotation as x,y,z,w quaternions>;
 * gr->startStroke(hmd_p, hmd_q);
 * 
 * // repeat the following while performing the gesture with your controller:
 * double p[3] = <your VR controller position in space>;
 * double q[4] = <your VR controller rotation in space as x,y,z,w quaternions>;
 * gr->contdStroke(p,q);
 * // ^ repeat while performing the gesture with your controller.
 * 
 * int identifiedGesture = gr->endStroke();
 * if (identifiedGesture == myFirstGesture) {
 *     // ...
 * }
 * </code>
 * 
 * 
 * (6) Now you can save and load the artificial intelligence.
 * <code>
 * gr->saveToFile("C:/myGestures.dat");
 * // ...
 * gr->loadFromFile("C:/myGestures.dat");
 * </code>
 * 
 */

#ifndef __GESTURE_RECOGNITION_H
#define __GESTURE_RECOGNITION_H

#include <iostream>
#include <vector>
#include <cstring>

#define GESTURERECOGNITION_RESULT_SUCCESS                  0   //!< Return code for: function executed successfully.
#define GESTURERECOGNITION_RESULT_NOGESTURE                -1  //!< Return code for: No gesture (or combination) matches.
#define GESTURERECOGNITION_RESULT_ERROR_INVALIDINDEX       -2  //!< Return code for: invalid index provided to function.
#define GESTURERECOGNITION_RESULT_ERROR_INVALIDPATH        -3  //!< Return code for: invalid file path provided to function.
#define GESTURERECOGNITION_RESULT_ERROR_INVALIDFILE        -4  //!< Return code for: path to an invalid file provided to function.
#define GESTURERECOGNITION_RESULT_ERROR_NUMERICINSTABILITY -5  //!< Return code for: calculations failed due to numeric instability (too small or too large numbers).
#define GESTURERECOGNITION_RESULT_ERROR_DATACORRUPTION     -6  //!< Return code for: the internal state of the AI was corrupted.
#define GESTURERECOGNITION_RESULT_ERROR_INSUFFICIENTDATA   -7  //!< Return code for: available data (number of samples etc) is insufficient for this operation.
#define GESTURERECOGNITION_RESULT_ERROR_CURRENTLYTRAINING  -8  //!< Return code for: the operation could not be performed because the AI is currently training.
#define GESTURERECOGNITION_RESULT_ERROR_NOGESTURES         -9  //!< Return code for: no gestures registered.
#define GESTURERECOGNITION_RESULT_ERROR_NNINCONSISTENT    -10  //!< Return code for: the neural network is inconsistent - re-training might solve the issue.
#define GESTURERECOGNITION_RESULT_ERROR_CANNOTOVERWRITE   -11  //!< Return code for: file or object exists and can't be overwritten.
#define GESTURERECOGNITION_RESULT_ERROR_STROKENOTSTARTED  -12  //!< Return code for: gesture performance (gesture motion, stroke) was not started yet (missing startStroke()).
#define GESTURERECOGNITION_RESULT_ERROR_STROKENOTENDED    -13  //!< Return code for: gesture performance (gesture motion, stroke) was not finished yet (missing endStroke()).
#define GESTURERECOGNITION_RESULT_ERROR_INTERNALLYCORRUPT -14  //!< Return code for: the gesture recognition/combinations object is internally corrupted or inconsistent.
#define GESTURERECOGNITION_RESULT_ERROR_CURRENTLYLOADING  -15  //!< Return code for: the operation could not be performed because the AI is loading a gesture database file.
#define GESTURERECOGNITION_RESULT_ERROR_INVALIDLICENSE    -16  //!< Return code for: the provided license key is not valid or the operation is not permitted under the current license.
#define GESTURERECOGNITION_RESULT_ERROR_CURRENTLYSAVING   -17  //!< Return code for: the operation could not be performed because the AI is currently being saved to database file.
#define GESTURERECOGNITION_RESULT_ERROR_INVALIDPARAM      -18  //!< Return code for: invalid parameter(s) provided to function.

#define GESTURERECOGNITION_DEFAULT_CONTDIDENTIFICATIONPERIOD       1000//!< Default time frame for continuous gesture identification in milliseconds.
#define GESTURERECOGNITION_DEFAULT_CONTDIDENTIFICATIONSMOOTHING    3   //!< Default smoothing setting for continuous gesture identification in number of samples.

#define GESTURERECOGNITION_UPDATEHEADPOSITIONPOLICY_USELATEST  0 //!< Identifier for "Use the hmd position most recently submitted as current head position".
#define GESTURERECOGNITION_UPDATEHEADPOSITIONPOLICY_USEINITIAL 1 //!< Identifier for "Use the initial head position, don't use later head positional updates".

#define GESTURERECOGNITION_FRAMEOFREFERENCE_HEAD    0   //!< Identifier for interpreting gestures as seen from the headset/HMD (user point of view). 
#define GESTURERECOGNITION_FRAMEOFREFERENCE_WORLD   1   //!< Identifier for interpreting gestures as seen from world origin (global coordinates).

#define GESTURERECOGNITION_ROTATIONORDER_XYZ 0 //!< Identifier for x->y->z order of (Euler) rotation angles.
#define GESTURERECOGNITION_ROTATIONORDER_XZY 1 //!< Identifier for x->z->y order of (Euler) rotation angles.
#define GESTURERECOGNITION_ROTATIONORDER_YXZ 2 //!< Identifier for y->x->z order of (Euler) rotation angles.
#define GESTURERECOGNITION_ROTATIONORDER_YZX 3 //!< Identifier for y->z->x order of (Euler) rotation angles.
#define GESTURERECOGNITION_ROTATIONORDER_ZXY 4 //!< Identifier for z->x->y order of (Euler) rotation angles.
#define GESTURERECOGNITION_ROTATIONORDER_ZYX 5 //!< Identifier for z->y->x order of (Euler) rotation angles.

#ifdef _WIN32
#define GESTURERECOGNITION_LIBEXPORT __declspec(dllexport)
#define GESTURERECOGNITION_CALLCONV __cdecl
#else
#define GESTURERECOGNITION_LIBEXPORT
#define GESTURERECOGNITION_CALLCONV
#endif
#ifdef __cplusplus
extern "C" {
#endif
    typedef void* GESTURERECOGNITION_CALLCONV MetadataCreatorFunction(); //!< Function pointer type to a function that creates Metadata objects.
    typedef void GESTURERECOGNITION_CALLCONV LoadingCallbackFunction(int status, void* metadata); //!< Function pointer to an optional callback function to be called when loading gesture database files.
    typedef void GESTURERECOGNITION_CALLCONV SavingCallbackFunction(int status, void* metadata); //!< Function pointer to an optional callback function to be called when saving gesture database files.
    typedef void GESTURERECOGNITION_CALLCONV TrainingCallbackFunction(double performance, void* metadata); //!< Function pointer to an optional callback function to be called during training.
    typedef void GESTURERECOGNITION_CALLCONV DebugCallbackFunction(const char* message, void* metadata); //!< Function pointer to an optional callback function that will receive debugging information during runtime.
    GESTURERECOGNITION_LIBEXPORT void* GestureRecognition_create(); //!< Create new instance.
    GESTURERECOGNITION_LIBEXPORT void  GestureRecognition_delete(void* gro); //!< Delete instance.
    GESTURERECOGNITION_LIBEXPORT int   GestureRecognition_activateLicense(void* gro, const char* license_name, const char* license_key); //!< Provide a license to enable additional functionality.
    GESTURERECOGNITION_LIBEXPORT int   GestureRecognition_startStroke(void* gro, const double hmd_p[3], const double hmd_q[4], int record_as_sample); //!< Start new stroke.
    GESTURERECOGNITION_LIBEXPORT int   GestureRecognition_startStrokeM(void* gro, const double hmd[4][4], int record_as_sample); //!< Start new stroke.
    GESTURERECOGNITION_LIBEXPORT int   GestureRecognition_updateHeadPositionM(void* gro, const double hmd[4][4]); //!< Update the current position of the HMD/headset during a gesture performance (stroke).
    GESTURERECOGNITION_LIBEXPORT int   GestureRecognition_updateHeadPositionQ(void* gro, const double hmd_p[3], const double hmd_q[4]); //!< Update the current position of the HMD/headset during a gesture performance (stroke).
    GESTURERECOGNITION_LIBEXPORT int   GestureRecognition_contdStroke(void* gro, const double p[3]); //!< Continue stroke data input (translational data only).
    GESTURERECOGNITION_LIBEXPORT int   GestureRecognition_contdStrokeQ(void* gro, const double p[3], const double q[4]); //!< Continue stroke data input with rotational data in the form of a quaternion.
    GESTURERECOGNITION_LIBEXPORT int   GestureRecognition_contdStrokeE(void* gro, const double p[3], const double r[3]); //!< Continue stroke data input with rotational data in the form of a Euler rotation.
    GESTURERECOGNITION_LIBEXPORT int   GestureRecognition_contdStrokeM(void* gro, const double m[4][4]); //!< Continue stroke data input with a transformation matrix (translation and rotation).
    GESTURERECOGNITION_LIBEXPORT int   GestureRecognition_endStroke(void* gro, double pos[3], double* scale, double dir0[3], double dir1[3], double dir2[3]); //!< End the stroke and identify the gesture.
    GESTURERECOGNITION_LIBEXPORT int   GestureRecognition_endStrokeAndGetAllProbabilities(void* gro, double p[], int* n, double pos[3], double* scale, double dir0[3], double dir1[3], double dir2[3]); //!< End the stroke and get gesture probabilities.
    GESTURERECOGNITION_LIBEXPORT int   GestureRecognition_endStrokeAndGetSimilarity(void* gro, double* similarity, double pos[3], double* scale, double dir0[3], double dir1[3], double dir2[3]); //!< End the stroke and get similarity value.
    GESTURERECOGNITION_LIBEXPORT int   GestureRecognition_endStrokeAndGetAllProbabilitiesAndSimilarities(void* gro, double p[], double s[], int* n, double pos[3], double* scale, double dir0[3], double dir1[3], double dir2[3]); //!< End the stroke and get gesture probabilities and similarity values.
    GESTURERECOGNITION_LIBEXPORT int   GestureRecognition_isStrokeStarted(void* gro); //!< Query whether a gesture performance (gesture motion, stroke) was started and is currently ongoing.
    GESTURERECOGNITION_LIBEXPORT int   GestureRecognition_cancelStroke(void* gro); //!< Cancel a started stroke.

    GESTURERECOGNITION_LIBEXPORT int   GestureRecognition_contdIdentify(void* gro, const double hmd_p[3], const double hmd_q[4], double* similarity); //!< Continuous gesture identification.
    GESTURERECOGNITION_LIBEXPORT int   GestureRecognition_contdIdentifyM(void* gro, const double hmd[4][4], double* similarity); //!< Continuous gesture identification.
    GESTURERECOGNITION_LIBEXPORT int   GestureRecognition_contdIdentifyAndGetAllProbabilitiesAndSimilarities(void* gro, const double hmd_p[3], const double hmd_q[4], double p[], double s[], int* n);
    GESTURERECOGNITION_LIBEXPORT int   GestureRecognition_contdIdentifyAndGetAllProbabilitiesAndSimilaritiesM(void* gro, const double hmd[4][4], double p[], double s[], int* n);
    GESTURERECOGNITION_LIBEXPORT int   GestureRecognition_contdRecord(void* gro, const double hmd_p[3], const double hmd_q[4]); //!< Continuous gesture recording.
    GESTURERECOGNITION_LIBEXPORT int   GestureRecognition_contdRecordM(void* gro, const double hmd[4][4]); //!< Continuous gesture recording.
    GESTURERECOGNITION_LIBEXPORT int   GestureRecognition_getContdIdentificationPeriod(void* gro); //!< Get time frame for continuous gesture identification in milliseconds.
    GESTURERECOGNITION_LIBEXPORT int   GestureRecognition_setContdIdentificationPeriod(void* gro, int ms); //!< Set time frame for continuous gesture identification in milliseconds.
    GESTURERECOGNITION_LIBEXPORT int   GestureRecognition_getContdIdentificationSmoothing(void* gro); //!< Get smoothing for continuous gesture identification in number of samples.
    GESTURERECOGNITION_LIBEXPORT int   GestureRecognition_setContdIdentificationSmoothing(void* gro, int samples); //!< Set smoothing for continuous gesture identification in number of samples.

    GESTURERECOGNITION_LIBEXPORT int  GestureRecognition_numberOfGestures(void* gro); //!< Get the number of gestures currently recorded in the system.
    GESTURERECOGNITION_LIBEXPORT int  GestureRecognition_deleteGesture(void* gro, int index); //!< Delete the recorded gesture with the specified index.
    GESTURERECOGNITION_LIBEXPORT int  GestureRecognition_deleteAllGestures(void* gro); //!< Delete recorded gestures.
    GESTURERECOGNITION_LIBEXPORT int  GestureRecognition_createGesture(void* gro, const char* name, void* metadata); //!< Create new gesture.
    GESTURERECOGNITION_LIBEXPORT double GestureRecognition_recognitionScore(void* gro); //!< Get the gesture recognition score of the current neural network (0~1).
        
    GESTURERECOGNITION_LIBEXPORT const char* GestureRecognition_getGestureName(void* gro, int index); //!< Get the name of a registered gesture.
    GESTURERECOGNITION_LIBEXPORT int         GestureRecognition_getGestureNameLength(void* gro, int index); //!< Get the length of the name of a registered gesture.
    GESTURERECOGNITION_LIBEXPORT int         GestureRecognition_copyGestureName(void* gro, int index, char* buf, int buflen); //!< Copy the name of a registered gesture to a buffer.

    GESTURERECOGNITION_LIBEXPORT void*       GestureRecognition_getGestureMetadata(void* gro, int index); //!< Get the command of a registered gesture.
    GESTURERECOGNITION_LIBEXPORT int         GestureRecognition_getGestureNumberOfSamples(void* gro, int index); //!< Get the number of recorded samples of a registered gesture.
    GESTURERECOGNITION_LIBEXPORT int         GestureRecognition_getGestureSampleLength(void* gro, int gesture_index, int sample_index, int processed); //!< Get the number of data points a sample has.
    GESTURERECOGNITION_LIBEXPORT int         GestureRecognition_getGestureSampleStroke(void* gro, int gesture_index, int sample_index, int processed, int stroke_buf_size, double p[][3], double q[][4], double hmd_p[][3], double hmd_q[][4]); //!< Retrieve a sample stroke.
    GESTURERECOGNITION_LIBEXPORT int         GestureRecognition_getGestureMeanLength(void* gro, int gesture_index); //!< Get the number of samples of the gesture mean (average over samples).
    GESTURERECOGNITION_LIBEXPORT int         GestureRecognition_getGestureMeanStroke(void* gro, int gesture_index, double p[][3], double q[][4], int stroke_buf_size, double stroke_p[3], double stroke_q[4], double* scale); //!< Retrieve a gesture mean (average over samples).
    GESTURERECOGNITION_LIBEXPORT int         GestureRecognition_deleteGestureSample(void* gro, int gesture_index, int sample_index); //!< Delete a gesture sample recording from the set.
    GESTURERECOGNITION_LIBEXPORT int         GestureRecognition_deleteAllGestureSamples(void* gro, int gesture_index); //!< Delete all gesture sample recordings from the set.

    GESTURERECOGNITION_LIBEXPORT int GestureRecognition_setGestureName(void* gro, int index, const char* name); //!< Set the name of a registered gesture.
    GESTURERECOGNITION_LIBEXPORT int GestureRecognition_setGestureMetadata(void* gro, int index, void* metadata); //!< Set the command of a registered gesture.

    GESTURERECOGNITION_LIBEXPORT int GestureRecognition_getGestureEnabled(void* gro, int index); //!< Get whether a registered gesture is enabled or disabled.
    GESTURERECOGNITION_LIBEXPORT int GestureRecognition_setGestureEnabled(void* gro, int index, int enabled); //!< Enable/disable a registered gesture.

    GESTURERECOGNITION_LIBEXPORT int GestureRecognition_setUpdateHeadPositionPolicy(void* gro, int p); //!< Change the current policy on whether the AI should consider changes in head position during the gesturing.
    GESTURERECOGNITION_LIBEXPORT int GestureRecognition_getUpdateHeadPositionPolicy(void* gro); //!< Query the current policy on whether the AI should consider changes in head position during the gesturing.

    GESTURERECOGNITION_LIBEXPORT int GestureRecognition_saveToFile(void* gro, const char* path); //!< Save the neural network and recorded training data to file.
    GESTURERECOGNITION_LIBEXPORT int GestureRecognition_saveToStream(void* gro, void* stream); //!< Save the neural network and recorded training data to std::ofstream.
    GESTURERECOGNITION_LIBEXPORT int GestureRecognition_loadFromFile(void* gro, const char* path, MetadataCreatorFunction* createMetadata); //!< Load the neural network and recorded training data from file.
    GESTURERECOGNITION_LIBEXPORT int GestureRecognition_loadFromBuffer(void* gro, const char* buffer, int buffer_size, MetadataCreatorFunction* createMetadata); //!< Load the neural network and recorded training data from buffer.
    GESTURERECOGNITION_LIBEXPORT int GestureRecognition_loadFromStream(void* gro, void* stream, MetadataCreatorFunction* createMetadata); //!< Load the neural network and recorded training data from std::istream.
    GESTURERECOGNITION_LIBEXPORT int GestureRecognition_importFromFile(void* gro, const char* path, MetadataCreatorFunction* createMetadata); //!< Import recorded gestures from file.
    GESTURERECOGNITION_LIBEXPORT int GestureRecognition_importFromBuffer(void* gro, const char* buffer, int buffer_size, MetadataCreatorFunction* createMetadata); //!< Import recorded gestures from buffer.
    GESTURERECOGNITION_LIBEXPORT int GestureRecognition_importFromStream(void* gro, void* stream, MetadataCreatorFunction* createMetadata); //!< Import recorded gestures std::istream.
    GESTURERECOGNITION_LIBEXPORT int GestureRecognition_saveToFileAsync(void* gro, const char* path); //!< Save the neural network and recorded training data to file, asynchronously.
    GESTURERECOGNITION_LIBEXPORT int GestureRecognition_setSavingUpdateCallbackFunction(void* gro, SavingCallbackFunction* cbf); //!< Set the callback function to call (repeatedly) while saving. Set to null for no callback.
    GESTURERECOGNITION_LIBEXPORT int GestureRecognition_setSavingUpdateCallbackMetadata(void* gro, void* metadata); //!< Set the metadata object to be sent to the callback function to call during saving.
    GESTURERECOGNITION_LIBEXPORT int GestureRecognition_setSavingFinishCallbackFunction(void* gro, SavingCallbackFunction* cbf); //!< Set the callback function to call when saving finishes. Set to null for no callback.
    GESTURERECOGNITION_LIBEXPORT int GestureRecognition_setSavingFinishCallbackMetadata(void* gro, void* metadata); //!< Set the metadata object to be sent to the callback function to call when saving finishes.
    GESTURERECOGNITION_LIBEXPORT int GestureRecognition_isSaving(void* gro); //!< Whether the Neural Network is currently saving from a file or buffer.
    GESTURERECOGNITION_LIBEXPORT int GestureRecognition_cancelSaving(void* gro); //!< Cancel a currently running saving process.
    GESTURERECOGNITION_LIBEXPORT int GestureRecognition_loadFromFileAsync(void* gro, const char* path, MetadataCreatorFunction* createMetadata); //!< Load the neural network and recorded training data from file, asynchronously.
    GESTURERECOGNITION_LIBEXPORT int GestureRecognition_loadFromBufferAsync(void* gro, const char* buffer, int buffer_size, MetadataCreatorFunction* createMetadata); //!< Load the neural network and recorded training data from buffer, asynchronously.
    GESTURERECOGNITION_LIBEXPORT int GestureRecognition_setLoadingUpdateCallbackFunction(void* gro, LoadingCallbackFunction* cbf); //!< Set the callback function to call (repeatedly) while loading. Set to null for no callback.
    GESTURERECOGNITION_LIBEXPORT int GestureRecognition_setLoadingUpdateCallbackMetadata(void* gro, void* metadata); //!< Set the metadata object to be sent to the callback function to call during loading.
    GESTURERECOGNITION_LIBEXPORT int GestureRecognition_setLoadingFinishCallbackFunction(void* gro, LoadingCallbackFunction* cbf); //!< Set the callback function to call when loading finishes. Set to null for no callback.
    GESTURERECOGNITION_LIBEXPORT int GestureRecognition_setLoadingFinishCallbackMetadata(void* gro, void* metadata); //!< Set the metadata object to be sent to the callback function to call when loading finishes.
    GESTURERECOGNITION_LIBEXPORT int GestureRecognition_isLoading(void* gro); //!< Whether the Neural Network is currently loading from a file or buffer.
    GESTURERECOGNITION_LIBEXPORT int GestureRecognition_cancelLoading(void* gro); //!< Cancel a currently running loading process.

    GESTURERECOGNITION_LIBEXPORT int GestureRecognition_importGestureSamples(void* gro, const void* from_gro, int from_gesture_index, int into_gesture_index); //!< Import recorded gesture samples from another gesture recognition object.
    GESTURERECOGNITION_LIBEXPORT int GestureRecognition_importGestures(void* gro, const void* from_gro); //!< Import recorded gesture samples from another gesture recognition object, merging gestures by name.

    GESTURERECOGNITION_LIBEXPORT int  GestureRecognition_startTraining(void* gro); //!< Start train the Neural Network based on the the currently collected data.
    GESTURERECOGNITION_LIBEXPORT int  GestureRecognition_isTraining(void* gro); //!< Whether the Neural Network is currently training.
    GESTURERECOGNITION_LIBEXPORT int  GestureRecognition_stopTraining(void* gro); //!< Stop the training process (last best result will be used).

    GESTURERECOGNITION_LIBEXPORT int   GestureRecognition_getMaxTrainingTime(void* gro); //!< Get maximum training time in seconds.
    GESTURERECOGNITION_LIBEXPORT void  GestureRecognition_setMaxTrainingTime(void* gro, int t); //!< Set maximum training time in seconds.

    GESTURERECOGNITION_LIBEXPORT void GestureRecognition_setTrainingUpdateCallback(void* gro, TrainingCallbackFunction* cbf); //!< Set callback function to be called during training.
    GESTURERECOGNITION_LIBEXPORT void GestureRecognition_setTrainingFinishCallback(void* gro, TrainingCallbackFunction* cbf); //!< Set callback function to be called when training is finished.
    
    GESTURERECOGNITION_LIBEXPORT void GestureRecognition_setTrainingUpdateCallbackMetadata(void* gro, void* metadata); //!< Set metadata for callback function to be called during training.
    GESTURERECOGNITION_LIBEXPORT void GestureRecognition_setTrainingFinishCallbackMetadata(void* gro, void* metadata); //!< Set metadata for callback function to be called when training is finished.
    GESTURERECOGNITION_LIBEXPORT void* GestureRecognition_getTrainingUpdateCallbackMetadata(void* gro); //!< Get callback data for function to be called during training.
    GESTURERECOGNITION_LIBEXPORT void* GestureRecognition_getTrainingFinishCallbackMetadata(void* gro); //!< Get callback data for function to be called when training is finished.

    GESTURERECOGNITION_LIBEXPORT int   GestureRecognition_getIgnoreHeadRotationX(void* gro); //!< Get whether the horizontal rotation of the users head (commonly called "pan" or "yaw", looking left or right) should be considered when recording and performing gestures.
    GESTURERECOGNITION_LIBEXPORT void  GestureRecognition_setIgnoreHeadRotationX(void* gro, int on_off); //!< Set whether the horizontal rotation of the users head (commonly called "pan" or "yaw", looking left or right) should be considered when recording and performing gestures.
    GESTURERECOGNITION_LIBEXPORT int   GestureRecognition_getIgnoreHeadRotationY(void* gro); //!< Get whether the vertical rotation of the users head (commonly called "pitch", looking up or down) should be considered when recording and performing gestures.
    GESTURERECOGNITION_LIBEXPORT void  GestureRecognition_setIgnoreHeadRotationY(void* gro, int on_off); //!< Set whether the vertical rotation of the users head (commonly called "pitch", looking up or down) should be considered when recording and performing gestures.
    GESTURERECOGNITION_LIBEXPORT int   GestureRecognition_getIgnoreHeadRotationZ(void* gro); //!< Get whether the tilting rotation of the users head (also called "roll" or "bank", tilting the head to the site without changing the view direction) should be considered when recording and performing gestures.
    GESTURERECOGNITION_LIBEXPORT void  GestureRecognition_setIgnoreHeadRotationZ(void* gro, int on_off); //!< Set whether the tilting rotation of the users head (also called "roll" or "bank", tilting the head to the site without changing the view direction) should be considered when recording and performing gestures.

    GESTURERECOGNITION_LIBEXPORT int  GestureRecognition_getRotationalFrameOfReferenceX(void* gro); //!< Get wether gestures are interpreted as seen by the user or relative to the world, regarding their x-axis rotation (commonly: pitch, looking up or down).
    GESTURERECOGNITION_LIBEXPORT void GestureRecognition_setRotationalFrameOfReferenceX(void* gro, int i); //!< Set wether gestures are interpreted as seen by the user or relative to the world, regarding their x-axis rotation (commonly: pitch, looking up or down).
    GESTURERECOGNITION_LIBEXPORT int  GestureRecognition_getRotationalFrameOfReferenceY(void* gro); //!< Get wether gestures are interpreted as seen by the user or relative to the world, regarding their y-axis rotation (commonly: pan/yaw, looking left or right).
    GESTURERECOGNITION_LIBEXPORT void GestureRecognition_setRotationalFrameOfReferenceY(void* gro, int i); //!< Set wether gestures are interpreted as seen by the user or relative to the world, regarding their y-axis rotation (commonly: pan/yaw, looking left or right).
    GESTURERECOGNITION_LIBEXPORT int  GestureRecognition_getRotationalFrameOfReferenceZ(void* gro); //!< Get wether gestures are interpreted as seen by the user or relative to the world, regarding their z-axis rotation (commonly: roll, tilting the head).
    GESTURERECOGNITION_LIBEXPORT void GestureRecognition_setRotationalFrameOfReferenceZ(void* gro, int i); //!< Set wether gestures are interpreted as seen by the user or relative to the world, regarding their z-axis rotation (commonly: roll, tilting the head).
    GESTURERECOGNITION_LIBEXPORT int  GestureRecognition_getRotationalFrameOfReferenceRotationOrder(void* gro); //!< Get the ID of the order of rotation used when interpreting the rotational frame of reference (eg. Y->X->Z order of rotations).
    GESTURERECOGNITION_LIBEXPORT void GestureRecognition_setRotationalFrameOfReferenceRotationOrder(void* gro, int rotOrd); //!< Set the ID of the order of rotation used when interpreting the rotational frame of reference (eg. Y->X->Z order of rotations).

    GESTURERECOGNITION_LIBEXPORT int   GestureRecognition_setMetadata(void* gro, void* metadata); //!< Set a Metadata object of this IGestureRecognition object.
    GESTURERECOGNITION_LIBEXPORT void* GestureRecognition_getMetadata(void* gro); //!< Get the current metadata object of this IGestureRecognition object.

    GESTURERECOGNITION_LIBEXPORT void  GestureRecognition_setDebugCallbackFunction(DebugCallbackFunction* dbf); //!< Set callback function to receive debugging information.
    GESTURERECOGNITION_LIBEXPORT void  GestureRecognition_setDebugCallbackMetadata(void* metadata); //!< Set metadata to receive with debugging output callback function.

    GESTURERECOGNITION_LIBEXPORT const char* GestureRecognition_getVersionString(); //!< Get the version of this library as human-readable string.
    GESTURERECOGNITION_LIBEXPORT int   GestureRecognition_getVersionStringLength(); //!< Get the length of the version string.
    GESTURERECOGNITION_LIBEXPORT int   GestureRecognition_copyVersionString(char* buf, int buflen); //!< Copy the version string into a buffer.

    GESTURERECOGNITION_LIBEXPORT void* GestureRecognition_createDefaultMetadata(); //!< Create DefaultMetadata object.
    GESTURERECOGNITION_LIBEXPORT void  GestureRecognition_deleteDefaultMetadata(void* dmo); //!< Delete DefaultMetadata object.
    GESTURERECOGNITION_LIBEXPORT int   GestureRecognition_getDefaultMetadataSize(void* dmo); //!< Get DefaultMetadata data size (bytes).
    GESTURERECOGNITION_LIBEXPORT int   GestureRecognition_getDefaultMetadataData(void* dmo, void* data, int size); //!< Retrieve a copy of current DefaultMetadata data.
    GESTURERECOGNITION_LIBEXPORT int   GestureRecognition_setDefaultMetadataData(void* dmo, const void* data, int size); //!< Update DefaultMetadata current data.
    GESTURERECOGNITION_LIBEXPORT void* GestureRecognition_defaultMetadataCreatorFunction(); //!< Global function to create DefaultMetadata objects.
    GESTURERECOGNITION_LIBEXPORT MetadataCreatorFunction* GestureRecognition_getDefaultMetadataCreatorFunction(); //!< Global function to retrieve a pointer to the createDefaultMetadataCreatorFunction.
#ifdef __cplusplus
}

class GESTURERECOGNITION_LIBEXPORT IGestureRecognition
{
public:
    enum Result {
        Success = GESTURERECOGNITION_RESULT_SUCCESS //!< Return code for: function executed successfully.
        ,
        NoGesture = GESTURERECOGNITION_RESULT_NOGESTURE //!< Return code for: No gesture (or combination) matches.
        ,
        Error_InvalidIndex = GESTURERECOGNITION_RESULT_ERROR_INVALIDINDEX //!< Return code for: invalid index provided to function.
        ,
        Error_InvalidPath = GESTURERECOGNITION_RESULT_ERROR_INVALIDPATH //!< Return code for: invalid file path provided to function.
        ,
        Error_InvalidFile = GESTURERECOGNITION_RESULT_ERROR_INVALIDFILE //!< Return code for: path to an invalid file provided to function.
        ,
        Error_NumericInstability = GESTURERECOGNITION_RESULT_ERROR_NUMERICINSTABILITY //!< Return code for: calculations failed due to numeric instability (too small or too large numbers).
        ,
        Error_DataCorruption = GESTURERECOGNITION_RESULT_ERROR_DATACORRUPTION //!< Return code for: the internal state of the AI was corrupted.
        ,
        Error_InsufficientData = GESTURERECOGNITION_RESULT_ERROR_INSUFFICIENTDATA //!< Return code for: available data (number of samples etc) is insufficient for this operation.
        ,
        Error_CurrentlyTraining = GESTURERECOGNITION_RESULT_ERROR_CURRENTLYTRAINING //!< Return code for: the operation could not be performed because the AI is currently training.
        ,
        Error_NoGestures = GESTURERECOGNITION_RESULT_ERROR_NOGESTURES //!< Return code for: no gestures registered.
        ,
        Error_NNInconsistent = GESTURERECOGNITION_RESULT_ERROR_NNINCONSISTENT //!< Return code for: the neural network is inconsistent - re-training might solve the issue.
        ,
        Error_CannotOverwrite = GESTURERECOGNITION_RESULT_ERROR_CANNOTOVERWRITE //!< Return code for: file or object exists and can't be overwritten.
        ,
        Error_StrokeNotStarted = GESTURERECOGNITION_RESULT_ERROR_STROKENOTSTARTED //!< Return code for: gesture performance (gesture motion, stroke) was not started yet (missing startStroke()).
        ,
        Error_StrokeNotEnded = GESTURERECOGNITION_RESULT_ERROR_STROKENOTENDED //!< Return code for: gesture performance (gesture motion, stroke) was not finished yet (missing endStroke()).
        ,
        Error_InternallyCorrupted = GESTURERECOGNITION_RESULT_ERROR_INTERNALLYCORRUPT //!< Return code for: the gesture recognition/combinations object is internally corrupted or inconsistent.
        ,
        Error_CurrentlyLoading = GESTURERECOGNITION_RESULT_ERROR_CURRENTLYLOADING //!< Return code for: the operation could not be performed because the AI is loading a gesture database file.
        ,
        Error_InvalidLicense = GESTURERECOGNITION_RESULT_ERROR_INVALIDLICENSE //!< Return code for: the provided license key is not valid or the operation is not permitted under the current license.
        ,
        Error_CurrentlySaving = GESTURERECOGNITION_RESULT_ERROR_CURRENTLYSAVING //!< Return code for: the operation could not be performed because the AI is current being saved to a database file.
        ,
        Error_InvalidParameter = GESTURERECOGNITION_RESULT_ERROR_INVALIDPARAM //!< Return code for: invalid parameter(s) provided to function.
    };

    /**
    * Create new GestureRecognition object.
    */
    static IGestureRecognition* create();

    /**
    * Destructor.
    */
    virtual ~IGestureRecognition();

    /**
    * Provide a license to enable additional functionality.
    * \param license_name   The license name text.
    * \param license_key    The license key text.
    * \return   Zero on success, a negative error code on failure.
    */
    virtual int activateLicense(const char* license_name, const char* license_key)=0;

    /**
    * Interface for metadata objects to attach to a gesture.
    */
    struct Metadata
    {
        virtual ~Metadata() {};
        virtual bool writeToStream(std::ostream* stream)=0;
        virtual bool readFromStream(std::istream* stream)=0;
    };

    /**
    * Default implementation of a metadata object to attach to a gesture.
    */
    struct DefaultMetadata : public Metadata
    {
    private:
        void* m_data = nullptr;
        int m_size = 0;
    public:
        virtual ~DefaultMetadata() {
            if (this->m_data) {
                free(this->m_data);
            }
        };
        virtual bool writeToStream(std::ostream* stream) override {
            char size_str[32];
            snprintf(size_str, sizeof(size_str), "%i ", this->m_size);
            stream->write(size_str, std::strlen(size_str)); // without null-terminator
            stream->write((const char*)this->m_data, this->m_size);
            return (stream->fail() || stream->bad()) ? false : true;
        };
        virtual bool readFromStream(std::istream* stream) override {
            if (this->m_data) {
                free(this->m_data);
                this->m_data = 0;
            }
            this->m_size = 0;
            for (int i = 0; i < 32; i++) {
                char size_str[32];
                size_str[i] = (char)stream->get();
                if (size_str[i] == ' ') {
                    size_str[i] = 0;
                    this->m_size = atoi(size_str);
                    if (this->m_size <= 0) {
                        return false;
                    }
                    this->m_data = malloc(this->m_size);
                    if (this->m_data == 0) {
                        this->m_size = 0;
                        return false;
                    }
                    stream->read((char*)this->m_data, this->m_size);
                    if (stream->eof() || stream->fail() || stream->bad() || stream->gcount() != this->m_size) {
                        free(this->m_data);
                        this->m_data = 0;
                        this->m_size = 0;
                        return false;
                    }
                    return true;
                }
            }
            return false;
        };
        /**
        * Set the data of this Metadata object. The provided data chunk will be copied.
        * \param    data    Memory location from where to copy.
        * \param    size    Size of memory chunk to copy (in byte).
        * \return           Zero on success, a negative error code on failure.
        */
        int setData(const void* data, int size) {
            if (data == 0 || size == 0) {
                if (this->m_data) {
                    free(this->m_data);
                    this->m_data = 0;
                }
                this->m_size = 0;
                return IGestureRecognition::Success;
            }
            if (size != this->m_size) {
                if (this->m_data) {
                    free(this->m_data);
                }
                this->m_data = malloc(size);
                if (this->m_data == 0) {
                    this->m_size = 0;
                    return IGestureRecognition::Error_InternallyCorrupted;
                }
                this->m_size = size;
            }
            memcpy(this->m_data, data, this->m_size);
            return IGestureRecognition::Success;
        };
        /**
        * Get the data of this Metadata object. The provided data chunk will be copied.
        * \param    data    Memory location to copy to.
        * \param    size    Size of memory chunk that can be written (in byte).
        * \return           The number of bytes actually written.
        */
        int getData(void* data, int size) {
            if (this->m_data == nullptr || this->m_size == 0) {
                return 0;
            }
            const int s = std::min(size, this->m_size);
            memcpy(data, this->m_data, s);
            return s;
        };
        /**
        * Get the size of the data chunk of this Metadata object.
        * \return           Memory size in bytes.
        */
        int getSize() {
            return this->m_size;
        };
    };

    /**
    * Function pointer type to a function that creates Metadata objects.
    */
    typedef Metadata* MetadataCreatorFunction();

    /**
    * Function pointer to an optional callback function to be called when loading.
    * \param status             During loading: progess in percent; when finished: the return code of the loading process.
    */
    typedef void GESTURERECOGNITION_CALLCONV LoadingCallbackFunction(int status, void* metadata);

    /**
    * Function pointer to an optional callback function to be called when saving.
    * \param status             During loading: progess in percent; when finished: the return code of the loading process.
    */
    typedef void GESTURERECOGNITION_CALLCONV SavingCallbackFunction(int status, void* metadata);

    /**
    * Function pointer to an optional callback function to be called during training.
    * \param performance        The current percentage of correctly recognized gestures (0~1).
    * \param metadata           The meta data pointer set on trainingUpdateCallbackMetadata or trainingFinishCallbackMetadata.
    */
    typedef void GESTURERECOGNITION_CALLCONV TrainingCallbackFunction(double performance, void* metadata);

    /**
    * Function pointer to an optional callback function that will receive debugging information during runtime.
    * \param message            The debug message (null-terminated).
    * \param metadata           The meta data pointer set on debugCallbackMetadata.
    */
    typedef void GESTURERECOGNITION_CALLCONV DebugCallbackFunction(const char* message, void* metadata);

    /**
    * Start new stroke (gesture motion).
    * \param  hmd               Transformation matrix (4x4) of the current headset position and rotation.
    * \param  record_as_sample  Which gesture this stroke will be a sample for, or -1 to identify the gesture.
    * \return                   Zero on success, a negative error code on failure.
    */
    virtual int startStroke(const double hmd[4][4], int record_as_sample=-1)=0;

    /**
    * Start new stroke (gesture motion).
    * \param  hmd_p             Vector (x,y,z) of the current headset position.
    * \param  hmd_q             Quaternion (x,y,z,w) of the current headset rotation.
    * \param  record_as_sample  Which gesture this stroke will be a sample for, or -1 to identify the gesture.
    * \return                   Zero on success, a negative error code on failure.
    */
    virtual int startStroke(const double hmd_p[3], const double hmd_q[4], int record_as_sample=-1)=0;

    /**
    * Whether to update the hmd (frame of reference) position/rotation during the gesturing motion.
    */
    enum UpdateHeadPositionPolicy {
        UpdateHeadPositionPolicy_UseLatest  = GESTURERECOGNITION_UPDATEHEADPOSITIONPOLICY_USELATEST //!< Use the hmd position most recently submitted as current head position.
        ,
        UpdateHeadPositionPolicy_UseInitial = GESTURERECOGNITION_UPDATEHEADPOSITIONPOLICY_USEINITIAL //!< Use the initial head position, don't use later head positional updates.
        ,
        UpdateHeadPositionPolicies = 2 //!< Number of head position update policies.
    };

    /**
    * Change the current policy on whether the AI should consider changes in head position during the gesturing.
    * This will change whether the data provided via calls to "updateHeadPosition" functions will be used,
    * so you also need to provide the changing head position via "updateHeadPosition" for this to have any effect.
    * The data provided via "updateHeadPosition" is stored regardless of the policy, but is only used if the policy
    * set by this function requires it.
    * \param  p     The policy on whether to use (and how to use) the data provided by "updateHeadPosition" during a gesture motion.
    * \return       Zero on success, a negative error code on failure.
    */
    virtual int setUpdateHeadPositionPolicy(UpdateHeadPositionPolicy p)=0;

    /**
    * Query the current policy on whether the AI should consider changes in head position during the gesturing.
    * \return       The current policy on whether to use (and how to use) the data provided by "updateHeadPosition" during a gesture motion.
    */
    virtual UpdateHeadPositionPolicy getUpdateHeadPositionPolicy()=0;

    /**
     * Update the current position of the HMD/headset during a gesture performance (stroke).
    * \param  hmd               Transformation matrix (4x4) of the current headset position and rotation.
    * \return                   Zero on success, a negative error code on failure.
    */
    virtual int updateHeadPositionM(const double hmd[4][4])=0;

    /**
     * Update the current position of the HMD/headset during a gesture performance (stroke).
    * \param  hmd_p             Vector (x,y,z) of the current headset position.
    * \param  hmd_q             Quaternion (x,y,z,w) of the current headset rotation.
    * \return                   Zero on success, a negative error code on failure.
    */
    virtual int updateHeadPositionQ(const double hmd_p[3], const double hmd_q[4])=0;

    /**
    * Continue stroke (gesture motion) data input (translational data only).
    * \param    p               Vector (x,y,z) of the current controller position.
    * \return                   Zero on success, a negative error code on failure.
    */
    virtual int contdStroke(const double p[3])=0;

    /**
    * Continue stroke (gesture motion) data input with rotational data in the form of a quaternion.
    * \param    p               Vector (x,y,z) of the current controller position.
    * \param    q               Quaternion (x,y,z,w) of the current controller rotation.
    * \return                   Zero on success, a negative error code on failure.
    */
    virtual int contdStrokeQ(const double p[3], const double q[4])=0;

    /**
    * Continue stroke (gesture motion) data input with rotational data in the form of a Euler rotation.
    * \param    p               Vector (x,y,z) of the current controller position.
    * \param    r               Euler rotations (x,y,z) of the current controller rotation.
    * \return                   Zero on success, a negative error code on failure.
    */
    virtual int contdStrokeE(const double p[3], const double r[3])=0;

    /**
    * Continue stroke (gesture motion) data input with a transformation matrix (translation and rotation).
    * \param    m               Transformation matrix (4x4) of the current controller position and rotation.
    * \return                   Zero on success, a negative error code on failure.
    */
    virtual int contdStrokeM(const double m[4][4])=0;

    /**
    * End the stroke (gesture motion) and identify the gesture.
    * \param    pos             [OUT][OPTIONAL] The position where the gesture was performed.
    * \param    scale           [OUT][OPTIONAL] The scale (size) at which the gesture was performed.
    * \param    dir0            [OUT][OPTIONAL] The primary direction at which the gesture was performed.
    * \param    dir1            [OUT][OPTIONAL] The secondary direction at which the gesture was performed.
    * \param    dir2            [OUT][OPTIONAL] The least-significant direction at which the gesture was performed.
    * \return                   The gesture ID of the identified gesture, or a negative error code on failure.
    */
    virtual int endStroke(double pos[3]=0, double* scale=0, double dir0[3]=0, double dir1[3]=0, double dir2[3]=0)=0;

    /**
    * End the stroke (gesture motion) and get gesture probabilities.
    * \param    p               [OUT] Array of length n to which to write the probability values (each 0~1).
    * \param    n               [IN/OUT] The length of the array p. Will be overwritten with the number of probability values actually written into the p array.
    * \param    pos             [OUT][OPTIONAL] The position where the gesture was performed.
    * \param    scale           [OUT][OPTIONAL] The scale (size) at which the gesture was performed.
    * \param    dir0            [OUT][OPTIONAL] The primary direction at which the gesture was performed.
    * \param    dir1            [OUT][OPTIONAL] The secondary direction at which the gesture was performed.
    * \param    dir2            [OUT][OPTIONAL] The least-significant direction at which the gesture was performed.
    * \return                   The gesture ID of the identified gesture, or a negative error code on failure.
    */
    virtual int endStrokeAndGetAllProbabilities(double p[], int* n, double pos[3]=0, double* scale=0, double dir0[3]=0, double dir1[3]=0, double dir2[3]=0)=0;

    /**
    * End the stroke (gesture motion) and get similarity value.
    * \param    similarity      [OUT] The similarity (0~1) expressing how different the performed gesture motion was from the identified gesture.
    * \param    pos             [OUT][OPTIONAL] The position where the gesture was performed.
    * \param    scale           [OUT][OPTIONAL] The scale (size) at which the gesture was performed.
    * \param    dir0            [OUT][OPTIONAL] The primary direction at which the gesture was performed.
    * \param    dir1            [OUT][OPTIONAL] The secondary direction at which the gesture was performed.
    * \param    dir2            [OUT][OPTIONAL] The least-significant direction at which the gesture was performed.
    * \return                   The gesture ID of the identified gesture, or a negative error code on failure.
    */
    virtual int endStrokeAndGetSimilarity(double* similarity, double pos[3]=0, double* scale=0, double dir0[3]=0, double dir1[3]=0, double dir2[3]=0)=0;

    /**
    * End the stroke (gesture motion) and get gesture probabilities and similarity values.
    * \param    p               [OUT] Array of length n to which to write the probability values (each 0~1).
    * \param    s               [OUT] Array of length n to which to write the similarity values (each 0~1).
    * \param    n               [IN/OUT] The length of the arrays p and s. Will be overwritten with the number of probability values actually written into the p and s arrays.
    * \param    pos             [OUT][OPTIONAL] The position where the gesture was performed.
    * \param    scale           [OUT][OPTIONAL] The scale (size) at which the gesture was performed.
    * \param    dir0            [OUT][OPTIONAL] The primary direction at which the gesture was performed.
    * \param    dir1            [OUT][OPTIONAL] The secondary direction at which the gesture was performed.
    * \param    dir2            [OUT][OPTIONAL] The least-significant direction at which the gesture was performed.
    * \return                   The gesture ID of the identified gesture, or a negative error code on failure.
    */
    virtual int endStrokeAndGetAllProbabilitiesAndSimilarities(double p[], double s[], int* n, double pos[3]=0, double* scale=0, double dir0[3]=0, double dir1[3]=0, double dir2[3]=0)=0;

    /**
    * Query whether a gesture performance (gesture motion, stroke) was started and is currently ongoing.
    * \return   True if a gesture motion (stroke) was started and is ongoing, false if not.
    */
    virtual bool isStrokeStarted()=0;

    /**
    * Cancel a started stroke (gesture motion).
    * \return   Zero on success, an error code on failure.
    */
    virtual int cancelStroke()=0;

    /**
    * Continuous gesture identification.
    * \param  hmd_p             Vector (x,y,z) of the current headset position.
    * \param  hmd_q             Quaternion (x,y,z,w) of the current headset rotation.
    * \param  similarity        [OUT][OPTIONAL] The similarity (0~1) expressing how different the performed gesture motion was from the identified gesture.
    * \return                   The ID of the identified gesture on success, an negative error code on failure.
    */
    virtual int contdIdentify(const double hmd_p[3], const double hmd_q[4], double* similarity=0)=0;

    /**
    * Continuous gesture identification.
    * \param  hmd               Matrix of the current headset position and rotation.
    * \param  similarity        [OUT][OPTIONAL] The similarity (0~1) expressing how different the performed gesture motion was from the identified gesture.
    * \return                   The ID of the identified gesture on success, an negative error code on failure.
    */
    virtual int contdIdentifyM(const double hmd[4][4], double* similarity=0)=0;

    /**
    * Continuous gesture identification.
    * \param  hmd_p             Vector (x,y,z) of the current headset position.
    * \param  hmd_q             Quaternion (x,y,z,w) of the current headset rotation.
    * \param    p               [OUT] Array of length n to which to write the probability values (each 0~1).
    * \param    s               [OUT] Array of length n to which to write the similarity values (each 0~1).
    * \param    n               [IN/OUT] The length of the arrays p and s. Will be overwritten with the number of probability values actually written into the p and s arrays.
    * \return                   The gesture ID of the identified gesture, or a negative error code on failure.
    */
    virtual int contdIdentifyAndGetAllProbabilitiesAndSimilarities(const double hmd_p[3], const double hmd_q[4], double p[], double s[], int* n)=0;

    /**
    * Continuous gesture identification.
    * \param  hmd               Matrix of the current headset position and rotation.
    * \param    p               [OUT] Array of length n to which to write the probability values (each 0~1).
    * \param    s               [OUT] Array of length n to which to write the similarity values (each 0~1).
    * \param    n               [IN/OUT] The length of the arrays p and s. Will be overwritten with the number of probability values actually written into the p and s arrays.
    * \return                   The gesture ID of the identified gesture, or a negative error code on failure.
    */
    virtual int contdIdentifyAndGetAllProbabilitiesAndSimilaritiesM(const double hmd[4][4], double p[], double s[], int* n)=0;

    /**
    * Continuous gesture recording.
    * \param  hmd_p             Vector (x,y,z) of the current headset position.
    * \param  hmd_q             Quaternion (x,y,z,w) of the current headset rotation.
    * \return                   The ID of the recorded gesture on success, an negative error code on failure.
    */
    virtual int contdRecord(const double hmd_p[3], const double hmd_q[4])=0;

    /**
    * Continuous gesture recording.
    * \param  hmd               Matrix of the current headset position and rotation.
    * \return                   The ID of the recorded gesture on success, an negative error code on failure.
    */
    virtual int contdRecordM(const double hmd[4][4])=0;

    /**
    * Time frame in milliseconds for continuous gesture identification.
    */
    unsigned int contdIdentificationPeriod;

    /**
    * The number of samples to use for smoothing continuous gesture identification results.
    */
    unsigned int contdIdentificationSmoothing;

    /**
    * Get the number of gestures currently recorded in the system.
    * \return                   The number of gestures currently recorded in the system.
    */
    virtual int numberOfGestures()=0;

    /**
    * Delete the recorded gesture with the specified index.
    * \param    index           The ID (zero-based index) of the gesture to delete.
    * \return                   Zero on success, a negative error code on failure.
    */
    virtual int deleteGesture(int index)=0;

    /**
    * Delete all recorded gestures.
    * \return                   Zero on success, a negative error code on failure.
    */
    virtual int deleteAllGestures()=0;

    /**
    * Create new gesture.
    * \param    name            The name for the gesture.
    * \param    metadata        [OPTIONAL] Meta data to be stored with the gesture.
    * \return                   New gesture ID or a negative error code on failure.
    */
    virtual int createGesture(const char* name, Metadata* metadata=0)=0;

    /**
    * Get the gesture recognition score of the current neural network (0~1).
    * Here, 0 means that not a single gesture was correctly identified, and 1 means that 100% of gestures
    * were correctly identified.
    * \param    all_samples     Whether to use all recorded samples to estimate the recognition score,
    *                           even those that were not used in the training of the AI.
    * \return                   The gesture recognition score of the current neural network (0~1).
    */
    virtual double recognitionScore(bool all_samples=false)=0;

    /**
    * Get the name of a registered gesture.
    * \param    index           The gesture ID of the gesture whose name to query.
    * \return                   String name of the gesture, zero on failure.
    */
    virtual const char* getGestureName(int index)=0;

    /**
    * Get the length of the name of a registered gesture.
    * \param    index           The gesture ID of the gesture whose name to query.
    * \return                   The number of characters in the gesture name.
    */
    virtual int getGestureNameLength(int index)=0;

    /**
    * Copy the name of a registered gesture to a buffer.
    * \param    index           The gesture ID of the gesture whose name to copy.
    * \param    buf             [OUT] The string buffer to which to write the gesture name.
    * \param    buflen          The length of the buf string buffer.
    * \return                   The number of characters written to the buffer, zero on failure.
    */
    virtual int copyGestureName(int index, char* buf, int buflen)=0;

    /**
    * Get the metadata of a registered gesture.
    * \param    index           The gesture ID of the gesture whose metadata to get.
    * \return                   The metadata registered with the gesture, null on failure or if none was registered.
    */
    virtual Metadata* getGestureMetadata(int index)=0;

    /**
    * Get whether a registered gesture is enabled or disabled.
    * A disabled gesture is not used in training and identification, but will retain its recorded samples.
    * \param    index           The gesture ID of the gesture whose status to get.
    * \return                   True if the gesture is enabled, false if disabled.
    */
    virtual bool getGestureEnabled(int index)=0;

    /**
    * Get the number of recorded samples of a registered gesture.
    * \param    index           The gesture ID of the gesture whose number of samples to query.
    * \return                   The number of samples recorded for that gesture, a negative error code on failure.
    */
    virtual int getGestureNumberOfSamples(int index) const =0;

    /**
    * Get the number of data points a sample has.
    * \param   gesture_index   The zero-based index (ID) of the gesture from where to retrieve the number of data points.
    * \param   sample_index    The zero-based index (ID) of the sample for which to retrieve the number of data points.
    * \param   processed       Whether the number of raw data points should be retrieved (false) or the number of processed data points (true).
    * \return  The number of data points on that stroke sample, 0 if an error occurred.
    */
    virtual int getGestureSampleLength(int gesture_index, int sample_index, bool processed) const =0;

    /**
    * Retrieve a sample stroke.
    * \param   gesture_index   The zero-based index (ID) of the gesture from where to retrieve the sample.
    * \param   sample_index    The zero-based index (ID) of the sample to retrieve.
    * \param   processed       Whether the raw data points should be retrieved (false) or the processed data points (true).
    * \param   stroke_buf_size The length of p, q, hmd_p, hmd_q in number of data points. The function will at most write this many data points.
    * \param   p               [OUT][OPTIONAL] A place to store the stroke positional data. May be zero if this data is not required.
    * \param   q               [OUT][OPTIONAL] A place to store the stroke rotational data. May be zero if this data is not required.
    * \param   hmd_p           [OUT][OPTIONAL] A place to store the HMD positional data. May be zero if this data is not required.
    * \param   hmd_q           [OUT][OPTIONAL] A place to store the HMD rotational data. May be zero if this data is not required.
    * \return  The number of stroke sample data points that have been written, 0 if an error occurred.
    */
    virtual int getGestureSampleStroke(int gesture_index, int sample_index, bool processed, int stroke_buf_size, double p[][3], double q[][4], double hmd_p[][3], double hmd_q[][4]) const =0;

    /**
    * Get the number of samples of the gesture mean (average over samples).
    * \param   gesture_index   The zero-based index (ID) of the gesture from where to retrieve the sample.
    * \return  The number of stroke sample data points that the mean stroke consists of.
    */
    virtual int getGestureMeanLength(int gesture_index) const =0;

    /**
    * Retrieve a gesture mean (average over samples).
    * \param   gesture_index   The zero-based index (ID) of the gesture from where to retrieve the sample.
    * \param   p               [OUT] A place to store the stroke positional data. May be zero if this data is not required.
    * \param   q               [OUT] A place to store the stroke rotational data. May be zero if this data is not required.
    * \param   stroke_buf_size The length of p and/or q in number of data points. The function will at most write this many data points.
    * \param   stroke_p        [OUT][OPTIONAL] A place to store the average gesture position relative to (ie. as seen by) the headset. May be zero if this data is not required.
    * \param   stroke_q        [OUT][OPTIONAL] A place to store the average gesture rotation relative to (ie. as seen by) the headset. May be zero if this data is not required.
    * \param   scale           [OUT][OPTIONAL] A place to store the average gesture Scale. May be zero if this data is not required.
    * \return  The number of stroke sample data points that have been written, 0 if an error occurred.
    */
    virtual int getGestureMeanStroke(int gesture_index, double p[][3], double q[][4], int stroke_buf_size, double stroke_p[3], double stroke_q[4], double* scale) const =0;

    /**
    * Delete a gesture sample recording from the set.
    * \param   gesture_index   The zero-based index (ID) of the gesture from where to delete the sample.
    * \param   sample_index    The zero-based index (ID) of the sample to delete.
    * \return                  Zero on success, a negative error code on failure.
    */
    virtual int deleteGestureSample(int gesture_index, int sample_index)=0;

    /**
    * Delete all gesture sample recordings from the set.
    * \param   gesture_index   The zero-based index (ID) of the gesture from where to delete the sample.
    * \return                  Zero on success, a negative error code on failure.
    */
    virtual int deleteAllGestureSamples(int gesture_index)=0;

    /**
    * Set the name of a registered gesture.
    * \param    index           The gesture ID of the gesture whose name to set.
    * \param    name            The new name for the gesture.
    * \return                   Zero on success, a negative error code on failure.
    */
    virtual int setGestureName(int index, const char* name)=0;

    /**
    * Set the metadata of a registered gesture.
    * \param    index           The gesture ID of the gesture whose metadata to set.
    * \param    metadata        The new metadata for the gesture, null to deregister the current Metadata object.
    * \return                   Zero on success, a negative error code on failure.
    */
    virtual int setGestureMetadata(int index, Metadata* metadata)=0;

    /**
    * Enable/disable a registered gesture.
    * A disabled gesture is not used in training and identification, but will retain its recorded samples.
    * \param    index           The gesture ID of the gesture whose status to set.
    * \param    enabled         Whether the gesture is supposed to be enabled (default) or disabled.
    * \return                   Zero on success, a negative error code on failure.
    */
    virtual int setGestureEnabled(int index, bool enabled)=0;

    /**
    * Save the neural network and recorded training data to file.
    * \param    path            The file path at which to save the AI and recorded data.
    * \return                   Zero on success, a negative error code on failure.
    */
    virtual int saveToFile(const char* path)=0;

    /**
    * Save the neural network and recorded training data to std::ofstream.
    * \param    stream      The std::ostream pointer to which to write the AI and recorded data.
    * \return                   Zero on success, a negative error code on failure.
    */
    virtual int saveToStream(void* stream)=0;

    /**
    * Load the neural network and recorded training data from file.
    * \param    path            The file path from which to load the AI and recorded data.
    * \param    createMetadata  [OPTIONAL] The function which can parse the metadata which was stored with the gestures.
    * \return                   Zero on success, a negative error code on failure.
    */
    virtual int loadFromFile(const char* path, MetadataCreatorFunction* createMetadata=0)=0;

    /**
    * Load the neural network and recorded training data buffer.
    * \param    buffer          Memory buffer from which to load the AI and recorded data.
    * \param    buffer_size     Size of the memory buffer in byte.
    * \param    createMetadata  [OPTIONAL] The function which can parse the metadata which was stored with the gestures.
    * \return                   Zero on success, a negative error code on failure.
    */
    virtual int loadFromBuffer(const char* buffer, int buffer_size, MetadataCreatorFunction* createMetadata=0)=0;

    /**
    * Load the neural network and recorded training data from std::istream.
    * \param    stream          The std::istream from which to load the AI and recorded data.
    * \param    createMetadata  [OPTIONAL] The function which can parse the metadata which was stored with the gestures.
    * \return                   Zero on success, a negative error code on failure.
    */
    virtual int loadFromStream(void* stream, MetadataCreatorFunction* createMetadata=0)=0;

    /**
    * Import recorded gestures from file.
    * Gestures of the same name will be merged into one. The optional 'mapping' parameter will
    * inform you of how the merging was performed.
    * After importing, you have to run the AI training process again for the changes to take place.
    * \param    path            The file path from which to import gestures.
    * \param    createMetadata  [OPTIONAL] The function which can parse the metadata which was stored with the gestures.
    * \param    mapping         [OUT][OPTIONAL] A vector expressing for each gesture index of the file into which gesture it was merged.
    * \return                   Zero on success, a negative error code on failure.
    */
    virtual int importFromFile(const char* path, MetadataCreatorFunction* createMetadata=0, std::vector<int>* mapping=0)=0;

    /**
    * Import recorded gestures from buffer.
    * Gestures of the same name will be merged into one. The optional 'mapping' parameter will
    * inform you of how the merging was performed.
    * After importing, you have to run the AI training process again for the changes to take place.
    * \param    buffer          The memory buffer from which to import gestures.
    * \param    buffer_size     The size of the memory buffer in byte.
    * \param    createMetadata  [OPTIONAL] The function which can parse the metadata which was stored with the gestures.
    * \param    mapping         [OUT][OPTIONAL] A vector expressing for each gesture index of the file into which gesture it was merged.
    * \return                   Zero on success, a negative error code on failure.
    */
    virtual int importFromBuffer(const char* buffer, int buffer_size, MetadataCreatorFunction* createMetadata=0, std::vector<int>* mapping=0)=0;

    /**
    * Import recorded gestures from std::istream.
    * Gestures of the same name will be merged into one. The optional 'mapping' parameter will
    * inform you of how the merging was performed.
    * After importing, you have to run the AI training process again for the changes to take place.
    * \param    stream          The std::istream from which to import gestures.
    * \param    createMetadata  [OPTIONAL] The function which can parse the metadata which was stored with the gestures.
    * \param    mapping         [OUT][OPTIONAL] A vector expressing for each gesture index of the file into which gesture it was merged.
    * \return                   Zero on success, a negative error code on failure.
    */
    virtual int importFromStream(void* stream, MetadataCreatorFunction* createMetadata=0, std::vector<int>* mapping=0)=0;

    /**
    * Save the neural network and recorded training data to file, asynchronously.
    * The function will return immediately, while the saving process will continue in the background.
    * Use isSaving() to check if the loading process is still ongoing.
    * You can use setSavingCallback() to receive a function call when saving finishes.
    * \param    path            The file path where to save the AI and recorded data.
    * \return                   Zero on success, a negative error code on failure. Note that this only relates to *starting* the saving process.
    */
    virtual int saveToFileAsync(const char* path)=0;

    /**
    * Set the callback function to be called (repeatedly) during saving. Set to null for no callback.
    * \return   Zero on success, a negative error code on failure.
    */
    virtual int setSavingUpdateCallbackFunction(SavingCallbackFunction* callback_function)=0;

    /**
    * Set the metadata object to be sent to the callback function to call during saving.
    * \return   Zero on success, a negative error code on failure.
    */
    virtual int setSavingUpdateCallbackMetadata(void* callback_metadata)=0;

    /**
    * Set the callback function to be called when saving finishes. Set to null for no callback.
    * \return   Zero on success, a negative error code on failure.
    */
    virtual int setSavingFinishCallbackFunction(SavingCallbackFunction* callback_function)=0;

    /**
    * Set the metadata object to be sent to the callback function to call when saving finishes.
    * \return   Zero on success, a negative error code on failure.
    */
    virtual int setSavingFinishCallbackMetadata(void* callback_metadata)=0;

    /**
    * Whether the Neural Network is currently being saved to file or buffer.
    * \return   True if the AI is currently being saved, false if not.
    */
    virtual bool isSaving()=0;

    /**
    * Cancel a currently running saving process.
    * \return   Zero on success, a negative error code on failure.
    */
    virtual int cancelSaving()=0;

    /**
    * Load the neural network and recorded training data from file, asynchronously.
    * The function will return immediately, while the loading process will continue in the background.
    * Use isLoading() to check if the loading process is still ongoing.
    * You can use setLoadingCallback() to receive a function call when loading finishes.
    * \param    path            The file path from which to load the AI and recorded data.
    * \param    createMetadata  [OPTIONAL] The function which can parse the metadata which was stored with the gestures.
    * \return                   Zero on success, a negative error code on failure. Note that this only relates to *starting* the loading process.
    */
    virtual int loadFromFileAsync(const char* path, MetadataCreatorFunction* createMetadata=0)=0;

    /**
    * Load the neural network and recorded training data buffer, asynchronously.
    * The function will return immediately, while the loading process will continue in the background.
    * Use isLoading() to check if the loading process is still ongoing.
    * You can use setLoadingCallback() to receive a function call when loading finishes.
    * \param    buffer          Memory buffer from which to load the AI and recorded data.
    * \param    buffer_size     Size of the memory buffer in byte.
    * \param    createMetadata  [OPTIONAL] The function which can parse the metadata which was stored with the gestures.
    * \return                   Zero on success, a negative error code on failure. Note that this only relates to *starting* the loading process.
    */
    virtual int loadFromBufferAsync(const char* buffer, int buffer_size, MetadataCreatorFunction* createMetadata=0)=0;

    /**
    * Set the callback function to be called (repeatedly) during loading. Set to null for no callback.
    * \return   Zero on success, a negative error code on failure.
    */
    virtual int setLoadingUpdateCallbackFunction(LoadingCallbackFunction* callback_function)=0;

    /**
    * Set the metadata object to be sent to the callback function to call during loading.
    * \return   Zero on success, a negative error code on failure.
    */
    virtual int setLoadingUpdateCallbackMetadata(void* callback_metadata)=0;

    /**
    * Set the callback function to be called when loading finishes. Set to null for no callback.
    * \return   Zero on success, a negative error code on failure.
    */
    virtual int setLoadingFinishCallbackFunction(LoadingCallbackFunction* callback_function)=0;

    /**
    * Set the metadata object to be sent to the callback function to call when loading finishes.
    * \return   Zero on success, a negative error code on failure.
    */
    virtual int setLoadingFinishCallbackMetadata(void* callback_metadata)=0;

    /**
    * Whether the Neural Network is currently loading from a file or buffer.
    * \return   True if the AI is currently loading, false if not.
    */
    virtual bool isLoading()=0;

    /**
    * Cancel a currently running loading process.
    * \return   Zero on success, a negative error code on failure.
    */
    virtual int cancelLoading()=0;

    /**
    * Import recorded gesture samples from another gesture recognition object.
    * \param   from_gro            The GestureRecognitionObject from where to import recorded gesture samples.
    * \param   from_gesture_index  The index (ID) of the gesture (on the other GRO) to import.
    * \param   into_gesture_index  The index (ID) of the gesture (on this object) to which the samples should be added.
    * \return                      Zero on success, a negative error code on failure.
    */
    virtual int importGestureSamples(const IGestureRecognition* from_gro, int from_gesture_index, int into_gesture_index)=0;

    /**
    * Import recorded gesture samples from another gesture recognition object, merging gestures by name.
    * Gestures with names which are not in the list of gestures yet will be appended.
    * \param   from_gro        The GestureRecognitionObject from where to import gestures.
    * \return                  Zero on success, a negative error code on failure.
    */
    virtual int importGestures(const IGestureRecognition* from_gro)=0;

    /**
    * Start train the Neural Network based on the the currently collected data.
    * \return                  Zero on success, a negative error code on failure.
    */
    virtual int startTraining()=0;

    /**
    * Whether the Neural Network is currently training.
    * \return   True if the AI is currently training, false if not.
    */
    virtual bool isTraining()=0;

    /**
    * Stop the training process (last best result will be used).
    * \return                  Zero on success, a negative error code on failure.
    */
    virtual int stopTraining()=0;

    /**
    * Maximum training time in seconds.
    */
    unsigned long maxTrainingTime;

    /**
    * Optional callback function to be called during training.
    */
    TrainingCallbackFunction* trainingUpdateCallback;

    /**
    * Optional metadata to be provided with the callback during training.
    */
    void* trainingUpdateCallbackMetadata;

    /**
    * Optional callback function to be called when training is finished.
    */
    TrainingCallbackFunction* trainingFinishCallback;

    /**
    * Optional metadata to be provided with the callback when training is finished.
    */
    void* trainingFinishCallbackMetadata;

    /**
    * Different coordinate system origins from which to interpret gestures.
    */
    enum FrameOfReference {
        Head  = GESTURERECOGNITION_FRAMEOFREFERENCE_HEAD  //!< Identifier for interpreting gestures as seen from the headset/HMD (user point of view). 
        ,
        World = GESTURERECOGNITION_FRAMEOFREFERENCE_WORLD //!< Identifier for interpreting gestures as seen from world origin (global coordinates).
    };

    /**
    * Different orderings of rotation for (Euler) rotation angles.
    */
    enum RotationOrder {
        XYZ = GESTURERECOGNITION_ROTATIONORDER_XYZ //!< Identifier for x->y->z order of (Euler) rotation angles.
        ,
        XZY = GESTURERECOGNITION_ROTATIONORDER_XZY //!< Identifier for x->y->z order of (Euler) rotation angles.
        ,
        YXZ = GESTURERECOGNITION_ROTATIONORDER_YXZ //!< Identifier for x->y->z order of (Euler) rotation angles.
        ,
        YZX = GESTURERECOGNITION_ROTATIONORDER_YZX //!< Identifier for x->y->z order of (Euler) rotation angles.
        ,
        ZXY = GESTURERECOGNITION_ROTATIONORDER_ZXY //!< Identifier for x->y->z order of (Euler) rotation angles.
        ,
        ZYX = GESTURERECOGNITION_ROTATIONORDER_ZYX //!< Identifier for x->y->z order of (Euler) rotation angles.
    };

    /**
    * Whether the rotation of the users head should be considered when recording and performing gestures.
    */
    struct RotationalFrameOfReference {
        FrameOfReference x = Head; //!< Whether the vertical rotation of the users head (commonly called "pitch", looking up or down) should be considered when recording and performing gestures.
        FrameOfReference y = Head; //!< Whether the horizontal rotation of the users head (commonly called "pan" or "yaw", looking left or right) should be considered when recording and performing gestures.
        FrameOfReference z = Head; //!< Whether the tilting rotation of the users head (also called "roll" or "bank", tilting the head to the site without changing the view direction) should be considered when recording and performing gestures.
        RotationOrder rotationOrder = RotationOrder::YXZ; //!< In which order the x, y, and z rotation are to be interpreted.
    } rotationalFrameOfReference; //!< Whether the rotation of the users head should be considered when recording and performing gestures.

    /**
    * Set a Metadata object of this IGestureRecognition object.
    * \param    metadata        The new metadata object.
    * \return                   Zero on success, a negative error code on failure.
    */
    virtual int setMetadata(Metadata* metadata)=0;

    /**
    * Get the current metadata object of this IGestureRecognition object.
    * \return                   The metadata registered for this IGestureRecognition object, null if no Metadata object was set.
    */
    virtual Metadata* getMetadata()=0;

    /**
    * Run internal tests to check for code correctness and data consistency.
    */
    virtual int runTests()=0;

    /**
    * Set callback function to receive debugging information.
    */
    static void setDebugCallbackFunction(DebugCallbackFunction* dbf);

    /**
    * Set metadata to receive with debugging output callback function.
    */
    static void setDebugCallbackMetadata(void* metadata);

    /**
    * Get the version of this library as human-readable string.
    * \return   A null-terminated string describing the version of MiVRy.
    */
    static const char* getVersionString();

    /**
    * Get the length of the version string.
    * \return   The number of characters that make up the version string.
    */
    static int getVersionStringLength();

    /**
    * Copy the version string into a buffer.
    * \param    buf             [OUT] The string buffer to which to write the gesture name.
    * \param    buflen          The length of the buf string buffer.
    * \return                   The number of characters written to the buffer, zero on failure.
    */
    static int copyVersionString(char* buf, int buflen);

    /**
    * Global function to create DefaultMetadata objects.
    * \return A DefaultMetadata object.
    */
    static DefaultMetadata* defaultMetadataCreatorFunction();

    /**
    * Global function to retrieve a pointer to the createDefaultMetadataCreatorFunction.
    * \return A function pointer to the global createDefaultMetadataCreatorFunction.
    */
    static MetadataCreatorFunction* getDefaultMetadataCreatorFunction();
};

#endif // #ifdef __cplusplus

#endif //__GESTURE_RECOGNITION_H