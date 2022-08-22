/*
 * MiVRy - 3D gesture recognition library plug-in for Unity.
 * Version 2.5
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
 * - "LeftTriggerThreshold" / "RightTriggerThreshold":
 *   If the input type is axis, how strongly (on a scale from zero to one) does
 *   the axis have to be pressed to trigger the start of the gesture.
 * - "OnGestureCompletion":
 *   Event callback functions to be called when a gesture was performed.
 * 
 * (4) To use MiVRy in Bolt state graphs, uncomment the following line "#define MIVRY_USE_BOLT"
 * by removing the "//" at the beginning of the line, then use the Bolt "Unit Options Wizard"
 * to add the "MiVRy" script and "Gesture Completion Data" to the Bolt "Type Options".
 * Then you can use Custom Events with the name of the gesture in Bolt state graphs to trigger
 * transitions.
 */
//#define MIVRY_USE_BOLT

using System;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif
#if UNITY_ANDROID
using UnityEngine.Networking;
#endif

#if MIVRY_USE_BOLT
using Ludiq;
using Bolt;
#endif

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
    /// Which Unity XR plug-in is used (see Unity Package Manager and Project Settings -> XR Plugin Management).
    /// </summary>
    [System.Serializable]
    public enum UnityXrPlugin
    {
        OpenXR,
        OculusVR,
        SteamVR
    };

    /// <summary>
    /// Which coordinate system Mivry uses internally (in the Gesture Database file).
    /// </summary>
    [System.Serializable]
    public enum MivryCoordinateSystem
    {
        OpenXR,
        OculusVR,
        SteamVR,
        UnrealEngine
    };

    /// <summary>
    /// Type of input to use to trigger the start/end of a gesture.
    /// </summary>
    [System.Serializable]
    public enum InputType {
#if ENABLE_INPUT_SYSTEM
        InputSystemControl,
#endif
        Axis,
        Button,
        Key,
        Value
    };

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
    /// The path to the gesture recognition database file to load.
    /// In the editor, this will be relative to the Assets/ folder.
    /// In stand-alone (build), this will be relative to the StreamingAssets/ folder.
    /// </summary>
    [Tooltip("Path to the .dat file to load gestures from.")]
    public string GestureDatabaseFile = "";

    /// <summary>
    /// Which Unity XR plugin is being used by this project.
    /// This is only important when the GestureDatabase file was created (or will be used) with a different PlugIn
    /// or on a different platform (such as Unreal Engine), because they are using different coordinate systems.
    /// See the Unity Package Manager and Project Settings -> XR Plugin Manager to see which plugin is being used.
    /// </summary>
    [Tooltip("The Unity XR plugin used in this project (see: Project Settings -> XR Plugin Manager).")]
    public UnityXrPlugin unityXrPlugin = UnityXrPlugin.OpenXR;

    /// <summary>
    /// The coordinate system MiVRy should use internally (or that the GestureDatabase file was created with).
    /// </summary>
    [Tooltip("The coordinate system MiVRy should use internally (or that the GestureDatabase file was created with).")]
    public MivryCoordinateSystem mivryCoordinateSystem = MivryCoordinateSystem.OpenXR;

    /// <summary>
    /// A game object that will be used as the position of the left hand.
    /// </summary>
    [Tooltip("GameObject to use as the position of the left hand.")]
    public GameObject LeftHand = null;

    /// <summary>
    /// The name of the input in the Input Manager (in Project settings)
    /// which will be used to start/end the gesture.
    /// </summary>
    [Tooltip("The name of the input (in Project Settings -> Input Manager) or Input System Control ('<XRController>{LeftHand}/trigger') that triggers the start/end of a gesture.")]
    public string LeftTriggerInput = "";

    /// <summary>
    /// The type of the input (Axis, Button, or Key) which triggers the gesture.
    /// "Input System Control" when using the new Input System.
    /// </summary>
    [Tooltip("Type of the input (in Project Settings -> Input Manager) that triggers the start/end of a gesture. 'Input System Control' when using the new Input System. 'Value' to manually set LeftTriggerValue.")]
    public InputType LeftTriggerInputType = InputType.Axis;

    /// <summary>
    /// The current value of the trigger input will be stored here.
    /// If the input type is 'value', this variable controls when a gesture starts/ends (>= threshold).
    /// </summary>
    [Tooltip("Current value of the trigger input (0~1). If the input type is 'value', this variable controls when a gesture starts/ends (>= threshold).")]
    public float LeftTriggerValue = 0.0f;

    /// <summary>
    /// If the input type is axis, how strongly (on a scale from zero to one) does
    /// the axis have to be pressed to trigger the start of the gesture.
    /// </summary>
    [Tooltip("If the input is 'Axis', how strongly it has to be pressed (0~1) to start the gesture.")]
    public float LeftTriggerThreshold = 0.9f;

    /// <summary>
    /// A game object that will be used as the position of the right hand.
    /// </summary>
    [Tooltip("GameObject to use as the position of the right hand.")]
    public GameObject RightHand = null;

    /// <summary>
    /// The name of the input in the Input Manager (in Project settings)
    /// which will be used to start/end the gesture.
    /// </summary>
    [Tooltip("The name of the input (in Project Settings -> Input Manager) or Input System Control ('<XRController>{LeftHand}/trigger') that triggers the start/end of a gesture.")]
    public string RightTriggerInput = "";

    /// <summary>
    /// The type of the input (Axis, Button, or Key) which triggers the gesture.
    /// "Input System Control" when using the new Input System.
    /// </summary>
    [Tooltip("Type of the input (in Project Settings -> Input Manager) that triggers the start/end of a gesture. 'Input System Control' when using the new Input System. 'Value' to manually set RightTriggerValue.")]
    public InputType RightTriggerInputType = InputType.Axis;

    /// <summary>
    /// The current value of the trigger input will be stored here.
    /// If the input type is 'value', this variable controls when a gesture starts/ends (>= threshold).
    /// </summary>
    [Tooltip("Current value of the trigger input (0~1). If the input type is 'value', this variable controls when a gesture starts/ends (>= threshold).")]
    public float RightTriggerValue = 0.0f;

    /// <summary>
    /// If the input type is 'axis', how strongly (on a scale from zero to one) does
    /// the axis have to be pressed to trigger the start of the gesture.
    /// </summary>
    [Tooltip("How strong the button has to be pressed (0~1) to start the gesture.")]
    public float RightTriggerThreshold = 0.9f;

    /// <summary>
    /// Whether gestures should be identified continuously during gesturing,
    /// instead of after a gesture motion is finished.
    /// </summary>
    [Tooltip("Whether gestures should be identified continuously during gesturing (instead of only after a gesture motion).")]
    public bool ContinuousGestureRecognition = false;

    /// <summary>
    /// Time frame (in milliseconds) that continuous gestures are expected to be.
    /// </summary>
    [Tooltip("Time frame (in milliseconds) that continuous gestures are expected to be.")]
    public int ContinuousGesturePeriod = 1000;

    /// <summary>
    /// Time frame (in milliseconds) that continuous gestures are expected to be.
    /// </summary>
    [Tooltip("Time frame (in milliseconds) that continuous gestures are expected to be.")]
    public int ContinuousGestureSmoothing = 3;

    /// <summary>
    /// Whether or not to compensate head motions during gesturing
    /// by continuously updating the current head position/rotation.
    /// </summary>
    public bool compensateHeadMotion = false;

    /// <summary>
    /// Event callback functions to be called when a gesture was performed.
    /// </summary>
    [Tooltip("Event to trigger when a gesture was performed.")]
    [SerializeField]
    public GestureCompletionEvent OnGestureCompletion = null;

#if ENABLE_INPUT_SYSTEM
    /// <summary>
    /// InputAction compatible function to start/stop gesture motions.
    /// To use this function, set the "Behavior" of your "Player Input" component to "Invoke Unity Events",
    /// add an event listener by pressing for the input in your "Action Map", and select this function as target.
    /// </summary>
    /// <param name="callbackContext">The InputAction CallbackContext</param>
    public void OnInputAction_LeftTrigger(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.action.type == InputActionType.Button) {
            if (callbackContext.started) {
                this.InputAction_LeftTriggerPressed = true;
            }
            if (callbackContext.canceled) {
                this.InputAction_LeftTriggerPressed = false;
            }
            return;
        }
        this.LeftTriggerValue = this.getInputControlValue(callbackContext.action.activeControl);
        this.InputAction_LeftTriggerPressed = this.InputAction_LeftTriggerPressed
            ? this.LeftTriggerValue >= this.LeftTriggerThreshold * 0.9f
            : this.LeftTriggerValue >= this.LeftTriggerThreshold;
    }

    /// <summary>
    /// InputAction compatible function to start a gesture motion.
    /// You can use these in your project by adding it to your Input Controls, for example:
    /// private void Awake()
    /// {
    ///     GameObject mivryGameObject = GameObject.Find("Name of the GameObject with MiVRy component");
    ///     Mivry mivryComponent = mivryGameObject.GetComponent<Mivry>();
    ///     this.myControls = new MyInputActionControls();
    ///     this.myControls.DefaultActionMap.userPressedTheLeftTriggerAction.Enable();
    ///     this.myControls.DefaultActionMap.userPressedTheLeftTriggerAction.started += mivryComponent.OnInputAction_LeftTriggerPress;
    /// }
    /// </summary>
    /// <param name="callbackContext">The InputAction CallbackContext</param>
    public void OnInputAction_LeftTriggerPress(InputAction.CallbackContext callbackContext)
    {
        this.InputAction_LeftTriggerPressed = true;
    }

    /// <summary>
    /// InputAction compatible function to start a gesture motion.
    /// You can use these in your project by adding it to your Input Controls, for example:
    /// private void Awake()
    /// {
    ///     GameObject mivryGameObject = GameObject.Find("Name of the GameObject with MiVRy component");
    ///     Mivry mivryComponent = mivryGameObject.GetComponent<Mivry>();
    ///     this.myControls = new MyInputActionControls();
    ///     this.myControls.DefaultActionMap.userPressedTheLeftTriggerAction.Enable();
    ///     this.myControls.DefaultActionMap.userPressedTheLeftTriggerAction.performed += mivryComponent.OnInputAction_LeftTriggerRelease;
    ///     this.myControls.DefaultActionMap.userPressedTheLeftTriggerAction.canceled += mivryComponent.OnInputAction_LeftTriggerRelease;
    /// }
    /// </summary>
    /// <param name="callbackContext">The InputAction CallbackContext</param>
    public void OnInputAction_LeftTriggerRelease(InputAction.CallbackContext callbackContext)
    {
        this.InputAction_LeftTriggerPressed = false;
    }

    /// <summary>
    /// InputAction compatible function to start/stop gesture motions.
    /// To use this function, set the "Behavior" of your "Player Input" component to "Invoke Unity Events",
    /// add an event listener by pressing for the input in your "Action Map", and select this function as target.
    /// </summary>
    /// <param name="callbackContext">The InputAction CallbackContext</param>
    public void OnInputAction_RightTrigger(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.action.type == InputActionType.Button) {
            if (callbackContext.started) {
                this.InputAction_RightTriggerPressed = true;
            }
            if (callbackContext.canceled) {
                this.InputAction_RightTriggerPressed = false;
            }
            return;
        }
        this.RightTriggerValue = this.getInputControlValue(callbackContext.action.activeControl);
        this.InputAction_RightTriggerPressed = this.InputAction_RightTriggerPressed
            ? this.RightTriggerValue >= this.RightTriggerThreshold * 0.9f
            : this.RightTriggerValue >= this.RightTriggerThreshold;
    }

    /// <summary>
    /// InputAction compatible function to start a gesture motion.
    /// You can use these in your project by adding it to your Input Controls, for example:
    /// private void Awake()
    /// {
    ///     GameObject mivryGameObject = GameObject.Find("Name of the GameObject with MiVRy component");
    ///     Mivry mivryComponent = mivryGameObject.GetComponent<Mivry>();
    ///     this.myControls = new MyInputActionControls();
    ///     this.myControls.DefaultActionMap.userPressedTheRightTriggerAction.Enable();
    ///     this.myControls.DefaultActionMap.userPressedTheRightTriggerAction.started += mivryComponent.OnInputAction_RightTriggerPress;
    /// }
    /// </summary>
    /// <param name="callbackContext">The InputAction CallbackContext</param>
    public void OnInputAction_RightTriggerPress(InputAction.CallbackContext callbackContext)
    {
        this.InputAction_RightTriggerPressed = true;
    }

    /// <summary>
    /// InputAction compatible function to start a gesture motion.
    /// You can use these in your project by adding it to your Input Controls, for example:
    /// private void Awake()
    /// {
    ///     GameObject mivryGameObject = GameObject.Find("Name of the GameObject with MiVRy component");
    ///     Mivry mivryComponent = mivryGameObject.GetComponent<Mivry>();
    ///     this.myControls = new MyInputActionControls();
    ///     this.myControls.DefaultActionMap.userPressedTheRightTriggerAction.Enable();
    ///     this.myControls.DefaultActionMap.userPressedTheRightTriggerAction.performed += mivryComponent.OnInputAction_RightTriggerRelease;
    ///     this.myControls.DefaultActionMap.userPressedTheRightTriggerAction.canceled += mivryComponent.OnInputAction_RightTriggerRelease;
    /// }
    /// </summary>
    /// <param name="callbackContext">The InputAction CallbackContext</param>
    public void OnInputAction_RightTriggerRelease(InputAction.CallbackContext callbackContext)
    {
        this.InputAction_RightTriggerPressed = false;
    }

    private bool InputAction_LeftTriggerPressed = false;

    private bool InputAction_RightTriggerPressed = false;
    
    public float getInputControlValue(string controlName)
    {
        InputControl control = InputSystem.FindControl(controlName); // eg: "<XRController>{RightHand}/trigger"
        if (control == null) {
            Debug.LogError($"Mivry.getInputControlValue : Control '${controlName}' not found.");
            return 0.0f;
        }
        return getInputControlValue(control);
    }

    public float getInputControlValue(InputControl control)
    {
        switch (control) {
            case AxisControl axisControl:
                return axisControl.ReadValue();
            case DoubleControl doubleControl:
                return (float)doubleControl.ReadValue();
            case IntegerControl integerControl:
                return integerControl.ReadValue();
            case QuaternionControl quaternionControl:
                Debug.LogError($"Mivry.getInputControlValue : QuaternionControl '${control.name}' not supported.");
                return 0.0f;
            case TouchControl touchControl:
                return touchControl.ReadValue().pressure;
            case TouchPhaseControl phaseControl:
                var phase = phaseControl.ReadValue();
                switch (phase)
                {
                    case UnityEngine.InputSystem.TouchPhase.Began:
                    case UnityEngine.InputSystem.TouchPhase.Stationary:
                    case UnityEngine.InputSystem.TouchPhase.Moved:
                        return 1.0f;
                    case UnityEngine.InputSystem.TouchPhase.None:
                    case UnityEngine.InputSystem.TouchPhase.Ended:
                    case UnityEngine.InputSystem.TouchPhase.Canceled:
                    default:
                        return 0.0f;
                }
            case Vector2Control vector2Control:
                return vector2Control.ReadValue().magnitude;
            case Vector3Control vector3Control:
                return vector3Control.ReadValue().magnitude;

        }
        Debug.LogError($"Mivry.getInputControlValue : Unknown control type '${control.name}'.");
        return 0.0f;
    }
#endif

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
        int ret;
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
        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.LogError("Failed to extract sample gesture database file from apk.");
            return;
        }
        string path = GesturesFilePath + "/" + GestureDatabaseFile;
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
        // This will be the case when exporting a stand-alone PC app.
        // In this case, we can load the gesture database file from the streamingAssets folder.
        string GesturesFilePath = Application.streamingAssetsPath;
#endif
        GesturesFilePath = GesturesFilePath + "/" + GestureDatabaseFile;
        // try to figure out if this is a gesture recognition or gesture combinations file
        gr = new GestureRecognition();
        gr.contdIdentificationPeriod = this.ContinuousGesturePeriod;
        gr.contdIdentificationSmoothing = this.ContinuousGestureSmoothing;
        
        if (this.LicenseKey != null && this.LicenseName != null && this.LicenseName.Length > 0) {
            ret = this.gr.activateLicense(this.LicenseName, this.LicenseKey);
            if (ret != 0)
            {
                Debug.LogError("[MiVRy] Failed to activate license: " + GestureRecognition.getErrorMessage(ret));
            }
        }
        
        ret = gr.loadFromFile(GesturesFilePath);
        if (ret == 0) // file loaded successfully
        {
            return;
        }
        byte[] file_contents = File.ReadAllBytes(GesturesFilePath);
        if (file_contents == null || file_contents.Length == 0)
        {
            Debug.LogError($"Could not find gesture database file ({GesturesFilePath}).");
            return;
        }
        ret = gr.loadFromBuffer(file_contents);
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

        if (this.LicenseKey != null && this.LicenseName != null && this.LicenseName.Length > 0)
        {
            ret = this.gc.activateLicense(this.LicenseName, this.LicenseKey);
            if (ret != 0)
            {
                Debug.LogError("[MiVRy] Failed to activate license: " + GestureRecognition.getErrorMessage(ret));
            }
        }

        ret = gc.loadFromFile(GesturesFilePath);
        if (ret == 0) // file loaded successfully
        {
            for (int part = gc.numberOfParts()-1; part>=0; part--)
            {
                gc.setContdIdentificationPeriod(part, this.ContinuousGesturePeriod);
                gc.setContdIdentificationSmoothing(part, this.ContinuousGestureSmoothing);
            }
            return;
        }
        ret = gc.loadFromBuffer(file_contents);
        if (ret == 0) // file loaded successfully
        {
            for (int part = gc.numberOfParts() - 1; part >= 0; part--)
            {
                gc.setContdIdentificationPeriod(part, this.ContinuousGesturePeriod);
                gc.setContdIdentificationSmoothing(part, this.ContinuousGestureSmoothing);
            }
            return;
        }
        gc = null;
        Debug.LogError("[MiVRy] Failed to load gesture recognition database file: " + GestureRecognition.getErrorMessage(ret));
    }

    /// <summary>
    /// Unity update function.
    /// </summary>
    void Update()
    {
#if ENABLE_INPUT_SYSTEM
        if (InputAction_LeftTriggerPressed)
        {
            LeftTriggerValue = 1.0f;
        }
        else
#endif
        if (LeftTriggerInput != null && LeftTriggerInput.Length > 0) {
            switch (LeftTriggerInputType)
            {
#if ENABLE_INPUT_SYSTEM
                case InputType.InputSystemControl:
                    LeftTriggerValue = getInputControlValue(LeftTriggerInput);
                    break;
#endif
                case InputType.Axis:
                    LeftTriggerValue = Input.GetAxis(LeftTriggerInput);
                    break;
                case InputType.Button:
                    LeftTriggerValue = Input.GetButton(LeftTriggerInput) ? 1 : 0;
                    break;
                case InputType.Key:
                    LeftTriggerValue = Input.GetKey(LeftTriggerInput) ? 1 : 0;
                    break;
            }
        } else
        {
            LeftTriggerValue = 0.0f;
        }


#if ENABLE_INPUT_SYSTEM
        if (InputAction_RightTriggerPressed)
        {
            RightTriggerValue = 1.0f;
        }
        else
#endif
        if (RightTriggerInput != null && RightTriggerInput.Length > 0) {
            switch (RightTriggerInputType)
            {
#if ENABLE_INPUT_SYSTEM
                case InputType.InputSystemControl:
                    RightTriggerValue = getInputControlValue(RightTriggerInput);
                    break;
#endif
                case InputType.Axis:
                    RightTriggerValue = Input.GetAxis(RightTriggerInput);
                    break;
                case InputType.Button:
                    RightTriggerValue = Input.GetButton(RightTriggerInput) ? 1 : 0;
                    break;
                case InputType.Key:
                    RightTriggerValue = Input.GetKey(RightTriggerInput) ? 1 : 0;
                    break;
            }
        }
        else
        {
            RightTriggerValue = 0.0f;
        }

        if (gr != null)
        {
            this.UpdateGR();
        } else if (gc != null)
        {
            this.UpdateGC();
        }
    }


    /// <summary>
    /// Update function for one-handed gesture recognition
    /// </summary>
    void UpdateGR()
    {   
        if (LeftHandActive == false && RightHandActive == false)
        {
            if (LeftTriggerValue >= LeftTriggerThreshold)
            {
                LeftHandActive = true;
            }
            else if (RightTriggerValue >= RightTriggerThreshold)
            {
                RightHandActive = true;
            } else
            {
                return; // neither button pressed: nothing to do
            }
            Transform hmd = Camera.main.gameObject.transform;
            Vector3 p = hmd.position;
            Quaternion q = hmd.rotation;
            convertHeadInput(this.mivryCoordinateSystem, ref p, ref q);
            gr.startStroke(p, q);
        }

        GameObject activeGameObject = LeftHand;
        float activeTrigger = LeftTriggerValue;
        GestureCompletionData.Part.Side side = GestureCompletionData.Part.Side.Left;
        float activeTriggerThreshold = LeftTriggerThreshold;
        if (RightHandActive)
        {
            activeGameObject = RightHand;
            activeTrigger = RightTriggerValue;
            side = GestureCompletionData.Part.Side.Right;
            activeTriggerThreshold = RightTriggerThreshold;
        }
        if (activeTrigger >= activeTriggerThreshold * 0.9f)
        {
            // still gesturing
            Transform hmd = Camera.main.gameObject.transform;
            Vector3 hmd_p = hmd.position;
            Quaternion hmd_q = hmd.rotation;
            convertHeadInput(this.mivryCoordinateSystem, ref hmd_p, ref hmd_q);
            if (compensateHeadMotion)
            {
                gr.updateHeadPosition(hmd_p, hmd_q);
            }
            Vector3 p = activeGameObject.transform.position;
            Quaternion q = activeGameObject.transform.rotation;
            convertHandInput(this.unityXrPlugin, this.mivryCoordinateSystem, ref p, ref q);
            gr.contdStrokeQ(p, q);
            if (this.ContinuousGestureRecognition)
            {
                data.gestureID = gr.contdIdentify(hmd_p, hmd_q, ref data.similarity);
                data.gestureName = (data.gestureID >= 0)
                    ? gr.getGestureName(data.gestureID)
                    : GestureRecognition.getErrorMessage(data.gestureID);
                OnGestureCompletion.Invoke(data);
                #if MIVRY_USE_BOLT
                CustomEvent.Trigger(this.gameObject, data.gestureName, data);
                #endif
            }
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
        convertOutput(this.mivryCoordinateSystem, ref part.position, ref part.orientation);
        part.primaryDirection = part.orientation * Vector3.right;
        part.secondaryDirection = part.orientation * Vector3.up;
        data.gestureName = (data.gestureID >= 0)
            ? gr.getGestureName(data.gestureID)
            : GestureRecognition.getErrorMessage(data.gestureID);
        OnGestureCompletion.Invoke(data);
        #if MIVRY_USE_BOLT
        CustomEvent.Trigger(this.gameObject, data.gestureName, data);
        #endif
        LeftHandActive = false;
        RightHandActive = false;
    }

    /// <summary>
    /// Update function for two-handed gesture combinations.
    /// </summary>
    void UpdateGC()
    {
        if (LeftHandActive == false)
        {
            if (LeftTriggerValue >= LeftTriggerThreshold)
            {
                Transform hmd = Camera.main.gameObject.transform;
                Vector3 p = hmd.position;
                Quaternion q = hmd.rotation;
                convertHeadInput(this.mivryCoordinateSystem, ref p, ref q);
                gc.startStroke((int)GestureCompletionData.Part.Side.Left, p, q);
                LeftHandActive = true;
            }
        }
        if (RightHandActive == false)
        {
            if (RightTriggerValue >= RightTriggerThreshold)
            {
                Transform hmd = Camera.main.gameObject.transform;
                Vector3 p = hmd.position;
                Quaternion q = hmd.rotation;
                convertHeadInput(this.mivryCoordinateSystem, ref p, ref q);
                gc.startStroke((int)GestureCompletionData.Part.Side.Right, p, q);
                RightHandActive = true;
            }
        }
        if (LeftHandActive == true)
        {
            if (LeftTriggerValue >= LeftTriggerThreshold * 0.9f)
            {
                Transform hmd = Camera.main.gameObject.transform;
                Vector3 hmd_p = hmd.position;
                Quaternion hmd_q = hmd.rotation;
                convertHeadInput(this.mivryCoordinateSystem, ref hmd_p, ref hmd_q);
                if (compensateHeadMotion)
                {
                    gc.updateHeadPosition(hmd_p, hmd_q);
                }
                Vector3 p = LeftHand.transform.position;
                Quaternion q = LeftHand.transform.rotation;
                convertHandInput(this.unityXrPlugin, this.mivryCoordinateSystem, ref p, ref q);
                gc.contdStrokeQ((int)GestureCompletionData.Part.Side.Left, p, q);
                if (this.ContinuousGestureRecognition)
                {
                    data.gestureID = gc.contdIdentify(hmd_p, hmd_q, ref data.similarity);
                    data.gestureName = (data.gestureID >= 0)
                        ? gc.getGestureCombinationName(data.gestureID)
                        : GestureRecognition.getErrorMessage(data.gestureID);
                    OnGestureCompletion.Invoke(data);
                    #if MIVRY_USE_BOLT
                    CustomEvent.Trigger(this.gameObject, data.gestureName, data);
                    #endif
                }
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
                convertOutput(this.mivryCoordinateSystem, ref part.position, ref part.orientation);
                part.primaryDirection = part.orientation * Vector3.right;
                part.secondaryDirection = part.orientation * Vector3.up;
                LeftHandActive = false;
                if (RightHandActive == false)
                {
                    // both finished or the other hand never started
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
        if (RightHandActive == true)
        {
            if (RightTriggerValue >= RightTriggerThreshold * 0.9f)
            {
                Transform hmd = Camera.main.gameObject.transform;
                Vector3 hmd_p = hmd.position;
                Quaternion hmd_q = hmd.rotation;
                convertHeadInput(this.mivryCoordinateSystem, ref hmd_p, ref hmd_q);
                if (compensateHeadMotion)
                {
                    gc.updateHeadPosition(hmd_p, hmd_q);
                }
                Vector3 p = RightHand.transform.position;
                Quaternion q = RightHand.transform.rotation;
                convertHandInput(this.unityXrPlugin, this.mivryCoordinateSystem, ref p, ref q);
                gc.contdStrokeQ((int)GestureCompletionData.Part.Side.Right, p, q);
                if (this.ContinuousGestureRecognition)
                {
                    data.gestureID = gc.contdIdentify(hmd_p, hmd_q, ref data.similarity);
                    data.gestureName = (data.gestureID >= 0)
                        ? gc.getGestureCombinationName(data.gestureID)
                        : GestureRecognition.getErrorMessage(data.gestureID);
                    OnGestureCompletion.Invoke(data);
                    #if MIVRY_USE_BOLT
                    CustomEvent.Trigger(this.gameObject, data.gestureName, data);
                    #endif
                }
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
                convertOutput(this.mivryCoordinateSystem, ref part.position, ref part.orientation);
                part.primaryDirection = part.orientation * Vector3.right;
                part.secondaryDirection = part.orientation * Vector3.up;
                RightHandActive = false;
                if (LeftHandActive == false)
                {
                    // both finished or the other hand never started
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

    /// <summary>
    /// Convert position and orientation of a VR controller based on the Unity XR plugin used
    /// to MiVRy's internal coordinate system, if it should differ from the one being used by the XR plugin.
    /// </summary>
    /// <param name="unityXrPlugin">Which Unity XR plug-in is used by your project (see: Project Settings -> XR Plugin Manager).</param>
    /// <param name="mivryCoordinateSystem">The coordinate system that MiVRy should use internally.</param>
    /// <param name="p">The VR controller position.</param>
    /// <param name="q">The VR controller orientation.</param>
    public static void convertHandInput(UnityXrPlugin unityXrPlugin, MivryCoordinateSystem mivryCoordinateSystem, ref Vector3 p, ref Quaternion q)
    {
        switch (unityXrPlugin)
        {
            case UnityXrPlugin.OpenXR:
            case UnityXrPlugin.SteamVR:
                switch (mivryCoordinateSystem)
                {
                    case MivryCoordinateSystem.OculusVR:
                        q = q * new Quaternion(0.7071068f, 0, 0, 0.7071068f);
                        break;
                    case MivryCoordinateSystem.UnrealEngine:
                        q = new Quaternion(0.5f, 0.5f, 0.5f, 0.5f) * q * new Quaternion(0, 0, -0.7071068f, 0.7071068f);
                        p = new Vector3(p.z, p.x, p.y) * 100.0f;
                        break;
                }
                break;
            case UnityXrPlugin.OculusVR:
                switch (mivryCoordinateSystem)
                {
                    case MivryCoordinateSystem.OpenXR:
                    case MivryCoordinateSystem.SteamVR:
                        q = q * new Quaternion(-0.7071068f, 0, 0, 0.7071068f);
                        break;
                    case MivryCoordinateSystem.UnrealEngine:
                        q = new Quaternion(0.5f, 0.5f, 0.5f, 0.5f) * q * new Quaternion(-0.5f, -0.5f, -0.5f, 0.5f);
                        p = new Vector3(p.z, p.x, p.y) * 100.0f;
                        break;
                }
                break;
        }
    }


    /// <summary>
    /// Convert position and orientation of a VR headset to MiVRy's internal coordinate system,
    /// if it should differ from the one being used by the Unity XR plugin.
    /// </summary>
    /// <param name="mivryCoordinateSystem">The coordinate system that MiVRy should use internally.</param>
    /// <param name="p">The VR headset position.</param>
    /// <param name="q">The VR headset orientation.</param>
    public static void convertHeadInput(MivryCoordinateSystem mivryCoordinateSystem, ref Vector3 p, ref Quaternion q)
    {
        if (mivryCoordinateSystem == MivryCoordinateSystem.UnrealEngine)
        {
            p = new Vector3(p.z, p.x, p.y) * 100.0f;
            q = new Quaternion(0.5f, 0.5f, 0.5f, 0.5f) * q * new Quaternion(-0.5f, -0.5f, -0.5f, 0.5f);
        }
    }

    /// <summary>
    /// Convert position and orientation of MiVRy gesture output from MiVRy's internal coordinate system,
    /// if it should differ from the one being used by the Unity XR plugin.
    /// </summary>
    /// <param name="mivryCoordinateSystem">The coordinate system that MiVRy should use internally.</param>
    /// <param name="p">The gesture position.</param>
    /// <param name="q">The gesture orientation.</param>
    public static void convertOutput(MivryCoordinateSystem mivryCoordinateSystem, ref Vector3 p, ref Quaternion q)
    {        
        if (mivryCoordinateSystem == MivryCoordinateSystem.UnrealEngine)
        {
            p = new Vector3(p.y, p.z, p.x) * 0.01f;
            q = new Quaternion(-0.5f, -0.5f, -0.5f, 0.5f) * q;
        }
    }



    /// <summary>
    /// Convert back position and orientation of a VR controller from MiVRy's internal coordinate system
    /// to the one used by the Unity XR plugin in use (if it should differ).
    /// </summary>
    /// <param name="mivryCoordinateSystem">The coordinate system that MiVRy should use internally.</param>
    /// <param name="unityXrPlugin">Which Unity XR plug-in is used by your project (see: Project Settings -> XR Plugin Manager).</param>
    /// <param name="p">The VR controller position.</param>
    /// <param name="q">The VR controller orientation.</param>
    public static void convertBackHandInput(MivryCoordinateSystem mivryCoordinateSystem, UnityXrPlugin unityXrPlugin, ref Vector3 p, ref Quaternion q)
    {
        switch (unityXrPlugin)
        {
            case UnityXrPlugin.OpenXR:
            case UnityXrPlugin.SteamVR:
                switch (mivryCoordinateSystem)
                {
                    case MivryCoordinateSystem.OculusVR:
                        q = q * new Quaternion(-0.7071068f, 0, 0, 0.7071068f);
                        break;
                    case MivryCoordinateSystem.UnrealEngine:
                        q = new Quaternion(-0.5f, -0.5f, -0.5f, 0.5f) * q * new Quaternion(0, 0, 0.7071068f, 0.7071068f);
                        p = new Vector3(p.y, p.z, p.x) * 0.01f;
                        break;
                }
                break;
            case UnityXrPlugin.OculusVR:
                switch (mivryCoordinateSystem)
                {
                    case MivryCoordinateSystem.OpenXR:
                    case MivryCoordinateSystem.SteamVR:
                        q = q * new Quaternion(0.7071068f, 0, 0, 0.7071068f);
                        break;
                    case MivryCoordinateSystem.UnrealEngine:
                        q = new Quaternion(-0.5f, -0.5f, -0.5f, 0.5f) * q * new Quaternion(0.5f, 0.5f, 0.5f, 0.5f);
                        p = new Vector3(p.y, p.z, p.x) * 0.01f;
                        break;
                }
                break;
        }
    }


    /// <summary>
    /// Convert back position and orientation of a VR headset from MiVRy's internal coordinate system,
    /// to the one being used by the Unity XR plugin.
    /// </summary>
    /// <param name="mivryCoordinateSystem">The coordinate system that MiVRy should use internally.</param>
    /// <param name="p">The VR headset position.</param>
    /// <param name="q">The VR headset orientation.</param>
    public static void convertBackHeadInput(MivryCoordinateSystem mivryCoordinateSystem, ref Vector3 p, ref Quaternion q)
    {
        if (mivryCoordinateSystem == MivryCoordinateSystem.UnrealEngine)
        {
            p = new Vector3(p.y, p.z, p.x) * 0.01f;
            q = new Quaternion(-0.5f, -0.5f, -0.5f, 0.5f) * q * new Quaternion(0.5f, 0.5f, 0.5f, 0.5f);
        }
    }
}
