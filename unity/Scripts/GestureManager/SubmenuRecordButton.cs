using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubmenuRecordButton : MonoBehaviour
{
    public bool forward;
    public TextMesh gestureNameDisplay;

    private void OnTriggerEnter(Collider other)
    {
        GestureManager gm = GestureManagerVR.me?.gestureManager;
        if (!other.name.EndsWith("pointer") || gm == null)
            return;
        if (GestureManagerVR.isGesturing)
            return;
        if (gm.gr != null)
        {
            int num_gestures = gm.gr.numberOfGestures();
            if (this.forward)
            {
                gm.record_gesture_id = (gm.record_gesture_id + 1 >= num_gestures) ? -1 : gm.record_gesture_id + 1;
            } else
            {
                gm.record_gesture_id = (gm.record_gesture_id - 1 < -1) ? num_gestures - 1 : gm.record_gesture_id - 1;
            }
            if (gestureNameDisplay != null)
            {
                string gestureName = gm.record_gesture_id == -1 ? "[Testing, not recording]" : gm.gr.getGestureName(gm.record_gesture_id);
                gestureNameDisplay.text = gestureName;
            }
        }
        else if (gm.gc != null)
        {
            int num_combinations = gm.gc.numberOfGestureCombinations();
            if (this.forward)
            {
                gm.record_combination_id = (gm.record_combination_id + 1 >= num_combinations) ? -1 : gm.record_combination_id + 1;
            }
            else
            {
                gm.record_combination_id = (gm.record_combination_id - 1 < -1) ? num_combinations - 1 : gm.record_combination_id - 1;
            }
            if (gestureNameDisplay != null)
            {
                string gestureName = gm.record_combination_id == -1 ? "[Testing, not recording]" : gm.gc.getGestureCombinationName(gm.record_combination_id);
                gestureNameDisplay.text = gestureName;
            }
        }
        GestureManagerVR.setInputFocus(null);
    }
}
