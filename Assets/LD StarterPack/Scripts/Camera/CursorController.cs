using System;
using UnityEngine;

public class CursorController : MonoBehaviour
{
    [SerializeField] KeyCode toggleKey = KeyCode.Escape;

    bool locked = true;

    void Start()
    {
        LockCursor();
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (locked) UnlockCursor();
            else LockCursor();
        }
        
        if (!locked && Input.GetMouseButtonDown(0))
        {
            LockCursor();
        }
    }

    void LockCursor()
    {
        locked = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void UnlockCursor()
    {
        locked = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
            LockCursor();
        else
            UnlockCursor();
    }
    

    public bool IsLocked => locked;
}