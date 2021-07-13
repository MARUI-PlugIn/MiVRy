using System;
using UnityEngine;

public class GestureManagerVR : MonoBehaviour
{
    public static GestureManagerVR me; // singleton
    
    public GestureManager gestureManager;

    public EditableTextField inputFocus = null;
    public Material inputFocusOnMaterial;
    public Material inputFocusOffMaterial;
    public GameObject keyboard;

    private GameObject submenuNumberOfParts = null;
    private GameObject submenuFiles = null;
    private GameObject submenuGesture = null;
    private GameObject submenuCombination = null;
    private GameObject submenuRecord = null;
    private GameObject submenuTraining = null;

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
                case "SubmenuFiles":
                    submenuFiles = child;
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
            me.submenuGesture.SetActive(false);
            me.submenuCombination.SetActive(false);
            me.submenuRecord.SetActive(false);
            me.submenuTraining.SetActive(false);
        } else if (me.gestureManager.numberOfParts == 1)
        {
            me.submenuNumberOfParts.SetActive(true);
            me.submenuFiles.SetActive(true);
            me.submenuFiles.GetComponent<SubmenuFiles>().refresh();
            me.submenuGesture.SetActive(true);
            me.submenuGesture.GetComponent<SubmenuGesture>().refesh();
            me.submenuCombination.SetActive(false);
            me.submenuRecord.SetActive(true);
            me.submenuRecord.GetComponent<SubmenuRecord>().refresh();
            me.submenuTraining.SetActive(true);
            me.submenuTraining.GetComponent<SubmenuTraining>().refresh();
            me.submenuRecord.transform.localPosition = Vector3.forward * 0.135f;
            me.submenuTraining.transform.localPosition = Vector3.forward * 0.135f;
        } else
        {
            me.submenuNumberOfParts.SetActive(true);
            me.submenuFiles.SetActive(true);
            me.submenuFiles.GetComponent<SubmenuFiles>().refresh();
            me.submenuGesture.SetActive(true);
            me.submenuGesture.GetComponent<SubmenuGesture>().refesh();
            me.submenuCombination.SetActive(true);
            me.submenuCombination.GetComponent<SubmenuCombination>().refresh();
            me.submenuRecord.SetActive(true);
            me.submenuRecord.GetComponent<SubmenuRecord>().refresh();
            me.submenuTraining.SetActive(true);
            me.submenuTraining.GetComponent<SubmenuTraining>().refresh();
            me.submenuRecord.transform.localPosition = Vector3.zero;
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
        if (Camera.main != null)
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

    public static bool isGesturing
    {
        get {
            if (me == null || me.gestureManager == null)
                return false;
            return me.gestureManager.gesture_started;
        }
    }
}
