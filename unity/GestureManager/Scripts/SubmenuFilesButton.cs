using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SubmenuFilesButton : MonoBehaviour
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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        GestureManager gm = GestureManagerVR.me?.gestureManager;
        if (!other.name.EndsWith("pointer") || gm == null)
            return;
        if (GestureManagerVR.isGesturing)
            return;
        switch (this.operation)
        {
            case Operation.LoadGestureFile:
                gm.loadFromFile();
                break;
            case Operation.SaveGestureFile:
                gm.saveToFile();
                break;
            case Operation.FileSuggestionUp:
                if (gm.file_suggestion > 0)
                {
                    gm.file_suggestion--;
                }
                break;
            case Operation.FileSuggestionDown:
                if (gm.file_suggestion + 1 < gm.file_suggestions.Count)
                {
                    gm.file_suggestion++;
                }
                break;
            case Operation.FileSuggestionSelect:
                if (gm.file_suggestion >= 0 && gm.file_suggestion < gm.file_suggestions.Count)
                {
                    if (gm.gr != null)
                    {
                        string dir = (gm.file_load_gestures.Contains("/") || gm.file_load_gestures.Contains("\\"))
                                   ? Path.GetDirectoryName(gm.file_load_gestures).Replace('\\', '/') + "/"
                                   : "";
                        gm.file_load_gestures = dir + gm.file_suggestions[gm.file_suggestion];
                    } else if (gm.gc != null)
                    {
                        string dir = (gm.file_load_combinations.Contains("/") || gm.file_load_combinations.Contains("\\"))
                                   ? Path.GetDirectoryName(gm.file_load_combinations).Replace('\\', '/') + "/"
                                   : "";
                        gm.file_load_combinations = dir + gm.file_suggestions[gm.file_suggestion];
                    }
                }
                
                break;
        }
        GestureManagerVR.setInputFocus(null);
        GestureManagerVR.refresh();
    }
}
