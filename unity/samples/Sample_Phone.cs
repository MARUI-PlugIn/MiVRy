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
using TouchPhase = UnityEngine.TouchPhase;
using System;
using System.IO;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.Networking;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif

namespace MiVRy {
public class Sample_Phone : MonoBehaviour
{
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

    // ID of the gesture currently being recorded,
    // or: -1 if not currently recording a new gesture,
    // or: -2 if the AI is currently trying to learn to identify gestures
    // or: -3 if the AI has recently finished learning to identify gestures
	private int recording_gesture = -1; 

    // Last reported recognition performance (during training).
    // 0 = 0% correctly recognized, 1 = 100% correctly recognized.
    private double last_performance_report = 0;
    

    // Button ID for recording/performing a gesture.
    private const int ButtonID_None = 0;

    // Button ID for recording/performing a gesture.
    private const int ButtonID_Record = 1;

    // Button ID for starting/stopping training or creating a new gesture.
    private const int ButtonID_Train = 2;

    // Button ID for exiting the app.
    private const int ButtonID_Exit = 3;

    // ID of the button currently activated.
    private int button_id = ButtonID_None;


    // Button state ID for button not being pressed.
    private const int ButtonState_Idle = 0;

    // Button state ID for button being pressed.
    private const int ButtonState_Pressed = 1;

    // Button state ID for button being released.
    private const int ButtonState_Released = 2;

    // Current button state.
    private int button_state = ButtonState_Idle;


    // Control variable whether the user is currently performing a gesture.
    private bool making_stroke = false;

    // List of gestures to use as gesture names.
    private readonly string[] gesture_name_suggestions = new string[] {
        "tycoon", "peasant", "police", "discover", "construct",
        "skip", "nuclear", "defeat", "shoot", "vote", "sell",
        "wife", "brother", "mail" , "forget", "health", "future",
        "minority", "moment", "layer", "stealth"
    };
    // List of yet-unused gesture names.
    private List<string> gesture_name_suggestions_list = null;

    // Random number generator.
    private System.Random rnd = new System.Random();

    // Helper function to get random word suggestions for gesture names.
    private string getRandomWord()
    {
        if (gesture_name_suggestions_list == null)
        {
            gesture_name_suggestions_list = new List<string>();
            for (int i=0; i< gesture_name_suggestions.Length; i++)
            {
                gesture_name_suggestions_list.Insert(0, gesture_name_suggestions[i]);
            }
        }
        if (gesture_name_suggestions_list.Count == 0)
        {
            // used all suggestions - use random number as string
            return rnd.Next(0, 1000).ToString();
        }
        int index = rnd.Next(0, gesture_name_suggestions_list.Count);
        string random_word = gesture_name_suggestions_list[index];
        gesture_name_suggestions_list.RemoveAt(index);
        return random_word;
    }

    // Handle to this object/script instance, so that callbacks from the plug-in arrive at the correct instance.
    GCHandle me;

    // Initialization:
    void Start ()
    {
        HUDText = GameObject.Find("HUDText").GetComponent<Text>();
        me = GCHandle.Alloc(this);

        if (this.LicenseName != null && this.LicenseKey != null && this.LicenseName.Length > 0)
        {
            if (this.gr.activateLicense(this.LicenseName, this.LicenseKey) != 0)
            {
                Debug.LogError("Failed to activate license");
            }
        }

        // Create a first gesture to record samples for
        string random_word = getRandomWord();
        recording_gesture = gr.createGesture(random_word);
        HUDText.text = "\n\n[TOUCH AND HOLD HERE]\nto record gesture sample.\nGesture keyword:\n"+ random_word;

        Input.gyro.enabled = true;
    }
    

    // Update:
    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector2 touch_pos = touch.position;
            Resolution res = Screen.currentResolution;
            touch_pos.x = touch_pos.x / res.width;
            touch_pos.y = touch_pos.y / res.height;
            if (touch.phase == TouchPhase.Began)
            {
                if (touch_pos.x > 0.1f && touch_pos.x < 0.9f)
                {
                    button_state = ButtonState_Pressed;
                    if (touch_pos.y > 0.3f && touch_pos.y < 0.7f)
                    {
                        button_id = ButtonID_Record;
                    }
                    else if (touch_pos.y > 0.8f)
                    {
                        button_id = ButtonID_Train;
                    }
                    else if (touch_pos.y < 0.2f)
                    {
                        button_id = ButtonID_Exit;
                    }
                }
            } else if (touch.phase == TouchPhase.Ended) {
                button_state = ButtonState_Released;
            }
        } else {
            button_id = ButtonID_None;
            button_state = ButtonState_Idle;
        }

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current.escapeKey.wasPressedThisFrame) {
            Application.Quit();
        }
#else
        float escape = Input.GetAxis("escape");
        if (escape > 0.0f)
        {
            Application.Quit();
        }
#endif

        if (button_id == ButtonID_Exit && button_state == ButtonState_Released)
        {
            Application.Quit();
        }
        
        // If recording_gesture is -3, that means that the AI has recently finished learning a new gesture.
        if (recording_gesture == -3) {
            // Save the data to file.
#if UNITY_EDITOR
            string save_file_path = "Assets/Samples";
#elif UNITY_ANDROID
            string save_file_path = Application.persistentDataPath;
#else
            string save_file_path = Application.streamingAssetsPath;
#endif
            this.gr.saveToFile(save_file_path + "/Sample_Phone_SavedGestures.dat");
            // Show "finished" message.
            double performance = gr.recognitionScore();
            HUDText.text = "Training finished!\n(Performance = " + (performance * 100.0) + "%)\n\n[TOUCH AND HOLD HERE]\nto perform a gesture\n";
            // Set recording_gesture to -1 to indicate normal operation (learning finished).
            recording_gesture = -1;
        }
        // If recording_gesture is -2, that means that the AI is currently learning a new gesture.
        if (recording_gesture == -2) {
            // Show "please wait" message
            HUDText.text = "[TOUCH HERE]\nto stop training\n\n\n\n\n\n\n...training...\n\n(" + (last_performance_report * 100.0) + " %)\n\n\n\n\n";
            if (button_id == ButtonID_Train && button_state == ButtonState_Released) {
                // Button pressed: stop the learning process.
                gr.stopTraining();
            }
            return;
		}
        // Else: if we arrive here, we're not in training/learning mode,
        // so the user can draw gestures.
        

        // If recording_gesture is -1, we're currently not recording a new gesture.
        if (recording_gesture == -1) {
            if (button_id == ButtonID_Train && button_state == ButtonState_Released) {
                string random_word = getRandomWord();
                recording_gesture = gr.createGesture(random_word);
                // from now on: recording a new gesture
                HUDText.text = "Learning a new gesture.\nKeyword:\n'" + random_word + "'\n\n[TOUCH AND HOLD HERE]\nto record gesture sample\n\n\n";
                return;
            }
        }

        // If the user is not yet dragging (pressing the trigger) on either controller, he hasn't started a gesture yet.
        if (button_id == ButtonID_Record) {
            if (!making_stroke) {
                // If we arrive here: either trigger was pressed, so we start the gesture.
                Vector3 hmd_p = new Vector3(0.0f,0.0f,0.0f);
                Quaternion hmd_q = new Quaternion(0.0f,0.0f,0.0f,1.0f);
                gr.startStroke(hmd_p, hmd_q, recording_gesture);
                making_stroke = true;
                RenderSettings.skybox.SetColor("_Tint", new Color(0.53f, 0.17f, 0.17f, 1.0f));
                HUDText.text = "Hold and move phone\nto make a gesture.\n\n\n\n";
            }
            // the user is dragging with the controller: continue the gesture.

            // Get phone position / motion:

            Vector3 p = Input.gyro.userAcceleration;

            // We could also sample over all recent acceleration events as a 
            /*
            Vector3 p = new Vector3(0.0f, 0.0f, 0.0f);
            if (Input.accelerationEventCount > 1)
            {
                foreach (AccelerationEvent acc_event in Input.accelerationEvents)
                {
                    p += acc_event.acceleration * acc_event.deltaTime;
                }
            } else
            {
                p = Input.acceleration;
            }
            */
            // We use the 
            //Vector3 p = Input.gyro.gravity;

            // Get phone rotation / orientation:

            // When using Input.gyro.attitude, the compass reading is included in the phone's orientation.
            // That means that a gesture performed northward can be a different gesture from the
            // same motion performed southwards. Usually, this is not what people expect,
            // so we're using "gravity" instead to detect the phone's orientation. 
            // Quaternion q = Input.gyro.attitude;

            // As an alternative, we can calculate the phone's orientation from the gravity ("down" vector).
            Quaternion q = Quaternion.FromToRotation(new Vector3(0, 1, 0), Input.gyro.gravity);

            // Or we can use the rotational acceleration directly as a pseudo orientation.
            // Quaternion q = Quaternion.FromToRotation(new Vector3(1, 0, 0), Input.gyro.rotationRateUnbiased);

            HUDText.text = "acc=\n" + Input.gyro.userAcceleration.x.ToString("0.00") + " " + Input.gyro.userAcceleration.y.ToString("0.00") + " " + Input.gyro.userAcceleration.z.ToString("0.00") + "\n"
                         + "grav=\n" + Input.gyro.gravity.x.ToString("0.00") + " " + Input.gyro.gravity.y.ToString("0.00") + " " + Input.gyro.gravity.z.ToString("0.00");

            gr.contdStrokeQ(p, q);
            return;
        }

        if (making_stroke && button_id == ButtonID_None)
        {
            double similarity = 0; // This will receive a value of how similar the performed gesture was to previous recordings.
            Vector3 pos = Vector3.zero; // This will receive the position where the gesture was performed.
            double scale = 0; // This will receive the scale at which the gesture was performed.
            Vector3 dir0 = Vector3.zero; // This will receive the primary direction in which the gesture was performed (greatest expansion).
            Vector3 dir1 = Vector3.zero; // This will receive the secondary direction of the gesture.
            Vector3 dir2 = Vector3.zero; // This will receive the minor direction of the gesture (direction of smallest expansion).
            int gesture_id = gr.endStroke(ref similarity, ref pos, ref scale, ref dir0, ref dir1, ref dir2);
            RenderSettings.skybox.SetColor("_Tint", new Color(0.5f, 0.5f, 0.5f, 1.0f));
            if (recording_gesture >= 0 )
            {
                int num_samples = gr.getGestureNumberOfSamples(recording_gesture);
                string gesture_name = gr.getGestureName(recording_gesture);
                HUDText.text = "[TOUCH HERE]\nto stop recording samples\nand start training the AI\n\n\n\n\n[TOUCH AND HOLD HERE]\nto record gesture sample.\n" + num_samples
                             + " samples recorded\n(record at least 20)\n\nGesture keyword:\n" + gesture_name + "\n";
            }
            else
            {
                string gesture_name = gr.getGestureName(gesture_id);
                HUDText.text = "[TOUCH HERE]\nto record a new gesture\n\n\n\n\n\n\n[TOUCH AND HOLD HERE]\nto perform a gesture\n\n\n identified gesture: \n " + gesture_name + "\n\n\n[TOUCH HERE TO EXIT]";
            }
            making_stroke = false;
            return;
        }
        
        // 
        if (button_id == ButtonID_Train && button_state == ButtonState_Released) {
            // Currently recording samples for a custom gesture - check how many we have recorded so far.
            // Enough samples recorded. Start the learning process.
            HUDText.text = "Learning gestures...";
            // Set up the call-backs to receive information about the learning process.
            gr.setTrainingUpdateCallback(trainingUpdateCallback);
            gr.setTrainingUpdateCallbackMetadata((IntPtr)me);
            gr.setTrainingFinishCallback(trainingFinishCallback);
            gr.setTrainingFinishCallbackMetadata((IntPtr)me);
            gr.setMaxTrainingTime(20);
            // Set recording_gesture to -2 to indicate that we're currently in learning mode.
            recording_gesture = -2;
            int ret = gr.startTraining();
            if (ret != 0) {
                HUDText.text = "Failed to start training:\n" + GestureRecognition.getErrorMessage(ret);
            }
            return;
        }
    }

    // Callback function to be called by the gesture recognition plug-in during the learning process.
    [MonoPInvokeCallback(typeof(GestureRecognition.TrainingCallbackFunction))]
    public static void trainingUpdateCallback(double performance, IntPtr ptr)
    {
        // Get the script/scene object back from metadata.
        GCHandle obj = (GCHandle)ptr;
        Sample_Phone me = (obj.Target as Sample_Phone);
        // Update the performance indicator with the latest estimate.
        me.last_performance_report = performance;
    }
    

    // Callback function to be called by the gesture recognition plug-in when the learning process was finished.
    [MonoPInvokeCallback(typeof(GestureRecognition.TrainingCallbackFunction))]
    public static void trainingFinishCallback(double performance, IntPtr ptr)
    {
        // Get the script/scene object back from metadata.
        GCHandle obj = (GCHandle)ptr;
        Sample_Phone me = (obj.Target as Sample_Phone);
        // Update the performance indicator with the latest estimate.
        me.last_performance_report = performance;
        // Signal that training was finished.
        me.recording_gesture = -3;
    }
}
}
