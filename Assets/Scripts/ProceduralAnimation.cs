using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

public class ProceduralAnimation : MonoBehaviour
{
    [SerializeField] private float footOffsetFromGround = 0.5f;
    [SerializeField] private float overStepMultiplier = 1.2f;
    [SerializeField] private AnimationCurve stepArc;
    [SerializeField] private float stepHeight = 1f;
    [SerializeField] private GameObject stepTargetPrefab;

    [SerializeField] private Transform headAimTarget;
    [SerializeField] private Transform bodyTransform;
    [SerializeField] private List<Transform> IKTargets = new List<Transform>();
    [SerializeField] private List<Transform> dampedTargets = new List<Transform>();
    private List<Transform> dampMoveTargets = new List<Transform>();
    [SerializeField] private float dampedTargetsYOffset = 5f;

    [SerializeField] private float afterStepWaitTime = 0.2f;
    private Dictionary<Transform, Transform> stepTargets = new Dictionary<Transform, Transform>();
    private Dictionary<Transform, Vector3> groundRootPoints = new Dictionary<Transform, Vector3>();
    private List<Transform> mainLegs = new List<Transform>();
    private List<Transform> oppositeLegs = new List<Transform>();
    private Dictionary<Transform, bool> isMoving = new Dictionary<Transform, bool>();
    private Dictionary<Transform, bool> isGrounded = new Dictionary<Transform, bool>();

    private Dictionary<Transform, float> YOffsets = new Dictionary<Transform, float>();
    private float baseYOffset;
    private float averageDeltaYBetweenTargets;
    private Transform centerTarget;
    private Transform lookTarget;

    [SerializeField] private bool nonUniformStepDistanceThreshold = false;
    [SerializeField] private float stepDistanceThreshold = 1f;
    [SerializeField] private List<float> stepDistanceThresholds = new List<float>();
    [SerializeField] private bool nonUniformGaitDuration = false;
    [SerializeField] private List<float> gaitDurations = new List<float>();
    [SerializeField] private float gaitDuration = 0.5f;

    void Start()
    {
        CreateTargetPoints();
        baseYOffset = bodyTransform.position.y;
    }

    void FixedUpdate()
    {
        UpdateLegPositions();
        UpdateLegRotations();

        bodyTransform.position = Vector3.Lerp(bodyTransform.position, CalculateBodyPos(), Time.deltaTime * 7f);

        //Quaternion targetRotation = Quaternion.Euler(bodyTransform.eulerAngles + (Vector3.right * CalculateRotationZ()));
        //bodyTransform.rotation = Quaternion.RotateTowards(bodyTransform.rotation, targetRotation, Time.deltaTime * 5f);
        
        if (headAimTarget != null)
        {
            headAimTarget.position = Vector3.Lerp(headAimTarget.position, lookTarget.position, Time.deltaTime * 7f);
        }

        if (dampedTargets.Count > 0)
        {
            for (int i = 0; i < dampedTargets.Count; i++)
            {
                dampedTargets[i].position = Vector3.Lerp(dampedTargets[i].position, dampMoveTargets[i].position + Vector3.up * (dampedTargetsYOffset), Time.deltaTime * 15f);
                dampedTargets[i].rotation = Quaternion.RotateTowards(dampedTargets[i].rotation, dampMoveTargets[i].rotation, Time.deltaTime * 50f);
            }
        }
    }

    public void AimAt(Vector3 targetPos)
    {
        lookTarget.position = targetPos;
    }

    Vector3 CalculateBodyPos()
    {
        float centroidX = 0f;
        float centroidZ = 0f;
        for (int i = 0; i < IKTargets.Count; i++)
        {
            centroidX += IKTargets[i].position.x;
            centroidZ += IKTargets[i].position.z;
        }
        centroidX /= IKTargets.Count;
        centroidZ /= IKTargets.Count;

        return new Vector3(centroidX, CalculateYOffset(), centroidZ);

    }

    float CalculateYOffset()
    {
        float highestY = 0f;
        for (int i = 0; i < IKTargets.Count; i++)
        {
            float yDist = stepTargets[IKTargets[i]].position.y - YOffsets[IKTargets[i]];
            if (yDist > highestY)
            {
                highestY = yDist;
            }
        }

        float centerYDist = centerTarget.position.y - baseYOffset;
        if (centerYDist > highestY)
        {
            highestY = centerYDist;
        }

        return Mathf.Clamp(baseYOffset + highestY, baseYOffset, baseYOffset * 1000f);
    }

    void CreateTargetPoints()
    {
        if (stepTargetPrefab == null)
        {
            return;
        }

        stepTargets.Clear();
        isMoving.Clear();
        mainLegs.Clear();
        oppositeLegs.Clear();
        groundRootPoints.Clear();
        isGrounded.Clear();

        for (int i = 0; i < IKTargets.Count; i++)
        {
            GameObject target = Instantiate(stepTargetPrefab, IKTargets[i].position, Quaternion.identity);
            target.transform.SetParent(transform, true);
            stepTargets.Add(IKTargets[i], target.transform);
            groundRootPoints.Add(IKTargets[i], target.transform.position);
            isMoving.Add(IKTargets[i], false);
            isGrounded.Add(IKTargets[i], true);
            YOffsets.Add(IKTargets[i], target.transform.position.y);

            if (!oppositeLegs.Contains(IKTargets[i]) && !mainLegs.Contains(IKTargets[i]))
            {
                mainLegs.Add(IKTargets[i]);
                if ((i + 1) < IKTargets.Count)
                {
                    if (!oppositeLegs.Contains(IKTargets[i + 1]) && !mainLegs.Contains(IKTargets[i + 1]))
                    {
                        oppositeLegs.Add(IKTargets[i + 1]);
                    }
                }
            }
        }

        GameObject _centerTarget = Instantiate(stepTargetPrefab, bodyTransform.position, Quaternion.identity);
        _centerTarget.transform.SetParent(bodyTransform, true);
        centerTarget = _centerTarget.transform;

        if (headAimTarget != null)
        {
            GameObject _headAimTarget = Instantiate(stepTargetPrefab, headAimTarget.position, Quaternion.identity);
            _headAimTarget.transform.SetParent(transform, true);
            lookTarget = _headAimTarget.transform;
        }

        dampMoveTargets.Clear();
        foreach (Transform target in dampedTargets)
        {
            GameObject _dampTarget = new GameObject("TargetPoint");
            _dampTarget.transform.SetParent(transform, true);
            _dampTarget.transform.position = target.position;
            _dampTarget.transform.rotation = Quaternion.identity;
            dampMoveTargets.Add(_dampTarget.transform);
        
        }
        
    }

    float CalculateRotationZ()
    {
        float centroidLeftX = 0f;
        float centroidLeftY = 0f;
        float centroidLeftZ = 0f;
        float centroidRightX = 0f;
        float centroidRightY = 0f;
        float centroidRightZ = 0f;
        for (int i = 0; i < mainLegs.Count; i++)
        {
            centroidLeftX += mainLegs[i].position.x;
            centroidLeftY += mainLegs[i].position.y;
            centroidLeftZ += mainLegs[i].position.z;

            centroidRightX += oppositeLegs[i].position.x;
            centroidRightY += oppositeLegs[i].position.y;
            centroidRightZ += oppositeLegs[i].position.z;
        }
        centroidLeftX /= mainLegs.Count;
        centroidLeftY /= mainLegs.Count;
        centroidLeftZ /= mainLegs.Count;

        centroidRightX /= oppositeLegs.Count;
        centroidRightY /= oppositeLegs.Count;
        centroidRightZ /= oppositeLegs.Count;

        Vector3 rightPos = new Vector3(centroidRightX, centroidRightY, centroidRightZ);
        Vector3 leftPos = new Vector3(centroidLeftX, centroidLeftY, centroidLeftZ);

        Vector3 rotationVector = (leftPos - rightPos).normalized;
        rotationVector = new Vector3(rotationVector.x, rotationVector.y, 0f);
        return Mathf.Atan2(rotationVector.y, rotationVector.x) * Mathf.Rad2Deg;
    }

    float CalculateRotationX()
    {
        return 0f;
    }

    void UpdateLegRotations()
    {
        for(int i = 0; i < IKTargets.Count; i++)
        {

            float rotationY = stepTargets[IKTargets[i]].eulerAngles.y;
            IKTargets[i].eulerAngles = new Vector3(IKTargets[i].eulerAngles.x, rotationY, IKTargets[i].eulerAngles.z);
            Vector3 targetPos = (isGrounded[IKTargets[i]]) ? groundRootPoints[IKTargets[i]] : stepTargets[IKTargets[i]].position;
            Vector3 startPos = IKTargets[i].position;
            startPos += new Vector3(0f, footOffsetFromGround, 0f);

            Vector3 rotationDir = targetPos - startPos;

            Quaternion targetRotation = Quaternion.FromToRotation(IKTargets[i].up, rotationDir.normalized) * IKTargets[i].rotation;

            float speed = 10.0f;
            IKTargets[i].rotation = Quaternion.Slerp(IKTargets[i].rotation, targetRotation, speed * Time.deltaTime);
        }
    }

    void UpdateLegPositions()
    {
        for(int i = 0; i < IKTargets.Count; i++)
        {
            if (oppositeLegs.Contains(IKTargets[i]))
            {
                continue;
            }

            float stepThreshold = stepDistanceThreshold;

            if (nonUniformStepDistanceThreshold)
            {
                if (i >= 0 && i < stepDistanceThresholds.Count)
                {
                    stepThreshold = stepDistanceThresholds[i];
                }  
            }

            Vector2 currentPos = new Vector2(IKTargets[i].position.x, IKTargets[i].position.z);
            Vector2 targetPos = new Vector2(stepTargets[IKTargets[i]].position.x, stepTargets[IKTargets[i]].position.z);

            if (Vector3.Distance(currentPos, targetPos) > stepThreshold)
            {
                if(!isMoving[IKTargets[i]] && OppositeLegGrounded(IKTargets[i]))
                {
                    StartCoroutine(MoveMainLegs());
                    return;
                }
                
            }
        }
    }

    IEnumerator MoveMainLegs()
    {
        foreach (Transform target in mainLegs)
        {
            StartCoroutine(MoveLeg(IKTargets.IndexOf(target), stepTargets[target].position));
            yield return new WaitForSeconds(0.02f);
        }
    }

    private bool OppositeLegGrounded(Transform leg)
    {
        if (mainLegs.Contains(leg))
        {
            int legIndex = mainLegs.IndexOf(leg);

            if (legIndex < 0 || legIndex >= oppositeLegs.Count)
            {
                return true;
            }

            return !isMoving[oppositeLegs[legIndex]];
        }

        if (oppositeLegs.Contains(leg))
        {
            int legIndex = oppositeLegs.IndexOf(leg);

            if (legIndex < 0 || legIndex >= mainLegs.Count)
            {
                return true;
            }
            
            return !isMoving[mainLegs[legIndex]];
        }

        return true;
    }

    IEnumerator MoveLeg(int index, Vector3 targetPos)
    {
        isMoving[IKTargets[index]] = true;
        isGrounded[IKTargets[index]] = false;

        Vector3 startPos = IKTargets[index].position;

        Vector2 startPosFlat = new Vector2(startPos.x, startPos.z);
        Vector2 targetPosFlat = new Vector2(targetPos.x, targetPos.z);
        Vector2 overStepVector = (targetPosFlat - startPosFlat) * overStepMultiplier;
        targetPosFlat = startPosFlat + overStepVector;
        targetPos = new Vector3(targetPosFlat.x, targetPos.y, targetPosFlat.y);

        targetPos += new Vector3(0f, footOffsetFromGround, 0f);
        
        float timeElapsed = 0f;

        float duration = gaitDuration;
        if (nonUniformGaitDuration)
        {
            if (index >= 0 && index < gaitDurations.Count)
            {
                duration = gaitDurations[index];
            }
        }


        while (timeElapsed < duration)
        {
            

            timeElapsed += Time.deltaTime;
            float t = timeElapsed / duration;

            IKTargets[index].position = Vector3.Lerp(startPos, targetPos, t);

            float stepYOffset = stepArc.Evaluate(t);
            IKTargets[index].position += Vector3.up * stepYOffset * stepHeight;


            yield return null;
        }

        IKTargets[index].position = targetPos;
        isGrounded[IKTargets[index]] = true;
        groundRootPoints[IKTargets[index]] = targetPos - new Vector3(0f, footOffsetFromGround, 0f);

        yield return new WaitForSeconds(afterStepWaitTime);
        isMoving[IKTargets[index]] = false;

        MoveOppositeLeg(IKTargets[index]);
    }

    void MoveOppositeLeg(Transform mainLeg)
    {
        if (!mainLegs.Contains(mainLeg))
        {
            return;
        }
        int mainLegIndex = mainLegs.IndexOf(mainLeg);

        if (mainLegIndex < 0 || mainLegIndex >= oppositeLegs.Count)
        {
            return;
        }
        int targetIndex = IKTargets.IndexOf(oppositeLegs[mainLegIndex]);
        Transform target = stepTargets[oppositeLegs[mainLegIndex]];
        StartCoroutine(MoveLeg(targetIndex, target.position));
    }

    Vector3 ClampAngleBetweenVectors(Vector3 fromDir, Vector3 toDir, float maxAngle)
    {
        // find the rotation step between the two vectors
        Quaternion rotationDiff = Quaternion.FromToRotation(fromDir, toDir);
        rotationDiff.ToAngleAxis(out float angle, out Vector3 axis);

        // if the angle exceeds the threshold, clamp it
        if (angle > maxAngle)
        {
            return Quaternion.AngleAxis(maxAngle, axis) * fromDir;
        }

        return toDir;
    }
}
 