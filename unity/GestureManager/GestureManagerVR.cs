/*
 * MiVRy - 3D gesture recognition library plug-in for Unity.
 * Version 2.1
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

using System;
using UnityEngine;

public class GestureManagerVR : MonoBehaviour
{
    public static GestureManagerVR me; // singleton

    public bool followUser = true;

    public GestureManager gestureManager;

    public EditableTextField inputFocus = null;
    public Material inputFocusOnMaterial;
    public Material inputFocusOffMaterial;
    public GameObject keyboard;
    public GameObject splashscreen;

    public GameObject submenuNumberOfParts = null;
    public GameObject submenuFiles = null;
    public GameObject submenuFileSuggestions = null;
    public GameObject submenuGesture = null;
    public GameObject submenuCombination = null;
    public GameObject submenuRecord = null;
    public GameObject submenuCoordinateSystem = null;
    public GameObject submenuFrameOfReference = null;
    public GameObject submenuTraining = null;

    public static GestureManagerButton activeButton = null;

    // Start is called before the first frame update
    void Start()
    {
        me = this;
        inputFocus = null;
        keyboard?.SetActive(false);

        for (int i=0; i<this.transform.childCount; i++)
        {
            GameObject child = this.transform.GetChild(i).gameObject;
            switch (child.name)
            {
                case "SubmenuNumberOfParts":
                    submenuNumberOfParts = child;
                    break;
                case "SubmenuCoordinateSystem":
                    submenuCoordinateSystem = child;
                    break;
                case "SubmenuFiles":
                    submenuFiles = child;
                    break;
                case "SubmenuFileSuggestions":
                    submenuFileSuggestions = child;
                    break;
                case "SubmenuGesture":
                    submenuGesture = child;
                    break;
                case "SubmenuCombination":
                    submenuCombination = child;
                    break;
                case "SubmenuRecord":
                    submenuRecord = child;
                    break;
                case "SubmenuFrameOfReference":
                    submenuFrameOfReference = child;
                    break;
                case "SubmenuTraining":
                    submenuTraining = child;
                    break;
            }
            for (int k=0; k<child.transform.childCount; k++)
            {
                GameObject grandChild = child.transform.GetChild(k).gameObject;
                EditableTextField editableTextField = grandChild.GetComponent<EditableTextField>();
                if (editableTextField != null)
                    editableTextField.refreshText();
            }
        }
        refresh();
    }

    public static void keyboardInput(KeyboardKey key)
    {
        if (me == null || me.inputFocus == null)
            return;
        me.inputFocus.keyboardInput(key);
        if (me.inputFocus.target == EditableTextField.Target.LoadFile)
        {
            me.submenuFileSuggestions.GetComponent<SubmenuFileSuggestions>().refresh();
        }
    }

    public static void setInputFocus(EditableTextField editableTextField)
    {
        if (me == null)
            return;
        if (me.inputFocus != null)
        {
            MeshRenderer meshRenderer = me.inputFocus.gameObject?.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.material = me.inputFocusOffMaterial;
            }
        }
        me.inputFocus = editableTextField;
        if (me.inputFocus != null)
        {
            MeshRenderer meshRenderer = me.inputFocus.gameObject?.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.material = me.inputFocusOnMaterial;
            }
        }
        if (me.keyboard != null)
        {
            if (me.inputFocus == null)
            {
                me.keyboard.SetActive(false);
            } else
            {
                me.keyboard.SetActive(true);
                me.keyboard.transform.rotation = Quaternion.LookRotation(Camera.main.transform.position - me.inputFocus.gameObject.transform.position) * Quaternion.AngleAxis(90.0f, Vector3.right);

                Bounds objBounds = new Bounds(me.inputFocus.gameObject.transform.position, Vector3.zero);
                foreach (Renderer r in me.inputFocus.gameObject.GetComponentsInChildren<Renderer>())
                {
                    objBounds.Encapsulate(r.bounds);
                }
                Bounds keyboardBounds = new Bounds(me.keyboard.transform.position, Vector3.zero);
                foreach (Renderer r in me.keyboard.GetComponentsInChildren<Renderer>())
                {
                    keyboardBounds.Encapsulate(r.bounds);
                }
                me.keyboard.transform.position = new Vector3(
                    objBounds.center.x,
                    objBounds.center.y - (objBounds.extents.y + keyboardBounds.extents.y + 0.05f),
                    objBounds.center.z
                ) + (me.keyboard.transform.up * 0.08f);
            }
        }
    }

    public static void refresh()
    {
        if (me == null)
            return;
        
        if (me.gestureManager.numberOfParts <= 0)
        {
            me.submenuNumberOfParts.SetActive(true);
            me.submenuFiles.SetActive(false);
            me.submenuFileSuggestions.SetActive(false);
            me.submenuGesture.SetActive(false);
            me.submenuCombination.SetActive(false);
            me.submenuRecord.SetActive(false);
            me.submenuCoordinateSystem.SetActive(false);
            me.submenuCoordinateSystem.GetComponent<SubmenuCoordinateSystem>().refresh();
            me.submenuFrameOfReference.SetActive(false);
            me.submenuTraining.SetActive(false);
        } else if (me.gestureManager.numberOfParts == 1)
        {
            me.submenuNumberOfParts.SetActive(true);
            me.submenuFiles.SetActive(true);
            me.submenuFiles.GetComponent<SubmenuFiles>().refresh();
            me.submenuFileSuggestions.SetActive(true);
            me.submenuFileSuggestions.GetComponent<SubmenuFileSuggestions>().refresh();
            me.submenuGesture.SetActive(true);
            me.submenuGesture.GetComponent<SubmenuGesture>().refresh();
            me.submenuCombination.SetActive(false);
            me.submenuRecord.SetActive(true);
            me.submenuRecord.GetComponent<SubmenuRecord>().refresh();
            me.submenuCoordinateSystem.SetActive(true);
            me.submenuCoordinateSystem.GetComponent<SubmenuCoordinateSystem>().refresh();
            me.submenuFrameOfReference.SetActive(true);
            me.submenuFrameOfReference.GetComponent<SubmenuFrameOfReference>().refresh();
            me.submenuTraining.SetActive(true);
            me.submenuTraining.GetComponent<SubmenuTraining>().refresh();
            me.submenuRecord.transform.localPosition = Vector3.forward * 0.135f;
            me.submenuCoordinateSystem.transform.localPosition = Vector3.forward * 0.135f;
            me.submenuFrameOfReference.transform.localPosition = Vector3.forward * 0.135f;
            me.submenuTraining.transform.localPosition = Vector3.forward * 0.135f;
        } else
        {
            me.submenuNumberOfParts.SetActive(true);
            me.submenuFiles.SetActive(true);
            me.submenuFiles.GetComponent<SubmenuFiles>().refresh();
            me.submenuFileSuggestions.SetActive(true);
            me.submenuFileSuggestions.GetComponent<SubmenuFileSuggestions>().refresh();
            me.submenuGesture.SetActive(true);
            me.submenuGesture.GetComponent<SubmenuGesture>().refresh();
            me.submenuCombination.SetActive(true);
            me.submenuCombination.GetComponent<SubmenuCombination>().refresh();
            me.submenuRecord.SetActive(true);
            me.submenuRecord.GetComponent<SubmenuRecord>().refresh();
            me.submenuCoordinateSystem.SetActive(true);
            me.submenuCoordinateSystem.GetComponent<SubmenuCoordinateSystem>().refresh();
            me.submenuFrameOfReference.SetActive(true);
            me.submenuFrameOfReference.GetComponent<SubmenuFrameOfReference>().refresh();
            me.submenuTraining.SetActive(true);
            me.submenuTraining.GetComponent<SubmenuTraining>().refresh();
            me.submenuRecord.transform.localPosition = Vector3.zero;
            me.submenuCoordinateSystem.transform.localPosition = Vector3.zero;
            me.submenuFrameOfReference.transform.localPosition = Vector3.zero;
            me.submenuTraining.transform.localPosition = Vector3.zero;
        }
    }

    public static void refreshTextInputs(GameObject go)
    {
        for (int i=0; i<go.transform.childCount; i++)
        {
            GameObject child = go.transform.GetChild(i).gameObject;
            EditableTextField field = child.GetComponent<EditableTextField>();
            if (field != null)
                field.refreshText();
        }
    }

    private void Update()
    {
        if (Camera.main == null || Camera.main.transform.position == Vector3.zero)
        {
            splashscreen?.SetActive(true);
        } else
        {
            if (splashscreen != null && splashscreen.activeSelf) splashscreen.SetActive(false);
            if (followUser)
            {
                Vector3 v = Camera.main.transform.worldToLocalMatrix.MultiplyPoint3x4(this.transform.position);
                if (v.magnitude > 0.6f || v.z < 0)
                {
                    v = new Vector3(0, 0, 0.5f);
                    v = Camera.main.transform.localToWorldMatrix.MultiplyPoint3x4(v);
                    this.transform.position = 0.9f * this.transform.position + 0.1f * v;
                }
            }
        }
    }

    public static bool isGesturing
    {
        get {
            if (me == null || me.gestureManager == null)
                return false;
            return me.gestureManager.gesture_started;
        }
    }

    public static bool setSubmenuGesture(int gesture_id)
    {
        if (me == null || me.submenuGesture == null)
            return false;
        GestureManagerVR.me.submenuGesture.GetComponent<SubmenuGesture>().CurrentGesture = gesture_id;
        GestureManagerVR.me.submenuGesture.GetComponent<SubmenuGesture>().refresh();
        return true;
    }

    public static bool setSubmenuCombination(int combination_id, int part=-1, int gesture_id=-1)
    {
        if (me == null || me.submenuCombination == null || me.gestureManager == null || me.gestureManager.gc == null)
            return false;
        if (gesture_id<0)
        {
            for (part = me.gestureManager.gc.numberOfParts()-1; part >=0 ; part--)
            {
                gesture_id = me.gestureManager.gc.getCombinationPartGesture(combination_id, part);
                if (gesture_id >= 0)
                    break;
            }
        }
        GestureManagerVR.me.submenuCombination.GetComponent<SubmenuCombination>().CurrentCombination = combination_id;
        GestureManagerVR.me.submenuCombination.GetComponent<SubmenuCombination>().CurrentPart = part;
        GestureManagerVR.me.submenuCombination.GetComponent<SubmenuCombination>().CurrentGesture = gesture_id;
        GestureManagerVR.me.submenuCombination.GetComponent<SubmenuCombination>().refresh();
        if (me.submenuGesture != null)
        {
            GestureManagerVR.me.submenuGesture.GetComponent<SubmenuGesture>().CurrentGesture = gesture_id;
            GestureManagerVR.me.submenuGesture.GetComponent<SubmenuGesture>().CurrentPart = part;
            GestureManagerVR.me.submenuGesture.GetComponent<SubmenuGesture>().refresh();
        }
        return true;
    }
}
