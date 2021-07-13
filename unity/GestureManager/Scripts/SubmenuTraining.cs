using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubmenuTraining : MonoBehaviour
{
    private bool initialized = false;
    private TextMesh TrainingCurrentStatus;
    private TextMesh TrainingCurrentPerformance;

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
                case "SubmenuTrainingCurrentStatus":
                    TrainingCurrentStatus = child.GetComponent<TextMesh>();
                    break;
                case "SubmenuTrainingCurrentPerformance":
                    TrainingCurrentPerformance = child.GetComponent<TextMesh>();
                    break;
            }
        }
        this.initialized = true;
    }

    public void refresh()
    {
        if (!this.initialized)
            this.init();
        if (GestureManagerVR.me == null || GestureManagerVR.me.gestureManager == null)
            return;
        if (GestureManagerVR.me.gestureManager.gr != null)
        {
            if (GestureManagerVR.me.gestureManager.gr.isTraining())
            {
                TrainingCurrentStatus.text = "yes";
                TrainingCurrentPerformance.text = $"{GestureManagerVR.me.gestureManager.last_performance_report * 100.0f}%";
            } else
            {
                TrainingCurrentStatus.text = "no";
                TrainingCurrentPerformance.text = $"{GestureManagerVR.me.gestureManager.gr.recognitionScore() * 100.0f}%";
            }
        } else if (GestureManagerVR.me.gestureManager.gc != null)
        {
            if (GestureManagerVR.me.gestureManager.gc.isTraining())
            {
                TrainingCurrentStatus.text = "yes";
                TrainingCurrentPerformance.text = $"{GestureManagerVR.me.gestureManager.last_performance_report * 100.0f}%";
            } else
            {
                TrainingCurrentStatus.text = "no";
                TrainingCurrentPerformance.text = $"{GestureManagerVR.me.gestureManager.gc.recognitionScore() * 100.0f}%";
            }
        }
    }
}
