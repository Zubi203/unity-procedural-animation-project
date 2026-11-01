using UnityEngine;
using System;
using System.Collections.Generic;

public class ProceduralSnake : MonoBehaviour
{
    [SerializeField] private Transform parentTransform;
    [SerializeField] private GameObject bodySegmentPrefab;
    [SerializeField] private int bodyLength = 20;
    [SerializeField] private float maxDistanceBetweenSegments = 0.5f;
    [SerializeField] private float minDistanceBetweenSegments = 0.3f;
    [SerializeField] private AnimationCurve bodyShapeCurve;
    [SerializeField] private AnimationCurve slitherForceCurve;
    [SerializeField] private float slitherMagnitude = 5f;
    [SerializeField] private float slitherFrequency = 1f;
    [SerializeField] private float spineAngleThreshold = 120f;

    private List<Transform> bodySegments = new List<Transform>();

    void Start()
    {
        // Instantiate body segments

        if (bodySegmentPrefab == null || parentTransform == null) return;

        for (int i = 0; i < bodyLength; i++)
        {
            GameObject obj = Instantiate(bodySegmentPrefab, transform.position, Quaternion.identity, parentTransform);
            obj.transform.position += new Vector3(0f, 0f, (i + 1) * (maxDistanceBetweenSegments + minDistanceBetweenSegments) / -2);
            bodySegments.Add(obj.transform);

            float bodySegmentPercent = (float)i / (float)bodyLength;
            obj.transform.localScale *= bodyShapeCurve.Evaluate(bodySegmentPercent);
        }
    }

    void FixedUpdate()
    {
        //Slither();
        EvaluateBodyConstraints();
        EvaluateBodyRotations();
    }

    void Slither()
    {
        float offset = 0f;
        for (int i = 0; i < bodySegments.Count; i++)
        {
            float forceMultiplier = slitherForceCurve.Evaluate((float)i / bodySegments.Count) * slitherMagnitude;
            float slitherForce = Mathf.Sin((Time.time + offset) / slitherFrequency) * forceMultiplier;

            if (bodySegments[i].TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                rb.AddForce(transform.right * slitherForce, ForceMode.VelocityChange);
            }
            
            float segmentOffset = Time.deltaTime * i;
            offset += segmentOffset;
            
        }
    }

    void EvaluateBodyRotations()
    {
        //Loop through list of body segments
        //calculate the angle for each segment relative to its previous and next segments
        //if angle is less than the threshold, clamp it to always be greater than the threshold

        for (int i = 0; i < bodySegments.Count; i++)
        {
            if (i == 0 || i == bodySegments.Count - 1)
            {
                continue;
            }

            Vector3 previousSegmentPos = bodySegments[i - 1].position;
            Vector3 currentSegmentPos = bodySegments[i].position;
            Vector3 nextSegmentPos = bodySegments[i + 1].position;
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
}
