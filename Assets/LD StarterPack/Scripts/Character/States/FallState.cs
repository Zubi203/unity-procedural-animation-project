using UnityEngine;

public class FallState : ICharacterState
{
    PlayerCharacter player;

    public FallState(PlayerCharacter player)
    {
        this.player = player;
    }

    public void Enter() { Debug.Log("Falling"); }

    public void Tick()
    {
        Vector2 input = new Vector2(
            Input.GetAxis("Horizontal"),
            Input.GetAxis("Vertical")
        );
        
        Vector3 forward = player.CameraController.cameraPivot.forward;
        Vector3 right = player.CameraController.cameraPivot.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 move = forward * input.y + right * input.x;

        if (move.sqrMagnitude > 1f)
            move.Normalize();
            
        if (player.CameraController.currentModeType == CameraModeType.ThirdPerson)
            player.Motor.RotateCharacter(move, player.transform);

        player.Motor.MoveSurface(move, player.stats.airControl);

        if (player.IsGrounded)
            player.StateMachine.SetState(new GroundedState(player));
        
        if (player.Interactor.TryEnterLadder(out Ladder ladder))
            player.StateMachine.SetState(new LadderState(player, ladder));
        
        if (player.TryGetMantleZone(out MantleZone zone))
        {
            player.StateMachine.SetState(new MantleState(player, zone));
        }


    }

    public void Exit() { }
}
