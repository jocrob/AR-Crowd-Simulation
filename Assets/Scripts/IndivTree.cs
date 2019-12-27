using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using TreeSharpPlus;
using UnityEngine.AI;
using RootMotion.FinalIK;

public class IndivTree : MonoBehaviour
{
    public FullBodyBipedEffector body;
    public FullBodyBipedEffector rFoot;
    public FullBodyBipedEffector lFoot;
    public FullBodyBipedEffector rShoulder;
    public FullBodyBipedEffector lShoulder;
    public FullBodyBipedEffector rHand;
    public FullBodyBipedEffector lHand;
    public FullBodyBipedEffector rLeg;
    public FullBodyBipedEffector lLeg;

    private BehaviorAgent behaviorAgent;
    private Vector3 wanderPos;
    private GameObject chairTarget;
    private float timer = 0.0f;

    void Start()
    {
        GameObject interaction = GameObject.Find("Interaction");
        int agentCount = interaction.GetComponent<ARInteraction>().agentCount;
        List<GameObject> chairs = interaction.GetComponent<ARInteraction>().chairs;
        if(agentCount <= chairs.Count) {
            chairTarget = chairs[agentCount - 1];
        }

        wanderPos = getWanderPos();
        behaviorAgent = new BehaviorAgent(this.BuildTreeRoot());
        BehaviorManager.Instance.Register(behaviorAgent);
        behaviorAgent.StartBehavior();
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if(timer >= 3.0f) {
            timer = 0.0f;
            wanderPos = getWanderPos();
        }
    }

    Vector3 getWanderPos() {
        Vector3 randomDirection = transform.position + UnityEngine.Random.insideUnitSphere * 6;
        NavMeshHit hit;
        Vector3 newPos = Vector3.zero;
        if (NavMesh.SamplePosition(randomDirection, out hit, 6, 1))
        {
            newPos = hit.position;
        }
        return newPos;
    }

    protected Node Wander() {
        Val<Vector3> newPos = Val.V(() => wanderPos);

        return new Sequence(gameObject.GetComponent<BehaviorMecanim>().Node_GoTo(newPos), new LeafWait(200));
    }

    protected Node GoToChair() {
        Val<Vector3> chairFront = Val.V(() => chairTarget.transform.GetChild(3).position);
        return gameObject.GetComponent<BehaviorMecanim>().Node_GoTo(chairFront);
    }

    protected Node Sit() {
        Transform torso = chairTarget.transform.GetChild(1);
        Transform legs = chairTarget.transform.GetChild(2);
        Val<Quaternion> chairR = Val.V(() => chairTarget.transform.rotation); 
        InteractionObject bodyIK = torso.gameObject.GetComponent<InteractionObject>();
        InteractionObject rsIK = torso.GetChild(0).gameObject.GetComponent<InteractionObject>();
        InteractionObject lsIK = torso.GetChild(1).gameObject.GetComponent<InteractionObject>();
        InteractionObject rhIK = torso.GetChild(2).gameObject.GetComponent<InteractionObject>();
        InteractionObject lhIK = torso.GetChild(3).gameObject.GetComponent<InteractionObject>();
        InteractionObject rfIK = legs.GetChild(0).gameObject.GetComponent<InteractionObject>();
        InteractionObject lfIK = legs.GetChild(1).gameObject.GetComponent<InteractionObject>();
        InteractionObject rlIK = legs.GetChild(2).gameObject.GetComponent<InteractionObject>();
        InteractionObject llIK = legs.GetChild(3).gameObject.GetComponent<InteractionObject>();
        GameObject p = gameObject;

        return new Sequence(
            p.GetComponent<BehaviorMecanim>().Node_Orient(chairR),
            new SequenceParallel(
            p.GetComponent<BehaviorMecanim>().Node_StartInteraction(body, bodyIK),
            p.GetComponent<BehaviorMecanim>().Node_StartInteraction(rShoulder, rsIK),
            p.GetComponent<BehaviorMecanim>().Node_StartInteraction(lShoulder, lsIK),
            p.GetComponent<BehaviorMecanim>().Node_StartInteraction(rHand, rhIK),
            p.GetComponent<BehaviorMecanim>().Node_StartInteraction(lHand, lhIK),
            p.GetComponent<BehaviorMecanim>().Node_StartInteraction(rFoot, rfIK),
            p.GetComponent<BehaviorMecanim>().Node_StartInteraction(lFoot, lfIK),
            p.GetComponent<BehaviorMecanim>().Node_StartInteraction(rLeg, rlIK),
            p.GetComponent<BehaviorMecanim>().Node_StartInteraction(lLeg, llIK)));
    }

    protected Node BuildTreeRoot()
    {
        Node roaming;
        if(chairTarget != null) {
            roaming = new Sequence(
                this.GoToChair(),
                this.Sit());
        }
        else {
            roaming = new DecoratorLoop(
                this.Wander());
        }

        return roaming;
    }
}