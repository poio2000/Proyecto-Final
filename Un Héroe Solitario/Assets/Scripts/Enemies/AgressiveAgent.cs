using UnityEngine;
using UnityEngine.Rendering;
using static Unity.Burst.Intrinsics.X86.Avx;

/// <summary>
/// Represents an aggressive agent with perception and decision-making capabilities.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class AgressiveAgent : BasicAgent {

    [SerializeField] Animator animator;
    [SerializeField] AgressiveAgentStates agentState;
    Rigidbody rb;
    string currentAnimationStateName;

    void Start () {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        agentState = AgressiveAgentStates.None;
        currentAnimationStateName = "";
    }

    void Update () {
        decisionManager();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            target = other.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            target = null;
        }
    }

    public void turnbool(Transform t_target) {
        target = t_target;
    }

    /// <summary>
    /// Manages decision-making based on the agent's perception.
    /// </summary>
    void decisionManager () {
        AgressiveAgentStates newState;
        if (target == null) {
            newState = AgressiveAgentStates.Wander;
        } 
        else{
            newState = AgressiveAgentStates.Pursuit;
            if (Vector3.Distance(transform.position, target.position) < stopThreshold) {
                newState = AgressiveAgentStates.Attack;
            }
        }
        changeAgentState(newState);
        movementManager();
    }

    /// <summary>
    /// Changes the state of the agent only if its a new state
    /// </summary>
    /// <param name="t_newState">The new state of the agent.</param>
    void changeAgentState (AgressiveAgentStates t_newState) {
        if (agentState == t_newState) {
            return;
        }
        agentState = t_newState;
        if (agentState != AgressiveAgentStates.Wander) {
            wanderNextPosition = null;
        }
    }

    /// <summary>
    /// Manages movement based on the current state of the agent.
    /// </summary>
    void movementManager () {
        switch (agentState) {
            case AgressiveAgentStates.None:
                rb.linearVelocity = Vector3.zero;
                break;
            case AgressiveAgentStates.Pursuit:
                pursuiting();
                break;
            case AgressiveAgentStates.Attack:
                attacking();
                break;
            case AgressiveAgentStates.Wander:
                wandering();
                break;
        }
    }

    /// <summary>
    /// Moves the agent randomly within the environment.
    /// </summary>
    private void wandering () {
        if (!currentAnimationStateName.Equals("Z_Walk_InPlace")) {
            Debug.Log(currentAnimationStateName);
            //animator.Play("Z_Walk_InPlace", 0);
            currentAnimationStateName = "Z_Walk_InPlace";
        }
        if (( wanderNextPosition == null ) ||
            ( Vector3.Distance(transform.position, wanderNextPosition.Value) < 0.5f )) {
            wanderNextPosition = SteeringBehaviours.wanderNextPos(this);
        }
        rb.linearVelocity = SteeringBehaviours.seek(this, wanderNextPosition.Value);
    }

    /// <summary>
    /// Handles pursuing the target.
    /// </summary>
    private void pursuiting () {
        if (!currentAnimationStateName.Equals("Z_Run_InPlace") && !currentAnimationStateName.Equals("Z_Walk_InPlace")) {
            Debug.Log(currentAnimationStateName);
            //animator.Play("Z_Run_InPlace", 0);
            currentAnimationStateName = "Z_Run_InPlace";
        }
        maxVel *= 2;
        rb.linearVelocity = SteeringBehaviours.seek(this, target.position);
        rb.linearVelocity = SteeringBehaviours.arrival(this, target.position, slowingRadius, stopThreshold);
        if (Vector3.Distance(transform.position, target.position) <= slowingRadius) {
            if (!currentAnimationStateName.Equals("Z_Walk_InPlace")) {
                animator.Play("Z_Walk_InPlace", 0);
                currentAnimationStateName = "Z_Walk_InPlace";
            }
        }
        maxVel /= 2;
    }

    /// <summary>
    /// Handles attacking the target.
    /// </summary>
    private void attacking () {
        if (!currentAnimationStateName.Equals("Z_Attack")) {
            Debug.Log(currentAnimationStateName);
            //animator.Play("Z_Attack", 0);
            currentAnimationStateName = "Z_Attack";
        }
    }

    /// <summary>
    /// Enumeration of possible agent states.
    /// </summary>
    private enum AgressiveAgentStates {
        None,
        Pursuit,
        Attack,
        Wander
    }
}