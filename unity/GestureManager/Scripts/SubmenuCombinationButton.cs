using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubmenuCombinationButton : MonoBehaviour
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
    };
    public Operation operation;

    private SubmenuCombination submenuCombination;

    // Start is called before the first frame update
    void Start()
    {
        this.submenuCombination = this.transform.parent.gameObject.GetComponent<SubmenuCombination>();
    }

    private void OnTriggerEnter(Collider other)
    {
        GestureManager gm = GestureManagerVR.me?.gestureManager;
        if (!other.name.EndsWith("pointer") || gm == null || gm.gc == null)
            return;
        if (GestureManagerVR.isGesturing)
            return;
        switch (this.operation)
        {
            case Operation.CreateCombination:
                this.submenuCombination.CurrentCombination = gm.gc.createGestureCombination("New Combination");
                break;
            case Operation.DeleteCombination:
                if (this.submenuCombination.CurrentCombination >= 0)
                {
                    gm.gc.deleteGestureCombination(this.submenuCombination.CurrentCombination);
                    this.submenuCombination.CurrentCombination--;
                    GestureManagerVR.refresh();
                }
                break;
            case Operation.NextCombination:
                {
                    int numCombinations = gm.gc.numberOfGestureCombinations();
                    if (numCombinations == 0)
                    {
                        this.submenuCombination.CurrentCombination = -1;
                    }
                    else if (this.submenuCombination.CurrentCombination + 1 >= numCombinations)
                    {
                        this.submenuCombination.CurrentCombination = 0;
                    }
                    else
                    {
                        this.submenuCombination.CurrentCombination++;
                    }

                }
                break;
            case Operation.PreviousCombination:
                {
                    int numCombinations = gm.gc.numberOfGestureCombinations();
                    if (numCombinations == 0)
                    {
                        this.submenuCombination.CurrentCombination = -1;
                    }
                    else if (this.submenuCombination.CurrentCombination - 1 < 0)
                    {
                        this.submenuCombination.CurrentCombination = numCombinations - 1;
                    }
                    else
                    {
                        this.submenuCombination.CurrentCombination--;
                    }
                }
                break;
            case Operation.NextPart:
                {
                    int numParts = gm.gc.numberOfParts();
                    if (numParts == 0)
                    {
                        this.submenuCombination.CurrentPart = -1;
                    }
                    else if (this.submenuCombination.CurrentPart + 1 >= numParts)
                    {
                        this.submenuCombination.CurrentPart = 0;
                    }
                    else
                    {
                        this.submenuCombination.CurrentPart++;
                    }

                }
                break;
            case Operation.PreviousPart:
                {
                    int numParts = gm.gc.numberOfParts();
                    if (numParts == 0)
                    {
                        this.submenuCombination.CurrentPart = -1;
                    }
                    else if (this.submenuCombination.CurrentPart - 1 < 0)
                    {
                        this.submenuCombination.CurrentPart = numParts - 1;
                    }
                    else
                    {
                        this.submenuCombination.CurrentPart--;
                    }
                }
                break;
            case Operation.NextGesture:
                {
                    int numGestures = gm.gc.numberOfGestures(this.submenuCombination.CurrentPart);
                    if (numGestures == 0)
                    {
                        this.submenuCombination.CurrentGesture = -1; // -1 = [NONE]
                    } else if (this.submenuCombination.CurrentGesture + 1 >= numGestures)
                    {
                        this.submenuCombination.CurrentGesture = -1; // -1 = [NONE]
                    } else
                    {
                        this.submenuCombination.CurrentGesture++;
                    }
                }
                break;
            case Operation.PreviousGesture:
                {
                    int numGestures = gm.gc.numberOfGestures(this.submenuCombination.CurrentPart);
                    if (numGestures == 0)
                    {
                        this.submenuCombination.CurrentGesture = -1; // -1 = [NONE]
                    }
                    else if (this.submenuCombination.CurrentGesture - 1 < -1) // -1 = [NONE]
                    {
                        this.submenuCombination.CurrentGesture = numGestures - 1;
                    }
                    else
                    {
                        this.submenuCombination.CurrentGesture--;
                    }
                }
                break;
        }
        GestureManagerVR.setInputFocus(null);
        this.submenuCombination.refresh();
    }
}
