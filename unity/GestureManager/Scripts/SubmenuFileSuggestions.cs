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
