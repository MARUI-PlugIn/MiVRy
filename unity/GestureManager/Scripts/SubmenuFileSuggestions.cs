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

public class SubmenuFileSuggestions : MonoBehaviour
{
    private bool initialized = false;
    TextMesh textField;
    GameObject upButton;
    GameObject selectButton;
    GameObject downButton;
    GameObject background;

    // File/folder suggestions for the load files button
    public static int file_suggestion_i = 0;
    public static List<string> file_suggestions = new List<string>();
    public static EditableTextField active_text_field = null;

    // Start is called before the first frame update
    void Start()
    {
        this.init();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void init()
    {
        for (int i = 0; i < this.transform.childCount; i++)
        {
            GameObject child = this.transform.GetChild(i).gameObject;
            switch (child.name)
            {
                case "SubmenuFilesSuggestionsText":
                    this.textField = child.GetComponent<TextMesh>();
                    break;
                case "SubmenuFilesSuggestionsUpBtn":
                    this.upButton = child;
                    break;
                case "SubmenuFilesSuggestionsSelectBtn":
                    this.selectButton = child;
                    break;
                case "SubmenuFilesSuggestionsDownBtn":
                    this.downButton = child;
                    break;
                case "SubmenuFilesSuggestionsBackground":
                    this.background = child;
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
        file_suggestions.Clear();
        string currentPath = null;
        if (active_text_field?.target == EditableTextField.Target.LoadFile) {
            currentPath = gm.gr != null ? gm.fileLoadGestures : gm.fileLoadCombinations;
            currentPath = gm.getLoadPath(currentPath);
        } else if (active_text_field?.target == EditableTextField.Target.SaveFile) {
            currentPath = gm.gr != null ? gm.fileSaveGestures : gm.fileSaveCombinations;
            currentPath = gm.getSavePath(currentPath);
        } else {
            this.gameObject.SetActive(false);
            return;
        }
        string currentDir = Path.GetDirectoryName(currentPath);
        if (!Directory.Exists(currentDir)) {
            this.gameObject.SetActive(false);
            return;
        }
        foreach(string f in Directory.GetDirectories(currentDir)) {
            file_suggestions.Add(Path.GetFileName(f) + "/");
        }
        foreach(string f in Directory.GetFiles(currentDir, "*.dat")) {
            file_suggestions.Add(Path.GetFileName(f));
        }
        if (file_suggestions.Count == 0) {
            this.gameObject.SetActive(false);
            return;
        }
        this.gameObject.SetActive(true);
        this.background.SetActive(true);
        if (!selectButton.activeSelf) selectButton.SetActive(true);
        if (file_suggestion_i < 0) {
            file_suggestion_i = 0;
        } else if (file_suggestion_i >= file_suggestions.Count) {
            file_suggestion_i = file_suggestions.Count - 1;
        }
        textField.text = "";
        if (file_suggestion_i == 0) {
            textField.text += "\n";
            upButton.SetActive(false);
        } else {
            textField.text += file_suggestions[file_suggestion_i - 1] + "\n";
            if (!upButton.activeSelf) upButton.SetActive(true);
        }
        textField.text += file_suggestions[file_suggestion_i] + "\n";
        if (file_suggestion_i + 1 < file_suggestions.Count) {
            textField.text += file_suggestions[file_suggestion_i + 1];
            if (!downButton.activeSelf) downButton.SetActive(true);
        } else {
            downButton.SetActive(false);
        }
    }
}
