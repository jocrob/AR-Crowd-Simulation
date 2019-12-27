using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using TreeSharpPlus;
using UnityEngine.AI;

public class Tree : MonoBehaviour
{
    public Transform wander1;
    private List<GameObject> participants;
    private BehaviorAgent behaviorAgent;
    // Use this for initialization
    void Start()
    {
        behaviorAgent = new BehaviorAgent(this.BuildTreeRoot());
        BehaviorManager.Instance.Register(behaviorAgent);
        behaviorAgent.StartBehavior();
    }

    // Update is called once per frame
    void Update()
    {
    }

    protected Node ST_ApproachAndWait(Transform target, GameObject participant)
    {
        Debug.Log("ran");
        bool mecanimExists = participant.GetComponent<BehaviorMecanim>() != null;
        Val<Vector3> position = Val.V(() => target.position);
        return new Sequence(participant.GetComponent<BehaviorMecanim>().Node_GoTo(position), new LeafWait(1000));
    }

    protected Node BuildTreeRoot()
    {
        Val<List<GameObject>> crowd = Val.V(() => participants);
        Func<GameObject, Node> stFactory = p => this.ST_ApproachAndWait(wander1, p);
        Node roaming = new DecoratorLoop(
                        new ForEach<GameObject>(stFactory, crowd.Value));
        // Node roaming = new DecoratorLoop(
        //                 this.ST_ApproachAndWait(this.wander1, participants[0]));
        return roaming;
    }
}