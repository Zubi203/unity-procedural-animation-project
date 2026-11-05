using UnityEngine;

public interface ICameraMode
{
    void Enter(CameraController controller);
    void Tick();
    void Exit();
}