using UnityEngine;

public class LadderState : ICharacterState
{
    PlayerCharacter player;
    Ladder ladder;

    public LadderState(PlayerCharacter player, Ladder ladder)
    {
        this.player = player;
        this.ladder = ladder;
    }

    public void Enter()
    {
        player.Motor.SetOnLadder(true);
        Debug.Log("OnLadder");
        
    }

    public void Tick()
    {
        Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        player.Motor.MoveLadder(input, player.stats.climbSpeed, player.CameraController.transform);

        if (player.IsGrounded || input.y <= 0f || player.transform.position.y > ladder.topExit.transform.position.y)
        {
            player.StateMachine.SetState(new FallState(player));
        }

    }


    public void Exit()
    {
        player.Motor.SetOnLadder(false);

    }
}
