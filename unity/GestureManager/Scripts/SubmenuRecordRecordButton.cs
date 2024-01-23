/*
 * MiVRy - 3D gesture recognition library plug-in for Unity.
 * Version 2.10
 * Copyright (c) 2024 MARUI-PlugIn (inc.)
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

public class SubmenuRecordRecordButton : GestureManagerButton
{
    public TextMesh buttonText;

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
        if (gm.gr != null) {
            if (gm.record_gesture_id < 0) {
                gm.record_gesture_id = GestureManagerVR.getSubmenuGesture();
            } else {
                gm.record_gesture_id = -1;
            }
            GestureManagerVR.refresh();
        } else if (gm.gc != null) {
            if (gm.record_combination_id < 0) {
                gm.record_combination_id = GestureManagerVR.getSubmenuCombination();
            } else {
                gm.record_combination_id = -1;
            }
            GestureManagerVR.refresh();
        }
        GestureManagerVR.setInputFocus(null);
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

    public void refresh()
    {
        GestureManager gm = GestureManagerVR.me?.gestureManager;
        if (gm == null) {
            return;
        }
        if (gm.gr != null) {
            buttonText.text = (gm.record_gesture_id < 0) ? "Off" : "On";
        } else if (gm.gc != null) {
            buttonText.text = (gm.record_combination_id < 0) ? "Off" : "On";
        }
    }
}
