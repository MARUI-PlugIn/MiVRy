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

public class SubmenuRecordButton : MonoBehaviour, GestureManagerButton
{
    public bool forward;
    public TextMesh gestureNameDisplay;

    [SerializeField] private Material inactiveButtonMaterial;
    [SerializeField] private Material activeButtonMaterial;
    [SerializeField] private SubmenuRecord submenuRecord;


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
            if (this.forward)
            {
                gm.record_gesture_id = (gm.record_gesture_id + 1 >= num_gestures) ? -1 : gm.record_gesture_id + 1;
            } else
            {
                gm.record_gesture_id = (gm.record_gesture_id - 1 < -1) ? num_gestures - 1 : gm.record_gesture_id - 1;
            }
            GestureManagerVR.setSubmenuGesture(gm.record_gesture_id);
            this.submenuRecord.refresh();
        }
        else if (gm.gc != null)
        {
            int num_combinations = gm.gc.numberOfGestureCombinations();
            if (this.forward)
            {
                gm.record_combination_id = (gm.record_combination_id + 1 >= num_combinations) ? -1 : gm.record_combination_id + 1;
            }
            else
            {
                gm.record_combination_id = (gm.record_combination_id - 1 < -1) ? num_combinations - 1 : gm.record_combination_id - 1;
            }
            GestureManagerVR.setSubmenuCombination(gm.record_combination_id);
            this.submenuRecord.refresh();
        }
        GestureManagerVR.setInputFocus(null);
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
