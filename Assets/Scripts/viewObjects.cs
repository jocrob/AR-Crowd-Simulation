using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class viewObjects : MonoBehaviour
{
    private UIDirector uiDirector;
    private UnityEngine.UI.Toggle toggle;
    void Start()
    {
        toggle = GetComponent<UnityEngine.UI.Toggle>();
        toggle.onValueChanged.AddListener(TaskOnClick);
        uiDirector = GameObject.Find("Canvas").GetComponent<UIDirector>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    void TaskOnClick(bool value) {
        if(value == true) {
            uiDirector.viewObjects = true;
        }
        else {
            uiDirector.viewObjects = false;
        }
    }
}
