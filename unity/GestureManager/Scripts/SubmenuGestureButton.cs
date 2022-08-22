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

public class SubmenuGestureButton : MonoBehaviour, GestureManagerButton
{
    public TextMesh gestureNameText;
    public TextMesh gestureSamplesText;

    [System.Serializable]
    public enum Operation {
        NextPart,
        PreviousPart,
        CreateGesture,
        DeleteGesture,
        DeleteLastSample,
        DeleteAllSamples,
        NextGesture,
        PreviousGesture
    };
    public Operation operation;

    private SubmenuGesture submenuGesture;

    [SerializeField] private Material inactiveButtonMaterial;
    [SerializeField] private Material activeButtonMaterial;

    // Start is called before the first frame update
    void Start()
    {
        this.submenuGesture = this.transform.parent.gameObject.GetComponent<SubmenuGesture>();
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
        this.GetComponent<Renderer>().material = activeButtonMaterial;
        switch (this.operation)
        {
            case Operation.CreateGesture:
                this.submenuGesture.CurrentGesture = 
                    (gm.gr != null) ? gm.gr.createGesture("New Gesture") :
                    (gm.gc != null) ? gm.gc.createGesture(this.submenuGesture.CurrentPart, "New Gesture") :
                    -1;
                break;
            case Operation.DeleteGesture:
                if (this.submenuGesture.CurrentGesture >= 0)
                {
                    if (gm.gr != null) gm.gr.deleteGesture(this.submenuGesture.CurrentGesture);
                    if (gm.gc != null) gm.gc.deleteGesture(this.submenuGesture.CurrentPart, this.submenuGesture.CurrentGesture);
                    this.submenuGesture.CurrentGesture--;
                    GestureManagerVR.refresh();
                }
                break;
            case Operation.DeleteLastSample:
                if (this.submenuGesture.CurrentGesture >= 0)
                {
                    if (gm.gr != null) {
                        int numSamples = gm.gr.getGestureNumberOfSamples(this.submenuGesture.CurrentGesture);
                        if (numSamples > 0)
                            gm.gr.deleteGestureSample(this.submenuGesture.CurrentGesture, numSamples - 1);
                    } else if (gm.gc != null)
                    {
                        int numSamples = gm.gc.getGestureNumberOfSamples(this.submenuGesture.CurrentPart, this.submenuGesture.CurrentGesture);
                        if (numSamples > 0)
                            gm.gc.deleteGestureSample(this.submenuGesture.CurrentPart, this.submenuGesture.CurrentGesture, numSamples - 1);

                    }
                }
                break;
            case Operation.DeleteAllSamples:
                if (this.submenuGesture.CurrentGesture >= 0)
                {
                    if (gm.gr != null)
                        gm.gr.deleteAllGestureSamples(this.submenuGesture.CurrentGesture);
                    else if (gm.gc != null)
                        gm.gc.deleteAllGestureSamples(this.submenuGesture.CurrentPart, this.submenuGesture.CurrentGesture);
                }
                break;
            case Operation.NextGesture:
                {
                    int numGestures = 
                        (gm.gr != null) ? gm.gr.numberOfGestures() : 
                        (gm.gc != null) ? gm.gc.numberOfGestures(this.submenuGesture.CurrentPart) : 
                        -1;
                    if (numGestures == 0)
                    {
                        this.submenuGesture.CurrentGesture = -1;
                    } else if (this.submenuGesture.CurrentGesture + 1 >= numGestures)
                    {
                        this.submenuGesture.CurrentGesture = 0;
                    } else
                    {
                        this.submenuGesture.CurrentGesture++;
                    }
                    
                }
                break;
            case Operation.PreviousGesture:
                {
                    int numGestures =
                        (gm.gr != null) ? gm.gr.numberOfGestures() :
                        (gm.gc != null) ? gm.gc.numberOfGestures(this.submenuGesture.CurrentPart) :
                        -1;
                    if (numGestures == 0)
                    {
                        this.submenuGesture.CurrentGesture = -1;
                    }
                    else if (this.submenuGesture.CurrentGesture - 1 < 0)
                    {
                        this.submenuGesture.CurrentGesture = numGestures - 1;
                    }
                    else
                    {
                        this.submenuGesture.CurrentGesture--;
                    }

                }
                break;
            case Operation.NextPart:
                {
                    if (gm.gc == null)
                        break;
                    int numParts = gm.gc.numberOfParts();
                    if (numParts == 0)
                        this.submenuGesture.CurrentPart = -1;
                    else if (this.submenuGesture.CurrentPart + 1 >= numParts)
                        this.submenuGesture.CurrentPart = 0;
                    else
                        this.submenuGesture.CurrentPart++;
                }
                break;
            case Operation.PreviousPart:
                {
                    if (gm.gc == null)
                        break;
                    int numParts = gm.gc.numberOfParts();
                    if (numParts == 0)
                        this.submenuGesture.CurrentPart = -1;
                    else if (this.submenuGesture.CurrentPart - 1 < 0)
                        this.submenuGesture.CurrentPart = numParts - 1;
                    else
                        this.submenuGesture.CurrentPart--;
                }
                break;
        }
        GestureManagerVR.setInputFocus(null);
        this.submenuGesture.refresh();
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
