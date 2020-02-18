/*
 * MiVRy - 3D gesture recognition library.
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

#define GESTURERECOGNITION_DEBUGOUTPUT                     0 //!< Whether to output debugging data

#define GESTURERECOGNITION_RESULT_SUCCESS                  0   //!< Return code for: function executed successfully.
#define GESTURERECOGNITION_RESULT_ERROR_INVALIDPARAM       -1  //!< Return code for: invalid parameter(s) provided to function.
#define GESTURERECOGNITION_RESULT_ERROR_INVALIDINDEX       -2  //!< Return code for: invalid index provided to function.
#define GESTURERECOGNITION_RESULT_ERROR_INVALIDPATH        -3  //!< Return code for: invalid file path provided to function.
#define GESTURERECOGNITION_RESULT_ERROR_INVALIDFILE        -4  //!< Return code for: path to an invalid file provided to function.
#define GESTURERECOGNITION_RESULT_ERROR_NUMERICINSTABILITY -5  //!< Return code for: calculations failed due to numeric instability (too small or too large numbers).
#define GESTURERECOGNITION_RESULT_ERROR_DATACORRUPTION     -6  //!< Return code for: the internal state of the AI was corrupted.
#define GESTURERECOGNITION_RESULT_ERROR_INSUFFICIENTDATA   -7  //!< Return code for: available data (number of samples etc) is insufficient for this operation.
#define GESTURERECOGNITION_RESULT_ERROR_CURRENTLYTRAINING  -8  //!< Return code for: the operation could not be performed because the AI is currently training.

#define GESTURERECOGNITION_DEFAULT_CONTDIDENTIFICATIONPERIOD       1000//!< Default time frame for continuous gesture identification in milliseconds.
#define GESTURERECOGNITION_DEFAULT_CONTDIDENTIFICATIONSMOOTHING    3   //!< Default smoothing setting for continuous gesture identification in number of samples.


#if _WIN32
#define LIBEXPORT __declspec(dllexport)
#else
#define LIBEXPORT 
#endif
#ifdef __cplusplus
extern "C" {
#endif
    typedef void* MetadataCreatorFunction(); //!< Function pointer type to a function that creates Metadata objects.
    typedef void TrainingCallbackFunction(double performance, void* metadata); //!< Function pointer to an optional callback function to e called during training.
    LIBEXPORT void* GestureRecognition_create(); //!< Create new instance.
    LIBEXPORT void  GestureRecognition_delete(void* gro); //!< Delete instance.
    LIBEXPORT int   GestureRecognition_startStroke(void* gro, const double hmd_p[3], const double hmd_q[4], int record_as_sample); //!< Start new stroke.
    LIBEXPORT int   GestureRecognition_startStrokeM(void* gro, const double hmd[4][4], int record_as_sample); //!< Start new stroke.
    LIBEXPORT int   GestureRecognition_contdStroke(void* gro, const double p[3]); //!< Continue stroke data input (translational data only).
    LIBEXPORT int   GestureRecognition_contdStrokeQ(void* gro, const double p[3], const double q[4]); //!< Continue stroke data input with rotational data in the form of a quaternion.
    LIBEXPORT int   GestureRecognition_contdStrokeE(void* gro, const double p[3], const double r[3]); //!< Continue stroke data input with rotational data in the form of a Euler rotation.
    LIBEXPORT int   GestureRecognition_contdStrokeM(void* gro, const double m[4][4]); //!< Continue stroke data input with a transformation matrix (translation and rotation).
    LIBEXPORT int   GestureRecognition_endStroke(void* gro, double pos[3], double* scale, double dir0[3], double dir1[3], double dir2[3]); //!< End the stroke and identify the gesture.
    LIBEXPORT int   GestureRecognition_endStrokeAndGetAllProbabilities(void* gro, double p[], int n, double pos[3], double* scale, double dir0[3], double dir1[3], double dir2[3]); //!< End the stroke and get gesture probabilities.
    LIBEXPORT int   GestureRecognition_endStrokeAndGetSimilarity(void* gro, double* similarity, double pos[3], double* scale, double dir0[3], double dir1[3], double dir2[3]); //!< End the stroke and get similarity value.
    LIBEXPORT int   GestureRecognition_endStrokeAndGetAllProbabilitiesAndSimilarities(void* gro, double p[], double s[], int n, double pos[3], double* scale, double dir0[3], double dir1[3], double dir2[3]); //!< End the stroke and get gesture probabilities and similarity values.
    LIBEXPORT int   GestureRecognition_cancelStroke(void* gro); //!< Cancel a started stroke.

    LIBEXPORT int   GestureRecognition_contdIdentify(void* gro, const double hmd_p[3], const double hmd_q[4], double* similarity); //!< Continuous gesture identification.
    LIBEXPORT int   GestureRecognition_contdRecord(void* gro, const double hmd_p[3], const double hmd_q[4]); //!< Continuous gesture recording.
    LIBEXPORT int   GestureRecognition_getContdIdentificationPeriod(void* gro); //!< Get time frame for continuous gesture identification in milliseconds.
    LIBEXPORT int   GestureRecognition_setContdIdentificationPeriod(void* gro, int ms); //!< Set time frame for continuous gesture identification in milliseconds.
    LIBEXPORT int   GestureRecognition_getContdIdentificationSmoothing(void* gro); //!< Get smoothing for continuous gesture identification in number of samples.
    LIBEXPORT int   GestureRecognition_setContdIdentificationSmoothing(void* gro, int samples); //!< Set smoothing for continuous gesture identification in number of samples.

    LIBEXPORT int  GestureRecognition_numberOfGestures(void* gro); //!< Get the number of gestures currently recorded in the system.
    LIBEXPORT int  GestureRecognition_deleteGesture(void* gro, int index); //!< Delete the recorded gesture with the specified index.
    LIBEXPORT int  GestureRecognition_deleteAllGestures(void* gro); //!< Delete recorded gestures.
    LIBEXPORT int  GestureRecognition_createGesture(void* gro, const char* name, void* metadata); //!< Create new gesture.
    LIBEXPORT double GestureRecognition_recognitionScore(void* gro); //!< Get the gesture recognition score of the current neural network (0~1).
        
    LIBEXPORT const char* GestureRecognition_getGestureName(void* gro, int index); //!< Get the name of a registered gesture.
    LIBEXPORT int         GestureRecognition_getGestureNameLength(void* gro, int index); //!< Get the length of the name of a registered gesture.
    LIBEXPORT int         GestureRecognition_copyGestureName(void* gro, int index, char* buf, int buflen); //!< Copy the name of a registered gesture to a buffer.

    LIBEXPORT void*       GestureRecognition_getGestureMetadata(void* gro, int index); //!< Get the command of a registered gesture.
    LIBEXPORT int         GestureRecognition_getGestureNumberOfSamples(void* gro, int index); //!< Get the number of recorded samples of a registered gesture.
    LIBEXPORT int         GestureRecognition_getGestureSampleLength(void* gro, int gesture_index, int sample_index, int processed); //!< Get the number of data points a sample has.
    LIBEXPORT int         GestureRecognition_getGestureSampleStroke(void* gro, int gesture_index, int sample_index, int processed, double hmd_p[3], double hmd_q[4], double p[][3], double q[][4], int stroke_buf_size); //!< Retrieve a sample stroke.
    LIBEXPORT int         GestureRecognition_deleteGestureSample(void* gro, int gesture_index, int sample_index); //!< Delete a gesture sample recording from the set.
    LIBEXPORT int         GestureRecognition_deleteAllGestureSamples(void* gro, int gesture_index); //!< Delete all gesture sample recordings from the set.

    LIBEXPORT int GestureRecognition_setGestureName(void* gro, int index, const char* name); //!< Set the name of a registered gesture.
    LIBEXPORT int GestureRecognition_setGestureMetadata(void* gro, int index, void* metadata); //!< Set the command of a registered gesture.

    LIBEXPORT int GestureRecognition_saveToFile(void* gro, const char* path); //!< Save the neural network and recorded training data to file.
    LIBEXPORT int GestureRecognition_saveToStream(void* gro, void* stream); //!< Save the neural network and recorded training data to std::ofstream.
    LIBEXPORT int GestureRecognition_loadFromFile(void* gro, const char* path, MetadataCreatorFunction* createMetadata); //!< Load the neural network and recorded training data from file.
    LIBEXPORT int GestureRecognition_loadFromBuffer(void* gro, const char* buffer, MetadataCreatorFunction* createMetadata); //!< Load the neural network and recorded training data from buffer.
    LIBEXPORT int GestureRecognition_loadFromStream(void* gro, void* stream, MetadataCreatorFunction* createMetadata); //!< Load the neural network and recorded training data from std::istream.

    LIBEXPORT int  GestureRecognition_startTraining(void* gro); //!< Start train the Neural Network based on the the currently collected data.
    LIBEXPORT int  GestureRecognition_isTraining(void* gro); //!< Whether the Neural Network is currently training.
    LIBEXPORT void GestureRecognition_stopTraining(void* gro); //!< Stop the training process (last best result will be used).

    LIBEXPORT int   GestureRecognition_getMaxTrainingTime(void* gro); //!< Get maximum training time in seconds.
    LIBEXPORT void  GestureRecognition_setMaxTrainingTime(void* gro, int t); //!< Set maximum training time in seconds.

    LIBEXPORT void GestureRecognition_setTrainingUpdateCallback(void* gro, TrainingCallbackFunction* cbf); //!< Set callback function to be called during training.
    LIBEXPORT void GestureRecognition_setTrainingFinishCallback(void* gro, TrainingCallbackFunction* cbf); //!< Set callback function to be called when training is finished.
    
    LIBEXPORT void GestureRecognition_setTrainingUpdateCallbackMetadata(void* gro, void* metadata); //!< Set metadata for callback function to be called during training.
    LIBEXPORT void GestureRecognition_setTrainingFinishCallbackMetadata(void* gro, void* metadata); //!< Set metadata for callback function to be called when training is finished.
    LIBEXPORT void* GestureRecognition_getTrainingUpdateCallbackMetadata(void* gro); //!< Get callback data for function to be called during training.
    LIBEXPORT void* GestureRecognition_getTrainingFinishCallbackMetadata(void* gro); //!< Get callback data for function to be called when training is finished.


    LIBEXPORT int   GestureRecognition_getIgnoreHeadRotationX(void* gro); //!< Get whether the horizontal rotation of the users head (commonly called "pan" or "yaw", looking left or right) should be considered when recording and performing gestures.
    LIBEXPORT void  GestureRecognition_setIgnoreHeadRotationX(void* gro, int on_off); //!< Set whether the horizontal rotation of the users head (commonly called "pan" or "yaw", looking left or right) should be considered when recording and performing gestures.
    LIBEXPORT int   GestureRecognition_getIgnoreHeadRotationY(void* gro); //!< Get whether the vertical rotation of the users head (commonly called "pitch", looking up or down) should be considered when recording and performing gestures.
    LIBEXPORT void  GestureRecognition_setIgnoreHeadRotationY(void* gro, int on_off); //!< Set whether the vertical rotation of the users head (commonly called "pitch", looking up or down) should be considered when recording and performing gestures.
    LIBEXPORT int   GestureRecognition_getIgnoreHeadRotationZ(void* gro); //!< Get whether the tilting rotation of the users head (also called "roll" or "bank", tilting the head to the site without changing the view direction) should be considered when recording and performing gestures.
    LIBEXPORT void  GestureRecognition_setIgnoreHeadRotationZ(void* gro, int on_off); //!< Set whether the tilting rotation of the users head (also called "roll" or "bank", tilting the head to the site without changing the view direction) should be considered when recording and performing gestures.

#if GESTURERECOGNITION_DEBUGOUTPUT
    LIBEXPORT void  GestureRecognition_setDebugOutputFile(void* gro, const char* path); //!< Set where to write debug information.
#endif
#ifdef __cplusplus
}

class _GestureRecognition
{
public:
    enum Result {
        Success = GESTURERECOGNITION_RESULT_SUCCESS //!< Return code for: function executed successfully.
        ,
        Error_InvalidParameter = GESTURERECOGNITION_RESULT_ERROR_INVALIDPARAM //!< Return code for: invalid parameter(s) provided to function.
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
    };

    typedef double DoubleVector3[3];        //!< Shorthand for a double[3] vector / array.
    typedef double DoubleVector4[4];        //!< Shorthand for a double[4] vector / array.
    typedef double DoubleMatrix4x4[4][4];   //!< Shorthand for a double[4][4] matrix / 2d-array.

    static _GestureRecognition* create(); //!< Create new GestureRecognition object.
    virtual ~_GestureRecognition(); //!< Destructor.

    // Interface for metadata objects to attach to a gesture.
    struct Metadata
    {
        virtual ~Metadata() {};
        virtual bool writeToStream(std::ostream* stream)=0;
        virtual bool readFromStream(std::istream* stream)=0;
    };
    typedef Metadata* MetadataCreatorFunction(); //!< Function pointer type to a function that creates Metadata objects.

    typedef void TrainingCallbackFunction(double performance, void* metadata); //!< Function pointer to an optional callback function to e called during training.

    virtual int startStroke(const double hmd[4][4], int record_as_sample=-1)=0; //!< Start new stroke.
    virtual int startStroke(const double hmd_p[3], const double hmd_q[4], int record_as_sample=-1)=0; //!< Start new stroke.
    virtual int contdStroke(const double p[3])=0; //!< Continue stroke data input  (translational data only).
    virtual int contdStrokeQ(const double p[3], const double q[4])=0; //!< Continue stroke data input with rotational data in the form of a quaternion.
    virtual int contdStrokeE(const double p[3], const double r[3])=0; //!< Continue stroke data input with rotational data in the form of a Euler rotation.
    virtual int contdStrokeM(const double m[4][4])=0; //!< Continue stroke data input with a transformation matrix (translation and rotation).
    virtual int endStroke(double pos[3]=0, double* scale=0, double dir0[3]=0, double dir1[3]=0, double dir2[3]=0)=0; //!< End the stroke and identify the gesture.
    virtual int endStrokeAndGetAllProbabilities(double p[], int n, double pos[3]=0, double* scale=0, double dir0[3]=0, double dir1[3]=0, double dir2[3]=0)=0; //!< End the stroke and get gesture probabilities.
    virtual int endStrokeAndGetSimilarity(double* similarity, double pos[3]=0, double* scale=0, double dir0[3]=0, double dir1[3]=0, double dir2[3]=0)=0; //!< End the stroke and get similarity value.
    virtual int endStrokeAndGetAllProbabilitiesAndSimilarities(double p[], double s[], int n, double pos[3]=0, double* scale=0, double dir0[3]=0, double dir1[3]=0, double dir2[3]=0)=0; //!< End the stroke and get gesture probabilities and similarity values.
    virtual int cancelStroke()=0; //!< Cancel a started stroke.

    virtual int contdIdentify(const double hmd_p[3], const double hmd_q[4], double* similarity=0)=0; //!< Continuous gesture identification.
    virtual int contdIdentifyAllProbabilitiesAndSimilarities(const double hmd_p[3], const double hmd_q[4], double p[], double s[], int n)=0; //!< Continuous gesture identification.
    virtual int contdRecord(const double hmd_p[3], const double hmd_q[4])=0; //!< Continuous gesture recording.
    unsigned int contdIdentificationPeriod; //!< Time frame in milliseconds for continuous gesture identification.
    unsigned int contdIdentificationSmoothing; //!< The number of samples to use for smoothing continuous gesture identification results.

    virtual int  numberOfGestures()=0; //!< Get the number of gestures currently recorded in the system.
    virtual bool deleteGesture(int index)=0; //!< Delete the recorded gesture with the specified index.
    virtual bool deleteAllGestures()=0; //!< Delete recorded gestures.
    virtual int  createGesture(const char*  name, Metadata* metadata=0)=0; //!< Create new gesture.
    virtual double recognitionScore()=0; //!< Get the gesture recognition score of the current neural network (0~1).
        
    virtual const char* getGestureName(int index)=0; //!< Get the name of a registered gesture.
    virtual int         getGestureNameLength(int index)=0; //!< Get the length of the name of a registered gesture.
    virtual int         copyGestureName(int index, char* buf, int buflen)=0; //!< Copy the name of a registered gesture to a buffer.
    virtual Metadata*   getGestureMetadata(int index)=0; //!< Get the command of a registered gesture.
    virtual int         getGestureNumberOfSamples(int index)=0; //!< Get the number of recorded samples of a registered gesture.
    virtual int         getGestureSampleLength(int gesture_index, int sample_index, bool processed)=0; //!< Get the number of data points a sample has.
    virtual int         getGestureSampleStroke(int gesture_index, int sample_index, bool processed, double hmd_p[3], double hmd_q[4], double p[][3], double q[][4], int stroke_buf_size)=0; //!< Retrieve a sample stroke.
    virtual bool        deleteGestureSample(int gesture_index, int sample_index)=0; //!< Delete a gesture sample recording from the set.
    virtual bool        deleteAllGestureSamples(int gesture_index)=0; //!< Delete all gesture sample recordings from the set.

    virtual bool setGestureName(int index, const char* name)=0; //!< Set the name of a registered gesture.
    virtual bool setGestureMetadata(int index, Metadata* metadata)=0; //!< Set the command of a registered gesture.

    virtual bool saveToFile(const char* path)=0; //!< Save the neural network and recorded training data to file.
    virtual bool saveToStream(void* stream)=0; //!< Save the neural network and recorded training data to std::ofstream.
    virtual bool loadFromFile(const char* path, MetadataCreatorFunction* createMetadata=0)=0; //!< Load the neural network and recorded training data from file.
    virtual bool loadFromBuffer(const char* buffer, MetadataCreatorFunction* createMetadata=0)=0; //!< Load the neural network and recorded training data buffer.
    virtual bool loadFromStream(void* stream, MetadataCreatorFunction* createMetadata=0)=0; //!< Load the neural network and recorded training data from std::istream.

    virtual bool startTraining()=0; //!< Start train the Neural Network based on the the currently collected data.
    virtual bool isTraining()=0; //!< Whether the Neural Network is currently training.
    virtual void stopTraining()=0; //!< Stop the training process (last best result will be used).

    unsigned long maxTrainingTime; //!< Maximum training time in seconds.
    TrainingCallbackFunction* trainingUpdateCallback; //!< Optional callback function to be called during training.
    void*                     trainingUpdateCallbackMetadata; //!< Optional metadata to be provided with the callback during training.
    TrainingCallbackFunction* trainingFinishCallback; //!< Optional callback function to be called when training is finished.
    void*                     trainingFinishCallbackMetadata; //!< Optional metadata to be provided with the callback when training is finished.
    
    /// Whether the rotation of the users head should be considered when recording and performing gestures.
    struct IgnoreHeadRotation {
        bool x; //!< Whether the horizontal rotation of the users head (commonly called "pan" or "yaw", looking left or right) should be considered when recording and performing gestures.
        bool y; //!< Whether the vertical rotation of the users head (commonly called "pitch", looking up or down) should be considered when recording and performing gestures.
        bool z; //!< Whether the tilting rotation of the users head (also called "roll" or "bank", tilting the head to the site without changing the view direction) should be considered when recording and performing gestures.
    } ignoreHeadRotation; //!< Whether the rotation of the users head should be considered when recording and performing gestures.

    virtual int runTests()=0; //!< Run internal tests to check for code correctness and data consistency.
    
#if GESTURERECOGNITION_DEBUGOUTPUT
    virtual void setDebugOutputFile(const char* path)=0; //!< Set where to write debug information.
#endif
};

typedef _GestureRecognition IGestureRecognition;
#endif // #ifdef __cplusplus

#endif //__GESTURE_RECOGNITION_H