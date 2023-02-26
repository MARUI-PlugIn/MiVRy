﻿/*
 * MiVRy - 3D gesture recognition library.
 * Version 2.7
 * Copyright (c) 2023 MARUI-PlugIn (inc.)
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
 * # HOW TO USE:
 * 
 * (1) Place the gesturerecognition.dll (Windows) and/or
 * libgesturerecognition.so (Android / MobileVR) files 
 * in the /Assets/Plugins/ folder in your unity project
 * and add the GestureRecognition.cs file to your project scripts. 
 * 
 * 
 * (2) Create a new Gesture recognition object and register the gestures that you want to identify later.
 * <code>
 * GestureRecognition gr = new GestureRecognition();
 * int myFirstGesture = gr.createGesture("my first gesture");
 * int mySecondGesture = gr.createGesture("my second gesture");
 * </code>
 * 
 * 
 * (3) Record a number of samples for each gesture by calling startStroke(), contdStroke() and endStroke()
 * for your registered gestures, each time inputting the headset and controller transformation.
 * <code>
 * Vector3 hmd_p = Camera.main.gameObject.transform.position;
 * Quaternion hmd_q = Camera.main.gameObject.transform.rotation;
 * gr.startStroke(hmd_p, hmd_q, myFirstGesture);
 * 
 * // repeat the following while performing the gesture with your controller:
 * Vector3 p = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
 * Quaternion q = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch);
 * gr.contdStrokeQ(p,q);
 * // ^ repead while performing the gesture with your controller.
 * 
 * gr.endStroke();
 * </code>
 * Repeat this multiple times for each gesture you want to identify.
 * We recommend recording at least 20 samples for each gesture.
 * 
 * 
 * (4) Start the training process by calling startTraining().
 * You can optionally register callback functions to receive updates on the learning progress
 * by calling setTrainingUpdateCallback() and setTrainingFinishCallback().
 * <code>
 * gr.setMaxTrainingTime(10); // Set training time to 10 seconds.
 * gr.startTraining();
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
 * gr.startStroke(hmd_p, hmd_q);
 * 
 * // repeat the following while performing the gesture with your controller:
 * Vector3 p = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
 * Quaternion q = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch);
 * gr.contdStroke(p,q);
 * // ^ repeat while performing the gesture with your controller.
 * 
 * int identifiedGesture = gr.endStroke();
 * if (identifiedGesture == myFirstGesture) {
 *     // ...
 * }
 * </code>
 * 
 * 
 * (6) Now you can save and load the artificial intelligence.
 * <code>
 * gr.saveToFile("C:/myGestures.dat");
 * // ...
 * gr.loadFromFile("C:/myGestures.dat");
 * </code>
 * 
 * (TroubleShooting)
 * Most of the functions return an integer code on whether they succeeded
 * or if there was any error, which is helpful to find the root cause of possible issues.
 * Here is a list of the possible error codes that functions may return:
 * (0) : Return code for: function executed successfully.
 * (-1) : Return code for: No gesture (or combination) matches.
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
 * (-15) : The operation could not be performed because the AI is loading a gesture database file.
 * (-16) : The provided license key is not valid or the operation is not permitted under the current license.
 * (-17) : Return code for: the operation could not be performed because the AI is currently being saved to database file.
 * (-18) : Return code for: invalid parameter(s) provided to function.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Runtime.InteropServices;
using System.Text;

public class GestureRecognition
{
    //                                                                       ___________________
    //______________________________________________________________________/ getErrorMessage()
    /// <summary>
    /// Get a descriptive string of an error code returned by a function.
    /// </summary>
    /// <param name="errorCode">The error code retuned by a function.</param>
    /// <returns>A descriptive string explaining the error code.</returns>
    public static string getErrorMessage(int errorCode)
    {
        switch (errorCode)
        {
            case 0:
                return "Function executed successfully.";
            case -1:
                return "No gesture (or combination) matches.";
            case -2:
                return "Invalid index provided to function.";
            case -3:
                return "Invalid file path provided to function.";
            case -4:
                return "Path to an invalid file provided to function.";
            case -5:
                return "Calculations failed due to numeric instability(too small or too large numbers).";
            case -6:
                return "The internal state of the AI was corrupted.";
            case -7:
                return "Available data(number of samples etc) is insufficient for this operation.";
            case -8:
                return "The operation could not be performed because the AI is currently training.";
            case -9:
                return "No gestures registered.";
            case -10:
                return "The neural network is inconsistent - re-training might solve the issue.";
            case -11:
                return "File or object exists and can't be overwritten.";
            case -12:
                return "Gesture performance (gesture motion, stroke) was not started yet (missing startStroke()).";
            case -13:
                return "Gesture performance (gesture motion, stroke) was not finished yet (missing endStroke()).";
            case -14:
                return "The gesture recognition/combinations object is internally corrupted or inconsistent.";
            case -15:
                return "The operation could not be performed because the AI is loading a gesture database file.";
            case -16:
                return "The provided license key is not valid or the operation is not permitted under the current license.";
            case -17:
                return "The operation could not be performed because the AI currently being saved to a database file.";
            case -18:
                return "Invalid parameter(s) provided to function.";
        }
        return "Unknown error.";
    }
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
    //_________________________________________________________/    frameOfReferenceY
    /// <summary>
    /// Whether or not to use the heads y-axis-rotation when calculating the relative
    /// controller position (as seen from the user's point of view). In Unity, this means
    /// which frame of reference is used to interpret where "front" and "back" are for the
    /// gesture. If the frame of reference is "Head" (default), then "front" will be the
    /// direction in which you look, no matter which direction you look.
    /// If the frame of reference is "World", then "front" will be towards your PC
    /// (or another room-fixed direction, based on your tracking system).
    /// Switch this setting to "World" if your gestures are specific to north/south/east/west
    /// of your world.
    /// </summary>
    public FrameOfReference frameOfReferenceY
    {
        get
        {
            return (FrameOfReference)GestureRecognition_getRotationalFrameOfReferenceY(m_gro);
        }
        set
        {
            GestureRecognition_setRotationalFrameOfReferenceY(m_gro, (int)value);
        }
    }
    //                                                          ________________________________
    //_________________________________________________________/  frameOfReferenceX
    /// <summary>
    /// Whether or not to use the heads x-axis-rotation when calculating the relative
    /// controller position (as seen from the user's point of view). In Unity, this means
    /// which frame of reference is used to interpret where "up" and "down" are for the
    /// gesture. If the frame of reference is "Head" (default), then "up" will be the
    /// the visual "up" no matter if you look up or down. If the frame of reference is "World",
    /// then "up" will be towards the sky in the world (direction of the y-axis).
    /// Switch this setting to "World" if your gestures are specific to up/down
    /// in your world.
    /// </summary>
    public FrameOfReference frameOfReferenceX
    {
        get
        {
            return (FrameOfReference)GestureRecognition_getRotationalFrameOfReferenceX(m_gro);
        }
        set
        {
            GestureRecognition_setRotationalFrameOfReferenceX(m_gro, (int)value);
        }
    }
    //                                                          ________________________________
    //_________________________________________________________/   frameOfReferenceZ
    /// <summary>
    /// Whether or not to use the heads z-axis-rotation when calculating the relative
    /// controller position (as seen from the user's point of view). In Unity, this means
    /// which frame of reference is used to interpret head tilting when performing a
    /// gesture. If the frame of reference is "Head" (default), then "right" will be the
    /// the visual "right", even if you tilt your head. If the frame of reference is "World",
    /// then the horizon (world) will be used to determine left/right/up/down.
    /// in your world.
    /// </summary>
    public FrameOfReference frameOfReferenceZ
    {
        get
        {
            return (FrameOfReference)GestureRecognition_getRotationalFrameOfReferenceZ(m_gro);
        }
        set
        {
            GestureRecognition_setRotationalFrameOfReferenceZ(m_gro, (int)value);
        }
    }

    //                                                          ________________________________
    //_________________________________________________________/    frameOfReferenceYaw
    /// <summary>
    /// DEPRECATED! Please use frameOfReferenceY instead.
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
            return (FrameOfReference)GestureRecognition_getRotationalFrameOfReferenceY(m_gro);
        }
        set
        {
            GestureRecognition_setRotationalFrameOfReferenceY(m_gro, (int)value);
        }
    }
    //                                                          ________________________________
    //_________________________________________________________/  frameOfReferenceUpDownPitch
    /// <summary>
    /// DEPRECATED! Please use frameOfReferenceX instead.
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
            return (FrameOfReference)GestureRecognition_getRotationalFrameOfReferenceX(m_gro);
        }
        set
        {
            GestureRecognition_setRotationalFrameOfReferenceX(m_gro, (int)value);
        }
    }
    //                                                          ________________________________
    //_________________________________________________________/   frameOfReferenceRollTilt
    /// <summary>
    /// DEPRECATED! Please use frameOfReferenceZ instead.
    /// Which frame of reference is used to interpret head tilting when performing a
    /// gesture. If the frame of reference is "Head" (default), then "right" will be the
    /// the visual "right", even if you tilt your head. If the frame of reference is "World",
    /// then the horizon (world) will be used to determine left/right/up/down.
    /// in your world.
    /// </summary>
    public FrameOfReference frameOfReferenceRollTilt
    {
        get
        {
            return (FrameOfReference)GestureRecognition_getRotationalFrameOfReferenceZ(m_gro);
        }
        set
        {
            GestureRecognition_setRotationalFrameOfReferenceZ(m_gro, (int)value);
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
            return GestureRecognition_getIgnoreHeadRotationY(m_gro) != 0;
        }
        set
        {
            GestureRecognition_setIgnoreHeadRotationY(m_gro, value ? 1 : 0);
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
            return GestureRecognition_getIgnoreHeadRotationX(m_gro) != 0;
        }
        set
        {
            GestureRecognition_setIgnoreHeadRotationX(m_gro, value ? 1 : 0);
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
            return GestureRecognition_getIgnoreHeadRotationZ(m_gro) != 0;
        }
        set
        {
            GestureRecognition_setIgnoreHeadRotationZ(m_gro, value ? 1 : 0);
        }
    }
    //                                                                       ___________________
    //______________________________________________________________________/   RotationOrder
    /// <summary>
    /// Different orderings of rotation for (Euler) rotation angles.
    /// </summary>
    public enum RotationOrder
    {
        XYZ = 0 //!< Identifier for x->y->z order of (Euler) rotation angles.
        ,
        XZY = 1 //!< Identifier for x->z->y order of (Euler) rotation angles.
        ,
        YXZ = 2 //!< Identifier for y->x->z order of (Euler) rotation angles.
        ,
        YZX = 3 //!< Identifier for y->z->x order of (Euler) rotation angles.
        ,
        ZXY = 4 //!< Identifier for z->x->y order of (Euler) rotation angles.
        ,
        ZYX = 5 //!< Identifier for z->y->x order of (Euler) rotation angles.
    }
    //                                                           _______________________________
    //__________________________________________________________/ frameOfReferenceRotationOrder
    /// <summary>
    /// The order of rotation used when interpreting the rotational frame of reference
    /// (eg. Y->X->Z order of rotations).
    /// </summary>
    public RotationOrder frameOfReferenceRotationOrder
    {
        get
        {
            return (RotationOrder)GestureRecognition_getRotationalFrameOfReferenceRotationOrder(m_gro);
        }
        set
        {
            GestureRecognition_setRotationalFrameOfReferenceRotationOrder(m_gro, (int)value);
        }
    }
    //                                                          ________________________________
    //_________________________________________________________/     GestureRecognition()
    /// <summary>
    /// Constructor.
    /// </summary>
    public GestureRecognition()
    {
        m_gro = GestureRecognition_create();
    }
    //                                                          ________________________________
    //_________________________________________________________/   ~GestureRecognition()
    /// <summary>
    /// Destructor.
    /// </summary>
    ~GestureRecognition()
    {
        if (m_gro.ToInt64() != 0)
        {
            GestureRecognition_delete(m_gro);
        }
    }
    //                                                          ________________________________
    //_________________________________________________________/      activateLicense()
    /// <summary>
    /// Provide a license to enable additional functionality.
    /// </summary>
    /// <param name="license_name">The license name text.</param>
    /// <param name="license_key">The license key text.</param>
    /// <returns>Zero on success, a negative error code on failure.</returns>
    public int activateLicense(string license_name, string license_key)
    {
        return GestureRecognition_activateLicense(m_gro, license_name, license_key);
    }
    //                                                          ________________________________
    //_________________________________________________________/     activateLicenseFile()
    /// <summary>
    /// Provide a license file to enable additional functionality.
    /// </summary>
    /// <param name="license_file_path">The file path to the license file.</param>
    /// <returns>Zero on success, a negative error code on failure.</returns>
    public int activateLicenseFile(string license_file_path)
    {
        return GestureRecognition_activateLicenseFile(m_gro, license_file_path);
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
    /// <param name="hmd">Current transformation of the VR headset / HMD.
    /// This must be a double[4,4] column-major matrix (where the translational component is in the m[3][*] sub-array).</param>
    /// <param name="record_as_sample">[OPTIONAL] Set this to the ID of a gesture to start recording a sample for that gesture.</param>
    /// <returns>
    /// Zero on success, an error code on failure.
    /// </returns>
    public int startStroke(double[,] hmd, int record_as_sample = -1)
    {
        return GestureRecognition_startStrokeM(m_gro, hmd, record_as_sample);
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
    /// <param name="hmd_p">The current position of the VR headset (users POV).</param>
    /// <param name="hmd_q">The current rotation of the VR headset (users POV).</param>
    /// <param name="record_as_sample">[OPTIONAL] Set this to the ID of a gesture to start recording a sample for that gesture.</param>
    /// <returns>
    /// Zero on success, an error code on failure.
    /// </returns>
    public int startStroke(Vector3 hmd_p, Quaternion hmd_q, int record_as_sample = -1)
    {
        double[] p = new double[3] { hmd_p.x, hmd_p.y, hmd_p.z };
        double[] q = new double[4] { hmd_q.x, hmd_q.y, hmd_q.z, hmd_q.w };
        return GestureRecognition_startStroke(m_gro, p, q, record_as_sample);
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
    /// <param name="hmd_p">The current position of the VR headset (users POV). This must be a double[3] array.</param>
    /// <param name="hmd_q">The current rotation of the VR headset (users POV). This must be a double[4] array.</param>
    /// <param name="record_as_sample">[OPTIONAL] Set this to the ID of a gesture to start recording a sample for that gesture.</param>
    /// <returns>
    /// Zero on success, an error code on failure.
    /// </returns>
    public int startStroke(double[] hmd_p, double[] hmd_q, int record_as_sample = -1)
    {
        return GestureRecognition_startStroke(m_gro, hmd_p, hmd_q, record_as_sample);
    }
    //                                                          ________________________________
    //_________________________________________________________/     updateHeadPosition()
    /// <summary>
    /// Update the current position of the HMD / headset during a gesture performance (stroke).
    /// </summary>
    /// <param name="hmd">The current position/orientation of the headset as a 4x4 matrix.</param>
    /// <returns>Zero on success, a negative error code on failure.</returns>
    public int updateHeadPosition(double[,] hmd)
    {
        return GestureRecognition_updateHeadPositionM(m_gro, hmd);
    }
    //                                                          ________________________________
    //_________________________________________________________/     updateHeadPosition()
    /// <summary>
    /// Update the current position of the HMD / headset during a gesture performance (stroke).
    /// </summary>
    /// <param name="hmd_p">The current position of the headset.</param>
    /// <param name="hmd_q">The current orientation (quaternion) of the headset.</param>
    /// <returns>Zero on success, a negative error code on failure.</returns>
    public int updateHeadPosition(Vector3 hmd_p, Quaternion hmd_q)
    {
        double[] p = new double[3] { hmd_p.x, hmd_p.y, hmd_p.z };
        double[] q = new double[4] { hmd_q.x, hmd_q.y, hmd_q.z, hmd_q.w };
        return GestureRecognition_updateHeadPositionQ(m_gro, p, q);
    }
    //                                                          ________________________________
    //_________________________________________________________/     updateHeadPosition()
    /// <summary>
    /// Update the current position of the HMD / headset during a gesture performance (stroke).
    /// </summary>
    /// <param name="hmd_p">The current position of the headset.</param>
    /// <param name="hmd_q">The current orientation (quaternion) of the headset.</param>
    /// <returns>Zero on success, a negative error code on failure.</returns>
    public int updateHeadPosition(double[] hmd_p, double[] hmd_q)
    {
        return GestureRecognition_updateHeadPositionQ(m_gro, hmd_p, hmd_q);
    }
    //                                                          ________________________________
    //_________________________________________________________/         contdStroke()
    /// <summary>
    /// Continue performing a gesture, given only the position of the device.
    /// </summary>
    /// <param name="m">Position of the controller/device with which the gesture is performed.</param>
    /// <returns>
    /// Zero on success, an error code on failure.
    /// </returns>
    public int contdStroke(Vector3 p)
    {
        double[] _p = new double[3] { p.x, p.y, p.z };
        return GestureRecognition_contdStroke(m_gro, _p);
    }
    //                                                          ________________________________
    //_________________________________________________________/         contdStroke()
    /// <summary>
    /// Continue performing a gesture, given only the position of the device.
    /// </summary>
    /// <param name="m">Position of the controller/device with which the gesture is performed.</param>
    /// <returns>
    /// Zero on success, an error code on failure.
    /// </returns>
    public int contdStroke(double[] p)
    {
        return GestureRecognition_contdStroke(m_gro, p);
    }
    //                                                          ________________________________
    //_________________________________________________________/         contdStrokeQ()
    /// <summary>
    /// Continue performing a gesture, given a position vector and rotation
    /// in the form of a quaternion.
    /// </summary>
    /// <param name="p">Position of the controller with which the gesture is performed.</param>
    /// <param name="q">Rotation of the controller with which the gesture is performed.</param>
    /// <returns>
    /// Zero on success, an error code on failure.
    /// </returns>
    public int contdStrokeQ(Vector3 p, Quaternion q)
    {
        double[] _p = new double[3] { p.x, p.y, p.z };
        double[] _q = new double[4] { q.x, q.y, q.z, q.w };
        return GestureRecognition_contdStrokeQ(m_gro, _p, _q);
    }
    //                                                          ________________________________
    //_________________________________________________________/        contdStrokeQ()
    /// <summary>
    /// Continue performing a gesture, given a position vector and rotation
    /// in the form of a quaternion.
    /// </summary>
    /// <param name="p">Position of the controller with which the gesture is performed. This must be a double[3] array.</param>
    /// <param name="q">Rotation of the controller with which the gesture is performed. This must be a double[4] array.</param>
    /// <returns>
    /// Zero on success, an error code on failure.
    /// </returns>
    public int contdStrokeQ(double[] p, double[] q)
    {
        return GestureRecognition_contdStrokeQ(m_gro, p, q);
    }
    //                                                          ________________________________
    //_________________________________________________________/         contdStrokeE()
    /// <summary>
    /// Continue performing a gesture, given a position vector and rotation
    /// in the form of Euler rotation angles (in radians).
    /// </summary>
    /// <param name="p">Position of the controller with which the gesture is performed.</param>
    /// <param name="q">Rotation of the controller with which the gesture is performed (angles in radians).</param>
    /// <returns>
    /// Zero on success, an error code on failure.
    /// </returns>
    public int contdStrokeQ(Vector3 p, Vector3 r)
    {
        double[] _p = new double[3] { p.x, p.y, p.z };
        double[] _r = new double[3] { r.x, r.y, r.z };
        return GestureRecognition_contdStrokeE(m_gro, _p, _r);
    }
    //                                                          ________________________________
    //_________________________________________________________/        contdStrokeE()
    /// <summary>
    /// Continue performing a gesture, given a position vector and rotation
    /// in the form of Euler rotation angles (in radians).
    /// </summary>
    /// <param name="p">Position of the controller with which the gesture is performed. This must be a double[3] array.</param>
    /// <param name="q">Rotation of the controller with which the gesture is performed. This must be a double[3] array.</param>
    /// <returns>
    /// Zero on success, an error code on failure.
    /// </returns>
    public int contdStrokeE(double[] p, double[] r)
    {
        return GestureRecognition_contdStrokeE(m_gro, p, r);
    }
    //                                                          ________________________________
    //_________________________________________________________/         contdStrokeM()
    /// <summary>
    /// Continue performing a gesture, given a transformation matrix containing both
    // position and rotation.
    /// </summary>
    /// <param name="m">Transformation of the controller with which the gesture is performed.
    /// This must be a double[4,4] column-major matrix (where the translational component is in the m[3][*] sub-array).</param>
    /// <returns>
    /// Zero on success, an error code on failure.
    /// </returns>
    public int contdStrokeM(double[,] m)
    {
        return GestureRecognition_contdStrokeM(m_gro, m);
    }
    //                                                          ________________________________
    //_________________________________________________________/          endStroke()
    /// <summary>
    /// End a gesture (stroke).
    /// If the gesture was started as recording a new sample (sample-recording-mode), then
    /// the gesture will be added to the reference examples for this gesture.
    /// If the gesture (stroke) was started in identification mode, the gesture recognition
    /// library will attempt to identify the gesture.
    /// </summary>
    /// <param name="pos">[OUT] The position where the gesture was performed.</param>
    /// <param name="scale">[OUT] The scale at which the gesture was performed.</param>
    /// <param name="dir0">[OUT] The primary direction in which the gesture was performed (ie. widest direction).</param>
    /// <param name="dir1">[OUT] The secondary direction in which the gesture was performed.</param>
    /// <param name="dir2">[OUT] The minor direction in which the gesture was performed (ie. narrowest direction).</param>
    /// <returns>
    /// The ID of the gesture identified, a negative error code on failure.
    /// </returns>
    public int endStroke(ref Vector3 pos, ref double scale, ref Vector3 dir0, ref Vector3 dir1, ref Vector3 dir2)
    {
        double[] _pos = new double[3];
        double[] _scale = new double[1];
        double[] _dir0 = new double[3];
        double[] _dir1 = new double[3];
        double[] _dir2 = new double[3];
        int ret = GestureRecognition_endStroke(m_gro, _pos, _scale, _dir0, _dir1, _dir2);
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
    /// library will attempt to identify the gesture.
    /// </summary>
    /// <param name="pos">[OUT] The position where the gesture was performed.</param>
    /// <param name="scale">[OUT] The scale at which the gesture was performed.</param>
    /// <param name="dir0">[OUT] The primary direction in which the gesture was performed (ie. widest direction).</param>
    /// <param name="dir1">[OUT] The secondary direction in which the gesture was performed.</param>
    /// <param name="dir2">[OUT] The minor direction in which the gesture was performed (ie. narrowest direction).</param>
    /// <returns>
    /// The ID of the gesture identified, a negative error code on failure.
    /// </returns>
    public int endStroke(ref Vector3 pos, ref double scale, ref Quaternion q)
    {
        double[] _pos = new double[3];
        double[] _scale = new double[1];
        double[] _dir0 = new double[3];
        double[] _dir1 = new double[3];
        double[] _dir2 = new double[3];
        int ret = GestureRecognition_endStroke(m_gro, _pos, _scale, _dir0, _dir1, _dir2);
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
        return ret;
    }
    //                                                          ________________________________
    //_________________________________________________________/          endStroke()
    /// <summary>
    /// End a gesture (stroke).
    /// If the gesture was started as recording a new sample (sample-recording-mode), then
    /// the gesture will be added to the reference examples for this gesture.
    /// If the gesture (stroke) was started in identification mode, the gesture recognition
    /// library will attempt to identify the gesture.
    /// </summary>
    /// <param name="pos">[OUT] The position where the gesture was performed. This must be a double[3] array.</param>
    /// <param name="scale">[OUT] The scale at which the gesture was performed. This must be a double[1] array.</param>
    /// <param name="dir0">[OUT] The primary direction in which the gesture was performed (ie. widest direction). This must be a double[3] array.</param>
    /// <param name="dir1">[OUT] The secondary direction in which the gesture was performed. This must be a double[3] array.</param>
    /// <param name="dir2">[OUT] The minor direction in which the gesture was performed (ie. narrowest direction). This must be a double[3] array.</param>
    /// <returns>
    /// The ID of the gesture identified, a negative error code on failure.
    /// </returns>
    public int endStroke(ref double[] pos, ref double[] scale, ref double[] dir0, ref double[] dir1, ref double[] dir2)
    {
        return GestureRecognition_endStroke(m_gro, pos, scale, dir0, dir1, dir2);
    }
    //                                                          ________________________________
    //_________________________________________________________/          endStroke()
    /// <summary>
    /// End a gesture (stroke).
    /// If the gesture was started as recording a new sample (sample-recording-mode), then
    /// the gesture will be added to the reference examples for this gesture.
    /// If the gesture (stroke) was started in identification mode, the gesture recognition
    /// library will attempt to identify the gesture.
    /// </summary>
    /// <returns>
    /// The ID of the gesture identified, a negative error code on failure.
    /// </returns>
    public int endStroke()
    {
        return GestureRecognition_endStroke(m_gro, null, null, null, null, null);
    }
    //                                                          ________________________________
    //_________________________________________________________/          endStroke()
    /// <summary>
    /// End a gesture (stroke).
    /// If the gesture was started as recording a new sample (sample-recording-mode), then
    /// the gesture will be added to the reference examples for this gesture.
    /// If the gesture (stroke) was started in identification mode, the gesture recognition
    /// library will attempt to identify the gesture.
    /// </summary>
    /// <param name="similarity">[OUT] Similarity between the performed stroke, and the identified gesture.</param>
    /// <param name="pos">[OUT] The position where the gesture was performed.</param>
    /// <param name="scale">[OUT] The scale at which the gesture was performed.</param>
    /// <param name="dir0">[OUT] The primary direction in which the gesture was performed (ie. widest direction).</param>
    /// <param name="dir1">[OUT] The secondary direction in which the gesture was performed.</param>
    /// <param name="dir2">[OUT] The minor direction in which the gesture was performed (ie. narrowest direction).</param>
    /// <returns>
    /// The ID of the gesture identified, a negative error code on failure.
    /// </returns>
    public int endStroke(ref double similarity, ref Vector3 pos, ref double scale, ref Vector3 dir0, ref Vector3 dir1, ref Vector3 dir2)
    {
        double[] _similarity = new double[1];
        double[] _pos = new double[3];
        double[] _scale = new double[1];
        double[] _dir0 = new double[3];
        double[] _dir1 = new double[3];
        double[] _dir2 = new double[3];
        int ret = GestureRecognition_endStrokeAndGetSimilarity(m_gro, _similarity, _pos, _scale, _dir0, _dir1, _dir2);
        similarity = _similarity[0];
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
    /// library will attempt to identify the gesture.
    /// </summary>
    /// <param name="similarity">[OUT] Similarity between the performed stroke, and the identified gesture.</param>
    /// <param name="pos">[OUT] The position where the gesture was performed.</param>
    /// <param name="scale">[OUT] The scale at which the gesture was performed.</param>
    /// <param name="dir0">[OUT] The primary direction in which the gesture was performed (ie. widest direction).</param>
    /// <param name="dir1">[OUT] The secondary direction in which the gesture was performed.</param>
    /// <param name="dir2">[OUT] The minor direction in which the gesture was performed (ie. narrowest direction).</param>
    /// <returns>
    /// The ID of the gesture identified, a negative error code on failure.
    /// </returns>
    public int endStroke(ref double similarity, ref Vector3 pos, ref double scale, ref Quaternion q)
    {
        double[] _similarity = new double[1];
        double[] _pos = new double[3];
        double[] _scale = new double[1];
        double[] _dir0 = new double[3];
        double[] _dir1 = new double[3];
        double[] _dir2 = new double[3];
        int ret = GestureRecognition_endStrokeAndGetSimilarity(m_gro, _similarity, _pos, _scale, _dir0, _dir1, _dir2);
        similarity = _similarity[0];
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
        return ret;
    }
    //                                                          ________________________________
    //_________________________________________________________/          endStroke()
    /// <summary>
    /// End a gesture (stroke).
    /// If the gesture was started as recording a new sample (sample-recording-mode), then
    /// the gesture will be added to the reference examples for this gesture.
    /// If the gesture (stroke) was started in identification mode, the gesture recognition
    /// library will attempt to identify the gesture.
    /// </summary>
    /// <param name="similarity">[OUT] Similarity between the performed stroke, and the identified gesture.</param>
    /// <param name="pos">[OUT] The position where the gesture was performed. This must be a double[3] array.</param>
    /// <param name="scale">[OUT] The scale at which the gesture was performed. This must be a double[1] array.</param>
    /// <param name="dir0">[OUT] The primary direction in which the gesture was performed (ie. widest direction). This must be a double[3] array.</param>
    /// <param name="dir1">[OUT] The secondary direction in which the gesture was performed. This must be a double[3] array.</param>
    /// <param name="dir2">[OUT] The minor direction in which the gesture was performed (ie. narrowest direction). This must be a double[3] array.</param>
    /// <returns>
    /// The ID of the gesture identified, a negative error code on failure.
    /// </returns>
    public int endStroke(ref double[] similarity, ref double[] pos, ref double[] scale, ref double[] dir0, ref double[] dir1, ref double[] dir2)
    {
        return GestureRecognition_endStrokeAndGetSimilarity(m_gro, similarity, pos, scale, dir0, dir1, dir2);
    }
    //                                                          ________________________________
    //_________________________________________________________/          endStroke()
    /// <summary>
    /// End a gesture (stroke).
    /// If the gesture was started as recording a new sample (sample-recording-mode), then
    /// the gesture will be added to the reference examples for this gesture.
    /// If the gesture (stroke) was started in identification mode, the gesture recognition
    /// library will attempt to identify the gesture.
    /// </summary>
    /// <param name="similarity">[OUT] Similarity between the performed stroke, and the identified gesture.</param>
    /// <returns>
    /// The ID of the gesture identified, a negative error code on failure.
    /// </returns>
    public int endStroke(ref double similarity)
    {
        double[] _similarity = new double[1];
        int ret = GestureRecognition_endStrokeAndGetSimilarity(m_gro, _similarity, null, null, null, null, null);
        similarity = _similarity[0];
        return ret;
    }
    //                                                      ____________________________________
    //_____________________________________________________/ endStrokeAndGetAllProbabilities()
    /// <summary>
    /// End a gesture (stroke) and get the probability estimate of it to belong to each
    /// of the recorded (and trained) gestures.
    /// If the gesture was started as recording a new sample (sample-recording-mode), then
    /// the gesture will be added to the reference examples for this gesture.
    /// If the gesture (stroke) was started in identification mode, the gesture recognition
    /// library will calculate and return the probability for each recorded gesture.
    /// </summary>
    /// <param name="p">List of probability estimate values, where 1.0 is the highest and 0.0 is the lowest.</param>
    /// <returns>
    /// The gesture ID of the identified gesture, or a negative error code on failure.
    /// </returns>
    public int endStrokeAndGetAllProbabilities(ref double[] p)
    {
        int num_gestures = this.numberOfGestures();
        p = new double[num_gestures];
        int[] _n = new int[1];
        _n[0] = num_gestures;
        return GestureRecognition_endStrokeAndGetAllProbabilities(m_gro, p, _n, null, null, null, null, null);
    }
    //                                                      ____________________________________
    //_____________________________________________________/ endStrokeAndGetAllProbabilities()
    /// <summary>
    /// End a gesture (stroke) and get the probability estimate of it to belong to each
    /// of the recorded (and trained) gestures.
    /// If the gesture was started as recording a new sample (sample-recording-mode), then
    /// the gesture will be added to the reference examples for this gesture.
    /// If the gesture (stroke) was started in identification mode, the gesture recognition
    /// library will calculate and return the probability for each recorded gesture.
    /// </summary>
    /// <param name="p">List of probability estimate values, where 1.0 is the highest and 0.0 is the lowest.</param>
    /// <param name="s">List of similarity values, where 1.0 is perfect similarity and 0.0 no similarity at all.</param>
    /// <returns>
    /// The gesture ID of the identified gesture, or a negative error code on failure.
    /// </returns>
    public int endStrokeAndGetAllProbabilitiesAndSimilarities(ref double[] p, ref double[] s)
    {
        int num_gestures = this.numberOfGestures();
        p = new double[num_gestures];
        s = new double[num_gestures];
        int[] _n = new int[1];
        _n[0] = num_gestures;
        double[] pos = new double[3];
        double[] scale = new double[1];
        double[] dir0 = new double[3];
        double[] dir1 = new double[3];
        double[] dir2 = new double[3];
        return GestureRecognition_endStrokeAndGetAllProbabilitiesAndSimilarities(m_gro, p, s, _n, pos, scale, dir0, dir1, dir2);
    }
    //                                                      ____________________________________
    //_____________________________________________________/        isStrokeStarted()
    /// <summary>
    /// Query whether a gesture performance (gesture motion, stroke) was started and is currently ongoing.
    /// </summary>
    /// <returns>
    /// True if a gesture motion (stroke) was started and is ongoing, false if not.
    /// </returns>
    public bool isStrokeStarted()
    {
        return GestureRecognition_isStrokeStarted(m_gro) != 0;
    }
    //                                                      ____________________________________
    //_____________________________________________________/        cancelStroke()
    /// <summary>
    /// Cancel a gesture performance without identifying it.
    /// </summary>
    /// <returns>
    /// Zero on success, an error code on failure.
    /// </returns>
    public int cancelStroke()
    {
        return GestureRecognition_cancelStroke(m_gro);
    }
    //                                                      ____________________________________
    //_____________________________________________________/        contdIdentify()
    /// <summary>
    /// Identify gesture while performing it continuously.
    /// </summary>
    /// <param name="hmd_p">The current position of the headset.</param>
    /// <param name="hmd_q">The current rotation (orientation) of the headset.</param>
    /// <param name="similarity">[OUT] Similarity between the performed stroke, and the identified gesture.</param>
    /// <returns>
    /// The ID of the identified gesture. On failure, a negative error code.
    /// </returns>
    public int contdIdentify(Vector3 hmd_p, Quaternion hmd_q, ref double similarity)
    {
        double[] _hmd_p = new double[3] { hmd_p.x, hmd_p.y, hmd_p.z };
        double[] _hmd_q = new double[4] { hmd_q.x, hmd_q.y, hmd_q.z, hmd_q.w };
        double[] _similarity = new double[1];
        int ret = GestureRecognition_contdIdentify(m_gro, _hmd_p, _hmd_q, _similarity);
        similarity = _similarity[0];
        return ret;
    }
    //                                    ______________________________________________________
    //___________________________________/ contdIdentifyAndGetAllProbabilitiesAndSimilarities()
    /// <summary>
    /// Continuous gesture identification.
    /// </summary>
    /// <param name="hmd_p">Vector(x, y, z) of the current headset position.</param>
    /// <param name="hmd_q">Quaternion(x, y, z, w) of the current headset rotation.</param>
    /// <param name="p">Array to which to write the probability values (each 0~1).</param>
    /// <param name="s">Array to which to write the similarity values (each 0~1).</param>
    /// <returns>The gesture ID of the identified gesture, or a negative error code on failure.</returns>
    public int contdIdentifyAndGetAllProbabilitiesAndSimilarities(Vector3 hmd_p, Quaternion hmd_q, ref double[] p, ref double[] s)
    {
        double[] _hmd_p = new double[3] { hmd_p.x, hmd_p.y, hmd_p.z };
        double[] _hmd_q = new double[4] { hmd_q.x, hmd_q.y, hmd_q.z, hmd_q.w };
        int num_gestures = this.numberOfGestures();
        p = new double[num_gestures];
        s = new double[num_gestures];
        int[] _n = new int[1];
        _n[0] = num_gestures;
        return GestureRecognition_contdIdentifyAndGetAllProbabilitiesAndSimilarities(m_gro, _hmd_p, _hmd_q, p, s, _n);
    }
    //                                                      ____________________________________
    //_____________________________________________________/        contdRecord()
    /// <summary>
    /// Record gesture while performing it continuously.
    /// </summary>
    /// <param name="hmd_p">The current position of the headset.</param>
    /// <param name="hmd_q">The current rotation (orientation) of the headset.</param>
    /// <returns>
    /// The ID of the recorded gesture, or a negative error code on failure.
    /// </returns>
    public int contdRecord(Vector3 hmd_p, Quaternion hmd_q)
    {
        double[] _hmd_p = new double[3] { hmd_p.x, hmd_p.y, hmd_p.z };
        double[] _hmd_q = new double[4] { hmd_q.x, hmd_q.y, hmd_q.z, hmd_q.w };
        return GestureRecognition_contdRecord(m_gro, _hmd_p, _hmd_q);
    }


    //                                                          ________________________________
    //_________________________________________________________/  contdIdentificationPeriod
    /// <summary>
    /// The time frame for continuous gesture identification / recording (via contdIdentift()
    /// and contdRecord) in milliseconds.
    /// By default, continuous identification will look for gestures which take about 500ms
    /// to perform. If your gestures are shorter or longer, try adjusting this value.
    /// </summary>
    public int contdIdentificationPeriod
    {
        get
        {
            return GestureRecognition_getContdIdentificationPeriod(m_gro);
        }
        set
        {
            GestureRecognition_setContdIdentificationPeriod(m_gro, value);
        }
    }


    //                                                          ________________________________
    //_________________________________________________________/  contdIdentificationSmoothing
    /// <summary>
    /// The number of samples to use for smoothing continuous gesture identification results.
    /// Since continuous gestures can sometimes be "jumpy" (that is, during some phases of the
    /// motion they briefly resemble other gestures) it is advised to set this value between 2
    /// and 5 so that several recent results will be analyzed together to provide a more stable
    /// identification.However, setting this value high means that the AI will take longer to
    /// react when the continuous motion changes from one gesture to another.
    /// </summary>
    public int contdIdentificationSmoothing
    {
        get
        {
            return GestureRecognition_getContdIdentificationSmoothing(m_gro);
        }
        set
        {
            GestureRecognition_setContdIdentificationSmoothing(m_gro, value);
        }
    }

    //                                                          ________________________________
    //_________________________________________________________/      numberOfGestures()
    /// <summary>
    /// Query the number of currently registered gestures.
    /// </summary>
    /// <returns>
    /// The number of gestures currently registered in the library.
    /// </returns>
    public int numberOfGestures()
    {
        return GestureRecognition_numberOfGestures(m_gro);
    }
    //                                                          ________________________________
    //_________________________________________________________/         deleteGesture()
    /// <summary>
    /// Delete currently registered gesture.
    /// </summary>
    /// <param name="index">ID of the gesture to delete.</param>
    /// <returns>
    /// Zero on success, a negative error code on failure.
    /// </returns>
    public int deleteGesture(int index)
    {
        return GestureRecognition_deleteGesture(m_gro, index);
    }
    //                                                          ________________________________
    //_________________________________________________________/      deleteAllGestures()
    /// <summary>
    /// Delete all currently registered gestures.
    /// </summary>
    /// <returns>
    /// Zero on success, a negative error code on failure.
    /// </returns>
    public int deleteAllGestures()
    {
        return GestureRecognition_deleteAllGestures(m_gro);
    }
    //                                                          ________________________________
    //_________________________________________________________/      createGesture()
    /// <summary>
    /// Create a new gesture.
    /// </summary>
    /// <param name="name">Name of the new gesture.</param>
    /// <returns>
    /// ID of the new gesture.
    /// </returns>
    public int createGesture(string name)
    {
        return GestureRecognition_createGesture(m_gro, name, IntPtr.Zero);
    }
    //                                                          ________________________________
    //_________________________________________________________/      recognitionScore()
    /// <summary>
    /// Get the current recognition performance
    /// (probabliity to recognize the correct gesture: a value between 0 and 1 where 0 is 0%
    /// and 1 is 100% correct recognition rate).
    /// </summary>
    /// <returns>
    /// The current recognition performance
    /// (probabliity to recognize the correct gesture: a value between 0 and 1 where 0 is 0%
    /// and 1 is 100% correct recognition rate).
    /// </returns>
    public double recognitionScore()
    {
        return GestureRecognition_recognitionScore(m_gro);
    }
    //                                                          ________________________________
    //_________________________________________________________/      getGestureName()
    /// <summary>
    /// Get the name associated to a gesture. 
    /// </summary>
    /// <param name="index">ID of the gesture to query.</param>
    /// <returns>
    /// Name of the gesture. On failure, an empty string is returned.
    /// </returns>
    public string getGestureName(int index)
    {
        int strlen = GestureRecognition_getGestureNameLength(m_gro, index);
        if (strlen <= 0)
        {
            return "";
        }
        StringBuilder sb = new StringBuilder(strlen + 1);
        GestureRecognition_copyGestureName(m_gro, index, sb, sb.Capacity);
        return sb.ToString();
    }
    //                                                          ________________________________
    //_________________________________________________________/  getGestureMetadataAsString()
    /// <summary>
    /// Get the metadata associated to a gesture, assuming it's a string. 
    /// </summary>
    /// <param name="index">ID of the gesture to query.</param>
    /// <returns>
    /// Metadata of the gesture as string. Null on failure or if no metadata was set.
    /// </returns>
    public string getGestureMetadataAsString(int index)
    {
        IntPtr dmo = GestureRecognition_getGestureMetadata(m_gro, index);
        if (dmo == IntPtr.Zero) {
            return null;
        }
        int size = GestureRecognition_getDefaultMetadataSize(dmo);
        if (size <= 0) {
            return null;
        }
        StringBuilder sb = new StringBuilder(new string(' ', size));
        int ret = GestureRecognition_getDefaultMetadataData(dmo, sb, sb.Capacity);
        if (ret < 0) {
            return null;
        }
        return sb.ToString(0, ret);
    }
    //                                                          ________________________________
    //_________________________________________________________/   getGestureNumberOfSamples()
    /// <summary>
    /// Query the number of samples recorded for one gesture.
    /// </summary>
    /// <param name="index">ID of the gesture to query.</param>
    /// <returns>
    /// Number of samples recorded for this gesture or -1 if the gesture does not exist.
    /// </returns>
    public int getGestureNumberOfSamples(int index)
    {
        return GestureRecognition_getGestureNumberOfSamples(m_gro, index);
    }
    //                                                          ________________________________
    //_________________________________________________________/    getGestureSampleLength()
    /// <summary>
    /// Get the number of data points a sample has.
    /// </summary>
    /// <param name="gesture_index">The zero-based index (ID) of the gesture from where to retrieve the sample.</param>
    /// <param name="sample_index">The zero-based index(ID) of the sample to retrieve.</param>
    /// <param name="processed">Whether the raw data points should be retrieved (0) or the processed data points (1).</param>
    /// <returns>
    /// The number of data points on that sample.
    /// </returns>
    public int getGestureSampleLength(int gesture_index, int sample_index, int processed)
    {
        return GestureRecognition_getGestureSampleLength(m_gro, gesture_index, sample_index, processed);
    }
    //                                                          ________________________________
    //_________________________________________________________/    getGestureSampleStroke()
    /// <summary>
    /// Retrieve a sample stroke.
    /// </summary>
    /// <param name="gesture_index">The zero-based index (ID) of the gesture from where to retrieve the sample.</param>
    /// <param name="sample_index">The zero-based index(ID) of the sample to retrieve.</param>
    /// <param name="processed">Whether the raw data points should be retrieved (0) or the processed data points (1).</param>
    /// <param name="p">[OUT] A vector array to receive the positional data points of the stroke / recorded sample.</param>
    /// <param name="q">[OUT] A quaternion array to receive the rotational data points of the stroke / recorded sample.</param>
    /// <param name="hmd_p">[OUT] A vector array to receive the position of the HMD at the time of the stroke sample recording.</param>
    /// <param name="hmd_q">[OUT] A quaternion array to receive the rotation of the HMD at the time of the stroke sample recording.</param>
    /// <returns>
    /// The number of data points on that sample (ie. resulting length of p and q).
    /// </returns>
    public int getGestureSampleStroke(int gesture_index, int sample_index, int processed, ref Vector3[] p, ref Quaternion[] q, ref Vector3[] hmd_p, ref Quaternion[] hmd_q)
    {
        p = null;
        q = null;
        hmd_p = null;
        hmd_q = null;
        int sample_length = this.getGestureSampleLength(gesture_index, sample_index, processed);
        if (sample_length <= 0)
        {
            return sample_length;
        }
        double[] _hmd_p = new double[3 * sample_length];
        double[] _hmd_q = new double[4 * sample_length];
        double[] _p = new double[3 * sample_length];
        double[] _q = new double[4 * sample_length];
        int samples_written = GestureRecognition_getGestureSampleStroke(m_gro, gesture_index, sample_index, processed, sample_length, _p, _q, _hmd_p, _hmd_q);
        if (samples_written <= 0)
        {
            return samples_written;
        }
        p = new Vector3[samples_written];
        q = new Quaternion[samples_written];
        hmd_p = new Vector3[samples_written];
        hmd_q = new Quaternion[samples_written];
        for (int i = 0; i < samples_written; i++)
        {
            p[i].x = (float)_p[i * 3 + 0];
            p[i].y = (float)_p[i * 3 + 1];
            p[i].z = (float)_p[i * 3 + 2];
            q[i].x = (float)_q[i * 4 + 0];
            q[i].y = (float)_q[i * 4 + 1];
            q[i].z = (float)_q[i * 4 + 2];
            q[i].w = (float)_q[i * 4 + 3];
            hmd_p[i].x = (float)_hmd_p[i * 3 + 0];
            hmd_p[i].y = (float)_hmd_p[i * 3 + 1];
            hmd_p[i].z = (float)_hmd_p[i * 3 + 2];
            hmd_q[i].x = (float)_hmd_q[i * 4 + 0];
            hmd_q[i].y = (float)_hmd_q[i * 4 + 1];
            hmd_q[i].z = (float)_hmd_q[i * 4 + 2];
            hmd_q[i].w = (float)_hmd_q[i * 4 + 3];
        }
        return samples_written;
    }
    //                                                          ________________________________
    //_________________________________________________________/    getGestureMeanStroke()
    /// <summary>
    /// Retrieve a gesture mean (average over samples).
    /// </summary>
    /// <param name="gesture_index">The zero-based index (ID) of the gesture from where to retrieve the sample.</param>
    /// <param name="p">[OUT] A vector array to receive the positional data points of the stroke / recorded sample.</param>
    /// <param name="q">[OUT] A quaternion array to receive the rotational data points of the stroke / recorded sample.</param>
    /// <param name="stroke_p">[OUT] A vector to receive the average gesture position relative to (ie. as seen by) the headset.</param>
    /// <param name="stroke_q">[OUT] A quaternion to receive the average gesture position rotation to (ie. as seen by) the headset.</param>
    /// <param name="scale">[OUT] The average scale of the gesture.</param>
    /// <returns>
    /// The number of data points on that sample (ie. resulting length of p and q).
    /// </returns>
    public int getGestureMeanStroke(int gesture_index, ref Vector3[] p, ref Quaternion[] q, ref Vector3 stroke_p, ref Quaternion stroke_q, ref float scale)
    {
        p = null;
        q = null;
        stroke_p = Vector3.zero;
        stroke_q = Quaternion.identity;
        scale = 0.0f;
        int sample_length = GestureRecognition_getGestureMeanLength(m_gro, gesture_index);
        if (sample_length == 0)
        {
            return 0;
        }
        double[] _p = new double[3 * sample_length];
        double[] _q = new double[4 * sample_length];
        double[] _stroke_p = new double[3];
        double[] _stroke_q = new double[4];
        double[] _scale = new double[1];
        int samples_written = GestureRecognition_getGestureMeanStroke(m_gro, gesture_index, _p, _q, sample_length, _stroke_p, _stroke_q, _scale);
        if (samples_written <= 0)
        {
            return 0;
        }
        scale = (float)_scale[0];
        p = new Vector3[samples_written];
        q = new Quaternion[samples_written];
        stroke_p.x = (float)_stroke_p[0];
        stroke_p.y = (float)_stroke_p[1];
        stroke_p.z = (float)_stroke_p[2];
        stroke_q.x = (float)_stroke_q[0];
        stroke_q.y = (float)_stroke_q[1];
        stroke_q.z = (float)_stroke_q[2];
        stroke_q.w = (float)_stroke_q[3];
        for (int i = 0; i < samples_written; i++)
        {
            p[i].x = (float)_p[i * 3 + 0];
            p[i].y = (float)_p[i * 3 + 1];
            p[i].z = (float)_p[i * 3 + 2];
            q[i].x = (float)_q[i * 4 + 0];
            q[i].y = (float)_q[i * 4 + 1];
            q[i].z = (float)_q[i * 4 + 2];
            q[i].w = (float)_q[i * 4 + 3];
        }
        return samples_written;
    }
    //                                                          ________________________________
    //_________________________________________________________/      deleteGestureSample()
    /// <summary>
    /// Delete a gesture sample recording from the set.
    /// </summary>
    /// <param name="gesture_index">The zero-based index (ID) of the gesture from where to delete the sample.</param>
    /// <param name="sample_index">The zero-based index(ID) of the sample to delete.</param>
    /// <returns>
    /// Zero on success, a negative error code on failure.
    /// </returns>
    public int deleteGestureSample(int gesture_index, int sample_index)
    {
        return GestureRecognition_deleteGestureSample(m_gro, gesture_index, sample_index);
    }
    //                                                          ________________________________
    //_________________________________________________________/    deleteAllGestureSamples()
    /// <summary>
    /// Delete all gesture sample recordings from the set.
    /// </summary>
    /// <param name="gesture_index">The zero-based index (ID) of the gesture from where to delete the sample.</param>
    /// <returns>
    /// Zero on success, a negative error code on failure.
    /// </returns>
    public int deleteAllGestureSamples(int gesture_index)
    {
        return GestureRecognition_deleteAllGestureSamples(m_gro, gesture_index);
    }
    //                                                          ________________________________
    //_________________________________________________________/       setGestureName()
    /// <summary>
    /// Set the name associated with a gesture.
    /// </summary>
    /// <param name="index">ID of the gesture whose name to set.</param>
    /// <param name="name">The new name of the gesture.</param>
    /// <returns>
    /// Zero on success, a negative error code on failure.
    /// </returns>
    public int setGestureName(int index, string name)
    {
        return GestureRecognition_setGestureName(m_gro, index, name);
    }
    //                                                          ________________________________
    //_________________________________________________________/  setGestureMetadataAsString()
    /// <summary>
    /// Set the metadata associated with a gesture, as a string.
    /// </summary>
    /// <param name="index">ID of the gesture whose metadata to set.</param>
    /// <param name="str">The new string for the gesture metadata.</param>
    /// <returns>
    /// Zero on success, a negative error code on failure.
    /// </returns>
    public int setGestureMetadataAsString(int index, string str)
    {
        int ret;
        IntPtr dmo = GestureRecognition_getGestureMetadata(m_gro, index);
        if (dmo == IntPtr.Zero) {
            dmo = GestureRecognition_createDefaultMetadata();
            ret = GestureRecognition_setGestureMetadata(m_gro, index, dmo);
            if (ret != 0) {
                GestureRecognition_deleteDefaultMetadata(dmo);
                return ret;
            }
        }
        // IntPtr strPtr = Marshal.StringToCoTaskMemUni(str); // .StringToHGlobalUni(str); // 
        // ret = GestureRecognition_setDefaultMetadataData(dmo, strPtr, str.Length);
        // Marshal.FreeCoTaskMem(strPtr); // .FreeHGlobal(strPtr); // 
        ret = GestureRecognition_setDefaultMetadataData(dmo, str, str.Length);
        return ret;
    }
    //                                                          ________________________________
    //_________________________________________________________/     getGestureEnabled()
    /// <summary>
    /// Get whether a registered gesture is enabled or disabled.
    /// A disabled gesture is not used in training and identification,
    /// but will retain its recorded samples.
    /// </summary>
    /// <param name="index">ID of the gesture whose status to set.</param>
    /// <returns>
    /// True if the gesture is used, false if it is disabled and being ignored.
    /// </returns>
    public bool getGestureEnabled(int index)
    {
        return GestureRecognition_getGestureEnabled(m_gro, index) != 0;
    }
    //                                                          ________________________________
    //_________________________________________________________/     setGestureEnabled()
    /// <summary>
    /// Enable/disable a registered gesture.
    /// A disabled gesture is not used in training and identification,
    /// but will retain its recorded samples.
    /// </summary>
    /// <param name="index">ID of the gesture whose status to set.</param>
    /// <param name="enabled">Whether the gesture is supposed to be enabled (default) or disabled.</param>
    /// <returns>
    /// Zero on success, a negative error code on failure.
    /// </returns>
    public int setGestureEnabled(int index, bool enabled)
    {
        return GestureRecognition_setGestureEnabled(m_gro, index, enabled ? 1 : 0);
    }

    //                                                               ___________________________
    //______________________________________________________________/ UpdateHeadPositionPolicy
    /// <summary>
    /// Whether to update the hmd (frame of reference) position/rotation during the gesturing motion.
    /// </summary>
    public enum UpdateHeadPositionPolicy
    {
        UseLatest = 0 //!< Use the hmd position most recently submitted as current head position.
        ,
        UseInitial = 1 //!< Use the initial head position, don't use later head positional updates.
    }

    //                                                          ________________________________
    //_________________________________________________________/  setUpdateHeadPositionPolicy()
    /// <summary>
    /// Change the current policy on whether the AI should consider changes in head position during the gesturing.
    /// This will change whether the data provided via calls to "updateHeadPosition" functions will be used,
    /// so you also need to provide the changing head position via "updateHeadPosition" for this to have any effect.
    /// The data provided via "updateHeadPosition" is stored regardless of the policy, but is only used if the policy
    /// set by this function requires it.
    /// </summary>
    /// <param name="p">The policy on whether to use (and how to use) the data provided by "updateHeadPosition" during a gesture motion.</param>
    /// <returns>
    /// Zero on success, a negative error code on failure.
    /// </returns>
    public int setUpdateHeadPositionPolicy(GestureRecognition.UpdateHeadPositionPolicy p)
    {
        return GestureRecognition_setUpdateHeadPositionPolicy(m_gro, (int)p);
    }
    //                                                          ________________________________
    //_________________________________________________________/  getUpdateHeadPositionPolicy()
    /// <summary>
    /// Query the current policy on whether the AI should consider changes in head position during the gesturing.
    /// </summary>
    /// <returns>
    /// The current policy on whether to use (and how to use) the data provided by "updateHeadPosition" during a gesture motion.
    /// </returns>
    public GestureRecognition.UpdateHeadPositionPolicy getUpdateHeadPositionPolicy()
    {
        return (GestureRecognition.UpdateHeadPositionPolicy)GestureRecognition_getUpdateHeadPositionPolicy(m_gro);
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
        return GestureRecognition_saveToFile(m_gro, path);
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
        MetadataCreatorFunction mcf = GestureRecognition_getDefaultMetadataCreatorFunction();
        return GestureRecognition_loadFromFile(m_gro, path, mcf);
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
        MetadataCreatorFunction mcf = GestureRecognition_getDefaultMetadataCreatorFunction();
        return GestureRecognition_loadFromBuffer(m_gro, buffer, buffer.Length, mcf);
    }
    //                                                          ________________________________
    //_________________________________________________________/    importFromFile()
    /// <summary>
    /// Import gestures from a previously saved gesture recognition AI from a file.
    /// </summary>
    /// <param name="path">File system path and filename from where to import.</param>
    /// <returns>
    /// Zero on success, a negative error code on failure.
    /// </returns>
    public int importFromFile(string path)
    {
        MetadataCreatorFunction mcf = GestureRecognition_getDefaultMetadataCreatorFunction();
        return GestureRecognition_importFromFile(m_gro, path, mcf);
    }
    //                                                          ________________________________
    //_________________________________________________________/     importFromBuffer()
    /// <summary>
    /// Import gestures from a previously saved gesture recognition AI from a byte buffer.
    /// </summary>
    /// <param name="buffer">The byte buffer from where to import.</param>
    /// <returns>
    /// Zero on success, a negative error code on failure.
    /// </returns>
    public int importFromBuffer(byte[] buffer)
    {
        MetadataCreatorFunction mcf = GestureRecognition_getDefaultMetadataCreatorFunction();
        return GestureRecognition_importFromBuffer(m_gro, buffer, buffer.Length, mcf);
    }
    //                                                          ________________________________
    //_________________________________________________________/     importGestureSamples()
    /// <summary>
    /// Import recorded gesture samples from another gesture recognition object.
    /// </summary>
    /// <param name="from_gro">The GestureRecognitionObject from where to import recorded gesture samples.</param>
    /// <param name="from_gesture_index">The index(ID) of the gesture(on the other GRO) to import.</param>
    /// <param name="into_gesture_index">The index (ID) of the gesture (on this object) to which the samples should be added.</param>
    /// <returns>
    /// Zero on success, a negative error code on failure.
    /// </returns>
    public int importGestureSamples(GestureRecognition from_gro, int from_gesture_index, int into_gesture_index)
    {
        return GestureRecognition_importGestureSamples(m_gro, from_gro.m_gro, from_gesture_index, into_gesture_index);
    }
    //                                                          ________________________________
    //_________________________________________________________/     importGestures()
    /// <summary>
    /// Import recorded gesture samples from another gesture recognition object, merging gestures by name.
    /// Gestures with names which are not in the list of gestures yet will be appended.
    /// </summary>
    /// <param name="from_gro">The GestureRecognitionObject from where to import gestures.</param>
    /// <returns>
    /// Zero on success, a negative error code on failure.
    /// </returns>
    public int importGestures(GestureRecognition from_gro)
    {
        return GestureRecognition_importGestures(m_gro, from_gro.m_gro);
    }
    //                                                          ________________________________
    //_________________________________________________________/    saveToFileAsync()
    /// <summary>
    /// Save the gesture recognition artificial intelligence and recorded gestures to a file, asynchronously.
    /// The function will return immediately, while the saving process will continue in the background.
    /// Use isSaving() to check if the saving process is still ongoing.
    /// </summary>
    /// <param name="path">File system path and filename where to save.</param>
    /// <returns>
    /// Zero on success, a negative error code on failure.
    /// </returns>
    public int saveToFileAsync(string path)
    {
        return GestureRecognition_saveToFileAsync(m_gro, path);
    }
    //                                                      ____________________________________
    //_____________________________________________________/ setSavingUpdateCallbackFunction()
    /// <summary>
    /// Set the callback function to be called (repeatedly) during saving.
    /// Set to null for no callback.
    /// </summary>
    /// <param name="callback_function">The function to be called during saving.</param>
    /// <returns>
    /// Zero on success, a negative error code on failure.
    /// </returns>
    public int setSavingUpdateCallbackFunction(SavingCallbackFunction callback_function)
    {
        return GestureRecognition_setSavingUpdateCallbackFunction(m_gro, callback_function);
    }
    //                                                      ____________________________________
    //_____________________________________________________/ setSavingUpdateCallbackMetadata()
    /// <summary>
    /// Set the metadata object to be sent to the callback function during saving.
    /// </summary>
    /// <param name="callback_metadata">The metadata object to be sent to the function during saving.</param>
    /// <returns>
    /// Zero on success, a negative error code on failure.
    /// </returns>
    public int setSavingUpdateCallbackMetadata(IntPtr callback_metadata)
    {
        return GestureRecognition_setSavingUpdateCallbackMetadata(m_gro, callback_metadata);
    }
    //                                                      ____________________________________
    //_____________________________________________________/ setSavingFinishCallbackFunction()
    /// <summary>
    /// Set the callback function to call when saving finishes. Set to null for no callback.
    /// </summary>
    /// <param name="callback_function">The function to call when saving is finished.</param>
    /// <returns>
    /// Zero on success, a negative error code on failure.
    /// </returns>
    public int setSavingFinishCallbackFunction(SavingCallbackFunction callback_function)
    {
        return GestureRecognition_setSavingFinishCallbackFunction(m_gro, callback_function);
    }
    //                                                      ____________________________________
    //_____________________________________________________/ setSavingFinishCallbackMetadata()
    /// <summary>
    /// Set the metadata object to be sent to the callback function to call when saving finishes.
    /// </summary>
    /// <param name="callback_metadata">The metadata object to be sent to the function to call when saving is finished.</param>
    /// <returns>
    /// Zero on success, a negative error code on failure.
    /// </returns>
    public int setSavingFinishCallbackMetadata(IntPtr callback_metadata)
    {
        return GestureRecognition_setSavingFinishCallbackMetadata(m_gro, callback_metadata);
    }
    //                                                          ________________________________
    //_________________________________________________________/    isSaving()
    /// <summary>
    /// hether the Neural Network is currently being saved to a file or buffer.
    /// </summary>
    /// <returns>
    /// True if the AI is currently being saved (to file or buffer), false if not.
    /// </returns>
    public bool isSaving()
    {
        return GestureRecognition_isSaving(m_gro) == 1;
    }
    //                                                          ________________________________
    //_________________________________________________________/    cancelSaving()
    /// <summary>
    /// Cancel a currently running savinging process.
    /// </summary>
    /// <returns>
    /// Zero on success, a negative error code on failure.
    /// </returns>
    public int cancelSaving()
    {
        return GestureRecognition_cancelSaving(m_gro);
    }
    //                                                          ________________________________
    //_________________________________________________________/    loadFromFileAsync()
    /// <summary>
    /// Load a previously saved gesture recognition artificial intelligence from a file, asynchronously.
    /// The function will return immediately, while the loading process will continue in the background.
    /// Use isLoading() to check if the loading process is still ongoing.
    /// </summary>
    /// <param name="path">File system path and filename from where to load.</param>
    /// <returns>
    /// Zero on success, a negative error code on failure.
    /// </returns>
    public int loadFromFileAsync(string path)
    {
        MetadataCreatorFunction mcf = GestureRecognition_getDefaultMetadataCreatorFunction();
        return GestureRecognition_loadFromFileAsync(m_gro, path, mcf);
    }
    //                                                          ________________________________
    //_________________________________________________________/     loadFromBufferAsync()
    /// <summary>
    /// Load a previously saved gesture recognition artificial intelligence from a byte buffer, asynchronously.
    /// The function will return immediately, while the loading process will continue in the background.
    /// Use isLoading() to check if the loading process is still ongoing.
    /// </summary>
    /// <param name="buffer">The byte buffer containing the artificial intelligence.</param>
    /// <returns>
    /// Zero on success, a negative error code on failure.
    /// </returns>
    public int loadFromBufferAsync(byte[] buffer)
    {
        MetadataCreatorFunction mcf = GestureRecognition_getDefaultMetadataCreatorFunction();
        return GestureRecognition_loadFromBufferAsync(m_gro, buffer, buffer.Length, mcf);
    }
    //                                                      ____________________________________
    //_____________________________________________________/ setLoadingUpdateCallbackFunction()
    /// <summary>
    /// Set the callback function to be called (repeatedly) during loading.
    /// Set to null for no callback.
    /// </summary>
    /// <param name="callback_function">The function to be called during loading.</param>
    /// <returns>
    /// Zero on success, a negative error code on failure.
    /// </returns>
    public int setLoadingUpdateCallbackFunction(LoadingCallbackFunction callback_function)
    {
        return GestureRecognition_setLoadingUpdateCallbackFunction(m_gro, callback_function);
    }
    //                                                      ____________________________________
    //_____________________________________________________/ setLoadingUpdateCallbackMetadata()
    /// <summary>
    /// Set the metadata object to be sent to the callback function during loading.
    /// </summary>
    /// <param name="callback_metadata">The metadata object to be sent to the function during loading.</param>
    /// <returns>
    /// Zero on success, a negative error code on failure.
    /// </returns>
    public int setLoadingUpdateCallbackMetadata(IntPtr callback_metadata)
    {
        return GestureRecognition_setLoadingUpdateCallbackMetadata(m_gro, callback_metadata);
    }
    //                                                      ____________________________________
    //_____________________________________________________/ setLoadingFinishCallbackFunction()
    /// <summary>
    /// Set the callback function to call when loading finishes. Set to null for no callback.
    /// </summary>
    /// <param name="callback_function">The function to call when loading is finished.</param>
    /// <returns>
    /// Zero on success, a negative error code on failure.
    /// </returns>
    public int setLoadingFinishCallbackFunction(LoadingCallbackFunction callback_function)
    {
        return GestureRecognition_setLoadingFinishCallbackFunction(m_gro, callback_function);
    }
    //                                                      ____________________________________
    //_____________________________________________________/ setLoadingFinishCallbackMetadata()
    /// <summary>
    /// Set the metadata object to be sent to the callback function to call when loading finishes.
    /// </summary>
    /// <param name="callback_metadata">The metadata object to be sent to the function to call when loading is finished.</param>
    /// <returns>
    /// Zero on success, a negative error code on failure.
    /// </returns>
    public int setLoadingFinishCallbackMetadata(IntPtr callback_metadata)
    {
        return GestureRecognition_setLoadingFinishCallbackMetadata(m_gro, callback_metadata);
    }
    //                                                          ________________________________
    //_________________________________________________________/    isLoading()
    /// <summary>
    /// hether the Neural Network is currently loading from a file or buffer.
    /// </summary>
    /// <returns>
    /// True if the AI is currently loading (from file or buffer), false if not.
    /// </returns>
    public bool isLoading()
    {
        return GestureRecognition_isLoading(m_gro) == 1;
    }
    //                                                          ________________________________
    //_________________________________________________________/    cancelLoading()
    /// <summary>
    /// Cancel a currently running loading process.
    /// </summary>
    /// <returns>
    /// Zero on success, a negative error code on failure.
    /// </returns>
    public int cancelLoading()
    {
        return GestureRecognition_cancelLoading(m_gro);
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
        return GestureRecognition_startTraining(m_gro);
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
        return GestureRecognition_isTraining(m_gro) != 0;
    }
    //                                                          ________________________________
    //_________________________________________________________/       stopTraining()
    /// <summary>
    /// Stop the learning process.
    /// Due to the asynchronous nature of the learning process ("learning in background")
    /// it may take a moment before the training in the background is actually stopped.
    /// </summary>
    /// <returns>
    /// Zero on success, a negative error code on failure.
    /// </returns>
    public int stopTraining()
    {
        return GestureRecognition_stopTraining(m_gro);
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
        return GestureRecognition_getMaxTrainingTime(m_gro);
    }
    //                                                          ________________________________
    //_________________________________________________________/     setMaxTrainingTime()
    /// <summary>
    /// Set the maximum time used for the learning process (in seconds).
    /// </summary>
    /// <param name="t">Maximum time to spend learning (in seconds).</param>
    public void setMaxTrainingTime(int t)
    {
        GestureRecognition_setMaxTrainingTime(m_gro, t);
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
        GestureRecognition_setTrainingUpdateCallback(m_gro, cbf);
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
        GestureRecognition_setTrainingUpdateCallbackMetadata(m_gro, metadata);
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
        GestureRecognition_setTrainingFinishCallback(m_gro, cbf);
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
        GestureRecognition_setTrainingFinishCallbackMetadata(m_gro, metadata);
    }
    //                                                          ________________________________
    //_________________________________________________________/  getMetadataAsString()
    /// <summary>
    /// Get the metadata assigned to this GestureRecognition object, assuming it's a string. 
    /// </summary>
    /// <returns>
    /// Metadata assigned to this GestureRecognition object as string. Null on failure or if no metadata was set.
    /// </returns>
    public string getMetadataAsString()
    {
        IntPtr dmo = GestureRecognition_getMetadata(m_gro);
        if (dmo == IntPtr.Zero) {
            return null;
        }
        int size = GestureRecognition.GestureRecognition_getDefaultMetadataSize(dmo);
        if (size <= 0) {
            return null;
        }
        StringBuilder sb = new StringBuilder(new string(' ', size));
        int ret = GestureRecognition.GestureRecognition_getDefaultMetadataData(dmo, sb, sb.Capacity);
        if (ret < 0) {
            return null;
        }
        return sb.ToString(0, ret);
    }
    //                                                          ________________________________
    //_________________________________________________________/  setMetadataAsString()
    /// <summary>
    /// Set the metadata associated with this GestureRecognition object, as a string.
    /// </summary>
    /// <param name="str">The new string for the GestureRecognition metadata.</param>
    /// <returns>
    /// Zero on success, a negative error code on failure.
    /// </returns>
    public int setMetadataAsString(string str)
    {
        int ret;
        IntPtr dmo = GestureRecognition_getMetadata(m_gro);
        if (dmo == IntPtr.Zero) {
            dmo = GestureRecognition_createDefaultMetadata();
            ret = GestureRecognition_setMetadata(m_gro, dmo);
            if (ret != 0) {
                GestureRecognition_deleteDefaultMetadata(dmo);
                return ret;
            }
        }
        ret = GestureRecognition_setDefaultMetadataData(dmo, str, str.Length);
        return ret;
    }
    //                                                          ________________________________
    //_________________________________________________________/      getVersionString()
    /// <summary>
    /// Get the version of MiVRy as a human-readable string. 
    /// </summary>
    /// <returns>
    /// A string describing the version. On failure, an empty string is returned.
    /// </returns>
    public static string getVersionString()
    {
        int strlen = GestureRecognition_getVersionStringLength();
        if (strlen <= 0)
        {
            return "";
        }
        StringBuilder sb = new StringBuilder(strlen + 1);
        GestureRecognition_copyVersionString(sb, sb.Capacity);
        return sb.ToString();
    }

    // ----------------------------------------------------------------------------------------------------------
    // Internal wrapper functions to the plug-in
    // ----------------------------------------------------------------------------------------------------------
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr MetadataCreatorFunction();
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void TrainingCallbackFunction(double performace, IntPtr metadata);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void LoadingCallbackFunction(int status, IntPtr metadata);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void SavingCallbackFunction(int status, IntPtr metadata);

    public const string libfile = "gesturerecognition";

    [DllImport(libfile, EntryPoint = "GestureRecognition_create", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr GestureRecognition_create();
    [DllImport(libfile, EntryPoint = "GestureRecognition_delete", CallingConvention = CallingConvention.Cdecl)]
    public static extern void GestureRecognition_delete(IntPtr gro);
    [DllImport(libfile, EntryPoint = "GestureRecognition_activateLicense", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_activateLicense(IntPtr gro, string license_name, string license_key);
    [DllImport(libfile, EntryPoint = "GestureRecognition_activateLicenseFile", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_activateLicenseFile(IntPtr gro, string license_file_path);
    [DllImport(libfile, EntryPoint = "GestureRecognition_startStroke", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_startStroke(IntPtr gro, double[] hmd_p, double[] hmd_q, int record_as_sample);
    [DllImport(libfile, EntryPoint = "GestureRecognition_startStrokeM", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_startStrokeM(IntPtr gro, double[,] hmd, int record_as_sample);
    [DllImport(libfile, EntryPoint = "GestureRecognition_updateHeadPositionM", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_updateHeadPositionM(IntPtr gro, double[,] hmd);
    [DllImport(libfile, EntryPoint = "GestureRecognition_updateHeadPositionQ", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_updateHeadPositionQ(IntPtr gro, double[] hmd_p, double[] hmd_q);
    [DllImport(libfile, EntryPoint = "GestureRecognition_contdStroke", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_contdStroke(IntPtr gro, double[] p);
    [DllImport(libfile, EntryPoint = "GestureRecognition_contdStrokeQ", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_contdStrokeQ(IntPtr gro, double[] p, double[] q);
    [DllImport(libfile, EntryPoint = "GestureRecognition_contdStrokeE", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_contdStrokeE(IntPtr gro, double[] p, double[] r);
    [DllImport(libfile, EntryPoint = "GestureRecognition_contdStrokeM", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_contdStrokeM(IntPtr gro, double[,] m);
    [DllImport(libfile, EntryPoint = "GestureRecognition_endStroke", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_endStroke(IntPtr gro, double[] pos, double[] scale, double[] dir0, double[] dir1, double[] dir2);
    [DllImport(libfile, EntryPoint = "GestureRecognition_endStrokeAndGetAllProbabilities", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_endStrokeAndGetAllProbabilities(IntPtr gro, double[] p, int[] n, double[] pos, double[] scale, double[] dir0, double[] dir1, double[] dir2);
    [DllImport(libfile, EntryPoint = "GestureRecognition_endStrokeAndGetSimilarity", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_endStrokeAndGetSimilarity(IntPtr gro, double[] similarity, double[] pos, double[] scale, double[] dir0, double[] dir1, double[] dir2);
    [DllImport(libfile, EntryPoint = "GestureRecognition_endStrokeAndGetAllProbabilitiesAndSimilarities", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_endStrokeAndGetAllProbabilitiesAndSimilarities(IntPtr gro, double[] p, double[] s, int[] n, double[] pos, double[] scale, double[] dir0, double[] dir1, double[] dir2);
    [DllImport(libfile, EntryPoint = "GestureRecognition_isStrokeStarted", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_isStrokeStarted(IntPtr gro);
    [DllImport(libfile, EntryPoint = "GestureRecognition_cancelStroke", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_cancelStroke(IntPtr gro);
    [DllImport(libfile, EntryPoint = "GestureRecognition_contdIdentify", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_contdIdentify(IntPtr gro, double[] hmd_p, double[] hmd_q, double[] similarity);
    [DllImport(libfile, EntryPoint = "GestureRecognition_contdIdentifyAndGetAllProbabilitiesAndSimilarities", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_contdIdentifyAndGetAllProbabilitiesAndSimilarities(IntPtr gro, double[] hmd_p, double[] hmd_q, double[] p, double[] s, int[] n);
   [DllImport(libfile, EntryPoint = "GestureRecognition_contdRecord", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_contdRecord(IntPtr gro, double[] hmd_p, double[] hmd_q);
    [DllImport(libfile, EntryPoint = "GestureRecognition_getContdIdentificationPeriod", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_getContdIdentificationPeriod(IntPtr gro);
    [DllImport(libfile, EntryPoint = "GestureRecognition_setContdIdentificationPeriod", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_setContdIdentificationPeriod(IntPtr gro, int ms);
    [DllImport(libfile, EntryPoint = "GestureRecognition_getContdIdentificationSmoothing", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_getContdIdentificationSmoothing(IntPtr gro);
    [DllImport(libfile, EntryPoint = "GestureRecognition_setContdIdentificationSmoothing", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_setContdIdentificationSmoothing(IntPtr gro, int samples);
    [DllImport(libfile, EntryPoint = "GestureRecognition_numberOfGestures", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_numberOfGestures(IntPtr gro);
    [DllImport(libfile, EntryPoint = "GestureRecognition_deleteGesture", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_deleteGesture(IntPtr gro, int index);
    [DllImport(libfile, EntryPoint = "GestureRecognition_deleteAllGestures", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_deleteAllGestures(IntPtr gro);
    [DllImport(libfile, EntryPoint = "GestureRecognition_createGesture", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_createGesture(IntPtr gro, string name, IntPtr metadata);
    [DllImport(libfile, EntryPoint = "GestureRecognition_recognitionScore", CallingConvention = CallingConvention.Cdecl)]
    public static extern double GestureRecognition_recognitionScore(IntPtr gro);
    // [DllImport(libfile, EntryPoint = "GestureRecognition_getGestureName", CallingConvention = CallingConvention.Cdecl)]
    // public static extern string GestureRecognition_getGestureName(IntPtr gro, int index);
    [DllImport(libfile, EntryPoint = "GestureRecognition_getGestureNameLength", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_getGestureNameLength(IntPtr gro, int index);
    [DllImport(libfile, EntryPoint = "GestureRecognition_copyGestureName", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_copyGestureName(IntPtr gro, int index, StringBuilder buf, int buflen);
    [DllImport(libfile, EntryPoint = "GestureRecognition_getGestureMetadata", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr GestureRecognition_getGestureMetadata(IntPtr gro, int index);
    [DllImport(libfile, EntryPoint = "GestureRecognition_getGestureNumberOfSamples", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_getGestureNumberOfSamples(IntPtr gro, int index);
    [DllImport(libfile, EntryPoint = "GestureRecognition_getGestureSampleLength", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_getGestureSampleLength(IntPtr gro, int gesture_index, int sample_index, int processed);
    [DllImport(libfile, EntryPoint = "GestureRecognition_getGestureSampleStroke", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_getGestureSampleStroke(IntPtr gro, int gesture_index, int sample_index, int processed, int stroke_buf_size, double[] p, double[] q, double[] hmd_p, double[] hmd_q);
    [DllImport(libfile, EntryPoint = "GestureRecognition_getGestureMeanLength", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_getGestureMeanLength(IntPtr gro, int gesture_index);
    [DllImport(libfile, EntryPoint = "GestureRecognition_getGestureMeanStroke", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_getGestureMeanStroke(IntPtr gro, int gesture_index, double[] p, double[] q, int stroke_buf_size, double[] stroke_p, double[] stroke_q, double[] scale);
    [DllImport(libfile, EntryPoint = "GestureRecognition_deleteGestureSample", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_deleteGestureSample(IntPtr gro, int gesture_index, int sample_index);
    [DllImport(libfile, EntryPoint = "GestureRecognition_deleteAllGestureSamples", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_deleteAllGestureSamples(IntPtr gro, int gesture_index);
    [DllImport(libfile, EntryPoint = "GestureRecognition_setGestureName", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_setGestureName(IntPtr gro, int index, string name);
    [DllImport(libfile, EntryPoint = "GestureRecognition_setGestureMetadata", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_setGestureMetadata(IntPtr gro, int index, IntPtr metadata);
    [DllImport(libfile, EntryPoint = "GestureRecognition_getGestureEnabled", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_getGestureEnabled(IntPtr gro, int index);
    [DllImport(libfile, EntryPoint = "GestureRecognition_setGestureEnabled", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_setGestureEnabled(IntPtr gro, int index, int enabled);
    [DllImport(libfile, EntryPoint = "GestureRecognition_setUpdateHeadPositionPolicy", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_setUpdateHeadPositionPolicy(IntPtr gro, int p);
    [DllImport(libfile, EntryPoint = "GestureRecognition_getUpdateHeadPositionPolicy", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_getUpdateHeadPositionPolicy(IntPtr gro);
    [DllImport(libfile, EntryPoint = "GestureRecognition_saveToFile", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_saveToFile(IntPtr gro, string path);
    [DllImport(libfile, EntryPoint = "GestureRecognition_loadFromFile", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_loadFromFile(IntPtr gro, string path, MetadataCreatorFunction createMetadata);
    [DllImport(libfile, EntryPoint = "GestureRecognition_loadFromBuffer", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_loadFromBuffer(IntPtr gro, byte[] buffer, int buffer_size, MetadataCreatorFunction createMetadata);
    [DllImport(libfile, EntryPoint = "GestureRecognition_importFromFile", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_importFromFile(IntPtr gro, string path, MetadataCreatorFunction createMetadata);
    [DllImport(libfile, EntryPoint = "GestureRecognition_importFromBuffer", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_importFromBuffer(IntPtr gro, byte[] buffer, int buffer_size, MetadataCreatorFunction createMetadata);
    [DllImport(libfile, EntryPoint = "GestureRecognition_importGestureSamples", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_importGestureSamples(IntPtr gro, IntPtr from_gro, int from_gesture_index, int into_gesture_index);
    [DllImport(libfile, EntryPoint = "GestureRecognition_importGestures", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_importGestures(IntPtr gro, IntPtr from_gro);
    [DllImport(libfile, EntryPoint = "GestureRecognition_saveToFileAsync", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_saveToFileAsync(IntPtr gro, string path);
    [DllImport(libfile, EntryPoint = "GestureRecognition_setSavingUpdateCallbackFunction", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_setSavingUpdateCallbackFunction(IntPtr gro, SavingCallbackFunction cbf);
    [DllImport(libfile, EntryPoint = "GestureRecognition_setSavingUpdateCallbackMetadata", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_setSavingUpdateCallbackMetadata(IntPtr gro, IntPtr metadata);
    [DllImport(libfile, EntryPoint = "GestureRecognition_setSavingFinishCallbackFunction", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_setSavingFinishCallbackFunction(IntPtr gro, SavingCallbackFunction cbf);
    [DllImport(libfile, EntryPoint = "GestureRecognition_setSavingFinishCallbackMetadata", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_setSavingFinishCallbackMetadata(IntPtr gro, IntPtr metadata);
    [DllImport(libfile, EntryPoint = "GestureRecognition_isSaving", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_isSaving(IntPtr gro);
    [DllImport(libfile, EntryPoint = "GestureRecognition_cancelSaving", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_cancelSaving(IntPtr gro);
    [DllImport(libfile, EntryPoint = "GestureRecognition_loadFromFileAsync", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_loadFromFileAsync(IntPtr gro, string path, MetadataCreatorFunction createMetadata);
    [DllImport(libfile, EntryPoint = "GestureRecognition_loadFromBufferAsync", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_loadFromBufferAsync(IntPtr gro, byte[] buffer, int buffer_size, MetadataCreatorFunction createMetadata);
    [DllImport(libfile, EntryPoint = "GestureRecognition_setLoadingUpdateCallbackFunction", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_setLoadingUpdateCallbackFunction(IntPtr gro, LoadingCallbackFunction cbf);
    [DllImport(libfile, EntryPoint = "GestureRecognition_setLoadingUpdateCallbackMetadata", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_setLoadingUpdateCallbackMetadata(IntPtr gro, IntPtr metadata);
    [DllImport(libfile, EntryPoint = "GestureRecognition_setLoadingFinishCallbackFunction", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_setLoadingFinishCallbackFunction(IntPtr gro, LoadingCallbackFunction cbf);
    [DllImport(libfile, EntryPoint = "GestureRecognition_setLoadingFinishCallbackMetadata", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_setLoadingFinishCallbackMetadata(IntPtr gro, IntPtr metadata);
    [DllImport(libfile, EntryPoint = "GestureRecognition_isLoading", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_isLoading(IntPtr gro);
    [DllImport(libfile, EntryPoint = "GestureRecognition_cancelLoading", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_cancelLoading(IntPtr gro);
    [DllImport(libfile, EntryPoint = "GestureRecognition_startTraining", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_startTraining(IntPtr gro);
    [DllImport(libfile, EntryPoint = "GestureRecognition_isTraining", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_isTraining(IntPtr gro);
    [DllImport(libfile, EntryPoint = "GestureRecognition_stopTraining", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_stopTraining(IntPtr gro);
    [DllImport(libfile, EntryPoint = "GestureRecognition_getMaxTrainingTime", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_getMaxTrainingTime(IntPtr gro);
    [DllImport(libfile, EntryPoint = "GestureRecognition_setMaxTrainingTime", CallingConvention = CallingConvention.Cdecl)]
    public static extern void GestureRecognition_setMaxTrainingTime(IntPtr gro, int t);
    [DllImport(libfile, EntryPoint = "GestureRecognition_setTrainingUpdateCallback", CallingConvention = CallingConvention.Cdecl)]
    public static extern void GestureRecognition_setTrainingUpdateCallback(IntPtr gro, TrainingCallbackFunction cbf);
    [DllImport(libfile, EntryPoint = "GestureRecognition_setTrainingFinishCallback", CallingConvention = CallingConvention.Cdecl)]
    public static extern void GestureRecognition_setTrainingFinishCallback(IntPtr gro, TrainingCallbackFunction cbf);
    [DllImport(libfile, EntryPoint = "GestureRecognition_setTrainingUpdateCallbackMetadata", CallingConvention = CallingConvention.Cdecl)]
    public static extern void GestureRecognition_setTrainingUpdateCallbackMetadata(IntPtr gro, IntPtr metadata);
    [DllImport(libfile, EntryPoint = "GestureRecognition_setTrainingFinishCallbackMetadata", CallingConvention = CallingConvention.Cdecl)]
    public static extern void GestureRecognition_setTrainingFinishCallbackMetadata(IntPtr gro, IntPtr metadata);
    [DllImport(libfile, EntryPoint = "GestureRecognition_getIgnoreHeadRotationX", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_getIgnoreHeadRotationX(IntPtr gro);
    [DllImport(libfile, EntryPoint = "GestureRecognition_setIgnoreHeadRotationX", CallingConvention = CallingConvention.Cdecl)]
    public static extern void GestureRecognition_setIgnoreHeadRotationX(IntPtr gro, int on_off);
    [DllImport(libfile, EntryPoint = "GestureRecognition_getIgnoreHeadRotationY", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_getIgnoreHeadRotationY(IntPtr gro);
    [DllImport(libfile, EntryPoint = "GestureRecognition_setIgnoreHeadRotationY", CallingConvention = CallingConvention.Cdecl)]
    public static extern void GestureRecognition_setIgnoreHeadRotationY(IntPtr gro, int on_off);
    [DllImport(libfile, EntryPoint = "GestureRecognition_getIgnoreHeadRotationZ", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_getIgnoreHeadRotationZ(IntPtr gro);
    [DllImport(libfile, EntryPoint = "GestureRecognition_setIgnoreHeadRotationZ", CallingConvention = CallingConvention.Cdecl)]
    public static extern void GestureRecognition_setIgnoreHeadRotationZ(IntPtr gro, int on_off);
    [DllImport(libfile, EntryPoint = "GestureRecognition_getRotationalFrameOfReferenceX", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_getRotationalFrameOfReferenceX(IntPtr gro);
    [DllImport(libfile, EntryPoint = "GestureRecognition_setRotationalFrameOfReferenceX", CallingConvention = CallingConvention.Cdecl)]
    public static extern void GestureRecognition_setRotationalFrameOfReferenceX(IntPtr gro, int i);
    [DllImport(libfile, EntryPoint = "GestureRecognition_getRotationalFrameOfReferenceY", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_getRotationalFrameOfReferenceY(IntPtr gro);
    [DllImport(libfile, EntryPoint = "GestureRecognition_setRotationalFrameOfReferenceY", CallingConvention = CallingConvention.Cdecl)]
    public static extern void GestureRecognition_setRotationalFrameOfReferenceY(IntPtr gro, int i);
    [DllImport(libfile, EntryPoint = "GestureRecognition_getRotationalFrameOfReferenceZ", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_getRotationalFrameOfReferenceZ(IntPtr gro);
    [DllImport(libfile, EntryPoint = "GestureRecognition_setRotationalFrameOfReferenceZ", CallingConvention = CallingConvention.Cdecl)]
    public static extern void GestureRecognition_setRotationalFrameOfReferenceZ(IntPtr gro, int i);
    [DllImport(libfile, EntryPoint = "GestureRecognition_getRotationalFrameOfReferenceRotationOrder", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_getRotationalFrameOfReferenceRotationOrder(IntPtr gro);
    [DllImport(libfile, EntryPoint = "GestureRecognition_setRotationalFrameOfReferenceRotationOrder", CallingConvention = CallingConvention.Cdecl)]
    public static extern void GestureRecognition_setRotationalFrameOfReferenceRotationOrder(IntPtr gro, int rotOrd);
    // [DllImport(libfile, EntryPoint = "GestureRecognition_getVersionString", CallingConvention = CallingConvention.Cdecl)]
    // public static extern string GestureRecognition_getVersionString();
    [DllImport(libfile, EntryPoint = "GestureRecognition_getVersionStringLength", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_getVersionStringLength();
    [DllImport(libfile, EntryPoint = "GestureRecognition_copyVersionString", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_copyVersionString(StringBuilder buf, int buflen);
    [DllImport(libfile, EntryPoint = "GestureRecognition_setMetadata", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_setMetadata(IntPtr gro, IntPtr metadata);
    [DllImport(libfile, EntryPoint = "GestureRecognition_getMetadata", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr GestureRecognition_getMetadata(IntPtr gro);
    [DllImport(libfile, EntryPoint = "GestureRecognition_createDefaultMetadata", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr GestureRecognition_createDefaultMetadata();
    [DllImport(libfile, EntryPoint = "GestureRecognition_deleteDefaultMetadata", CallingConvention = CallingConvention.Cdecl)]
    public static extern void GestureRecognition_deleteDefaultMetadata(IntPtr dmo);
    [DllImport(libfile, EntryPoint = "GestureRecognition_getDefaultMetadataSize", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_getDefaultMetadataSize(IntPtr dmo);
    [DllImport(libfile, EntryPoint = "GestureRecognition_getDefaultMetadataData", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_getDefaultMetadataData(IntPtr dmo, IntPtr data, int size);
    [DllImport(libfile, EntryPoint = "GestureRecognition_getDefaultMetadataData", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_getDefaultMetadataData(IntPtr dmo, StringBuilder sb, int sbLen);
    [DllImport(libfile, EntryPoint = "GestureRecognition_setDefaultMetadataData", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_setDefaultMetadataData(IntPtr dmo, IntPtr data, int size);
    [DllImport(libfile, EntryPoint = "GestureRecognition_setDefaultMetadataData", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GestureRecognition_setDefaultMetadataData(IntPtr dmo, string str, int strLen);
    [DllImport(libfile, EntryPoint = "GestureRecognition_defaultMetadataCreatorFunction", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr GestureRecognition_defaultMetadataCreatorFunction();
    [DllImport(libfile, EntryPoint = "GestureRecognition_getDefaultMetadataCreatorFunction", CallingConvention = CallingConvention.Cdecl)]
    public static extern MetadataCreatorFunction GestureRecognition_getDefaultMetadataCreatorFunction();

    private IntPtr m_gro;
}
