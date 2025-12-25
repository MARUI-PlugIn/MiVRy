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

public class SubmenuGestureSampleDeleteButton : GestureManagerButton
{
    public TextMesh sampleIndexDisplay;

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
        int gestureId = SubmenuGesture.me.CurrentGesture;
        if (gestureId < 0) {
            return;
        }
        if (gm.gr != null) {
            int numGestures = gm.gr.numberOfGestures();
            if (gestureId >= numGestures) {
                return; // should not happen
            }
            int numSamples = gm.gr.getGestureNumberOfSamples(gestureId);
            if (SampleDisplay.sampleId >= numSamples) {
                return;
            }
            int ret = gm.gr.deleteGestureSample(gestureId, SampleDisplay.sampleId);
            if (ret != 0) {
                gm.consoleText = "Failed to delete sample";
                return;
            }
            SampleDisplay.sampleId--;
            if (sampleIndexDisplay != null) {
                sampleIndexDisplay.text = (SampleDisplay.sampleId < 0)
                    ? "Off"
                    : $"{SampleDisplay.sampleId}";
            }
        } else if (gm.gc != null) {
            int part = SubmenuGesture.me.CurrentPart;
            if (part < 0 || part >= gm.gc.numberOfParts()) {
                return;
            }
            int numGestures = gm.gc.numberOfGestures(part);
            if (gestureId >= numGestures) {
                return;
            }
            int numSamples = gm.gc.getGestureNumberOfSamples(part, gestureId);
            if (SampleDisplay.sampleId >= numSamples) {
                return;
            }
            int ret = gm.gc.deleteGestureSample(part, gestureId, SampleDisplay.sampleId);
            if (ret != 0) {
                gm.consoleText = "Failed to delete sample";
                return;
            }
            SampleDisplay.sampleId--;
            if (sampleIndexDisplay != null) {
                sampleIndexDisplay.text = (SampleDisplay.sampleId < 0)
                    ? "Off"
                    : $"{SampleDisplay.sampleId}";
            }
        }
        GestureManagerVR.setInputFocus(null);
        if (SampleDisplay.sampleId < 0) {
            this.gameObject.SetActive(false);
        }
        SubmenuGesture submenuGesture = Component.FindAnyObjectByType<SubmenuGesture>();
        if (submenuGesture != null) {
            submenuGesture.refresh();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.name.EndsWith("pointer") && (UnityEngine.Object)GestureManagerVR.activeButton == this)
            GestureManagerVR.activeButton = null;
        this.material = inactiveButtonMaterial;
    }

    private void OnDisable()
    {
        if ((UnityEngine.Object)GestureManagerVR.activeButton == this)
            GestureManagerVR.activeButton = null;
        this.material = inactiveButtonMaterial;
    }
}
