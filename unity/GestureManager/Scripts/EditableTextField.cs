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
using System.Text.RegularExpressions;

public class EditableTextField : MonoBehaviour
{
    [System.Serializable]
    public enum Target
    {
        NumberOfParts,
        GestureName,
        CombinationName,
        LoadFile,
        SaveFile
    }
    public Target target;

    public TextMesh displayText;

    public int maxDisplayLength;

    // Start is called before the first frame update
    void Start()
    {
        this.refreshText();
    }

    public void refreshText()
    {
        GestureManager gm = GestureManagerVR.me?.gestureManager;
        if (gm == null)
            return;
        if (displayText == null)
            return;

        string text = null;
        switch (this.target)
        {
            case Target.NumberOfParts:
                text = $"{gm.numberOfParts}";
                break;
            case Target.GestureName: {
                SubmenuGesture submenuGesture = this.transform.parent.gameObject.GetComponent<SubmenuGesture>();
                if (submenuGesture.CurrentGesture < 0)
                {
                    text = "";
                } else if (gm.gr != null)
                {
                    text = gm.gr.getGestureName(submenuGesture.CurrentGesture);
                } else if (gm.gc != null)
                {
                    text = gm.gc.getGestureName(submenuGesture.CurrentPart, submenuGesture.CurrentGesture);
                } else
                {
                    text = "???";
                }
                } break;
            case Target.CombinationName: {
                SubmenuCombination submenuCombination = this.transform.parent.gameObject.GetComponent<SubmenuCombination>();
                if (submenuCombination.CurrentCombination < 0)
                {
                    text = "";
                }
                else if (gm.gc != null)
                {
                    text = gm.gc.getGestureCombinationName(submenuCombination.CurrentCombination);
                }
                else
                {
                    text = "???";
                }
                } break;
            case Target.LoadFile:
                if (gm.gr != null)
                    text = gm.file_load_gestures;
                else if (gm.gc != null)
                    text = gm.file_load_combinations;
                else
                    text = "";
                break;
            case Target.SaveFile:
                if (gm.gr != null)
                    text = gm.file_save_gestures;
                else if (gm.gc != null)
                    text = gm.file_save_combinations;
                else
                    text = "";
                break;
            default:
                text = "???";
                break;
        }
        if (text.Length > this.maxDisplayLength)
            text = text.Substring(text.Length - this.maxDisplayLength);
        this.displayText.text = text;
    }

    public void setValue(string text)
    {
        GestureManager gm = GestureManagerVR.me?.gestureManager;
        if (gm == null)
            return;
        switch (this.target)
        {
            case Target.NumberOfParts:
                text = Regex.Replace(text, "[^0-9]", "");
                text = (text.Length == 0) ? "0" : text.Substring(text.Length - 1);// only use last digit
                gm.numberOfParts = int.Parse(text);
                GestureManagerVR.refresh();
                break;
            case Target.GestureName: {
                SubmenuGesture submenuGesture = this.transform.parent.gameObject.GetComponent<SubmenuGesture>();
                if (submenuGesture.CurrentGesture < 0)
                {
                    return;
                }
                if (gm.gr != null)
                    gm.gr.setGestureName(submenuGesture.CurrentGesture, text);
                else if (gm.gc != null)
                    gm.gc.setGestureName(submenuGesture.CurrentPart, submenuGesture.CurrentGesture, text);
                } break;
            case Target.CombinationName: {
                SubmenuCombination submenuCombination = this.transform.parent.gameObject.GetComponent<SubmenuCombination>();
                if (submenuCombination.CurrentCombination < 0)
                {
                    return;
                }
                gm.gc.setGestureCombinationName(submenuCombination.CurrentCombination, text);
                } break;
            case Target.LoadFile:
                if (gm.gr != null)
                    gm.file_load_gestures = text;
                else if (gm.gc != null)
                    gm.file_load_combinations = text;
                break;
            case Target.SaveFile:
                if (gm.gr != null)
                    gm.file_save_gestures = text;
                else if (gm.gc != null)
                    gm.file_save_combinations = text;
                break;
        }
        this.refreshText();
    }

    public string getValue()
    {
        GestureManager gm = GestureManagerVR.me?.gestureManager;
        if (gm == null)
            return "";
        switch (this.target)
        {
            case Target.NumberOfParts:
                return $"{gm.numberOfParts}";
            case Target.GestureName: {
                SubmenuGesture submenuGesture = this.transform.parent.gameObject.GetComponent<SubmenuGesture>();
                if (submenuGesture.CurrentGesture < 0)
                {
                    return "";
                }
                if (gm.gr != null)
                    return gm.gr.getGestureName(submenuGesture.CurrentGesture);
                else if (gm.gc != null)
                    return gm.gc.getGestureName(submenuGesture.CurrentPart, submenuGesture.CurrentGesture);
                else
                    return "[ERROR]";
                }
            case Target.CombinationName: {
                SubmenuCombination submenuCombination = this.transform.parent.gameObject.GetComponent<SubmenuCombination>();
                if (submenuCombination.CurrentCombination < 0)
                {
                    return "";
                }
                return gm.gc.getGestureCombinationName(submenuCombination.CurrentCombination);
                }
            case Target.LoadFile:
                if (gm.gr != null)
                    return gm.file_load_gestures;
                else if (gm.gc != null)
                    return gm.file_load_combinations;
                return "[ERROR]";
            case Target.SaveFile:
                if (gm.gr != null)
                    return gm.file_save_gestures;
                else if (gm.gc != null)
                    return gm.file_save_combinations;
                return "[ERROR]";
        }
        return "[ERROR]";
    }

    public void keyboardInput(KeyboardKey key)
    {
        if (displayText == null)
            return;
        this.setValue(key.applyTo(this.getValue()));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.name.EndsWith("pointer"))
            return;
        if (GestureManagerVR.isGesturing)
            return;
        GestureManagerVR.setInputFocus(this);
    }
}
