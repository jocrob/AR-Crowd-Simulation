using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class FinishButton : MonoBehaviour
{
    public GameObject mainPanel;
    private Button button;
    private UIDirector uiDirector;
    
    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(TaskOnClick);
        uiDirector = GameObject.Find("Canvas").GetComponent<UIDirector>();
    }
    void TaskOnClick()
    {
        uiDirector.actionStatus = "interact";
        mainPanel.SetActive(true);
        gameObject.SetActive(false);
    }
}
