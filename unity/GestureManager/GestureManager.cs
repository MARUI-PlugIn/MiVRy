/*
 * Advaced Gesture Recognition - Unity Plug-In
 * 
 * Copyright (c) 2019 MARUI-PlugIn (inc.)
 * This software is free to use for non-commercial purposes.
 * You may use this software in part or in full for any project
 * that does not pursue financial gain, including free software 
 * and projectes completed for evaluation or educational purposes only.
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
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.IO;
using AOT;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.Networking;

public class GestureManager : MonoBehaviour
{
    // Fields to be controlled by the editor:
    public int numberOfParts {
        get
        {
            if (gr != null)
                return 1;
            if (gc != null)
                return gc.numberOfParts();
            return 0;
        }
        set {
            if (value <= 0)
            {
                gr = null;
                gc = null;
                if (ConsoleText != null)
                    ConsoleText.text = "Deleted Gesture Recognition Object";
            } else if (value == 1) {
                gc = null;
                if (gr == null)
                {
                    gr = new GestureRecognition();
                    gr.setTrainingUpdateCallback(GestureManager.trainingUpdateCallback);
                    gr.setTrainingUpdateCallbackMetadata((IntPtr)me);
                    gr.setTrainingFinishCallback(GestureManager.trainingFinishCallback);
                    gr.setTrainingFinishCallbackMetadata((IntPtr)me);
                    if (ConsoleText != null)
                        ConsoleText.text = "Created Gesture Recognition Object\n(for one-handed gestures)";
                }
            } else
            {
                gr = null;
                if (gc == null || gc.numberOfParts() != value) {
                    gc = new GestureCombinations(value);
                    gc.setTrainingUpdateCallback(GestureManager.trainingUpdateCallback);
                    gc.setTrainingUpdateCallbackMetadata((IntPtr)me);
                    gc.setTrainingFinishCallback(GestureManager.trainingFinishCallback);
                    gc.setTrainingFinishCallbackMetadata((IntPtr)me);
                    if (ConsoleText != null)
                        ConsoleText.text = $"Created Gesture Combination Object\n(for {value} parts)";
                }
            }
            GestureManagerVR.refresh();
        }
    }
    public string file_load_combinations = "Samples/Sample_TwoHanded_Gestures.dat";
    public string file_import_combinations = "Samples/Sample_Military_Gestures.dat";
    public string file_load_subgestures = "Samples/Sample_OneHanded_Gestures.dat";
    public int file_load_subgestures_i = 0;
    public string file_load_gestures = "Samples/Sample_OneHanded_Gestures.dat";
    public string file_import_gestures = "Samples/Sample_Pixie_Gestures.dat";
    public string file_save_combinations = "Sample_TwoHanded_MyGestures.dat";
    // public string file_save_subgestures = "Sample_TwoHanded_MyGesturesLeft.dat";
    // public int file_save_subgestures_i = 0;
    public string file_save_gestures = "Sample_OneHanded_MyGestures.dat";

    public string create_combination_name = "(new gesture combination name)";
    public string create_gesture_name = "(new gesture name)";
    public string[] create_gesture_names = null;

    public int record_gesture_id = -1;
    public int record_combination_id = -1;
    public int lefthand_combination_part = 0; // combination part performed with the left hand
    public int righthand_combination_part = 1; // combination part performed with the left hand

    public int copy_gesture_from_part = 0; // combination part from where to copy a gesture
    public int copy_gesture_to_part = 1; // combination part to where to copy a gesture
    public int copy_gesture_from_id = 0; // which gesture to copy
    public int copy_gesture_to_id = 0; // which gesture to copy (+1 so that "0" means "new gesture (-1))
    public bool copy_gesture_mirror = true; // whether to mirror the gesture to copy
    public bool copy_gesture_rotate = true; // whether to rotate the gesture to copy

    // The gesture recognition object:
    // You can have as many of these as you want simultaneously.
    public GestureRecognition gr = null;
    public GestureCombinations gc = null;

    // The text field to display instructions.
    private Text ConsoleText;

    // The game object associated with the currently active controller (if any):
    private GameObject active_controller = null;

    // Whether the training process is was recently started.
    public bool training_started = false;

    // Last reported recognition performance (during training).
    // 0 = 0% correctly recognized, 1 = 100% correctly recognized.
    public double last_performance_report = 0; 

    // Temporary storage for objects to display the gesture stroke.
    List<string> stroke = new List<string>(); 

    // Temporary counter variable when creating objects for the stroke display:
    public int stroke_index = 0; 
    
    // Handle to this object/script instance, so that callbacks from the plug-in arrive at the correct instance.
    public GCHandle me;

    // Whether the user is currently pressing the contoller trigger.
    private bool trigger_pressed_left = false;
    private bool trigger_pressed_right = false;

    // Wether a gesture was already started
    public bool gesture_started = false;

    public GestureManager() : base()
    {
        me = GCHandle.Alloc(this);
    }

    // Initialization:
    void Start ()
    {
        // Set the welcome message.
        ConsoleText = GameObject.Find("ConsoleText").GetComponent<Text>();
        consoleText = "Welcome to MiVRy Gesture Recognition!\n"
                      + "This manager allows you to create and record gestures,\n"
                      + "and organize gesture files.";

        me = GCHandle.Alloc(this);
        
        GameObject controller_oculus_left = GameObject.Find("controller_oculus_left");
        GameObject controller_oculus_right = GameObject.Find("controller_oculus_right");
        GameObject controller_vive_left = GameObject.Find("controller_vive_left");
        GameObject controller_vive_right = GameObject.Find("controller_vive_right");
        GameObject controller_microsoft_left = GameObject.Find("controller_microsoft_left");
        GameObject controller_microsoft_right = GameObject.Find("controller_microsoft_right");
        GameObject controller_dummy_left = GameObject.Find("controller_dummy_left");
        GameObject controller_dummy_right = GameObject.Find("controller_dummy_right");

        controller_oculus_left.SetActive(false);
        controller_oculus_right.SetActive(false);
        controller_vive_left.SetActive(false);
        controller_vive_right.SetActive(false);
        controller_microsoft_left.SetActive(false);
        controller_microsoft_right.SetActive(false);
        controller_dummy_left.SetActive(false);
        controller_dummy_right.SetActive(false);

        var input_devices = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevices(input_devices);
        String input_device = "";
        foreach (var device in input_devices)
        {
            if (device.characteristics.HasFlag(InputDeviceCharacteristics.HeadMounted))
            {
                input_device = device.name;
                break;
            }
        }

        if (input_device.Length >= 6 && input_device.Substring(0, 6) == "Oculus")
        {
            controller_oculus_left.SetActive(true);
            controller_oculus_right.SetActive(true);
        } else if (input_device.Length >= 4 && input_device.Substring(0, 4) == "Vive")
        {
            controller_vive_left.SetActive(true);
            controller_vive_right.SetActive(true);
        }
        else if (input_device.Length >= 4 && input_device.Substring(0, 4) == "DELL")
        {
            controller_microsoft_left.SetActive(true);
            controller_microsoft_right.SetActive(true);
        }
        else // 
        {
            controller_dummy_left.SetActive(true);
            controller_dummy_right.SetActive(true);
        }
    }


    // Update:
    void Update()
    {
        float escape = Input.GetAxis("escape");
        if (escape > 0.0f)
        {
            Application.Quit();
        }
        if (this.gr == null && this.gc == null)
        {
            consoleText = "Welcome to MiVRy Gesture Recognition!\n"
                             + "This manager allows you to create and record gestures,\n"
                             + "and organize gesture files.";
            return;
        }
        if (training_started)
        {
            if ((this.gr != null && this.gr.isTraining()) || (this.gc != null && this.gc.isTraining()))
            {
                consoleText = "Currently training...\n"
                                 + "Current recognition performance: " + (this.last_performance_report * 100).ToString() + "%.\n";
                GestureManagerVR.refresh();
                return;
            } else
            {
                training_started = false;
                consoleText = "Training finished!\n"
                                 + "Final recognition performance: " + (this.last_performance_report * 100).ToString() + "%.\n";
                GestureManagerVR.refresh();
            }
        } else if ((this.gr != null && this.gr.isTraining()) || (this.gc != null && this.gc.isTraining()))
        {
            training_started = true;
            consoleText = "Currently training...\n"
                             + "Current recognition performance: " + (this.last_performance_report * 100).ToString() + "%.\n";
            GestureManagerVR.refresh();
            return;
        }

        float trigger_left = Input.GetAxis("LeftControllerTrigger");
        float trigger_right = Input.GetAxis("RightControllerTrigger");

        // Single Gesture recognition / 1-handed operation
        if (this.gr != null)
        {
            // If the user is not yet dragging (pressing the trigger) on either controller, he hasn't started a gesture yet.
            if (active_controller == null)
            {
                // If the user presses either controller's trigger, we start a new gesture.
                if (trigger_right > 0.85)
                {
                    // Right controller trigger pressed.
                    active_controller = GameObject.Find("Right Hand");
                }
                else if (trigger_left > 0.85)
                {
                    // Left controller trigger pressed.
                    active_controller = GameObject.Find("Left Hand");
                }
                else
                {
                    // If we arrive here, the user is pressing neither controller's trigger:
                    // nothing to do.
                    return;
                }
                // If we arrive here: either trigger was pressed, so we start the gesture.
                GameObject hmd = GameObject.Find("Main Camera"); // alternative: Camera.main.gameObject
                Vector3 hmd_p = hmd.transform.position;
                Quaternion hmd_q = hmd.transform.rotation;
                gr.startStroke(hmd_p, hmd_q, record_gesture_id);
                gesture_started = true;
                return;
            }

            // If we arrive here, the user is currently dragging with one of the controllers.
            // Check if the user is still dragging or if he let go of the trigger button.
            if (trigger_left > 0.85 || trigger_right > 0.85)
            {
                // The user is still dragging with the controller: continue the gesture.
                Vector3 p = active_controller.transform.position;
                Quaternion q = active_controller.transform.rotation;
                gr.contdStrokeQ(p, q);
                addToStrokeTrail(p);
                return;
            }
            // else: if we arrive here, the user let go of the trigger, ending a gesture.
            active_controller = null;

            // Delete the objectes that we used to display the gesture.
            foreach (string star in stroke)
            {
                Destroy(GameObject.Find(star));
                stroke_index = 0;
            }

            double similarity = 0; // This will receive the similarity value (0~1)
            Vector3 pos = Vector3.zero; // This will receive the position where the gesture was performed.
            double scale = 0; // This will receive the scale at which the gesture was performed.
            Vector3 dir0 = Vector3.zero; // This will receive the primary direction in which the gesture was performed (greatest expansion).
            Vector3 dir1 = Vector3.zero; // This will receive the secondary direction of the gesture.
            Vector3 dir2 = Vector3.zero; // This will receive the minor direction of the gesture (direction of smallest expansion).
            int gesture_id = gr.endStroke(ref similarity, ref pos, ref scale, ref dir0, ref dir1, ref dir2);
            gesture_started = false;
            GestureManagerVR.refresh();

            // If we are currently recording samples for a custom gesture, check if we have recorded enough samples yet.
            if (record_gesture_id >= 0)
            {
                // Currently recording samples for a custom gesture - check how many we have recorded so far.
                consoleText = "Recorded a gesture sample for " + gr.getGestureName(record_gesture_id) + ".\n"
                                 + "Total number of recorded samples for this gesture: " + gr.getGestureNumberOfSamples(record_gesture_id) + ".\n";
                return;
            }
            // else: if we arrive here, we're not recording new samples,
            // but instead have identified a gesture.
            if (gesture_id < 0)
            {
                // Error trying to identify any gesture
                consoleText = "Failed to identify gesture.";
            }
            else
            {
                string gesture_name = gr.getGestureName(gesture_id);
                consoleText = "Identified gesture " + gesture_name + "(" + gesture_id + ")\n(Similarity: " + similarity + ")";
            }
            return;
        }


        // GestureCombination recognition / 2-handed operation
        if (this.gc != null)
        {
            // If the user presses either controller's trigger, we start a new gesture.
            if (trigger_pressed_left == false && trigger_left > 0.9)
            {
                // Controller trigger pressed.
                trigger_pressed_left = true;
                GameObject hmd = GameObject.Find("Main Camera"); // alternative: Camera.main.gameObject
                Vector3 hmd_p = hmd.transform.position;
                Quaternion hmd_q = hmd.transform.rotation;
                int gesture_id = -1;
                if (record_combination_id >= 0)
                {
                    gesture_id = gc.getCombinationPartGesture(record_combination_id, lefthand_combination_part);
                }
                gc.startStroke(lefthand_combination_part, hmd_p, hmd_q, gesture_id);
                gesture_started = true;
            }
            if (trigger_pressed_right == false && trigger_right > 0.9)
            {
                // Controller trigger pressed.
                trigger_pressed_right = true;
                GameObject hmd = GameObject.Find("Main Camera"); // alternative: Camera.main.gameObject
                Vector3 hmd_p = hmd.transform.position;
                Quaternion hmd_q = hmd.transform.rotation;
                int gesture_id = -1;
                if (record_combination_id >= 0)
                {
                    gesture_id = gc.getCombinationPartGesture(record_combination_id, righthand_combination_part);
                }
                gc.startStroke(righthand_combination_part, hmd_p, hmd_q, gesture_id);
                gesture_started = true;
            }
            if (gesture_started == false)
            {
                // nothing to do.
                return;
            }

            // If we arrive here, the user is currently dragging with one of the controllers.
            if (trigger_pressed_left == true)
            {
                if (trigger_left < 0.85)
                {
                    // User let go of a trigger and held controller still
                    gc.endStroke(lefthand_combination_part);
                    trigger_pressed_left = false;
                }
                else
                {
                    // User still dragging or still moving after trigger pressed
                    GameObject left_hand = GameObject.Find("Left Hand");
                    gc.contdStrokeQ(lefthand_combination_part, left_hand.transform.position, left_hand.transform.rotation);
                    // Show the stroke by instatiating new objects
                    addToStrokeTrail(left_hand.transform.position);
                }
            }

            if (trigger_pressed_right == true)
            {
                if (trigger_right < 0.85)
                {
                    // User let go of a trigger and held controller still
                    gc.endStroke(righthand_combination_part);
                    trigger_pressed_right = false;
                }
                else
                {
                    // User still dragging or still moving after trigger pressed
                    GameObject right_hand = GameObject.Find("Right Hand");
                    gc.contdStrokeQ(righthand_combination_part, right_hand.transform.position, right_hand.transform.rotation);
                    // Show the stroke by instatiating new objects
                    addToStrokeTrail(right_hand.transform.position);
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
            foreach (string star in stroke)
            {
                Destroy(GameObject.Find(star));
                stroke_index = 0;
            }

            double similarity = 0; // This will receive a similarity value (0~1).
            int recognized_combination_id = gc.identifyGestureCombination(ref similarity);

            // If we are currently recording samples for a custom gesture, check if we have recorded enough samples yet.
            if (record_combination_id >= 0)
            {
                // Currently recording samples for a custom gesture - check how many we have recorded so far.
                int connected_gesture_id_left = gc.getCombinationPartGesture(record_combination_id, lefthand_combination_part);
                int connected_gesture_id_right = gc.getCombinationPartGesture(record_combination_id, righthand_combination_part);
                int num_samples_left = gc.getGestureNumberOfSamples(lefthand_combination_part, connected_gesture_id_left);
                int num_samples_right = gc.getGestureNumberOfSamples(righthand_combination_part, connected_gesture_id_right);
                // Currently recording samples for a custom gesture - check how many we have recorded so far.
                consoleText = "Recorded a gesture sample for " + gc.getGestureCombinationName(record_combination_id) + ".\n"
                                 + "Total number of recorded samples for this gesture: " + num_samples_left + " left / " + num_samples_right + " right.\n";
                GestureManagerVR.refresh();
                return;
            }
            // else: if we arrive here, we're not recording new samples for custom gestures,
            // but instead have identified a new gesture.
            // Perform the action associated with that gesture.
            if (recognized_combination_id < 0)
            {
                // Error trying to identify any gesture
                consoleText = "Failed to identify gesture.";
            }
            else
            {
                string combination_name = gc.getGestureCombinationName(recognized_combination_id);
                consoleText = "Identified gesture combination '"+ combination_name+"' ("+ recognized_combination_id + ")\n(Similarity: " + similarity + ")";
            }
        }
    }

    // Callback function to be called by the gesture recognition plug-in during the learning process.
    [MonoPInvokeCallback(typeof(GestureRecognition.TrainingCallbackFunction))]
    public static void trainingUpdateCallback(double performance, IntPtr ptr)
    {
        if (ptr.ToInt32() == 0)
        {
            return;
        }
        // Get the script/scene object back from metadata.
        GCHandle obj = (GCHandle)ptr;
        GestureManager me = (obj.Target as GestureManager);
        // Update the performance indicator with the latest estimate.
        me.last_performance_report = performance;
    }

    // Callback function to be called by the gesture recognition plug-in when the learning process was finished.
    [MonoPInvokeCallback(typeof(GestureRecognition.TrainingCallbackFunction))]
    public static void trainingFinishCallback(double performance, IntPtr ptr)
    {
        if (ptr.ToInt32() == 0)
        {
            return;
        }
        // Get the script/scene object back from metadata.
        GCHandle obj = (GCHandle)ptr;
        GestureManager me = (obj.Target as GestureManager);
        // Update the performance indicator with the latest estimate.
        me.last_performance_report = performance;
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
    

    // Helper function to get the actual file path for a file to load
    public string getLoadPath(string file)
    {
#if UNITY_ANDROID
        // On android, the file is in the .apk,
        // so we need to first "download" it to the apps' cache folder.
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        string cachePath = activity.Call<AndroidJavaObject>("getCacheDir").Call<string>("getCanonicalPath");
        UnityWebRequest request = UnityWebRequest.Get(Application.streamingAssetsPath + "/" + file);
        request.SendWebRequest();
        while (!request.isDone)
        {
            // wait for file extraction to finish
        }
        if (request.isNetworkError)
        {
            this.consoleText = "Failed to extract sample gesture database file from apk.";
            return file;
        }
        string path = cachePath + "/" + file;
        try {
            Directory.CreateDirectory(path);
            Directory.Delete(path);
        } catch (Exception e) { }
        try {
            File.WriteAllBytes(path, request.downloadHandler.data);
        } catch (Exception e) {
            this.consoleText = "Exception writing temporary file: " + e.ToString();
        }
        return path;
#elif UNITY_EDITOR
        return (!Path.IsPathRooted(file))
            ? "Assets/GestureRecognition/" + file
            : file;
#else
        return (!Path.IsPathRooted(file))
            ? Application.streamingAssetsPath + "/" + file
            : file;
#endif
    }

    // Helper function to get the actual file path for a file to save
    public string getSavePath(string file)
    {
#if UNITY_ANDROID
        return Application.persistentDataPath + "/" + file;
#elif UNITY_EDITOR
        return (!Path.IsPathRooted(file))
            ? "Assets/GestureRecognition/" + file
            : file;
#else
        return (!Path.IsPathRooted(file))
            ? Application.streamingAssetsPath + "/" + file
            : file;
#endif
    }

    // Helper function to load from gestures file.
    public bool loadFromFile()
    {
        if (this.gr != null)
        {
            string path = getLoadPath(this.file_load_gestures);
            int ret = this.gr.loadFromFile(path);
            if (ret == 0)
            {
                this.consoleText = "Gesture file loaded successfully";
                return true;
            } else
            {
                this.consoleText = $"[ERROR] Failed to load gesture file\n{path}\n{GestureRecognition.getErrorMessage(ret)}";
                return false;
            }
        }
        else if (this.gc != null)
        {
            string path = getLoadPath(this.file_load_combinations);
            int ret = this.gc.loadFromFile(path);
            if (ret == 0)
            {
                this.consoleText = "Gesture combinations file loaded successfully";
                return true;
            }
            else
            {
                this.consoleText = $"[ERROR] Failed to load gesture combinations file\n{path}\n{GestureRecognition.getErrorMessage(ret)}";
                return false;
            }
        }
        this.consoleText = "[ERROR] No Gesture Recognition object\nto load gestures into.";
        return false;
    }

    // Helper function to import gestures file.
    public bool importFromFile()
    {
        if (this.gr != null)
        {
            string path = getLoadPath(this.file_import_gestures);
            int ret = this.gr.importFromFile(path);
            if (ret == 0)
            {
                this.consoleText = "Gesture file imported successfully";
                return true;
            } else
            {
                this.consoleText = $"[ERROR] Failed to import gesture file\n{path}\n{GestureRecognition.getErrorMessage(ret)}";
                return false;
            }
        }
        else if (this.gc != null)
        {
            string path = getLoadPath(this.file_import_combinations);
            int ret = this.gc.importFromFile(path);
            if (ret == 0)
            {
                this.consoleText = "Gesture combinations file imported successfully";
                return true;
            }
            else
            {
                this.consoleText = $"[ERROR] Failed to import gesture combinations file\n{path}\n{GestureRecognition.getErrorMessage(ret)}";
                return false;
            }
        }
        this.consoleText = "[ERROR] No Gesture Recognition object\nto import gestures into.";
        return false;
    }

    // Helper function to save gestures to file. 
    public bool saveToFile()
    {
        if (this.gr != null)
        {
            string path = getSavePath(this.file_save_gestures);
            int ret = this.gr.saveToFile(path);
            if (ret == 0)
            {
                this.consoleText = "Gesture file saved successfully at\n" + path;
                return true;
            }
            else
            {
                this.consoleText = $"[ERROR] Failed to saved gesture file\n{path}\n{GestureRecognition.getErrorMessage(ret)}";
                return false;
            }
        }
        else if (this.gc != null)
        {
            string path = getSavePath(this.file_save_combinations);
            int ret = this.gc.saveToFile(path);
            if (ret == 0)
            {
                this.consoleText = "Gesture combinations file saved successfully at\n" + path;
                return true;
            }
            else
            {
                this.consoleText = $"[ERROR] Failed to save gesture combinations file\n{path}\n{GestureRecognition.getErrorMessage(ret)}";
                return false;
            }
        }
        this.consoleText = "[ERROR] No Gesture Recognition object\nto save to file.";
        return false;
    }

    public string consoleText
    {
        set
        {
            if (this.ConsoleText != null)
                this.ConsoleText.text = value;
            else
                Debug.Log(value);
        }
    }
}
