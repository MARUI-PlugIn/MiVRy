using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubmenuFilesButton : MonoBehaviour
{
    [System.Serializable]
    public enum Operation {
        LoadGestureFile,
        SaveGestureFile,
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
        }
        GestureManagerVR.setInputFocus(null);
        GestureManagerVR.refresh();
    }
}
