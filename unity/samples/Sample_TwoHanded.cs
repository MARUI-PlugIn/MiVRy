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
using System.Runtime.InteropServices;
using AOT;
using UnityEngine.UI;
using UnityEngine.XR;
using InputDevice = UnityEngine.XR.InputDevice;
using System.IO;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif

namespace MiVRy {
public class Sample_TwoHanded : MonoBehaviour
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

    // The file from which to load gestures on startup (left hand).
    // For example: "Assets/Samples/Sample_TwoHanded_Gestures.dat"
    [SerializeField] private string LoadGesturesFile;

    // File where to save recorded gestures.
    // For example: "Assets/Samples/Sample_TwoHanded_MyGestures.dat"
    [SerializeField] private string SaveGesturesFile;
    
    // Tolerance for averaged controller motion to detect if the controller is still moving.
    [SerializeField] private double ControllerMotionDistanceThreshold;

    // Time the controller must be held still for averaged controller motion to detect if the controller is still moving.
    [SerializeField] private double ControllerMotionTimeThreshold;

    // Averaged controller motion (distance).
    private double controller_motion_distance_left = 0;
    private double controller_motion_distance_right = 0;

    // Timestamp when controller motion was last detected (if pressing the trigger button).
    private System.DateTime controller_motion_time_left = System.DateTime.Now;
    private System.DateTime controller_motion_time_right = System.DateTime.Now;

    // Short-term storage of controller positions to calculate motion.
    private Vector3 controller_motion_last_left = new Vector3(0, 0, 0);
    private Vector3 controller_motion_last_right = new Vector3(0, 0, 0);

    // Whether the user is currently pressing the contoller trigger.
    private bool trigger_pressed_left = false;
    private bool trigger_pressed_right = false;

    // Wether a gesture was already started
    private bool gesture_started = false;

    // The gesture recognition object for bimanual gestures.
    private GestureCombinations gc = new GestureCombinations(2);

    // The text field to display instructions.
    private Text HUDText;

    // The game object associated with the currently active controller (if any):
    private bool button_a_pressed = false;

    // ID of the gesture currently being recorded,
    // or: -1 if not currently recording a new gesture,
    // or: -2 if the AI is currently trying to learn to identify gestures
    // or: -3 if the AI has recently finished learning to identify gestures
    private int recording_gesture = -1; 

    // Last reported recognition performance (during training).
    // 0 = 0% correctly recognized, 1 = 100% correctly recognized.
    private double last_performance_report = 0; 

    // Temporary storage for objects to display the gesture stroke.
    List<string> stroke = new List<string>(); 

    // Temporary counter variable when creating objects for the stroke display:
    int stroke_index = 0;

    // Handle to this object/script instance, so that callbacks from the plug-in arrive at the correct instance.
    GCHandle me;

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
        // Set the welcome message.
        HUDText = GameObject.Find("HUDText").GetComponent<Text>();
        HUDText.text = "Welcome to 3D Gesture Recognition Plug-in!\n"
                      + "Press triggers of both controllers to draw a gesture,\n"
                      + "and hold the end position for " + ControllerMotionTimeThreshold + "s.\n"
                      + "Available gestures:\n"
                      + "1 - throw your hands up\n"
                      + "2 - pound your chest (like King Kong)\n"
                      + "3 - shoot an arrow (bow-and-arrow)\n"
                      + "4 - draw a heart shape (with both hands)\n"
                      + "or: press 'A'/'X'/Menu button\nto create new gesture.";

        me = GCHandle.Alloc(this);

        if (this.LicenseName != null && this.LicenseKey != null && this.LicenseName.Length > 0)
        {
            if (this.gc.activateLicense(this.LicenseName, this.LicenseKey) != 0)
            {
                Debug.LogError("Failed to activate license");
            }
        }

        // Global setting:
        // Ignore head tilt and roll rotation to approximate torso position.
        gc.ignoreHeadRotationUpDown = true;
        gc.ignoreHeadRotationTilt = true;

        // Load the default set of gestures.
#if UNITY_EDITOR
        string gesture_file_path = "Assets";
#else
        string gesture_file_path = Application.streamingAssetsPath;
#endif
        if (LoadGesturesFile == null)
        {
            LoadGesturesFile = "Samples/Sample_TwoHanded_Gestures.dat";
        }
        gesture_file_path = gesture_file_path + "/" + LoadGesturesFile;
        int ret = gc.loadFromFile(gesture_file_path);
        if (ret != 0)
        {
            byte[] file_contents = File.ReadAllBytes(gesture_file_path);
            if (file_contents == null || file_contents.Length == 0)
            {
                HUDText.text = $"Could not find gesture database file ({gesture_file_path}).";
                return;
            }
            ret = gc.loadFromBuffer(file_contents);
            if (ret != 0)
            {
                HUDText.text = $"Failed to load sample gesture database file ({ret}).";
                return;
            }
        }

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

        InputDevices.deviceConnected += DeviceConnected;
        List<InputDevice> devices = new List<InputDevice>();
        InputDevices.GetDevices(devices);
        foreach (var device in devices)
            DeviceConnected(device);

        if (ControllerMotionDistanceThreshold == 0)
        {
            ControllerMotionDistanceThreshold = 1;
        }
        if (ControllerMotionTimeThreshold == 0)
        {
            ControllerMotionTimeThreshold = 0.01;
        }
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
        if (escape > 0.0f)
        {
            Application.Quit();
        }
        float trigger_left = Input.GetAxis("LeftControllerTrigger");
        float trigger_right = Input.GetAxis("RightControllerTrigger");
#endif

        bool button_a_left = Input.GetButton("LeftControllerButtonA");
        bool button_a_right = Input.GetButton("RightControllerButtonA");
        if (button_a_pressed)
        {
            if (!button_a_left && !button_a_right)
            {
                button_a_pressed = false;
            }
            return;
        }

        // If recording_gesture is -3, that means that the AI has recently finished learning a new gesture.
        if (recording_gesture == -3) {
            // Save the data to file.
#if UNITY_EDITOR
            string gesture_file_path = "Assets";
#elif UNITY_ANDROID
            string gesture_file_path = Application.persistentDataPath;
#else
            string gesture_file_path = Application.streamingAssetsPath;
#endif
            if (this.SaveGesturesFile == null)
            {
                this.SaveGesturesFile = "Sample_TwoHanded_MyRecordedGestures.dat";
            }
            this.gc.saveToFile(gesture_file_path + "/" + this.SaveGesturesFile);
            // Show "finished" message.
            double performance = gc.recognitionScore();
            HUDText.text = "Training finished!\n(Final recognition performance = " + (performance * 100.0) + "%)\nFeel free to use your new gesture.";
            // Set recording_gesture to -1 to indicate normal operation (learning finished).
            recording_gesture = -1;
        }
        // If recording_gesture is -2, that means that the AI is currently learning a new gesture.
        if (recording_gesture == -2) {
            // Show "please wait" message
            HUDText.text = "...training...\n(Current recognition performance = " + (last_performance_report * 100.0) + "%)\nPress the 'A'/'X'/Menu button to cancel training.";
            // In this mode, the user may press the "B/Y/menu" button to cancel the learning process.
            if (button_a_left || button_a_right) {
                // Button pressed: stop the learning process.
                gc.stopTraining();
            }
            return;
		}
        // Else: if we arrive here, we're not in training/learning mode,
        // so the user can draw gestures.

        // If recording_gesture is -1, we're currently not recording a new gesture.
        if (recording_gesture == -1) {
            // In this mode, the user can press button A/X to create a new gesture
            if (button_a_left || button_a_right) {
                int recording_gesture_left = gc.createGesture(Side_Left, "custom gesture " + (gc.numberOfGestures(Side_Left) + 1));
                int recording_gesture_right = gc.createGesture(Side_Right, "custom gesture " + (gc.numberOfGestures(Side_Right) + 1));
                recording_gesture = gc.createGestureCombination("custom gesture " + (gc.numberOfGestureCombinations() + 1));
                gc.setCombinationPartGesture(recording_gesture, Side_Left, recording_gesture_left);
                gc.setCombinationPartGesture(recording_gesture, Side_Right, recording_gesture_right);
                // from now on: recording a new gesture
                HUDText.text = "Learning a new gesture (custom gesture " + (recording_gesture-3) + "):\nPlease perform the gesture 25 times.\n(0 / 25)";
            }
        }

        GameObject hmd = Camera.main.gameObject; // alternative: GameObject.Find("Main Camera");
        Vector3 hmd_p = hmd.transform.position;
        Quaternion hmd_q = hmd.transform.rotation;
        
        // If the user presses either controller's trigger, we start a new gesture.
        if (trigger_pressed_left == false && trigger_left > 0.9) { 
            // Controller trigger pressed.
            trigger_pressed_left = true;
            gc.startStroke(Side_Left, hmd_p, hmd_q, recording_gesture);
            gesture_started = true;
        }
        if (trigger_pressed_right == false && trigger_right > 0.9)
        {
            // Controller trigger pressed.
            trigger_pressed_right = true;
            gc.startStroke(Side_Right, hmd_p, hmd_q, recording_gesture);
            gesture_started = true;
        }
        if (gesture_started == false) {
            // nothing to do.
            return;
        }

        // If we arrive here, the user is currently dragging with one of the controllers.
        gc.updateHeadPosition(hmd_p, hmd_q);

        if (trigger_pressed_left == true)
        {
            if (trigger_left < 0.85 && controller_motion_distance_left < ControllerMotionDistanceThreshold && System.DateTime.Now.Subtract(controller_motion_time_left).Seconds > ControllerMotionTimeThreshold)
            {
                // User let go of a trigger and held controller still
                gc.endStroke(Side_Left);
                trigger_pressed_left = false;
            } else
            {
                // User still dragging or still moving after trigger pressed
                GameObject left_hand = GameObject.Find("Left Hand");
                gc.contdStrokeQ(Side_Left, left_hand.transform.position, left_hand.transform.rotation);
                // Show the stroke by instatiating new objects
                GameObject left_hand_pointer = GameObject.FindGameObjectWithTag("Left Pointer");
                addToStrokeTrail(left_hand_pointer.transform.position);
                float contoller_motion = (left_hand.transform.position - controller_motion_last_left).magnitude;
                controller_motion_last_left = left_hand.transform.position;
                controller_motion_distance_left = (controller_motion_distance_left + contoller_motion) * 0.5f; // averaging
                if (controller_motion_distance_left > ControllerMotionDistanceThreshold)
                {
                    controller_motion_time_left = System.DateTime.Now;
                }
            }
        }

        if (trigger_pressed_right == true)
        {
            if (trigger_right < 0.85 && controller_motion_distance_right < ControllerMotionDistanceThreshold && System.DateTime.Now.Subtract(controller_motion_time_right).Seconds > ControllerMotionTimeThreshold)
            {
                // User let go of a trigger and held controller still
                gc.endStroke(Side_Right);
                trigger_pressed_right = false;
            }
            else
            {
                // User still dragging or still moving after trigger pressed
                GameObject right_hand = GameObject.Find("Right Hand");
                gc.contdStrokeQ(Side_Right, right_hand.transform.position, right_hand.transform.rotation);
                // Show the stroke by instatiating new objects
                GameObject right_hand_pointer = GameObject.FindGameObjectWithTag("Right Pointer");
                addToStrokeTrail(right_hand_pointer.transform.position);
                float contoller_motion = (right_hand.transform.position - controller_motion_last_right).magnitude;
                controller_motion_last_right = right_hand.transform.position;
                controller_motion_distance_right = (controller_motion_distance_right + contoller_motion) * 0.5f; // averaging
                if (controller_motion_distance_right > ControllerMotionDistanceThreshold)
                {
                    controller_motion_time_right = System.DateTime.Now;
                }
            }
        }

        if (trigger_pressed_left || trigger_pressed_right)
        {
            // User still dragging with either hand - nothing left to do
            return;
        }
        // else: if we arrive here, the user let go of both triggers, ending the gesture.
        gesture_started = false;

        // Delete the objectes that we used to display the gesture.
        foreach (string star in stroke) {
            Destroy(GameObject.Find(star));
            stroke_index = 0;
        }

        double similarity = 0;
        int multigesture_id = gc.identifyGestureCombination(ref similarity);
        
        // If we are currently recording samples for a custom gesture, check if we have recorded enough samples yet.
        if (recording_gesture >= 0) {
            // Currently recording samples for a custom gesture - check how many we have recorded so far.
            int num_samples = gc.getGestureNumberOfSamples(Side_Left, recording_gesture);
            if (num_samples < 25) {
                // Not enough samples recorded yet.
                HUDText.text = "Learning a new gesture (custom gesture " + (recording_gesture - 3) + "):\nPlease perform the gesture 25 times.\n(" + num_samples + " / 25)";
            } else {
                // Enough samples recorded. Start the learning process.
                HUDText.text = "Learning gestures - please wait...\n(press B button to stop the learning process)";
                // Set up the call-backs to receive information about the learning process.
                gc.setTrainingUpdateCallback(trainingUpdateCallback);
                gc.setTrainingUpdateCallbackMetadata((IntPtr)me);
                gc.setTrainingFinishCallback(trainingFinishCallback);
                gc.setTrainingFinishCallbackMetadata((IntPtr)me);
                gc.setMaxTrainingTime(30);
                // Set recording_gesture to -2 to indicate that we're currently in learning mode.
                recording_gesture = -2;
                int ret = gc.startTraining();
                if (ret != 0) {
                    Debug.Log("COULD NOT START TRAINING: " + GestureRecognition.getErrorMessage(ret));
                }
            }
            return;
        }
        // else: if we arrive here, we're not recording new samples for custom gestures,
        // but instead have identified a new gesture.
        // Perform the action associated with that gesture.
        if (multigesture_id < 0)
        {
            // Error trying to identify any gesture
            HUDText.text = "Failed to identify gesture.";
        }
        else if (multigesture_id == 0)
        {
            HUDText.text = "Identified a \"throw-your-hands-up\" gesture!";
        }
        else if (multigesture_id == 1)
        {
            HUDText.text = "Identified a chest-pounding gesture!";
        }
        else if (multigesture_id == 2)
        {
            HUDText.text = "Identified a bow-and-arrow gesture!";
        }
        else if (multigesture_id == 3)
        {
            HUDText.text = "Identified a heart-shape gesture!";
        }
        else
        {
            // Other ID: one of the user-registered gestures:
            HUDText.text = " identified custom registered gesture " + (multigesture_id - 3);
        }
    }


    // Helper function to add a new star to the stroke trail.
    public void addToStrokeTrail(Vector3 p)
    {
        GameObject star_instance = Instantiate(GameObject.Find("star"));
        GameObject star = new GameObject("stroke_" + stroke_index++);
        star_instance.name = star.name + "_instance";
        star_instance.transform.SetParent(star.transform, false);
        System.Random random = new System.Random();
        star.transform.position = new Vector3((float)random.NextDouble() / 80, (float)random.NextDouble() / 80, (float)random.NextDouble() / 80) + p;
        star.transform.rotation = new Quaternion((float)random.NextDouble() - 0.5f, (float)random.NextDouble() - 0.5f, (float)random.NextDouble() - 0.5f, (float)random.NextDouble() - 0.5f).normalized;
        //star.transform.rotation.Normalize();
        float star_scale = (float)random.NextDouble() + 0.3f;
        star.transform.localScale = new Vector3(star_scale, star_scale, star_scale);
        stroke.Add(star.name);
    }

    // Callback function to be called by the left-hand gesture recognition plug-in during the learning process.
    [MonoPInvokeCallback(typeof(GestureRecognition.TrainingCallbackFunction))]
    public static void trainingUpdateCallback(double performance, IntPtr ptr)
    {
        // Get the script/scene object back from metadata.
        GCHandle obj = (GCHandle)ptr;
        Sample_TwoHanded me = (obj.Target as Sample_TwoHanded);
        // Update the performance indicator with the latest estimate.
        me.last_performance_report = performance;
    }
    // Callback function to be called by the gesture recognition plug-in when the learning process was finished.
    [MonoPInvokeCallback(typeof(GestureRecognition.TrainingCallbackFunction))]
    public static void trainingFinishCallback(double performance, IntPtr ptr)
    {
        // Get the script/scene object back from metadata.
        GCHandle obj = (GCHandle)ptr;
        Sample_TwoHanded me = (obj.Target as Sample_TwoHanded);
        // Update the performance indicator with the latest estimate.
        me.last_performance_report = performance;
        // Signal that training was finished.
        me.recording_gesture = -3;
    }
}
}
