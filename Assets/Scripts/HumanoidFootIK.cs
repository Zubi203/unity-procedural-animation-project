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
    private Transform hip;
    float hipYOffset;

    void Awake()
    {
        anim = GetComponent<Animator>();
        leftFoot = anim.GetBoneTransform(HumanBodyBones.LeftFoot);
        rightFoot = anim.GetBoneTransform(HumanBodyBones.RightFoot);
        leftKnee = anim.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
        rightKnee = anim.GetBoneTransform(HumanBodyBones.RightLowerLeg);
        hip = anim.GetBoneTransform(HumanBodyBones.Hips);

        RaycastHit hit;
        if (Physics.Raycast(hip.position, -Vector3.up, out hit, Mathf.Infinity))
        {
            hipYOffset = Vector3.Distance(hip.position, hit.point);
        }
         
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

    Vector3 SetHipOffset()
    {
        Vector3 hipPosition = hip.position;
        Vector3 lowestFootPosition;
        Vector3 leftFootGroundPos = leftFoot.position;
        Vector3 rightFootGroundPos = rightFoot.position;

        RaycastHit rightHit;
        if (Physics.Raycast(rightKnee.position, -Vector3.up, out rightHit, 2000f))
        {
            rightFootGroundPos = rightHit.point;
        }

        RaycastHit leftHit;
        if (Physics.Raycast(leftKnee.position, -Vector3.up, out leftHit, 2000f))
        {
            leftFootGroundPos = leftHit.point;
        }

        lowestFootPosition = (leftFootGroundPos.y < rightFootGroundPos.y) ? leftFootGroundPos : rightFootGroundPos;

        hipPosition = new Vector3(hipPosition.x, lowestFootPosition.y + hipYOffset, hipPosition.z);

        return hipPosition;
    }

    void OnAnimatorIK(int layerIndex)
    {
        anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f);
        anim.SetIKPosition(AvatarIKGoal.LeftFoot, SetFootPosition(leftFoot, leftKnee));

        anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);
        anim.SetIKPosition(AvatarIKGoal.RightFoot, SetFootPosition(rightFoot, rightKnee));

        hip.position = SetHipOffset();

        anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 0.6f);
        anim.SetIKRotation(AvatarIKGoal.LeftFoot, SetFootRotation(leftFoot, leftKnee));

        anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, 0.6f);
        anim.SetIKRotation(AvatarIKGoal.RightFoot, SetFootRotation(rightFoot, rightKnee));
    }
}
