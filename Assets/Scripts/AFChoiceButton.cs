using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class AFChoiceButton : MonoBehaviour
{
    public string afName;
    public GameObject finishButton;

    private GameObject affordanceList;
    private Button button;
    private UIDirector uiDirector;
    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(TaskOnClick);
        uiDirector = GameObject.Find("Canvas").GetComponent<UIDirector>();
        affordanceList = GameObject.Find("AffordanceList");
    }
    void Update() {
        if (uiDirector.groundPlaced == true)
        {
            button.interactable = true;
        }
    }
    void TaskOnClick()
    {
        Debug.Log("clicked");
        uiDirector.affordance = afName;
        finishButton.SetActive(true);
        affordanceList.SetActive(false);
    }
}
