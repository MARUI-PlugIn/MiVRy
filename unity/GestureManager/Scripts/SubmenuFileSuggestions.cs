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
using System.IO;

public class SubmenuFileSuggestions : MonoBehaviour
{
    private bool initialized = false;
    TextMesh textField;
    GameObject upButton;
    GameObject selectButton;
    GameObject downButton;
    GameObject background;

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
        string currentPath = gm.gr != null ? gm.file_load_gestures : gm.file_load_combinations;
        currentPath = gm.getLoadPath(currentPath);
        string currentDir = Path.GetDirectoryName(currentPath);
        gm.file_suggestions.Clear();
        textField.text = "";
        if (!Directory.Exists(currentDir))
        {
            upButton.SetActive(false);
            selectButton.SetActive(false);
            downButton.SetActive(false);
            background.SetActive(false);
            textField.text = "";
            return;
        }
        foreach(string f in Directory.GetDirectories(currentDir))
        {
            gm.file_suggestions.Add(Path.GetFileName(f) + "/");
        }
        foreach(string f in Directory.GetFiles(currentDir, "*.dat"))
        {
            gm.file_suggestions.Add(Path.GetFileName(f));
        }
        if (gm.file_suggestions.Count == 0)
        {
            upButton.SetActive(false);
            selectButton.SetActive(false);
            downButton.SetActive(false);
            background.SetActive(false);
            return;
        }
        background.SetActive(true);
        if (!selectButton.activeSelf) selectButton.SetActive(true);
        if (gm.file_suggestion < 0)
        {
            gm.file_suggestion = 0;
        } else if (gm.file_suggestion >= gm.file_suggestions.Count)
        {
            gm.file_suggestion = gm.file_suggestions.Count - 1;
        }
        if (gm.file_suggestion == 0)
        {
            textField.text += "\n";
            upButton.SetActive(false);
        } else
        {
            textField.text += gm.file_suggestions[gm.file_suggestion - 1] + "\n";
            if (!upButton.activeSelf) upButton.SetActive(true);
        }
        textField.text += gm.file_suggestions[gm.file_suggestion] + "\n";
        if (gm.file_suggestion + 1 < gm.file_suggestions.Count)
        {
            textField.text += gm.file_suggestions[gm.file_suggestion + 1];
            if (!downButton.activeSelf) downButton.SetActive(true);
        } else
        {
            downButton.SetActive(false);
        }
    }
}
