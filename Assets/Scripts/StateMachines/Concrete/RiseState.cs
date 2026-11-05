using UnityEngine;

public class RiseState : EnvironmentInteractionState
{
    float lerpDuration = 8.0f;
    float IKConstraintTargetWeight = 1f;
    float RotationConstraintTargetWeight = 1f;
    float rotationSpeed = 1000f;
    LayerMask interactableLayer = LayerMask.GetMask("Interactable");
    float maxRaycastDistance = 50f;
    Quaternion targetHandRotation;
    float stateDuration = 2.0f;
    float touchDistanceThreshold = 5f;
    float elapsedTime = 0.0f;
    float FingerConstraintTargetWeight = 1f;

    public RiseState(EnvironmentInteractionContext context, 
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

        CalculateTargetHandRotation();
        Context.InteractionPointYOffset = Context.currentShoulder.position.y;
        float timeElapsedPercent = elapsedTime / lerpDuration;

        Context.currentRotationConstraint.weight = Mathf.Lerp(Context.currentRotationConstraint.weight, RotationConstraintTargetWeight, timeElapsedPercent);
        Context.currentIKTarget.rotation = Quaternion.RotateTowards(Context.currentIKTarget.rotation, targetHandRotation, rotationSpeed * Time.deltaTime);
        Context.currentIKConstraint.weight = Mathf.Lerp(Context.currentIKConstraint.weight, IKConstraintTargetWeight, timeElapsedPercent);
        Context.currentFingerRig.weight = Mathf.Lerp(Context.currentFingerRig.weight, FingerConstraintTargetWeight, timeElapsedPercent);
    }

    private void CalculateTargetHandRotation()
    {
        Vector3 targetDir = Context.ClosestPointOnColliderFromShoulder - Context.currentShoulder.position;

        RaycastHit hit;
        if (Physics.Raycast(Context.currentShoulder.position, targetDir.normalized, out hit, maxRaycastDistance, interactableLayer))
        {
            Vector3 surfaceNormal = hit.normal;
            Vector3 targetForward = -surfaceNormal;
            targetHandRotation = Quaternion.LookRotation(targetForward, Vector3.up);
        }
    }

    public override EnvironmentInteractionStateMachine.EEnvironmentInteractionState GetNextState()
    {
        if (CheckShouldReset())
        {
            return EnvironmentInteractionStateMachine.EEnvironmentInteractionState.Reset;
        }

        bool stateTimeElapsed = elapsedTime > stateDuration;
        bool handTouchingSurface = Vector3.Distance(Context.currentIKTarget.position, Context.ClosestPointOnColliderFromShoulder) < touchDistanceThreshold;

        if (handTouchingSurface && stateTimeElapsed)
        {
            return EnvironmentInteractionStateMachine.EEnvironmentInteractionState.Touch;
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
