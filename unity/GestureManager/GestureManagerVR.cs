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

using System;
using UnityEngine;

public class GestureManagerVR : MonoBehaviour
{
    public static GestureManagerVR me; // singleton

    public bool followUser = true;

    public GestureManager gestureManager;

    public EditableTextField inputFocus = null;
    public Material inputFocusOnMaterial;
    public Material inputFocusOffMaterial;
    public GameObject keyboard;
    public GameObject splashscreen;

    public GameObject submenuNumberOfParts = null;
    public GameObject submenuFiles = null;
    public GameObject submenuFileSuggestions = null;
    public GameObject submenuGesture = null;
    public GameObject submenuCombination = null;
    public GameObject submenuRecord = null;
    public GameObject submenuCoordinateSystem = null;
    public GameObject submenuFrameOfReference = null;
    public GameObject submenuTraining = null;

    public static GestureManagerButton activeButton = null;

    public class SampleDisplay
    {
        public int sampleId = -1;
        public int dataPointIndex = 0;
        public GameObject headsetModel = null;
        public GameObject controllerModelLeft = null;
        public GameObject controllerModelRight = null;
        public struct Stroke
        {
            public Vector3[] hmd_p;
            public Quaternion[] hmd_q;
            public Vector3[] p;
            public Quaternion[] q;
        };
        public Stroke strokeLeft;
        public Stroke strokeRight;
        public void reloadStrokes()
        {
            int ret;
            if (this.sampleId < 0)
            {
                strokeRight.p = null;
                strokeRight.q = null;
                strokeRight.hmd_p = null;
                strokeRight.hmd_q = null;
                strokeLeft.p = null;
                strokeLeft.q = null;
                strokeLeft.hmd_p = null;
                strokeLeft.hmd_q = null;
                return;
            }
            var gm = GestureManagerVR.me.gestureManager;
            if (gm.gr != null)
            {
                int numSamples = gm.gr.getGestureNumberOfSamples(gm.record_gesture_id);
                if (this.sampleId >= numSamples)
                {
                    this.sampleId = numSamples - 1;
                }
                if (numSamples >= 0)
                {
                    ret = gm.gr.getGestureSampleStroke(gm.record_gesture_id, this.sampleId, 0,
                        ref this.strokeRight.p, ref this.strokeRight.q, ref this.strokeRight.hmd_p, ref this.strokeRight.hmd_q
                    );
                    if (ret <= 0)
                    {
                        gm.consoleText = $"Failed to get sample data ({ret}).";
                        return;
                    }
                    for (int i=0; i < this.strokeRight.p.Length; i++) {
                        Mivry.convertBackHandInput(gm.mivryCoordinateSystem, gm.unityXrPlugin, ref this.strokeRight.p[i], ref this.strokeRight.q[i]);
                        Mivry.convertBackHeadInput(gm.mivryCoordinateSystem, ref this.strokeRight.hmd_p[i], ref this.strokeRight.hmd_q[i]);
                    }
                } else
                {
                    strokeRight.p = null;
                    strokeRight.q = null;
                    strokeRight.hmd_p = null;
                    strokeRight.hmd_q = null;
                }
                strokeLeft.p = null;
                strokeLeft.q = null;
                strokeLeft.hmd_p = null;
                strokeLeft.hmd_q = null;
            } else if (gm.gc != null)
            {
                // Right
                int gestureIdRight = gm.gc.getCombinationPartGesture(gm.record_combination_id, gm.righthand_combination_part);
                if (gestureIdRight < 0 || gestureIdRight >= gm.gc.numberOfGestures(gm.righthand_combination_part))
                {
                    strokeRight.p = null;
                    strokeRight.q = null;
                    strokeRight.hmd_p = null;
                    strokeRight.hmd_q = null;
                } else
                {
                    int sampleIdRight = Math.Min(this.sampleId, gm.gc.getGestureNumberOfSamples(gm.righthand_combination_part, gestureIdRight) - 1);
                    if (sampleIdRight < 0)
                    {
                        strokeRight.p = null;
                        strokeRight.q = null;
                        strokeRight.hmd_p = null;
                        strokeRight.hmd_q = null;
                    } else
                    {
                        ret = gm.gc.getGestureSampleStroke(gm.righthand_combination_part, gestureIdRight, sampleIdRight, 0,
                            ref this.strokeRight.p, ref this.strokeRight.q, ref this.strokeRight.hmd_p, ref this.strokeRight.hmd_q
                        );
                        if (ret <= 0)
                        {
                            gm.consoleText = $"Failed to get sample data ({ret}).";
                            return;
                        }
                        for (int i = 0; i < this.strokeRight.p.Length; i++)
                        {
                            Mivry.convertBackHandInput(gm.mivryCoordinateSystem, gm.unityXrPlugin, ref this.strokeRight.p[i], ref this.strokeRight.q[i]);
                            Mivry.convertBackHeadInput(gm.mivryCoordinateSystem, ref this.strokeRight.hmd_p[i], ref this.strokeRight.hmd_q[i]);
                        }
                    }
                }
                // Left
                int gestureIdLeft = gm.gc.getCombinationPartGesture(gm.record_combination_id, gm.lefthand_combination_part);
                if (gestureIdLeft < 0 || gestureIdLeft >= gm.gc.numberOfGestures(gm.lefthand_combination_part))
                {
                    strokeLeft.p = null;
                    strokeLeft.q = null;
                    strokeLeft.hmd_p = null;
                    strokeLeft.hmd_q = null;
                }
                else
                {
                    int sampleIdLeft = Math.Min(this.sampleId, gm.gc.getGestureNumberOfSamples(gm.lefthand_combination_part, gestureIdLeft) - 1);
                    if (sampleIdLeft < 0)
                    {
                        strokeLeft.p = null;
                        strokeLeft.q = null;
                        strokeLeft.hmd_p = null;
                        strokeLeft.hmd_q = null;
                    }
                    else
                    {
                        ret = gm.gc.getGestureSampleStroke(gm.lefthand_combination_part, gestureIdLeft, sampleIdLeft, 0,
                            ref this.strokeLeft.p, ref this.strokeLeft.q, ref this.strokeLeft.hmd_p, ref this.strokeLeft.hmd_q
                        );
                        if (ret <= 0)
                        {
                            gm.consoleText = $"Failed to get sample data ({ret}).";
                            return;
                        }
                        for (int i = 0; i < this.strokeLeft.p.Length; i++)
                        {
                            Mivry.convertBackHandInput(gm.mivryCoordinateSystem, gm.unityXrPlugin, ref this.strokeLeft.p[i], ref this.strokeLeft.q[i]);
                            Mivry.convertBackHeadInput(gm.mivryCoordinateSystem, ref this.strokeLeft.hmd_p[i], ref this.strokeLeft.hmd_q[i]);
                        }
                    }
                }
            }
        }
    };
    public static SampleDisplay sampleDisplay = new SampleDisplay();

    // Start is called before the first frame update
    void Start()
    {
        me = this;
        inputFocus = null;
        keyboard?.SetActive(false);

        for (int i=0; i<this.transform.childCount; i++)
        {
            GameObject child = this.transform.GetChild(i).gameObject;
            switch (child.name)
            {
                case "SubmenuNumberOfParts":
                    submenuNumberOfParts = child;
                    break;
                case "SubmenuCoordinateSystem":
                    submenuCoordinateSystem = child;
                    break;
                case "SubmenuFiles":
                    submenuFiles = child;
                    break;
                case "SubmenuFileSuggestions":
                    submenuFileSuggestions = child;
                    break;
                case "SubmenuGesture":
                    submenuGesture = child;
                    break;
                case "SubmenuCombination":
                    submenuCombination = child;
                    break;
                case "SubmenuRecord":
                    submenuRecord = child;
                    break;
                case "SubmenuFrameOfReference":
                    submenuFrameOfReference = child;
                    break;
                case "SubmenuTraining":
                    submenuTraining = child;
                    break;
                case "SampleDisplayHeadset":
                    sampleDisplay.headsetModel = child;
                    break;
            }
            for (int k=0; k<child.transform.childCount; k++)
            {
                GameObject grandChild = child.transform.GetChild(k).gameObject;
                EditableTextField editableTextField = grandChild.GetComponent<EditableTextField>();
                if (editableTextField != null)
                    editableTextField.refreshText();
            }
        }
        refresh();
    }

    public static void keyboardInput(KeyboardKey key)
    {
        if (me == null || me.inputFocus == null)
            return;
        me.inputFocus.keyboardInput(key);
        if (me.inputFocus.target == EditableTextField.Target.LoadFile)
        {
            me.submenuFileSuggestions.GetComponent<SubmenuFileSuggestions>().refresh();
        }
    }

    public static void setInputFocus(EditableTextField editableTextField)
    {
        if (me == null)
            return;
        if (me.inputFocus != null)
        {
            MeshRenderer meshRenderer = me.inputFocus.gameObject?.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.material = me.inputFocusOffMaterial;
            }
        }
        me.inputFocus = editableTextField;
        if (me.inputFocus != null)
        {
            MeshRenderer meshRenderer = me.inputFocus.gameObject?.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.material = me.inputFocusOnMaterial;
            }
        }
        if (me.keyboard != null)
        {
            if (me.inputFocus == null)
            {
                me.keyboard.SetActive(false);
            } else
            {
                me.keyboard.SetActive(true);
                if (GestureManagerHandle.draggingHandle == null || GestureManagerHandle.draggingHandle.target != GestureManagerHandle.Target.Keyboard) {
                    if ((me.inputFocus.gameObject.transform.position - me.keyboard.transform.position).magnitude > 0.4) {
                        me.keyboard.transform.rotation = Quaternion.LookRotation(Camera.main.transform.position - me.inputFocus.gameObject.transform.position) * Quaternion.AngleAxis(90.0f, Vector3.right);

                        Bounds objBounds = new Bounds(me.inputFocus.gameObject.transform.position, Vector3.zero);
                        foreach (Renderer r in me.inputFocus.gameObject.GetComponentsInChildren<Renderer>())
                        {
                            objBounds.Encapsulate(r.bounds);
                        }
                        Bounds keyboardBounds = new Bounds(me.keyboard.transform.position, Vector3.zero);
                        foreach (Renderer r in me.keyboard.GetComponentsInChildren<Renderer>())
                        {
                            keyboardBounds.Encapsulate(r.bounds);
                        }
                        me.keyboard.transform.position = new Vector3(
                            objBounds.center.x,
                            objBounds.center.y - (objBounds.extents.y + keyboardBounds.extents.y + 0.05f),
                            objBounds.center.z
                        ) + (me.keyboard.transform.up * 0.1f);
                    }
                }
            }
        }
    }

    public static void refresh()
    {
        if (me == null)
            return;
        
        if (me.gestureManager.numberOfParts <= 0)
        {
            me.submenuNumberOfParts.SetActive(true);
            me.submenuFiles.SetActive(false);
            me.submenuFileSuggestions.SetActive(false);
            me.submenuGesture.SetActive(false);
            me.submenuCombination.SetActive(false);
            me.submenuRecord.SetActive(false);
            me.submenuCoordinateSystem.SetActive(false);
            me.submenuCoordinateSystem.GetComponent<SubmenuCoordinateSystem>().refresh();
            me.submenuFrameOfReference.SetActive(false);
            me.submenuTraining.SetActive(false);
        } else if (me.gestureManager.numberOfParts == 1)
        {
            me.submenuNumberOfParts.SetActive(true);
            me.submenuFiles.SetActive(true);
            me.submenuFiles.GetComponent<SubmenuFiles>().refresh();
            me.submenuFileSuggestions.SetActive(true);
            me.submenuFileSuggestions.GetComponent<SubmenuFileSuggestions>().refresh();
            me.submenuGesture.SetActive(true);
            me.submenuGesture.GetComponent<SubmenuGesture>().refresh();
            me.submenuCombination.SetActive(false);
            me.submenuRecord.SetActive(true);
            me.submenuRecord.GetComponent<SubmenuRecord>().refresh();
            me.submenuCoordinateSystem.SetActive(true);
            me.submenuCoordinateSystem.GetComponent<SubmenuCoordinateSystem>().refresh();
            me.submenuFrameOfReference.SetActive(true);
            me.submenuFrameOfReference.GetComponent<SubmenuFrameOfReference>().refresh();
            me.submenuTraining.SetActive(true);
            me.submenuTraining.GetComponent<SubmenuTraining>().refresh();
            me.submenuRecord.transform.localPosition = Vector3.forward * 0.135f;
            me.submenuCoordinateSystem.transform.localPosition = Vector3.forward * 0.135f;
            me.submenuFrameOfReference.transform.localPosition = Vector3.forward * 0.135f;
            me.submenuTraining.transform.localPosition = Vector3.forward * 0.135f;
        } else
        {
            me.submenuNumberOfParts.SetActive(true);
            me.submenuFiles.SetActive(true);
            me.submenuFiles.GetComponent<SubmenuFiles>().refresh();
            me.submenuFileSuggestions.SetActive(true);
            me.submenuFileSuggestions.GetComponent<SubmenuFileSuggestions>().refresh();
            me.submenuGesture.SetActive(true);
            me.submenuGesture.GetComponent<SubmenuGesture>().refresh();
            me.submenuCombination.SetActive(true);
            me.submenuCombination.GetComponent<SubmenuCombination>().refresh();
            me.submenuRecord.SetActive(true);
            me.submenuRecord.GetComponent<SubmenuRecord>().refresh();
            me.submenuCoordinateSystem.SetActive(true);
            me.submenuCoordinateSystem.GetComponent<SubmenuCoordinateSystem>().refresh();
            me.submenuFrameOfReference.SetActive(true);
            me.submenuFrameOfReference.GetComponent<SubmenuFrameOfReference>().refresh();
            me.submenuTraining.SetActive(true);
            me.submenuTraining.GetComponent<SubmenuTraining>().refresh();
            me.submenuRecord.transform.localPosition = Vector3.zero;
            me.submenuCoordinateSystem.transform.localPosition = Vector3.zero;
            me.submenuFrameOfReference.transform.localPosition = Vector3.zero;
            me.submenuTraining.transform.localPosition = Vector3.zero;
        }
        var editableTextFields = FindObjectsOfType<EditableTextField>();
        foreach (var editableTextField in editableTextFields)
        {
            if (editableTextField.gameObject.activeSelf)
            {
                editableTextField.refreshText();
            }
        }
    }

    public static void refreshTextInputs(GameObject go)
    {
        for (int i=0; i<go.transform.childCount; i++)
        {
            GameObject child = go.transform.GetChild(i).gameObject;
            EditableTextField field = child.GetComponent<EditableTextField>();
            if (field != null)
                field.refreshText();
        }
    }

    private void Update()
    {
        if (Camera.main == null || Camera.main.transform.position == Vector3.zero)
        {
            splashscreen?.SetActive(true);
            return;
        }
        if (splashscreen != null && splashscreen.activeSelf) splashscreen.SetActive(false);
        if (followUser)
        {
            Vector3 v = Camera.main.transform.worldToLocalMatrix.MultiplyPoint3x4(this.transform.position);
            if (v.magnitude > 0.6f || v.z < 0)
            {
                v = new Vector3(0, 0, 0.5f);
                v = Camera.main.transform.localToWorldMatrix.MultiplyPoint3x4(v);
                this.transform.position = 0.9f * this.transform.position + 0.1f * v;
                Vector3 lookDir = Camera.main.transform.position - this.transform.position;
                lookDir.y = 0; // not facing up or down
                this.transform.rotation = Quaternion.LookRotation(lookDir) * Quaternion.AngleAxis(180.0f, Vector3.up) * Quaternion.AngleAxis(-90.0f, Vector3.right);
            }
        }
        if (GestureManagerHandle.draggingHandle != null && Time.time - GestureManagerHandle.draggingHandleLastUpdate > 1)
        {
            GestureManagerHandle.draggingHandle = null;
        }
        if (GestureManagerHandle.hoverHandle != null && Time.time - GestureManagerHandle.hoverHandleLastUpdate > 1)
        {
            GestureManagerHandle.hoverHandle = null;
        }
        if (sampleDisplay.sampleId < 0)
        {
            if (sampleDisplay.controllerModelRight != null && sampleDisplay.controllerModelRight.activeSelf)
            {
                sampleDisplay.controllerModelRight.SetActive(false);
            }
            if (sampleDisplay.controllerModelLeft != null && sampleDisplay.controllerModelLeft.activeSelf)
            {
                sampleDisplay.controllerModelLeft.SetActive(false);
            }
            if (sampleDisplay.headsetModel != null && sampleDisplay.headsetModel.activeSelf)
            {
                sampleDisplay.headsetModel.SetActive(false);
            }
        } else
        {
            if (sampleDisplay.controllerModelRight == null)
            {
                var rightHand = GameObject.Find("Right Hand");
                sampleDisplay.controllerModelRight = UnityEngine.Object.Instantiate(rightHand, this.transform);
                sampleDisplay.controllerModelRight.name = "sampleDisplay.controllerModelRight";
                Destroy(sampleDisplay.controllerModelRight.GetComponent<UnityEngine.SpatialTracking.TrackedPoseDriver>());
                for (int i = sampleDisplay.controllerModelRight.transform.childCount - 1; i>=0; i--)
                {
                    var child = sampleDisplay.controllerModelRight.transform.GetChild(i);
                    
                    for (int j = child.childCount-1; j>=0; j--)
                    {
                        var grandChild = child.GetChild(j);
                        if (grandChild.gameObject.name.EndsWith("pointer"))
                        {
                            GameObject.Destroy(grandChild.gameObject);
                        } else
                        {
                            MeshRenderer meshRenderer = grandChild.gameObject.GetComponent<MeshRenderer>();
                            if (meshRenderer != null)
                                meshRenderer.material = sampleDisplay.headsetModel?.GetComponent<MeshRenderer>()?.material;
                        }
                    }
                }
            }
            if (sampleDisplay.controllerModelLeft == null)
            {
                var leftHand = GameObject.Find("Left Hand");
                sampleDisplay.controllerModelLeft = UnityEngine.Object.Instantiate(leftHand, this.transform);
                sampleDisplay.controllerModelLeft.name = "sampleDisplay.controllerModelLeft";
                Destroy(sampleDisplay.controllerModelLeft.GetComponent<UnityEngine.SpatialTracking.TrackedPoseDriver>());
                for (int i = sampleDisplay.controllerModelLeft.transform.childCount - 1; i >= 0; i--)
                {
                    var child = sampleDisplay.controllerModelLeft.transform.GetChild(i);
                    for (int j = child.childCount - 1; j >= 0; j--)
                    {
                        var grandChild = child.GetChild(j);
                        if (grandChild.gameObject.name.EndsWith("pointer"))
                        {
                            GameObject.Destroy(grandChild.gameObject);
                        } else
                        {
                            MeshRenderer meshRenderer = grandChild.gameObject.GetComponent<MeshRenderer>();
                            if (meshRenderer != null)
                                meshRenderer.material = sampleDisplay.headsetModel?.GetComponent<MeshRenderer>()?.material;
                        }
                    }
                }
            }

            int numDpi = 0;
            if (sampleDisplay.strokeRight.p != null && sampleDisplay.strokeRight.p.Length > 0)
            {
                numDpi = sampleDisplay.strokeRight.p.Length;
                int dpi = Math.Min(sampleDisplay.strokeRight.p.Length - 1, Math.Max(0, sampleDisplay.dataPointIndex));
                sampleDisplay.controllerModelRight.SetActive(true);
                sampleDisplay.controllerModelRight.transform.position = sampleDisplay.strokeRight.p[dpi];
                sampleDisplay.controllerModelRight.transform.rotation = sampleDisplay.strokeRight.q[dpi];
                sampleDisplay.headsetModel.SetActive(true);
                sampleDisplay.headsetModel.transform.position = sampleDisplay.strokeRight.hmd_p[dpi];
                sampleDisplay.headsetModel.transform.rotation = sampleDisplay.strokeRight.hmd_q[dpi];
            } else
            {
                sampleDisplay.controllerModelRight.SetActive(false);
            }
            if (sampleDisplay.strokeLeft.p != null && sampleDisplay.strokeLeft.p.Length > 0)
            {
                numDpi = Math.Max(numDpi, sampleDisplay.strokeLeft.p.Length);
                int dpi = Math.Min(sampleDisplay.strokeLeft.p.Length - 1, Math.Max(0, sampleDisplay.dataPointIndex));
                sampleDisplay.controllerModelLeft.SetActive(true);
                sampleDisplay.controllerModelLeft.transform.position = sampleDisplay.strokeLeft.p[dpi];
                sampleDisplay.controllerModelLeft.transform.rotation = sampleDisplay.strokeLeft.q[dpi];
                sampleDisplay.headsetModel.SetActive(true);
                sampleDisplay.headsetModel.transform.position = sampleDisplay.strokeLeft.hmd_p[dpi];
                sampleDisplay.headsetModel.transform.rotation = sampleDisplay.strokeLeft.hmd_q[dpi];
            } else
            {
                sampleDisplay.controllerModelLeft.SetActive(false);
            }

            sampleDisplay.dataPointIndex++;
            int fps = Math.Max(10, (int)(1.0f / Time.deltaTime));
            if (sampleDisplay.dataPointIndex > numDpi + fps)
            { // allowing for one second before and after
                sampleDisplay.dataPointIndex = -fps;
            }
        }
    }

    public static bool isGesturing
    {
        get {
            if (me == null || me.gestureManager == null)
                return false;
            return me.gestureManager.gesture_started;
        }
    }

    public static bool gesturingEnabled
    {
        get
        {
            if (me == null || me.gestureManager == null)
                return false;
            return me.gestureManager.gesturing_enabled;
        }
        set
        {
            if (me == null || me.gestureManager == null)
                return;
            me.gestureManager.gesturing_enabled = value;
        }
    }

    public static bool setSubmenuGesture(int gesture_id)
    {
        if (me == null || me.submenuGesture == null)
            return false;
        GestureManagerVR.me.submenuGesture.GetComponent<SubmenuGesture>().CurrentGesture = gesture_id;
        GestureManagerVR.me.submenuGesture.GetComponent<SubmenuGesture>().refresh();
        return true;
    }

    public static bool setSubmenuCombination(int combination_id, int part=-1, int gesture_id=-1)
    {
        if (me == null || me.submenuCombination == null || me.gestureManager == null || me.gestureManager.gc == null)
            return false;
        if (gesture_id<0)
        {
            for (part = me.gestureManager.gc.numberOfParts()-1; part >=0 ; part--)
            {
                gesture_id = me.gestureManager.gc.getCombinationPartGesture(combination_id, part);
                if (gesture_id >= 0)
                    break;
            }
        }
        GestureManagerVR.me.submenuCombination.GetComponent<SubmenuCombination>().CurrentCombination = combination_id;
        GestureManagerVR.me.submenuCombination.GetComponent<SubmenuCombination>().CurrentPart = part;
        GestureManagerVR.me.submenuCombination.GetComponent<SubmenuCombination>().CurrentGesture = gesture_id;
        GestureManagerVR.me.submenuCombination.GetComponent<SubmenuCombination>().refresh();
        if (me.submenuGesture != null)
        {
            GestureManagerVR.me.submenuGesture.GetComponent<SubmenuGesture>().CurrentGesture = gesture_id;
            GestureManagerVR.me.submenuGesture.GetComponent<SubmenuGesture>().CurrentPart = part;
            GestureManagerVR.me.submenuGesture.GetComponent<SubmenuGesture>().refresh();
        }
        return true;
    }
}
