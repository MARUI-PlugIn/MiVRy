using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubmenuTrainingButton : MonoBehaviour
{
    public enum Operation
    {
        StartTraining,
        StopTraining
    }
    public Operation operation;

    private void OnTriggerEnter(Collider other)
    {
        GestureManager gm = GestureManagerVR.me?.gestureManager;
        if (!other.name.EndsWith("pointer") || gm == null)
            return;
        if (GestureManagerVR.isGesturing)
            return;
        if (gm.gr != null)
        {
            if (this.operation == Operation.StartTraining)
            {
                gm.gr.startTraining();
            } else
            {
                gm.gr.stopTraining();
            }
        } else if (gm.gc != null)
        {
            if (this.operation == Operation.StartTraining)
            {
                gm.gc.startTraining();
            }
            else
            {
                gm.gc.stopTraining();
            }
        }
        GestureManagerVR.setInputFocus(null);
        GestureManagerVR.refresh();
    }
}
