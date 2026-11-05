using UnityEngine;

public class ApproachState : EnvironmentInteractionState
{
    float riseDistanceThreshold = 50f;
    float lerpDuration = 5.0f;
    float resetDuration = 3.0f;
    float IKConstraintTargetWeight = 0.3f;
    float RotationConstraintTargetWeight = 0.7f;
    float FingerConstraintTargetWeight = 0.7f;
    float rotationSpeed = 500f;
    float elapsedTime = 0.0f;

    public ApproachState(EnvironmentInteractionContext context, 
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

        Context.InteractionPointYOffset = Context.ColliderCenterY;
        float timeElapsedPercent = elapsedTime / lerpDuration;

        Quaternion targetRotation = Quaternion.LookRotation(-Vector3.up, Context.RootTransform.up);
        Context.currentRotationConstraint.weight = Mathf.Lerp(Context.currentRotationConstraint.weight, RotationConstraintTargetWeight, timeElapsedPercent);
        Context.currentIKTarget.rotation = Quaternion.RotateTowards(Context.currentIKTarget.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        Context.currentIKConstraint.weight = Mathf.Lerp(Context.currentIKConstraint.weight, IKConstraintTargetWeight, timeElapsedPercent);
        Context.currentFingerRig.weight = Mathf.Lerp(Context.currentFingerRig.weight, FingerConstraintTargetWeight, timeElapsedPercent);
    }

    public override EnvironmentInteractionStateMachine.EEnvironmentInteractionState GetNextState()
    {
        if (CheckShouldReset())
        {
            return EnvironmentInteractionStateMachine.EEnvironmentInteractionState.Reset;
        }

        if (elapsedTime > resetDuration)
        {
            return EnvironmentInteractionStateMachine.EEnvironmentInteractionState.Reset;
        }

        bool isWithinArmsReach = Vector3.Distance(Context.ClosestPointOnColliderFromShoulder, Context.currentShoulder.position) < riseDistanceThreshold;
        bool isTargetPointValid = Context.ClosestPointOnColliderFromShoulder != Vector3.positiveInfinity;

        if (isWithinArmsReach && isTargetPointValid)
        {
            return EnvironmentInteractionStateMachine.EEnvironmentInteractionState.Rise;
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
