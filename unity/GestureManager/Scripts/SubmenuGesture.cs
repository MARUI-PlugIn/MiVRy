/*
 * MiVRy - 3D gesture recognition library plug-in for Unity.
 * Version 2.14
 * Copyright (c) 2025 MARUI-PlugIn (inc.)
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS 
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, 
 * THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR 
 * PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR 
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, 
 * EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, 
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR 
 * PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY 
 * OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT 
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE 
 * OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using UnityEngine;

public class SubmenuGesture : MonoBehaviour
{
    public static SubmenuGesture me;

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

    private GameObject SampleDisplayText;
    private TextMesh SampleDisplayValue;
    private SubmenuGestureSampleDisplayButton SampleDisplayNextButton;
    private SubmenuGestureSampleDisplayButton SampleDisplayPrevButton;
    private SubmenuGestureSampleDeleteButton SampleDeleteButton;

    private int currentPart = -1;

    public int CurrentPart
    {
        get { return currentPart; }
        set {
            if (value != currentPart) {
                currentPart = value;
                SampleDisplay.sampleId = -1;
                refresh();
            }
        }
    }

    private int currentGesture = -1;

    public int CurrentGesture
    {
        get { return currentGesture; }
        set {
            if (value != currentGesture) {
                currentGesture = value;
                SampleDisplay.sampleId = -1;
                refresh();
            }
        }
    }

    void Start()
    {
        me = this;
        this.init();
        this.refresh();
    }

    private void init()
    {
        for (int i=0; i<this.transform.childCount; i++) {
            GameObject child = this.transform.GetChild(i).gameObject;
            switch (child.name) {
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
                case "SubmenuGestureSampleDisplayText":
                    SampleDisplayText = child;
                    break;
                case "SubmenuGestureSampleDisplayPrevBtn":
                    SampleDisplayPrevButton = child.GetComponent<SubmenuGestureSampleDisplayButton>();
                    break;
                case "SubmenuGestureSampleDisplayValue":
                    SampleDisplayValue = child.GetComponent<TextMesh>();
                    break;
                case "SubmenuGestureSampleDisplayNextBtn":
                    SampleDisplayNextButton = child.GetComponent<SubmenuGestureSampleDisplayButton>();
                    break;
                case "SubmenuGestureDeleteSampleBtn":
                    SampleDeleteButton = child.GetComponent<SubmenuGestureSampleDeleteButton>();
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
        if (gm.gr != null) {
            PartNextBtn.SetActive(false);
            PartPrevBtn.SetActive(false);
            PartText.SetActive(false);
            GestureCreateBtn.SetActive(true);
            int numGestures = gm.gr.numberOfGestures();
            if (numGestures <= 0) {
                GestureNextBtn.SetActive(false);
                GesturePrevBtn.SetActive(false);
                GestureNameText.SetActive(false);
                GestureNameInput.SetActive(false);
                GestureDeleteLastSampleBtn.SetActive(false);
                GestureDeleteAllSamplesBtn.SetActive(false);
                GestureSamplesText.SetActive(false);
                GestureDeleteGestureBtn.SetActive(false);
                SampleDisplayText.SetActive(false);
                SampleDisplayPrevButton.gameObject.SetActive(false);
                SampleDisplayValue.gameObject.SetActive(false);
                SampleDisplayNextButton.gameObject.SetActive(false);
                SampleDeleteButton.gameObject.SetActive(false);
                return;
            }
            if (this.currentGesture < 0 || this.currentGesture >= numGestures) {
                this.currentGesture = 0;
            }
            GestureNextBtn.SetActive(true);
            GesturePrevBtn.SetActive(true);
            GestureNameText.SetActive(true);
            GestureNameInput.SetActive(true);
            GestureDeleteLastSampleBtn.SetActive(true);
            GestureDeleteAllSamplesBtn.SetActive(true);
            GestureSamplesText.SetActive(true);
            GestureDeleteGestureBtn.SetActive(true);
            TextMesh tm = GestureNameText.GetComponent<TextMesh>();
            if (tm != null) {
                tm.text = gm.gr.getGestureName(this.currentGesture);
            }
            int numSamples = gm.gr.getGestureNumberOfSamples(this.currentGesture);
            tm = GestureSamplesText.GetComponent<TextMesh>();
            if (tm != null) {
                tm.text = $"{numSamples} samples";
            }
            if (numSamples <= 0) {
                SampleDisplayText.SetActive(false);
                SampleDisplayPrevButton.gameObject.SetActive(false);
                SampleDisplayValue.gameObject.SetActive(false);
                SampleDisplayNextButton.gameObject.SetActive(false);
                SampleDeleteButton.gameObject.SetActive(false);
            } else {
                SampleDisplayText.SetActive(true);
                SampleDisplayPrevButton.gameObject.SetActive(true);
                SampleDisplayNextButton.gameObject.SetActive(true);
                SampleDisplayValue.gameObject.SetActive(true);
                if (SampleDisplay.sampleId < 0) {
                    SampleDisplayValue.text = $"Off";
                    SampleDeleteButton.gameObject.SetActive(false);
                } else {
                    SampleDeleteButton.gameObject.SetActive(true);
                    SampleDisplayValue.text = $"{SampleDisplay.sampleId}";
                }
            }
            return;
        } else if (gm.gc != null) {
            int numParts = gm.gc.numberOfParts();
            if (numParts <= 0) {
                return;
            }
            if (this.currentPart < 0 || this.currentPart >= numParts) {
                this.currentPart = numParts - 1;
            }
            PartNextBtn.SetActive(true);
            PartPrevBtn.SetActive(true);
            PartText.SetActive(true);
            TextMesh tm = PartText.GetComponent<TextMesh>();
            if (tm != null) {
                tm.text = (currentPart == 0) ? "Left (0)" : (currentPart == 1) ? "Right (1)" : $"Part {currentPart+1}";
            }
            GestureCreateBtn.SetActive(true);
            int numGestures = gm.gc.numberOfGestures(this.currentPart);
            if (numGestures <= 0) {
                GestureNextBtn.SetActive(false);
                GesturePrevBtn.SetActive(false);
                GestureNameText.SetActive(false);
                GestureNameInput.SetActive(false);
                GestureDeleteLastSampleBtn.SetActive(false);
                GestureDeleteAllSamplesBtn.SetActive(false);
                GestureSamplesText.SetActive(false);
                GestureDeleteGestureBtn.SetActive(false);
                SampleDisplayText.SetActive(false);
                SampleDisplayPrevButton.gameObject.SetActive(false);
                SampleDisplayValue.gameObject.SetActive(false);
                SampleDisplayNextButton.gameObject.SetActive(false);
                SampleDeleteButton.gameObject.SetActive(false);
                return;
            }
            if (this.currentGesture < 0 || this.currentGesture >= numGestures) {
                this.currentGesture = 0;
            }
            GestureNextBtn.SetActive(true);
            GesturePrevBtn.SetActive(true);
            GestureNameText.SetActive(true);
            GestureNameInput.SetActive(true);
            GestureDeleteLastSampleBtn.SetActive(true);
            GestureDeleteAllSamplesBtn.SetActive(true);
            GestureSamplesText.SetActive(true);
            GestureDeleteGestureBtn.SetActive(true);
            tm = GestureNameText.GetComponent<TextMesh>();
            if (tm != null) {
                tm.text = gm.gc.getGestureName(this.currentPart, this.currentGesture);
            }
            int numSamples = gm.gc.getGestureNumberOfSamples(this.currentPart, this.currentGesture);
            tm = GestureSamplesText.GetComponent<TextMesh>();
            if (tm != null) {
                tm.text = $"{numSamples} samples";
            }
            if (numSamples <= 0) {
                SampleDisplayText.SetActive(false);
                SampleDisplayPrevButton.gameObject.SetActive(false);
                SampleDisplayValue.gameObject.SetActive(false);
                SampleDisplayNextButton.gameObject.SetActive(false);
                SampleDeleteButton.gameObject.SetActive(false);
            } else {
                SampleDisplayText.SetActive(true);
                SampleDisplayPrevButton.gameObject.SetActive(true);
                SampleDisplayNextButton.gameObject.SetActive(true);
                SampleDisplayValue.gameObject.SetActive(true);
                if (SampleDisplay.sampleId < 0) {
                    SampleDisplayValue.text = $"Off";
                    SampleDeleteButton.gameObject.SetActive(false);
                } else {
                    SampleDeleteButton.gameObject.SetActive(true);
                    SampleDisplayValue.text = $"{SampleDisplay.sampleId}";
                }
            }
            return;
        }
    }
}
