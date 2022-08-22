/*
 * MiVRy - 3D gesture recognition library plug-in for Unity.
 * Version 2.5
 * Copyright (c) 2022 MARUI-PlugIn (inc.)
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SubmenuRecordButtonSampleDisplayButton : MonoBehaviour, GestureManagerButton
{
    public bool forward;
    public TextMesh sampleIndexDisplay;

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
            if (gm.record_gesture_id >= num_gestures)
            {
                return; // should not happen (TODO: reset to valid value)
            }
            int numSamples = gm.gr.getGestureNumberOfSamples(gm.record_gesture_id);
            if (this.forward)
            {
                GestureManagerVR.sampleDisplay.sampleId = (GestureManagerVR.sampleDisplay.sampleId + 1 >= numSamples)
                    ? -1
                    : GestureManagerVR.sampleDisplay.sampleId + 1;
            } else {
                GestureManagerVR.sampleDisplay.sampleId = (GestureManagerVR.sampleDisplay.sampleId - 1 < -1)
                    ? numSamples - 1
                    : GestureManagerVR.sampleDisplay.sampleId - 1;
            }
            GestureManagerVR.sampleDisplay.reloadStrokes();
            if (sampleIndexDisplay != null)
            {
                sampleIndexDisplay.text = (GestureManagerVR.sampleDisplay.sampleId < 0)
                    ? "[off]"
                    : $"{GestureManagerVR.sampleDisplay.sampleId}";
            }
        }
        else if (gm.gc != null)
        {
            int num_combinations = gm.gc.numberOfGestureCombinations();
            if (num_combinations <= gm.record_combination_id)
            {
                return; // should not happen (TODO: reset to valid value)
            }
            int numSamples = 0;
            for (int part = gm.gc.numberOfParts()-1; part >=0; part--)
            {
                int partGestureId = gm.gc.getCombinationPartGesture(gm.record_combination_id, part);
                if (partGestureId >= 0)
                {
                    int partNumSamples = gm.gc.getGestureNumberOfSamples(part, partGestureId);
                    numSamples = Math.Max(numSamples, partNumSamples);
                }
            }
            
            if (this.forward)
            {
                GestureManagerVR.sampleDisplay.sampleId = (GestureManagerVR.sampleDisplay.sampleId + 1 >= numSamples)
                    ? -1
                    : GestureManagerVR.sampleDisplay.sampleId + 1;
            } else {
                GestureManagerVR.sampleDisplay.sampleId = (GestureManagerVR.sampleDisplay.sampleId - 1 < -1)
                    ? numSamples - 1
                    : GestureManagerVR.sampleDisplay.sampleId - 1;
            }
            GestureManagerVR.sampleDisplay.reloadStrokes();
            if (sampleIndexDisplay != null)
            {
                sampleIndexDisplay.text = (GestureManagerVR.sampleDisplay.sampleId < 0)
                    ? "[off]"
                    : $"{GestureManagerVR.sampleDisplay.sampleId}";
            }
        }
        GestureManagerVR.setInputFocus(null);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.name.EndsWith("pointer") && (UnityEngine.Object)GestureManagerVR.activeButton == this)
            GestureManagerVR.activeButton = null;
        this.GetComponent<Renderer>().material = inactiveButtonMaterial;
    }

    private void OnDisable()
    {
        if ((UnityEngine.Object)GestureManagerVR.activeButton == this)
            GestureManagerVR.activeButton = null;
        this.GetComponent<Renderer>().material = inactiveButtonMaterial;
    }

}
