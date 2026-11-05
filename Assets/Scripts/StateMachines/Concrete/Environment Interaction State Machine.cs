using UnityEngine;
using System;
using UnityEngine.Animations.Rigging;
using UnityEngine.Assertions;

public class EnvironmentInteractionStateMachine : StateMachine<EnvironmentInteractionStateMachine.EEnvironmentInteractionState>
{
    private EnvironmentInteractionContext context;

    public enum EEnvironmentInteractionState
    {
        Search,
        Approach,
        Rise,
        Touch,
        Reset
    }

    [SerializeField] private TwoBoneIKConstraint leftHandIKConstraint;
    [SerializeField] private TwoBoneIKConstraint rightHandIKConstraint;
    [SerializeField] private MultiRotationConstraint leftHandRotationConstraint;
    [SerializeField] private MultiRotationConstraint rightHandRotationConstraint;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private CapsuleCollider collider;
    [SerializeField] private Transform rootTransform;
    [SerializeField] private Rig leftFingersRig;
    [SerializeField] private Rig rightFingersRig;

    private void Awake()
    {
        ValidateConstraints();

        context = new EnvironmentInteractionContext(leftHandIKConstraint, rightHandIKConstraint, leftHandRotationConstraint, 
        rightHandRotationConstraint, rb, collider, rootTransform, leftFingersRig, rightFingersRig);

        InitializeStates();
        ConstructEnvironmentDetectionCollider();
    }

    private void ValidateConstraints()
    {
        Assert.IsNotNull(leftHandIKConstraint, "Left IK constraint is not assigned");
        Assert.IsNotNull(rightHandIKConstraint, "Right IK constraint is not assigned");
        Assert.IsNotNull(leftHandRotationConstraint, "Left multi-rotation constraint is not assigned");
        Assert.IsNotNull(rightHandRotationConstraint, "Right multi-rotation constraint is not assigned");
        Assert.IsNotNull(rb, "Rigidbody is not assigned");
        Assert.IsNotNull(collider, "Capsule collider is not assigned");
        Assert.IsNotNull(rootTransform, "Root transform is not assigned");
        Assert.IsNotNull(leftFingersRig, "left finger rig is not assigned");
        Assert.IsNotNull(rightFingersRig, "right finger rig is not assigned");
    }

    private void InitializeStates()
    {
        States.Clear();

        States.Add(EEnvironmentInteractionState.Search, new SearchState(context, EEnvironmentInteractionState.Search));
        States.Add(EEnvironmentInteractionState.Approach, new ApproachState(context, EEnvironmentInteractionState.Approach));
        States.Add(EEnvironmentInteractionState.Rise, new RiseState(context, EEnvironmentInteractionState.Rise));
        States.Add(EEnvironmentInteractionState.Touch, new TouchState(context, EEnvironmentInteractionState.Touch));
        States.Add(EEnvironmentInteractionState.Reset, new ResetState(context, EEnvironmentInteractionState.Reset));

        currentState = States[EEnvironmentInteractionState.Reset];
    }

    void ConstructEnvironmentDetectionCollider()
    {
        float wingspan = collider.height / 100f;
        Vector3 rootColliderCenter = collider.center / 100f;

        BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
        boxCollider.size = new Vector3(wingspan, wingspan, wingspan);
        boxCollider.center = new Vector3(rootColliderCenter.x, rootColliderCenter.y + (0.8f * wingspan), rootColliderCenter.z + (0.5f * wingspan));
        boxCollider.isTrigger = true;
    }
}
