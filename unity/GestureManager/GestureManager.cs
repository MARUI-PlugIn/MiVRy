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
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif

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
                    if (this.license_id != null && this.license_key != null && this.license_id != "")
                    {
                        gr.activateLicense(license_id, license_key);
                    }
                    gr.setTrainingUpdateCallback(GestureManager.trainingUpdateCallback);
                    gr.setTrainingUpdateCallbackMetadata((IntPtr)me);
                    gr.setTrainingFinishCallback(GestureManager.trainingFinishCallback);
                    gr.setTrainingFinishCallbackMetadata((IntPtr)me);
                    gr.setLoadingFinishCallbackFunction(GestureManager.loadingFinishCallback);
                    gr.setLoadingFinishCallbackMetadata((IntPtr)me);
                    gr.setSavingFinishCallbackFunction(GestureManager.savingFinishCallback);
                    gr.setSavingFinishCallbackMetadata((IntPtr)me);
                    if (ConsoleText != null)
                        ConsoleText.text = "Created Gesture Recognition Object\n(for one-handed gestures)";
                }
            } else
            {
                gr = null;
                if (gc == null || gc.numberOfParts() != value) {
                    gc = new GestureCombinations(value);
                    if (this.license_id != null && this.license_key != null && this.license_id != "")
                    {
                        gc.activateLicense(license_id, license_key);
                    }
                    gc.setTrainingUpdateCallback(GestureManager.trainingUpdateCallback);
                    gc.setTrainingUpdateCallbackMetadata((IntPtr)me);
                    gc.setTrainingFinishCallback(GestureManager.trainingFinishCallback);
                    gc.setTrainingFinishCallbackMetadata((IntPtr)me);
                    gc.setLoadingFinishCallbackFunction(GestureManager.loadingFinishCallback);
                    gc.setLoadingFinishCallbackMetadata((IntPtr)me);
                    gc.setSavingFinishCallbackFunction(GestureManager.savingFinishCallback);
                    gc.setSavingFinishCallbackMetadata((IntPtr)me);
                    if (ConsoleText != null)
                        ConsoleText.text = $"Created Gesture Combination Object\n(for {value} parts)";
                }
            }
            GestureManagerVR.refresh();
        }
    }

    public string license_id = "";
    public string license_key = "";
    public Mivry.UnityXrPlugin unityXrPlugin = Mivry.UnityXrPlugin.OpenXR;
    public Mivry.MivryCoordinateSystem mivryCoordinateSystem = Mivry.MivryCoordinateSystem.OpenXR;

    
    public GestureRecognition.FrameOfReference frameOfReferenceYaw {
        get
        {
            if (this.mivryCoordinateSystem == Mivry.MivryCoordinateSystem.UnrealEngine)
            {
                if (gr != null)
                {
                    return gr.frameOfReferenceZ;
                }
                if (gc != null)
                {
                    return (GestureRecognition.FrameOfReference)gc.frameOfReferenceZ;
                }
            } else
            {
                if (gr != null)
                {
                    return gr.frameOfReferenceY;
                }
                if (gc != null)
                {
                    return (GestureRecognition.FrameOfReference)gc.frameOfReferenceY;
                }
            }
            return GestureRecognition.FrameOfReference.Head;
        }
        set
        {
            if (this.mivryCoordinateSystem == Mivry.MivryCoordinateSystem.UnrealEngine)
            {
                if (gr != null)
                {
                    gr.frameOfReferenceZ = value;
                }
                if (gc != null)
                {
                    gc.frameOfReferenceZ = (GestureCombinations.FrameOfReference)value;
                }
            }
            else
            {
                if (gr != null)
                {
                    gr.frameOfReferenceY = value;
                }
                if (gc != null)
                {
                    gc.frameOfReferenceY = (GestureCombinations.FrameOfReference)value;
                }
            }
            GestureManagerVR.me?.submenuFrameOfReference?.GetComponent<SubmenuFrameOfReference>().refresh();
        }
    }

    public GestureRecognition.FrameOfReference frameOfReferenceUpDownPitch
    {
        get
        {
            if (this.mivryCoordinateSystem == Mivry.MivryCoordinateSystem.UnrealEngine)
            {
                if (gr != null)
                {
                    return gr.frameOfReferenceY;
                }
                if (gc != null)
                {
                    return (GestureRecognition.FrameOfReference)gc.frameOfReferenceY;
                }
            }
            else
            {
                if (gr != null)
                {
                    return gr.frameOfReferenceX;
                }
                if (gc != null)
                {
                    return (GestureRecognition.FrameOfReference)gc.frameOfReferenceX;
                }
            }
            return GestureRecognition.FrameOfReference.Head;
        }
        set
        {
            if (this.mivryCoordinateSystem == Mivry.MivryCoordinateSystem.UnrealEngine)
            {
                if (gr != null)
                {
                    gr.frameOfReferenceY = value;
                }
                if (gc != null)
                {
                    gc.frameOfReferenceY = (GestureCombinations.FrameOfReference)value;
                }
            }
            else
            {
                if (gr != null)
                {
                    gr.frameOfReferenceX = value;
                }
                if (gc != null)
                {
                    gc.frameOfReferenceX = (GestureCombinations.FrameOfReference)value;
                }
            }
            GestureManagerVR.me?.submenuFrameOfReference?.GetComponent<SubmenuFrameOfReference>().refresh();
        }
    }

    public GestureRecognition.FrameOfReference frameOfReferenceRollTilt
    {
        get
        {
            if (this.mivryCoordinateSystem == Mivry.MivryCoordinateSystem.UnrealEngine)
            {
                if (gr != null)
                {
                    return gr.frameOfReferenceX;
                }
                if (gc != null)
                {
                    return (GestureRecognition.FrameOfReference)gc.frameOfReferenceX;
                }
            }
            else
            {
                if (gr != null)
                {
                    return gr.frameOfReferenceZ;
                }
                if (gc != null)
                {
                    return (GestureRecognition.FrameOfReference)gc.frameOfReferenceZ;
                }
            }
            return GestureRecognition.FrameOfReference.Head;
        }
        set
        {
            if (this.mivryCoordinateSystem == Mivry.MivryCoordinateSystem.UnrealEngine)
            {
                if (gr != null)
                {
                    gr.frameOfReferenceX = value;
                }
                if (gc != null)
                {
                    gc.frameOfReferenceX = (GestureCombinations.FrameOfReference)value;
                }
            }
            else
            {
                if (gr != null)
                {
                    gr.frameOfReferenceZ = value;
                }
                if (gc != null)
                {
                    gc.frameOfReferenceZ = (GestureCombinations.FrameOfReference)value;
                }
            }
            GestureManagerVR.me?.submenuFrameOfReference?.GetComponent<SubmenuFrameOfReference>().refresh();
        }
    }

    public GestureRecognition.RotationOrder frameOfReferenceRotationOrder
    {
        get
        {
            if (gr != null)
            {
                return gr.frameOfReferenceRotationOrder;
            }
            if (gc != null)
            {
                return (GestureRecognition.RotationOrder)gc.frameOfReferenceRotationOrder;
            }
            return GestureRecognition.RotationOrder.YXZ;
        }
        set
        {
            if (gr != null)
            {
                gr.frameOfReferenceRotationOrder = value;
            }
            if (gc != null)
            {
                gc.frameOfReferenceRotationOrder = (GestureCombinations.RotationOrder)value;
            }
            GestureManagerVR.me?.submenuFrameOfReference?.GetComponent<SubmenuFrameOfReference>().refresh();
        }
    }

    public string[] file_imports = {
        "Samples/Sample_Continuous_Gestures.dat",
        "Samples/Sample_Military_Gestures.dat",
        "Samples/Sample_OneHanded_Gestures.dat",
        "Samples/Sample_Pixie_Gestures.dat",
        "Samples/Sample_TwoHanded_Gestures.dat",
    };

    public string file_load_combinations = "Samples/Sample_TwoHanded_Gestures.dat";
    public string file_import_combinations = "Samples/Sample_Military_Gestures.dat";
    public string file_load_subgestures = "Samples/Sample_OneHanded_Gestures.dat";
    public int file_load_subgestures_i = 0;
    public string file_load_gestures = "Samples/Sample_OneHanded_Gestures.dat";
    public string file_import_gestures = "Samples/Sample_Pixie_Gestures.dat";
    public string file_save_combinations = "Samples/Sample_TwoHanded_MyGestures.dat";
    // public string file_save_subgestures = "Samples/Sample_TwoHanded_MyGesturesLeft.dat";
    // public int file_save_subgestures_i = 0;
    public string file_save_gestures = "Samples/Sample_OneHanded_MyGestures.dat";

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
    [System.NonSerialized] public GestureRecognition gr = null;
    [System.NonSerialized] public GestureCombinations gc = null;

    // The text field to display instructions.
    private Text ConsoleText;

    // The game object associated with the currently active controller (if any):
    private GameObject active_controller = null;

    // The pointing tip of the active_controller (used for visualization).
    private GameObject active_controller_pointer = null;

    // Whether the training process is was recently started.
    [System.NonSerialized] public bool training_started = false;

    // Last reported recognition performance (during training).
    // 0 = 0% correctly recognized, 1 = 100% correctly recognized.
    [System.NonSerialized] public double last_performance_report = 0;

    // Whether the loading process is was recently started.
    [System.NonSerialized] public bool loading_started = false;

    // Result of the loading result. Zero on success, a negative error code on failure.
    [System.NonSerialized] public int loading_result = 0;

    // Whether the saving process is was recently started.
    [System.NonSerialized] public bool saving_started = false;

    // Result of the saving result. Zero on success, a negative error code on failure.
    [System.NonSerialized] public int saving_result = 0;

    // The path where the file was saved.
    private string saving_path = "";

    // Temporary storage for objects to display the gesture stroke.
    private List<string> stroke = new List<string>(); 

    // Temporary counter variable when creating objects for the stroke display:
    private int stroke_index = 0; 
    
    // Handle to this object/script instance, so that callbacks from the plug-in arrive at the correct instance.
    private GCHandle me;

    // Whether the user is currently pressing the contoller trigger.
    private bool trigger_pressed_left = false;
    private bool trigger_pressed_right = false;

    // Whether gesturing (performing gesture motions) is currently possible.
    [System.NonSerialized] public bool gesturing_enabled = true;

    // Wether a gesture was already started
    [System.NonSerialized] public bool gesture_started = false;

    // Whether or not to update (and thus compensate for) the head position during gesturing.
    public bool compensate_head_motion = false;

    // File/folder suggestions for the load files button
    [System.NonSerialized] public int file_suggestion = 0;
    [System.NonSerialized] public List<string> file_suggestions = new List<string>();

    public GestureManager() : base()
    {
        me = GCHandle.Alloc(this);
    }

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
        else if (type.Contains("Vive")) // "HTC Vive Controller OpenXR"
        {
            controller_vive.SetActive(true);
        }
        else
        {
            controller_dummy.SetActive(true);
        }
    }

    // Helper function to handle new VR controllers being detected.
    void DeviceConnected(UnityEngine.XR.InputDevice device)
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
        ConsoleText = GameObject.Find("ConsoleText").GetComponent<Text>();
        consoleText = "Welcome to MiVRy Gesture Manager!\n"
                    + "(" + GestureRecognition.getVersionString() + ")\n"
                    + "This manager allows you to\ncreate and record gestures,\n"
                    + "and organize gesture files.";

        me = GCHandle.Alloc(this);

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
        List<UnityEngine.XR.InputDevice> devices = new List<UnityEngine.XR.InputDevice>();
        InputDevices.GetDevices(devices);
        foreach (var device in devices) {
            DeviceConnected(device);
        }

        for (int i=this.file_imports.Length-1; i>=0; i--) {
            this.importFromStreamingAssets(this.file_imports[i]);
        }
    }


    // Update:
    void Update()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Application.Quit();
        }
#else
        float escape = Input.GetAxis("escape");
        if (escape > 0.0f)
        {
            Application.Quit();
        }
#endif
        if (this.gr == null && this.gc == null)
        {
            consoleText = "Welcome to MiVRy Gesture Manager!\n"
                             + "(" + GestureRecognition.getVersionString() + ")\n"
                             + "This manager allows you to\ncreate and record gestures,\n"
                             + "and organize gesture files.";
            return;
        }
        if (training_started)
        {
            if ((this.gr != null && this.gr.isTraining()) || (this.gc != null && this.gc.isTraining()))
            {
                consoleText = "Currently training...\n"
                                 + "Current recognition performance: " + (this.last_performance_report * 100).ToString("0.00") + "%.\n";
                GestureManagerVR.refresh();
                return;
            } else
            {
                training_started = false;
                consoleText = "Training finished!\n"
                                 + "Final recognition performance: " + (this.last_performance_report * 100).ToString("0.00") + "%.\n";
                GestureManagerVR.refresh();
            }
        } else if ((this.gr != null && this.gr.isTraining()) || (this.gc != null && this.gc.isTraining()))
        {
            training_started = true;
            consoleText = "Currently training...\n"
                             + "Current recognition performance: " + (this.last_performance_report * 100).ToString("0.00") + "%.\n";
            GestureManagerVR.refresh();
            return;
        }

        if (loading_started)
        {
            if ((this.gr != null && this.gr.isLoading()) || (this.gc != null && this.gc.isLoading()))
            {
                consoleText = "Currently loading...\n";
                return;
            } else
            {
                loading_started = false;
                consoleText = "Loading Finished!\n"
                                 + "Result: " + GestureRecognition.getErrorMessage(this.loading_result) + "\n";
                GestureManagerVR.refresh();
            }
        } else if ((this.gr != null && this.gr.isLoading()) || (this.gc != null && this.gc.isLoading()))
        {
            loading_started = true;
            consoleText = "Currently loading...\n";
            GestureManagerVR.refresh();
            return;
        }

        if (saving_started)
        {
            if ((this.gr != null && this.gr.isSaving()) || (this.gc != null && this.gc.isSaving()))
            {
                consoleText = "Currently saving...\n";
                return;
            }
            else
            {
                saving_started = false;
                if (this.saving_result == 0)
                {
                    consoleText = "Saving Finished!\n"
                        + "File Location : " + this.saving_path;
                } else if (this.saving_result == -3)
                {
                    consoleText = $"Saving Failed!\nThe path {this.saving_path} is not writable.";
                } else
                {
                    consoleText = "Saving Failed!\n"
                        + GestureRecognition.getErrorMessage(this.saving_result);
                }
                GestureManagerVR.refresh();
            }
        }
        else if ((this.gr != null && this.gr.isSaving()) || (this.gc != null && this.gc.isSaving()))
        {
            saving_started = true;
            consoleText = "Currently saving...\n";
            GestureManagerVR.refresh();
            return;
        }

        if (!this.gesturing_enabled)
        {
            return;
        }

#if ENABLE_INPUT_SYSTEM
        float trigger_left = getInputControlValue("<XRController>{LeftHand}/trigger");
        float trigger_right = getInputControlValue("<XRController>{RightHand}/trigger");
#else
        float trigger_left = Input.GetAxis("LeftControllerTrigger");
        float trigger_right = Input.GetAxis("RightControllerTrigger");
#endif

        GameObject hmd = Camera.main.gameObject; // alternative: GameObject.Find("Main Camera");
        Vector3 hmd_p = hmd.transform.position;
        Quaternion hmd_q = hmd.transform.rotation;
        Mivry.convertHeadInput(this.mivryCoordinateSystem, ref hmd_p, ref hmd_q);

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
                    active_controller_pointer = GameObject.FindGameObjectWithTag("Right Pointer");
                }
                else if (trigger_left > 0.85)
                {
                    // Left controller trigger pressed.
                    active_controller = GameObject.Find("Left Hand");
                    active_controller_pointer = GameObject.FindGameObjectWithTag("Left Pointer");
                }
                else
                {
                    // If we arrive here, the user is pressing neither controller's trigger:
                    // nothing to do.
                    return;
                }
                // If we arrive here: either trigger was pressed, so we start the gesture.
                gr.startStroke(hmd_p, hmd_q, record_gesture_id);
                gesture_started = true;
                return;
            }

            // If we arrive here, the user is currently dragging with one of the controllers.
            // Check if the user is still dragging or if he let go of the trigger button.
            if (trigger_left > 0.85 || trigger_right > 0.85)
            {
                // The user is still dragging with the controller: continue the gesture.
                if (this.compensate_head_motion)
                {
                    gr.updateHeadPosition(hmd_p, hmd_q);
                }
                Vector3 p = active_controller.transform.position;
                Quaternion q = active_controller.transform.rotation;
                Mivry.convertHandInput(this.unityXrPlugin, this.mivryCoordinateSystem, ref p, ref q);
                gr.contdStrokeQ(p, q);
                addToStrokeTrail(active_controller_pointer.transform.position);
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
                consoleText = "Failed to identify gesture: " + GestureRecognition.getErrorMessage(gesture_id);
            }
            else
            {
                string gesture_name = gr.getGestureName(gesture_id);
                consoleText = "Identified gesture " + gesture_name + "(" + gesture_id + ")\n(Similarity: " + similarity.ToString("0.000") + ")";
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
            if (this.compensate_head_motion)
            {
                gc.updateHeadPosition(hmd_p, hmd_q);
            }
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
                    Vector3 p = left_hand.transform.position;
                    Quaternion q = left_hand.transform.rotation;
                    Mivry.convertHandInput(this.unityXrPlugin, this.mivryCoordinateSystem, ref p, ref q);
                    gc.contdStrokeQ(lefthand_combination_part, p, q);
                    // Show the stroke by instatiating new objects
                    GameObject left_hand_pointer = GameObject.FindGameObjectWithTag("Left Pointer");
                    addToStrokeTrail(left_hand_pointer.transform.position);
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
                    Vector3 p = right_hand.transform.position;
                    Quaternion q = right_hand.transform.rotation;
                    Mivry.convertHandInput(this.unityXrPlugin, this.mivryCoordinateSystem, ref p, ref q);
                    gc.contdStrokeQ(righthand_combination_part, p, q);
                    // Show the stroke by instatiating new objects
                    GameObject right_hand_pointer = GameObject.FindGameObjectWithTag("Right Pointer");
                    addToStrokeTrail(right_hand_pointer.transform.position);
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
                consoleText = "Failed to identify gesture: " + GestureRecognition.getErrorMessage(recognized_combination_id);
            }
            else
            {
                string combination_name = gc.getGestureCombinationName(recognized_combination_id);
                consoleText = "Identified gesture combination '"+ combination_name+"' ("+ recognized_combination_id + ")\n(Similarity: " + similarity.ToString("0.000") + ")";
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

    // Callback function to be called by the gesture recognition plug-in when the loading process was finished.
    [MonoPInvokeCallback(typeof(GestureRecognition.LoadingCallbackFunction))]
    public static void loadingFinishCallback(int result, IntPtr ptr)
    {
        if (ptr.ToInt32() == 0)
        {
            return;
        }
        // Get the script/scene object back from metadata.
        GCHandle obj = (GCHandle)ptr;
        GestureManager me = (obj.Target as GestureManager);
        me.loading_result = result;
    }

    // Callback function to be called by the gesture recognition plug-in when the saving process was finished.
    [MonoPInvokeCallback(typeof(GestureRecognition.SavingCallbackFunction))]
    public static void savingFinishCallback(int result, IntPtr ptr)
    {
        if (ptr.ToInt32() == 0)
        {
            return;
        }
        // Get the script/scene object back from metadata.
        GCHandle obj = (GCHandle)ptr;
        GestureManager me = (obj.Target as GestureManager);
        me.saving_result = result;
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
        if (this.compensate_head_motion)
        {
            star.transform.SetParent(Camera.main.gameObject.transform);
        }
        stroke.Add(star.name);
    }

    public bool importFromStreamingAssets(string file)
    {
        string srcPath = Application.streamingAssetsPath + "/" + file;
        string dstPath = Application.persistentDataPath + "/" + file;
#if !UNITY_EDITOR && UNITY_ANDROID
        // On android, the file is in the .apk,
        // so we need to first "download" it to the apps' cache folder.
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        UnityWebRequest request = UnityWebRequest.Get(srcPath);
        request.SendWebRequest();
        while (!request.isDone) {
            // wait for file extraction to finish
        }
        if (request.result == UnityWebRequest.Result.ConnectionError) {
            this.consoleText = "Failed to extract sample gesture database file from apk.";
            return false;
        }
        // string cachePath = activity.Call<AndroidJavaObject>("getCacheDir").Call<string>("getCanonicalPath");
        try {
            Directory.CreateDirectory(dstPath);
        } catch (Exception /* e */) { }
        try {
            Directory.Delete(dstPath);
        } catch (Exception /* e */) { }
        try {
            File.WriteAllBytes(dstPath, request.downloadHandler.data);
        } catch (Exception e) {
            this.consoleText = "Exception writing temporary file: " + e.ToString();
            return false;
        }
        return true;
#else
        try {
            Directory.CreateDirectory(dstPath);
        } catch (Exception /* e */) { }
        try {
            Directory.Delete(dstPath);
        } catch (Exception /* e */) { }
        try {
            File.Copy(srcPath, dstPath, true);
        } catch (Exception e) {
            this.consoleText = "Exception importing file: " + e.ToString();
            return false;
        }
        return true;
#endif
    }

    // Helper function to get the actual file path for a file to load
    public string getLoadPath(string file)
    {
        return (Path.IsPathRooted(file))
            ? file
            : Application.persistentDataPath + "/" + file;
    }

    // Helper function to get the actual file path for a file to save
    public string getSavePath(string file)
    {
        return (Path.IsPathRooted(file))
            ? file
            : Application.persistentDataPath + "/" + file;
    }

    // Helper function to load from gestures file.
    public bool loadFromFile()
    {
        if (this.gr != null)
        {
            string path = getLoadPath(this.file_load_gestures);
            this.consoleText = "Loading Gesture file...";
            int ret = this.gr.loadFromFileAsync(path);
            if (ret != 0)
            {
                byte[] file_contents = File.ReadAllBytes(path);
                if (file_contents == null || file_contents.Length == 0)
                {
                    this.consoleText = $"Could not find gesture database file\n({path}).";
                    return false;
                }
                ret = this.gr.loadFromBuffer(file_contents);
                if (ret != 0)
                {
                    this.consoleText = $"[ERROR] Failed to load gesture file\n{path}\n{GestureRecognition.getErrorMessage(ret)}";
                    return false;
                }
            }
            this.loading_started = true;
            return true;
        }
        else if (this.gc != null)
        {
            string path = getLoadPath(this.file_load_combinations);
            this.consoleText = "Loading Gesture Combinations file...";
            int ret = this.gc.loadFromFileAsync(path);
            if (ret != 0)
            {
                byte[] file_contents = File.ReadAllBytes(path);
                if (file_contents == null || file_contents.Length == 0)
                {
                    this.consoleText = $"Could not find gesture database file\n({path}).";
                    return false;
                }
                ret = this.gc.loadFromBuffer(file_contents);
                if (ret != 0)
                {
                    this.consoleText = $"[ERROR] Failed to load gesture combinations file\n{path}\n{GestureRecognition.getErrorMessage(ret)}";
                    return false;
                }
            }
            this.loading_started = true;
            return true;
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
            this.saving_path = getSavePath(this.file_save_gestures);
            int ret = this.gr.saveToFileAsync(this.saving_path);
            if (ret == 0)
            {
                this.consoleText = "Started saving file at\n" + this.saving_path;
                this.saving_started = true;
                return true;
            }
            else
            {
                this.consoleText = $"[ERROR] Failed to saved gesture file at\n'{this.saving_path}'\n{GestureRecognition.getErrorMessage(ret)}";
                return false;
            }
        }
        else if (this.gc != null)
        {
            this.saving_path = getSavePath(this.file_save_combinations);
            int ret = this.gc.saveToFileAsync(this.saving_path);
            if (ret == 0)
            {
                this.consoleText = "Started saving file at\n" + this.saving_path;
                this.saving_started = true;
                return true;
            }
            else
            {
                this.consoleText = $"[ERROR] Failed to save gesture combinations file at \n'{this.saving_path}'\n{GestureRecognition.getErrorMessage(ret)}";
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


#if ENABLE_INPUT_SYSTEM
    public static float getInputControlValue(string controlName)
    {

        InputControl control = InputSystem.FindControl(controlName); // eg: "<XRController>{RightHand}/trigger"
        switch (control)
        {
            case AxisControl axisControl:
                return axisControl.ReadValue();
            case DoubleControl doubleControl:
                return (float)doubleControl.ReadValue();
            case IntegerControl integerControl:
                return integerControl.ReadValue();
            case QuaternionControl quaternionControl:
                Debug.LogError($"Mivry.getInputControlValue : QuaternionControl '${controlName}' not supported.");
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
        return 0.0f;
    }
#endif
}
