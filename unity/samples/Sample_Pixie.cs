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
public class Sample_Pixie : MonoBehaviour
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
    public static Text HUDText;

    // The game object associated with the currently active controller (if any):
    private GameObject active_controller = null;

    // The pointing tip of the active_controller (used for visualization).
    private GameObject active_controller_pointer = null;

    // Gesture ID for "come here" gesture.
    private const int gestureid_come = 0;
    
    // Gesture ID for "go there" gesture.
    private const int gestureid_go = 1;
    
    // Gesture ID for "spin around" gesture.
    private const int gestureid_spin = 2;
    
    // Gesture ID for "make a flip (loop)" gesture.
    private const int gestureid_flip = 3;
    
    // Gesture ID for "peekaboo" gesture.
    private const int gestureid_peekaboo = 4;
    

    // Last reported recognition performance (during training).
    // 0 = 0% correctly recognized, 1 = 100% correctly recognized.
    private double last_performance_report = 0; 

    // Temporary storage for objects to display the gesture stroke.
    List<string> stroke = new List<string>(); 

    // Temporary counter variable when creating objects for the stroke display:
    int stroke_index = 0; 

    // List of Objects created with gestures:
    List<GameObject> created_objects = new List<GameObject>();

    // Handle to this object/script instance, so that callbacks from the plug-in arrive at the correct instance.
    GCHandle me;
    
    public class Pixie
    {
        public GameObject gameobject = null;
        public Animator animator = null;
        
        public bool action_finished = true;
        
        public Vector3 target_position;
        public Vector3 target_orientation;
        public string target_trigger = null;
        
        float speed = 0.0f;
        
        public Pixie()
        {
            gameobject = GameObject.Find("pixie");
            animator = gameobject.GetComponent<Animator>();
            target_position = gameobject.transform.position;
            target_orientation = -gameobject.transform.position; // facing the room center / player
        }
        
        public void update()
        {
            Vector3 current_position = gameobject.transform.position;
            Quaternion current_rotation = gameobject.transform.rotation;
            Quaternion target_rotation = new Quaternion(0, 0, 0, 0);
            if (Vector3.Distance(target_position, current_position) > 0.001f)
            {
                if (speed < 1.5f)
                {
                    speed += 0.15f;
                }
                float step =  speed * Time.deltaTime;
                target_rotation.SetLookRotation(target_position - gameobject.transform.position);
                gameobject.transform.position = Vector3.MoveTowards(gameobject.transform.position, target_position, step);
                gameobject.transform.rotation = Quaternion.RotateTowards(gameobject.transform.rotation, target_rotation, step * 100.0f);
                return;
            } else {
                speed = 0.0f;
            }
            target_rotation.SetLookRotation(target_orientation);
            if (Quaternion.Angle(gameobject.transform.rotation, target_rotation) > 1.0f)
            {
                float step = 200.0f * Time.deltaTime;
                gameobject.transform.rotation = Quaternion.RotateTowards(gameobject.transform.rotation, target_rotation, step);
                return;
            }
            if (target_trigger != null)
            {
                animator.SetTrigger(target_trigger);
                target_trigger = null;
                return;
            }
            if (animator.IsInTransition(0))
            {
                return;
            }
            this.action_finished = animator.GetCurrentAnimatorStateInfo(0).IsName("Idle");
        }
        public void triggerCome(Vector3 pos)
        {
            this.target_position = pos;
            // afterwards, face the player again
            this.target_orientation = Camera.main.gameObject.transform.position - pos;
            this.action_finished = false;
        }
        public void triggerGo(Vector3 pos)
        {
            // limit range of motion
            pos = new Vector3(
                Math.Sign(pos.x) * Math.Min(Math.Abs(pos.x), 3.5f),
                Math.Max(0.1f, Math.Min(pos.y, 5.0f)),
                Math.Sign(pos.z) * Math.Min(Math.Abs(pos.z), 3.5f)
            );
            this.target_position = pos;
            // afterwards, face the player again
            this.target_orientation = Camera.main.gameObject.transform.position - pos;
            this.action_finished = false;
        }
        public void triggerSpin(Vector3 pos)
        {
            this.target_position = pos;
            this.target_orientation = Camera.main.gameObject.transform.position - pos;
            this.target_trigger = "DoSpin";
            this.action_finished = false;
        }
        public void triggerFlip(Vector3 pos, Vector3 dir)
        {
            this.target_position = pos;
            this.target_orientation = dir;
            this.target_trigger = "DoFlip";
            this.action_finished = false;
        }
        public void triggerPeekaboo()
        {
            this.target_trigger = "DoPeekaboo";
            this.action_finished = false;
        }
    }
    
    private Pixie pixie = null;
    
    public abstract class Step
    {
        protected double similarity = 0; // This will receive a value of how similar the performed gesture was to previous recordings.
        protected Vector3 pos = Vector3.zero; // This will receive the position where the gesture was performed.
        protected double scale = 0; // This will receive the scale at which the gesture was performed.
        protected Vector3 dir0 = Vector3.zero; // This will receive the primary direction in which the gesture was performed (greatest expansion).
        protected Vector3 dir1 = Vector3.zero; // This will receive the secondary direction of the gesture.
        protected Vector3 dir2 = Vector3.zero; // This will receive the minor direction of the gesture (direction of smallest expansion).
            
        public bool completed = false;
        public abstract void init(ref GestureRecognition gr);
        public virtual void dragStart(ref GestureRecognition gr, Vector3 hmd_p, Quaternion hmd_q)
        {
            gr.startStroke(hmd_p, hmd_q);
        }
        public virtual void dragContd(ref GestureRecognition gr, Vector3 hmd_p, Quaternion hmd_q, Vector3 controller_p, Quaternion controller_q)
        {
            gr.updateHeadPosition(hmd_p, hmd_q);
            gr.contdStrokeQ(controller_p, controller_q);
        }
        public abstract void dragStop(ref GestureRecognition gr, ref Pixie pixie);
        public abstract Step nextStep();
    }

    //                                                                              ________________________________
    // ____________________________________________________________________________/     Step0_PressTrigger
    public class Step0_PressTrigger : Step
    {
        public override void init(ref GestureRecognition gr)
        {
            this.completed = false;
            Sample_Pixie.HUDText.text = "Welcome to MiVRy\nPress the trigger button on your controller to start";
        }
        public override void dragStart(ref GestureRecognition gr, Vector3 hmd_p, Quaternion hmd_q)
        {
            // nothing to do
        }
        public override void dragContd(ref GestureRecognition gr, Vector3 hmd_p, Quaternion hmd_q, Vector3 controller_p, Quaternion controller_q)
        {
            // nothing to do
        }
        public override void dragStop(ref GestureRecognition gr, ref Pixie pixie)
        {
            this.completed = true;
            Sample_Pixie.HUDText.text = "";
            // reparent the text to be static in front
            Transform hud_text_transform = GameObject.Find("Canvas").transform;
            hud_text_transform.SetParent(GameObject.Find("XR Rig").transform, true);
            hud_text_transform.rotation.SetLookRotation(Camera.main.gameObject.transform.position - hud_text_transform.position);
        }
        public override Step nextStep()
        {
            return new Step1_ComeHere();
        }
    }
    
    //                                                                              ________________________________
    // ____________________________________________________________________________/        Step1_ComeHere
    public class Step1_ComeHere : Step
    {
        public override void init(ref GestureRecognition gr)
        {
            this.completed = false;
            Sample_Pixie.HUDText.text = "Make a 'come here' gesture\nto call the pixie close to you.";
        }
        public override void dragStop(ref GestureRecognition gr, ref Pixie pixie)
        {
            int gesture_id = gr.endStroke(ref similarity, ref pos, ref scale, ref dir0, ref dir1, ref dir2);
            if (gesture_id == gestureid_come) {
                pixie.triggerCome(pos);
                this.completed = true;
                Sample_Pixie.HUDText.text = "";
            }
        }
        public override Step nextStep()
        {
            return new Step2_GoThere();
        }
    }
    
    
    //                                                                              ________________________________
    // ____________________________________________________________________________/        Step2_GoThere
    public class Step2_GoThere : Step
    {
        public override void init(ref GestureRecognition gr)
        {
            this.completed = false;
            Sample_Pixie.HUDText.text = "Nice!\n Now make a throwing gesture\nto send the pixie away";
        }
        public override void dragStop(ref GestureRecognition gr, ref Pixie pixie)
        {
            int gesture_id = gr.endStroke(ref similarity, ref pos, ref scale, ref dir0, ref dir1, ref dir2);
            if (gesture_id == gestureid_go)
            {
                pixie.triggerGo(pos + (dir0 * 6.0f * (float)scale));
                this.completed = true;
                Sample_Pixie.HUDText.text = "";
            }
        }
        public override Step nextStep()
        {
            return new Step3_ComeHereAndGoThere();
        }
    }
    
    
    //                                                                              ________________________________
    // ____________________________________________________________________________/    Step3_ComeHereAndGoThere
    public class Step3_ComeHereAndGoThere : Step
    {
        private int num_commands_issued = 0;
        public override void init(ref GestureRecognition gr)
        {
            this.completed = false;
            Sample_Pixie.HUDText.text = "Good!\nTry calling and sending the pixie away a few times.";
            this.num_commands_issued = 0;
        }
        public override void dragStop(ref GestureRecognition gr, ref Pixie pixie)
        {
            int gesture_id = gr.endStroke(ref similarity, ref pos, ref scale, ref dir0, ref dir1, ref dir2);
            if (gesture_id == gestureid_go) {
                pixie.triggerGo(pos + (dir0 * 6.0f * (float)scale));
                this.num_commands_issued += 1;
                Sample_Pixie.HUDText.text = "That's a 'go there' gesture. Nice!\nTry calling and sending the pixie away a few more times.\n("+ this.num_commands_issued+"/4)";
            } else if (gesture_id == gestureid_come) {
                pixie.triggerCome(pos);
                this.num_commands_issued += 1;
                Sample_Pixie.HUDText.text = "That's a 'come here' gesture. Nice!\nTry calling and sending the pixie away a few more times.\n(" + this.num_commands_issued + "/4)";
            }
            if (this.num_commands_issued >= 4) {
                this.completed = true;
                Sample_Pixie.HUDText.text = "";
            }
        }
        public override Step nextStep()
        {
            return new Step4_Spin();
        }
    }
    
    
    //                                                                              ________________________________
    // ____________________________________________________________________________/    Step4_Spin
    public class Step4_Spin : Step
    {
        private int num_commands_issued = 0;
        
        public override void init(ref GestureRecognition gr)
        {
            this.completed = false;
            this.num_commands_issued = 0;
            Sample_Pixie.HUDText.text = "Make a twirl (whirling) gesture\nto make the pixie spin.";
        }
        public override void dragStop(ref GestureRecognition gr, ref Pixie pixie)
        {
            int gesture_id = gr.endStroke(ref similarity, ref pos, ref scale, ref dir0, ref dir1, ref dir2);
            if (gesture_id == gestureid_spin) {
                pixie.triggerSpin(pos);
                this.num_commands_issued += 1;
                Sample_Pixie.HUDText.text = "Great!\nTry it again!\nMake a twirl (whirling) gesture\nto make the pixie spin. ("+ this.num_commands_issued+"/3)";
            }
            if (this.num_commands_issued >= 3) {
                this.completed = true;
                Sample_Pixie.HUDText.text = "";
            }
        }
        public override Step nextStep()
        {
            return new Step5_FlipRecord();
        }
    }
    
    
    //                                                                              ________________________________
    // ____________________________________________________________________________/    Step5_FlipRecord
    public class Step5_FlipRecord : Step
    {
        private int recorded_samples = 0;
        
        public override void init(ref GestureRecognition gr)
        {
            this.completed = false;
            Sample_Pixie.HUDText.text = "Now teach your pixie something new!\nInvent a new gesture and do it 20 times.\n(0/20)";
            this.recorded_samples = 0;
            int record_gesture_id = gr.createGesture("Flip");
            // record_gesture_id should be gestureid_peekaboo
            if (record_gesture_id != gestureid_flip)
            {
                Sample_Pixie.HUDText.text = "[ERROR: FAILED TO CREATE NEW GESTURE]";
            }
        }
        public override void dragStart(ref GestureRecognition gr, Vector3 hmd_p, Quaternion hmd_q)
        {
            gr.startStroke(hmd_p, hmd_q, gestureid_flip);
        }
        public override void dragStop(ref GestureRecognition gr, ref Pixie pixie)
        {
            int gesture_id = gr.endStroke(ref similarity, ref pos, ref scale, ref dir0, ref dir1, ref dir2);
            if (gesture_id == gestureid_flip) {
                recorded_samples += 1;
                Sample_Pixie.HUDText.text = "Now teach your pixie something new!\nInvent a new gesture and do it 20 times.\n(" + recorded_samples + "/20)";
            }
            if (recorded_samples >= 20) {
                this.completed = true;
                gr.startTraining();
                Sample_Pixie.HUDText.text = "Please wait while your pixie is learning the new gesture...";
            }
        }
        public override Step nextStep()
        {
            return new Step6_FlipPerform();
        }
    }
    
    
    //                                                                              ________________________________
    // ____________________________________________________________________________/    Step6_FlipPerform
    public class Step6_FlipPerform : Step
    {
        private int num_commands_issued = 0;
        
        public override void init(ref GestureRecognition gr)
        {
            this.completed = false;
            this.num_commands_issued = 0;
            Sample_Pixie.HUDText.text = "Learning finished! Try out your new gesture.";
        }
        public override void dragStop(ref GestureRecognition gr, ref Pixie pixie)
        {
            int gesture_id = gr.endStroke(ref similarity, ref pos, ref scale, ref dir0, ref dir1, ref dir2);
            if (gesture_id == gestureid_flip)
            {
                pixie.triggerFlip(pos, dir0);
                this.num_commands_issued += 1;
                Sample_Pixie.HUDText.text = "That's your new gesture. Nice!\nTry it a few more times.\n(" + this.num_commands_issued + "/4)";
            }
            else if (gesture_id == gestureid_spin)
            {
                pixie.triggerSpin(pos);
                Sample_Pixie.HUDText.text = "That's a 'spin' gesture. Nice!\nTry a few more times.\n(" + this.num_commands_issued + "/4)";
            }
            else if (gesture_id == gestureid_go)
            {
                pixie.triggerGo(pos + (dir0 * 6.0f * (float)scale));
                Sample_Pixie.HUDText.text = "That's a 'go there' gesture. Nice!\nTry a few more times.\n(" + this.num_commands_issued + "/4)";
            }
            else if (gesture_id == gestureid_come)
            {
                pixie.triggerCome(pos);
                Sample_Pixie.HUDText.text = "That's a 'come here' gesture. Nice!\nTry a few more times.\n(" + this.num_commands_issued + "/4)";
            }
            if (this.num_commands_issued >= 4) {
                this.completed = true;
                Sample_Pixie.HUDText.text = "";
            }
        }
        public override Step nextStep()
        {
            return new Step7_PeekabooRecord();
        }
    }
    
    
    //                                                                              ________________________________
    // ____________________________________________________________________________/    Step7_PeekabooRecord
    public class Step7_PeekabooRecord : Step
    {
        private int recorded_samples = 0;
        
        public override void init(ref GestureRecognition gr)
        {
            this.completed = false;
            Sample_Pixie.HUDText.text = "Let's try it again!\nInvent a new gesture and do it 20 times.\n(0/20)";
            int record_gesture_id = gr.createGesture("Peekaboo");
            // record_gesture_id should be gestureid_peekaboo
            if (record_gesture_id != gestureid_peekaboo)
            {
                Sample_Pixie.HUDText.text = "[ERROR: FAILED TO CREATE NEW GESTURE]";
            }
            this.recorded_samples = 0;
        }
        public override void dragStart(ref GestureRecognition gr, Vector3 hmd_p, Quaternion hmd_q)
        {
            gr.startStroke(hmd_p, hmd_q, gestureid_peekaboo);
        }
        public override void dragStop(ref GestureRecognition gr, ref Pixie pixie)
        {
            int gesture_id = gr.endStroke(ref similarity, ref pos, ref scale, ref dir0, ref dir1, ref dir2);
            if (gesture_id == gestureid_peekaboo) {
                recorded_samples += 1;
                Sample_Pixie.HUDText.text = "Let's try it again!\nInvent a new gesture and do it 20 times.\n(" + recorded_samples + "/20)";
            }
            if (recorded_samples >= 20) {
                this.completed = true;
                gr.startTraining();
                Sample_Pixie.HUDText.text = "Please wait while your pixie is learning the new gesture...";
            }
        }
        public override Step nextStep()
        {
            return new Step8_FreePlay();
        }
    }
    
    
    //                                                                              ________________________________
    // ____________________________________________________________________________/    Step8_FreePlay
    public class Step8_FreePlay : Step
    {
        public override void init(ref GestureRecognition gr)
        {
            this.completed = false;
            Sample_Pixie.HUDText.text = "Learning finished! Try out any of the gestures.";
        }
        public override void dragStop(ref GestureRecognition gr, ref Pixie pixie)
        {
            int gesture_id = gr.endStroke(ref similarity, ref pos, ref scale, ref dir0, ref dir1, ref dir2);
            if (gesture_id == gestureid_peekaboo)
            {
                pixie.triggerPeekaboo();
                Sample_Pixie.HUDText.text = "That's your new gesture. Awesome!\nFeel try to try out any gesture.";
            }
            else if (gesture_id == gestureid_flip)
            {
                pixie.triggerFlip(pos, dir0);
                Sample_Pixie.HUDText.text = "That's your previous gesture. Cool!\nFeel try to try out any gesture.";
            }
            else if (gesture_id == gestureid_spin)
            {
                pixie.triggerSpin(pos);
                Sample_Pixie.HUDText.text = "That's a 'spin' gesture. Nice!\nFeel try to try out any gesture.";
            }
            else if (gesture_id == gestureid_go)
            {
                pixie.triggerGo(pos + (dir0 * 6.0f * (float)scale));
                Sample_Pixie.HUDText.text = "That's a 'go there' gesture. Splended!\nMake it larger to send the pixie further away.";
            }
            else if (gesture_id == gestureid_come)
            {
                pixie.triggerCome(pos);
                Sample_Pixie.HUDText.text = "That's a 'come here' gesture. Not bad!\nFeel try to try out any gesture.";
            }
        }
        public override Step nextStep()
        {
            // this step never ends
            return new Step8_FreePlay();
        }
    }
    
    private Step current_step = null;

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
        pixie = new Pixie();
        
        // Set the welcome message.
        HUDText = GameObject.Find("HUDText").GetComponent<Text>();
        current_step = new Step0_PressTrigger();
        current_step.init(ref gr);

        me = GCHandle.Alloc(this);

        if (this.LicenseName != null && this.LicenseKey != null && this.LicenseName.Length > 0)
        {
            if (this.gr.activateLicense(this.LicenseName, this.LicenseKey) != 0)
            {
                Debug.LogError("Failed to activate license");
            }
        }

        const string LoadGesturesFile = "Samples/Sample_Pixie_Gestures.dat";
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
            HUDText.text = "Failed to extract sample gesture database file from apk.\n";
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
    }
    

    // Update:
    void Update()
    {
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

        // Don't allow the player to make any gestures when the pixie is still
        // busy with the previous command.
        pixie.update();
        if (!pixie.action_finished)
        {
            return;
        }

        // If we're currently learning a new gesture, don't allow new input.
        if (gr.isTraining())
        {
            return;
        }
        
        // bool button_a_left = Input.GetButton("LeftControllerButtonA");
        // bool button_a_right = Input.GetButton("RightControllerButtonA");
        
        if (current_step == null)
        {
            return;
        }
        if (current_step.completed)
        {
            current_step = current_step.nextStep();
            current_step.init(ref this.gr);
        }

#if ENABLE_INPUT_SYSTEM
        float trigger_left = Sample_Utils.getInputControlValue("<XRController>{LeftHand}/trigger");
        float trigger_right = Sample_Utils.getInputControlValue("<XRController>{RightHand}/trigger");
#else
        float trigger_left = Input.GetAxis("LeftControllerTrigger");
        float trigger_right = Input.GetAxis("RightControllerTrigger");
#endif

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
            current_step.dragStart(ref gr, hmd_p, hmd_q);
        }

        // If we arrive here, the user is currently dragging with one of the controllers.
        Vector3 p = active_controller.transform.position;
        Quaternion q = active_controller.transform.rotation;
        current_step.dragContd(ref gr, hmd_p, hmd_q, p, q);
        // Show the stroke by instatiating new objects
        {
            p = active_controller_pointer.transform.position;
            GameObject star_instance = Instantiate(GameObject.Find("star"));
            GameObject star = new GameObject("stroke_" + stroke_index++);
            star_instance.name = star.name + "_instance";
            star_instance.transform.SetParent(star.transform, false);
            System.Random random = new System.Random();
            star.transform.position = new Vector3(p.x + (float)random.NextDouble() / 80, p.y + (float)random.NextDouble() / 80, p.z + (float)random.NextDouble() / 80);
            star.transform.rotation = new Quaternion((float)random.NextDouble() - 0.5f, (float)random.NextDouble() - 0.5f, (float)random.NextDouble() - 0.5f, (float)random.NextDouble() - 0.5f).normalized;
            //star.transform.rotation.Normalize();
            float star_scale = (float)random.NextDouble() + 0.3f;
            star.transform.localScale    = new Vector3(star_scale, star_scale, star_scale);
            stroke.Add(star.name);
        }
        
        // Check if the user is still dragging or if he let go of the trigger button.
        if (trigger_left < 0.85 && trigger_right < 0.85) {
            // the user let go of the trigger, ending a gesture.
            active_controller = null;

            // Delete the objectes that we used to display the gesture.
            foreach (string star in stroke) {
                GameObject star_object = GameObject.Find(star);
                if (star_object != null) {
                    Destroy(star_object);
                }
            }
            stroke.Clear();
            stroke_index = 0;

            current_step.dragStop(ref gr, ref this.pixie);
        }
    }

    // Callback function to be called by the gesture recognition plug-in during the learning process.
    [MonoPInvokeCallback(typeof(GestureRecognition.TrainingCallbackFunction))]
    public static void trainingUpdateCallback(double performance, IntPtr ptr)
    {
        // Get the script/scene object back from metadata.
        GCHandle obj = (GCHandle)ptr;
        Sample_Pixie me = (obj.Target as Sample_Pixie);
        // Update the performance indicator with the latest estimate.
        me.last_performance_report = performance;
    }
    

    // Callback function to be called by the gesture recognition plug-in when the learning process was finished.
    [MonoPInvokeCallback(typeof(GestureRecognition.TrainingCallbackFunction))]
    public static void trainingFinishCallback(double performance, IntPtr ptr)
    {
        // Get the script/scene object back from metadata.
        GCHandle obj = (GCHandle)ptr;
        Sample_Pixie me = (obj.Target as Sample_Pixie);
        // Update the performance indicator with the latest estimate.
        me.last_performance_report = performance;
    }
}
}
