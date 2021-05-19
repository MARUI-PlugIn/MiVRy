using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubmenuGesture : MonoBehaviour
{
    private bool initialized = false;
    private GameObject PartNextBtn;
    private GameObject PartPrevBtn;
    private GameObject PartText;
    private GameObject GestureNextBtn;
    private GameObject GesturePrevBtn;
    private GameObject GestureNameText;
    private GameObject GestureNameInput;
    private GameObject GestureCreateBtn;
    private GameObject GestureDeleteLastSampleBtn;
    private GameObject GestureDeleteAllSamplesBtn;
    private GameObject GestureSamplesText;
    private GameObject GestureDeleteGestureBtn;

    private int currentPart = -1;

    public int CurrentPart
    {
        get { return currentPart; }
        set { currentPart = value; refesh(); }
    }

    private int currentGesture = -1;

    public int CurrentGesture
    {
        get { return currentGesture; }
        set { currentGesture = value; refesh(); }
    }

    void Start()
    {
        this.init();
        this.refesh();
    }

    private void init()
    {
        for (int i=0; i<this.transform.childCount; i++)
        {
            GameObject child = this.transform.GetChild(i).gameObject;
            switch (child.name)
            {
                case "SubmenuGesturePartNextBtn":
                    PartNextBtn = child;
                    break;
                case "SubmenuGesturePartPrevBtn":
                    PartPrevBtn = child;
                    break;
                case "SubmenuGesturePartText":
                    PartText = child;
                    break;
                case "SubmenuGestureNextBtn":
                    GestureNextBtn = child;
                    break;
                case "SubmenuGesturePrevBtn":
                    GesturePrevBtn = child;
                    break;
                case "SubmenuGestureNameText":
                    GestureNameText = child;
                    break;
                case "SubmenuGestureNameInput":
                    GestureNameInput = child;
                    break;
                case "SubmenuGestureCreateBtn":
                    GestureCreateBtn = child;
                    break;
                case "SubmenuGestureDeleteLastSampleBtn":
                    GestureDeleteLastSampleBtn = child;
                    break;
                case "SubmenuGestureDeleteAllSamplesBtn":
                    GestureDeleteAllSamplesBtn = child;
                    break;
                case "SubmenuGestureSamplesText":
                    GestureSamplesText = child;
                    break;
                case "SubmenuGestureDeleteGestureBtn":
                    GestureDeleteGestureBtn = child;
                    break;
            }
        }
        this.initialized = true;
    }

    public void refesh()
    {
        if (!this.initialized)
            this.init();
        GestureManager gm = GestureManagerVR.me?.gestureManager;
        if (gm == null)
            return;
        if (gm.gr != null)
        {
            PartNextBtn.SetActive(false);
            PartPrevBtn.SetActive(false);
            PartText.SetActive(false);
            GestureCreateBtn.SetActive(true);
            int numGestures = gm.gr.numberOfGestures();
            if (numGestures <= 0)
            {
                GestureNextBtn.SetActive(false);
                GesturePrevBtn.SetActive(false);
                GestureNameText.SetActive(false);
                GestureNameInput.SetActive(false);
                GestureDeleteLastSampleBtn.SetActive(false);
                GestureDeleteAllSamplesBtn.SetActive(false);
                GestureSamplesText.SetActive(false);
                GestureDeleteGestureBtn.SetActive(false);
                return;
            }
            if (this.currentGesture < 0 || this.currentGesture >= numGestures)
                this.currentGesture = 0;
            GestureNextBtn.SetActive(true);
            GesturePrevBtn.SetActive(true);
            GestureNameText.SetActive(true);
            GestureNameInput.SetActive(true);
            GestureDeleteLastSampleBtn.SetActive(true);
            GestureDeleteAllSamplesBtn.SetActive(true);
            GestureSamplesText.SetActive(true);
            GestureDeleteGestureBtn.SetActive(true);
            TextMesh tm = GestureNameText.GetComponent<TextMesh>();
            if (tm != null)
                tm.text = gm.gr.getGestureName(this.currentGesture);
            tm = GestureSamplesText.GetComponent<TextMesh>();
            if (tm != null)
                tm.text = $"{gm.gr.getGestureNumberOfSamples(this.currentGesture)} samples";
            return;
        } else if (gm.gc != null)
        {
            int numParts = gm.gc.numberOfParts();
            if (numParts <= 0)
                return;
            if (this.currentPart < 0 || this.currentPart >= numParts)
                this.currentPart = numParts - 1;
            PartNextBtn.SetActive(true);
            PartPrevBtn.SetActive(true);
            PartText.SetActive(true);
            TextMesh tm = PartText.GetComponent<TextMesh>();
            if (tm != null)
                tm.text = (currentPart == 0) ? "Left (0)" : (currentPart == 1) ? "Right (1)" : $"Part {currentPart+1}";

            GestureCreateBtn.SetActive(true);
            int numGestures = gm.gc.numberOfGestures(this.currentPart);
            if (numGestures <= 0)
            {
                GestureNextBtn.SetActive(false);
                GesturePrevBtn.SetActive(false);
                GestureNameText.SetActive(false);
                GestureNameInput.SetActive(false);
                GestureDeleteLastSampleBtn.SetActive(false);
                GestureDeleteAllSamplesBtn.SetActive(false);
                GestureSamplesText.SetActive(false);
                GestureDeleteGestureBtn.SetActive(false);
                return;
            }
            if (this.currentGesture < 0 || this.currentGesture >= numGestures)
                this.currentGesture = 0;
            GestureNextBtn.SetActive(true);
            GesturePrevBtn.SetActive(true);
            GestureNameText.SetActive(true);
            GestureNameInput.SetActive(true);
            GestureDeleteLastSampleBtn.SetActive(true);
            GestureDeleteAllSamplesBtn.SetActive(true);
            GestureSamplesText.SetActive(true);
            GestureDeleteGestureBtn.SetActive(true);
            tm = GestureNameText.GetComponent<TextMesh>();
            if (tm != null)
                tm.text = gm.gc.getGestureName(this.currentPart, this.currentGesture);
            tm = GestureSamplesText.GetComponent<TextMesh>();
            if (tm != null)
                tm.text = $"{gm.gc.getGestureNumberOfSamples(this.currentPart, this.currentGesture)} samples";
            return;
        }
    }
}
