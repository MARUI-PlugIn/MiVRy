/*
 * GestureCombinations - VR gesture recognition library for multi-part gesture combinations.
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
    GESTURERECOGNITION_LIBEXPORT int   GestureCombinations_isStrokeStarted(void* gco, int part); //!< Query whether a gesture performance (gesture motion, stroke) was started and is currently ongoing.
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
    GESTURERECOGNITION_LIBEXPORT int  GestureCombinations_copyGesture(void* gco, int from_part, int from_gesture_index, int to_part, int to_gesture_index, int mirror_x, int mirror_y, int mirror_z); //!< Copy gesture from one part/side to another.
    GESTURERECOGNITION_LIBEXPORT double GestureCombinations_gestureRecognitionScore(void* gco, int part); //!< Get the gesture recognition score of the current neural network (0~1).
        
    GESTURERECOGNITION_LIBEXPORT const char* GestureCombinations_getGestureName(void* gco, int part, int index); //!< Get the name of a registered gesture.
    GESTURERECOGNITION_LIBEXPORT int         GestureCombinations_getGestureNameLength(void* gco, int part, int index); //!< Get the length of the name of a registered gesture.
    GESTURERECOGNITION_LIBEXPORT int         GestureCombinations_copyGestureName(void* gco, int part, int index, char* buf, int buflen); //!< Copy the name of a registered gesture to a buffer.

    GESTURERECOGNITION_LIBEXPORT void*       GestureCombinations_getGestureMetadata(void* gco, int part, int index); //!< Get the command of a registered gesture.
    GESTURERECOGNITION_LIBEXPORT int         GestureCombinations_getGestureNumberOfSamples(void* gco, int part, int index); //!< Get the number of recorded samples of a registered gesture.
    GESTURERECOGNITION_LIBEXPORT int         GestureCombinations_getGestureSampleLength(void* gco, int part, int gesture_index, int sample_index, int processed); //!< Get the number of data points a sample has.
    GESTURERECOGNITION_LIBEXPORT int         GestureCombinations_getGestureSampleStroke(void* gco, int part, int gesture_index, int sample_index, int processed, double hmd_p[3], double hmd_q[4], double p[][3], double q[][4], int stroke_buf_size); //!< Retrieve a sample stroke.
    GESTURERECOGNITION_LIBEXPORT int         GestureCombinations_getGestureMeanLength(void* gco, int part, int gesture_index); //!< Get the number of samples of the gesture mean (average over samples).
    GESTURERECOGNITION_LIBEXPORT int         GestureCombinations_getGestureMeanStroke(void* gco, int part, int gesture_index, double p[][3], double q[][4], int stroke_buf_size, double hmd_p[3], double hmd_q[4], double* scale); //!< Retrieve a gesture mean (average over samples).
    GESTURERECOGNITION_LIBEXPORT int         GestureCombinations_deleteGestureSample(void* gco, int part, int gesture_index, int sample_index); //!< Delete a gesture sample recording from the set.
    GESTURERECOGNITION_LIBEXPORT int         GestureCombinations_deleteAllGestureSamples(void* gco, int part, int gesture_index); //!< Delete all gesture sample recordings from the set.

    GESTURERECOGNITION_LIBEXPORT int GestureCombinations_setGestureName(void* gco, int part, int index, const char* name); //!< Set the name of a registered gesture.
    GESTURERECOGNITION_LIBEXPORT int GestureCombinations_setGestureMetadata(void* gco, int part, int index, void* metadata); //!< Set the command of a registered gesture.

    GESTURERECOGNITION_LIBEXPORT int GestureCombinations_saveToFile(void* gco, const char* path); //!< Save the neural network and recorded training data to file.
    GESTURERECOGNITION_LIBEXPORT int GestureCombinations_loadFromFile(void* gco, const char* path, MetadataCreatorFunction* createMetadata); //!< Load the neural network and recorded training data from file.
    GESTURERECOGNITION_LIBEXPORT int GestureCombinations_loadFromBuffer(void* gco, const char* buffer, int buffer_size, MetadataCreatorFunction* createMetadata); //!< Load the neural network and recorded training data buffer.
    GESTURERECOGNITION_LIBEXPORT int GestureCombinations_loadFromStream(void* gco, void* stream, MetadataCreatorFunction* createMetadata); //!< Load the neural network and recorded training data from std::istream.
    GESTURERECOGNITION_LIBEXPORT int GestureCombinations_importFromFile(void* gco, const char* path, MetadataCreatorFunction* createMetadata); //!< Import recorded training data from file.
    GESTURERECOGNITION_LIBEXPORT int GestureCombinations_importFromBuffer(void* gco, const char* buffer, int buffer_size, MetadataCreatorFunction* createMetadata); //!< Import recorded training data from buffer.
    GESTURERECOGNITION_LIBEXPORT int GestureCombinations_importFromStream(void* gco, void* stream, MetadataCreatorFunction* createMetadata); //!< Import recorded training data from std::istream.
    GESTURERECOGNITION_LIBEXPORT int GestureCombinations_saveGestureToFile(void* gco, int part, const char* path); //!< Save the neural network and recorded training data to file.
    GESTURERECOGNITION_LIBEXPORT int GestureCombinations_loadGestureFromFile(void* gco, int part, const char* path, MetadataCreatorFunction* createMetadata); //!< Load the neural network and recorded training data from file.
    GESTURERECOGNITION_LIBEXPORT int GestureCombinations_loadGestureFromBuffer(void* gco, int part, const char* buffer, int buffer_size, MetadataCreatorFunction* createMetadata); //!< Load the neural network and recorded training data buffer.

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
    
    GESTURERECOGNITION_LIBEXPORT int  GestureCombinations_getRotationalFrameOfReferenceX(void* gco); //!< Get wether gestures are interpreted as seen by the user or relative to the world, regarding their x-axis rotation (commonly: pitch, looking up or down).
    GESTURERECOGNITION_LIBEXPORT void GestureCombinations_setRotationalFrameOfReferenceX(void* gco, int i); //!< Set wether gestures are interpreted as seen by the user or relative to the world, regarding their x-axis rotation (commonly: pitch, looking up or down).
    GESTURERECOGNITION_LIBEXPORT int  GestureCombinations_getRotationalFrameOfReferenceY(void* gco); //!< Get wether gestures are interpreted as seen by the user or relative to the world, regarding their y-axis rotation (commonly: pan/yaw, looking left or right).
    GESTURERECOGNITION_LIBEXPORT void GestureCombinations_setRotationalFrameOfReferenceY(void* gco, int i); //!< Set wether gestures are interpreted as seen by the user or relative to the world, regarding their y-axis rotation (commonly: pan/yaw, looking left or right).
    GESTURERECOGNITION_LIBEXPORT int  GestureCombinations_getRotationalFrameOfReferenceZ(void* gco); //!< Get wether gestures are interpreted as seen by the user or relative to the world, regarding their z-axis rotation (commonly: roll, tilting the head).
    GESTURERECOGNITION_LIBEXPORT void GestureCombinations_setRotationalFrameOfReferenceZ(void* gco, int i); //!< Set wether gestures are interpreted as seen by the user or relative to the world, regarding their z-axis rotation (commonly: roll, tilting the head).

#ifdef __cplusplus
}

class IGestureCombinations
{
public:
    /**
    * Create new GestureCombinations object.
    */
    static IGestureCombinations* create(int number_of_parts);

    /**
    * Destructor.
    */
    virtual ~IGestureCombinations();

    /**
    * Get the number of subgestures / parts / hands used by this multi-gesture object.
    * \return   The number of parts/sub-gestures that this object is set to handle.
    */
    virtual int numberOfParts()=0;

    /**
    * Start new stroke (gesture motion).
    * \param  part              The sub-gesture index (or side) of the gesture motion.
    * \param  hmd               Transformation matrix (4x4) of the current headset position and rotation.
    * \param  record_as_sample  Which gesture this stroke will be a sample for, or -1 to identify the gesture.
    * \return                   Zero on success, a negative error code on failure.
    */
    virtual int startStroke(int part, const double hmd[4][4], int record_as_sample=-1)=0;

    /**
    * Start new stroke (gesture motion).
    * \param  part              The sub-gesture index (or side) of the gesture motion.
    * \param  hmd_p             Vector (x,y,z) of the current headset position.
    * \param  hmd_q             Quaternion (x,y,z,w) of the current headset rotation.
    * \param  record_as_sample  Which gesture this stroke will be a sample for, or -1 to identify the gesture.
    * \return                   Zero on success, a negative error code on failure.
    */
    virtual int startStroke(int part, const double hmd_p[3], const double hmd_q[4], int record_as_sample=-1)=0;

    /**
    * Continue stroke (gesture motion) data input (translational data only).
    * \param  part              The sub-gesture index (or side) of the gesture motion.
    * \param    p               Vector (x,y,z) of the current controller position.
    * \return                   Zero on success, a negative error code on failure.
    */
    virtual int contdStroke(int part, const double p[3])=0;

    /**
    * Continue stroke (gesture motion) data input with rotational data in the form of a quaternion.
    * \param    part            The sub-gesture index (or side) of the gesture motion.
    * \param    p               Vector (x,y,z) of the current controller position.
    * \param    q               Quaternion (x,y,z,w) of the current controller rotation.
    * \return                   Zero on success, a negative error code on failure.
    */
    virtual int contdStrokeQ(int part, const double p[3], const double q[4])=0;

    /**
    * Continue stroke (gesture motion) data input with rotational data in the form of a Euler rotation.
    * \param    part            The sub-gesture index (or side) of the gesture motion.
    * \param    p               Vector (x,y,z) of the current controller position.
    * \param    r               Euler rotations (x,y,z) of the current controller rotation.
    * \return                   Zero on success, a negative error code on failure.
    */
    virtual int contdStrokeE(int part, const double p[3], const double r[3])=0;

    /**
    * Continue stroke (gesture motion) data input with a transformation matrix (translation and rotation).
    * \param    part            The sub-gesture index (or side) of the gesture motion.
    * \param    m               Transformation matrix (4x4) of the current controller position and rotation.
    * \return                   Zero on success, a negative error code on failure.
    */
    virtual int contdStrokeM(int part, const double m[4][4])=0;

    /**
    * End the stroke (gesture motion) of one sub-gesture (part/side).
    * \param    part            The sub-gesture index (or side) of the gesture motion.
    * \param    pos             [OUT][OPTIONAL] The position where the gesture was performed.
    * \param    scale           [OUT][OPTIONAL] The scale (size) at which the gesture was performed.
    * \param    dir0            [OUT][OPTIONAL] The primary direction at which the gesture was performed.
    * \param    dir1            [OUT][OPTIONAL] The secondary direction at which the gesture was performed.
    * \param    dir2            [OUT][OPTIONAL] The least-significant direction at which the gesture was performed.
    * \return                   Zero on success, or a negative error code on failure.
    */
    virtual int endStroke(int part, double pos[3]=0, double* scale=0, double dir0[3]=0, double dir1[3]=0, double dir2[3]=0)=0;

    /**
    * Query whether a gesture performance (gesture motion, stroke) was started and is currently ongoing.
    * \return   True if a gesture motion (stroke) was started and is ongoing, false if not.
    */
    virtual bool isStrokeStarted(int part)=0;

    /**
    * Cancel a started stroke (gesture motion).
    * \param    part            The sub-gesture index (or side) of the gesture motion.
    * \return   Zero on success, an error code on failure.
    */
    virtual int cancelStroke(int part)=0;

    /**
    * Return the most likely gesture candidate for the previous multi-gesture.
    * \param    similarity      [OUT][OPTIONAL] The similarity (0~1) expressing how different the performed gesture motion was from the identified gesture.
    * \return                   The ID of the identified gesture combination, or a negative error code on failure.
    */
    virtual int identifyGestureCombination(double* similarity=0)=0;

    /**
    * Continuous gesture identification.
    * \param    hmd_p           Vector (x,y,z) of the current headset position.
    * \param    hmd_q           Quaternion (x,y,z,w) of the current headset rotation.
    * \param    similarity      [OUT][OPTIONAL] The similarity (0~1) expressing how different the performed gesture motion was from the identified gesture.
    * \return                   The ID of the identified gesture combination, or a negative error code on failure.
    */
    virtual int contdIdentify(const double hmd_p[3], const double hmd_q[4], double* similarity=0)=0;

    /**
    * Continuous gesture recording.
    * \param    hmd_p           Vector (x,y,z) of the current headset position.
    * \param    hmd_q           Quaternion (x,y,z,w) of the current headset rotation.
    * \return                   The ID of the recorded gesture on success, an negative error code on failure.
    */
    virtual int contdRecord(const double hmd_p[3], const double hmd_q[4])=0;

    /**
    * Get time frame for continuous gesture identification in milliseconds.
    * \param    part            The sub-gesture index (or side).
    * \return                   The time frame for continuous gesture identification in milliseconds.
    */
    virtual int getContdIdentificationPeriod(int part)=0;

    /**
    * Set time frame for continuous gesture identification in milliseconds.
    * \param    part            The sub-gesture index (or side).
    * \param    ms              The time frame for continuous gesture identification in milliseconds.
    * \return                   Zero on success, a negative error code on failure.
    */
    virtual int setContdIdentificationPeriod(int part, int ms)=0;

    /**
    * Get smoothing for continuous gesture identification in number of samples.
    * \param    part            The sub-gesture index (or side).
    * \return                   The number of samples for smoothing of continuous gesture identification.
    */
    virtual int getContdIdentificationSmoothing(int part)=0;

    /**
    * Set smoothing for continuous gesture identification in number of samples.
    * \param    part            The sub-gesture index (or side).
    * \param    samples         The number of samples for smoothing of continuous gesture identification.
    * \return                   Zero on success, a negative error code on failure.
    */
    virtual int setContdIdentificationSmoothing(int part, int samples)=0;

    /**
    * Get the number of gestures currently recorded in the i's sub-gesture AI.
    * \param    part            The sub-gesture index (or side).
    * \return                   The number of gestures currently recorded in the i's sub-gesture AI,
    *                           a negative error code on failure.
    */
    virtual int numberOfGestures(int part)=0;

    /**
    * Delete the recorded gesture with the specified index.
    * \param    part            The sub-gesture index (or side).
    * \param    index           The ID of the gesture to delete.
    * \return                   Zero on success, a negative error code on failure.
    */
    virtual int deleteGesture(int part, int index)=0;

    /**
    * Delete all recorded gestures of one sub-gesture (eg. side).
    * \param    part            The sub-gesture index (or side).
    * \return                   Zero on success, a negative error code on failure.
    */
    virtual int deleteAllGestures(int part)=0;

    /**
    * Create new gesture.
    * \param    part            The sub-gesture index (or side).
    * \param    name            The name for the new gesture.
    * \param    metadata        [OPTIONAL] Metadata to be associated with the gesture.
    * \return  New gesture ID or a negative error code on failure.
    */
    virtual int createGesture(int part, const char*  name, IGestureRecognition::Metadata* metadata=0)=0;

    /**
    * Copy gesture from one part/side to another.
    * \param    from_part           The sub-gesture index (or side) from which to copy.
    * \param    from_gesture_index  The index (ID) of the gesture to copy.
    * \param    to_part             The sub-gesture index (or side) to which to copy.
    * \param    into_gesture_index  The index (ID) of the gesture into to which the samples should be added.
    * \param    mirror_x            Whether to mirror the gesture samples over the x-axis.
    * \param    mirror_y            Whether to mirror the gesture samples over the y-axis.
    * \param    mirror_z            Whether to mirror the gesture samples over the z-axis.
    * \return                       The gesture index to which the copy was performed, or a negative error code on failure.
    */
    virtual int copyGesture(int from_part, int from_gesture_index, int to_part, int to_gesture_index, bool mirror_x=false, bool mirror_y=false, bool mirror_z=false)=0;

    /**
    * Get the gesture recognition score of the current neural network (0~1).
    * \param    part            The sub-gesture index (or side).
    * \return                   The gesture recognition score of the current neural network (0~1).
    */
    virtual double gestureRecognitionScore(int part, bool all_samples=false)=0;

    /**
    * Get the name of a registered gesture.
    * \param    part            The sub-gesture index (or side).
    */
    virtual const char* getGestureName(int part, int index)=0;

    /**
    * Get the length of the name of a registered gesture.
    * \param    part            The sub-gesture index (or side).
    * \param    index           The ID of the gesture.
    * \return                   The length of the gesture name (in chars), a negative error code on failure.
    */
    virtual int getGestureNameLength(int part, int index)=0;

    /**
    * Copy the name of a registered gesture to a buffer.
    * \param    part            The sub-gesture index (or side).
    * \param    index           The ID of the gesture.
    * \param    buf             [OUT] The string buffer to which to write the gesture name.
    * \param    buflen          The length of the buf string buffer.
    * \return                   The number of characters written to the buffer, zero on failure.
    */
    virtual int copyGestureName(int part, int index, char* buf, int buflen)=0;

    /**
    * Get the metadata of a registered gesture.
    * \param    part            The sub-gesture index (or side).
    * \param    index           The gesture ID of the gesture whose metadata to get.
    * \return                   The metadata registered with the gesture, null on failure.
    */
    virtual IGestureRecognition::Metadata* getGestureMetadata(int part, int index)=0;

    /**
    * Get the number of recorded samples of a registered gesture.
    * \param    part            The sub-gesture index (or side).
    * \param    index           The gesture ID of the gesture.
    * \return                   The number of recorded samples, a negative error code on failure.
    */
    virtual int getGestureNumberOfSamples(int part, int index)=0;

    /**
    * Get the number of data points a sample has.
    * \param   part            The sub-gesture index (or side).
    * \param   gesture_index   The zero-based index (ID) of the gesture from where to retrieve the number of data points.
    * \param   sample_index    The zero-based index (ID) of the sample for which to retrieve the number of data points.
    * \param   processed       Whether the number of raw data points should be retrieved (false) or the number of processed data points (true).
    * \return  The number of data points on that stroke sample, 0 if an error occurred.
    */
    virtual int getGestureSampleLength(int part, int gesture_index, int sample_index, bool processed)=0;

    /**
    * Retrieve a sample stroke.
    * \param   part            The sub-gesture index (or side).
    * \param   gesture_index   The zero-based index (ID) of the gesture from where to retrieve the sample.
    * \param   sample_index    The zero-based index (ID) of the sample to retrieve.
    * \param   processed       Whether the raw data points should be retrieved (false) or the processed data points (true).
    * \param   hmd_p           [OUT][OPTIONAL] A place to store the HMD positional data. May be zero if this data is not required.
    * \param   hmd_q           [OUT][OPTIONAL] A place to store the HMD rotational data. May be zero if this data is not required.
    * \param   p               [OUT][OPTIONAL] A place to store the stroke positional data. May be zero if this data is not required.
    * \param   q               [OUT][OPTIONAL] A place to store the stroke rotational data. May be zero if this data is not required.
    * \param   stroke_buf_size The length of p and/or q in number of data points. The function will at most write this many data points.
    * \return                  The number of stroke sample data points that have been written, 0 if an error occurred.
    */
    virtual int getGestureSampleStroke(int part, int gesture_index, int sample_index, bool processed, double hmd_p[3], double hmd_q[4], double p[][3], double q[][4], int stroke_buf_size)=0;

    /**
    * Get the number of samples of the gesture mean (average over samples).
    * \param   part            The sub-gesture index (or side).
    * \param   gesture_index   The zero-based index (ID) of the gesture from where to retrieve the sample.
    * \return                  The number of stroke sample data points that the mean consists of.
    */
    virtual int getGestureMeanLength(int part, int gesture_index)=0;

    /**
    * Retrieve a gesture mean (average over samples).
    * \param   part            The sub-gesture index (or side).
    * \param   gesture_index   The zero-based index (ID) of the gesture from where to retrieve the sample.
    * \param   sample_index    The zero-based index (ID) of the sample to retrieve.
    * \param   p               [OUT] A place to store the stroke positional data. May be zero if this data is not required.
    * \param   q               [OUT] A place to store the stroke rotational data. May be zero if this data is not required.
    * \param   stroke_buf_size The length of p and/or q in number of data points. The function will at most write this many data points.
    * \param   hmd_p           [OUT][OPTIONAL] A place to store the average gesture position relative to (ie. as seen by) the headset. May be zero if this data is not required.
    * \param   hmd_q           [OUT][OPTIONAL] A place to store the average gesture rotation relative to (ie. as seen by) the headset. May be zero if this data is not required.
    * \param   scale           [OUT][OPTIONAL] A place to store the average gesture Scale. May be zero if this data is not required.
    * \return                  The number of stroke sample data points that have been written, 0 if an error occurred.
    */
    virtual int getGestureMeanStroke(int part, int gesture_index, double p[][3], double q[][4], int stroke_buf_size, double hmd_p[3], double hmd_q[4], double* scale)=0;

    /**
    * Delete a gesture sample recording from the set.
    * \param   part            The sub-gesture index (or side).
    * \param   gesture_index   The zero-based index (ID) of the gesture from where to delete the sample.
    * \param   sample_index    The zero-based index (ID) of the sample to delete.
    * \return                  Zero on success, a negative error code on failure.
    */
    virtual int deleteGestureSample(int part, int gesture_index, int sample_index)=0;

    /**
    * Delete all gesture sample recordings from the set.
    * \param   part            The sub-gesture index (or side).
    * \param   gesture_index   The zero-based index (ID) of the gesture from where to delete the sample.
    * \return                  Zero on success, a negative error code on failure.
    */
    virtual int deleteAllGestureSamples(int part, int gesture_index)=0;

    /**
    * Set the name of a registered gesture.
    * \param    part        The sub-gesture index (or side).
    * \param    index       The zero-based index (ID) of the gesture to rename.
    * \param    name        The new name for the gesture.
    * \return               Zero on success, a negative error code on failure.
    */
    virtual int setGestureName(int part, int index, const char* name)=0;

    /**
    * Set the metadata of a registered gesture.
    * \param    part            The sub-gesture index (or side).
    * \param    metadata        The new metadata to be stored with the gesture.
    * \return                   Zero on success, a negative error code on failure.
    */
    virtual int setGestureMetadata(int part, int index, IGestureRecognition::Metadata* metadata)=0;

    /**
    * Save the neural network and recorded training data to file.
    * \param    path            The file path at which to save the AI and recorded data.
    * \return                   Zero on success, a negative error code on failure.
    */
    virtual int saveToFile(const char* path)=0;

    /**
    * Load the neural network and recorded training data from file.
    * \param    path            The file path from which to load the AI and recorded data.
    * \param    createMetadata  [OPTIONAL] The function which can parse the metadata which was stored with the gestures.
    * \return                   Zero on success, a negative error code on failure.
    */
    virtual int loadFromFile(const char* path, IGestureRecognition::MetadataCreatorFunction* createMetadata=0)=0;

    /**
    * Load the neural network and recorded training data buffer.
    * \param    buffer          Memory buffer from which to load the AI and recorded data.
    * \param    buffer_size     Size of the memory buffer in byte.
    * \param    createMetadata  [OPTIONAL] The function which can parse the metadata which was stored with the gestures.
    * \return                   Zero on success, a negative error code on failure.
    */
    virtual int loadFromBuffer(const char* buffer, int buffer_size, IGestureRecognition::MetadataCreatorFunction* createMetadata=0)=0;

    /**
    * Load the neural network and recorded training data from stream.
    * \param    stream          The std::istream from which to load the AI and recorded data.
    * \param    createMetadata  [OPTIONAL] The function which can parse the metadata which was stored with the gestures.
    * \return                   Zero on success, a negative error code on failure.
    */
    virtual int loadFromStream(void* stream, IGestureRecognition::MetadataCreatorFunction* createMetadata=0)=0;

    /**
    * Import recorded training data from file.
    * Gestures of the same name will be merged into one. The optional 'mapping' parameter will
    * inform you of how the merging was performed.
    * After importing, you have to run the AI training process again for the changes to take place.
    * \param    path            The file path from which to import gestures.
    * \param    createMetadata  [OPTIONAL] The function which can parse the metadata which was stored with the gestures.
    * \return                   Zero on success, a negative error code on failure.
    */
    virtual int importFromFile(const char* path, IGestureRecognition::MetadataCreatorFunction* createMetadata=0)=0;

    /**
    * Import recorded training data buffer.
    * Gestures of the same name will be merged into one. The optional 'mapping' parameter will
    * inform you of how the merging was performed.
    * After importing, you have to run the AI training process again for the changes to take place.
    * \param    buffer          The memory buffer from which to import gestures.
    * \param    buffer_size     The size of the memory buffer in byte.
    * \param    createMetadata  [OPTIONAL] The function which can parse the metadata which was stored with the gestures.
    * \return                   Zero on success, a negative error code on failure.
    */
    virtual int importFromBuffer(const char* buffer, int buffer_size, IGestureRecognition::MetadataCreatorFunction* createMetadata=0)=0;

    /**
    * Import recorded training data from stream.
    * Gestures of the same name will be merged into one. The optional 'mapping' parameter will
    * inform you of how the merging was performed.
    * After importing, you have to run the AI training process again for the changes to take place.
    * \param    stream          The std::istream from which to import gestures.
    * \param    createMetadata  [OPTIONAL] The function which can parse the metadata which was stored with the gestures.
    * \return                   Zero on success, a negative error code on failure.
    */
    virtual int importFromStream(void* stream, IGestureRecognition::MetadataCreatorFunction* createMetadata=0)=0;

    /**
    * Save the neural network and recorded training data to file.
    * \param    part            The sub-gesture index (or side).
    * \param    path            The file path at which to save the AI and recorded data.
    * \return                   Zero on success, a negative error code on failure.
    */
    virtual int saveGestureToFile(int part, const char* path)=0;

    /**
    * Load the neural network and recorded training data from file.
    * \param    part            The sub-gesture index (or side).
    * \param    path            The file path from which to load the AI and recorded data.
    * \param    createMetadata  [OPTIONAL] The function which can parse the metadata which was stored with the gestures.
    * \return                   Zero on success, a negative error code on failure.
    */
    virtual int loadGestureFromFile(int part, const char* path, IGestureRecognition::MetadataCreatorFunction* createMetadata=0)=0;

    /**
    * Load the neural network and recorded training data buffer.
    * \param    part            The sub-gesture index (or side).
    * \param    buffer          Memory buffer from which to load the AI and recorded data.
    * \param    buffer_size     Size of the memory buffer in byte.
    * \param    createMetadata  [OPTIONAL] The function which can parse the metadata which was stored with the gestures.
    * \return                   Zero on success, a negative error code on failure.
    */
    virtual int loadGestureFromBuffer(int part, const char* buffer, int buffer_size, IGestureRecognition::MetadataCreatorFunction* createMetadata=0)=0;

    /**
    * Get the number of currently recorded gesture combinations.
    * \return                   The number of currently recorded gesture combinations.
    */
    virtual int numberOfGestureCombinations()=0;

    /**
    * Delete the recorded gesture combination with the specified index.
    * \param    index           The ID (zero-based index) of the gesture combination to delete.
    * \return                   Zero on success, a negative error code on failure.
    */
    virtual int deleteGestureCombination(int index)=0;

    /**
    * Delete all gesture combinations.
    * \return                   Zero on success, a negative error code on failure.
    */
    virtual int deleteAllGestureCombinations()=0;

    /**
    * Create new gesture combination.
    * \param    name            The name for the new gesture combination.
    * \return                   The ID (index) of the newly created gesture combination, or a negative error code on failure.
    */
    virtual int createGestureCombination(const char*  name)=0;

    /**
    * Set which gesture this multi-gesture expects for part (side) i.
    * \param    combination_index The gesture combination ID.
    * \param    part            The sub-gesture index (or side).
    * \param    gesture_index   The gesture ID on the i's part which to be set as expected gesture for this part(side).
    * \return                   Zero on success, a negative error code on failure.
    */
    virtual int setCombinationPartGesture(int combination_index, int part, int gesture_index)=0;

    /**
    * Get which gesture this multi-gesture expects for step i.
    * \param    combination_index The gesture combination ID.
    * \param    part            The sub-gesture index (or side).
    * \return                   The gesture ID (index) on the i's part(side) for this gesture combination.
    */
    virtual int getCombinationPartGesture(int combination_index, int part)=0;

    /**
    * Get the name of a registered gesture combination.
    * \param    index           The gesture combination ID.
    * \return                   The name of the gesture combination, zero on error.
    */
    virtual const char* getGestureCombinationName(int index)=0;

    /**
    * Get the length of the name of a registered multi-gesture.
    * \param    index           The gesture combination ID.
    * \return                   The length of the name of the gesture combination, a negative error code on failure.
    */
    virtual int getGestureCombinationNameLength(int index)=0;

    /**
    * Copy the name of a registered multi-gesture to a buffer.
    * \param    index           The gesture combination ID.
    * \param    buf             [OUT] The string buffer to which to write the gesture name.
    * \param    buflen          The length of the buf string buffer.
    * \return                   The number of characters written to the buffer, zero on failure.
    */
    virtual int copyGestureCombinationName(int index, char* buf, int buflen)=0;

    /**
    * Set the name of a registered multi-gesture.
    * \param    index           The gesture combination ID.
    * \param    name            The new name for the gesture combination.
    * \return                   Zero on success, a negative error code on failure.
    */
    virtual int setGestureCombinationName(int index, const char* name)=0;

    /**
    * Start train the Neural Network based on the the currently collected data.
    * \return                   Zero on success, a negative error code on failure.
    */
    virtual int startTraining()=0;

    /**
    * Whether the Neural Network is currently training.
    * \return                   True if the GestureCombinations AI is currently training, false if not.
    */
    virtual bool isTraining()=0;

    /**
    * Stop the training process (last best result will be used).
    */
    virtual void stopTraining()=0;

    /**
    * Get the gesture recognition score of the current neural network (0~1).
    * Here, 0 means that not a single gesture combination was correctly identified,
    * and 1 means that 100% of gestures were correctly identified.
    * \return                   The gesture recognition score of the current neural network (0~1).
    */
    virtual double recognitionScore()=0;

    /**
    * Maximum training time in seconds.
    */
    unsigned long maxTrainingTime;

    /**
    * Function pointer to an optional callback function to e called during training.
    * \param performance        The current percentage of correctly recognized gestures (0~1).
    * \param metadata           The meta data pointer set on trainingUpdateCallbackMetadata or trainingFinishCallbackMetadata.
    */
    typedef void GESTURERECOGNITION_CALLCONV TrainingCallbackFunction(double performance, void* metadata);

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
    * Get the maximum time for training in seconds.
    * \return                   The maximum time for training in seconds.
    */
    virtual unsigned long getMaxTrainingTime()=0;

    /**
    * Set the current maximum training time in seconds
    * \param    t               The maximum time for training in seconds.
    */
    virtual void setMaxTrainingTime(unsigned long t)=0;

    /**
    * Whether the rotation of the users head should be considered when recording and performing gestures.
    */
    IGestureRecognition::RotationalFrameOfReference rotationalFrameOfReference;

    /**
    * Run internal tests to check for code correctness and data consistency.
    */
    virtual int runTests() = 0;
};

typedef IGestureCombinations IGestureCombinations;
#endif // #ifdef __cplusplus

#endif //__GESTURE_COMBINATIONS