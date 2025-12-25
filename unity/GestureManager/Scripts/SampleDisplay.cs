/*
 * MiVRy - 3D gesture recognition library plug-in for Unity.
 * Version 2.14
 * Copyright (c) 2025 MARUI-PlugIn (inc.)
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

public class SampleDisplay : MonoBehaviour
{
    public static SubmenuGesture submenuGesture;

    private static int _sampleId = -1;
    public static int sampleId
    {
        get => _sampleId;
        set
        {
            if (value >= 0) {
                GestureManager gm = GestureManagerVR.me.gestureManager;
                int gestureId = submenuGesture.CurrentGesture;
                if (gestureId < 0) {
                    value = -1;
                } else {
                    if (gm.gr != null) {
                        int numSamples = gm.gr.getGestureNumberOfSamples(gestureId);
                        value = Math.Min(value, numSamples - 1);
                    } else if (gm.gc != null) {
                        int part = submenuGesture.CurrentPart;
                        int numSamples = gm.gc.getGestureNumberOfSamples(part, gestureId);
                        value = Math.Min(value, numSamples - 1);
                    } else {
                        value = -1;
                    }
                }
            }
            _sampleId = value;
            if (_sampleId >= 0) {
                reloadStroke();
            }
        }
    }
    public GameObject headsetModel = null;
    public GameObject controllerModelLeft = null;
    public GameObject controllerModelRight = null;
    public struct Stroke
    {
        public Vector3[] hmd_p;
        public Quaternion[] hmd_q;
        public Vector3[] p;
        public Quaternion[] q;
        public double[] t;
    };
    private static Stroke stroke = new Stroke();
    private static double playTime = -1.0; 
    private static int dataPointIndex = 0;

    private static void reloadStroke()
    {
        stroke.p = null;
        stroke.q = null;
        stroke.hmd_p = null;
        stroke.hmd_q = null;
        if (submenuGesture == null || !submenuGesture.gameObject.activeSelf) {
            _sampleId = -1;
        }
        if (_sampleId < 0) {
            return;
        }
        int gestureId = submenuGesture.CurrentGesture;
        int ret;
        var gm = GestureManagerVR.me.gestureManager;
        if (gm.gr != null) {
            int numSamples = gm.gr.getGestureNumberOfSamples(gestureId);
            if (_sampleId >= numSamples) {
                _sampleId = numSamples - 1;
            }
            if (numSamples >= 0) {
                ret = gm.gr.getGestureSampleStroke(gestureId, _sampleId, 0,
                    ref stroke.p, ref stroke.q, ref stroke.hmd_p, ref stroke.hmd_q, ref stroke.t
                );
                if (ret <= 0 || stroke.t == null || stroke.t.Length == 0) {
                    gm.consoleText = $"Failed to get sample data ({ret}).";
                    return;
                }
                double t0 = stroke.t[0];
                double tn = stroke.t[stroke.t.Length - 1];
                double dt = tn - t0;
                if (dt <= 0.0) { // old file, does not have time stamps
                    dt = 1.0 / (double)stroke.t.Length; // display whole gesture in one second
                    for (int i = 0; i < stroke.t.Length; i++) {
                        stroke.t[i] = t0 + i * dt;
                    }
                }
                for (int i = 0; i < stroke.p.Length; i++) {
                    Mivry.convertBackHandInput(gm.mivryCoordinateSystem, gm.unityXrPlugin, ref stroke.p[i], ref stroke.q[i]);
                    Mivry.convertBackHeadInput(gm.mivryCoordinateSystem, ref stroke.hmd_p[i], ref stroke.hmd_q[i]);
                }
                playTime = -1;
                dataPointIndex = 0;
            }
        } else if (gm.gc != null) {
            int part = submenuGesture.CurrentPart;
            _sampleId = Math.Min(_sampleId, gm.gc.getGestureNumberOfSamples(part, gestureId) - 1);
            if (_sampleId < 0) {
                return;
            }
            ret = gm.gc.getGestureSampleStroke(part, gestureId, _sampleId, 0,
                ref stroke.p, ref stroke.q, ref stroke.hmd_p, ref stroke.hmd_q, ref stroke.t
            );
            if (ret <= 0 || stroke.t == null || stroke.t.Length == 0) {
                gm.consoleText = $"Failed to get sample data ({ret}).";
                return;
            }
            double t0 = stroke.t[0];
            double tn = stroke.t[stroke.t.Length - 1];
            double dt = tn - t0;
            if (dt <= 0.0) { // old file, does not have time stamps
                dt = 1.0 / (double)stroke.t.Length; // display whole gesture in one second
                for (int i = 0; i < stroke.t.Length; i++) {
                    stroke.t[i] = t0 + i * dt;
                }
            }
            for (int i = 0; i < stroke.p.Length; i++) {
                Mivry.convertBackHandInput(gm.mivryCoordinateSystem, gm.unityXrPlugin, ref stroke.p[i], ref stroke.q[i]);
                Mivry.convertBackHeadInput(gm.mivryCoordinateSystem, ref stroke.hmd_p[i], ref stroke.hmd_q[i]);
            }
            playTime = -1;
            dataPointIndex = 0;
        } else {
            gm.consoleText = "ERROR on reloadStroke: neither GR nor GC set.";
        }
        GestureManagerVR.me.submenuGesture.GetComponent<SubmenuGesture>().refresh();
    }

    void Start()
    {
        for (int i = 0; i < this.transform.childCount; i++) {
            GameObject child = this.transform.GetChild(i).gameObject;
            switch (child.name) {
                case "SampleDisplayHeadset":
                    this.headsetModel = child;
                    break;
                case "SubmenuGesture":
                    submenuGesture = child.GetComponent<SubmenuGesture>();
                    break;
            }
        }
    }

    void InitializeControllerModel(ref GameObject controllerModel, string name)
    {
        var gameObject = GameObject.Find(name);
        controllerModel = UnityEngine.Object.Instantiate(gameObject, GestureManagerVR.me.transform);
        controllerModel.name = "SampleDisplay ControllerModel " + name;
        Destroy(controllerModel.GetComponent<UnityEngine.SpatialTracking.TrackedPoseDriver>());
        for (int i = controllerModel.transform.childCount - 1; i >= 0; i--) {
            var child = controllerModel.transform.GetChild(i);
            for (int j = child.childCount - 1; j >= 0; j--) {
                var grandChild = child.GetChild(j);
                if (grandChild.gameObject.name.EndsWith("pointer")) {
                    GameObject.Destroy(grandChild.gameObject);
                } else {
                    MeshRenderer meshRenderer = grandChild.gameObject.GetComponent<MeshRenderer>();
                    if (meshRenderer != null) {
                        meshRenderer.material = this.headsetModel?.GetComponent<MeshRenderer>()?.material;
                    }
                }
            }
        }
    }

    void Update()
    {
        if (_sampleId < 0 || stroke.p == null || stroke.p.Length <= 0) {
            if (controllerModelLeft != null) {
                controllerModelLeft.SetActive(false);
            }
            if (controllerModelRight != null) {
                controllerModelRight.SetActive(false);
            }
            headsetModel.SetActive(false);
            return;
        }
        if (controllerModelLeft == null) {
            InitializeControllerModel(ref controllerModelLeft, "Left Hand");
        }
        if (controllerModelRight == null) {
            InitializeControllerModel(ref controllerModelRight, "Right Hand");
        }
        if (stroke.t.Length == 0) {
            this.controllerModelLeft.SetActive(false);
            this.controllerModelRight.SetActive(false);
            return;
        }
        if (dataPointIndex >= stroke.t.Length) {
            dataPointIndex = 0;
        }
        playTime += Time.deltaTime;
        while (playTime > stroke.t[dataPointIndex]) {
            if (dataPointIndex < stroke.t.Length - 1) {
                dataPointIndex++;
            } else if (playTime < stroke.t[stroke.t.Length - 1] + 1.0) {
                dataPointIndex = stroke.t.Length - 1; // hold last sample for one second after end
                break;
            } else { // more than one second after last sample
                playTime = -1; // restart, but with one second hold-time before displaying motion
                dataPointIndex = 0;
                break;
            }
        }
        int part = submenuGesture.CurrentPart;
        GameObject controllerModel;
        if (part == GestureManagerVR.me.gestureManager.lefthand_combination_part) {
            controllerModel = this.controllerModelLeft;
            this.controllerModelRight.SetActive(false);
        } else {
            controllerModel = this.controllerModelRight;
            this.controllerModelLeft.SetActive(false);
        }
        controllerModel.SetActive(true);
        controllerModel.transform.position = stroke.p[dataPointIndex];
        controllerModel.transform.rotation = stroke.q[dataPointIndex];
        this.headsetModel.SetActive(true);
        this.headsetModel.transform.position = stroke.hmd_p[dataPointIndex];
        this.headsetModel.transform.rotation = stroke.hmd_q[dataPointIndex];
    }
}
