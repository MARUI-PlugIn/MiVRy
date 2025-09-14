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

public class SubmenuCombinationButton : GestureManagerButton
{
    [System.Serializable]
    public enum Operation {
        CreateCombination,
        DeleteCombination,
        NextCombination,
        PreviousCombination,
        NextPart,
        PreviousPart,
        NextGesture,
        PreviousGesture,
        CreatePartsForNewCombinationYes,
        CreatePartsForNewCombinationNo,
    };
    public Operation operation;

    public SubmenuCombination submenuCombination;

    // Start is called before the first frame update
    void Start()
    {
        if (submenuCombination == null) {
            this.submenuCombination = this.transform.parent.gameObject.GetComponent<SubmenuCombination>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        GestureManager gm = GestureManagerVR.me?.gestureManager;
        if (!other.name.EndsWith("pointer") || gm == null || gm.gc == null)
            return;
        if (GestureManagerVR.isGesturing)
            return;
        if (GestureManagerVR.activeButton != null)
            return;
        GestureManagerVR.activeButton = this;
        this.material = activeButtonMaterial;
        switch (this.operation)
        {
            case Operation.CreateCombination:
                this.submenuCombination.CurrentCombination = gm.gc.createGestureCombination("New Combination");
                if (gm.record_combination_id >= 0) {
                    gm.record_combination_id = this.submenuCombination.CurrentCombination;
                }
                this.submenuCombination.SubmenuCreateCombinationParts.SetActive(true);
                break;
            case Operation.CreatePartsForNewCombinationYes:
                SubmenuGesture submenuGesture = Component.FindAnyObjectByType<SubmenuGesture>();
                if (gm.gc != null && this.submenuCombination.CurrentCombination >= 0) {
                    int comboId = this.submenuCombination.CurrentCombination;
                    string comboName = gm.gc.getGestureCombinationName(comboId);
                    for (int part = gm.gc.numberOfParts()-1; part >=0; part--) {
                        int gestureId = gm.gc.createGesture(part, comboName);
                        gm.gc.setCombinationPartGesture(comboId, part, gestureId);
                        if (submenuGesture != null) {
                            if (submenuGesture.CurrentPart < 0  || submenuGesture.CurrentPart == part) {
                                submenuGesture.CurrentPart = part;
                                submenuGesture.CurrentGesture = gestureId;
                                submenuGesture.refresh();
                            }
                        }
                    }
                }
                this.submenuCombination.SubmenuCreateCombinationParts.SetActive(false);
                this.submenuCombination.refresh();

                break;
            case Operation.CreatePartsForNewCombinationNo:
                this.submenuCombination.SubmenuCreateCombinationParts.SetActive(false);
                break;
            case Operation.DeleteCombination:
                if (this.submenuCombination.CurrentCombination >= 0) {
                    gm.gc.deleteGestureCombination(this.submenuCombination.CurrentCombination);
                    this.submenuCombination.CurrentCombination--;
                    if (gm.record_combination_id >= 0) {
                        gm.record_combination_id = this.submenuCombination.CurrentCombination;
                    }
                }
                break;
            case Operation.NextCombination:
                {
                    int numCombinations = gm.gc.numberOfGestureCombinations();
                    if (numCombinations == 0) {
                        this.submenuCombination.CurrentCombination = -1;
                    } else if (this.submenuCombination.CurrentCombination + 1 >= numCombinations) {
                        this.submenuCombination.CurrentCombination = 0;
                    } else {
                        this.submenuCombination.CurrentCombination++;
                    }
                    if (gm.record_combination_id >= 0) {
                        gm.record_combination_id = this.submenuCombination.CurrentCombination;
                    }
                }
                break;
            case Operation.PreviousCombination:
                {
                    int numCombinations = gm.gc.numberOfGestureCombinations();
                    if (numCombinations == 0) {
                        this.submenuCombination.CurrentCombination = -1;
                    } else if (this.submenuCombination.CurrentCombination - 1 < 0) {
                        this.submenuCombination.CurrentCombination = numCombinations - 1;
                    } else {
                        this.submenuCombination.CurrentCombination--;
                    }
                    if (gm.record_combination_id >= 0) {
                        gm.record_combination_id = this.submenuCombination.CurrentCombination;
                    }
                }
                break;
            case Operation.NextPart:
                {
                    int numParts = gm.gc.numberOfParts();
                    if (numParts == 0) {
                        this.submenuCombination.CurrentPart = -1;
                    } else if (this.submenuCombination.CurrentPart + 1 >= numParts) {
                        this.submenuCombination.CurrentPart = 0;
                    } else {
                        this.submenuCombination.CurrentPart++;
                    }
                }
                break;
            case Operation.PreviousPart:
                {
                    int numParts = gm.gc.numberOfParts();
                    if (numParts == 0) {
                        this.submenuCombination.CurrentPart = -1;
                    } else if (this.submenuCombination.CurrentPart - 1 < 0) {
                        this.submenuCombination.CurrentPart = numParts - 1;
                    } else {
                        this.submenuCombination.CurrentPart--;
                    }
                }
                break;
            case Operation.NextGesture:
                {
                    int numGestures = gm.gc.numberOfGestures(this.submenuCombination.CurrentPart);
                    if (numGestures == 0) {
                        this.submenuCombination.CurrentGesture = -1; // -1 = [NONE]
                    } else if (this.submenuCombination.CurrentGesture + 1 >= numGestures) {
                        this.submenuCombination.CurrentGesture = -1; // -1 = [NONE]
                    } else {
                        this.submenuCombination.CurrentGesture++;
                    }
                }
                break;
            case Operation.PreviousGesture:
                {
                    int numGestures = gm.gc.numberOfGestures(this.submenuCombination.CurrentPart);
                    if (numGestures == 0) {
                        this.submenuCombination.CurrentGesture = -1; // -1 = [NONE]
                    } else if (this.submenuCombination.CurrentGesture - 1 < -1) { // -1 = [NONE]
                        this.submenuCombination.CurrentGesture = numGestures - 1;
                    } else {
                        this.submenuCombination.CurrentGesture--;
                    }
                }
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
}
