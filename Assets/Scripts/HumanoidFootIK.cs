using UnityEngine;
using UnityEngine.Animations.Rigging;

[RequireComponent(typeof(Animator))]
public class HumanoidFootIK : MonoBehaviour
{
    [SerializeField] private float KneeRaycastDistance = 50f;
    [SerializeField] private float footOffsetFromGround = 10f;
    private Animator anim;
    private Transform leftFoot;
    private Transform rightFoot;
    private Transform leftKnee;
    private Transform rightKnee;

    void Awake()
    {
        anim = GetComponent<Animator>();
        leftFoot = anim.GetBoneTransform(HumanBodyBones.LeftFoot);
        rightFoot = anim.GetBoneTransform(HumanBodyBones.RightFoot);
        leftKnee = anim.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
        rightKnee = anim.GetBoneTransform(HumanBodyBones.RightLowerLeg);
    }


    Vector3 SetFootPosition(Transform foot, Transform knee)
    {
        Vector3 footPosition = Vector3.zero;
        RaycastHit hit;
        if (Physics.Raycast(knee.position, -Vector3.up, out hit, KneeRaycastDistance))
        {
            footPosition = hit.point;
            footPosition = new Vector3(foot.position.x, footPosition.y + footOffsetFromGround, foot.position.z);
        }
        return footPosition;
    }

    Quaternion SetFootRotation(Transform foot, Transform knee)
    {
        Quaternion footRotation = foot.rotation;
        RaycastHit hit;
        if (Physics.Raycast(knee.position, -Vector3.up, out hit, KneeRaycastDistance))
        {
            Vector3 surfaceNormal = hit.normal;
            Vector3 lookDir = Vector3.Cross(-surfaceNormal, transform.right);
            footRotation = Quaternion.LookRotation(lookDir, foot.forward);
        }
        return footRotation;
    }

    void OnAnimatorIK(int layerIndex)
    {
        anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f);
        anim.SetIKPosition(AvatarIKGoal.LeftFoot, SetFootPosition(leftFoot, leftKnee));

        anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);
        anim.SetIKPosition(AvatarIKGoal.RightFoot, SetFootPosition(rightFoot, rightKnee));

        anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 0.6f);
        anim.SetIKRotation(AvatarIKGoal.LeftFoot, SetFootRotation(leftFoot, leftKnee));

        anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, 0.6f);
        anim.SetIKRotation(AvatarIKGoal.RightFoot, SetFootRotation(rightFoot, rightKnee));
    }
}
