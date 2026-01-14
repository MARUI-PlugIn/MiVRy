/*
 * MiVRy - 3D gesture recognition library plug-in for Unity.
 * Version 2.15
 * Copyright (c) 2026 MARUI-PlugIn (inc.)
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

using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine.UI;
using UnityEngine.XR;
using InputDevice = UnityEngine.XR.InputDevice;
#if UNITY_ANDROID
using UnityEngine.Networking;
#endif
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using static MiVRy.GestureCompletionData.Part;
#endif

namespace MiVRy {
public class Sample_Military : MonoBehaviour
{
    // Name (ID) of your MiVRy license.
    // Leave emtpy for free version of MiVRy.
    [SerializeField] public string LicenseName;

    // License key of your MiVRy license.
    // Leave emtpy for free version of MiVRy.
    [SerializeField] public string LicenseKey;

    // Convenience ID's for the "left" and "right" sub-gestures.
    public const int Side_Left = 0;
    public const int Side_Right = 1;
    
    // Whether the user is currently pressing the contoller trigger.
    private bool trigger_pressed_left = false;
    private bool trigger_pressed_right = false;

    // Wether a gesture was already started
    private bool gesture_started = false;

    // The gesture recognition object for bimanual gestures.
    private GestureCombinations gc = new GestureCombinations(2);

    // The step in the tutorial where we currently are at
    private int current_tutorial_step = 0;

    // The text field to display instructions.
    private Text display_text;

    // The texture of the illustration canvas.
    private RawImage display_illustration;

    // Temporary storage for objects to display the gesture stroke.
    List<string> stroke = new List<string>(); 

    // Ball GameObject to illustrate the 3d motion path of the hand.
    private GameObject ball = null;

    // Temporary counter variable when creating objects for the stroke display:
    int stroke_index = 0;

#if UNITY_EDITOR
    private const string illustration_path = "Assets/Samples/Resources/Sample_Military_Illustration_";
#else
    private const string illustration_path = "Sample_Military_Illustration_";
#endif

    // List of the illustrations for the tutorial
    private string[] gestures = {
        "Assemble"
        ,
        "Disperse"
        ,
        "AirAttack"
        ,
        "TakeCover"
        ,
        "SpeedUp"
        ,
        "EnemySpotted"
        ,
        "Come"
    };

    // Database of all controller models in the scene
    private Dictionary<string, GameObject> controller_gameobjs = new Dictionary<string, GameObject>();

    // Helper function to set the currently active controller model
    void SetActiveControllerModel(string side, string type)
    {
        GameObject controller_oculus = controller_gameobjs["controller_oculus_" + side];
        GameObject controller_vive = controller_gameobjs["controller_vive_" + side];
        GameObject controller_microsoft = controller_gameobjs["controller_microsoft_" + side];
        GameObject controller_index = controller_gameobjs["controller_index_" + side];
        GameObject controller_dummy = controller_gameobjs["controller_dummy_" + side];
        controller_oculus.SetActive(false);
        controller_vive.SetActive(false);
        controller_microsoft.SetActive(false);
        controller_index.SetActive(false);
        controller_dummy.SetActive(false);
        if (type.Contains("Oculus")) // "Oculus Touch Controller OpenXR"
        {
            controller_oculus.SetActive(true);
        }
        else if (type.Contains("Windows MR")) // "Windows MR Controller OpenXR"
        {
            controller_microsoft.SetActive(true);
        }
        else if (type.Contains("Index")) // "Index Controller OpenXR"
        {
            controller_index.SetActive(true);
        }
        else if (type.ToLower().Contains("vive")) // "HTC Vive Controller OpenXR" or "OpenVR Controller(VIVE Controller Pro MV)"
        {
            controller_vive.SetActive(true);
        }
        else
        {
            controller_dummy.SetActive(true);
        }
    }

    // Helper function to handle new VR controllers being detected.
    void DeviceConnected(InputDevice device)
    {
        if ((device.characteristics & InputDeviceCharacteristics.Left) != 0)
        {
            SetActiveControllerModel("left", device.name);
        }
        else if ((device.characteristics & InputDeviceCharacteristics.Right) != 0)
        {
            SetActiveControllerModel("right", device.name);
        }
    }

    // Initialization:
    void Start ()
    {
        this.ball = GameObject.Find("ball");
        this.ball.transform.localScale = Vector3.zero;

        // Set the welcome message.
        this.display_text = GameObject.Find("DisplayText").GetComponent<Text>();
        this.display_text.text = "Welcome to 3D Gesture Recognition Plug-in!\n"
                               + "Press triggers of either or both controllers\n"
                               + "and perform the gesture displayed.";

        if (this.LicenseName != null && this.LicenseKey != null && this.LicenseName.Length > 0) {
            if (this.gc.activateLicense(this.LicenseName, this.LicenseKey) != 0) {
                Debug.LogError("Failed to activate license");
            }
        }

        // Load the default set of gestures.
#if UNITY_EDITOR
        // When running the scene inside the Unity editor,
        // we can just load the file from the Assets/ folder:
        string gesture_file_path = "Assets";
#elif UNITY_ANDROID
        // On android, the file is in the .apk,
        // so we need to first "download" it to the apps' cache folder.
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        string gesture_file_path = activity.Call <AndroidJavaObject>("getCacheDir").Call<string>("getCanonicalPath");
        UnityWebRequest request = UnityWebRequest.Get(Application.streamingAssetsPath + "/Samples/Sample_Military_Gestures.dat");
        request.SendWebRequest();
        while (!request.isDone) {
            // wait for file extraction to finish
        }
        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            display_text.text = "Failed to extract sample gesture database file from apk.\n";
            return;
        }
        string path = gesture_file_path + "/Samples/Sample_Military_Gestures.dat";
        try {
            Directory.CreateDirectory(path);
            Directory.Delete(path);
        } catch (Exception) { }
        try {
            File.WriteAllBytes(path, request.downloadHandler.data);
        } catch (Exception e) {
            display_text.text = "Exception writing temporary file: " + e.ToString();
            return;
        }
#else
        // This will be the case when exporting a stand-alone PC app.
        // In this case, we can load the gesture database file from the streamingAssets folder.
        string gesture_file_path = Application.streamingAssetsPath;
#endif
        gesture_file_path = gesture_file_path + "/Samples/Sample_Military_Gestures.dat";
        int ret = gc.loadFromFile(gesture_file_path);
        if (ret != 0) {
            byte[] file_contents = File.ReadAllBytes(gesture_file_path);
            if (file_contents == null || file_contents.Length == 0)
            {
                display_text.text = $"Could not find gesture database file ({gesture_file_path}).";
                return;
            }
            ret = gc.loadFromBuffer(file_contents);
            if (ret != 0)
            {
                display_text.text = $"Failed to load gesture database file ({ret}).";
                return;
            }
        }

        // Show the first gesture
        display_illustration = GameObject.Find("GestureIllustration").GetComponent<RawImage>();
#if UNITY_EDITOR
        display_illustration.texture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture>(illustration_path + gestures[0] + ".png");
#else
        display_illustration.texture = Resources.Load<Texture>(illustration_path + gestures[0]);
#endif

        controller_gameobjs["controller_oculus_left"] = GameObject.Find("controller_oculus_left");
        controller_gameobjs["controller_oculus_right"] = GameObject.Find("controller_oculus_right");
        controller_gameobjs["controller_vive_left"] = GameObject.Find("controller_vive_left");
        controller_gameobjs["controller_vive_right"] = GameObject.Find("controller_vive_right");
        controller_gameobjs["controller_microsoft_left"] = GameObject.Find("controller_microsoft_left");
        controller_gameobjs["controller_microsoft_right"] = GameObject.Find("controller_microsoft_right");
        controller_gameobjs["controller_index_left"] = GameObject.Find("controller_index_left");
        controller_gameobjs["controller_index_right"] = GameObject.Find("controller_index_right");
        controller_gameobjs["controller_dummy_left"] = GameObject.Find("controller_dummy_left");
        controller_gameobjs["controller_dummy_right"] = GameObject.Find("controller_dummy_right");

        controller_gameobjs["controller_oculus_left"].SetActive(false);
        controller_gameobjs["controller_oculus_right"].SetActive(false);
        controller_gameobjs["controller_vive_left"].SetActive(false);
        controller_gameobjs["controller_vive_right"].SetActive(false);
        controller_gameobjs["controller_microsoft_left"].SetActive(false);
        controller_gameobjs["controller_microsoft_right"].SetActive(false);
        controller_gameobjs["controller_index_left"].SetActive(false);
        controller_gameobjs["controller_index_right"].SetActive(false);
        controller_gameobjs["controller_dummy_left"].SetActive(false);
        controller_gameobjs["controller_dummy_right"].SetActive(false);

        // Initial assumption
        SetActiveControllerModel("left", "Oculus");
        SetActiveControllerModel("right", "Oculus");

        InputDevices.deviceConnected += DeviceConnected;
        List<InputDevice> devices = new List<InputDevice>();
        InputDevices.GetDevices(devices);
        foreach (var device in devices)
            DeviceConnected(device);
    }
    

    // Update:
    void Update()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current.escapeKey.wasPressedThisFrame) {
            Application.Quit();
        }
        float trigger_left = Sample_Utils.getInputControlValue("<XRController>{LeftHand}/trigger");
        float trigger_right = Sample_Utils.getInputControlValue("<XRController>{RightHand}/trigger");
#else
        float escape = Input.GetAxis("escape");
        if (escape > 0.0f) {
            Application.Quit();
        }
        float trigger_left = Input.GetAxis("LeftControllerTrigger");
        float trigger_right = Input.GetAxis("RightControllerTrigger");
#endif
        GameObject hmd = Camera.main.gameObject; // alternative: GameObject.Find("Main Camera");
        Vector3 hmd_p = hmd.transform.position;
        Quaternion hmd_q = hmd.transform.rotation;
        
        // If the user presses either controller's trigger, we start a new gesture.
        if (trigger_pressed_left == false && trigger_left > 0.9) { 
            // Controller trigger pressed.
            trigger_pressed_left = true;
            gc.startStroke(Side_Left, hmd_p, hmd_q);
            gesture_started = true;
        }
        if (trigger_pressed_right == false && trigger_right > 0.9) {
            // Controller trigger pressed.
            trigger_pressed_right = true;
            gc.startStroke(Side_Right, hmd_p, hmd_q);
            gesture_started = true;
        }
        if (gesture_started == false) {
            return;
        }

        // If we arrive here, the user is currently gesturing with one of the controllers.
        gc.updateHeadPosition(hmd_p, hmd_q);

        if (trigger_pressed_left == true) {
            if (trigger_left < 0.85) {
                // User let go of a trigger and held controller still
                gc.endStroke(Side_Left);
                trigger_pressed_left = false;
            } else {
                // User still gesturing or still moving after trigger pressed
                GameObject left_hand = GameObject.Find("Left Hand");
                gc.contdStrokeQ(Side_Left, left_hand.transform.position, left_hand.transform.rotation);
                // Show the stroke by instatiating new objects
                GameObject left_hand_pointer = GameObject.FindGameObjectWithTag("Left Pointer");
                addToStrokeTrail(left_hand_pointer.transform.position);
            }
        }

        if (trigger_pressed_right == true) {
            if (trigger_right < 0.85) {
                // User let go of a trigger and held controller still
                gc.endStroke(Side_Right);
                trigger_pressed_right = false;
            } else {
                // User still dragging or still moving after trigger pressed
                GameObject right_hand = GameObject.Find("Right Hand");
                gc.contdStrokeQ(Side_Right, right_hand.transform.position, right_hand.transform.rotation);
                // Show the stroke by instatiating new objects
                GameObject right_hand_pointer = GameObject.FindGameObjectWithTag("Right Pointer");
                addToStrokeTrail(right_hand_pointer.transform.position);
            }
        }

        if (trigger_pressed_left || trigger_pressed_right) {
            // User still dragging with either hand - nothing left to do
            return;
        }
        // else: if we arrive here, the user let go of both triggers, ending the gesture.
        gesture_started = false;

        // Delete the objectes that we used to display the gesture.
        foreach (string ball_instance_name in stroke) {
            GameObject ball_instance = GameObject.Find(ball_instance_name);
            if (ball_instance != null) {
                Destroy(ball_instance);
            }
        }
        stroke.Clear();
        stroke_index = 0;

        int gesture_combination_id = gc.identifyGestureCombination();
        if (gesture_combination_id < 0) {
            display_text.text = "Failed to identify gesture";
            return; // something went wrong
        }
        string gesture_combination_name = gc.getGestureCombinationName(gesture_combination_id);
        if (current_tutorial_step >=0) {
            // currently in tutorial mode
            if (String.Compare(gesture_combination_name, 0, gestures[current_tutorial_step], 0, gestures[current_tutorial_step].Length, true) != 0) {
                display_text.text = "INCORRECT!\nPlease try again."; // \nRequested gesture was: '"+ gestures[current_tutorial_step]+"'\nDetected gesture was: '"+ gesture_combination_name+"'\nPlease try again.";
                
            } else {
                current_tutorial_step++;
                if (current_tutorial_step < gestures.Length) {
                    display_text.text = "CORRECT!\nNext gesture:";
#if UNITY_EDITOR
                    display_illustration.texture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture>(illustration_path + gestures[current_tutorial_step] + ".png");
#else
                    display_illustration.texture = Resources.Load<Texture>(illustration_path + gestures[current_tutorial_step]);
#endif
                } else {
                    display_text.text = "CORRECT!\nTutorial finished.\nFeel free to try out any of the gestures now.";
                    current_tutorial_step = -1;
                    display_illustration.texture = null;
                }
            }
            return;
        }
        // else: if we arrive here: the tutorial was finished
        // Show the gesture illustration
        int gesture_index = 0;
        for (; gesture_index < gestures.Length; gesture_index++) {
            if (String.Compare(gesture_combination_name, 0, gestures[gesture_index], 0, gestures[gesture_index].Length, true) == 0) {
                break;
            }
        }
        if (gesture_index >= gestures.Length) {
            return;
        }
#if UNITY_EDITOR
        display_illustration.texture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture>(illustration_path + gestures[gesture_index] + ".png");
#else
        display_illustration.texture = Resources.Load<Texture>(illustration_path + gestures[gesture_index]);
#endif
        display_text.text = "Identified gesture:"; //  + gesture_combination_name;
    }


    // Helper function to add a new ball to the stroke trail.
    public void addToStrokeTrail(Vector3 p)
    {
        GameObject ball_instance = Instantiate(this.ball);
        ball_instance.name = ("stroke_" + stroke_index++);
        ball_instance.transform.position = p;
        ball_instance.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        stroke.Add(ball_instance.name);
    }
    
}
}
