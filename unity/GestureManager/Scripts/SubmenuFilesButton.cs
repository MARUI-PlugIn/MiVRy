using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SubmenuFilesButton : MonoBehaviour, GestureManagerButton
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
