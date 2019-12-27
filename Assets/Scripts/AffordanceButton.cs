using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class AffordanceButton : MonoBehaviour
{
    public GameObject affordanceList;
    private Button button;
    private UIDirector uiDirector;
    private GameObject mainPanel;
    private GameObject finishButton;
    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(TaskOnClick);
        uiDirector = GameObject.Find("Canvas").GetComponent<UIDirector>();
        mainPanel = GameObject.Find("MainPanel");
        finishButton = GameObject.Find("FinishAction");
    }
    void TaskOnClick()
    {
        uiDirector.actionStatus = "affordance";
        affordanceList.SetActive(true);
        mainPanel.SetActive(false);
    }
}
