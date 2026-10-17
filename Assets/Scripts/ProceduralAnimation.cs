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
    [SerializeField] private List<Transform> IKTargets = new List<Transform>();
    [SerializeField] private List<Transform> dampedTargets = new List<Transform>();
    private List<Transform> dampMoveTargets = new List<Transform>();
    [SerializeField] private float dampedTargetsYOffset = 5f;

    [SerializeField] private float afterStepWaitTime = 0.2f;
    private Dictionary<Transform, Transform> stepTargets = new Dictionary<Transform, Transform>();
    private List<Transform> mainLegs = new List<Transform>();
    private List<Transform> oppositeLegs = new List<Transform>();
    private Dictionary<Transform, bool> isMoving = new Dictionary<Transform, bool>();

    private Dictionary<Transform, float> YOffsets = new Dictionary<Transform, float>();
    private float baseYOffset;
    private float averageDeltaYBetweenTargets;
    private float targetY;
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
        baseYOffset = transform.position.y;
    }

    void FixedUpdate()
    {
        CalculateYOffset();
        UpdateLegPositions();

        transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, targetY, transform.position.z), Time.deltaTime * 5f);

        //Quaternion targetRotation = Quaternion.Euler(transform.eulerAngles + (Vector3.right * CalculateRotationX()) + (Vector3.forward * CalculateRotationZ()));

        //transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.deltaTime * 100f);
        
        if (headAimTarget != null)
        {
            headAimTarget.position = Vector3.Lerp(headAimTarget.position, lookTarget.position, Time.deltaTime * 7f);
        }

        if (dampedTargets.Count > 0)
        {
            for (int i = 0; i < dampedTargets.Count; i++)
            {
                dampedTargets[i].position = Vector3.Lerp(dampedTargets[i].position, dampMoveTargets[i].position + Vector3.up * (dampedTargetsYOffset), Time.deltaTime * 7f);
                dampedTargets[i].rotation = Quaternion.RotateTowards(dampedTargets[i].rotation, dampMoveTargets[i].rotation, Time.deltaTime * 500f);
            }
        }
    }

    public void AimAt(Vector3 targetPos)
    {
        lookTarget.position = targetPos;
    }

    void CalculateYOffset()
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

        targetY = Mathf.Clamp(baseYOffset + highestY, baseYOffset, baseYOffset * 1000f);
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

        for (int i = 0; i < IKTargets.Count; i++)
        {
            GameObject target = Instantiate(stepTargetPrefab, IKTargets[i].position, Quaternion.identity);
            target.transform.SetParent(transform, true);
            stepTargets.Add(IKTargets[i], target.transform);
            isMoving.Add(IKTargets[i], false);
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

        GameObject _centerTarget = Instantiate(stepTargetPrefab, transform.position, Quaternion.identity);
        _centerTarget.transform.SetParent(transform, true);
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
        List<Transform> leftTargets = new List<Transform>();
        List<Transform> rightTargets = new List<Transform>();

        foreach (Transform ikTarget in IKTargets)
        {
            if (stepTargets[ikTarget].localPosition.x > 0f)
            {
                rightTargets.Add(stepTargets[ikTarget]);
            }
            else
            {
                leftTargets.Add(stepTargets[ikTarget]);
            }
        }

        if (leftTargets.Count == 0) {return 0f;}
        if (rightTargets.Count == 0) {return 0f;}

        float avgLeftX = 0f;
        foreach (Transform target in leftTargets)
        {
            avgLeftX += target.localPosition.x;
        }
        avgLeftX /= leftTargets.Count;

        float avgRightX = 0f;
        foreach (Transform target in rightTargets)
        {
            avgRightX += target.localPosition.x;
        }
        avgRightX /= rightTargets.Count;

        float xDifference = avgRightX - avgLeftX;



        float avgLeftY = 0f;
        foreach (Transform target in leftTargets)
        {
            avgLeftY += target.localPosition.z;
        }
        avgLeftY /= leftTargets.Count;

        float avgRightY = 0f;
        foreach (Transform target in rightTargets)
        {
            avgRightY += target.localPosition.z;
        }
        avgRightY /= rightTargets.Count;

        float yDifference = avgRightY - avgLeftY;

        return Mathf.Atan(yDifference / xDifference);
    }

    float CalculateRotationX()
    {
        List<Transform> frontTargets = new List<Transform>();
        List<Transform> backTargets = new List<Transform>();

        foreach (Transform ikTarget in IKTargets)
        {
            if (stepTargets[ikTarget].localPosition.y < 0f)
            {
                frontTargets.Add(stepTargets[ikTarget]);
            }
            else
            {
                backTargets.Add(stepTargets[ikTarget]);
            }
        }

        if (frontTargets.Count == 0) {return 0f;}
        if (backTargets.Count == 0) {return 0f;}

        float avgFrontX = 0f;
        foreach (Transform target in frontTargets)
        {
            avgFrontX += target.localPosition.y;
        }
        avgFrontX /= frontTargets.Count;

        float avgBackX = 0f;
        foreach (Transform target in backTargets)
        {
            avgBackX += target.localPosition.y;
        }
        avgBackX /= backTargets.Count;

        float xDifference = avgFrontX - avgBackX;



        float avgFrontY = 0f;
        foreach (Transform target in frontTargets)
        {
            avgFrontY += target.localPosition.z;
        }
        avgFrontY /= frontTargets.Count;

        float avgBackY = 0f;
        foreach (Transform target in backTargets)
        {
            avgBackY += target.localPosition.z;
        }
        avgBackY /= backTargets.Count;

        float yDifference = avgFrontY - avgBackY;

        return Mathf.Atan(yDifference / xDifference);
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

            if (Vector3.Distance(IKTargets[i].position, stepTargets[IKTargets[i]].position) > stepThreshold)
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
}
 