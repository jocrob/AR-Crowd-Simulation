using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class setText : MonoBehaviour
{
    private UIDirector uiDirector;
    private UnityEngine.UI.Text textComp;
    void Start()
    {
        uiDirector = GameObject.Find("Canvas").GetComponent<UIDirector>();
        textComp = gameObject.GetComponent<UnityEngine.UI.Text>();
    }

    void Update()
    {
        textComp.text = uiDirector.directionText;
    }
}
