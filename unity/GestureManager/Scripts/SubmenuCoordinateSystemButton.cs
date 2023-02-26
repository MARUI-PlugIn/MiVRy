﻿/*
 * MiVRy - 3D gesture recognition library plug-in for Unity.
 * Version 2.7
 * Copyright (c) 2023 MARUI-PlugIn (inc.)
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

public class SubmenuCoordinateSystemButton : MonoBehaviour, GestureManagerButton
{
    public bool forward;
    public TextMesh coordinateSystemDisplay;

    [SerializeField] private Material inactiveButtonMaterial;
    [SerializeField] private Material activeButtonMaterial;

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
        this.GetComponent<Renderer>().material = activeButtonMaterial;
        switch (gm.mivryCoordinateSystem)
        {
            case Mivry.MivryCoordinateSystem.UnrealEngine:
                if (forward)
                {
                    gm.mivryCoordinateSystem = Mivry.MivryCoordinateSystem.SteamVR;
                    coordinateSystemDisplay.text = "SteamVR";
                }
                else
                {
                    gm.mivryCoordinateSystem = Mivry.MivryCoordinateSystem.OpenXR;
                    coordinateSystemDisplay.text = "OpenXR";
                }
                break;
            case Mivry.MivryCoordinateSystem.OpenXR:
                if (forward)
                {
                    gm.mivryCoordinateSystem = Mivry.MivryCoordinateSystem.UnrealEngine;
                    coordinateSystemDisplay.text = "Unreal";
                } else
                {
                    gm.mivryCoordinateSystem = Mivry.MivryCoordinateSystem.OculusVR;
                    coordinateSystemDisplay.text = "OculusVR";
                }
                break;
            case Mivry.MivryCoordinateSystem.OculusVR:
                if (forward)
                {
                    gm.mivryCoordinateSystem = Mivry.MivryCoordinateSystem.OpenXR;
                    coordinateSystemDisplay.text = "OpenXR";
                }
                else
                {
                    gm.mivryCoordinateSystem = Mivry.MivryCoordinateSystem.SteamVR;
                    coordinateSystemDisplay.text = "SteamVR";
                }
                break;
            case Mivry.MivryCoordinateSystem.SteamVR:
            default:
                if (forward)
                {
                    gm.mivryCoordinateSystem = Mivry.MivryCoordinateSystem.OculusVR;
                    coordinateSystemDisplay.text = "OculusVR";
                }
                else
                {
                    gm.mivryCoordinateSystem = Mivry.MivryCoordinateSystem.UnrealEngine;
                    coordinateSystemDisplay.text = "Unreal";
                }
                break;
        }
        GestureManagerVR.me?.submenuFrameOfReference?.GetComponent<SubmenuFrameOfReference>()?.refresh();
        GestureManagerVR.setInputFocus(null);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.name.EndsWith("pointer") && (Object)GestureManagerVR.activeButton == this)
            GestureManagerVR.activeButton = null;
        this.GetComponent<Renderer>().material = inactiveButtonMaterial;
    }

    private void OnDisable()
    {
        if ((Object)GestureManagerVR.activeButton == this)
            GestureManagerVR.activeButton = null;
        this.GetComponent<Renderer>().material = inactiveButtonMaterial;
    }
}
