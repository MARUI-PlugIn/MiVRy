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

public class SubmenuTrainingButton : GestureManagerButton
{
    public TextMesh buttonText;
    public SubmenuTrainingSettings submenuTrainingSettings;

    public enum Operation
    {
        StartTraining,
        StopTraining,
        ToggleTraining,
        ToggleSettings,
        ToggleSampleResolution,
        ToggleControllerRotation,
        ToggleRotatePath,
    }
    public Operation operation;


    private void ToggleTrainingParameter(GestureRecognition.TrainingParameter parameter, int min, int max, int step)
    {
        GestureManager gm = GestureManagerVR.me?.gestureManager;
        int value;
        if (gm.gr != null) {
            value = gm.gr.getTrainingParameter(parameter);
        } else if (gm.gc != null) {
            value = gm.gc.getTrainingParameter(parameter);
        } else {
            return;
        }
        if (value == max) {
            value = min;
        } else {
            value += step;
            if (value > max)
                value = max;
        }
        if (gm.gr != null) {
            gm.gr.setTrainingParameter(parameter, value);
        } else if (gm.gc != null) {
            gm.gc.setTrainingParameter(parameter, value);
        } else {
            return;
        }
    }


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
        this.material = activeButtonMaterial;
        switch (this.operation) {
            case Operation.StartTraining:
                if (gm.gr != null) {
                    gm.gr.startTraining();
                } else if (gm.gc != null) {
                    gm.gc.startTraining();
                }
                break;
            case Operation.StopTraining:
                if (gm.gr != null) {
                    gm.gr.stopTraining();
                } else if (gm.gc != null) {
                    gm.gc.stopTraining();
                }
                break;
            case Operation.ToggleTraining:
                if (gm.gr != null) {
                    if (gm.gr.isTraining()) {
                        gm.gr.stopTraining();
                    } else {
                        gm.gr.startTraining();
                    }
                } else if (gm.gc != null) {
                    if (gm.gc.isTraining()) {
                        gm.gc.stopTraining();
                    } else {
                        gm.gc.startTraining();
                    }
                }
                break;
            case Operation.ToggleSettings:
                if (this.submenuTrainingSettings != null) {
                    GameObject go = this.submenuTrainingSettings.gameObject;
                    go.SetActive(!go.activeSelf);
                }
                break;
            case Operation.ToggleSampleResolution:
                this.ToggleTrainingParameter(GestureRecognition.TrainingParameter.TrainingParameter_SampleResolution, -1, 29, 5);
                break;
            case Operation.ToggleControllerRotation:
                this.ToggleTrainingParameter(GestureRecognition.TrainingParameter.TrainingParameter_ControllerRotation, -1, 1, 1);
                break;
            case Operation.ToggleRotatePath:
                this.ToggleTrainingParameter(GestureRecognition.TrainingParameter.TrainingParameter_RotatePath, -1, 1, 1);
                break;
        }
        GestureManagerVR.setInputFocus(null);
        GestureManagerVR.refresh();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.name.EndsWith("pointer") && (Object)GestureManagerVR.activeButton == this)
            GestureManagerVR.activeButton = null;
        this.material = inactiveButtonMaterial;
    }

    private void OnDisable()
    {
        if ((Object)GestureManagerVR.activeButton == this)
            GestureManagerVR.activeButton = null;
        this.material = inactiveButtonMaterial;
    }

    public void refreshText()
    {
        if (buttonText == null)
            return;
        GestureManager gm = GestureManagerVR.me?.gestureManager;
        int value;
        switch (this.operation) {
            case Operation.StartTraining:
            case Operation.StopTraining:
                return;
            case Operation.ToggleSettings:
                if (this.submenuTrainingSettings != null) {
                    GameObject go = this.submenuTrainingSettings.gameObject;
                    buttonText.text = go.activeSelf ? "Settings <" : "Settings >";
                }
                return;
            case Operation.ToggleTraining:
                if (gm.gr != null) {
                    buttonText.text = gm.gr.isTraining() ? "Stop Training" : "Start Training";
                } else if (gm.gc != null) {
                    buttonText.text = gm.gc.isTraining() ? "Stop Training" : "Start Training";
                }
                break;
            case Operation.ToggleSampleResolution:
                if (gm.gr != null) {
                    value = gm.gr.getTrainingParameter(GestureRecognition.TrainingParameter.TrainingParameter_SampleResolution);
                } else if (gm.gc != null) {
                    value = gm.gc.getTrainingParameter(GestureRecognition.TrainingParameter.TrainingParameter_SampleResolution);
                } else {
                    return;
                }
                buttonText.text = (value < 0) ? "Auto" : $"{value}";
                break;
            case Operation.ToggleControllerRotation:
                if (gm.gr != null) {
                    value = gm.gr.getTrainingParameter(GestureRecognition.TrainingParameter.TrainingParameter_ControllerRotation);
                } else if (gm.gc != null) {
                    value = gm.gc.getTrainingParameter(GestureRecognition.TrainingParameter.TrainingParameter_ControllerRotation);
                } else {
                    return;
                }
                buttonText.text = (value < 0) ? "Auto" : (value > 0) ? "On" : "Off";
                break;
            case Operation.ToggleRotatePath:
                if (gm.gr != null) {
                    value = gm.gr.getTrainingParameter(GestureRecognition.TrainingParameter.TrainingParameter_RotatePath);
                } else if (gm.gc != null) {
                    value = gm.gc.getTrainingParameter(GestureRecognition.TrainingParameter.TrainingParameter_RotatePath);
                } else {
                    return;
                }
                buttonText.text = (value < 0) ? "Auto" : (value > 0) ? "On" : "Off";
                break;
        }
    }
}
