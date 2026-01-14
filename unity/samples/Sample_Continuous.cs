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
using AOT;
using UnityEngine.UI;
using UnityEngine.XR;
using InputDevice = UnityEngine.XR.InputDevice;
#if UNITY_ANDROID
using UnityEngine.Networking;
#endif
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif

namespace MiVRy {
public class Sample_Continuous : MonoBehaviour
{
    // The interval in seconds at which to perform the
    // identification (or recording) of the gesture.
    [SerializeField] private float RecognitionInterval;

    // The approximate duration in seconds of the gestures to be
    // detected.
    [SerializeField] private float GesturePeriod;

    // The file from which to load gestures on startup.
    // For example: "Assets/Samples/sample_gestures.dat"
    [SerializeField] private string LoadGesturesFile;

    // File where to save recorded gestures.
    // For example: "Assets/Samples/my_custom_gestures.dat"
    [SerializeField] private string SaveGesturesFile;

    // Name (ID) of your MiVRy license.
    // Leave emtpy for free version of MiVRy.
    [SerializeField] public string LicenseName;

    // License key of your MiVRy license.
    // Leave emtpy for free version of MiVRy.
    [SerializeField] public string LicenseKey;

    // The gesture recognition object:
    // You can have as many of these as you want simultaneously.
    private GestureRecognition gr = new GestureRecognition();

    // The text field to display instructions.
    private Text HUDText;

    // The game object associated with the currently active controller (if any):
    private GameObject active_controller = null;

    // The pointing tip of the active_controller (used for visualization).
    private GameObject active_controller_pointer = null;

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

    // Last timestamp when a gesture recognition was performed.
    private float last_recognition_time = 0.0f;

    // Timestamp when a gesture stroke was started.
    private float stroke_start_time = 0.0f;

    public class StrokePoint
    {
        public float time; // Time when the star object was created.
        public string name; // Name of the created star object.
        public static int stroke_index = 0;
        public StrokePoint(Vector3 p)
        {
            GameObject star_instance = Instantiate(GameObject.Find("star"));
            GameObject star = new GameObject("stroke_" + stroke_index++);
            star_instance.name = star.name + "_instance";
            star_instance.transform.SetParent(star.transform, false);
            System.Random random = new System.Random();
            star.transform.position = new Vector3(p.x + (float)random.NextDouble() / 80, p.y + (float)random.NextDouble() / 80, p.z + (float)random.NextDouble() / 80);
            star.transform.rotation = new Quaternion((float)random.NextDouble() - 0.5f, (float)random.NextDouble() - 0.5f, (float)random.NextDouble() - 0.5f, (float)random.NextDouble() - 0.5f).normalized;
            float star_scale = (float)random.NextDouble() + 0.3f;
            star.transform.localScale = new Vector3(star_scale, star_scale, star_scale);
            this.time = Time.time;
            this.name = star.name;
        }
    };

    // Temporary storage for objects to display the gesture stroke.
    List<StrokePoint> stroke = new List<StrokePoint>(); 

    // List of Objects created with gestures:
    List<GameObject> created_objects = new List<GameObject>();

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
        me = GCHandle.Alloc(this);
        
        if (RecognitionInterval == 0)
        {
            RecognitionInterval = 0.1f;
        }
        if (GesturePeriod == 0)
        {
            GesturePeriod = 1.0f;
        }

        if (this.LicenseName != null && this.LicenseKey != null && this.LicenseName.Length > 0)
        {
            if (this.gr.activateLicense(this.LicenseName, this.LicenseKey) != 0)
            {
                Debug.LogError("Failed to activate license");
            }
        }

        // Load the set of gestures.
        if (LoadGesturesFile == null)
        {
            LoadGesturesFile = "Samples/Sample_Continuous_Gestures.dat";
        }

        // Find the location for the gesture database (.dat) file
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
        UnityWebRequest request = UnityWebRequest.Get(Application.streamingAssetsPath + "/" + LoadGesturesFile);
        request.SendWebRequest();
        while (!request.isDone) {
            // wait for file extraction to finish
        }
        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            HUDText.text = "Failed to extract sample gesture database file from apk.";
            return;
        }
        string path = gesture_file_path + "/" + LoadGesturesFile;
        try {
            Directory.CreateDirectory(path);
            Directory.Delete(path);
        } catch (Exception) { }
        try {
            File.WriteAllBytes(path, request.downloadHandler.data);
        } catch (Exception e) {
            HUDText.text = "Exception writing temporary file: " + e.ToString();
            return;
        }
#else
        // This will be the case when exporting a stand-alone PC app.
        // In this case, we can load the gesture database file from the streamingAssets folder.
        string gesture_file_path = Application.streamingAssetsPath;
#endif
        gesture_file_path = gesture_file_path + "/" + LoadGesturesFile;
        int ret = gr.loadFromFile(gesture_file_path);
        if (ret != 0)
        {
            byte[] file_contents = File.ReadAllBytes(gesture_file_path);
            if (file_contents == null || file_contents.Length == 0)
            {
                HUDText.text = $"Could not find gesture database file ({gesture_file_path}).";
                return;
            }
            ret = gr.loadFromBuffer(file_contents);
            if (ret != 0)
            {
                HUDText.text = $"Failed to load sample gesture database file ({ret}).";
                return;
            }
        }
        gr.contdIdentificationSmoothing = 5;

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

        GameObject star = GameObject.Find("star");
        star.transform.localScale = new Vector3(0.0f, 0.0f, 0.0f);
        GameObject controller_dummy = GameObject.Find("controller_dummy");
        controller_dummy.transform.localScale = new Vector3(0.0f, 0.0f, 0.0f);


        // Set the welcome message.
        HUDText = GameObject.Find("HUDText").GetComponent<Text>();
        HUDText.text = "Welcome to MARUI Gesture Plug-in!\n"
                      + "Hold the trigger to draw gestures.\nAvailable gestures:";
        for (int i=0; i<gr.numberOfGestures(); i++)
        {
            HUDText.text += "\n" + (i + 1) + " : " + gr.getGestureName(i);
        }
        HUDText.text += "\nor: press 'A'/'X'/Menu button\nto create new gesture.";
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
            string GesturesFilePath = "Assets";
#elif UNITY_ANDROID
        string GesturesFilePath = Application.persistentDataPath;
#else
        string GesturesFilePath = Application.streamingAssetsPath;
#endif
            if (this.SaveGesturesFile == null)
            {
                this.SaveGesturesFile = "Sample_Continuous_MyRecordedGestures.dat";
            }
            this.gr.saveToFile(GesturesFilePath + "/" + this.SaveGesturesFile);
            // Show "finished" message.
            double performance = gr.recognitionScore();
            HUDText.text = "Training finished!\n(Final recognition performance = " + (performance * 100.0) + "%)\nFeel free to use your new gesture.";
            // Set recording_gesture to -1 to indicate normal operation (learning finished).
            recording_gesture = -1;
            return;
        }
        // If recording_gesture is -2, that means that the AI is currently learning a new gesture.
        if (recording_gesture == -2) {
            // Show "please wait" message
            HUDText.text = "...training...\n(Current recognition performance = " + (last_performance_report * 100.0) + "%)\nPress the 'A'/'X'/Menu button to stop training.";
            // In this mode, the user may press the "A/X/menu" button to cancel the learning process.
            if (button_a_left || button_a_right) {
                // Button pressed: stop the learning process.
                gr.stopTraining();
                button_a_pressed = true;
            }
            return;
		}
        // Else: if we arrive here, we're not in training/learning mode,
        // so the user can draw gestures.

        if (button_a_left || button_a_right)
        {
            button_a_pressed = true;
            // If recording_gesture is -1, we're currently not recording a new gesture.
            if (recording_gesture == -1) {
                // In this mode, the user can press button A/X/menu to create a new gesture
                recording_gesture = gr.createGesture("Your gesture #" + (gr.numberOfGestures() - 4));
                // from now on: recording a new gesture
                HUDText.text = "Learning a new gesture (" + (gr.getGestureName(recording_gesture)) + "):\nPlease perform the gesture for a while.\n(0 samples recorded)";
                gr.contdIdentificationPeriod = (int)(this.GesturePeriod * 1000.0f); // to milliseconds
            } else
            {
                HUDText.text = "Learning gestures - please wait...\n(press A/X/menu button to stop the learning process)";
                // Set up the call-backs to receive information about the learning process.
                gr.setTrainingUpdateCallback(trainingUpdateCallback);
                gr.setTrainingUpdateCallbackMetadata((IntPtr)me);
                gr.setTrainingFinishCallback(trainingFinishCallback);
                gr.setTrainingFinishCallbackMetadata((IntPtr)me);
                gr.setMaxTrainingTime(30);
                // Set recording_gesture to -2 to indicate that we're currently in learning mode.
                recording_gesture = -2;
                int ret = gr.startTraining();
                if (ret != 0)
                {
                    Debug.Log("COULD NOT START TRAINING: " + GestureRecognition.getErrorMessage(ret));
                }
            }
            return;
        }
        GameObject hmd = Camera.main.gameObject; // alternative: GameObject.Find("Main Camera");
        Vector3 hmd_p = hmd.transform.position;
        Quaternion hmd_q = hmd.transform.rotation;

        // If the user is not yet dragging (pressing the trigger) on either controller, he hasn't started a gesture yet.
        if (active_controller == null) {
            // If the user presses either controller's trigger, we start a new gesture.
            if (trigger_right > 0.9) {
                // Right controller trigger pressed.
                active_controller = GameObject.Find("Right Hand");
                active_controller_pointer = GameObject.FindGameObjectWithTag("Right Pointer");
            } else if (trigger_left > 0.9) {
                // Left controller trigger pressed.
                active_controller = GameObject.Find("Left Hand");
                active_controller_pointer = GameObject.FindGameObjectWithTag("Left Pointer");
            } else {
                // If we arrive here, the user is pressing neither controller's trigger:
                // nothing to do.
                return;
            }
            // If we arrive here: either trigger was pressed, so we start the gesture.
            gr.contdIdentificationPeriod = (int)(this.GesturePeriod * 1000.0f); // to milliseconds
            gr.startStroke(hmd_p, hmd_q, recording_gesture);
            this.stroke_start_time = Time.time;
        }

        double similarity = -1.0; // This will receive a value of how similar the performed gesture was to previous recordings.
        int gesture_id = -1; // This will receive the ID of the gesture.

        // If we arrive here, the user is currently dragging with one of the controllers.
        // Check if the user is still dragging or if he let go of the trigger button.
        if (trigger_left > 0.85 || trigger_right > 0.85)
        {
            gr.updateHeadPosition(hmd_p, hmd_q);
            Vector3 p = active_controller.transform.position;
            Quaternion q = active_controller.transform.rotation;
            gr.contdStrokeQ(p, q);
            // The user is still dragging with the controller: continue the gesture.
            if (Time.time - this.stroke_start_time > GesturePeriod && Time.time - this.last_recognition_time > RecognitionInterval)
            {
                if (recording_gesture >= 0)
                {
                    gr.contdRecord(hmd_p, hmd_q);
                    int num_samples = gr.getGestureNumberOfSamples(recording_gesture);
                    HUDText.text = "Learning a new gesture (" + (gr.getGestureName(recording_gesture)) + ").\n\n(" + num_samples + " samples recorded)";
                }
                else
                {
                    gesture_id = gr.contdIdentify(hmd_p, hmd_q, ref similarity);
                    if (gesture_id >= 0)
                    {
                        string gesture_name = gr.getGestureName(gesture_id);
                        HUDText.text = "Identifying gesture '" + gesture_name + "'.\n\n(similarity: " + similarity + "%)";
                    } else
                    {
                        string error_string = GestureRecognition.getErrorMessage(gesture_id);
                        HUDText.text = "Error identifying gesture: " + error_string;
                    }
                }
                this.last_recognition_time = Time.time;
            }
            // Prune the stroke of all items that are too old to be used
            float cutoff_time = Time.time - GesturePeriod;
            while (stroke.Count > 0 && stroke[0].time < cutoff_time)
            {
                GameObject star_object = GameObject.Find(stroke[0].name);
                if (star_object != null)
                {
                    Destroy(star_object);
                }
                stroke.RemoveAt(0);
            }
            // Extend the stroke by instatiating new objects
            stroke.Add(new StrokePoint(active_controller_pointer.transform.position));
            return;
        }
        // else: if we arrive here, the user let go of the trigger, ending a gesture.
        active_controller = null;
        gr.cancelStroke();
        // Delete the objects that we used to display the gesture.
        foreach (StrokePoint star in stroke)
        {
            GameObject star_object = GameObject.Find(star.name);
            if (star_object != null)
            {
                Destroy(star_object);
            }
        }
        stroke.Clear();

        // If we are currently recording samples for a custom gesture, check if we have recorded enough samples yet.
        if (recording_gesture >= 0) {
            // Currently recording samples for a custom gesture - check how many we have recorded so far.
            int num_samples = gr.getGestureNumberOfSamples(recording_gesture);
            HUDText.text = "Learning a new gesture (" + (gr.getGestureName(recording_gesture)) + "):\n"
                         + "Please perform the gesture for a while.\n(" + num_samples + " samples recorded)\n"
                         + "\nor: press 'A'/'X'/Menu button\nto finish recording and start training.";
            return;
        }
        // else: if we arrive here, we're not recording new sampled for custom gestures,
        // but instead have identified a new gesture.
        // Perform the action associated with that gesture.


        HUDText = GameObject.Find("HUDText").GetComponent<Text>();
        HUDText.text = "Hold the trigger to draw gestures.\nAvailable gestures:";
        for (int i = 0; i < gr.numberOfGestures(); i++)
        {
            HUDText.text += "\n" + (i + 1) + " : " + gr.getGestureName(i);
        }
        HUDText.text += "\nor: press 'A'/'X'/Menu button\nto create new gesture.";
    }

    // Callback function to be called by the gesture recognition plug-in during the learning process.
    [MonoPInvokeCallback(typeof(GestureRecognition.TrainingCallbackFunction))]
    public static void trainingUpdateCallback(double performance, IntPtr ptr)
    {
        // Get the script/scene object back from metadata.
        GCHandle obj = (GCHandle)ptr;
        Sample_Continuous me = (obj.Target as Sample_Continuous);
        // Update the performance indicator with the latest estimate.
        me.last_performance_report = performance;
    }
    

    // Callback function to be called by the gesture recognition plug-in when the learning process was finished.
    [MonoPInvokeCallback(typeof(GestureRecognition.TrainingCallbackFunction))]
    public static void trainingFinishCallback(double performance, IntPtr ptr)
    {
        // Get the script/scene object back from metadata.
        GCHandle obj = (GCHandle)ptr;
        Sample_Continuous me = (obj.Target as Sample_Continuous);
        // Update the performance indicator with the latest estimate.
        me.last_performance_report = performance;
        // Signal that training was finished.
        me.recording_gesture = -3;
    }
}
}
