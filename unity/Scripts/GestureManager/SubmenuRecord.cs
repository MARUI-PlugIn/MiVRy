using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubmenuRecord : MonoBehaviour
{
    private bool initialized = false;
    private TextMesh SubmenuRecordValue;
    
    void Start()
    {
        this.init();
    }

    private void init()
    {
        for (int i=0; i<this.transform.childCount; i++)
        {
            GameObject child = this.transform.GetChild(i).gameObject;
            switch (child.name)
            {
                case "SubmenuRecordValue":
                    this.SubmenuRecordValue = child.GetComponent<TextMesh>();
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
        if (gm.gr != null)
        {
            int num_gestures = gm.gr.numberOfGestures();
            if (gm.record_gesture_id >= num_gestures)
                gm.record_gesture_id = num_gestures - 1;
            string gestureName = (gm.record_gesture_id == -1) ? "[Testing, not recording]" : gm.gr.getGestureName(gm.record_gesture_id);
            SubmenuRecordValue.text = gestureName;
        }
        else if (gm.gc != null)
        {
            int num_combinations = gm.gc.numberOfGestureCombinations();
            if (gm.record_combination_id >= num_combinations)
                gm.record_combination_id = num_combinations - 1;
            string gestureName = (gm.record_combination_id == -1) ? "[Testing, not recording]" : gm.gc.getGestureCombinationName(gm.record_combination_id);
            SubmenuRecordValue.text = gestureName;
        }
    }
}
