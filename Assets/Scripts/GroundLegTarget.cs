using UnityEngine;

public class GroundLegTarget : MonoBehaviour
{
    RaycastHit hit;
    [SerializeField] private float raycastHeight = 3f;

    private Vector3 raycastPoint;

    void FixedUpdate()
    {
        raycastPoint = new Vector3(transform.position.x, transform.position.y + raycastHeight, transform.position.z);
        if (Physics.Raycast(raycastPoint, -Vector3.up, out hit, Mathf.Infinity))
        {
            transform.position = hit.point;
        }
    }
}
