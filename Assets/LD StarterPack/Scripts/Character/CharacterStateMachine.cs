public class CharacterStateMachine
{
    ICharacterState currentState;

    public void SetState(ICharacterState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState.Enter();
    }

    public void Tick()
    {
        currentState?.Tick();
    }
}