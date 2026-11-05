using UnityEngine;

[System.Serializable]
public class FirstPersonCameraMode : ICameraMode
{
    public float horizontalSensitivity = 3f;
    public float verticalSensitivity = 2.2f;
    public float minY = -80f;
    public float maxY = 80f;

    float yaw;
    float pitch;
    CameraController cam;

    public void Enter(CameraController controller)
    {
        cam = controller;
        yaw = cam.target.eulerAngles.y;
        pitch = 0f;

        cam.cameraTransform.localPosition = Vector3.zero;
    }

    public void Tick()
    {
        float mouseX = Input.GetAxis("Mouse X") * horizontalSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * verticalSensitivity;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minY, maxY);

        cam.target.rotation = Quaternion.Euler(0, yaw, 0);
        cam.cameraPivot.localRotation = Quaternion.Euler(pitch, 0, 0);
    }

    public void Exit() { }
}
