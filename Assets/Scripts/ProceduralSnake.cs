using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Splines;

public class ProceduralSnake : MonoBehaviour
{
    [SerializeField] private Transform armatureRoot;
    [SerializeField] private Transform parentTransform;
    [SerializeField] private GameObject bodySegmentPrefab;
    [SerializeField] private int bodyLength = 20;
    [SerializeField] private float maxDistanceBetweenSegments = 0.5f;
    [SerializeField] private float minDistanceBetweenSegments = 0.3f;
    [SerializeField] private AnimationCurve bodyShapeCurve;
    [SerializeField] private AnimationCurve slitherForceCurve;
    [SerializeField] private float slitherMagnitude = 0.5f;
    [SerializeField] private float slitherPeriod = 0.5f;
    [SerializeField] private float slitherPhaseShiftSpeed = 10f;
    [SerializeField] private float spineAngleThreshold = 120f;
    [SerializeField] private float headOscillationMagnitude = 2f;

    private List<Transform> bodySegments = new List<Transform>();
    private float slitherTime = 0f;
    private bool isSlithering = false;

    void Start()
    {
        // Instantiate body segments

        if (bodySegmentPrefab == null || parentTransform == null) return;

        if (armatureRoot != null)
        {
            Transform[] childTransforms = armatureRoot.GetComponentsInChildren<Transform>();

            float constraintDist = Vector3.Distance(armatureRoot.position, armatureRoot.GetChild(0).position);
            minDistanceBetweenSegments = constraintDist;
            maxDistanceBetweenSegments = constraintDist;

            foreach (Transform child in childTransforms)
            {
                GameObject obj = Instantiate(bodySegmentPrefab, child.position, Quaternion.identity, parentTransform);
                child.SetParent(obj.transform, true);
                bodySegments.Add(obj.transform);
            }
        }
        else
        {
            for (int i = 0; i < bodyLength; i++)
            {
                GameObject obj = Instantiate(bodySegmentPrefab, transform.position, Quaternion.identity, parentTransform);
                obj.transform.position += new Vector3(0f, 0f, (i + 1) * (maxDistanceBetweenSegments + minDistanceBetweenSegments) / -2);
                bodySegments.Add(obj.transform);

                float bodySegmentPercent = (float)i / (float)bodyLength;
                obj.transform.localScale *= bodyShapeCurve.Evaluate(bodySegmentPercent);
            }
        }

    }

    void FixedUpdate()
    {
        EvaluateBodyRotations();
        EvaluateBodyConstraints();

        if (!isSlithering)
        {
            SlitherStop();
        }
    }


    public void Slither(bool slithering)
    {
        if (!slithering)
        {
            isSlithering = false;
            return;
        }

        isSlithering = true;

        if (slitherTime < 360f)
        {
            slitherTime += Time.deltaTime;
        }
        else
        {
            slitherTime = 0.0f;
        }

        for (int i = 0; i < bodySegments.Count; i++)
        {
            float phaseOffset = slitherPhaseShiftSpeed * i;
            float slitherForce = slitherMagnitude * slitherForceCurve.Evaluate((float)i / (float)bodySegments.Count);
            float slither =  Mathf.Sin((slitherTime - phaseOffset) / slitherPeriod) * slitherForce;
            if (bodySegments[i].TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                rb.AddForce(transform.right * slither, ForceMode.VelocityChange);
            }

            float headOscillation = Mathf.Sin(slitherTime / slitherPeriod) * headOscillationMagnitude;
            transform.Translate(Vector3.right * headOscillation * Time.deltaTime, Space.Self);
        }
        
    }

    void SlitherStop()
    {
        for (int i = 0; i < bodySegments.Count; i++)
        {
            if (bodySegments[i].TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                Vector3 targetVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
                rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, Time.fixedDeltaTime * 10f);
            }
        }
    }

    void EvaluateBodyRotations()
    {
        //Loop through list of body segments
        //calculate the angle for each segment relative to its previous and next segments
        //if angle is less than the threshold, clamp it to always be greater than the threshold

        for (int i = 0; i < bodySegments.Count; i++)
        {
            Vector3 currentSeg = bodySegments[i].position;
            Vector3 targetSeg = (i == 0) ? transform.position : bodySegments[i-1].position;
            
            Vector3 rawDirection = (targetSeg - currentSeg).normalized;
            
            Vector3 referenceDirection = (i <= 1) ? transform.forward : (bodySegments[i-2].position - bodySegments[i-1].position).normalized;

            Vector3 constrainedDirection = ClampAngleBetweenVectors(referenceDirection, rawDirection, spineAngleThreshold);

            bodySegments[i].position = targetSeg - (constrainedDirection * maxDistanceBetweenSegments);
            
            bodySegments[i].rotation = Quaternion.LookRotation(constrainedDirection);
        }
    }

    void EvaluateBodyConstraints()
    {
        if (bodySegments.Count == 0) return;

        //Loop through list of body segments
        //if the distance between a body segment and its previous one is outside the constrained range, clamp it to the range
        for (int i = 0; i < bodySegments.Count; i++)
        {
            Vector3 currentSegmentPosition = bodySegments[i].position;
            Vector3 previousSegmentPosition;
            if (i > 0)
            {
                previousSegmentPosition = bodySegments[i - 1].position;
            }
            else
            {
                previousSegmentPosition = transform.position;
            }

            Vector3 distanceFromPreviousSegment = currentSegmentPosition - previousSegmentPosition;
            bodySegments[i].rotation = Quaternion.LookRotation(-distanceFromPreviousSegment);
            if (distanceFromPreviousSegment.magnitude < minDistanceBetweenSegments)
            {
                distanceFromPreviousSegment = distanceFromPreviousSegment.normalized * minDistanceBetweenSegments;
                currentSegmentPosition = previousSegmentPosition + distanceFromPreviousSegment;
            }
            else if (distanceFromPreviousSegment.magnitude > maxDistanceBetweenSegments)
            {
                distanceFromPreviousSegment = distanceFromPreviousSegment.normalized * maxDistanceBetweenSegments;
                currentSegmentPosition = previousSegmentPosition + distanceFromPreviousSegment;
            }
            bodySegments[i].position = currentSegmentPosition;
        }
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
