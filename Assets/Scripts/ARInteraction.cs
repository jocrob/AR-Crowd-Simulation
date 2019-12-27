using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class ARInteraction : MonoBehaviour
{
    NavMeshDataInstance navMeshDataInstance;
    public GameObject navPlane;
    public GameObject entrance;
    public GameObject chair;
    public GameObject agent;
    public GameObject wanderPoint;
    public GameObject behaviorObj;
    public GameObject placementIndicator;
    public GameObject canvas;
    public Material transparentMat;
    public Material affordanceMat;
    public Material arrow;
    public Material selectedArrow;
    public int crowdSize;
    public int agentCount;
    public List<GameObject> chairs = new List<GameObject>();

    private ARSessionOrigin arOrigin;
    private ARRaycastManager rcManager;
    private Pose placementPose;         //Data structure describing the position and rotation of a 3d point
    private bool placementPoseIsValid = false;
    private UIDirector uiDirector;
    private bool agentPlaced = false;
    private NavMeshBuildSettings buildSettings = new NavMeshBuildSettings();
    private GameObject scaleHandle; 
    private bool entrancePlaced = false;
    private List<GameObject> placedNavs = new List<GameObject>();
    private List<GameObject> entrances = new List<GameObject>();
    private List<GameObject> placedAgents = new List<GameObject>();
    private int touchid;

    void Start()
    {
        agentCount = 0;
        uiDirector = canvas.GetComponent<UIDirector>();

        buildSettings.minRegionArea = 0.1f;
        buildSettings.agentRadius = 0.1f;
        buildSettings.agentHeight = 1f;

        arOrigin = FindObjectOfType<ARSessionOrigin>();
        rcManager = arOrigin.GetComponent<ARRaycastManager>();
        placementIndicator.SetActive(false);
    }

    void FixedUpdate()
    {
        if(uiDirector.actionStatus == "affordance") {

            if (uiDirector.affordance == "navigation")
            {
                //Rendering placed affordances
                if (placedNavs.Count != 0)
                {
                    foreach (GameObject plane in placedNavs)
                    {
                        plane.transform.GetChild(0).GetComponent<Renderer>().material = affordanceMat;
                        for (int i = 1; i <= 4; i++)
                        {
                            plane.transform.GetChild(i).gameObject.SetActive(true);
                        }
                    }
                }

                navInteraction();
            }
            else if (uiDirector.affordance == "entrance")
            {
                if (entrances.Count != 0)
                {
                    foreach (GameObject e in entrances)
                    {
                        e.GetComponent<Renderer>().material = affordanceMat;
                        e.transform.GetChild(0).gameObject.SetActive(true);
                    }
                }

                entranceInteraction();
            }
            else if (uiDirector.affordance == "chair") {
                uiDirector.directionText = "Detect a plane on the seat of a chair to place it";

                if (chairs.Count != 0)
                {
                    foreach (GameObject c in chairs)
                    {
                        GameObject chairIndicator = c.transform.GetChild(0).gameObject;
                        chairIndicator.GetComponent<Renderer>().material = affordanceMat;
                        chairIndicator.transform.GetChild(0).gameObject.SetActive(true);
                    }
                }

                chairInteraction();
            }
        }
        else {
            uiDirector.directionText = "";
            placementIndicator.SetActive(false);

            if(uiDirector.actionStatus == "interact") {     //Crowd simulation isn't being ran
                CancelInvoke();
                behaviorObj.SetActive(false);
                foreach (GameObject a in placedAgents) {
                    Destroy(a);
                }
                placedAgents.Clear();
                agentCount = 0;
            }

            if(uiDirector.affordance != null) {             //Environment was altered (builds navmesh and sets affordance objects to transparent)
                BuildNavMesh();

                if (placedNavs.Count != 0)
                {
                    foreach (GameObject plane in placedNavs)
                    {
                        plane.transform.GetChild(0).GetComponent<Renderer>().material = transparentMat;
                        for (int i = 1; i <= 5; i++)
                        {
                            plane.transform.GetChild(i).gameObject.SetActive(false);
                        }
                    }
                }
                if(entrances.Count != 0) {
                    foreach (GameObject e in entrances) {
                        e.GetComponent<Renderer>().material = transparentMat;
                        e.transform.GetChild(0).gameObject.SetActive(false);
                    }

                    uiDirector.canPlaceAgent = true;
                }
                else {
                    uiDirector.canPlaceAgent = false;
                }

                if(chairs.Count != 0) {
                    foreach(GameObject c in chairs) {
                        GameObject chairIndicator = c.transform.GetChild(0).gameObject;
                        chairIndicator.GetComponent<Renderer>().material = transparentMat;
                        chairIndicator.transform.GetChild(0).gameObject.SetActive(false);
                    }
                }

                uiDirector.affordance = null;
            }

            if (uiDirector.actionStatus == "agent")         //crowd simulation was started
            {
                InvokeRepeating("PlaceAgent", 1f, 2f);
                behaviorObj.SetActive(true);
                uiDirector.actionStatus = null;
            }

            if (uiDirector.viewObjects == true)
            {
                foreach (GameObject plane in placedNavs)
                {
                    plane.transform.GetChild(0).GetComponent<Renderer>().material = affordanceMat;
                }

                foreach (GameObject e in entrances)
                {
                    e.GetComponent<Renderer>().material = affordanceMat;
                }

                if (chairs.Count != 0)
                {
                    foreach (GameObject c in chairs)
                    {
                        GameObject chairIndicator = c.transform.GetChild(0).gameObject;
                        chairIndicator.GetComponent<Renderer>().material = affordanceMat;
                    }
                }
            }
            else {
                foreach (GameObject plane in placedNavs)
                {
                    plane.transform.GetChild(0).GetComponent<Renderer>().material = transparentMat;
                }

                foreach (GameObject e in entrances)
                {
                    e.GetComponent<Renderer>().material = transparentMat;
                }

                if (chairs.Count != 0)
                {
                    foreach (GameObject c in chairs)
                    {
                        GameObject chairIndicator = c.transform.GetChild(0).gameObject;
                        chairIndicator.GetComponent<Renderer>().material = transparentMat;
                    }
                }
            }
        }
    }

    private GameObject PlaceObject(GameObject objectToPlace)
    {
        return Instantiate(objectToPlace, placementPose.position, placementPose.rotation);
    }

    private void PlaceAgent() {
        if(agentCount < crowdSize) {
            Vector3 startPos = entrances[0].transform.position;
            agentCount++;
            GameObject agentInstance = Instantiate(agent, startPos, entrances[0].transform.rotation);
            agentInstance.GetComponent<NavMeshAgent>().Warp(startPos);
            placedAgents.Add(agentInstance);
        }
        else {
            CancelInvoke();
        }
    }

    private void shuffleList<T>(List<T> list) {
        System.Random rnd = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rnd.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    private void UpdatePlacementIndicator(Vector3 size)
    {
        if (placementPoseIsValid)
        {
            placementIndicator.SetActive(true);

            placementIndicator.transform.SetPositionAndRotation(placementPose.position, placementPose.rotation);
            placementIndicator.transform.localScale = new Vector3(size.x, 1, size.z);
        }
        else
        {
            placementIndicator.SetActive(false);
        }
    }

    private void UpdatePlacementPose()
    {
        var screenCenter = Camera.current.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
        var hits = new List<ARRaycastHit>();
        rcManager.Raycast(screenCenter, hits, TrackableType.Planes);   

        placementPoseIsValid = hits.Count > 0;
        if (placementPoseIsValid)
        {
            placementPose = hits[0].pose;

            var cameraForward = Camera.current.transform.forward;
            var cameraBearing = new Vector3(cameraForward.x, 0, cameraForward.z).normalized;
            placementPose.rotation = Quaternion.LookRotation(cameraBearing);
        }
    }

    private void UpdatePlacementPoseVirtual()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Camera.current.ViewportToScreenPoint(new Vector3(0.5f, 0.5f)));

        if (Physics.Raycast(ray, out hit))
        {
            placementPose.position = hit.point;
            var cameraForward = Camera.current.transform.forward;
            var cameraBearing = new Vector3(cameraForward.x, 0, cameraForward.z).normalized;
            placementPose.rotation = Quaternion.LookRotation(cameraBearing);

            placementPoseIsValid = true;
        }
        else {
            placementPoseIsValid = false;
        }
    }

    private GameObject getVirtualObject(Vector3 screenPoint)
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(screenPoint);
        if (Physics.Raycast(ray, out hit)) {
            return hit.transform.gameObject;
        }

        return null;
    }

    private void BuildNavMesh()
    {

        // delete the existing navmesh if there is one
        navMeshDataInstance.Remove();

        List<NavMeshBuildSource> buildSources = new List<NavMeshBuildSource>();

        UnityEngine.AI.NavMeshBuilder.CollectSources(
            new Bounds(Vector3.zero, new Vector3(10000, 10000, 10000)),
            1,
            NavMeshCollectGeometry.RenderMeshes,
            0,
            new List<NavMeshBuildMarkup>(),
            buildSources);

        NavMeshData navData = UnityEngine.AI.NavMeshBuilder.BuildNavMeshData(
            buildSettings,
            buildSources,
            new Bounds(Vector3.zero, new Vector3(10000, 10000, 10000)),
            Vector3.down,
            Quaternion.Euler(Vector3.up));

        navMeshDataInstance = NavMesh.AddNavMeshData(navData);
    }

    private void navInteraction() {
        
        if(placedNavs.Count == 0) {
            UpdatePlacementPose();
            UpdatePlacementIndicator(new Vector3(1, 1, 1));
        }
        else {
            placementIndicator.SetActive(false);
        }

        if(placementPoseIsValid && placedNavs.Count == 0) {
            uiDirector.directionText = "Line the indicator up with the room orientation and tap to place";
        }
        else if(placementPoseIsValid == false && placedNavs.Count == 0) {
            uiDirector.directionText = "Move the camera around the ground to detect a plane";
        }
        else {
            uiDirector.directionText = "Tap and drag an arrow to scale the plane to the size of the room";
        }
        
        Touch touch = Input.GetTouch(0);
        if (Input.touchCount > 0 && touch.phase == TouchPhase.Began)
        {
            if (placementPoseIsValid && placedNavs.Count == 0)
            {       //Placing initial nav plane
                placedNavs.Add(PlaceObject(navPlane));
                wanderPoint.transform.position = placementPose.position;
                uiDirector.groundPlaced = true;
            }
            else
            {      //Starting plane scaling
                GameObject selectedObj = getVirtualObject(Input.GetTouch(0).position);
                if (selectedObj)
                {
                    if (selectedObj.tag == "Scale")
                    {
                        scaleHandle = selectedObj;
                        scaleHandle.GetComponent<Renderer>().material = selectedArrow;
                    }
                }
            }
        }

        if (scaleHandle)
        {
            Transform navParent = scaleHandle.transform.parent;
            Transform plane = navParent.GetChild(0);
            Transform handleZ1 = navParent.GetChild(1);
            Transform handleX1 = navParent.GetChild(2);
            Transform handleZ2 = navParent.GetChild(3);
            Transform handleX2 = navParent.GetChild(4);
            Transform ground = navParent.GetChild(5);
            //Transform x = navParent.GetChild(6);

            ground.gameObject.SetActive(true);

            RaycastHit hit;
            Vector3 scalePos = scaleHandle.transform.position;
            Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            if (Physics.Raycast(ray, out hit)) {
                scalePos = hit.point;
            }

            if (scaleHandle.name == "HandleX" || scaleHandle.name == "Handle-X") {
                scaleHandle.transform.position = new Vector3(scalePos.x, scaleHandle.transform.position.y, scaleHandle.transform.position.z);
            }
            else {
                scaleHandle.transform.position = new Vector3(scaleHandle.transform.position.x, scaleHandle.transform.position.y, scalePos.z);
            }

            float xSize = (handleX1.localPosition.x - handleX2.localPosition.x);
            float zSize = (handleZ1.localPosition.z - handleZ2.localPosition.z);
            plane.localPosition = new Vector3(xSize / 2 + handleX2.localPosition.x, plane.localPosition.y, zSize / 2 + handleZ2.localPosition.z);
            plane.localScale = new Vector3(xSize / 10, plane.localScale.y, zSize / 10);
            handleZ1.localPosition = new Vector3(xSize / 2 + handleX2.localPosition.x, handleZ1.localPosition.y, handleZ1.localPosition.z);
            handleZ2.localPosition = new Vector3(xSize / 2 + handleX2.localPosition.x, handleZ2.localPosition.y, handleZ2.localPosition.z);
            handleX1.localPosition = new Vector3(handleX1.localPosition.x, handleX1.localPosition.y, zSize / 2 + handleZ2.localPosition.z);
            handleX2.localPosition = new Vector3(handleX2.localPosition.x, handleX2.localPosition.y, zSize / 2 + handleZ2.localPosition.z);
            //x.localPosition = new Vector3(plane.localPosition.x, 0.1f, plane.localPosition.z);

            ground.gameObject.SetActive(false);

            if (touch.phase == TouchPhase.Ended)
            {
                scaleHandle.GetComponent<Renderer>().material = arrow;
                scaleHandle = null;
            }
        }
    }

    public void entranceInteraction() {
        GameObject virtualObj = getVirtualObject(Camera.current.ViewportToScreenPoint(new Vector3(0.5f, 0.5f)));
        if(virtualObj && virtualObj.tag == "Navigation" && entrances.Count == 0) {
            UpdatePlacementPoseVirtual();
            Vector3 rot = placementPose.rotation.eulerAngles;
            rot = new Vector3(rot.x, rot.y + 180, rot.z);
            placementPose.rotation = Quaternion.Euler(rot);
            UpdatePlacementIndicator(new Vector3(1, 1, 1));
        }

        if(entrances.Count == 0) {
            uiDirector.directionText = "Tap to place the entrance";
        }
        else {
            uiDirector.directionText = "";
        }

        Touch touch = Input.GetTouch(0);

        GameObject selectedObj = getVirtualObject(Input.GetTouch(0).position);
        if (Input.touchCount > 0 && touch.phase == TouchPhase.Ended && selectedObj && selectedObj.tag == "Delete")
        {
            GameObject e = selectedObj.transform.parent.gameObject;
            entrances.Remove(e);
            Destroy(e);
        }

        if (touch.position.y < Screen.height / 8)
        {
            placementIndicator.SetActive(false);
        }

        if (Input.touchCount > 0 && touch.phase == TouchPhase.Began && entrances.Count == 0 && placementIndicator.activeSelf == true)
        {

            Vector3 placePos = new Vector3(placementIndicator.transform.position.x, placementIndicator.transform.position.y, placementIndicator.transform.position.z);
            GameObject currEntrance = Instantiate(entrance, placePos, placementIndicator.transform.rotation);
            currEntrance.transform.position = new Vector3(currEntrance.transform.position.x, virtualObj.transform.position.y, currEntrance.transform.position.z);
            entrances.Add(currEntrance);
            placementIndicator.SetActive(false);
        }
    }

    public void chairInteraction() {
        UpdatePlacementPose();

        RaycastHit hit;
        GameObject objBelow;
        if(Physics.Raycast(placementPose.position, -Vector3.up, out hit)) {
            objBelow = hit.transform.gameObject;

            if (objBelow && objBelow.tag == "Navigation" && hit.distance <= 0.8)
            {
                Vector3 rot = placementPose.rotation.eulerAngles;
                rot = new Vector3(rot.x, rot.y + 180, rot.z);
                placementPose.rotation = Quaternion.Euler(rot);

                UpdatePlacementIndicator(new Vector3(0.5f, 0.5f, 0.5f));
            }

            GameObject virtualObj = getVirtualObject(Camera.current.ViewportToScreenPoint(new Vector3(0.5f, 0.5f)));
            if (virtualObj && virtualObj.tag == "Chair")
            { //Check to make sure chair isn't placed over another chair
                placementIndicator.SetActive(false);
            }

            Touch touch = Input.GetTouch(0);

            GameObject selectedObj = getVirtualObject(Input.GetTouch(0).position);
            if (Input.touchCount > 0 && selectedObj && selectedObj.tag == "Delete")
            {
                if(touch.phase == TouchPhase.Began) {
                    placementIndicator.SetActive(false);
                }
                else {
                    GameObject c = selectedObj.transform.parent.parent.gameObject;
                    chairs.Remove(c);
                    Destroy(c);
                }
            }

            if (touch.position.y < Screen.height / 8)
            {
                placementIndicator.SetActive(false);
            }

            if (Input.touchCount > 0 && touch.phase == TouchPhase.Began && placementIndicator.activeSelf == true)
            {

                Vector3 placePos = new Vector3(placementIndicator.transform.position.x, placedNavs[0].transform.position.y, placementIndicator.transform.position.z);
                GameObject currChair = Instantiate(chair, placePos, placementIndicator.transform.rotation);

                Transform chairIndicator = currChair.transform.GetChild(0);
                float chairHeight = placementPose.position.y - currChair.transform.position.y;  
                chairIndicator.localPosition = new Vector3(0, chairHeight / 2.0f, 0);
                chairIndicator.localScale = new Vector3(chairIndicator.localScale.x, chairHeight, chairIndicator.localScale.z);

                Transform torsoPose = currChair.transform.GetChild(1);
                torsoPose.localPosition = new Vector3(0, chairHeight + 0.35f, 0);
                chairs.Add(currChair);
                placementIndicator.SetActive(false);
            }
        }
    }
}
