using UnityEngine;

public enum CameraModeType
{
    FirstPerson,
    ThirdPerson
}

public class CameraController : MonoBehaviour
{
    public Transform target;
    public Transform cameraPivot;
    public Transform cameraTransform;
    public GameObject view;
    
    [Header("Camera Modes")]
    public FirstPersonCameraMode fpsMode;
    public ThirdPersonCameraMode tpsMode;

    [Header("Switching")]
    public CameraModeType startMode = CameraModeType.FirstPerson;
    public KeyCode switchModeKey = KeyCode.V;
    
    [HideInInspector]
    public CameraModeType currentModeType;
    ICameraMode currentMode;
    CursorController cursor;

    private void Start()
    {
        cursor = GetComponent<CursorController>();
        target = transform;
        SetMode(startMode);
    }

    private void LateUpdate()
    {
        if (!cursor.IsLocked)
            return;
        
        if (Input.GetKeyDown(switchModeKey))
        {
            ToggleMode();
        }
        
        currentMode?.Tick();
    }

    public void ToggleMode()
    {
        if (currentModeType == CameraModeType.FirstPerson)
            SetMode(CameraModeType.ThirdPerson);
        else
            SetMode(CameraModeType.FirstPerson);
        
    }

    public void SetMode(CameraModeType mode)
    {
        currentMode?.Exit();

        currentModeType = mode;

        if (mode == CameraModeType.FirstPerson)
        {
            currentMode = fpsMode;
            view.SetActive(false);
        }
        else
        {
            currentMode = tpsMode;
            view.SetActive(true);
        }

        currentMode.Enter(this);
    }
}