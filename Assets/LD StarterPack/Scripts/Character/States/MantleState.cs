using UnityEngine;

public class MantleState : ICharacterState
{
    PlayerCharacter player;
    MantleZone zone;

    public MantleState(PlayerCharacter player, MantleZone zone)
    {
        this.player = player;
        this.zone = zone;
    }

    public void Enter()
    {
        player.Motor.CancelVerticalVelocity();
        player.Motor.SetOnLadder(true);
        Debug.Log("Climbing");
    }

    public void Tick()
    {
        player.transform.position = Vector3.MoveTowards(
            player.transform.position,
            zone.standPoint.position,
            player.stats.mantleMoveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(player.transform.position, zone.standPoint.position) < 0.05f)
            player.StateMachine.SetState(new GroundedState(player));
    }

    public void Exit() { }
}
