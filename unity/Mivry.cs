/*
 * MiVRy - VR gesture recognition library plug-in for Unity.
 * Version 1.16
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
 * # HOW TO USE:
 * 
 * (1) Add this script as a component to one (any) object in your scene.
 * 
 * 
 * (2) In one of your own scripts, add a new function to handle the event
 * when a gesture is performed and recognized. The function should have a
 * parameter of the type GestureCompletionData and return type void.
 * Example:
 * <code>
 * public void OnGestureCompleted(GestureCompletionData data) {
 *  if (data.gestureID == 123) {
 *    ...
 *  }
 * }
 * </code>
 * 
 * 
 * (3) In the inspector, set the fields of the MiVRy script components:
 * - "GestureDatabaseFile":
 *   The path to the gesture recognition database file to load.
 *   In the editor, this will be relative to the Assets/ folder.
 *   In stand-alone (build), this will be relative to the StreamingAssets/ folder.
 * - "LeftHand" / "RightHand":
 *   A game object that will be used as the position of the left hand.
 * - "LeftTriggerInput" / "RightTriggerInput":
 *   The name of the input in the Input Manager (in Project settings)
 *   which will be used to start/end the gesture.
 * - "LeftTriggerInputType" / "RightTriggerInputType":
 *   The type of the input (Axis, Button, or Key) which triggers the gesture.
 * - "LeftTriggerPressure" / "RightTriggerPressure":
 *   If the input type is axis, how strongly (on a scale from zero to one) does
 *   the axis have to be pressed to trigger the start of the gesture.
 * - "OnGestureCompletion":
 *   Event callback functions to be called when a gesture was performed.
 * 
 */

using System;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

/// <summary>
/// Data regarding the identified gesture.
/// This will be provided to the registered event handler function.
/// </summary>
[System.Serializable]
public class GestureCompletionData
{
    /// <summary>
    /// The ID (index) of the gesture which was just identified.
    /// '-1' if no gesture could be identified.
    /// </summary>
    public int gestureID;
    /// <summary>
    /// The name of the gesture which was just identified.
    /// </summary>
    public string gestureName;
    /// <summary>
    /// The similarity between the performed gesture and the identified
    /// gesture on a scale from 0 (no similarity) to 1 (perfect match).
    /// </summary>
    public double similarity;
    /// <summary>
    /// Data about the individual parts of the gesture (if two-handed / multi-part gesture).
    /// </summary>
    public class Part
    {
        /// <summary>
        /// Index of the side (left or right) of the hand that performed the gesture part.
        /// </summary>
        public enum Side { Left=0, Right=1 };
        /// <summary>
        /// Which side (hand) performed this gesture part.
        /// </summary>
        public Side side;
        /// <summary>
        /// Where in the scene the gesture was performed.
        /// </summary>
        public Vector3 position;
        /// <summary>
        /// At which scale the gesture was performed.
        /// </summary>
        public double scale;
        /// <summary>
        /// In which direction the gesture was performed.
        /// </summary>
        public Quaternion orientation;
        /// <summary>
        /// Primary axis along which the gesture was performed.
        /// For example, a figure eight ('8') starting from the top
        /// will have the primary direction "downwards".
        /// </summary>
        public Vector3 primaryDirection;
        /// <summary>
        /// Secondary axis along which the gesture was performed.
        /// For example, a figure eight ('8')
        /// will have the secondary direction "right".
        /// </summary>
        public Vector3 secondaryDirection;
    };
    /// <summary>
    /// Data about the individual parts of the gesture.
    /// If the gesture was one-handed, only one part will be provided.
    /// If the gesture was two-handed, two parts are provided.
    /// </summary>
    public Part[] parts = new Part[0];
}

/// <summary>
/// Event callback function object type definition.
/// </summary>
[System.Serializable]
public class GestureCompletionEvent : UnityEvent<GestureCompletionData>
{

}

/// <summary>
/// Convenience Unity script that can be added as a component to any script
/// for easy gesture recognition.
/// </summary>
public class Mivry : MonoBehaviour
{
    /// <summary>
    /// Type of input to use to trigger the start/end of a gesture.
    /// </summary>
    [System.Serializable]
    public enum InputType {
        Axis,
        Button,
        Key
    };

    /// <summary>
    /// The path to the gesture recognition database file to load.
    /// In the editor, this will be relative to the Assets/ folder.
    /// In stand-alone (build), this will be relative to the StreamingAssets/ folder.
    /// </summary>
    [Tooltip("Path to the .dat file to load gestures from.")]
    public string GestureDatabaseFile = "";

    /// <summary>
    /// A game object that will be used as the position of the left hand.
    /// </summary>
    [Tooltip("GameObject to use as the position of the left hand.")]
    public GameObject LeftHand = null;

    /// <summary>
    /// The name of the input in the Input Manager (in Project settings)
    /// which will be used to start/end the gesture.
    /// </summary>
    [Tooltip("The name of the input (in Project Settings -> Input Manager) that triggers the start/end of a gesture.")]
    public string LeftTriggerInput = "";

    /// <summary>
    /// The type of the input (Axis, Button, or Key) which triggers the gesture.
    /// </summary>
    [Tooltip("Type of the input (in Project Settings -> Input Manager) that triggers the start/end of a gesture.")]
    public InputType LeftTriggerInputType = InputType.Axis;

    /// <summary>
    /// If the input type is axis, how strongly (on a scale from zero to one) does
    /// the axis have to be pressed to trigger the start of the gesture.
    /// </summary>
    [Tooltip("If the input is 'Axis', how strongly it has to be pressed (0~1) to start the gesture.")]
    public float LeftTriggerPressure = 0.9f;

    /// <summary>
    /// A game object that will be used as the position of the right hand.
    /// </summary>
    [Tooltip("GameObject to use as the position of the right hand.")]
    public GameObject RightHand = null;

    /// <summary>
    /// The name of the input in the Input Manager (in Project settings)
    /// which will be used to start/end the gesture.
    /// </summary>
    [Tooltip("The name of the input (in Project Settings -> Input Manager) that triggers the start/end of a gesture.")]
    public string RightTriggerInput = "";

    /// <summary>
    /// The type of the input (Axis, Button, or Key) which triggers the gesture.
    /// </summary>
    [Tooltip("Type of the input (in Project Settings -> Input Manager) that triggers the start/end of a gesture.")]
    public InputType RightTriggerInputType = InputType.Axis;

    /// <summary>
    /// If the input type is axis, how strongly (on a scale from zero to one) does
    /// the axis have to be pressed to trigger the start of the gesture.
    /// </summary>
    [Tooltip("How strong the button has to be pressed (0~1) to start the gesture.")]
    public float RightTriggerPressure = 0.9f;

    /// <summary>
    /// Event callback functions to be called when a gesture was performed.
    /// </summary>
    [Tooltip("Event to trigger when a gesture was performed.")]
    [SerializeField]
    public GestureCompletionEvent OnGestureCompletion = null;

    private bool LeftHandActive = false;
    private bool RightHandActive = false;

    private GestureRecognition gr = null;
    private GestureCombinations gc = null;
    private GestureCompletionData data = new GestureCompletionData();

    /// <summary>
    /// Unity start function.
    /// </summary>
    void Start()
    {
#if UNITY_EDITOR
        // When running the scene inside the Unity editor,
        // we can just load the file from the Assets/ folder:
        string GesturesFilePath = "Assets";
#elif UNITY_ANDROID
        // On android, the file is in the .apk,
        // so we need to first "download" it to the apps' cache folder.
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        string GesturesFilePath = activity.Call <AndroidJavaObject>("getCacheDir").Call<string>("getCanonicalPath");
        UnityWebRequest request = UnityWebRequest.Get(Application.streamingAssetsPath + "/" + GestureDatabaseFile);
        request.SendWebRequest();
        while (!request.isDone) {
            // wait for file extraction to finish
        }
        if (request.isNetworkError)
        {
            Debug.LogError("Failed to extract sample gesture database file from apk.");
            return;
        }
        string path = GesturesFilePath + "/" + GestureDatabaseFile;
        try {
            Directory.CreateDirectory(path);
            Directory.Delete(path);
        } catch (Exception e) { }
        try {
            File.WriteAllBytes(path, request.downloadHandler.data);
        } catch (Exception e) {
            Debug.LogError("Exception writing temporary file: " + e.ToString());
            return;
        }
#else
        // This will be the case when exporting a stand-alone PC app.
        // In this case, we can load the gesture database file from the streamingAssets folder.
        string GesturesFilePath = Application.streamingAssetsPath;
#endif
        // try to figure out if this is a gesture recognition or gesture combinations file
        gr = new GestureRecognition();
        int ret = gr.loadFromFile(GesturesFilePath + "/" + GestureDatabaseFile);
        if (ret == 0) // file loaded successfully
        {
            return;
        }
        gr = null;
        if (ret != -4) // "-4" ("invalid file") could mean that it's a GestureCombinations file 
        {
            Debug.LogError("[MiVRy] Failed to load gesture recognition database file: " + GestureRecognition.getErrorMessage(ret));
        }
        gc = new GestureCombinations(0);
        ret = gc.loadFromFile(GesturesFilePath + "/" + GestureDatabaseFile);
        if (ret != 0)
        {
            gc = null;
            Debug.LogError("[MiVRy] Failed to load gesture recognition database file: " + GestureRecognition.getErrorMessage(ret));
        }
    }

    /// <summary>
    /// Unity update function.
    /// </summary>
    void Update()
    {
        float leftTrigger = 0.0f;
        if (LeftTriggerInput != null && LeftTriggerInput.Length > 0) {
            switch (LeftTriggerInputType)
            {
                case InputType.Axis:
                    leftTrigger = Input.GetAxis(LeftTriggerInput);
                    break;
                case InputType.Button:
                    leftTrigger = Input.GetButton(LeftTriggerInput) ? 1 : 0;
                    break;
                case InputType.Key:
                    leftTrigger = Input.GetKey(LeftTriggerInput) ? 1 : 0;
                    break;
            }
        }
            
        float rightTrigger = 0.0f;
        if (RightTriggerInput != null && RightTriggerInput.Length > 0) {
            switch (RightTriggerInputType)
            {
                case InputType.Axis:
                    rightTrigger = Input.GetAxis(RightTriggerInput);
                    break;
                case InputType.Button:
                    rightTrigger = Input.GetButton(RightTriggerInput) ? 1 : 0;
                    break;
                case InputType.Key:
                    rightTrigger = Input.GetKey(RightTriggerInput) ? 1 : 0;
                    break;
            }
        }

        if (gr != null)
        {
            this.UpdateGR(leftTrigger, rightTrigger);
        } else if (gc != null)
        {
            this.UpdateGC(leftTrigger, rightTrigger);
        }
    }


    /// <summary>
    /// Update function for one-handed gesture recognition
    /// </summary>
    void UpdateGR(float leftTrigger, float rightTrigger)
    {   
        if (LeftHandActive == false && RightHandActive == false)
        {
            if (leftTrigger > LeftTriggerPressure)
            {
                LeftHandActive = true;
            }
            else if (rightTrigger > RightTriggerPressure)
            {
                RightHandActive = true;
            } else
            {
                return; // neither button pressed: nothing to do
            }
            Transform hmd = Camera.main.gameObject.transform;
            gr.startStroke(hmd.position, hmd.rotation);
        }

        GameObject activeGameObject = LeftHand;
        float activeTrigger = leftTrigger;
        GestureCompletionData.Part.Side side = GestureCompletionData.Part.Side.Left;
        float activeTriggerPressure = LeftTriggerPressure;
        if (RightHandActive)
        {
            activeGameObject = RightHand;
            activeTrigger = rightTrigger;
            side = GestureCompletionData.Part.Side.Right;
            activeTriggerPressure = RightTriggerPressure;
        }
        if (activeTrigger > activeTriggerPressure * 0.9f)
        {
            // still gesturing
            gr.contdStrokeQ(activeGameObject.transform.position, activeGameObject.transform.rotation);
            return;
        }
        // else: user released the trigger, ending the gesture
        Array.Resize<GestureCompletionData.Part>(ref data.parts, 1);
        GestureCompletionData.Part part = data.parts[0] = new GestureCompletionData.Part();
        part.side = side;
        data.gestureID = gr.endStroke(
            ref data.similarity,
            ref part.position,
            ref part.scale,
            ref part.orientation);
        part.primaryDirection = part.orientation * Vector3.right;
        part.secondaryDirection = part.orientation * Vector3.up;
        data.gestureName = gr.getGestureName(data.gestureID);
        OnGestureCompletion.Invoke(data);
        LeftHandActive = false;
        RightHandActive = false;
    }

    /// <summary>
    /// Update function for two-handed gesture combinations.
    /// </summary>
    void UpdateGC(float leftTrigger, float rightTrigger)
    {
        if (LeftHandActive == false)
        {
            if (leftTrigger > LeftTriggerPressure)
            {
                Transform hmd = Camera.main.gameObject.transform;
                gc.startStroke((int)GestureCompletionData.Part.Side.Left, hmd.position, hmd.rotation);
                LeftHandActive = true;
            }
        }
        if (RightHandActive == false)
        {
            if (rightTrigger > RightTriggerPressure)
            {
                Transform hmd = Camera.main.gameObject.transform;
                gc.startStroke((int)GestureCompletionData.Part.Side.Right, hmd.position, hmd.rotation);
                RightHandActive = true;
            }
        }
        if (LeftHandActive == true)
        {
            if (leftTrigger > LeftTriggerPressure * 0.9f)
            {
                gc.contdStrokeQ(
                    (int)GestureCompletionData.Part.Side.Left,
                    LeftHand.transform.position,
                    LeftHand.transform.rotation);
            } else
            {
                GestureCompletionData.Part part = null;
                foreach (var partK in data.parts)
                {
                    if (partK.side == GestureCompletionData.Part.Side.Left)
                    {
                        part = partK;
                        break;
                    }
                }
                if (part == null)
                {
                    Array.Resize<GestureCompletionData.Part>(ref data.parts, data.parts.Length + 1);
                    part = data.parts[data.parts.Length - 1] = new GestureCompletionData.Part();
                    part.side = GestureCompletionData.Part.Side.Left;
                }
                gc.endStroke((int)GestureCompletionData.Part.Side.Left, ref part.position, ref part.scale, ref part.orientation);
                part.primaryDirection = part.orientation * Vector3.right;
                part.secondaryDirection = part.orientation * Vector3.up;
                LeftHandActive = false;
                if (RightHandActive == false)
                {
                    // both finished or the other hand never started
                    data.gestureID = gc.identifyGestureCombination(ref data.similarity);
                    data.gestureName = gc.getGestureCombinationName(data.gestureID);
                    OnGestureCompletion.Invoke(data);
                    data.parts = new GestureCompletionData.Part[0]; // reset
                }
            }
        }
        if (RightHandActive == true)
        {
            if (rightTrigger > RightTriggerPressure*0.9f)
            {
                gc.contdStrokeQ(
                    (int)GestureCompletionData.Part.Side.Right,
                    RightHand.transform.position,
                    RightHand.transform.rotation);
            }
            else
            {
                GestureCompletionData.Part part = null;
                foreach (var partK in data.parts)
                {
                    if (partK.side == GestureCompletionData.Part.Side.Right)
                    {
                        part = partK;
                        break;
                    }
                }
                if (part == null)
                {
                    Array.Resize<GestureCompletionData.Part>(ref data.parts, data.parts.Length + 1);
                    part = data.parts[data.parts.Length - 1] = new GestureCompletionData.Part();
                    part.side = GestureCompletionData.Part.Side.Right;
                }
                gc.endStroke((int)GestureCompletionData.Part.Side.Right, ref part.position, ref part.scale, ref part.orientation);
                part.primaryDirection = part.orientation * Vector3.right;
                part.secondaryDirection = part.orientation * Vector3.forward;
                RightHandActive = false;
                if (LeftHandActive == false)
                {
                    // both finished or the other hand never started
                    data.gestureID = gc.identifyGestureCombination(ref data.similarity);
                    data.gestureName = gc.getGestureCombinationName(data.gestureID);
                    OnGestureCompletion.Invoke(data);
                    data.parts = new GestureCompletionData.Part[0]; // reset
                }
            }
        }
    }
}
