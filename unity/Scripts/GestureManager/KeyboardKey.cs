using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardKey : MonoBehaviour
{
    public string key = null;
    public string keyShift = null;
    public string keyAlt = null;

    public static bool shiftActive = false;
    public static bool altActive = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.name.EndsWith("pointer"))
            return;
        if (GestureManagerVR.isGesturing)
            return;
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
}
