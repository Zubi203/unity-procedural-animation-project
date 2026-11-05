using UnityEngine;
public class CharacterMotor
{
    CharacterController controller;
    CharacterStats stats;
    
    float verticalVelocity;

    float characterYaw;
    bool grounded;
    bool isOnLadder = false;
    bool isOnMantle = false;

    public CharacterMotor(CharacterController controller, CharacterStats stats)
    {
        this.controller = controller;
        this.stats = stats;
        characterYaw = controller.transform.eulerAngles.y;
    }
    
    public void SetOnLadder(bool value)
    {
        isOnLadder = value;
        if (isOnLadder)
            verticalVelocity = 0f;
    }

    public void TickGravity(bool grounded)
    {
        if (isOnLadder || isOnMantle)
            return;
        
        if (grounded && verticalVelocity < 0)
            verticalVelocity = -2f;
        else
            verticalVelocity += stats.gravity * Time.deltaTime;

        Vector3 gravityMove = Vector3.up * verticalVelocity;
        controller.Move(gravityMove * Time.deltaTime);
    }

    public void Jump()
    {
        verticalVelocity = Mathf.Sqrt(-2f * Physics.gravity.y * stats.jumpHeight);
    }

    public void CancelVerticalVelocity()
    {
        verticalVelocity = 0f;
    }
    
    
    public void MoveSurface(Vector3 dir, float speed)
    {
        controller.Move(speed * Time.deltaTime * dir);
    }
    
    
    public void MoveLadder(Vector2 input, float speed, Transform view)
    {
        Vector3 dir = new Vector3(input.x, input.y, 0);
        dir = Quaternion.Euler(0, view.eulerAngles.y, 0) * dir;

        controller.Move(speed * Time.deltaTime * dir);
    }


    public void ApplySlide(Vector3 normal)
    {
        Vector3 slideDir = Vector3.ProjectOnPlane(Vector3.down, normal);
        controller.Move(stats.slideGravity * Time.deltaTime * slideDir);
    }
    
    
    public void RotateCharacter(Vector3 moveDir, Transform player)
    {
        if (moveDir.sqrMagnitude < 0.01f)
            return;

        float targetYaw = Mathf.Atan2(moveDir.x, moveDir.z) * Mathf.Rad2Deg;

        characterYaw = Mathf.LerpAngle(
            characterYaw,
            targetYaw,
            10f * Time.deltaTime
        );

        player.rotation = Quaternion.Euler(0f, characterYaw, 0f);
    }

    

}
