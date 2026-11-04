using UnityEngine;

public class TouchState : EnvironmentInteractionState
{
    float stateDuration = 1f;
    float elapsedTime = 0.0f;

    public TouchState(EnvironmentInteractionContext context, 
    EnvironmentInteractionStateMachine.EEnvironmentInteractionState estate) : base (context, estate)
    {
        EnvironmentInteractionContext Context = context;
    }

    public override void EnterState()
    {
        elapsedTime = 0.0f;
    }

    public override void ExitState()
    {

    }

    public override void UpdateState()
    {
        elapsedTime += Time.deltaTime;
    }

    public override EnvironmentInteractionStateMachine.EEnvironmentInteractionState GetNextState()
    {
        if (CheckShouldReset())
        {
            return EnvironmentInteractionStateMachine.EEnvironmentInteractionState.Reset;
        }
        
        if (elapsedTime > stateDuration)
        {
            return EnvironmentInteractionStateMachine.EEnvironmentInteractionState.Reset;
        }
        return StateKey;
    }

    public override void OnTriggerEnter(Collider other)
    {
        StartIKTargetTracking(other);
    }

    public override void OnTriggerStay(Collider other)
    {
        UpdateIKTargetTracking(other);
    }

    public override void OnTriggerExit(Collider other)
    {
        ResetIKTargetTracking(other);
    }
}
