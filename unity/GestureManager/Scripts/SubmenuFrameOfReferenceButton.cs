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

public class SubmenuFrameOfReferenceButton : GestureManagerButton
{
    public bool forward;
    public TextMesh frameOfReferenceDisplay;

    enum FrameOfReference
    {
        Yaw, Pitch, Roll, RotationOrder
    };
    [SerializeField] private FrameOfReference frameOfReference;

    private void OnTriggerEnter(Collider other)
    {
        GestureManager gm = GestureManagerVR.me?.gestureManager;
        if (!other.name.EndsWith("pointer") || gm == null)
            return;
        if (GestureManagerVR.isGesturing)
            return;
        if (GestureManagerVR.activeButton != null)
            return;
        GestureManagerVR.activeButton = this;
        this.material = activeButtonMaterial;
        switch (frameOfReference)
        {
            case FrameOfReference.Yaw:
                if (gm.frameOfReferenceYaw == GestureRecognition.FrameOfReference.Head)
                {
                    gm.frameOfReferenceYaw = GestureRecognition.FrameOfReference.World;
                    frameOfReferenceDisplay.text = "World";
                } else
                {
                    gm.frameOfReferenceYaw = GestureRecognition.FrameOfReference.Head;
                    frameOfReferenceDisplay.text = "Head";
                }
                break;
            case FrameOfReference.Pitch:
                if (gm.frameOfReferenceUpDownPitch == GestureRecognition.FrameOfReference.Head)
                {
                    gm.frameOfReferenceUpDownPitch = GestureRecognition.FrameOfReference.World;
                    frameOfReferenceDisplay.text = "World";
                }
                else
                {
                    gm.frameOfReferenceUpDownPitch = GestureRecognition.FrameOfReference.Head;
                    frameOfReferenceDisplay.text = "Head";
                }
                break;
            case FrameOfReference.Roll:
                if (gm.frameOfReferenceRollTilt == GestureRecognition.FrameOfReference.Head)
                {
                    gm.frameOfReferenceRollTilt = GestureRecognition.FrameOfReference.World;
                    frameOfReferenceDisplay.text = "World";
                }
                else
                {
                    gm.frameOfReferenceRollTilt = GestureRecognition.FrameOfReference.Head;
                    frameOfReferenceDisplay.text = "Head";
                }
                break;
            case FrameOfReference.RotationOrder:
                if (forward)
                {
                    if (gm.frameOfReferenceRotationOrder == GestureRecognition.RotationOrder.ZYX) {
                        gm.frameOfReferenceRotationOrder = GestureRecognition.RotationOrder.XYZ;
                    } else
                    {
                        gm.frameOfReferenceRotationOrder = (GestureRecognition.RotationOrder)((int)gm.frameOfReferenceRotationOrder + 1);
                    }
                } else // backwards
                {
                    if (gm.frameOfReferenceRotationOrder == GestureRecognition.RotationOrder.XYZ) {
                        gm.frameOfReferenceRotationOrder = GestureRecognition.RotationOrder.ZYX;
                    } else
                    {
                        gm.frameOfReferenceRotationOrder = (GestureRecognition.RotationOrder)((int)gm.frameOfReferenceRotationOrder - 1);
                    }
                }
                break;
        }
        GestureManagerVR.setInputFocus(null);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.name.EndsWith("pointer") && (Object)GestureManagerVR.activeButton == this)
            GestureManagerVR.activeButton = null;
        this.material = inactiveButtonMaterial;
    }

    private void OnDisable()
    {
        if ((Object)GestureManagerVR.activeButton == this)
            GestureManagerVR.activeButton = null;
        this.material = inactiveButtonMaterial;
    }
}
