using UnityEngine;

public class SearchState : EnvironmentInteractionState
{
    float approachDistanceThreshold = 170f;
    public SearchState(EnvironmentInteractionContext context, 
    EnvironmentInteractionStateMachine.EEnvironmentInteractionState estate) : base (context, estate)
    {
        EnvironmentInteractionContext Context = context;
    }

    public override void EnterState()
    {
        
    }

    public override void ExitState()
    {

    }

    public override void UpdateState()
    {
        
    }

    public override EnvironmentInteractionStateMachine.EEnvironmentInteractionState GetNextState()
    {
        if (CheckShouldReset())
        {
            return EnvironmentInteractionStateMachine.EEnvironmentInteractionState.Reset;
        }

        bool isCloseToTarget = Vector3.Distance(Context.ClosestPointOnColliderFromShoulder, Context.RootTransform.position) < approachDistanceThreshold;
        bool isTargetPointValid = Context.ClosestPointOnColliderFromShoulder != Vector3.positiveInfinity;

        if (isCloseToTarget && isTargetPointValid)
        {
            return EnvironmentInteractionStateMachine.EEnvironmentInteractionState.Approach;
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
