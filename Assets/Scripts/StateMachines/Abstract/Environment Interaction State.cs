using UnityEngine;

public abstract class EnvironmentInteractionState : BaseState<EnvironmentInteractionStateMachine.EEnvironmentInteractionState>
{
    protected EnvironmentInteractionContext Context;
    protected float elapsedTime = 0.0f;
    float movingAwayOffset = 20f;
    bool shouldReset;

    public EnvironmentInteractionState(EnvironmentInteractionContext context, 
    EnvironmentInteractionStateMachine.EEnvironmentInteractionState stateKey) : base (stateKey)
    {
        Context = context;
    }

    private Vector3 GetClosestPointOnCollider(Collider collider, Vector3 positionToCheck)
    {
        return collider.ClosestPoint(positionToCheck);
    }

    protected bool CheckShouldReset()
    {
        if (shouldReset)
        {
            Context.LowestDistance = Mathf.Infinity;
            shouldReset = false;
            return true;
        }

        bool isPlayerStopped = Context.RootRigidbody.linearVelocity == Vector3.zero;
        bool isMovingAway = CheckIsMovingAway();
        bool hasBadAngle = CheckBadAngle();
        //bool isPlayerJumping = Mathf.Round(Context.RootRigidbody.linearVelocity.y) > 1;

        if (isPlayerStopped || isMovingAway || hasBadAngle)
        {
            Context.LowestDistance = Mathf.Infinity;
            return true;
        }

        return false;
    }

    private bool CheckBadAngle()
    {
        if (Context.currentIntersectingCollider == null)
        {
            return false;
        }

        Vector3 targetDir = Context.ClosestPointOnColliderFromShoulder - Context.currentShoulder.position;
        targetDir = new Vector3(targetDir.x, 0f, targetDir.z);
        Vector3 shoulderDir = (Context.currentSide == EnvironmentInteractionContext.EBodySide.Right) ?
        Context.RootTransform.right : -Context.RootTransform.right;
        shoulderDir = new Vector3(shoulderDir.x, 0f, shoulderDir.z);

        float dotProduct = Vector3.Dot(targetDir.normalized, shoulderDir.normalized);

        if (dotProduct < 0)
        {
            return true;
        }

        return false;
    }

    private bool CheckIsMovingAway()
    {
        float distanceFromTarget = Vector3.Distance(Context.RootTransform.position, Context.ClosestPointOnColliderFromShoulder);
        bool isSearchingForCollider = Context.currentIntersectingCollider == null;

        if (isSearchingForCollider)
        {
            return false;
        }

        if (distanceFromTarget <= Context.LowestDistance)
        {
            Context.LowestDistance = distanceFromTarget;
            return false;
        }

        if (distanceFromTarget > Context.LowestDistance + movingAwayOffset)
        {
            Context.LowestDistance = Mathf.Infinity;
            return true;
        }

        return false;
    }

    protected void StartIKTargetTracking(Collider intersectingCollider)
    {
        if (intersectingCollider.gameObject.layer != LayerMask.NameToLayer("Interactable")) return;
        if (Context.currentIntersectingCollider != null) return;

        Context.currentIntersectingCollider = intersectingCollider;
        Vector3 closestIntersectingPoint = GetClosestPointOnCollider(intersectingCollider, Context.RootTransform.position);
        Context.SetCurrentSide(closestIntersectingPoint);

        SetCurrentIKTargetPosition();

    }

    protected void UpdateIKTargetTracking(Collider intersectingCollider)
    {
        if (Context.currentIntersectingCollider != intersectingCollider) return;
        SetCurrentIKTargetPosition();
    }

    protected void ResetIKTargetTracking(Collider intersectingCollider)
    {
        if (Context.currentIntersectingCollider != intersectingCollider) return;
        Context.currentIntersectingCollider = null;
        Context.ClosestPointOnColliderFromShoulder = Vector3.positiveInfinity;
        shouldReset = true;
    }

    protected void SetCurrentIKTargetPosition()
    {
        if (Context.ClosestPointOnColliderFromShoulder == Vector3.positiveInfinity) return;

        Context.ClosestPointOnColliderFromShoulder = GetClosestPointOnCollider(Context.currentIntersectingCollider,
        new Vector3(Context.currentShoulder.position.x, Context.CharacterShoulderHeight, Context.currentShoulder.position.z));

        Vector3 rayDirection = Context.currentShoulder.position - Context.ClosestPointOnColliderFromShoulder;
        float handOffsetFromWall = 5f;
        Vector3 offsetVector = rayDirection.normalized * handOffsetFromWall;

        Vector3 targetPos = Context.ClosestPointOnColliderFromShoulder + offsetVector;

        Context.currentIKTarget.position = new Vector3(targetPos.x, Context.InteractionPointYOffset, targetPos.z);
    }
}
