using UnityEngine;

public class ResetState : EnvironmentInteractionState
{
    private float lerpDuration = 10.0f;
    private float resetDuration = 2.0f;
    private float rotateSpeed = 500f;
    private float elapsedTime = 0.0f;

    public ResetState(EnvironmentInteractionContext context, 
    EnvironmentInteractionStateMachine.EEnvironmentInteractionState estate) : base (context, estate)
    {
        EnvironmentInteractionContext Context = context;
    }

    public override void EnterState()
    {
        elapsedTime = 0.0f;
        Context.currentIntersectingCollider = null;
        Context.ClosestPointOnColliderFromShoulder = Vector3.positiveInfinity;
    }

    public override void ExitState()
    {
        
    }

    public override void UpdateState()
    {
        elapsedTime += Time.deltaTime;

        float timeElapsedPercent = elapsedTime / lerpDuration;

        Context.currentIKConstraint.weight = Mathf.Lerp(Context.currentIKConstraint.weight, 0.0f, timeElapsedPercent);

        Context.currentRotationConstraint.weight = Mathf.Lerp(Context.currentRotationConstraint.weight, 0.0f, timeElapsedPercent);

        Context.currentIKTarget.localPosition = Vector3.Lerp(Context.currentIKTarget.localPosition, Context.currentOriginalTargetPosition, timeElapsedPercent);

        Context.InteractionPointYOffset = Mathf.Lerp(Context.InteractionPointYOffset, Context.ColliderCenterY, timeElapsedPercent);

        Context.currentIKTarget.rotation = Quaternion.RotateTowards(Context.currentIKTarget.rotation, Context.OriginalTargetRotation, Time.deltaTime * rotateSpeed);


    }

    public override EnvironmentInteractionStateMachine.EEnvironmentInteractionState GetNextState()
    {
        bool isMoving = Context.RootRigidbody.linearVelocity != Vector3.zero;

        if (elapsedTime > resetDuration && isMoving)
        {
            return EnvironmentInteractionStateMachine.EEnvironmentInteractionState.Search;
        }

        return StateKey;
    }

    public override void OnTriggerEnter(Collider other)
    {

    }

    public override void OnTriggerStay(Collider other)
    {

    }

    public override void OnTriggerExit(Collider other)
    {
        
    }
}
