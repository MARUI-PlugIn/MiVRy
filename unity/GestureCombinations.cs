/*
 * MiVRy - VR gesture recognition library for multi-part gesture combinations plug-in for Unity.
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
 * (1) Place the gesturerecognition.dll (Windows) and/or
 * libgesturerecognition.so (Android / MobileVR) files 
 * in the /Assets/Plugins/ folder in your unity project
 * and add the GestureCombinations.cs file to your project scripts. 
 * 
 * 
 * (2) Create a new Gesture recognition object and register the gestures that you want to identify later.
 * (In this example, we use gesture part "0" to mean "left hand" and gesture part "1" to mean right hand,
 * but it could also be two sequential gesture parts performed with the same hand.)
 * <code>
 * GestureCombinations gc = new GestureCombinations(2);
 * int myFirstCombo = gc.createGestureCombination("wave your hands");
 * int mySecondCombo = gc.createGesture("play air-guitar");
 * </code>
 * Also, create the individual gestures that each combo will consist.
 * <code>
 * int myFirstCombo_leftHandGesture = gc.createGesture(0, "Wave left hand");
 * int myFirstCombo_rightHandGesture = gc.createGesture(1, "Wave right hand");
 * int mySecondCombo_leftHandGesture = gc.createGesture(0, "Hold guitar neck");
 * int mySecondCombo_rightHandGesture = gc.createGesture(1, "Hit strings");
 * </code>
 * Then set the Gesture Combinations to be the connection of those gestures.
 * <code>
 * gc.setCombinationPartGesture(myFirstCombo, 0, myFirstCombo_leftHandGesture);
 * gc.setCombinationPartGesture(myFirstCombo, 1, myFirstCombo_rightHandGesture);
 * gc.setCombinationPartGesture(mySecondCombo, 0, mySecondCombo_leftHandGesture);
 * gc.setCombinationPartGesture(mySecondCombo, 1, mySecondCombo_rightHandGesture);
 * </code>
 * 
 * 
 * 
 * (3) Record a number of samples for each gesture by calling startStroke(), contdStroke() and endStroke()
 * for your registered gestures, each time inputting the headset and controller transformation.
 * <code>
 * Vector3 hmd_p = Camera.main.gameObject.transform.position;
 * Quaternion hmd_q = Camera.main.gameObject.transform.rotation;
 * gc.startStroke(0, hmd_p, hmd_q, myFirstCombo_leftHandGesture);
 * gc.startStroke(1, hmd_p, hmd_q, myFirstCombo_rightHandGesture);
 * 
 * // repeat the following while performing the gesture with your controller:
 * Vector3 p_left = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch);
 * Quaternion q_left = OVRInput.GetLocalControllerRotation(OVRInput.Controller.LTouch);
 * gc.contdStrokeQ(0, p_left, q_left);
 * Vector3 p_right = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
 * Quaternion q_right = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch);
 * gc.contdStrokeQ(0, p_right, q_right);
 * // ^ repead while performing the gesture with your controller.
 * 
 * gc.endStroke(0);
 * gc.endStroke(1);
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
 * gc.setMaxTrainingTime(60); // Set training time to 60 seconds.
 * gc.startTraining();
 * </code>
 * You can stop the training process by calling stopTraining().
 * After training, you can check the gesture identification performance by calling recognitionScore()
 * (a value of 1 means 100% correct recognition).
 * 
 * 
 * (5) Now you can identify new gestures performed by the user in the same way
 * as you were recording samples:
 * <code>
 * Vector3 hmd_p = Camera.main.gameObject.transform.position;
 * Quaternion hmd_q = Camera.main.gameObject.transform.rotation;
 * gc.startStroke(0, hmd_p, hmd_q);
 * gc.startStroke(1, hmd_p, hmd_q);
 * 
 * // repeat the following while performing the gesture with your controller:
 * Vector3 p_left = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch);
 * Quaternion q_left = OVRInput.GetLocalControllerRotation(OVRInput.Controller.LTouch);
 * gc.contdStroke(0, p_left, q_left);
 * Vector3 p_right = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
 * Quaternion q_right = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch);
 * gc.contdStroke(1, p_right, q_right);
 * // ^ repead while performing the gesture with your controller.
 * 
 * gc.endStroke(0);
 * gc.endStroke(1);
 * 
 * int identifiedGestureCombo = gc.identifyGestureCombination();
 * if (identifiedGestureCombo == myFirstCombo) {
 *     // ...
 * }
 * </code>
 * 
 * 
 * (6) Now you can save and load the artificial intelligence.
 * <code>
 * gc.saveToFile("C:/myGestureCombos.dat");
 * // ...
 * gc.loadFromFile("C:/myGestureCombos.dat");
 * </code>
 * 
 * (TroubleShooting)
 * Most of the functions return an integer code on whether they succeeded
 * or if there was any error, which is helpful to find the root cause of possible issues.
 * Here is a list of the possible error codes that functions may return:
 * (0) : Return code for: function executed successfully.
 * (-1) : Return code for: invalid parameter(s) provided to function.
 * (-2) : Return code for: invalid index provided to function.
 * (-3) : Return code for: invalid file path provided to function.
 * (-4) : Return code for: path to an invalid file provided to function.
 * (-5) : Return code for: calculations failed due to numeric instability (too small or too large numbers).
 * (-6) : Return code for: the internal state of the AI was corrupted.
 * (-7) : Return code for: available data (number of samples etc) is insufficient for this operation.
 * (-8) : Return code for: the operation could not be performed because the AI is currently training.
 * (-9) : Return code for: no gestures registered.
 * (-10) : Return code for: the neural network is inconsistent - re-training might solve the issue.
 * (-11) : Return code for: file or object exists and can't be overwritten.
 * (-12) : Return code for: gesture performance (gesture motion, stroke) was not started yet (missing startStroke()).
 * (-13) : Return code for: gesture performance (gesture motion, stroke) was not finished yet (missing endStroke()).
 * (-14) : Return code for: the gesture recognition/combinations object is internally corrupted or inconsistent.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Runtime.InteropServices;
using System.Text;

public class GestureCombinations
{
    //                                                                       ___________________
    //______________________________________________________________________/ FrameOfReference
    /// <summary>
    /// What frame of reference (point of view) is used to interpret data.
    /// </summary>
    public enum FrameOfReference
    {
        Head = 0
        ,
        World = 1
    }
    //                                                          ________________________________
    //_________________________________________________________/    frameOfReferenceYaw
    /// <summary>
    /// Which frame of reference is used to interpret where "front" and "back" are for the
    /// gesture. If the frame of reference is "Head" (default), then "front" will be the
    /// direction in which you look, no matter which direction you look.
    /// If the frame of reference is "World", then "front" will be towards your PC
    /// (or another room-fixed direction, based on your tracking system).
    /// Switch this setting to "World" if your gestures are specific to north/south/east/west
    /// of your world.
    /// </summary>
    public FrameOfReference frameOfReferenceYaw
    {
        get
        {
            return (FrameOfReference)GestureCombinations_getRotationalFrameOfReferenceY(m_gc);
        }
        set
        {
            GestureCombinations_setRotationalFrameOfReferenceY(m_gc, (int)value);
        }
    }
    //                                                          ________________________________
    //_________________________________________________________/  frameOfReferenceUpDownPitch
    /// <summary>
    /// Which frame of reference is used to interpret where "up" and "down" are for the
    /// gesture. If the frame of reference is "Head" (default), then "up" will be the
    /// the visual "up" no matter if you look up or down. If the frame of reference is "World",
    /// then "up" will be towards the sky in the world (direction of the y-axis).
    /// Switch this setting to "World" if your gestures are specific to up/down
    /// in your world.
    /// </summary>
    public FrameOfReference frameOfReferenceUpDownPitch
    {
        get
        {
            return (FrameOfReference)GestureCombinations_getRotationalFrameOfReferenceX(m_gc);
        }
        set
        {
            GestureCombinations_setRotationalFrameOfReferenceX(m_gc, (int)value);
        }
    }
    //                                                          ________________________________
    //_________________________________________________________/   frameOfReferenceRollTilt
    /// <summary>
    /// Which frame of reference is used to interpret head tilting when performing a
    /// gesture. If the frame of reference is "Head" (default), then "right" will be the
    /// the visual "right", even if you tilt your head. If the frame of reference is "World",
    /// then the horizon (world) will be used to determine left/right/up/down.
    /// </summary>
    public FrameOfReference frameOfReferenceRollTilt
    {
        get
        {
            return (FrameOfReference)GestureCombinations_getRotationalFrameOfReferenceZ(m_gc);
        }
        set
        {
            GestureCombinations_setRotationalFrameOfReferenceZ(m_gc, (int)value);
        }
    }
    //                                                          ________________________________
    //_________________________________________________________/  ignoreHeadRotationLeftRight
    /// <summary>
    /// DEPRECATED! Please use frameOfReferenceYaw instead.
    /// Setting whether the horizontal rotation of the users head
    /// (commonly called "pan" or "yaw", looking left or right) should be considered when 
    /// recording and performing gestures.
    /// By default, the users view direction is considered in the calculation of gestures.
    /// So if you turn away from your PC and make a "swipe right" gesture, your visual "right"
    /// is considered, not "the right side of your PC or room".
    /// If you consider gestures to be "room-relative", set this variable to "true".
    /// </summary>
    public bool ignoreHeadRotationLeftRight
    {
        get
        {
            return GestureCombinations_getIgnoreHeadRotationY(m_gc) != 0;
        }
        set
        {
            GestureCombinations_setIgnoreHeadRotationY(m_gc, value ? 1 : 0);
        }
    }
    //                                                          ________________________________
    //_________________________________________________________/   ignoreHeadRotationUpDown
    /// <summary>
    /// DEPRECATED! Please use frameOfReferenceUpDownPitch instead.
    /// Setting whether the vertical rotation of the users head
    /// (commonly called "pitch", looking up or down) should be considered when 
    /// recording and performing gestures.
    /// By default, the users view direction is considered in the calculation of gestures.
    /// So if you look up and make a "pushing" gesture, it is considered to be a "forward"
    /// motion, not an "upward" motion.
    /// If you consider gestures to be "room-relative", set this variable to "true".
    /// </summary>
    public bool ignoreHeadRotationUpDown
    {
        get
        {
            return GestureCombinations_getIgnoreHeadRotationX(m_gc) != 0;
        }
        set
        {
            GestureCombinations_setIgnoreHeadRotationX(m_gc, value ? 1 : 0);
        }
    }
    //                                                          ________________________________
    //_________________________________________________________/   ignoreHeadRotationTilt
    /// <summary>
    /// DEPRECATED! Please use frameOfReferenceRollTilt instead.
    /// Setting whether the tilting rotation of the users head
    /// (also called "roll" or "bank", tilting the head to the site without changing the
    /// view direction) should be considered when recording and performing gestures.
    /// By default, the users view direction is considered in the calculation of gestures.
    /// So if you tilt your head 90 degrees to the side a and make a "swipe right" gesture,
    /// your visual "right" is considered, not "the right side of your PC or room".
    /// If you consider gestures to be "room-relative", set this variable to "true".
    /// </summary>
    public bool ignoreHeadRotationTilt
    {
        get
        {
            return GestureCombinations_getIgnoreHeadRotationZ(m_gc) != 0;
        }
        set
        {
            GestureCombinations_setIgnoreHeadRotationZ(m_gc, value ? 1 : 0);
        }
    }
    //                                                          ________________________________
    //_________________________________________________________/   GestureCombinations()
    /// <summary>
    /// Constructor.
    /// <param name="number_of_parts">The number of parts of which multi-gestures will consist (eg. 2 for bimanual gestures).</param>
    /// </summary>
    public GestureCombinations(int number_of_parts)
    {
        m_gc = GestureCombinations_create(number_of_parts);
    }
    //                                                          ________________________________
    //_________________________________________________________/   ~GestureCombinations()
    /// <summary>
    /// Destructor.
    /// </summary>
    ~GestureCombinations()
    {
        if (m_gc.ToInt64() != 0)
        {
            GestureCombinations_delete(m_gc);
        }
    }
    //                                                          ________________________________
    //_________________________________________________________/         startStroke()
    /// <summary>
    /// Get the number of subgestures / parts / hands used by this multi-gesture object.
    /// </summary>
    /// <returns>
    /// The number of subgestures / parts / hands used by this multi-gesture object.
    /// </returns>
    public int numberOfParts()
    {
        return GestureCombinations_numberOfParts(m_gc);
    }
    //                                                          ________________________________
    //_________________________________________________________/         startStroke()
    /// <summary>
    /// Start a new gesture (stroke) performance.
    /// If record_as_sample is set to a gesture ID, the following gesture performance will be recorded as a
    /// sample of that gesture (sample-recording-mode).
    /// If record_as_sample is not set, the gesture recognition library will attempt to identify the
    /// gesture based on previously recorded samples (gesture-identification-mode).
    /// Note that you must first record samples and train the artificial intelligence (by calling startTraining())
    /// before new and unknown gestures can be identified.
    /// </summary>
    /// <param name="part">The sub-gesture index of the gesture stroke to perform.</param>
    /// <param name="hmd">Current transformation of the VR headset / HMD.
    /// This must be a double[4,4] column-major matrix (where the translational component is in the m[3][*] sub-array).</param>
    /// <param name="record_as_sample">[OPTIONAL] Set this to the ID of a gesture to start recording a sample for that gesture.</param>
    /// <returns>
    /// Zero on success, an error code on failure.
    /// </returns>
    public int startStroke(int part, double[,] hmd, int record_as_sample = -1)
    {
        return GestureCombinations_startStrokeM(m_gc, part, hmd, record_as_sample);
    }
    //                                                          ________________________________
    //_________________________________________________________/         startStroke()
    /// <summary>
    /// Start a new gesture (stroke) performance.
    /// If record_as_sample is set to a gesture ID, the following gesture performance will be recorded as a
    /// sample of that gesture (sample-recording-mode).
    /// If record_as_sample is not set, the gesture recognition library will attempt to identify the
    /// gesture based on previously recorded samples (gesture-identification-mode).
    /// Note that you must first record samples and train the artificial intelligence (by calling startTraining())
    /// before new and unknown gestures can be identified.
    /// </summary>
    /// <param name="part">The sub-gesture index of the gesture stroke to perform.</param>
    /// <param name="hmd_p">The current position of the VR headset (users POV).</param>
    /// <param name="hmd_q">The current rotation of the VR headset (users POV).</param>
    /// <param name="record_as_sample">[OPTIONAL] Set this to the ID of a gesture to start recording a sample for that gesture.</param>
    /// <returns>
    /// Zero on success, an error code on failure.
    /// </returns>
    public int startStroke(int part, Vector3 hmd_p, Quaternion hmd_q, int record_as_sample = -1)
    {
        double[] p = new double[3] { hmd_p.x, hmd_p.y, hmd_p.z };
        double[] q = new double[4] { hmd_q.x, hmd_q.y, hmd_q.z, hmd_q.w };
        return GestureCombinations_startStroke(m_gc, part, p, q, record_as_sample);
    }
    //                                                          ________________________________
    //_________________________________________________________/         startStroke()
    /// <summary>
    /// Start a new gesture (stroke) performance.
    /// If record_as_sample is set to a gesture ID, the following gesture performance will be recorded as a
    /// sample of that gesture (sample-recording-mode).
    /// If record_as_sample is not set, the gesture recognition library will attempt to identify the
    /// gesture based on previously recorded samples (gesture-identification-mode).
    /// Note that you must first record samples and train the artificial intelligence (by calling startTraining())
    /// before new and unknown gestures can be identified.
    /// </summary>
    /// <param name="part">The sub-gesture index of the gesture stroke to perform.</param>
    /// <param name="hmd_p">The current position of the VR headset (users POV). This must be a double[3] array.</param>
    /// <param name="hmd_q">The current rotation of the VR headset (users POV). This must be a double[4] array.</param>
    /// <param name="record_as_sample">[OPTIONAL] Set this to the ID of a gesture to start recording a sample for that gesture.</param>
    /// <returns>
    /// Zero on success, an error code on failure.
    /// </returns>
    public int startStroke(int part, double[] hmd_p, double[] hmd_q, int record_as_sample = -1)
    {
        return GestureCombinations_startStroke(m_gc, part, hmd_p, hmd_q, record_as_sample);
    }
    //                                                          ________________________________
    //_________________________________________________________/         contdStroke()
    /// <summary>
    /// Continue performing a gesture.
    /// </summary>
    /// <param name="part">The sub-gesture index of the gesture stroke to perform.</param>
    /// <param name="p">Position of the controller with which the gesture is performed.</param>
    /// <returns>
    /// Zero on success, an error code on failure.
    /// </returns>
    public int contdStroke(int part, double[] p)
    {
        return GestureCombinations_contdStroke(m_gc, part, p);
    }
    //                                                          ________________________________
    //_________________________________________________________/         contdStroke()
    /// <summary>
    /// Continue performing a gesture.
    /// </summary>
    /// <param name="part">The sub-gesture index of the gesture stroke to perform.</param>
    /// <param name="p">Position of the controller with which the gesture is performed.</param>
    /// <returns>
    /// Zero on success, an error code on failure.
    /// </returns>
    public int contdStroke(int part, Vector3 p)
    {
        double[] _p = new double[3] { p.x, p.y, p.z };
        return GestureCombinations_contdStroke(m_gc, part, _p);
    }
    //                                                          ________________________________
    //_________________________________________________________/         contdStrokeQ()
    /// <summary>
    /// Continue performing a gesture, given rotation in the form of
    /// a quaternion.
    /// </summary>
    /// <param name="part">The sub-gesture index of the gesture stroke to perform.</param>
    /// <param name="p">Position of the controller with which the gesture is performed.</param>
    /// <param name="q">Rotation of the controller with which the gesture is performed.</param>
    /// <returns>
    /// Zero on success, an error code on failure.
    /// </returns>
    public int contdStrokeQ(int part, Vector3 p, Quaternion q)
    {
        double[] _p = new double[3] { p.x, p.y, p.z };
        double[] _q = new double[4] { q.x, q.y, q.z, q.w };
        return GestureCombinations_contdStrokeQ(m_gc, part, _p, _q);
    }
    //                                                          ________________________________
    //_________________________________________________________/          contdStrokeQ()
    /// <summary>
    /// Continue performing a gesture, given rotation in the form of
    /// a quaternion.
    /// </summary>
    /// <param name="part">The sub-gesture index of the gesture stroke to perform.</param>
    /// <param name="p">Position of the controller with which the gesture is performed. This must be a double[3] array.</param>
    /// <param name="q">Rotation of the controller with which the gesture is performed. This must be a double[4] array.</param>
    /// <returns>
    /// Zero on success, an error code on failure.
    /// </returns>
    public int contdStrokeQ(int part, double[] p, double[] q)
    {
        return GestureCombinations_contdStrokeQ(m_gc, part, p, q);
    }
    //                                                          ________________________________
    //_________________________________________________________/         contdStrokeE()
    /// <summary>
    /// Continue performing a gesture, given rotation in the form of
    /// Euler rotation angles in radians.
    /// </summary>
    /// <param name="part">The sub-gesture index of the gesture stroke to perform.</param>
    /// <param name="p">Position of the controller with which the gesture is performed.</param>
    /// <param name="q">Rotation of the controller with which the gesture is performed.</param>
    /// <returns>
    /// Zero on success, an error code on failure.
    /// </returns>
    public int contdStrokeE(int part, Vector3 p, Vector3 r)
    {
        double[] _p = new double[3] { p.x, p.y, p.z };
        double[] _r = new double[3] { r.x, r.y, r.z };
        return GestureCombinations_contdStrokeE(m_gc, part, _p, _r);
    }
    //                                                          ________________________________
    //_________________________________________________________/          contdStrokeE()
    /// <summary>
    /// Continue performing a gesture, given rotation in the form of
    /// Euler rotation angles in radians.
    /// </summary>
    /// <param name="part">The sub-gesture index of the gesture stroke to perform.</param>
    /// <param name="p">Position of the controller with which the gesture is performed. This must be a double[3] array.</param>
    /// <param name="r">Rotation of the controller with which the gesture is performed. This must be a double[3] array.</param>
    /// <returns>
    /// Zero on success, an error code on failure.
    /// </returns>
    public int contdStrokeE(int part, double[] p, double[] r)
    {
        return GestureCombinations_contdStrokeE(m_gc, part, p, r);
    }

    //                                                          ________________________________
    //_________________________________________________________/         contdStrokeM()
    /// <summary>
    /// Continue performing a gesture, given a transformation matrix
    /// combining rotation and translation.
    /// </summary>
    /// <param name="part">The sub-gesture index of the gesture stroke to perform.</param>
    /// <param name="m">Transformation of the controller with which the gesture is performed.
    /// This must be a double[4,4] column-major matrix (where the translational component is in the m[3][*] sub-array).</param>
    /// <returns>
    /// Zero on success, an error code on failure.
    /// </returns>
    public int contdStrokeM(int part, double[,] m)
    {
        return GestureCombinations_contdStrokeM(m_gc, part, m);
    }
    //                                                          ________________________________
    //_________________________________________________________/          endStroke()
    /// <summary>
    /// End a gesture (stroke).
    /// If the gesture was started as recording a new sample (sample-recording-mode), then
    /// the gesture will be added to the reference examples for this gesture.
    /// If the gesture (stroke) was started in identification mode, the gesture recognition
    /// library will store the result internally as the current performance for this sub-gesture.
    /// Use identifyGestureCombination to combine the various sub gesture results into one identification.
    /// </summary>
    /// <param name="part">The sub-gesture index of the gesture stroke to perform.</param>
    /// <param name="pos">[OUT] The position where the gesture was performed.</param>
    /// <param name="scale">[OUT] The scale at which the gesture was performed.</param>
    /// <param name="dir0">[OUT] The primary direction in which the gesture was performed (ie. widest direction).</param>
    /// <param name="dir1">[OUT] The secondary direction in which the gesture was performed.</param>
    /// <param name="dir2">[OUT] The minor direction in which the gesture was performed (ie. narrowest direction).</param>
    /// <returns>
    /// Zero on success, an error code on failure.
    /// </returns>
    public int endStroke(int part, ref Vector3 pos, ref double scale, ref Vector3 dir0, ref Vector3 dir1, ref Vector3 dir2)
    {
        double[] _pos = new double[3];
        double[] _scale = new double[1];
        double[] _dir0 = new double[3];
        double[] _dir1 = new double[3];
        double[] _dir2 = new double[3];
        int ret = GestureCombinations_endStroke(m_gc, part, _pos, _scale, _dir0, _dir1, _dir2);
        pos.x = (float)_pos[0];
        pos.y = (float)_pos[1];
        pos.z = (float)_pos[2];
        scale = _scale[0];
        dir0.x = (float)_dir0[0];
        dir0.y = (float)_dir0[1];
        dir0.z = (float)_dir0[2];
        dir1.x = (float)_dir1[0];
        dir1.y = (float)_dir1[1];
        dir1.z = (float)_dir1[2];
        dir2.x = (float)_dir2[0];
        dir2.y = (float)_dir2[1];
        dir2.z = (float)_dir2[2];
        return ret;
    }
    //                                                          ________________________________
    //_________________________________________________________/          endStroke()
    /// <summary>
    /// End a gesture (stroke).
    /// If the gesture was started as recording a new sample (sample-recording-mode), then
    /// the gesture will be added to the reference examples for this gesture.
    /// If the gesture (stroke) was started in identification mode, the gesture recognition
    /// library will store the result internally as the current performance for this sub-gesture.
    /// Use identifyGestureCombination to combine the various sub gesture results into one identification.
    /// </summary>
    /// <param name="part">The sub-gesture index of the gesture stroke to perform.</param>
    /// <param name="pos">[OUT] The position where the gesture was performed.</param>
    /// <param name="scale">[OUT] The scale at which the gesture was performed.</param>
    /// <param name="dir0">[OUT] The primary direction in which the gesture was performed (ie. widest direction).</param>
    /// <param name="dir1">[OUT] The secondary direction in which the gesture was performed.</param>
    /// <param name="dir2">[OUT] The minor direction in which the gesture was performed (ie. narrowest direction).</param>
    /// <returns>
    /// True on success, false on failure.
    /// </returns>
    public bool endStroke(int part, ref Vector3 pos, ref double scale, ref Quaternion q)
    {
        double[] _pos = new double[3];
        double[] _scale = new double[1];
        double[] _dir0 = new double[3];
        double[] _dir1 = new double[3];
        double[] _dir2 = new double[3];
        int ret = GestureCombinations_endStroke(m_gc, part, _pos, _scale, _dir0, _dir1, _dir2);
        pos.x = (float)_pos[0];
        pos.y = (float)_pos[1];
        pos.z = (float)_pos[2];
        scale = _scale[0];
        double tr = _dir0[0] + _dir1[1] + _dir2[2];
        if (tr > 0)
        {
            double s = Math.Sqrt(tr + 1.0) * 2.0;
            q.w = (float)(0.25 * s);
            q.x = (float)((_dir1[2] - _dir2[1]) / s);
            q.y = (float)((_dir2[0] - _dir0[2]) / s);
            q.z = (float)((_dir0[1] - _dir1[0]) / s);
        }
        else if ((_dir0[0] > _dir1[1]) & (_dir0[0] > _dir2[2]))
        {
            double s = Math.Sqrt(1.0 + _dir0[0] - _dir1[1] - _dir2[2]) * 2.0;
            q.w = (float)((_dir1[2] - _dir2[1]) / s);
            q.x = (float)(0.25 * s);
            q.y = (float)((_dir1[0] + _dir0[1]) / s);
            q.z = (float)((_dir2[0] + _dir0[2]) / s);
        }
        else if (_dir1[1] > _dir2[2])
        {
            double s = Math.Sqrt(1.0 + _dir1[1] - _dir0[0] - _dir2[2]) * 2.0;
            q.w = (float)((_dir2[0] - _dir0[2]) / s);
            q.x = (float)((_dir1[0] + _dir0[1]) / s);
            q.y = (float)(0.25 * s);
            q.z = (float)((_dir2[1] + _dir1[2]) / s);
        }
        else
        {
            double s = Math.Sqrt(1.0 + _dir2[2] - _dir0[0] - _dir1[1]) * 2.0;
            q.w = (float)((_dir0[1] - _dir1[0]) / s);
            q.x = (float)((_dir2[0] + _dir0[2]) / s);
            q.y = (float)((_dir2[1] + _dir1[2]) / s);
            q.z = (float)(0.25 * s);
        }
        return ret != 0;
    }
    //                                                          ________________________________
    //_________________________________________________________/          endStroke()
    /// <summary>
    /// End a gesture (stroke).
    /// If the gesture was started as recording a new sample (sample-recording-mode), then
    /// the gesture will be added to the reference examples for this gesture.
    /// If the gesture (stroke) was started in identification mode, the gesture recognition
    /// library will store the result internally as the current performance for this sub-gesture.
    /// Use identifyGestureCombination to combine the various sub gesture results into one identification.
    /// </summary>
    /// <param name="part">The sub-gesture index of the gesture stroke to perform.</param>
    /// <param name="pos">[OUT] The position where the gesture was performed. This must be a double[3] array.</param>
    /// <param name="scale">[OUT] The scale at which the gesture was performed. This must be a double[1] array.</param>
    /// <param name="dir0">[OUT] The primary direction in which the gesture was performed (ie. widest direction). This must be a double[3] array.</param>
    /// <param name="dir1">[OUT] The secondary direction in which the gesture was performed. This must be a double[3] array.</param>
    /// <param name="dir2">[OUT] The minor direction in which the gesture was performed (ie. narrowest direction). This must be a double[3] array.</param>
    /// <returns>
    /// True on success, false on failure.
    /// </returns>
    public bool endStroke(int part, ref double[] pos, ref double[] scale, ref double[] dir0, ref double[] dir1, ref double[] dir2)
    {
        return GestureCombinations_endStroke(m_gc, part, pos, scale, dir0, dir1, dir2) != 0;
    }
    //                                                          ________________________________
    //_________________________________________________________/          endStroke()
    /// <summary>
    /// End a gesture (stroke).
    /// If the gesture was started as recording a new sample (sample-recording-mode), then
    /// the gesture will be added to the reference examples for this gesture.
    /// If the gesture (stroke) was started in identification mode, the gesture recognition
    /// library will store the result internally as the current performance for this sub-gesture.
    /// Use identifyGestureCombination to combine the various sub gesture results into one identification.
    /// </summary>
    /// <param name="part">The sub-gesture index of the gesture stroke to perform.</param>
    /// <returns>
    /// True on success, false on failure.
    /// </returns>
    public bool endStroke(int part)
    {
        return GestureCombinations_endStroke(m_gc, part, null, null, null, null, null) != 0;
    }
    //                                                      ____________________________________
    //_____________________________________________________/        isStrokeStarted()
    /// <summary>
    /// Query whether a gesture performance (gesture motion, stroke) was started and is currently ongoing.
    /// </summary>
    /// <param name="part">The sub-gesture index of the gesture stroke to perform.</param>
    /// <returns>
    /// True if a gesture motion (stroke) was started and is ongoing, false if not.
    /// </returns>
    public bool isStrokeStarted(int part)
    {
        return GestureCombinations_isStrokeStarted(m_gc, part) != 0;
    }
    //                                                          ________________________________
    //_________________________________________________________/         cancelStroke()
    /// <summary>
    /// Cancel a gesture (stroke).
    /// </summary>
    /// <param name="part">The sub-gesture index of the gesture stroke to perform.</param>
    /// <returns>
    /// Zero on success, an error code on failure.
    /// </returns>
    public int cancelStroke(int part)
    {
        return GestureCombinations_cancelStroke(m_gc, part);
    }
    //                                                          ________________________________
    //_________________________________________________________/    identifyGestureCombination()
    /// <summary>
    /// Calculate the most likely candidate of a multi-gesture (combination) from the last
    /// performed strokes for each of the sub-gestures.
    /// </summary>
    /// <returns>
    /// The ID of the identified multi-gesture. -1 if an error occurred.
    /// </returns>
    public int identifyGestureCombination()
    {
        double[] _similarity = new double[1];
        int gesture_id = GestureCombinations_identifyGestureCombination(m_gc, _similarity);
        return gesture_id;
    }
    //                                                          ________________________________
    //_________________________________________________________/   identifyGestureCombination()
    /// <summary>
    /// Calculate the most likely candidate of a multi-gesture (combination) from the last
    /// performed strokes for each of the sub-gestures.
    /// </summary>
    /// <returns>
    /// The ID of the identified multi-gesture. -1 if an error occurred.
    /// </returns>
    public int identifyGestureCombination(ref double similarity)
    {
        double[] _similarity = new double[1];
        int gesture_id = GestureCombinations_identifyGestureCombination(m_gc, _similarity);
        similarity = _similarity[0];
        return gesture_id;
    }
    //                                                          ________________________________
    //_________________________________________________________/    contdIdentify()
    /// <summary>
    /// Identify gesture while performing it continuously.
    /// </summary>
    /// <param name="hmd_p">The current position of the headset.</param>
    /// <param name="hmd_q">The current rotation/orientation of the headset.</param>
    /// <param name="similarity">[OUT] How similar the gesture is to the recorded gesture samples.</param>
    /// <returns>
    /// The ID of the identified multi-gesture. A negative error code if an error occurred.
    /// </returns>
    public int contdIdentify(Vector3 hmd_p, Quaternion hmd_q, ref double similarity)
    {
        double[] _hmd_p = new double[3] { hmd_p.x, hmd_p.y, hmd_p.z };
        double[] _hmd_q = new double[4] { hmd_q.x, hmd_q.y, hmd_q.z, hmd_q.w };
        double[] _similarity = new double[1];
        int gesture_id = GestureCombinations_contdIdentify(m_gc, _hmd_p, _hmd_q, _similarity);
        similarity = _similarity[0];
        return gesture_id;
    }
    //                                                          ________________________________
    //_________________________________________________________/    contdRecord()
    /// <summary>
    /// Record gesture while performing it continuously.
    /// </summary>
    /// <param name="hmd_p">The current position of the headset.</param>
    /// <param name="hmd_q">The current rotation/orientation of the headset.</param>
    /// <returns>
    /// Zero on success, a negative error code if an error occurred.
    /// </returns>
    public int contdRecord(Vector3 hmd_p, Quaternion hmd_q)
    {
        double[] _hmd_p = new double[3] { hmd_p.x, hmd_p.y, hmd_p.z };
        double[] _hmd_q = new double[4] { hmd_q.x, hmd_q.y, hmd_q.z, hmd_q.w };
        return GestureCombinations_contdRecord(m_gc, _hmd_p, _hmd_q);
    }
    //                                                          ________________________________
    //_________________________________________________________/ getContdIdentificationPeriod()
    /// <summary>
    /// Get the time frame for continuous gesture identification / recording
    /// (via contdIdentift() and contdRecord) in milliseconds.
    /// </summary>
    /// <param name="part">The sub-gesture index of the gesture stroke to perform.</param>
    /// <returns>
    /// Zero on success, a negative error code if an error occurred.
    /// </returns>
    public int getContdIdentificationPeriod(int part)
    {
        return GestureCombinations_getContdIdentificationPeriod(m_gc, part);
    }
    //                                                          ________________________________
    //_________________________________________________________/ setContdIdentificationPeriod()
    /// <summary>
    /// Set the time frame for continuous gesture identification / recording
    /// (via contdIdentift() and contdRecord) in milliseconds.
    /// </summary>
    /// <param name="part">The sub-gesture index of the gesture stroke to perform.</param>
    /// <param name="ms">The preferred gesture duration in milliseconds.</param>
    /// <returns>
    /// Zero on success, a negative error code if an error occurred.
    /// </returns>
    public int setContdIdentificationPeriod(int part, int ms)
    {
        return GestureCombinations_setContdIdentificationPeriod(m_gc, part, ms);
    }
    //                                                       ___________________________________
    //______________________________________________________/ getContdIdentificationSmoothing()
    /// <summary>
    /// Get the number of samples to use for smoothing continuous gesture identification results.
    /// Since continuous gestures can sometimes be "jumpy" (that is, during some phases of the
    /// motion they briefly resemble other gestures) it is advised to set this value between 2
    /// and 5 so that several recent results will be analyzed together to provide a more stable
    /// identification.However, setting this value high means that the AI will take longer to
    /// react when the continuous motion changes from one gesture to another.
    /// </summary>
    /// <param name="part">The sub-gesture index of the gesture stroke to perform.</param>
    /// <returns>
    /// Number of samples to use for smoothing.
    /// </returns>
    public int getContdIdentificationSmoothing(int part)
    {
        return GestureCombinations_getContdIdentificationSmoothing(m_gc, part);
    }
    //                                                      ____________________________________
    //_____________________________________________________/ setContdIdentificationSmoothing()
    /// <summary>
    /// Get the number of samples to use for smoothing continuous gesture identification results.
    /// Since continuous gestures can sometimes be "jumpy" (that is, during some phases of the
    /// motion they briefly resemble other gestures) it is advised to set this value between 2
    /// and 5 so that several recent results will be analyzed together to provide a more stable
    /// identification.However, setting this value high means that the AI will take longer to
    /// react when the continuous motion changes from one gesture to another.
    /// </summary>
    /// <param name="part">The sub-gesture index of the gesture stroke to perform.</param>
    /// <param name="samples">The preferred number of samples for smoothing.</param>
    /// <returns>
    /// Zero on success, a negative error code if an error occurred.
    /// </returns>
    public int setContdIdentificationSmoothing(int part, int samples)
    {
        return GestureCombinations_setContdIdentificationSmoothing(m_gc, part, samples);
    }
    //                                                          ________________________________
    //_________________________________________________________/    numberOfGestureCombinations()
    /// <summary>
    /// Get the number of multi-gestures currently registered in the system.
    /// </summary>
    /// <returns>
    /// The number of multi-gestures currently registered in the system.
    /// </returns>
    public int numberOfGestureCombinations()
    {
        return GestureCombinations_numberOfGestureCombinations(m_gc);
    }
    //                                                          ________________________________
    //_________________________________________________________/    deleteGestureCombination()
    /// <summary>
    /// Delete a registered multi-gesture.
    /// </summary>
    /// <param name="index">The index of the multi-gesture to delete.</param>
    /// <returns>
    /// Zero on success, a negative error code on failure.
    /// </returns>
    public int deleteGestureCombination(int index)
    {
        return GestureCombinations_deleteGestureCombination(m_gc, index);
    }
    //                                                          ________________________________
    //_________________________________________________________/    deleteAllGestureCombinations()
    /// <summary>
    /// Delete all registered multi-gestures.
    /// </summary>
    /// <returns>
    /// Zero on success, a negative error code on failure.
    /// </returns>
    public int deleteAllGestureCombinations()
    {
        return GestureCombinations_deleteAllGestureCombinations(m_gc);
    }
    //                                                          ________________________________
    //_________________________________________________________/    createGestureCombination()
    /// <summary>
    /// Create a new multi-gesture (combination).
    /// </summary>
    /// <param name="name">A name for the multi-gesture to create.</param>
    /// <returns>
    /// The ID (index) of the newly created Multi-Gesture, a negative error code if an error occurred.
    /// </returns>
    public int createGestureCombination(string name)
    {
        return GestureCombinations_createGestureCombination(m_gc, name);
    }
    //                                                          ________________________________
    //_________________________________________________________/  setCombinationPartGesture()
    /// <summary>
    /// Set the sub-gesture IDs of which a multi-gesture consists.
    /// </summary>
    /// <param name="combination_index">The index (ID) of the Multi-Gesture.</param>
    /// <param name="part">The index of the sub-gesture.</param>
    /// <param name="gesture_index">The index of the gesture expected for this sub-gesture.</param>
    /// <returns>
    /// Zero on success, a negative error code on failure.
    /// </returns>
    public int setCombinationPartGesture(int combination_index, int part, int gesture_index)
    {
        return GestureCombinations_setCombinationPartGesture(m_gc, combination_index, part, gesture_index);
    }
    //                                                          ________________________________
    //_________________________________________________________/  getCombinationPartGesture()
    /// <summary>
    /// Get the sub-gesture IDs of which a multi-gesture consists.
    /// </summary>
    /// <param name="combination_index">The index (ID) of the Multi-Gesture.</param>
    /// <param name="part">The index of the sub-gesture.</param>
    /// <returns>
    /// The index of the gesture expected for this sub-gesture. -1 On failure.
    /// </returns>
    public int getCombinationPartGesture(int combination_index, int part)
    {
        return GestureCombinations_getCombinationPartGesture(m_gc, combination_index, part);
    }
    //                                                          ________________________________
    //_________________________________________________________/    getGestureCombinationName()
    /// <summary>
    /// Get the name associated to a multi-gesture. 
    /// </summary>
    /// <param name="index">ID of the multi-gesture to query.</param>
    /// <returns>
    /// Name of the gesture. On failure, an empty string is returned.
    /// </returns>
    public string getGestureCombinationName(int index)
    {
        int strlen = GestureCombinations_getGestureCombinationNameLength(m_gc, index);
        if (strlen <= 0)
        {
            return "";
        }
        StringBuilder sb = new StringBuilder(strlen + 1);
        GestureCombinations_copyGestureCombinationName(m_gc, index, sb, sb.Capacity);
        return sb.ToString();
    }
    //                                                          ________________________________
    //_________________________________________________________/       setGestureCombinationName()
    /// <summary>
    /// Set the name associated with a multi-gesture.
    /// </summary>
    /// <param name="index">ID of the multi-gesture whose name to set.</param>
    /// <param name="name">The new name of the multi-gesture.</param>
    /// <returns>
    /// Zero on success, a negative error code on failure.
    /// </returns>
    public int setGestureCombinationName(int index, string name)
    {
        return GestureCombinations_setGestureCombinationName(m_gc, index, name);
    }
    //                                                          ________________________________
    //_________________________________________________________/      numberOfGestures()
    /// <summary>
    /// Query the number of currently registered gestures.
    /// </summary>
    /// <param name="part">The sub-gesture index of the gesture stroke to perform.</param>
    /// <returns>
    /// The number of gestures currently registered in the library.
    /// </returns>
    public int numberOfGestures(int part)
    {
        return GestureCombinations_numberOfGestures(m_gc, part);
    }
    //                                                          ________________________________
    //_________________________________________________________/         deleteGesture()
    /// <summary>
    /// Delete currently registered gesture.
    /// </summary>
    /// <param name="part">The sub-gesture index of the gesture stroke to perform.</param>
    /// <param name="index">ID of the gesture to delete.</param>
    /// <returns>
    /// Zero on success, a negative error code on failure.
    /// </returns>
    public int deleteGesture(int part, int index)
    {
        return GestureCombinations_deleteGesture(m_gc, part, index);
    }
    //                                                          ________________________________
    //_________________________________________________________/      deleteAllGestures()
    /// <summary>
    /// Delete all currently registered gestures.
    /// </summary>
    /// <param name="part">The sub-gesture index of the gesture stroke to perform.</param>
    /// <returns>
    /// Zero on success, a negative error code on failure.
    /// </returns>
    public int deleteAllGestures(int part)
    {
        return GestureCombinations_deleteAllGestures(m_gc, part);
    }
    //                                                          ________________________________
    //_________________________________________________________/      createGesture()
    /// <summary>
    /// Create a new gesture.
    /// </summary>
    /// <param name="part">The sub-gesture index of the gesture stroke to perform.</param>
    /// <param name="name">Name of the new gesture.</param>
    /// <returns>
    /// ID of the new gesture.
    /// </returns>
    public int createGesture(int part, string name)
    {
        return GestureCombinations_createGesture(m_gc, part, name, IntPtr.Zero);
    }
    //                                                          ________________________________
    //_________________________________________________________/      copyGesture()
    /// <summary>
    /// Create a new gesture.
    /// </summary>
    /// <param name="from_part">The sub-gesture index from which to copy.</param>
    /// <param name="gesture_index">The gesture ID index to copy.</param>
    /// <param name="to_part">The sub-gesture index to which to copy.</param>
    /// <param name="mirror_x">Whether to mirror about the x-axis.</param>
    /// <param name="mirror_y">Whether to mirror about the y-axis.</param>
    /// <param name="mirror_z">Whether to mirror about the z-axis.</param>
    /// <returns>
    /// ID of the new gesture, -1 on failure.
    /// </returns>
    public int copyGesture(int from_part, int from_gesture_index, int to_part, int to_gesture_index, bool mirror_x, bool mirror_y, bool mirror_z)
    {
        return GestureCombinations_copyGesture(m_gc, from_part, from_gesture_index, to_part, to_gesture_index, mirror_x ? 1 : 0, mirror_y ? 1 : 0, mirror_z ? 1 : 0);
    }
    //                                                          ________________________________
    //_________________________________________________________/ subGestureRecognitionScore()
    /// <summary>
    /// Get the current recognition performance
    /// (probabliity to recognize the correct gesture: a value between 0 and 1 where 0 is 0%
    /// and 1 is 100% correct recognition rate).
    /// </summary>
    /// <param name="part">The sub-gesture index of the gesture stroke to perform.</param>
    /// <returns>
    /// The current recognition performance
    /// (probabliity to recognize the correct gesture: a value between 0 and 1 where 0 is 0%
    /// and 1 is 100% correct recognition rate).
    /// </returns>
    public double gestureRecognitionScore(int part)
    {
        return GestureCombinations_gestureRecognitionScore(m_gc, part);
    }
    //                                                          ________________________________
    //_________________________________________________________/      getGestureName()
    /// <summary>
    /// Get the name associated to a gesture. 
    /// </summary>
    /// <param name="part">The sub-gesture index of the gesture stroke to perform.</param>
    /// <param name="index">ID of the gesture to query.</param>
    /// <returns>
    /// Name of the gesture. On failure, an empty string is returned.
    /// </returns>
    public string getGestureName(int part, int index)
    {
        int strlen = GestureCombinations_getGestureNameLength(m_gc, part, index);
        if (strlen <= 0)
        {
            return "";
        }
        StringBuilder sb = new StringBuilder(strlen + 1);
        GestureCombinations_copyGestureName(m_gc, part, index, sb, sb.Capacity);
        return sb.ToString();
    }
    //                                                          ________________________________
    //_________________________________________________________/   getGestureNumberOfSamples()
    /// <summary>
    /// Query the number of samples recorded for one gesture.
    /// </summary>
    /// <param name="part">The sub-gesture index of the gesture stroke to perform.</param>
    /// <param name="index">ID of the gesture to query.</param>
    /// <returns>
    /// Number of samples recorded for this gesture or -1 if the gesture does not exist.
    /// </returns>
    public int getGestureNumberOfSamples(int part, int index)
    {
        return GestureCombinations_getGestureNumberOfSamples(m_gc, part, index);
    }
    //                                                          ________________________________
    //_________________________________________________________/    getGestureSampleLength()
    /// <summary>
    /// Get the number of data points a sample has.
    /// </summary>
    /// <param name="part">The sub-gesture index of the gesture stroke to perform.</param>
    /// <param name="gesture_index">The zero-based index (ID) of the gesture from where to retrieve the sample.</param>
    /// <param name="sample_index">The zero-based index(ID) of the sample to retrieve.</param>
    /// <param name="processed">Whether the raw data points should be retrieved (0) or the processed data points (1).</param>
    /// <returns>
    /// The number of data points on that sample.
    /// </returns>
    public int getGestureSampleLength(int part, int gesture_index, int sample_index, int processed)
    {
        return GestureCombinations_getGestureSampleLength(m_gc, part, gesture_index, sample_index, processed);
    }
    //                                                          ________________________________
    //_________________________________________________________/    getGestureSampleStroke()
    /// <summary>
    /// Retrieve a sample stroke.
    /// </summary>
    /// <param name="part">The sub-gesture index of the gesture stroke to perform.</param>
    /// <param name="gesture_index">The zero-based index (ID) of the gesture from where to retrieve the sample.</param>
    /// <param name="sample_index">The zero-based index(ID) of the sample to retrieve.</param>
    /// <param name="processed">Whether the raw data points should be retrieved (0) or the processed data points (1).</param>
    /// <param name="hmd_p">[OUT] A vector to receive the position of the HMD at the time of the stroke sample recording.</param>
    /// <param name="hmd_q">[OUT] A quaternion to receive the rotation of the HMD at the time of the stroke sample recording.</param>
    /// <param name="p">[OUT] A vector array to receive the positional data points of the stroke / recorded sample.</param>
    /// <param name="q">[OUT] A quaternion array to receive the rotational data points of the stroke / recorded sample.</param>
    /// <returns>
    /// The number of data points on that sample (ie. resulting length of p and q).
    /// </returns>
    public int getGestureSampleStroke(int part, int gesture_index, int sample_index, int processed, ref Vector3 hmd_p, ref Quaternion hmd_q, ref Vector3[] p, ref Quaternion[] q)
    {
        int sample_length = this.getGestureSampleLength(part, gesture_index, sample_index, processed);
        if (sample_length == 0)
        {
            return 0;
        }
        double[] _hmd_p = new double[3];
        double[] _hmd_q = new double[4];
        double[] _p = new double[3 * sample_length];
        double[] _q = new double[4 * sample_length];
        int samples_written = GestureCombinations_getGestureSampleStroke(m_gc, part, gesture_index, sample_index, processed, _hmd_p, _hmd_q, _p, _q, sample_length);
        if (samples_written == 0)
        {
            return 0;
        }
        hmd_p.x = (float)_hmd_p[0];
        hmd_p.y = (float)_hmd_p[1];
        hmd_p.z = (float)_hmd_p[2];
        hmd_q.x = (float)_hmd_q[0];
        hmd_q.y = (float)_hmd_q[1];
        hmd_q.z = (float)_hmd_q[2];
        hmd_q.w = (float)_hmd_q[3];
        p = new Vector3[samples_written];
        q = new Quaternion[samples_written];
        for (int k = 0; k < samples_written; k++)
        {
            p[k].x = (float)_p[k * 3 + 0];
            p[k].y = (float)_p[k * 3 + 1];
            p[k].z = (float)_p[k * 3 + 2];
            q[k].x = (float)_q[k * 4 + 0];
            q[k].y = (float)_q[k * 4 + 1];
            q[k].z = (float)_q[k * 4 + 2];
            q[k].w = (float)_q[k * 4 + 3];
        }
        return samples_written;
    }
    //                                                          ________________________________
    //_________________________________________________________/    getGestureMeanStroke()
    /// <summary>
    /// Retrieve a gesture mean (average over samples).
    /// </summary>
    /// <param name="part">The sub-gesture index of the gesture stroke to perform.</param>
    /// <param name="gesture_index">The zero-based index (ID) of the gesture from where to retrieve the sample.</param>
    /// <param name="p">[OUT] A vector array to receive the positional data points of the stroke / recorded sample.</param>
    /// <param name="q">[OUT] A quaternion array to receive the rotational data points of the stroke / recorded sample.</param>
    /// <param name="hmd_p">[OUT] A vector to receive the average gesture position relative to (ie. as seen by) the headset.</param>
    /// <param name="hmd_q">[OUT] A quaternion to receive the average gesture position rotation to (ie. as seen by) the headset.</param>
    /// <param name="scale">[OUT] The average scale of the gesture.</param>
    /// <returns>
    /// The number of data points on that sample (ie. resulting length of p and q).
    /// </returns>
    public int getGestureMeanStroke(int part, int gesture_index, ref Vector3[] p, ref Quaternion[] q, ref Vector3 hmd_p, ref Quaternion hmd_q, ref float scale)
    {
        int sample_length = GestureCombinations_getGestureMeanLength(m_gc, part, gesture_index);
        if (sample_length == 0)
        {
            return 0;
        }
        double[] _p = new double[3 * sample_length];
        double[] _q = new double[4 * sample_length];
        double[] _hmd_p = new double[3];
        double[] _hmd_q = new double[4];
        double[] _scale = new double[1];
        int samples_written = GestureCombinations_getGestureMeanStroke(m_gc, part, gesture_index, _p, _q, sample_length, _hmd_p, _hmd_q, _scale);
        if (samples_written <= 0)
        {
            return 0;
        }
        hmd_p.x = (float)_hmd_p[0];
        hmd_p.y = (float)_hmd_p[1];
        hmd_p.z = (float)_hmd_p[2];
        hmd_q.x = (float)_hmd_q[0];
        hmd_q.y = (float)_hmd_q[1];
        hmd_q.z = (float)_hmd_q[2];
        hmd_q.w = (float)_hmd_q[3];
        scale = (float)_scale[0];
        p = new Vector3[samples_written];
        q = new Quaternion[samples_written];
        for (int k = 0; k < samples_written; k++)
        {
            p[k].x = (float)_p[k * 3 + 0];
            p[k].y = (float)_p[k * 3 + 1];
            p[k].z = (float)_p[k * 3 + 2];
            q[k].x = (float)_q[k * 4 + 0];
            q[k].y = (float)_q[k * 4 + 1];
            q[k].z = (float)_q[k * 4 + 2];
            q[k].w = (float)_q[k * 4 + 3];
        }
        return samples_written;
    }
    //                                                          ________________________________
    //_________________________________________________________/      deleteGestureSample()
    /// <summary>
    /// Delete a gesture sample recording from the set.
    /// </summary>
    /// <param name="part">The sub-gesture index of the gesture stroke to perform.</param>
    /// <param name="gesture_index">The zero-based index (ID) of the gesture from where to delete the sample.</param>
    /// <param name="sample_index">The zero-based index(ID) of the sample to delete.</param>
    /// <returns>
    /// Zero on success, a negative error code on failure.
    /// </returns>
    public int deleteGestureSample(int part, int gesture_index, int sample_index)
    {
        return GestureCombinations_deleteGestureSample(m_gc, part, gesture_index, sample_index);
    }
    //                                                          ________________________________
    //_________________________________________________________/    deleteAllGestureSamples()
    /// <summary>
    /// Delete all gesture sample recordings from the set.
    /// </summary>
    /// <param name="part">The sub-gesture index of the gesture stroke to perform.</param>
    /// <param name="gesture_index">The zero-based index (ID) of the gesture from where to delete the sample.</param>
    /// <returns>
    /// Zero on success, a negative error code on failure.
    /// </returns>
    public int deleteAllGestureSamples(int part, int gesture_index)
    {
        return GestureCombinations_deleteAllGestureSamples(m_gc, part, gesture_index);
    }
    //                                                          ________________________________
    //_________________________________________________________/       setGestureName()
    /// <summary>
    /// Set the name associated with a gesture.
    /// </summary>
    /// <param name="part">The sub-gesture index of the gesture stroke to perform.</param>
    /// <param name="index">ID of the gesture whose name to set.</param>
    /// <param name="name">The new name of the gesture.</param>
    /// <returns>
    /// Zero on success, a negative error code on failure.
    /// </returns>
    public int setGestureName(int part, int index, string name)
    {
        return GestureCombinations_setGestureName(m_gc, part, index, name);
    }
    //                                                          ________________________________
    //_________________________________________________________/      saveToFile()
    /// <summary>
    /// Save the current artificial intelligence to a file.
    /// </summary>
    /// <param name="path">File system path and filename where to save the file.</param>
    /// <returns>
    /// Zero on success, a negative error code on failure.
    /// </returns>
    public int saveToFile(string path)
    {
        return GestureCombinations_saveToFile(m_gc, path);
    }
    //                                                          ________________________________
    //_________________________________________________________/    loadFromFile()
    /// <summary>
    /// Load a previously saved gesture recognition artificial intelligence from a file.
    /// </summary>
    /// <param name="path">File system path and filename from where to load.</param>
    /// <returns>
    /// Zero on success, a negative error code on failure.
    /// </returns>
    public int loadFromFile(string path)
    {
        return GestureCombinations_loadFromFile(m_gc, path, null);
    }
    //                                                          ________________________________
    //_________________________________________________________/     loadFromBuffer()
    /// <summary>
    /// Load a previously saved gesture recognition artificial intelligence from a byte buffer.
    /// </summary>
    /// <param name="buffer">The byte buffer containing the artificial intelligence.</param>
    /// <returns>
    /// Zero on success, a negative error code on failure.
    /// </returns>
    public int loadFromBuffer(byte[] buffer)
    {
        return GestureCombinations_loadFromBuffer(m_gc, buffer, buffer.Length, null);
    }
    //                                                          ________________________________
    //_________________________________________________________/    importFromFile()
    /// <summary>
    /// Import gestures from a GestureCombinations file.
    /// </summary>
    /// <param name="path">File system path and filename from where to import.</param>
    /// <returns>
    /// Zero on success, a negative error code on failure.
    /// </returns>
    public int importFromFile(string path)
    {
        return GestureCombinations_importFromFile(m_gc, path, null);
    }
    //                                                          ________________________________
    //_________________________________________________________/    importFromBuffer()
    /// <summary>
    /// Import gestures from a GestureCombinations byte buffer.
    /// </summary>
    /// <param name="buffer">The byte buffer from which to import.</param>
    /// <returns>
    /// Zero on success, a negative error code on failure.
    /// </returns>
    public int importFromBuffer(byte[] buffer)
    {
        return GestureCombinations_importFromBuffer(m_gc, buffer, buffer.Length, null);
    }
    //                                                          ________________________________
    //_________________________________________________________/      saveGestureToFile()
    /// <summary>
    /// Save the current artificial intelligence to a file.
    /// </summary>
    /// <param name="part">The sub-gesture index of the gesture stroke to perform.</param>
    /// <param name="path">File system path and filename where to save the file.</param>
    /// <returns>
    /// Zero on success, a negative error code on failure.
    /// </returns>
    public int saveGestureToFile(int part, string path)
    {
        return GestureCombinations_saveGestureToFile(m_gc, part, path);
    }
    //                                                          ________________________________
    //_________________________________________________________/    loadGestureFromFile()
    /// <summary>
    /// Load a previously saved gesture recognition artificial intelligence from a file.
    /// </summary>
    /// <param name="part">The sub-gesture index of the gesture stroke to perform.</param>
    /// <param name="path">File system path and filename from where to load.</param>
    /// <returns>
    /// Zero on success, a negative error code on failure.
    /// </returns>
    public int loadGestureFromFile(int part, string path)
    {
        return GestureCombinations_loadGestureFromFile(m_gc, part, path, null);
    }
    //                                                          ________________________________
    //_________________________________________________________/     loadGestureFromBuffer()
    /// <summary>
    /// Load a previously saved gesture recognition artificial intelligence from a byte buffer.
    /// </summary>
    /// <param name="part">The sub-gesture index of the gesture stroke to perform.</param>
    /// <param name="buffer">The byte buffer containing the artificial intelligence.</param>
    /// <returns>
    /// Zero on success, a negative error code on failure.
    /// </returns>
    public int loadGestureFromBuffer(int part, byte[] buffer)
    {
        return GestureCombinations_loadGestureFromBuffer(m_gc, part, buffer, buffer.Length, null);
    }
    //                                                          ________________________________
    //_________________________________________________________/      startTraining()
    /// <summary>
    /// Start the learning process.
    /// After calling this function the gesture recognition library will attempt to learn
    /// patterns in the previously recorded sample strokes in order to find commonalities
    /// and identify future gestures.
    /// </summary>
    /// <returns>
    /// Zero on success, a negative error code on failure.
    /// </returns>
    public int startTraining()
    {
        return GestureCombinations_startTraining(m_gc);
    }
    //                                                          ________________________________
    //_________________________________________________________/       isTraining()
    /// <summary>
    /// Query whether the gesture recognition library is currently trying to learn
    /// gesture identification.
    /// </summary>
    /// <returns>
    /// True if the gesture recognition library is currently in the learning process,
    /// false if not.
    /// </returns>
    public bool isTraining()
    {
        return GestureCombinations_isTraining(m_gc) != 0;
    }
    //                                                          ________________________________
    //_________________________________________________________/       stopTraining()
    /// <summary>
    /// Stop the learning process.
    /// Due to the asynchronous nature of the learning process ("learning in background")
    /// it may take a moment before the training in the background is actually stopped.
    /// </summary>
    public void stopTraining()
    {
        GestureCombinations_stopTraining(m_gc);
    }
    //                                                          ________________________________
    //_________________________________________________________/     recognitionScore()
    /// <summary>
    /// Get the gesture recognition score of the current artificial intelligence (0~1).
    /// 0 indicates a 0% chance that any gesture will be correctly identifies,
    /// 1 indicates a 100% chance.
    /// </summary>
    public double recognitionScore()
    {
        return GestureCombinations_recognitionScore(m_gc);
    }
    //                                                          ________________________________
    //_________________________________________________________/     getMaxTrainingTime()
    /// <summary>
    /// Get the maximum time used for the learning process (in seconds).
    /// </summary>
    /// <returns>
    /// Maximum time to spend learning (in seconds).
    /// </returns>
    public int getMaxTrainingTime()
    {
        return GestureCombinations_getMaxTrainingTime(m_gc);
    }
    //                                                          ________________________________
    //_________________________________________________________/     setMaxTrainingTime()
    /// <summary>
    /// Set the maximum time used for the learning process (in seconds).
    /// </summary>
    /// <param name="t">Maximum time to spend learning (in seconds).</param>
    public void setMaxTrainingTime(int t)
    {
        GestureCombinations_setMaxTrainingTime(m_gc, t);
    }
    //                                                          ________________________________
    //_________________________________________________________/   setTrainingUpdateCallback()
    /// <summary>
    /// Set a function to be called during the training process.
    /// The function will be called whenever the gesture recognition library was able to
    /// increase its performance.
    /// The callback function will receive the current recognition performance
    /// (probabliity to recognize the correct gesture: a value between 0 and 1 where 0 is 0%
    /// and 1 is 100% correct recognition rate).
    /// </summary>
    /// <param name="cbf">The function to call when the recognition performance could be increased.</param>
    public void setTrainingUpdateCallback(TrainingCallbackFunction cbf)
    {
        GestureCombinations_setTrainingUpdateCallback(m_gc, cbf);
    }
    //                                                      ____________________________________
    //_____________________________________________________/ setTrainingUpdateCallbackMetadata()
    /// <summary>
    /// Set a pointer to a metadata object that will be provided to the
	/// TrainingUpdateCallbackFunction.
    /// </summary>
    /// <param name="metadata">The metadata pointer to provide to the TrainingUpdateCallbackFunction.</param>
    public void setTrainingUpdateCallbackMetadata(IntPtr metadata)
    {
        GestureCombinations_setTrainingUpdateCallbackMetadata(m_gc, metadata);
    }
    //                                                          ________________________________
    //_________________________________________________________/   setTrainingFinishCallback()
    /// <summary>
    /// Set a callback function to be called when the learning process finishes.
    /// The callback function will receive the final recognition performance
    /// (probabliity to recognize the correct gesture: a value between 0 and 1 where 0 is 0%
    /// and 1 is 100% correct recognition rate).
    /// </summary>
    /// <param name="cbf">The function to call when the learning process is finished.</param>
    public void setTrainingFinishCallback(TrainingCallbackFunction cbf)
    {
        GestureCombinations_setTrainingFinishCallback(m_gc, cbf);
    }
    //                                                      ____________________________________
    //_____________________________________________________/ setTrainingFinishCallbackMetadata()
    /// <summary>
    /// Set a pointer to a metadata object that will be provided to the
	/// TrainingFinishCallbackFunction.
    /// </summary>
    /// <param name="metadata">The metadata pointer to provide to the TrainingFinishCallbackFunction.</param>
    public void setTrainingFinishCallbackMetadata(IntPtr metadata)
    {
        GestureCombinations_setTrainingFinishCallbackMetadata(m_gc, metadata);
    }


    // ----------------------------------------------------------------------------------------------------------
    // Internal wrapper functions to the plug-in
    // ----------------------------------------------------------------------------------------------------------
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr MetadataCreatorFunction();
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void TrainingCallbackFunction(double performace, IntPtr metadata);

    public const string libfile = "gesturerecognition";

    [DllImport(libfile, EntryPoint = "GestureCombinations_create", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr GestureCombinations_create(int number_of_parts); //!< Create new instance.
    [DllImport(libfile, EntryPoint = "GestureCombinations_delete", CallingConvention = CallingConvention.Cdecl)]
    public static extern void GestureCombinations_delete(IntPtr gco); //!< Delete instance.
    [DllImport(libfile, EntryPoint = "GestureCombinations_numberOfParts", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureCombinations_numberOfParts(IntPtr gco);
    [DllImport(libfile, EntryPoint = "GestureCombinations_startStroke", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureCombinations_startStroke(IntPtr gco, int part, double[] hmd_p, double[] hmd_q, int record_as_sample); //!< Start new stroke.
    [DllImport(libfile, EntryPoint = "GestureCombinations_startStrokeM", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureCombinations_startStrokeM(IntPtr gco, int part, double[,] hmd, int record_as_sample); //!< Start new stroke.
    [DllImport(libfile, EntryPoint = "GestureCombinations_contdStroke", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureCombinations_contdStroke(IntPtr gco, int part, double[] p); //!< Continue stroke data input.
    [DllImport(libfile, EntryPoint = "GestureCombinations_contdStrokeQ", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureCombinations_contdStrokeQ(IntPtr gco, int part, double[] p, double[] q); //!< Continue stroke data input.
    [DllImport(libfile, EntryPoint = "GestureCombinations_contdStrokeE", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureCombinations_contdStrokeE(IntPtr gco, int part, double[] p, double[] r); //!< Continue stroke data input.
    [DllImport(libfile, EntryPoint = "GestureCombinations_contdStrokeM", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureCombinations_contdStrokeM(IntPtr gco, int part, double[,] m); //!< Continue stroke data input.
    [DllImport(libfile, EntryPoint = "GestureCombinations_endStroke", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureCombinations_endStroke(IntPtr gco, int part, double[] pos, double[] scale, double[] dir0, double[] dir1, double[] dir2); //!< End the stroke and identify the gesture.
    [DllImport(libfile, EntryPoint = "GestureCombinations_isStrokeStarted", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureCombinations_isStrokeStarted(IntPtr gco, int part);
    [DllImport(libfile, EntryPoint = "GestureCombinations_cancelStroke", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureCombinations_cancelStroke(IntPtr gco, int part);
    [DllImport(libfile, EntryPoint = "GestureCombinations_identifyGestureCombination", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureCombinations_identifyGestureCombination(IntPtr gco, double[] similarity); //!< Return the most likely gesture candidate for the previous gesture combination.
    [DllImport(libfile, EntryPoint = "GestureCombinations_contdIdentify", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureCombinations_contdIdentify(IntPtr gco, double[] hmd_p, double[] hmd_q, double[] similarity);
    [DllImport(libfile, EntryPoint = "GestureCombinations_contdRecord", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureCombinations_contdRecord(IntPtr gco, double[] hmd_p, double[] hmd_q);
    [DllImport(libfile, EntryPoint = "GestureCombinations_getContdIdentificationPeriod", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureCombinations_getContdIdentificationPeriod(IntPtr gco, int part);
    [DllImport(libfile, EntryPoint = "GestureCombinations_setContdIdentificationPeriod", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureCombinations_setContdIdentificationPeriod(IntPtr gco, int part, int ms);
    [DllImport(libfile, EntryPoint = "GestureCombinations_getContdIdentificationSmoothing", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureCombinations_getContdIdentificationSmoothing(IntPtr gco, int part);
    [DllImport(libfile, EntryPoint = "GestureCombinations_setContdIdentificationSmoothing", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureCombinations_setContdIdentificationSmoothing(IntPtr gco, int part, int samples);
    [DllImport(libfile, EntryPoint = "GestureCombinations_numberOfGestures", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureCombinations_numberOfGestures(IntPtr gco, int part); //!< Get the number of gestures currently recorded in the system.
    [DllImport(libfile, EntryPoint = "GestureCombinations_deleteGesture", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureCombinations_deleteGesture(IntPtr gco, int part, int index); //!< Delete the recorded gesture with the specified index.
    [DllImport(libfile, EntryPoint = "GestureCombinations_deleteAllGestures", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureCombinations_deleteAllGestures(IntPtr gco, int part); //!< Delete recorded gestures.
    [DllImport(libfile, EntryPoint = "GestureCombinations_createGesture", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureCombinations_createGesture(IntPtr gco, int part, string name, IntPtr metadata); //!< Create new gesture.
    [DllImport(libfile, EntryPoint = "GestureCombinations_copyGesture", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureCombinations_copyGesture(IntPtr gco, int from_part, int from_gesture_index, int to_part, int to_gesture_index, int mirror_x, int mirror_y, int mirror_z); //!< Copy gesture from one part/side to another.
    [DllImport(libfile, EntryPoint = "GestureCombinations_gestureRecognitionScore", CallingConvention = CallingConvention.Cdecl)]
    public static extern double GestureCombinations_gestureRecognitionScore(IntPtr gco, int part); //!< Get the gesture recognition score of the current artificial intelligence (0~1).
    // [DllImport(libfile, EntryPoint = "GestureCombinations_getGestureName", CallingConvention = CallingConvention.Cdecl)]
    // public static extern char* GestureCombinations_getGestureName(IntPtr gco, int part, int index); //!< Get the name of a registered gesture.
    [DllImport(libfile, EntryPoint = "GestureCombinations_getGestureNameLength", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureCombinations_getGestureNameLength(IntPtr gco, int part, int index); //!< Get the length of the name of a registered gesture.
    [DllImport(libfile, EntryPoint = "GestureCombinations_copyGestureName", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureCombinations_copyGestureName(IntPtr gco, int part, int index, StringBuilder buf, int buflen); //!< Copy the name of a registered gesture to a buffer.
    [DllImport(libfile, EntryPoint = "GestureCombinations_getGestureMetadata", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr GestureCombinations_getGestureMetadata(IntPtr gco, int part, int index); //!< Get the command of a registered gesture.
    [DllImport(libfile, EntryPoint = "GestureCombinations_getGestureNumberOfSamples", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureCombinations_getGestureNumberOfSamples(IntPtr gco, int part, int index); //!< Get the number of recorded samples of a registered gesture.
    [DllImport(libfile, EntryPoint = "GestureCombinations_getGestureSampleLength", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureCombinations_getGestureSampleLength(IntPtr gco, int part, int gesture_index, int sample_index, int processed); //!< Get the number of data points a sample has.
    [DllImport(libfile, EntryPoint = "GestureCombinations_getGestureSampleStroke", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureCombinations_getGestureSampleStroke(IntPtr gco, int part, int gesture_index, int sample_index, int processed, double[] hmd_p, double[] hmd_q, double[] p, double[] q, int stroke_buf_size); //!< Retrieve a sample stroke.
    [DllImport(libfile, EntryPoint = "GestureCombinations_getGestureMeanLength", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureCombinations_getGestureMeanLength(IntPtr gco, int part, int gesture_index); //!< Get the number of samples of the gesture mean (average over samples).
    [DllImport(libfile, EntryPoint = "GestureCombinations_getGestureMeanStroke", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureCombinations_getGestureMeanStroke(IntPtr gco, int part, int gesture_index, double[] p, double[] q, int stroke_buf_size, double[] hmd_p, double[] hmd_q, double[] scale); //!< Retrieve a gesture mean (average over samples).
    [DllImport(libfile, EntryPoint = "GestureCombinations_deleteGestureSample", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureCombinations_deleteGestureSample(IntPtr gco, int part, int gesture_index, int sample_index); //!< Delete a gesture sample recording from the set.
    [DllImport(libfile, EntryPoint = "GestureCombinations_deleteAllGestureSamples", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureCombinations_deleteAllGestureSamples(IntPtr gco, int part, int gesture_index); //!< Delete all gesture sample recordings from the set.
    [DllImport(libfile, EntryPoint = "GestureCombinations_setGestureName", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureCombinations_setGestureName(IntPtr gco, int part, int index, string name); //!< Set the name of a registered gesture.
    [DllImport(libfile, EntryPoint = "GestureCombinations_setGestureMetadata", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureCombinations_setGestureMetadata(IntPtr gco, int part, int index, IntPtr metadata); //!< Set the command of a registered gesture.
    [DllImport(libfile, EntryPoint = "GestureCombinations_saveToFile", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureCombinations_saveToFile(IntPtr gco, string path); //!< Save the artificial intelligence and recorded training data to file.
    [DllImport(libfile, EntryPoint = "GestureCombinations_loadFromFile", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureCombinations_loadFromFile(IntPtr gco, string path, MetadataCreatorFunction createMetadata); //!< Load the artificial intelligence and recorded training data from file.
    [DllImport(libfile, EntryPoint = "GestureCombinations_loadFromBuffer", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureCombinations_loadFromBuffer(IntPtr gco, byte[] buffer, int buffer_size, MetadataCreatorFunction createMetadata); //!< Load the artificial intelligence and recorded training data buffer.
    [DllImport(libfile, EntryPoint = "GestureCombinations_importFromFile", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureCombinations_importFromFile(IntPtr gco, string path, MetadataCreatorFunction createMetadata); //!< Import gestures from GestureRecognition file.
    [DllImport(libfile, EntryPoint = "GestureCombinations_importFromBuffer", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureCombinations_importFromBuffer(IntPtr gco, byte[] buffer, int buffer_size, MetadataCreatorFunction createMetadata); //!< Import gestures from GestureRecognition data buffer.
    [DllImport(libfile, EntryPoint = "GestureCombinations_saveGestureToFile", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureCombinations_saveGestureToFile(IntPtr gco, int part, string path); //!< Save the artificial intelligence and recorded training data to file.
    [DllImport(libfile, EntryPoint = "GestureCombinations_loadGestureFromFile", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureCombinations_loadGestureFromFile(IntPtr gco, int part, string path, MetadataCreatorFunction createMetadata); //!< Load the artificial intelligence and recorded training data from file.
    [DllImport(libfile, EntryPoint = "GestureCombinations_loadGestureFromBuffer", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureCombinations_loadGestureFromBuffer(IntPtr gco, int part, byte[] buffer, int buffer_size, MetadataCreatorFunction createMetadata); //!< Load the artificial intelligence and recorded training data buffer.
    [DllImport(libfile, EntryPoint = "GestureCombinations_numberOfGestureCombinations", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureCombinations_numberOfGestureCombinations(IntPtr gco); //!< Get the number of gestures currently recorded in the part's system.
    [DllImport(libfile, EntryPoint = "GestureCombinations_deleteGestureCombination", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureCombinations_deleteGestureCombination(IntPtr gco, int index); //!< Delete the recorded gesture with the specified index.
    [DllImport(libfile, EntryPoint = "GestureCombinations_deleteAllGestureCombinations", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureCombinations_deleteAllGestureCombinations(IntPtr gco); //!< Delete recorded gestures.
    [DllImport(libfile, EntryPoint = "GestureCombinations_createGestureCombination", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureCombinations_createGestureCombination(IntPtr gco, string name); //!< Create new gesture.
    [DllImport(libfile, EntryPoint = "GestureCombinations_setCombinationPartGesture", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureCombinations_setCombinationPartGesture(IntPtr gco, int combination_index, int part, int gesture_index); //!< Set which gesture this multi-gesture expects for step i.
    [DllImport(libfile, EntryPoint = "GestureCombinations_getCombinationPartGesture", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureCombinations_getCombinationPartGesture(IntPtr gco, int combination_index, int part); //!< Get which gesture this multi-gesture expects for step i.
    // [DllImport(libfile, EntryPoint = "GestureCombinations_getGestureCombinationName", CallingConvention = CallingConvention.Cdecl)]
    // const char* GestureCombinations_getGestureCombinationName(IntPtr gco, int index); //!< Get the name of a registered multi-gesture.
    [DllImport(libfile, EntryPoint = "GestureCombinations_getGestureCombinationNameLength", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureCombinations_getGestureCombinationNameLength(IntPtr gco, int index); //!< Get the length of the name of a registered multi-gesture.
    [DllImport(libfile, EntryPoint = "GestureCombinations_copyGestureCombinationName", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureCombinations_copyGestureCombinationName(IntPtr gco, int index, StringBuilder buf, int buflen); //!< Copy the name of a registered multi-gesture to a buffer.
    [DllImport(libfile, EntryPoint = "GestureCombinations_setGestureCombinationName", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureCombinations_setGestureCombinationName(IntPtr gco, int index, string name); //!< Set the name of a registered multi-gesture.
    [DllImport(libfile, EntryPoint = "GestureCombinations_startTraining", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureCombinations_startTraining(IntPtr gco); //!< Start train the artificial intelligence based on the the currently collected data.
    [DllImport(libfile, EntryPoint = "GestureCombinations_isTraining", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureCombinations_isTraining(IntPtr gco); //!< Whether the artificial intelligence is currently training.
    [DllImport(libfile, EntryPoint = "GestureCombinations_stopTraining", CallingConvention = CallingConvention.Cdecl)]
    public static extern void GestureCombinations_stopTraining(IntPtr gco); //!< Stop the training process (last best result will be used).
    [DllImport(libfile, EntryPoint = "GestureCombinations_recognitionScore", CallingConvention = CallingConvention.Cdecl)]
    public static extern double GestureCombinations_recognitionScore(IntPtr gco); //!< Get the gesture recognition score of the current artificial intelligence (0~1).
    [DllImport(libfile, EntryPoint = "GestureCombinations_getMaxTrainingTime", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureCombinations_getMaxTrainingTime(IntPtr gco); //!< Get maximum training time in seconds.
    [DllImport(libfile, EntryPoint = "GestureCombinations_setMaxTrainingTime", CallingConvention = CallingConvention.Cdecl)]
    public static extern void GestureCombinations_setMaxTrainingTime(IntPtr gco, int t); //!< Set maximum training time in seconds.
    [DllImport(libfile, EntryPoint = "GestureCombinations_setTrainingUpdateCallback", CallingConvention = CallingConvention.Cdecl)]
    public static extern void GestureCombinations_setTrainingUpdateCallback(IntPtr gco, TrainingCallbackFunction cbf); //!< Set callback function to be called during training.
    [DllImport(libfile, EntryPoint = "GestureCombinations_setTrainingFinishCallback", CallingConvention = CallingConvention.Cdecl)]
    public static extern void GestureCombinations_setTrainingFinishCallback(IntPtr gco, TrainingCallbackFunction cbf); //!< Set callback function to be called when training is finished.
    [DllImport(libfile, EntryPoint = "GestureCombinations_setTrainingUpdateCallbackMetadata", CallingConvention = CallingConvention.Cdecl)]
    public static extern void GestureCombinations_setTrainingUpdateCallbackMetadata(IntPtr gco, IntPtr metadata); //!< Set callback function to be called during training.
    [DllImport(libfile, EntryPoint = "GestureCombinations_setTrainingFinishCallbackMetadata", CallingConvention = CallingConvention.Cdecl)]
    public static extern void GestureCombinations_setTrainingFinishCallbackMetadata(IntPtr gco, IntPtr metadata); //!< Set callback function to be called when training is finished.
    [DllImport(libfile, EntryPoint = "GestureCombinations_getIgnoreHeadRotationX", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureCombinations_getIgnoreHeadRotationX(IntPtr gco); //!< Get whether the horizontal rotation of the users head (commonly called "pan" or "yaw", looking left or right) should be considered when recording and performing gestures.
    [DllImport(libfile, EntryPoint = "GestureCombinations_setIgnoreHeadRotationX", CallingConvention = CallingConvention.Cdecl)]
    public static extern void GestureCombinations_setIgnoreHeadRotationX(IntPtr gco, int on_off); //!< Set whether the horizontal rotation of the users head (commonly called "pan" or "yaw", looking left or right) should be considered when recording and performing gestures.
    [DllImport(libfile, EntryPoint = "GestureCombinations_getIgnoreHeadRotationY", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureCombinations_getIgnoreHeadRotationY(IntPtr gco); //!< Get whether the vertical rotation of the users head (commonly called "pitch", looking up or down) should be considered when recording and performing gestures.
    [DllImport(libfile, EntryPoint = "GestureCombinations_setIgnoreHeadRotationY", CallingConvention = CallingConvention.Cdecl)]
    public static extern void GestureCombinations_setIgnoreHeadRotationY(IntPtr gco, int on_off); //!< Set whether the vertical rotation of the users head (commonly called "pitch", looking up or down) should be considered when recording and performing gestures.
    [DllImport(libfile, EntryPoint = "GestureCombinations_getIgnoreHeadRotationZ", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureCombinations_getIgnoreHeadRotationZ(IntPtr gco); //!< Get whether the tilting rotation of the users head (also called "roll" or "bank", tilting the head to the site without changing the view direction) should be considered when recording and performing gestures.
    [DllImport(libfile, EntryPoint = "GestureCombinations_setIgnoreHeadRotationZ", CallingConvention = CallingConvention.Cdecl)]
    public static extern void GestureCombinations_setIgnoreHeadRotationZ(IntPtr gco, int on_off); //!< Set whether the tilting rotation of the users head (also called "roll" or "bank", tilting the head to the site without changing the view direction) should be considered when recording and performing gestures.
    [DllImport(libfile, EntryPoint = "GestureCombinations_getRotationalFrameOfReferenceX", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureCombinations_getRotationalFrameOfReferenceX(IntPtr gco); //!< Get whether the horizontal rotation of the users head (commonly called "pan" or "yaw", looking left or right) should be considered when recording and performing gestures.
    [DllImport(libfile, EntryPoint = "GestureCombinations_setRotationalFrameOfReferenceX", CallingConvention = CallingConvention.Cdecl)]
    public static extern void GestureCombinations_setRotationalFrameOfReferenceX(IntPtr gco, int i); //!< Set whether the horizontal rotation of the users head (commonly called "pan" or "yaw", looking left or right) should be considered when recording and performing gestures.
    [DllImport(libfile, EntryPoint = "GestureCombinations_getRotationalFrameOfReferenceY", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureCombinations_getRotationalFrameOfReferenceY(IntPtr gco); //!< Get whether the vertical rotation of the users head (commonly called "pitch", looking up or down) should be considered when recording and performing gestures.
    [DllImport(libfile, EntryPoint = "GestureCombinations_setRotationalFrameOfReferenceY", CallingConvention = CallingConvention.Cdecl)]
    public static extern void GestureCombinations_setRotationalFrameOfReferenceY(IntPtr gco, int i); //!< Set whether the vertical rotation of the users head (commonly called "pitch", looking up or down) should be considered when recording and performing gestures.
    [DllImport(libfile, EntryPoint = "GestureCombinations_getRotationalFrameOfReferenceZ", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureCombinations_getRotationalFrameOfReferenceZ(IntPtr gco); //!< Get whether the tilting rotation of the users head (also called "roll" or "bank", tilting the head to the site without changing the view direction) should be considered when recording and performing gestures.
    [DllImport(libfile, EntryPoint = "GestureCombinations_setRotationalFrameOfReferenceZ", CallingConvention = CallingConvention.Cdecl)]
    public static extern void GestureCombinations_setRotationalFrameOfReferenceZ(IntPtr gco, int i); //!< Set whether the tilting rotation of the users head (also called "roll" or "bank", tilting the head to the site without changing the view direction) should be considered when recording and performing gestures.

    private IntPtr m_gc;
}
