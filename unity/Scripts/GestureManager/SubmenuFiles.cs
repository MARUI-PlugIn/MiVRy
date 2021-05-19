using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubmenuFiles : MonoBehaviour
{
    private bool initialized = false;
    GameObject loadFileTextField;
    GameObject saveFileTextField;
    
    
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
                case "SubmenuFilesLoadFileInput":
                    this.loadFileTextField = child;
                    break;
                case "SubmenuFilesSaveFileInput":
                    this.saveFileTextField = child;
                    break;
            }
        }
        this.initialized = true;
    }

    public void refresh()
    {
        if (!this.initialized)
            this.init();
        GestureManagerVR.refreshTextInputs(this.gameObject);
    }


}
