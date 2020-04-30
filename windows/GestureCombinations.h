/*
 * GestureCombinations - VR gesture recognition library for multi-part gesture combinations.
 * Copyright (c) 2019 MARUI-PlugIn (inc.)
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
 * 
 * --------------------------------------------------------------------------
 * 
 * # HOW TO USE:
 * 
 * (1) Place the GestureRecognition.h and GestureCombinations.h
 * as well as the .dll and/or .lib files appropriate for your platform
 * in your project. 
 * 
 * 
 * (2) Create a new Gesture recognition object and register the gestures that you want to identify later.
 * (In this example, we use gesture part "0" to mean "left hand" and gesture part "1" to mean right hand,
 * but it could also be two sequential gesture parts performed with the same hand.)
 * <code>
 * IGestureCombinations* gc = new GestureCombinations(2);
 * int myFirstCombo = gc->createGestureCombination("wave your hands");
 * int mySecondCombo = gc->createGestureCombination("play air-guitar");
 * </code>
 * Also, create the individual gestures that each combo will consist.
 * <code>
 * int myFirstCombo_leftHandGesture = gc->createGesture(0, "Wave left hand");
 * int myFirstCombo_rightHandGesture = gc->createGesture(1, "Wave right hand");
 * int mySecondCombo_leftHandGesture = gc->createGesture(0, "Hold guitar neck");
 * int mySecondCombo_rightHandGesture = gc->createGesture(1, "Hit strings");
 * </code>
 * Then set the Gesture Combinations to be the connection of those gestures.
 * <code>
 * gc->setCombinationPartGesture(myFirstCombo, 0, myFirstCombo_leftHandGesture);
 * gc->setCombinationPartGesture(myFirstCombo, 1, myFirstCombo_rightHandGesture);
 * gc->setCombinationPartGesture(mySecondCombo, 0, mySecondCombo_leftHandGesture);
 * gc->setCombinationPartGesture(mySecondCombo, 1, mySecondCombo_rightHandGesture);
 * </code>
 * 
 * 
 * 
 * (3) Record a number of samples for each gesture by calling startStroke(), contdStroke() and endStroke()
 * for your registered gestures, each time inputting the headset and controller transformation.
 * <code>
 * double hmd_p[3] = <your headset or reference coordinate system position>;
 * double hmd_q[4] = <your headset or reference coordinate system rotation as x,y,z,w quaternions>;
 * gc->startStroke(0, hmd_p, hmd_q, myFirstCombo_leftHandGesture);
 * gc->startStroke(1, hmd_p, hmd_q, myFirstCombo_rightHandGesture);
 * 
 * // repeat the following while performing the gesture with your controller:
 * double p_left[3] = <your left VR controller position in space>;
 * double q_left[4] = <your left VR controller rotation in space as x,y,z,w quaternions)>;
 * gc->contdStrokeQ(0, p_left, q_left);
 * double p_right[3] = <your right VR controller position in space>;
 * double q_right[4] = <your right VR controller rotation in space as x,y,z,w quaternions)>;
 * gc->contdStrokeQ(0, p_right, q_right);
 * // ^ repead while performing the gesture with your controller.
 * 
 * gc->endStroke(0);
 * gc->endStroke(1);
 * </code>
 * Repeat this multiple times for each gesture you want to identify.
 * We recommend recording at least 20 samples for each gesture,
 * and have different people perform each gesture.
 * 
 * 
 * (4) Start the training process by calling startTraining().
 * You can optionally register callback functions to receive updates on the learning progress
 * by calling setTrainingUpdateCallback() and setTrainingFinishCallback().
 * <code>
 * gc->setMaxTrainingTime(60); // Set training time to 60 seconds.
 * gc->startTraining();
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
 * gc->startStroke(0, hmd_p, hmd_q);
 * gc->startStroke(1, hmd_p, hmd_q);
 * 
 * // repeat the following while performing the gesture with your controller:
 * double p_left[3] = <your left VR controller position in space>;
 * double q_left[4] = <your left VR controller rotation in space as x,y,z,w quaternions)>;
 * gc->contdStroke(0, p_left, q_left);
 * double p_right[3] = <your right VR controller position in space>;
 * double q_right[4] = <your right VR controller rotation in space as x,y,z,w quaternions)>;
 * gc->contdStroke(1, p_right, q_right);
 * // ^ repead while performing the gesture with your controller.
 * 
 * gc->endStroke(0);
 * gc->endStroke(1);
 * 
 * int identifiedGestureCombo = gc->identifyGestureCombination();
 * if (identifiedGestureCombo == myFirstCombo) {
 *     // ...
 * }
 * </code>
 * 
 * 
 * (6) Now you can save and load the artificial intelligence.
 * <code>
 * gc->saveToFile("C:/myGestureCombos.dat");
 * // ...
 * gc->loadFromFile("C:/myGestureCombos.dat");
 * </code>
 * 
 */
#ifndef __GESTURE_COMBINATIONS
#define __GESTURE_COMBINATIONS

#include "GestureRecognition.h"

#ifdef __cplusplus
extern "C" {
#endif
    GESTURERECOGNITION_LIBEXPORT void* GestureCombinations_create(int number_of_parts); //!< Create new instance.
    GESTURERECOGNITION_LIBEXPORT void  GestureCombinations_delete(void* gco); //!< Delete instance.

    GESTURERECOGNITION_LIBEXPORT int   GestureCombinations_numberOfParts(void* gco); //!< Get the number of subgestures / parts / hands used by this multi-gesture object.

    GESTURERECOGNITION_LIBEXPORT int   GestureCombinations_startStroke(void* gco, int part, const double hmd_p[3], const double hmd_q[4], int record_as_sample); //!< Start new stroke.
    GESTURERECOGNITION_LIBEXPORT int   GestureCombinations_startStrokeM(void* gco, int part, const double hmd[4][4], int record_as_sample); //!< Start new stroke.
    GESTURERECOGNITION_LIBEXPORT int   GestureCombinations_contdStroke(void* gco, int part, const double p[3]); //!< Continue stroke data input - positional data only.
    GESTURERECOGNITION_LIBEXPORT int   GestureCombinations_contdStrokeQ(void* gco, int part, const double p[3], const double q[4]); //!< Continue stroke data input - rotation as quaternion.
    GESTURERECOGNITION_LIBEXPORT int   GestureCombinations_contdStrokeE(void* gco, int part, const double p[3], const double r[3]); //!< Continue stroke data input - rotation as Euler angles (rad).
    GESTURERECOGNITION_LIBEXPORT int   GestureCombinations_contdStrokeM(void* gco, int part, const double m[4][4]); //!< Continue stroke data input - position and rotation as transformation matrix.
    GESTURERECOGNITION_LIBEXPORT int   GestureCombinations_endStroke(void* gco, int part, double pos[3], double* scale, double dir0[3], double dir1[3], double dir2[3]); //!< End the stroke and identify the gesture.
    GESTURERECOGNITION_LIBEXPORT int   GestureCombinations_cancelStroke(void* gco, int part); //!< Cancel a started stroke.
    GESTURERECOGNITION_LIBEXPORT int   GestureCombinations_identifyGestureCombination(void* gco, double* similarity); //!< Return the most likely gesture candidate for the previous multi-gesture.
    GESTURERECOGNITION_LIBEXPORT int   GestureCombinations_contdIdentify(void* gco, const double hmd_p[3], const double hmd_q[4], double* similarity=0); //!< Continuous gesture identification.
    GESTURERECOGNITION_LIBEXPORT int   GestureCombinations_contdRecord(void* gco, const double hmd_p[3], const double hmd_q[4]); //!< Continuous gesture recording.
    GESTURERECOGNITION_LIBEXPORT int   GestureCombinations_getContdIdentificationPeriod(void* gco, int part); //!< Get time frame for continuous gesture identification in milliseconds.
    GESTURERECOGNITION_LIBEXPORT int   GestureCombinations_setContdIdentificationPeriod(void* gco, int part, int ms); //!< Set time frame for continuous gesture identification in milliseconds.
    GESTURERECOGNITION_LIBEXPORT int   GestureCombinations_getContdIdentificationSmoothing(void* gco, int part); //!< Get smoothing for continuous gesture identification in number of samples.
    GESTURERECOGNITION_LIBEXPORT int   GestureCombinations_setContdIdentificationSmoothing(void* gco, int part, int samples); //!< Set smoothing for continuous gesture identification in number of samples.

    GESTURERECOGNITION_LIBEXPORT int  GestureCombinations_numberOfGestures(void* gco, int part); //!< Get the number of gestures currently recorded in the system.
    GESTURERECOGNITION_LIBEXPORT int  GestureCombinations_deleteGesture(void* gco, int part, int index); //!< Delete the recorded gesture with the specified index.
    GESTURERECOGNITION_LIBEXPORT int  GestureCombinations_deleteAllGestures(void* gco, int part); //!< Delete recorded gestures.
    GESTURERECOGNITION_LIBEXPORT int  GestureCombinations_createGesture(void* gco, int part, const char* name, void* metadata); //!< Create new gesture.
    GESTURERECOGNITION_LIBEXPORT int  GestureCombinations_copyGesture(void* gco, int from_part, int gesture_index, int to_part, int mirror_x, int mirror_y, int mirror_z); //!< Copy gesture from one part/side to another.
    GESTURERECOGNITION_LIBEXPORT double GestureCombinations_gestureRecognitionScore(void* gco, int part); //!< Get the gesture recognition score of the current neural network (0~1).
        
    GESTURERECOGNITION_LIBEXPORT const char* GestureCombinations_getGestureName(void* gco, int part, int index); //!< Get the name of a registered gesture.
    GESTURERECOGNITION_LIBEXPORT int         GestureCombinations_getGestureNameLength(void* gco, int part, int index); //!< Get the length of the name of a registered gesture.
    GESTURERECOGNITION_LIBEXPORT int         GestureCombinations_copyGestureName(void* gco, int part, int index, char* buf, int buflen); //!< Copy the name of a registered gesture to a buffer.

    GESTURERECOGNITION_LIBEXPORT void*       GestureCombinations_getGestureMetadata(void* gco, int part, int index); //!< Get the command of a registered gesture.
    GESTURERECOGNITION_LIBEXPORT int         GestureCombinations_getGestureNumberOfSamples(void* gco, int part, int index); //!< Get the number of recorded samples of a registered gesture.
    GESTURERECOGNITION_LIBEXPORT int         GestureCombinations_getGestureSampleLength(void* gco, int part, int gesture_index, int sample_index, int processed); //!< Get the number of data points a sample has.
    GESTURERECOGNITION_LIBEXPORT int         GestureCombinations_getGestureSampleStroke(void* gco, int part, int gesture_index, int sample_index, int processed, double hmd_p[3], double hmd_q[4], double p[][3], double q[][4], int stroke_buf_size); //!< Retrieve a sample stroke.
    GESTURERECOGNITION_LIBEXPORT int         GestureCombinations_deleteGestureSample(void* gco, int part, int gesture_index, int sample_index); //!< Delete a gesture sample recording from the set.
    GESTURERECOGNITION_LIBEXPORT int         GestureCombinations_deleteAllGestureSamples(void* gco, int part, int gesture_index); //!< Delete all gesture sample recordings from the set.

    GESTURERECOGNITION_LIBEXPORT int GestureCombinations_setGestureName(void* gco, int part, int index, const char* name); //!< Set the name of a registered gesture.
    GESTURERECOGNITION_LIBEXPORT int GestureCombinations_setGestureMetadata(void* gco, int part, int index, void* metadata); //!< Set the command of a registered gesture.

    GESTURERECOGNITION_LIBEXPORT int GestureCombinations_saveToFile(void* gco, const char* path); //!< Save the neural network and recorded training data to file.
    GESTURERECOGNITION_LIBEXPORT int GestureCombinations_loadFromFile(void* gco, const char* path, MetadataCreatorFunction* createMetadata); //!< Load the neural network and recorded training data from file.
    GESTURERECOGNITION_LIBEXPORT int GestureCombinations_loadFromBuffer(void* gco, const char* buffer, MetadataCreatorFunction* createMetadata); //!< Load the neural network and recorded training data buffer.
    GESTURERECOGNITION_LIBEXPORT int GestureCombinations_saveGestureToFile(void* gco, int part, const char* path); //!< Save the neural network and recorded training data to file.
    GESTURERECOGNITION_LIBEXPORT int GestureCombinations_loadGestureFromFile(void* gco, int part, const char* path, MetadataCreatorFunction* createMetadata); //!< Load the neural network and recorded training data from file.
    GESTURERECOGNITION_LIBEXPORT int GestureCombinations_loadGestureFromBuffer(void* gco, int part, const char* buffer, MetadataCreatorFunction* createMetadata); //!< Load the neural network and recorded training data buffer.

    GESTURERECOGNITION_LIBEXPORT int GestureCombinations_numberOfGestureCombinations(void* gco); //!< Get the number of gestures currently recorded in the i's system.
    GESTURERECOGNITION_LIBEXPORT int GestureCombinations_deleteGestureCombination(void* gco, int index); //!< Delete the recorded gesture with the specified index.
    GESTURERECOGNITION_LIBEXPORT int GestureCombinations_deleteAllGestureCombinations(void* gco); //!< Delete recorded gestures.
    GESTURERECOGNITION_LIBEXPORT int GestureCombinations_createGestureCombination(void* gco, const char*  name); //!< Create new gesture.
    GESTURERECOGNITION_LIBEXPORT const char* GestureCombinations_getGestureCombinationName(void* gco, int index); //!< Get the name of a registered multi-gesture.
    GESTURERECOGNITION_LIBEXPORT int         GestureCombinations_getGestureCombinationNameLength(void* gco, int index); //!< Get the length of the name of a registered multi-gesture.
    GESTURERECOGNITION_LIBEXPORT int         GestureCombinations_copyGestureCombinationName(void* gco, int index, char* buf, int buflen); //!< Copy the name of a registered multi-gesture to a buffer.
    GESTURERECOGNITION_LIBEXPORT int         GestureCombinations_setGestureCombinationName(void* gco, int index, const char* name); //!< Set the name of a registered multi-gesture.

    GESTURERECOGNITION_LIBEXPORT int GestureCombinations_setCombinationPartGesture(void* gco, int multigesture_index, int part, int gesture_index); //!< Set which gesture this multi-gesture expects for step i.
    GESTURERECOGNITION_LIBEXPORT int GestureCombinations_getCombinationPartGesture(void* gco, int multigesture_index, int part); //!< Get which gesture this multi-gesture expects for step i.

    GESTURERECOGNITION_LIBEXPORT int  GestureCombinations_startTraining(void* gco); //!< Start train the Neural Network based on the the currently collected data.
    GESTURERECOGNITION_LIBEXPORT int  GestureCombinations_isTraining(void* gco); //!< Whether the Neural Network is currently training.
    GESTURERECOGNITION_LIBEXPORT void GestureCombinations_stopTraining(void* gco); //!< Stop the training process (last best result will be used).
    
    GESTURERECOGNITION_LIBEXPORT double GestureCombinations_recognitionScore(void* gco); //!< Get the gesture recognition score of the current neural networks (0~1).

    GESTURERECOGNITION_LIBEXPORT int   GestureCombinations_getMaxTrainingTime(void* gco); //!< Get maximum training time in seconds.
    GESTURERECOGNITION_LIBEXPORT void  GestureCombinations_setMaxTrainingTime(void* gco, int t); //!< Set maximum training time in seconds.

    GESTURERECOGNITION_LIBEXPORT void GestureCombinations_setTrainingUpdateCallback(void* gco, TrainingCallbackFunction* cbf); //!< Set callback function to be called during training.
    GESTURERECOGNITION_LIBEXPORT void GestureCombinations_setTrainingFinishCallback(void* gco, TrainingCallbackFunction* cbf); //!< Set callback function to be called when training is finished.
    
    GESTURERECOGNITION_LIBEXPORT void GestureCombinations_setTrainingUpdateCallbackMetadata(void* gco, void* metadata); //!< Set callback function to be called during training.
    GESTURERECOGNITION_LIBEXPORT void GestureCombinations_setTrainingFinishCallbackMetadata(void* gco, void* metadata); //!< Set callback function to be called when training is finished.
    
    GESTURERECOGNITION_LIBEXPORT int   GestureCombinations_getIgnoreHeadRotationX(void* gco); //!< Get whether the horizontal rotation of the users head (commonly called "pan" or "yaw", looking left or right) should be considered when recording and performing gestures.
    GESTURERECOGNITION_LIBEXPORT void  GestureCombinations_setIgnoreHeadRotationX(void* gco, int on_off); //!< Set whether the horizontal rotation of the users head (commonly called "pan" or "yaw", looking left or right) should be considered when recording and performing gestures.
    GESTURERECOGNITION_LIBEXPORT int   GestureCombinations_getIgnoreHeadRotationY(void* gco); //!< Get whether the vertical rotation of the users head (commonly called "pitch", looking up or down) should be considered when recording and performing gestures.
    GESTURERECOGNITION_LIBEXPORT void  GestureCombinations_setIgnoreHeadRotationY(void* gco, int on_off); //!< Set whether the vertical rotation of the users head (commonly called "pitch", looking up or down) should be considered when recording and performing gestures.
    GESTURERECOGNITION_LIBEXPORT int   GestureCombinations_getIgnoreHeadRotationZ(void* gco); //!< Get whether the tilting rotation of the users head (also called "roll" or "bank", tilting the head to the site without changing the view direction) should be considered when recording and performing gestures.
    GESTURERECOGNITION_LIBEXPORT void  GestureCombinations_setIgnoreHeadRotationZ(void* gco, int on_off); //!< Set whether the tilting rotation of the users head (also called "roll" or "bank", tilting the head to the site without changing the view direction) should be considered when recording and performing gestures.

#ifdef __cplusplus
}

class _GestureCombinations
{
public:
    static _GestureCombinations* create(int number_of_parts); //!< Create new GestureCombinations object.
    virtual ~_GestureCombinations(); //!< Destructor.

    virtual int numberOfParts()=0; //!< Get the number of subgestures / parts / hands used by this multi-gesture object.

    virtual int startStroke(int part, const double hmd[4][4], int record_as_sample=-1)=0; //!< Start new stroke.
    virtual int startStroke(int part, const double hmd_p[3], const double hmd_q[4], int record_as_sample=-1)=0; //!< Start new stroke.
    virtual int contdStroke(int part, const double p[3])=0; //!< Continue stroke data input.
    virtual int contdStrokeQ(int part, const double p[3], const double q[4])=0; //!< Continue stroke data input (with quaternion rotation).
    virtual int contdStrokeE(int part, const double p[3], const double r[3])=0; //!< Continue stroke data input (with Euler angles rotation in radians).
    virtual int contdStrokeM(int part, const double m[4][4])=0; //!< Continue stroke data input (with transformation matrix).
    virtual int endStroke(int part, double pos[3]=0, double* scale=0, double dir0[3]=0, double dir1[3]=0, double dir2[3]=0)=0; //!< End the stroke and identify the gesture.
    virtual int cancelStroke(int part)=0; //!< Cancel a started stroke.

    virtual int identifyGestureCombination(double* similarity=0)=0; //!< Return the most likely gesture candidate for the previous multi-gesture.

    virtual int contdIdentify(const double hmd_p[3], const double hmd_q[4], double* similarity=0)=0; //!< Continuous gesture identification.
    virtual int contdRecord(const double hmd_p[3], const double hmd_q[4])=0; //!< Continuous gesture recording.
    virtual int getContdIdentificationPeriod(int part)=0; //!< Get time frame for continuous gesture identification in milliseconds.
    virtual int setContdIdentificationPeriod(int part, int ms)=0; //!< Set time frame for continuous gesture identification in milliseconds.
    virtual int getContdIdentificationSmoothing(int part)=0; //!< Get smoothing for continuous gesture identification in number of samples.
    virtual int setContdIdentificationSmoothing(int part, int samples)=0; //!< Set smoothing for continuous gesture identification in number of samples.

    // Access to the respective gesture recognition objects:
    virtual int  numberOfGestures(int part)=0; //!< Get the number of gestures currently recorded in the i's system.
    virtual bool deleteGesture(int part, int index)=0; //!< Delete the recorded gesture with the specified index.
    virtual bool deleteAllGestures(int part)=0; //!< Delete recorded gestures.
    virtual int  createGesture(int part, const char*  name, _GestureRecognition::Metadata* metadata=0)=0; //!< Create new gesture.
    virtual int  copyGesture(int from_part, int gesture_index, int to_part, bool mirror_x=false, bool mirror_y=false, bool mirror_z=false)=0; //!< Copy gesture from one part/side to another.
    virtual double gestureRecognitionScore(int part)=0; //!< Get the gesture recognition score of the current neural network (0~1).
        
    virtual const char* getGestureName(int part, int index)=0; //!< Get the name of a registered gesture.
    virtual int         getGestureNameLength(int part, int index)=0; //!< Get the length of the name of a registered gesture.
    virtual int         copyGestureName(int part, int index, char* buf, int buflen)=0; //!< Copy the name of a registered gesture to a buffer.
    virtual _GestureRecognition::Metadata* getGestureMetadata(int part, int index)=0; //!< Get the command of a registered gesture.
    virtual int         getGestureNumberOfSamples(int part, int index)=0; //!< Get the number of recorded samples of a registered gesture.
    virtual int         getGestureSampleLength(int part, int gesture_index, int sample_index, bool processed)=0; //!< Get the number of data points a sample has.
    virtual int         getGestureSampleStroke(int part, int gesture_index, int sample_index, bool processed, double hmd_p[3], double hmd_q[4], double p[][3], double q[][4], int stroke_buf_size)=0; //!< Retrieve a sample stroke.
    virtual bool        deleteGestureSample(int part, int gesture_index, int sample_index)=0; //!< Delete a gesture sample recording from the set.
    virtual bool        deleteAllGestureSamples(int part, int gesture_index)=0; //!< Delete all gesture sample recordings from the set.

    virtual bool setGestureName(int part, int index, const char* name)=0; //!< Set the name of a registered gesture.
    virtual bool setGestureMetadata(int part, int index, _GestureRecognition::Metadata* metadata)=0; //!< Set the command of a registered gesture.

    virtual bool saveToFile(const char* path)=0; //!< Save the neural network and recorded training data to file.
    virtual bool loadFromFile(const char* path, _GestureRecognition::MetadataCreatorFunction* createMetadata=0)=0; //!< Load the neural network and recorded training data from file.
    virtual bool loadFromBuffer(const char* buffer, _GestureRecognition::MetadataCreatorFunction* createMetadata=0)=0; //!< Load the neural network and recorded training data buffer.
    virtual bool saveGestureToFile(int part, const char* path)=0; //!< Save the neural network and recorded training data to file.
    virtual bool loadGestureFromFile(int part, const char* path, _GestureRecognition::MetadataCreatorFunction* createMetadata=0)=0; //!< Load the neural network and recorded training data from file.
    virtual bool loadGestureFromBuffer(int part, const char* buffer, _GestureRecognition::MetadataCreatorFunction* createMetadata=0)=0; //!< Load the neural network and recorded training data buffer.

    // Functions for handling gesture combinations
    virtual int  numberOfGestureCombinations()=0; //!< Get the number of gestures currently recorded in the i's system.
    virtual bool deleteGestureCombination(int index)=0; //!< Delete the recorded gesture with the specified index.
    virtual bool deleteAllGestureCombinations()=0; //!< Delete recorded gestures.
    virtual int  createGestureCombination(const char*  name)=0; //!< Create new gesture.
    virtual bool setCombinationPartGesture(int combination_index, int part, int gesture_index)=0; //!< Set which gesture this multi-gesture expects for step i.
    virtual int  getCombinationPartGesture(int combination_index, int part)=0; //!< Get which gesture this multi-gesture expects for step i.
    virtual const char* getGestureCombinationName(int index)=0; //!< Get the name of a registered multi-gesture.
    virtual int         getGestureCombinationNameLength(int index)=0; //!< Get the length of the name of a registered multi-gesture.
    virtual int         copyGestureCombinationName(int index, char* buf, int buflen)=0; //!< Copy the name of a registered multi-gesture to a buffer.
    virtual bool        setGestureCombinationName(int index, const char* name)=0; //!< Set the name of a registered multi-gesture.

    virtual bool startTraining()=0; //!< Start train the Neural Network based on the the currently collected data.
    virtual bool isTraining()=0; //!< Whether the Neural Network is currently training.
    virtual void stopTraining()=0; //!< Stop the training process (last best result will be used).

    virtual double recognitionScore()=0; //!< Get the gesture recognition score of the current neural network (0~1).
    
    unsigned long maxTrainingTime; //!< Maximum training time in seconds.
    typedef void GESTURERECOGNITION_CALLCONV TrainingCallbackFunction(double performance, void* metadata); //!< Function pointer to an optional callback function to e called during training.
    TrainingCallbackFunction* trainingUpdateCallback; //!< Optional callback function to be called during training.
    void*                     trainingUpdateCallbackMetadata; //!< Optional metadata to be provided with the callback during training.
    TrainingCallbackFunction* trainingFinishCallback; //!< Optional callback function to be called when training is finished.
    void*                     trainingFinishCallbackMetadata; //!< Optional metadata to be provided with the callback when training is finished.

    virtual unsigned long getMaxTrainingTime()=0; //!< Set the maximum time for training in seconds.
    virtual void setMaxTrainingTime(unsigned long t)=0; //!< Get the current maximum training time in seconds

    /// Whether the rotation of the users head should be considered when recording and performing gestures.
    _GestureRecognition::IgnoreHeadRotation ignoreHeadRotation; //!< Whether the rotation of the users head should be considered when recording and performing gestures.
};

typedef _GestureCombinations IGestureCombinations;
#endif // #ifdef __cplusplus

#endif //__GESTURE_COMBINATIONS