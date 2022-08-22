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

public class SubmenuCombination : MonoBehaviour
{
    private bool initialized = false;

    private GameObject CombinationCreateBtn;
    private GameObject CombinationDeleteBtn;

    private GameObject CombinationNextBtn;
    private GameObject CombinationPrevBtn;
    private GameObject CombinationText;
    private GameObject CombinationInput;

    private GameObject PartNextBtn;
    private GameObject PartPrevBtn;
    private GameObject PartText;

    private GameObject GestureNextBtn;
    private GameObject GesturePrevBtn;
    private GameObject GestureText;

    private int currentCombination = -1;

    public int CurrentCombination
    {
        get { return currentCombination; }
        set { currentCombination = value; refresh(); }
    }

    private int currentPart = -1;

    public int CurrentPart
    {
        get { return currentPart; }
        set { currentPart = value; refresh(); }
    }

    private int currentGesture = -1;

    public int CurrentGesture
    {
        get { return currentGesture; }
        set {
            GestureManager gm = GestureManagerVR.me?.gestureManager;
            if (gm == null || gm.gc == null)
                return;
            gm.gc.setCombinationPartGesture(this.CurrentCombination, this.CurrentPart, value);
            // currentGesture = value; will be updated in refresh
            refresh();
        }
    }

    void Start()
    {
        this.init();
        this.refresh();
    }


    private void init()
    {
        for (int i=0; i<this.transform.childCount; i++)
        {
            GameObject child = this.transform.GetChild(i).gameObject;
            switch (child.name)
            {
                case "SubmenuCombinationCreateBtn":
                    CombinationCreateBtn = child;
                    break;
                case "SubmenuCombinationDeleteBtn":
                    CombinationDeleteBtn = child;
                    break;

                case "SubmenuCombinationText":
                    CombinationText = child;
                    break;
                case "SubmenuCombinationInput":
                    CombinationInput = child;
                    break;
                case "SubmenuCombinationNextBtn":
                    CombinationNextBtn = child;
                    break;
                case "SubmenuCombinationPrevBtn":
                    CombinationPrevBtn = child;
                    break;

                case "SubmenuCombinationPartText":
                    PartText = child;
                    break;
                case "SubmenuCombinationPartNextBtn":
                    PartNextBtn = child;
                    break;
                case "SubmenuCombinationPartPrevBtn":
                    PartPrevBtn = child;
                    break;

                case "SubmenuCombinationGestureText":
                    GestureText = child;
                    break;
                case "SubmenuCombinationGestureNextBtn":
                    GestureNextBtn = child;
                    break;
                case "SubmenuCombinationGesturePrevBtn":
                    GesturePrevBtn = child;
                    break;
            }
        }
        this.initialized = true;
    }

    public void refresh()
    {
        if (!this.initialized)
            this.init();
        GestureManager gm = GestureManagerVR.me?.gestureManager;
        if (gm == null || gm.gc == null) {
            return;
        }
        int numCombinations = gm.gc.numberOfGestureCombinations();
        if (currentCombination < 0 || currentCombination >= numCombinations)
            currentCombination = numCombinations - 1;
        if (currentCombination < 0)
        {
            CombinationCreateBtn.SetActive(true);
            CombinationDeleteBtn.SetActive(false);

            CombinationNextBtn.SetActive(false);
            CombinationPrevBtn.SetActive(false);
            CombinationInput.SetActive(false);
            CombinationText.SetActive(false);

            PartNextBtn.SetActive(false);
            PartPrevBtn.SetActive(false);
            PartText.SetActive(false);

            GestureNextBtn.SetActive(false);
            GesturePrevBtn.SetActive(false);
            GestureText.SetActive(false);
            return;
        }
        CombinationCreateBtn.SetActive(true);
        CombinationDeleteBtn.SetActive(true);
        string combinationName = gm.gc.getGestureCombinationName(currentCombination);
        if (combinationName.Length > 30)
            combinationName = combinationName.Substring(combinationName.Length - 30);

        CombinationNextBtn.SetActive(true);
        CombinationPrevBtn.SetActive(true);
        CombinationInput.SetActive(true);
        CombinationText.SetActive(true);
        CombinationText.GetComponent<TextMesh>().text = combinationName;

        int numParts = gm.gc.numberOfParts();
        if (currentPart < 0 || currentPart >= numParts)
            currentPart = numParts - 1;
        if (currentPart < 0)
            return;
        string partName = (currentPart == 0) ? "Left (0)" : (currentPart == 1) ? "Right (1)" : $"Part {currentPart}";

        PartNextBtn.SetActive(true);
        PartPrevBtn.SetActive(true);
        PartText.SetActive(true);
        PartText.GetComponent<TextMesh>().text = partName;

        GestureNextBtn.SetActive(true);
        GesturePrevBtn.SetActive(true);
        GestureText.SetActive(true);
        this.currentGesture = gm.gc.getCombinationPartGesture(this.currentCombination, this.currentPart);
        if (currentGesture < 0)
        {
            GestureText.GetComponent<TextMesh>().text = "[None]";
        } else
        {
            string gestureName = gm.gc.getGestureName(currentPart, currentGesture);
            if (gestureName.Length > 30)
                gestureName = gestureName.Substring(gestureName.Length - 30);
            GestureText.GetComponent<TextMesh>().text = gestureName;
        }
    }
}
