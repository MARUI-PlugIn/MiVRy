using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubmenuTrainingButton : MonoBehaviour, GestureManagerButton
{
    public enum Operation
    {
        StartTraining,
        StopTraining
    }
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
