using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardFiller : MonoBehaviour, IState
{
    private GridGenerator generator;
    private StateMachine owner;
    private MatchChecker matchChecker;
    private CandySwapper swapper;

    private bool _fillEmptyIndexesCoroutineIsRunning;
    public bool FillEmptyIndexesCoroutineIsRunning => _fillEmptyIndexesCoroutineIsRunning;
    private void Awake()
    {
        generator = GetComponent<GridGenerator>();
        matchChecker = GetComponent<MatchChecker>();
        swapper = GetComponent<CandySwapper>();
    }
    public IEnumerator FillEmptyIndexes()
    {
        _fillEmptyIndexesCoroutineIsRunning = true;
        yield return new WaitForSeconds(1f);
        List<Vector2Int> emptyIndexes = generator.FindEmpty();
        for (int i = 0; i < emptyIndexes.Count; i++)
        {
            Vector2 position = generator.IndexToPosition(emptyIndexes[i]);
            Candy prefab = generator.GetRandomCandyPrefab();
            Candy instance = Instantiate(prefab, position, Quaternion.identity);
            generator.SetCandyByIndex(emptyIndexes[i], instance);
        }
        if (matchChecker.FindMatches())
        {
            owner.ChangeState(matchChecker);
        }
        else
        {
            owner.ChangeState(swapper);
        }
        _fillEmptyIndexesCoroutineIsRunning = false;
    }

    public void Enter()
    {
        StartCoroutine(FillEmptyIndexes());
    }

    public void Execute()
    {

    }

    public void Exit()
    {

    }

    public void SetOwner(StateMachine stateMachine)
    {
        owner = stateMachine;
    }
}
