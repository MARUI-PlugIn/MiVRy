/*
 * MiVRy - 3D gesture recognition library plug-in for Unity.
 * Version 2.10
 * Copyright (c) 2024 MARUI-PlugIn (inc.)
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

//#define MIVRY_USE_BOLT

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using System.IO;
#if UNITY_ANDROID
using UnityEngine.Networking;
#endif
#if MIVRY_USE_BOLT
using Ludiq;
using Bolt;
#endif

/// <summary>
/// Convenience Unity script that can be added as a component to any script
/// for easy gesture recognition.
/// </summary>
public class MivryQuestHands : MonoBehaviour
{
    #region ENUMS

    /// <summary>
    /// Which hand is being used for gesturing.
    /// </summary>
    [System.Serializable]
    public enum TrackedHand
    {
        LeftHand    = 0,
        RightHand   = 1,
        BothHands   = 2,
        _NUM_VALUES = 3
    };

    /// <summary>
    /// Which data points (bones) are being tracked.
    /// </summary>
    [System.Serializable]
    public enum TrackingPoints
    {
        AllBones = 0,
        AllFingerTips = 1,
        IndexFingerTipOnly = 2,
        _NUM_VALUES = 3
    };

    /// <summary>
    /// What triggers the start / end of a gesture.
    /// </summary>
    [System.Serializable]
    public enum GestureTrigger
    {
        LeftHandPinch = 0,
        LeftHandGrab = 1,
        RightHandPinch = 2,
        RightHandGrab = 3,
        Manual= 4,
        _NUM_VALUES = 5
    };

    #endregion

    #region INSPECTOR_VARIABLES

    /// <summary>
    /// The name (ID) of the MiVRy license to use.
    /// If left empty, MiVRy will not activate any license and will run as "free" version.
    /// </summary>
    [Tooltip("License Name (ID) of your MiVRy license. Leave empty for free version.")]
    public string LicenseName = "";

    /// <summary>
    /// The license key of the MiVRy license to use.
    /// If left empty, MiVRy will not activate any license and will run as "free" version.
    /// </summary>
    [Tooltip("License Key of your MiVRy license. Leave empty for free version.")]
    public string LicenseKey = "";

    /// <summary>
    /// Path to file with license ID and license key of the MiVRy license to use.
    /// If left empty, MiVRy will not activate any license and will run as "free" version.
    /// </summary>
    [Tooltip("Path to file with License ID and License Key of your MiVRy license. Leave empty for free version.")]
    public string LicenseFilePath = "";

    /// <summary>
    /// The path to the gesture recognition database file to load.
    /// In the editor, this will be relative to the Assets/ folder.
    /// In stand-alone (build), this will be relative to the StreamingAssets/ folder.
    /// </summary>
    [Tooltip("Path to the .dat file to load gestures from.")]
    public string GestureDatabaseFile = "";

    /// <summary>
    /// Which hand is being tracked.
    /// Note that you can have two separated instances of Mivry, one for each hand, for independent gesturing.
    /// </summary>
    [Tooltip("Note that you can have two separated instances of Mivry, one for each hand, for independent gesturing.")]
    public TrackedHand trackedHand = TrackedHand.BothHands;

    /// <summary>
    /// The OculusIntegration OVRHandPrefab for the left hand.
    /// </summary>
    [Tooltip("The OculusIntegration OVRHandPrefab for the left hand.")]
    public OVRHand leftHand = null;
    // Internal reference to the OVRSkeleton of that hand.
    private OVRSkeleton leftHandSkeleton = null;

    /// <summary>
    /// Which points of the hand to track for gesturing.
    /// </summary>
    [Tooltip("Which points of the hand to track for gesturing.")]
    public TrackingPoints leftHandTrackingPoints = TrackingPoints.AllBones;

    /// <summary>
    /// What will indicate the start/end of a gesture motion of the left hand.
    /// </summary>
    [Tooltip("What will indicate the start/end of a gesture motion of the left hand.")]
    public GestureTrigger leftGestureTrigger = GestureTrigger.LeftHandPinch;

    /// <summary>
    /// What value will indicate if the trigger is 'on' or 'off'.
    /// </summary>
    [Tooltip("What value will indicate if the trigger is 'on' or 'off'.")]
    public float leftGestureTriggerThreshold = 0.5f;

    /// <summary>
    /// The current value of the trigger. When the trigger is 'manual', use this to indicate the start/end of the gesture motion.
    /// </summary>
    [Tooltip("The current value of the trigger. When the trigger is 'manual', use this to indicate the start/end of the gesture motion.")]
    public float leftGestureTriggerValue = 0;



    /// <summary>
    /// The OculusIntegration OVRHandPrefab for the right hand.
    /// </summary>
    [Tooltip("The OculusIntegration OVRHandPrefab for the right hand.")]
    public OVRHand rightHand = null;
    // Internal reference to the OVRSkeleton of that hand.
    private OVRSkeleton rightHandSkeleton = null;

    /// <summary>
    /// Which points of the hand to track for gesturing.
    /// </summary>
    [Tooltip("Which points of the hand to track for gesturing.")]
    public TrackingPoints rightHandTrackingPoints = TrackingPoints.AllBones;

    /// <summary>
    /// What will indicate the start/end of a gesture motion of the right hand.
    /// </summary>
    [Tooltip("What will indicate the start/end of a gesture motion of the right hand.")]
    public GestureTrigger rightGestureTrigger = GestureTrigger.RightHandPinch;

    /// <summary>
    /// What value will indicate if the trigger is 'on' or 'off'.
    /// </summary>
    [Tooltip("What value will indicate if the trigger is 'on' or 'off'.")]
    public float rightGestureTriggerThreshold = 0.5f;

    /// <summary>
    /// The current value of the trigger. When the trigger is 'manual', use this to indicate the start/end of the gesture motion.
    /// </summary>
    [Tooltip("The current value of the trigger. When the trigger is 'manual', use this to indicate the start/end of the gesture motion.")]
    public float rightGestureTriggerValue = 0;

    /// <summary>
    /// Event callback functions to be called when a gesture was performed.
    /// </summary>
    [Tooltip("Event to trigger when a gesture was performed.")]
    [SerializeField]
    public GestureCompletionEvent OnGestureCompletion = null;

    /// <summary>
    /// Which Unity XR plugin is being used by this project.
    /// This is only important when the GestureDatabase file was created (or will be used) with a different PlugIn
    /// or on a different platform (such as Unreal Engine), because they are using different coordinate systems.
    /// See the Unity Package Manager and Project Settings -> XR Plugin Manager to see which plugin is being used.
    /// </summary>
    [Tooltip("The Unity XR plugin used in this project (see: Project Settings -> XR Plugin Manager).")]
    public Mivry.UnityXrPlugin unityXrPlugin = Mivry.UnityXrPlugin.OculusVR;

    /// <summary>
    /// The coordinate system MiVRy should use internally (or that the GestureDatabase file was created with).
    /// </summary>
    [Tooltip("The coordinate system MiVRy should use internally (or that the GestureDatabase file was created with).")]
    public Mivry.MivryCoordinateSystem mivryCoordinateSystem = Mivry.MivryCoordinateSystem.Unity_OculusVR;

    /// <summary>
    /// The OVRSkeleton bone indices used for the finger tips.
    /// </summary>
    public static readonly int[] fingerTipIndices = new int[5] {
        (int)OVRSkeleton.BoneId.Hand_ThumbTip,
        (int)OVRSkeleton.BoneId.Hand_IndexTip,
        (int)OVRSkeleton.BoneId.Hand_MiddleTip,
        (int)OVRSkeleton.BoneId.Hand_RingTip,
        (int)OVRSkeleton.BoneId.Hand_PinkyTip
    };

    #endregion

    #region INTERNAL_VARIABLES

    private GestureCombinations gc = null; //!< The gesture combinations object used to identify the gestures.
    private GestureCompletionData data = new GestureCompletionData(); //!< Gesture data object to be passed to event listeners.
    private int numberOfTrackingPointsLeft  = 0; //!< The number of tracked points for the left hand (if any).
    private int numberOfTrackingPointsRight = 0; //!< The number of tracked points for the right hand (if any).

    private bool isGesturingLeft  = false; //!< Whether the left hand is currently making a gesture motion.
    private bool isGesturingRight = false; //!< Whether the right hand is currently making a gesture motion.

    private int recordGestureSample = -1; //!< Which (if any) gesture where recording a new sample for.

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        int ret;

        this.leftHandSkeleton = this.leftHand.GetComponent<OVRSkeleton>();
        this.rightHandSkeleton = this.rightHand.GetComponent<OVRSkeleton>();
        if (this.trackedHand == TrackedHand.LeftHand || this.trackedHand == TrackedHand.BothHands) {
            switch (this.leftHandTrackingPoints) {
                case TrackingPoints.AllBones:
                    this.numberOfTrackingPointsLeft = this.leftHandSkeleton.GetCurrentNumBones();
                    break;
                case TrackingPoints.AllFingerTips:
                    this.numberOfTrackingPointsLeft = 5;
                    break;
                case TrackingPoints.IndexFingerTipOnly:
                default:
                    this.numberOfTrackingPointsLeft = 1;
                    break;
            }
        }
        if (this.trackedHand == TrackedHand.RightHand || this.trackedHand == TrackedHand.BothHands) {
            switch (this.rightHandTrackingPoints) {
                case TrackingPoints.AllBones:
                    this.numberOfTrackingPointsRight = this.rightHandSkeleton.GetCurrentNumBones();
                    break;
                case TrackingPoints.AllFingerTips:
                    this.numberOfTrackingPointsRight = 5;
                    break;
                case TrackingPoints.IndexFingerTipOnly:
                default:
                    this.numberOfTrackingPointsRight = 1;
                    break;
            }
        }
        int numberOfParts = this.numberOfTrackingPointsLeft + this.numberOfTrackingPointsRight;

        // Active license if set:
        gc = new GestureCombinations(numberOfParts);
        if (this.LicenseName != null && this.LicenseKey != null && this.LicenseName != "") {
            ret = gc.activateLicense(this.LicenseName, this.LicenseKey);
            if (ret != 0) {
                Debug.LogError($"[MivryQuestHands] Failed to activate license: {GestureRecognition.getErrorMessage(ret)}");
            }
        } else if (this.LicenseFilePath != null && this.LicenseFilePath.Length > 0) {
            ret = gc.activateLicenseFile(this.LicenseFilePath);
            if (ret != 0) {
                Debug.LogError($"[MivryQuestHands] Failed to activate license: {GestureRecognition.getErrorMessage(ret)}");
            }
        }

        // Load gesture database file:
#if UNITY_EDITOR
        string GesturesFilePath = Application.streamingAssetsPath;
#elif UNITY_ANDROID
        // On android, the file is in the .apk,
        // so we need to first "download" it to the apps' cache folder.
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        string GesturesFilePath = activity.Call <AndroidJavaObject>("getCacheDir").Call<string>("getCanonicalPath");
        UnityWebRequest request = UnityWebRequest.Get(Application.streamingAssetsPath + "/" + this.GestureDatabaseFile);
        request.SendWebRequest();
        while (!request.isDone) {
            // wait for file extraction to finish
        }
        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.LogError("Failed to extract sample gesture database file from apk.");
            return;
        }
        string path = GesturesFilePath + "/" + this.GestureDatabaseFile;
        try {
            Directory.CreateDirectory(path);
            Directory.Delete(path);
        } catch (Exception) { }
        try {
            File.WriteAllBytes(path, request.downloadHandler.data);
        } catch (Exception e) {
            Debug.LogError("Exception writing temporary file: " + e.ToString());
            return;
        }
#else
        // This will be the case for a stand-alone PC app.
        // In this case, we can load the gesture database file from the StreamingAssets folder.
        string GesturesFilePath = Application.streamingAssetsPath;
#endif
        GesturesFilePath = GesturesFilePath + "/" + this.GestureDatabaseFile;
        ret = this.gc.loadFromFile(GesturesFilePath);
        if (ret != 0) {
            byte[] fileContents = File.ReadAllBytes(GesturesFilePath);
            if (fileContents == null || fileContents.Length == 0) {
                Debug.LogError($"Could not find gesture database file ({GesturesFilePath}).");
            } else {
                ret = gc.loadFromBuffer(fileContents);
                if (ret != 0) {
                    Debug.LogError("[MivryQuestHands] Failed to load gesture recognition database file: " + GestureRecognition.getErrorMessage(ret));
                    return;
                }
            }
        }

        // Set the tracking points to what's described in the file's metadata
        string metadata = this.gc.getMetadataAsString();
        if (metadata == null || metadata == "") {
            Debug.LogWarning("[MivryQuestHands] Failed to get metadata from database file");
            return;
        }
        string[] metadata_parts = metadata.Split(' ');
        if (metadata_parts.Length != 3 || metadata_parts[0] != "MivryQuestHands") {
            Debug.LogWarning("[MivryQuestHands] Failed to parse metadata from database file");
            return;
        }
        string[] metadata_lhtp = metadata_parts[1].Split('=');
        string[] metadata_rhtp = metadata_parts[2].Split('=');
        if (metadata_lhtp.Length != 2 || metadata_rhtp.Length != 2) {
            Debug.LogWarning("[MivryQuestHands] Failed to parse metadata from database file");
            return;
        }
        switch (metadata_lhtp[1]) {
            case "AllBones":
                this.leftHandTrackingPoints = MivryQuestHands.TrackingPoints.AllBones;
                break;
            case "AllFingerTips":
                this.leftHandTrackingPoints = MivryQuestHands.TrackingPoints.AllFingerTips;
                break;
            case "IndexFingerTipOnly":
                this.leftHandTrackingPoints = MivryQuestHands.TrackingPoints.IndexFingerTipOnly;
                break;
            default:
                Debug.LogWarning("[MivryQuestHands] Failed to parse metadata from database file");
                break;
        }
        switch (metadata_rhtp[1]) {
            case "AllBones":
                this.rightHandTrackingPoints = MivryQuestHands.TrackingPoints.AllBones;
                break;
            case "AllFingerTips":
                this.rightHandTrackingPoints = MivryQuestHands.TrackingPoints.AllFingerTips;
                break;
            case "IndexFingerTipOnly":
                this.rightHandTrackingPoints = MivryQuestHands.TrackingPoints.IndexFingerTipOnly;
                break;
            default:
                Debug.LogWarning("[MivryQuestHands] Failed to parse metadata from database file");
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        Transform t;
        Vector3 p;
        Quaternion q;

        if (this.trackedHand == TrackedHand.LeftHand || this.trackedHand == TrackedHand.BothHands) {
            if (!this.isGesturingLeft) { // Not currently gesturing - check if a new gesture motion was started.
                if (this.isLeftTriggerPressed) {
                    p = Camera.main.transform.position;
                    q = Camera.main.transform.rotation;
                    Mivry.convertHeadInput(this.mivryCoordinateSystem, ref p, ref q);
                    for (int i = this.numberOfTrackingPointsLeft - 1; i > 0; i--) {
                        this.gc.startStroke(i, p, q, this.recordGestureSample);
                    }
                    this.isGesturingLeft = true;
                }
            }
            if (this.isGesturingLeft) { // Currently gesturing - add latest data.
                switch (this.leftHandTrackingPoints) {
                    case TrackingPoints.AllBones:
                        for (int i = this.leftHandSkeleton.GetCurrentNumBones() - 1; i >= 0; i--) {
                            t = leftHandSkeleton.Bones[i].Transform;
                            p = t.position;
                            q = t.rotation;
                            Mivry.convertHandInput(this.unityXrPlugin, this.mivryCoordinateSystem, ref p, ref q);
                            this.gc.contdStrokeQ(i, p, q);
                        }
                        break;
                    case TrackingPoints.AllFingerTips:
                        t = leftHandSkeleton.Bones[(int)OVRSkeleton.BoneId.Hand_ThumbTip].Transform;
                        p = t.position;
                        q = t.rotation;
                        Mivry.convertHandInput(this.unityXrPlugin, this.mivryCoordinateSystem, ref p, ref q);
                        this.gc.contdStrokeQ(0, p, q);
                        t = leftHandSkeleton.Bones[(int)OVRSkeleton.BoneId.Hand_IndexTip].Transform;
                        p = t.position;
                        q = t.rotation;
                        Mivry.convertHandInput(this.unityXrPlugin, this.mivryCoordinateSystem, ref p, ref q);
                        this.gc.contdStrokeQ(1, p, q);
                        t = leftHandSkeleton.Bones[(int)OVRSkeleton.BoneId.Hand_MiddleTip].Transform;
                        p = t.position;
                        q = t.rotation;
                        Mivry.convertHandInput(this.unityXrPlugin, this.mivryCoordinateSystem, ref p, ref q);
                        this.gc.contdStrokeQ(2, p, q);
                        t = leftHandSkeleton.Bones[(int)OVRSkeleton.BoneId.Hand_RingTip].Transform;
                        p = t.position;
                        q = t.rotation;
                        Mivry.convertHandInput(this.unityXrPlugin, this.mivryCoordinateSystem, ref p, ref q);
                        this.gc.contdStrokeQ(3, p, q);
                        t = leftHandSkeleton.Bones[(int)OVRSkeleton.BoneId.Hand_PinkyTip].Transform;
                        p = t.position;
                        q = t.rotation;
                        Mivry.convertHandInput(this.unityXrPlugin, this.mivryCoordinateSystem, ref p, ref q);
                        this.gc.contdStrokeQ(4, p, q);
                        break;
                    case TrackingPoints.IndexFingerTipOnly:
                    default:
                        t = leftHandSkeleton.Bones[(int)OVRSkeleton.BoneId.Hand_IndexTip].Transform;
                        p = t.position;
                        q = t.rotation;
                        Mivry.convertHandInput(this.unityXrPlugin, this.mivryCoordinateSystem, ref p, ref q);
                        this.gc.contdStrokeQ(0, p, q);
                        break;
                }
                if (!this.isLeftTriggerPressed) {
                    for (int i = this.numberOfTrackingPointsLeft - 1; i > 0; i--) {
                        this.gc.endStroke(i);
                    }
                    this.isGesturingLeft = false;
                    if (!this.isGesturingRight) {
                        data.gestureID = gc.identifyGestureCombination(ref data.similarity);
                        data.gestureName = (data.gestureID >= 0)
                            ? gc.getGestureCombinationName(data.gestureID)
                            : GestureRecognition.getErrorMessage(data.gestureID);
                        OnGestureCompletion.Invoke(data);
                        #if MIVRY_USE_BOLT
                        CustomEvent.Trigger(this.gameObject, data.gestureName, data);
                        #endif
                        data.parts = new GestureCompletionData.Part[0]; // reset
                    }
                }
            }
        }

        
        if (this.trackedHand == TrackedHand.RightHand || this.trackedHand == TrackedHand.BothHands) {
            if (!this.isGesturingRight) { // Not currently gesturing - check if a new gesture motion was started.
                if (this.isRightTriggerPressed) {
                    p = Camera.main.transform.position;
                    q = Camera.main.transform.rotation;
                    Mivry.convertHeadInput(this.mivryCoordinateSystem, ref p, ref q);
                    for (int i = this.numberOfTrackingPointsRight - 1; i > 0; i--) {
                        this.gc.startStroke(i, p, q, this.recordGestureSample);
                    }
                    this.isGesturingRight = true;
                }
            }
            if (this.isGesturingRight) { // Currently gesturing - add latest data.
                switch (this.rightHandTrackingPoints) {
                    case TrackingPoints.AllBones:
                        for (int i = this.rightHandSkeleton.GetCurrentNumBones() - 1; i >= 0; i--) {
                            t = rightHandSkeleton.Bones[i].Transform;
                            p = t.position;
                            q = t.rotation;
                            Mivry.convertHandInput(this.unityXrPlugin, this.mivryCoordinateSystem, ref p, ref q);
                            this.gc.contdStrokeQ(i, p, q);
                        }
                        break;
                    case TrackingPoints.AllFingerTips:
                        t = rightHandSkeleton.Bones[(int)OVRSkeleton.BoneId.Hand_ThumbTip].Transform;
                        p = t.position;
                        q = t.rotation;
                        Mivry.convertHandInput(this.unityXrPlugin, this.mivryCoordinateSystem, ref p, ref q);
                        this.gc.contdStrokeQ(0, p, q);
                        t = rightHandSkeleton.Bones[(int)OVRSkeleton.BoneId.Hand_IndexTip].Transform;
                        p = t.position;
                        q = t.rotation;
                        Mivry.convertHandInput(this.unityXrPlugin, this.mivryCoordinateSystem, ref p, ref q);
                        this.gc.contdStrokeQ(1, p, q);
                        t = rightHandSkeleton.Bones[(int)OVRSkeleton.BoneId.Hand_MiddleTip].Transform;
                        p = t.position;
                        q = t.rotation;
                        Mivry.convertHandInput(this.unityXrPlugin, this.mivryCoordinateSystem, ref p, ref q);
                        this.gc.contdStrokeQ(2, p, q);
                        t = rightHandSkeleton.Bones[(int)OVRSkeleton.BoneId.Hand_RingTip].Transform;
                        p = t.position;
                        q = t.rotation;
                        Mivry.convertHandInput(this.unityXrPlugin, this.mivryCoordinateSystem, ref p, ref q);
                        this.gc.contdStrokeQ(3, p, q);
                        t = rightHandSkeleton.Bones[(int)OVRSkeleton.BoneId.Hand_PinkyTip].Transform;
                        p = t.position;
                        q = t.rotation;
                        Mivry.convertHandInput(this.unityXrPlugin, this.mivryCoordinateSystem, ref p, ref q);
                        this.gc.contdStrokeQ(4, p, q);
                        break;
                    case TrackingPoints.IndexFingerTipOnly:
                    default:
                        t = rightHandSkeleton.Bones[(int)OVRSkeleton.BoneId.Hand_IndexTip].Transform;
                        p = t.position;
                        q = t.rotation;
                        Mivry.convertHandInput(this.unityXrPlugin, this.mivryCoordinateSystem, ref p, ref q);
                        this.gc.contdStrokeQ(0, p, q);
                        break;
                }
                if (!this.isRightTriggerPressed) {
                    for (int i = this.numberOfTrackingPointsRight - 1; i > 0; i--) {
                        this.gc.endStroke(i);
                    }
                    this.isGesturingRight = false;
                    if (!this.isGesturingLeft) {
                        data.gestureID = gc.identifyGestureCombination(ref data.similarity);
                        data.gestureName = (data.gestureID >= 0)
                            ? gc.getGestureCombinationName(data.gestureID)
                            : GestureRecognition.getErrorMessage(data.gestureID);
                        OnGestureCompletion.Invoke(data);
                        #if MIVRY_USE_BOLT
                        CustomEvent.Trigger(this.gameObject, data.gestureName, data);
                        #endif
                        data.parts = new GestureCompletionData.Part[0]; // reset
                    }
                }
            }
        }
    }

    #region UTILITY_FUNCTIONS

    public static int[] getTrackingPointsIndices(TrackingPoints trackingPoints)
    {
        switch (trackingPoints) {
            case TrackingPoints.AllBones:
                return new int[24] {0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23};
            case TrackingPoints.AllFingerTips:
                return new int[5] {
                    (int)OVRSkeleton.BoneId.Hand_ThumbTip,
                    (int)OVRSkeleton.BoneId.Hand_IndexTip,
                    (int)OVRSkeleton.BoneId.Hand_MiddleTip,
                    (int)OVRSkeleton.BoneId.Hand_RingTip,
                    (int)OVRSkeleton.BoneId.Hand_PinkyTip,
                };
            case TrackingPoints.IndexFingerTipOnly:
                return new int[1] {
                    (int)OVRSkeleton.BoneId.Hand_IndexTip
                };
        }
        return new int[0];
    }

    public static float getGrabStrength(OVRSkeleton handSkeleton)
    {
        if (handSkeleton?.Bones?.Count <= (int)OVRSkeleton.BoneId.Hand_Pinky2) {
            return 0.0f;
        }
        float minStrength = 1.0f;
        Quaternion wristRotation = handSkeleton.Bones[(int)OVRSkeleton.BoneId.Hand_WristRoot].Transform.rotation;
        float indexBend = Quaternion.Angle(
            wristRotation,
            handSkeleton.Bones[(int)OVRSkeleton.BoneId.Hand_Index2].Transform.rotation
        );
        float indexStrength = Mathf.Clamp((indexBend - 80.0f) / 100.0f,  0, 1);
        minStrength = Mathf.Min(minStrength, indexStrength);
        float middleBend = Quaternion.Angle(
            wristRotation,
            handSkeleton.Bones[(int)OVRSkeleton.BoneId.Hand_Middle2].Transform.rotation
        );
        float middleStrength = Mathf.Clamp((middleBend - 80.0f) / 100.0f, 0, 1);
        minStrength = Mathf.Min(minStrength, middleStrength);
        float ringBend = Quaternion.Angle(
            wristRotation,
            handSkeleton.Bones[(int)OVRSkeleton.BoneId.Hand_Ring2].Transform.rotation
        );
        float ringStrength = Mathf.Clamp((ringBend - 80.0f) / 100.0f, 0, 1);
        minStrength = Mathf.Min(minStrength, ringStrength);
        float pinkyBend = Quaternion.Angle(
            wristRotation,
            handSkeleton.Bones[(int)OVRSkeleton.BoneId.Hand_Pinky2].Transform.rotation
        );
        float pinkyStrength = Mathf.Clamp((pinkyBend - 80.0f) / 100.0f, 0, 1);
        minStrength = Mathf.Min(minStrength, pinkyStrength);
        return minStrength;
    }

    public static float getTriggerValue(GestureTrigger trigger, OVRHand leftHand, OVRHand rightHand)
    {
        switch (trigger) {
            case GestureTrigger.LeftHandPinch:
                return leftHand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
            case GestureTrigger.LeftHandGrab:
                return getGrabStrength(leftHand.GetComponent<OVRSkeleton>());
            case GestureTrigger.RightHandPinch:
                return rightHand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
            case GestureTrigger.RightHandGrab:
                return getGrabStrength(rightHand.GetComponent<OVRSkeleton>());
            // case GestureTrigger.Manual:
        }
        return 0.0f;
    }

    /// <summary>
    /// Helper function to check if the gesture trigger for the left hand is currenty 'on' (pressed) or 'off'.
    /// </summary>
    private bool isLeftTriggerPressed
    {
        get {
            switch (this.leftGestureTrigger) {
                case GestureTrigger.LeftHandPinch:
                    this.leftGestureTriggerValue = this.leftHand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
                    break;
                case GestureTrigger.LeftHandGrab:
                    this.leftGestureTriggerValue = getGrabStrength(leftHand.GetComponent<OVRSkeleton>());
                    break;
                case GestureTrigger.RightHandPinch:
                    this.leftGestureTriggerValue = this.rightHand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
                    break;
                case GestureTrigger.RightHandGrab:
                    this.leftGestureTriggerValue = getGrabStrength(rightHand.GetComponent<OVRSkeleton>());
                    break;
                // case GestureTrigger.Manual:
            }
            Debug.Log($"this.leftGestureTriggerValue = {this.leftGestureTriggerValue}");
            return this.leftGestureTriggerValue >= this.leftGestureTriggerThreshold;
        }
    }

    /// <summary>
    /// Helper function to check if the gesture trigger for the left hand is currenty 'on' (pressed) or 'off'.
    /// </summary>
    private bool isRightTriggerPressed
    {
        get
        {
            switch (this.rightGestureTrigger) {
                case GestureTrigger.LeftHandPinch:
                    this.rightGestureTriggerValue = this.leftHand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
                    break;
                case GestureTrigger.LeftHandGrab:
                    this.rightGestureTriggerValue = getGrabStrength(leftHand.GetComponent<OVRSkeleton>());
                    break;
                case GestureTrigger.RightHandPinch:
                    this.rightGestureTriggerValue = this.rightHand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
                    break;
                case GestureTrigger.RightHandGrab:
                    this.rightGestureTriggerValue = getGrabStrength(rightHand.GetComponent<OVRSkeleton>());
                    break;
                    // case GestureTrigger.Manual:
            }
            return this.rightGestureTriggerValue >= this.rightGestureTriggerThreshold;
        }
    }

    #endregion
}
