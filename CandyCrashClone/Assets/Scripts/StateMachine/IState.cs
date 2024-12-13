using Unity.VisualScripting;

public interface IState
{
    void SetOwner(StateMachine stateMachine);
    public void Enter();
    public void Execute();
    public void Exit();
}

