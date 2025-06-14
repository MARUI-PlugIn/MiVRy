﻿/*
 * MiVRy - 3D gesture recognition library plug-in for Unity.
 * Version 2.12
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

using UnityEngine;

public class GestureManagerHandle : MonoBehaviour
{
    public enum Target
    {
        GestureManager
        ,
        Keyboard
    };
    [SerializeField] public  Target   target;
    [SerializeField] private Material inactiveHandleMaterial;
    [SerializeField] private Material hoverHandleMaterial;
    [SerializeField] private Material activeHandleMaterial;

    private GameObject activePointer = null;
    private Matrix4x4  lastPointerMat;

    public static GestureManagerHandle hoverHandle = null;
    public static GestureManagerHandle draggingHandle = null;
    public static float hoverHandleLastUpdate = 0.0f;
    public static float draggingHandleLastUpdate = 0.0f;

    private Material material {
        set {
            var renderer = this.GetComponent<Renderer>();
            if (renderer != null) {
                renderer.material = value;
            }
            for (int i=this.transform.childCount-1; i>=0; i--) {
                renderer = this.transform.GetChild(i)?.gameObject.GetComponent<Renderer>();
                if (renderer != null) {
                    renderer.material = value;
                }
            }
        }
    }

    private void Update()
    {
        if (draggingHandle == this) {
            #if ENABLE_INPUT_SYSTEM
            float trigger_pressure = activePointer.name.ToLower().Contains("left")
                ? GestureManager.getInputControlValue("<XRController>{LeftHand}/trigger")
                : GestureManager.getInputControlValue("<XRController>{RightHand}/trigger");
            #else
            float trigger_pressure = activePointer.name.ToLower().Contains("left")
                ? Input.GetAxis("LeftControllerTrigger")
                : Input.GetAxis("RightControllerTrigger");
            #endif
            if (trigger_pressure < 0.80f)
            {
                draggingHandle = null;
                GestureManagerVR.gesturingEnabled = true;
                this.material = (hoverHandle == this) ? hoverHandleMaterial : inactiveHandleMaterial;
                this.lastPointerMat = Matrix4x4.identity;
                return;
            } // else:
            Matrix4x4 pointerMat = Matrix4x4.TRS(this.activePointer.transform.position, this.activePointer.transform.rotation, Vector3.one);
            if (!this.lastPointerMat.isIdentity) {
                GameObject targetObject = this.target == Target.Keyboard
                ? GestureManagerVR.me.keyboard
                : GestureManagerVR.me.gameObject;
                Matrix4x4 gmMat = Matrix4x4.TRS(
                    targetObject.transform.position,
                    targetObject.transform.rotation,
                    Vector3.one
                );
                gmMat = (pointerMat * this.lastPointerMat.inverse) * gmMat;
                targetObject.transform.position = gmMat.GetColumn(3);
                targetObject.transform.rotation = gmMat.rotation;
            }
            this.material = activeHandleMaterial;
            this.lastPointerMat = pointerMat;
            draggingHandleLastUpdate = Time.time;
            if (GestureManagerVR.me != null && GestureManagerVR.me.followUser)
            {
                GestureManagerVR.me.followUser = false;
                GameObject followMeButtonText = GameObject.Find("SubmenuGestureManagerFollowValue");
                TextMesh followMeButtonTextComponent = followMeButtonText?.GetComponent<TextMesh>();
                if (followMeButtonTextComponent != null)
                {
                    followMeButtonTextComponent.text = "No";
                }
            }
            return;
        }

        if (hoverHandle == this)
        {
            #if ENABLE_INPUT_SYSTEM
            float trigger_pressure = activePointer.name.ToLower().Contains("left")
                ? GestureManager.getInputControlValue("<XRController>{LeftHand}/trigger")
                : GestureManager.getInputControlValue("<XRController>{RightHand}/trigger");
            #else
            float trigger_pressure = activePointer.name.ToLower().Contains("left")
                ? Input.GetAxis("LeftControllerTrigger")
                : Input.GetAxis("RightControllerTrigger");
            #endif
            if (trigger_pressure > 0.85f) {
                GestureManagerVR.gesturingEnabled = false;
                draggingHandle = this;
                draggingHandleLastUpdate = Time.time;
                this.material = activeHandleMaterial;
                this.lastPointerMat = Matrix4x4.identity;
            }
            return;
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (!other.name.EndsWith("pointer"))
            return;
        if (GestureManagerVR.isGesturing)
            return;
        if (hoverHandle != null)
            return;
        GestureManagerVR.gesturingEnabled = false;
        hoverHandle = this;
        activePointer = other.gameObject;
        hoverHandleLastUpdate = Time.time;
        this.material = hoverHandleMaterial;
    }

    public void OnTriggerStay(Collider other)
    {
        if (!other.name.EndsWith("pointer"))
            return;
        if (GestureManagerVR.isGesturing)
            return;
        if (hoverHandle == null)
        {
            this.OnTriggerEnter(other);
        } else if (hoverHandle == this)
        {
            hoverHandleLastUpdate = Time.time;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject != this.activePointer)
            return;
        if (hoverHandle != this)
            return;
        hoverHandle = null;
        GestureManagerVR.gesturingEnabled = true;
        this.material = inactiveHandleMaterial;
    }
}
