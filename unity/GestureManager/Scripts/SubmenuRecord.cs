﻿/*
 * MiVRy - 3D gesture recognition library plug-in for Unity.
 * Version 2.12
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

public class SubmenuRecord : MonoBehaviour
{
    private bool initialized = false;
    private SubmenuRecordRecordButton recordButton;
    private TextMesh SubmenuRecordSampleDisplayValue;

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
                case "SubmenuRecordRecordBtn":
                    this.recordButton = child.GetComponent<SubmenuRecordRecordButton>();
                    break;
                case "SubmenuRecordSampleDisplayValue":
                    this.SubmenuRecordSampleDisplayValue = child.GetComponent<TextMesh>();
                    break;
            }
        }
        this.initialized = true;
    }

    public void refresh()
    {
        if (!this.initialized)
            this.init();
        this.recordButton.refresh();
        GestureManager gm = GestureManagerVR.me?.gestureManager;
        if (gm == null)
            return;
        if (gm.gr != null) {
            int num_gestures = gm.gr.numberOfGestures();
            if (gm.record_gesture_id >= num_gestures) {
                gm.record_gesture_id = num_gestures - 1;
            }
            if (gm.record_gesture_id < 0) {
                this.SubmenuRecordSampleDisplayValue.text = "[off]";
                GestureManagerVR.sampleDisplay.sampleId = -1;
            } else {
                int numSamples = gm.gr.getGestureNumberOfSamples(gm.record_gesture_id);
                if (GestureManagerVR.sampleDisplay.sampleId >= numSamples) {
                    GestureManagerVR.sampleDisplay.sampleId = numSamples - 1;
                }
                if (GestureManagerVR.sampleDisplay.sampleId >= 0) {
                    this.SubmenuRecordSampleDisplayValue.text = $"{GestureManagerVR.sampleDisplay.sampleId}";
                } else {
                    this.SubmenuRecordSampleDisplayValue.text = "[off]";
                }
            }
        }
        else if (gm.gc != null)
        {
            int num_combinations = gm.gc.numberOfGestureCombinations();
            if (gm.record_combination_id >= num_combinations)
                gm.record_combination_id = num_combinations - 1;
            if (gm.record_combination_id < 0) {
                this.SubmenuRecordSampleDisplayValue.text = "[off]";
                GestureManagerVR.sampleDisplay.sampleId = -1;
            } else {
                int numSamples = 0;
                for (int part = gm.gc.numberOfParts() - 1; part >=0; part--) {
                    int partGestureId = gm.gc.getCombinationPartGesture(gm.record_combination_id, part);
                    if (partGestureId >= 0) {
                        int partNumSamples = gm.gc.getGestureNumberOfSamples(part, partGestureId);
                        numSamples = Mathf.Max(numSamples, partNumSamples);
                    }
                }
                if (GestureManagerVR.sampleDisplay.sampleId >= numSamples) {
                    GestureManagerVR.sampleDisplay.sampleId = numSamples - 1;
                }
                if (GestureManagerVR.sampleDisplay.sampleId >= 0) {
                    this.SubmenuRecordSampleDisplayValue.text = $"{GestureManagerVR.sampleDisplay.sampleId}";
                } else {
                    this.SubmenuRecordSampleDisplayValue.text = "[off]";
                }
            }
        } else {
            this.SubmenuRecordSampleDisplayValue.text = "[off]";
            GestureManagerVR.sampleDisplay.sampleId = -1;
        }
        GestureManagerVR.sampleDisplay.reloadStrokes();
    }
}
