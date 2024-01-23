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
using System.IO;

public class SubmenuFilesButton : GestureManagerButton
{
    [System.Serializable]
    public enum Operation {
        LoadGestureFile,
        SaveGestureFile,
        FileSuggestionUp,
        FileSuggestionDown,
        FileSuggestionSelect
    };
    public Operation operation;

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
        switch (this.operation) {
            case Operation.LoadGestureFile:
                gm.loadFromFile();
                break;
            case Operation.SaveGestureFile:
                gm.saveToFile();
                break;
            case Operation.FileSuggestionUp:
                if (SubmenuFileSuggestions.file_suggestion_i > 0) {
                    SubmenuFileSuggestions.file_suggestion_i--;
                }
                break;
            case Operation.FileSuggestionDown:
                if (SubmenuFileSuggestions.file_suggestion_i + 1 < SubmenuFileSuggestions.file_suggestions.Count) {
                    SubmenuFileSuggestions.file_suggestion_i++;
                }
                break;
            case Operation.FileSuggestionSelect:
                if (SubmenuFileSuggestions.file_suggestion_i >= 0 && SubmenuFileSuggestions.file_suggestion_i < SubmenuFileSuggestions.file_suggestions.Count) {
                    if (SubmenuFileSuggestions.active_text_field?.target == EditableTextField.Target.LoadFile) {
                        if (gm.gr != null) {
                            string dir = (gm.fileLoadGestures.Contains("/") || gm.fileLoadGestures.Contains("\\"))
                                       ? Path.GetDirectoryName(gm.fileLoadGestures).Replace('\\', '/') + "/"
                                       : "";
                            gm.fileLoadGestures = dir + SubmenuFileSuggestions.file_suggestions[SubmenuFileSuggestions.file_suggestion_i];
                        } else if (gm.gc != null) {
                            string dir = (gm.fileLoadCombinations.Contains("/") || gm.fileLoadCombinations.Contains("\\"))
                                       ? Path.GetDirectoryName(gm.fileLoadCombinations).Replace('\\', '/') + "/"
                                       : "";
                            gm.fileLoadCombinations = dir + SubmenuFileSuggestions.file_suggestions[SubmenuFileSuggestions.file_suggestion_i];
                        }
                    } else if (SubmenuFileSuggestions.active_text_field?.target == EditableTextField.Target.SaveFile) {
                        if (gm.gr != null) {
                            string dir = (gm.fileSaveGestures.Contains("/") || gm.fileSaveGestures.Contains("\\"))
                                       ? Path.GetDirectoryName(gm.fileSaveGestures).Replace('\\', '/') + "/"
                                       : "";
                            gm.fileSaveGestures = dir + SubmenuFileSuggestions.file_suggestions[SubmenuFileSuggestions.file_suggestion_i];
                        } else if (gm.gc != null) {
                            string dir = (gm.fileSaveCombinations.Contains("/") || gm.fileSaveCombinations.Contains("\\"))
                                       ? Path.GetDirectoryName(gm.fileSaveCombinations).Replace('\\', '/') + "/"
                                       : "";
                            gm.fileSaveCombinations = dir + SubmenuFileSuggestions.file_suggestions[SubmenuFileSuggestions.file_suggestion_i];
                        }
                    }
                }
                break;
        }
        GestureManagerVR.setInputFocus(null);
        GestureManagerVR.refresh();
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
