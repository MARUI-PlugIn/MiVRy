/*
 * MiVRy - 3D gesture recognition library plug-in for Unity.
 * Version 2.13
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

public class SubmenuTraining : MonoBehaviour
{
    private bool initialized = false;
    public TextMesh TrainingCurrentStatus;
    public TextMesh TrainingCurrentPerformance;
    public SubmenuTrainingButton TrainingToggleButton; 
    public SubmenuTrainingButton SettingsToggleButton;
    public SubmenuTrainingSettings SubmenuTrainingSettings;

    void Start()
    {
        this.init();
    }

    private void init()
    {
        for (int i=0; i<this.transform.childCount; i++) {
            GameObject child = this.transform.GetChild(i).gameObject;
            switch (child.name) {
                case "SubmenuTrainingCurrentStatus":
                    TrainingCurrentStatus = child.GetComponent<TextMesh>();
                    break;
                case "SubmenuTrainingCurrentPerformance":
                    TrainingCurrentPerformance = child.GetComponent<TextMesh>();
                    break;
                case "SubmenuTrainingStartStopBtn":
                    TrainingToggleButton = child.GetComponent<SubmenuTrainingButton>();
                    break;
                case "SubmenuTrainingSettingsBtn":
                    SettingsToggleButton = child.GetComponent<SubmenuTrainingButton>();
                    break;
            }
        }
        if (SubmenuTrainingSettings != null) {
            SubmenuTrainingSettings.gameObject.SetActive(false);
        }
        this.initialized = true;
    }

    public void refresh()
    {
        if (!this.initialized)
            this.init();
        if (GestureManagerVR.me == null || GestureManagerVR.me.gestureManager == null)
            return;
        if (GestureManagerVR.me.gestureManager.numberOfParts == 1) {
            this.transform.localPosition = Vector3.forward * 0.135f;
            if (SubmenuTrainingSettings != null) {
                SubmenuTrainingSettings.transform.localPosition = Vector3.forward * 0.135f;
            }
        } else {
            this.transform.localPosition = Vector3.zero;
            if (SubmenuTrainingSettings != null) {
                SubmenuTrainingSettings.transform.localPosition = Vector3.zero;
            }
        }
        double score = 0;
        if (GestureManagerVR.me.gestureManager.gr != null) {
            if (GestureManagerVR.me.gestureManager.gr.isTraining() || GestureManagerVR.me.gestureManager.gr.isLoading()) {
                TrainingCurrentStatus.text = "yes";
                score = GestureManagerVR.me.gestureManager.last_performance_report;
            } else {
                TrainingCurrentStatus.text = "no";
                score = GestureManagerVR.me.gestureManager.gr.recognitionScore();
            }
        } else if (GestureManagerVR.me.gestureManager.gc != null) {
            if (GestureManagerVR.me.gestureManager.gc.isTraining() || GestureManagerVR.me.gestureManager.gc.isLoading()) {
                TrainingCurrentStatus.text = "yes";
                score = GestureManagerVR.me.gestureManager.last_performance_report;
            } else {
                TrainingCurrentStatus.text = "no";
                score = GestureManagerVR.me.gestureManager.gc.recognitionScore();
            }
        }
        score *= 100.0;
        TrainingCurrentPerformance.text = score.ToString("0.00") + "%";

        if (TrainingToggleButton != null) {
            TrainingToggleButton.refreshText();
        }
        if (SettingsToggleButton != null) {
            SettingsToggleButton.refreshText();
        }
        if (SubmenuTrainingSettings != null && SubmenuTrainingSettings.gameObject.activeSelf) {
            SubmenuTrainingSettings.refresh();
        }
    }
}
