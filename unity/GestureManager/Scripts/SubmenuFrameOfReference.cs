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

public class SubmenuFrameOfReference : MonoBehaviour
{
    private bool initialized = false;
    private TextMesh SubmenuFrameOfReferenceYawValue;
    private TextMesh SubmenuFrameOfReferencePitchValue;
    private TextMesh SubmenuFrameOfReferenceRollValue;
    private TextMesh SubmenuFrameOfReferenceRotationOrderValue;

    void Start()
    {
        this.init();
    }

    private void init()
    {
        for (int i=0; i<this.transform.childCount; i++)
        {
            GameObject child = this.transform.GetChild(i).gameObject;
            switch (child.name)
            {
                case "SubmenuFrameOfReferenceYawValue":
                    this.SubmenuFrameOfReferenceYawValue = child.GetComponent<TextMesh>();
                    break;
                case "SubmenuFrameOfReferencePitchValue":
                    this.SubmenuFrameOfReferencePitchValue = child.GetComponent<TextMesh>();
                    break;
                case "SubmenuFrameOfReferenceRollValue":
                    this.SubmenuFrameOfReferenceRollValue = child.GetComponent<TextMesh>();
                    break;
                case "SubmenuFrameOfReferenceRotationOrderValue":
                    this.SubmenuFrameOfReferenceRotationOrderValue = child.GetComponent<TextMesh>();
                    break;
            }
        }
        this.initialized = true;
    }

    public void refresh()
    {
        if (!this.initialized) {
            this.init();
        }
        GestureManager gm = GestureManagerVR.me?.gestureManager;
        if (gm == null) {
            return;
        }
        SubmenuFrameOfReferenceYawValue.text   = gm.frameOfReferenceYaw == GestureRecognition.FrameOfReference.Head         ? "Head" : "World";
        SubmenuFrameOfReferencePitchValue.text = gm.frameOfReferenceUpDownPitch == GestureRecognition.FrameOfReference.Head ? "Head" : "World";
        SubmenuFrameOfReferenceRollValue.text  = gm.frameOfReferenceRollTilt == GestureRecognition.FrameOfReference.Head    ? "Head" : "World";
        switch (gm.frameOfReferenceRotationOrder)
        {
            case GestureRecognition.RotationOrder.XYZ:
                SubmenuFrameOfReferenceRotationOrderValue.text = "XYZ";
                break;
            case GestureRecognition.RotationOrder.XZY:
                SubmenuFrameOfReferenceRotationOrderValue.text = "XZY";
                break;
            case GestureRecognition.RotationOrder.YXZ:
                SubmenuFrameOfReferenceRotationOrderValue.text = "YXZ(Unity)";
                break;
            case GestureRecognition.RotationOrder.YZX:
                SubmenuFrameOfReferenceRotationOrderValue.text = "YZX";
                break;
            case GestureRecognition.RotationOrder.ZXY:
                SubmenuFrameOfReferenceRotationOrderValue.text = "ZXY";
                break;
            case GestureRecognition.RotationOrder.ZYX:
                SubmenuFrameOfReferenceRotationOrderValue.text = "ZYX(Unreal)";
                break;
        }
    }
}
