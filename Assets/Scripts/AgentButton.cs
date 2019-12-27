using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class AgentButton : MonoBehaviour
{
    public GameObject finishButton;

    private Button button;
    private UIDirector uiDirector;
    private GameObject mainPanel;
    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(TaskOnClick);
        uiDirector = GameObject.Find("Canvas").GetComponent<UIDirector>();
        mainPanel = GameObject.Find("MainPanel");
    }

    void Update() {
        if(uiDirector.canPlaceAgent == true) {
            button.interactable = true;
        }
    }
    void TaskOnClick()
    {
        uiDirector.actionStatus = "agent";
        finishButton.SetActive(true);
        mainPanel.SetActive(false);
    }
}
