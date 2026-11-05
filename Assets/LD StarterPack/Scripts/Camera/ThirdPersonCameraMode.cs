using UnityEngine;

[System.Serializable]
public class ThirdPersonCameraMode : ICameraMode
{
    public float horizontalSensitivity = 3f;
    public float verticalSensitivity = 2.2f;
    
    public float minY = -30f;
    public float maxY = 65f;
    
    [Header("Collision")]
    [SerializeField] float cameraRadius = 0.2f;
    [SerializeField] LayerMask collisionMask;
    [SerializeField] float minDistance = 0.5f;
    [SerializeField] float maxDistance = 4f;
    float distance = 4f;

    CameraController cam;
    float yaw;
    float pitch;

    public void Enter(CameraController controller)
    {
        cam = controller;
        yaw = cam.target.eulerAngles.y;
        pitch = 15f;
    }

    public void Tick()
    {
        float mouseX = Input.GetAxis("Mouse X") * horizontalSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * verticalSensitivity;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minY, maxY);

        cam.cameraPivot.rotation = Quaternion.Euler(pitch, yaw, 0);
        
        HandleCollision();
        ScrollDistance();
    }

    void ScrollDistance()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
            maxDistance = Mathf.Clamp(maxDistance-scroll, minDistance, 10);
    }
    
    public void HandleCollision()
    {
        Vector3 desiredPos = cam.cameraPivot.position - cam.cameraPivot.forward * maxDistance;

        if (Physics.SphereCast(
                cam.cameraPivot.position,
                cameraRadius,
                (desiredPos - cam.cameraPivot.position).normalized,
                out RaycastHit hit,
                maxDistance,
                collisionMask
            ))
        {
            float dist = hit.distance - 0.05f;
            distance = Mathf.Clamp(dist, minDistance, maxDistance);
        }
        else
        {
            distance = Mathf.Lerp(
                distance,
                maxDistance,
                Time.deltaTime * 12f
            );
        }

        cam.cameraTransform.position = cam.cameraPivot.position - cam.cameraPivot.forward * distance;
    }

    public void Exit() { }
}
