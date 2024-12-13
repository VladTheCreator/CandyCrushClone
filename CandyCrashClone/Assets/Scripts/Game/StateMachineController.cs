using UnityEngine;

public class StateMachineController : MonoBehaviour
{
    [SerializeField] private CandySwapper swapper;
    private StateMachine stateMachine;
    void Start()
    {
        stateMachine = new StateMachine();
        swapper.SetOwner(stateMachine);
        stateMachine.ChangeState(swapper);
    }

    void Update()
    {
        stateMachine.Update();
    }
}
