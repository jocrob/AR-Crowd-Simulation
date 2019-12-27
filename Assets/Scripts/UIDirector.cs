using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class UIDirector : MonoBehaviour
{
    public string actionStatus;
    public string affordance;
    public string directionText;
    public bool canPlaceAgent;
    public bool groundPlaced;
    public bool viewObjects;
    public GameObject toggle;
    void Start()
    {
        actionStatus = "interact";
        canPlaceAgent = false;
        groundPlaced = false;
        viewObjects = false;
        directionText = "";
    }

    // Update is called once per frame
    void Update()
    {
        if(actionStatus == "agent") {
            toggle.SetActive(true);
        }
        
        if(actionStatus == "interact") {
            viewObjects = false;
            toggle.GetComponent<Toggle>().isOn = false;
            toggle.SetActive(false);
        }
    }
}
