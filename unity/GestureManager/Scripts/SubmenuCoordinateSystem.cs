/*
 * MiVRy - 3D gesture recognition library plug-in for Unity.
 * Version 2.10
 * Copyright (c) 2024 MARUI-PlugIn (inc.)
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

public class SubmenuCoordinateSystem : MonoBehaviour
{
    private bool initialized = false;
    private TextMesh SubmenuCoordinateSystemValue;
    
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
                case "SubmenuCoordinateSystemValue":
                    this.SubmenuCoordinateSystemValue = child.GetComponent<TextMesh>();
                    break;
            }
        }
        this.initialized = true;
    }

    public void refresh()
    {
        if (!this.initialized)
            this.init();
        GestureManager gm = GestureManagerVR.me?.gestureManager;
        if (gm == null)
            return;
        switch (gm.mivryCoordinateSystem)
        {
            case Mivry.MivryCoordinateSystem.Unity_OpenXR:
                SubmenuCoordinateSystemValue.text = "OpenXR";
                break;
            case Mivry.MivryCoordinateSystem.Unity_OculusVR:
                SubmenuCoordinateSystemValue.text = "OculusVR";
                break;
            case Mivry.MivryCoordinateSystem.Unity_SteamVR:
                SubmenuCoordinateSystemValue.text = "SteamVR";
                break;
            case Mivry.MivryCoordinateSystem.Unreal_OpenXR:
                SubmenuCoordinateSystemValue.text = "UE OpenXR";
                break;
            case Mivry.MivryCoordinateSystem.Unreal_OculusVR:
                SubmenuCoordinateSystemValue.text = "UE OculusVR";
                break;
            case Mivry.MivryCoordinateSystem.Unreal_SteamVR:
                SubmenuCoordinateSystemValue.text = "UE SteamVR";
                break;
        }
    }
}
