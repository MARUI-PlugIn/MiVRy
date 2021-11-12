using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubmenuRecordButton : MonoBehaviour, GestureManagerButton
{
    public bool forward;
    public TextMesh gestureNameDisplay;

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
        if (gm.gr != null)
        {
            int num_gestures = gm.gr.numberOfGestures();
            if (this.forward)
            {
                gm.record_gesture_id = (gm.record_gesture_id + 1 >= num_gestures) ? -1 : gm.record_gesture_id + 1;
                GestureManagerVR.setSubmenuGesture(gm.record_gesture_id);
            } else
            {
                gm.record_gesture_id = (gm.record_gesture_id - 1 < -1) ? num_gestures - 1 : gm.record_gesture_id - 1;
                GestureManagerVR.setSubmenuGesture(gm.record_gesture_id);
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
                GestureManagerVR.setSubmenuCombination(gm.record_combination_id);
            }
            else
            {
                gm.record_combination_id = (gm.record_combination_id - 1 < -1) ? num_combinations - 1 : gm.record_combination_id - 1;
                GestureManagerVR.setSubmenuCombination(gm.record_combination_id);
            }
            if (gestureNameDisplay != null)
            {
                string gestureName = gm.record_combination_id == -1 ? "[Testing, not recording]" : gm.gc.getGestureCombinationName(gm.record_combination_id);
                gestureNameDisplay.text = gestureName;
            }
        }
        GestureManagerVR.setInputFocus(null);
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
