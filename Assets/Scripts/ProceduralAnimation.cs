using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

public class ProceduralAnimation : MonoBehaviour
{
    [SerializeField] private float footOffsetFromGround = 0.5f;
    [SerializeField] private AnimationCurve stepArc;
    [SerializeField] private float stepHeight = 1f;
    [SerializeField] private GameObject stepTargetPrefab;
    [SerializeField] private List<Transform> IKTargets = new List<Transform>();
    [SerializeField] private List<float> stepDelays = new List<float>();
    [SerializeField] private float afterStepWaitTime = 0.2f;
    private List<Transform> stepTargets = new List<Transform>();
    private List<bool> isMoving = new List<bool>();

    private List<float> YOffsets = new List<float>();
    private float baseYOffset;
    private float averageDeltaYBetweenTargets;
    private float targetY;

    [SerializeField] private float stepDistanceThreshold = 1f;
    [SerializeField] private float gaitDuration = 0.5f;
    [SerializeField] private int gaitSmoothness = 4;

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

        
    }

    void CalculateYOffset()
    {
        float averageY = 0f;
        for (int i = 0; i < IKTargets.Count; i++)
        {
            averageY += stepTargets[i].position.y - YOffsets[i];
        }

        averageY = averageY / IKTargets.Count;

        targetY = Mathf.Clamp(baseYOffset + averageY * 5f, baseYOffset, baseYOffset * 100f);
    }

    void CreateTargetPoints()
    {
        if (stepTargetPrefab == null)
        {
            return;
        }

        stepTargets.Clear();
        isMoving.Clear();

        foreach (Transform ikTarget in IKTargets)
        {
            GameObject target = Instantiate(stepTargetPrefab, ikTarget.position, Quaternion.identity);
            target.transform.SetParent(transform, true);
            stepTargets.Add(target.transform);
            isMoving.Add(false);
            YOffsets.Add(target.transform.position.y);
        }

        if (stepDelays.Count < IKTargets.Count)
        {
            for(int i = 0; i < IKTargets.Count - stepDelays.Count; i++)
            {
                stepDelays.Add(0f);
            }
        }
    }

    void UpdateLegPositions()
    {
        for(int i = 0; i < IKTargets.Count; i++)
        {
            if (Vector3.Distance(IKTargets[i].position, stepTargets[i].position) > stepDistanceThreshold)
            {
                if(!isMoving[i])
                {
                    StartCoroutine(MoveLeg(i, stepTargets[i].position));
                }
                
            }
        }
    }

    IEnumerator MoveLeg(int index, Vector3 targetPos)
    {
        isMoving[index] = true;
        yield return new WaitForSeconds(stepDelays[index]);

        targetPos += new Vector3(0f, footOffsetFromGround, 0f);
        Vector3 startPos = IKTargets[index].position;

        float timeElapsed = 0f;


        while (timeElapsed < gaitDuration)
        {
            

            timeElapsed += Time.deltaTime;
            float t = timeElapsed / gaitDuration;

            IKTargets[index].position = Vector3.Lerp(startPos, targetPos, t);

            float stepYOffset = stepArc.Evaluate(t);
            IKTargets[index].position += Vector3.up * stepYOffset * stepHeight;


            yield return null;
        }

        IKTargets[index].position = targetPos;

        yield return new WaitForSeconds(afterStepWaitTime);
        isMoving[index] = false;
    }
}
 