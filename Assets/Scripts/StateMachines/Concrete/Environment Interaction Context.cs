using UnityEngine;
using UnityEngine.Animations.Rigging;

public class EnvironmentInteractionContext 
{
    public enum EBodySide
    {
        Right,
        Left
    }

    private TwoBoneIKConstraint _leftHandIKConstraint;
    private TwoBoneIKConstraint _rightHandIKConstraint;
    private MultiRotationConstraint _leftHandRotationConstraint;
    private MultiRotationConstraint _rightHandRotationConstraint;
    private Rigidbody _rb;
    private CapsuleCollider _collider;
    private Transform _rootTransform;
    private Vector3 _leftOriginalTargetPosition;
    private Vector3 _rightOriginalTargetPosition;
    private Rig _leftFingersRig;
    private Rig _rightFingersRig;

    public EnvironmentInteractionContext(TwoBoneIKConstraint leftHandIKConstraint, TwoBoneIKConstraint rightHandIKConstraint, 
    MultiRotationConstraint leftHandRotationConstraint, MultiRotationConstraint rightHandRotationConstraint, 
    Rigidbody rb, CapsuleCollider collider, Transform rootTransform, Rig leftFingersRig, Rig rightFingersRig)
    {
        _leftHandIKConstraint = leftHandIKConstraint;
        _rightHandIKConstraint = rightHandIKConstraint;
        _leftHandRotationConstraint = leftHandRotationConstraint;
        _rightHandRotationConstraint = rightHandRotationConstraint;
        _rb = rb;
        _collider = collider;
        _rootTransform = rootTransform;
        _leftFingersRig = leftFingersRig;
        _rightFingersRig = rightFingersRig;

        CharacterShoulderHeight = _leftHandIKConstraint.data.root.transform.position.y;
        _leftOriginalTargetPosition = _leftHandIKConstraint.data.target.transform.localPosition;
        _rightOriginalTargetPosition = _rightHandIKConstraint.data.target.transform.localPosition;
        OriginalTargetRotation = _leftHandIKConstraint.data.target.transform.rotation;
        
        SetCurrentSide(Vector3.positiveInfinity);
    }

    public TwoBoneIKConstraint LeftHandIKConstraint => _leftHandIKConstraint;
    public TwoBoneIKConstraint RightHandIKConstraint => _rightHandIKConstraint;
    public MultiRotationConstraint LeftHandRotationConstraint => _leftHandRotationConstraint;
    public MultiRotationConstraint RightHandRotationConstraint => _rightHandRotationConstraint;
    public Rigidbody RootRigidbody => _rb;
    public CapsuleCollider RootCollider => _collider;
    public Transform RootTransform => _rootTransform;
    public float ColliderCenterY => RootCollider.center.y + 130f;

    public Collider currentIntersectingCollider {get; set;}
    public TwoBoneIKConstraint currentIKConstraint {get; private set;}
    public Rig currentFingerRig {get; private set;}
    public MultiRotationConstraint currentRotationConstraint {get; private set;}
    public Transform currentIKTarget {get; private set;}
    public Vector3 currentOriginalTargetPosition {get; private set;}
    public Transform currentShoulder {get; private set;}
    public EBodySide currentSide {get; private set;}
    public Vector3 ClosestPointOnColliderFromShoulder {get; set;} = Vector3.positiveInfinity;
    public float CharacterShoulderHeight {get; private set;}
    public float InteractionPointYOffset {get; set;}
    public Quaternion OriginalTargetRotation {get; private set;}
    public float LowestDistance {get; set;} = Mathf.Infinity;

    public void SetCurrentSide(Vector3 targetPos)
    {
        Vector3 leftShoulder = _leftHandIKConstraint.data.root.transform.position;
        Vector3 rightShoulder = _rightHandIKConstraint.data.root.transform.position;

        bool isLeftSide = Vector3.Distance(targetPos, leftShoulder) < Vector3.Distance(targetPos, rightShoulder);

        if (isLeftSide)
        {
            currentSide = EBodySide.Left;
            currentIKConstraint = _leftHandIKConstraint;
            currentRotationConstraint = _leftHandRotationConstraint;
            currentOriginalTargetPosition = _leftOriginalTargetPosition;
            currentFingerRig = _leftFingersRig;
        }
        else
        {
            currentSide = EBodySide.Right;
            currentIKConstraint = _rightHandIKConstraint;
            currentRotationConstraint = _rightHandRotationConstraint;
            currentOriginalTargetPosition = _rightOriginalTargetPosition;
            currentFingerRig = _rightFingersRig;
        }

        currentIKTarget = currentIKConstraint.data.target;
        currentShoulder = currentIKConstraint.data.root;
    }
}
