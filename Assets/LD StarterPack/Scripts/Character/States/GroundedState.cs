using UnityEngine;

public class GroundedState : ICharacterState
{
    PlayerCharacter player;

    public GroundedState(PlayerCharacter player)
    {
        this.player = player;
    }

    public void Enter()
    {
        Debug.Log("Grounded");
        player.Motor.SetOnLadder(false);
    }

    public void Tick()
    {
        if (!player.IsGrounded)
        {
            player.StateMachine.SetState(new FallState(player));
            return;
        }
        
        if (player.Interactor.TryEnterLadder(out Ladder ladder))
            player.StateMachine.SetState(new LadderState(player, ladder));


        Vector2 input = new Vector2(
            Input.GetAxis("Horizontal"),
            Input.GetAxis("Vertical")
        );

        if (Input.GetKeyDown(player.jumpKey) && player.IsStableGround)
            player.Motor.Jump();

        Vector3 forward = player.CameraController.cameraPivot.forward;
        Vector3 right = player.CameraController.cameraPivot.right;

        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        Vector3 move = forward * input.y + right * input.x;

        if (move.sqrMagnitude > 1f)
            move.Normalize();
        
        if (player.CameraController.currentModeType == CameraModeType.ThirdPerson)
            player.Motor.RotateCharacter(move, player.transform);

        if (!player.IsStableGround)
        {
            player.Motor.ApplySlide(player.GroundHit.normal);
            return;
        }

        Vector3 surfaceMove = Vector3.ProjectOnPlane(move, player.GroundHit.normal);

        float speed = player.stats.moveSpeed;

        if (player.SprintHeld())
            speed = player.stats.sprintSpeed;

        if (player.CrouchHeld())
            speed = player.stats.crouchSpeed;

        player.Motor.MoveSurface(surfaceMove, speed);
    }

    public void Exit() { }
}
