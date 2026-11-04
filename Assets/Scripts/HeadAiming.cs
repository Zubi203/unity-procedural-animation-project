using UnityEngine;
using UnityEngine.Animations.Rigging;
using System;
using System.Collections.Generic;

public class HeadAiming : MonoBehaviour
{
    [SerializeField] private String lookMaskName = "LookObject";
    [SerializeField] private MultiAimConstraint headAimConstraint;
    
    Vector3 originalLocalPos;
    private List<Transform> ObjectsInRange = new List<Transform>();
    private Transform closestObject;

    void Awake()
    {
        originalLocalPos = headAimConstraint.data.sourceObjects[0].transform.localPosition;
    }

    void Update()
    {
        if (ObjectsInRange.Count == 0)
        {
            headAimConstraint.data.sourceObjects[0].transform.localPosition = Vector3.Lerp(headAimConstraint.data.sourceObjects[0].transform.localPosition, originalLocalPos, Time.deltaTime * 5f);
            return;
        }

        CheckClosestObject();

        if (closestObject == null) return;
        headAimConstraint.data.sourceObjects[0].transform.position = Vector3.Lerp(headAimConstraint.data.sourceObjects[0].transform.position, closestObject.position, Time.deltaTime * 2f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer(lookMaskName)) return;

        ObjectsInRange.Add(other.transform);
        
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer(lookMaskName)) return;
        if (!ObjectsInRange.Contains(other.transform)) return;

        ObjectsInRange.Remove(other.transform);
        
    }

    void CheckClosestObject()
    {
        float closestDist = Mathf.Infinity;

        foreach (Transform obj in ObjectsInRange)
        {
            float dist = Vector3.Distance(obj.position, headAimConstraint.data.constrainedObject.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestObject = obj;
            }
        }
    }
}
