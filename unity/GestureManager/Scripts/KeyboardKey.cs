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

public class KeyboardKey : MonoBehaviour
{
    public string key = null;
    public string keyShift = null;
    public string keyAlt = null;

    [SerializeField] private Material inactiveButtonMaterial;
    [SerializeField] private Material activeButtonMaterial;

    public static bool shiftActive = false;
    public static bool altActive = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.name.EndsWith("pointer"))
            return;
        if (GestureManagerVR.isGesturing)
            return;
        if (GestureManagerVR.activeButton != null)
            return;
        this.GetComponent<Renderer>().material = activeButtonMaterial;
        if (this.key == "Shift")
        {
            shiftActive = true;
            return;
        }
        if (this.key == "Alt")
        {
            altActive = true;
            return;
        }
        if (this.key == "CapsLock")
        {
            shiftActive = !shiftActive;
            return;
        }
        if (this.key == "Escape" || this.key == "Enter")
        {
            GestureManagerVR.setInputFocus(null);
            return;
        }
        GestureManagerVR.keyboardInput(this);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.name.EndsWith("pointer"))
            return;
        this.GetComponent<Renderer>().material = inactiveButtonMaterial;
        if (GestureManagerVR.isGesturing)
            return;
        if (this.key == "Shift")
        {
            shiftActive = false;
            return;
        }
        if (this.key == "Alt")
        {
            altActive = false;
            return;
        }
    }

    public string applyTo(string input)
    {
        string k = altActive ? keyAlt : shiftActive ? keyShift : key;
        switch (k)
        {
            case null:
                return input;
            case "Backspace":
                return (input.Length > 0) ? input.Substring(0, input.Length -1) : "";
            case "Enter":
                return input;
            default:
                return input + k;
        }
    }

    private void OnEnable()
    {
        this.GetComponent<Renderer>().material = inactiveButtonMaterial;
    }

    private void OnDisable()
    {
        this.GetComponent<Renderer>().material = inactiveButtonMaterial;
    }
}
