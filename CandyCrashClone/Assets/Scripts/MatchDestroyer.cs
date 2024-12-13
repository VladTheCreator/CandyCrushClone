using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MatchDestroyer : MonoBehaviour, IState
{
    private GridGenerator generator;
    private CandyMover candyMover;
    private ScoreCounter scoreCounter;
    private List<Candy> candiesToDestroy;
    private List<Vector2Int> candiesToDestroyIndexes;
    private bool _destroyMatchesCoroutineIsRunning;
    private StateMachine owner;
    private Dictionary<Vector2Int, MatchType> centerInMatchAndMatchType;
    public event Action OnDestroyMatches;
    public bool DestroyMatchesCoroutineIsRunning => _destroyMatchesCoroutineIsRunning;

    public List<Vector2Int> CandiesToDestroyIndexes => candiesToDestroyIndexes;
    private void Awake()
    {
        candyMover = GetComponent<CandyMover>();
        generator = GetComponent<GridGenerator>();
        scoreCounter = GetComponent<ScoreCounter>();
        candiesToDestroy = new List<Candy>();
        candiesToDestroyIndexes = new List<Vector2Int>();
    }
    private void AddToDestroyList(List<Candy> range)
    {
        foreach (Candy candy in range)
        {
            if (!candiesToDestroy.Contains(candy))
                candiesToDestroy.Add(candy);
        }
    }
    public void SetCenterInMatchAndMatchType(Dictionary<Vector2Int, MatchType> centerInMatchAndMatchType)
    {
        this.centerInMatchAndMatchType = centerInMatchAndMatchType;
    }
    internal IEnumerator DestroyMatches()
    {
        _destroyMatchesCoroutineIsRunning = true;
        yield return new WaitForSeconds(1f);
        ClearCandiesToDestroy();
        List<Vector2Int> centersInMatches = centerInMatchAndMatchType.Keys.ToList();
        for (int i = 0; i < centersInMatches.Count; i++)
        {
            MatchType matchType = centerInMatchAndMatchType[centersInMatches[i]];
            int column = centersInMatches[i].x;
            int row = centersInMatches[i].y;
            List<Candy> rightLeftRange = generator.CurrentLeftAndRightCandy(column, row);
            List<Candy> topBottomRange = generator.CurrentTopAndBottomCandy(column, row);
            if (matchType == MatchType.horizontal)
            {
                AddToDestroyList(rightLeftRange);
                RememberIndexesOfDestroyedCandiesHorizontal(column, row);
            }
            else if (matchType == MatchType.vertical)
            {
                AddToDestroyList(topBottomRange);
                RememberIndexesOfDestroyedCandiesVertical(column, row);
            }
            else if (matchType == MatchType.horizontalAndVertical)
            {
                AddToDestroyList(rightLeftRange);
                RememberIndexesOfDestroyedCandiesHorizontal(column, row);
                AddToDestroyList(topBottomRange);
                RememberIndexesOfDestroyedCandiesVertical(column, row);
            }
        }
        foreach (Candy candy in candiesToDestroy)
        {
            Destroy(candy.gameObject);
        }
        generator.SetCandiesToNullByIndexes(candiesToDestroyIndexes);
        scoreCounter.IncreaseScore(candiesToDestroyIndexes, this);
        OnDestroyMatches?.Invoke();
        candyMover.SetOwner(owner);
        candyMover.SetCandiesToDestroyIndexes(candiesToDestroyIndexes);
        owner.ChangeState(candyMover);
        _destroyMatchesCoroutineIsRunning = false;
    }
    public List<Vector2Int> GetCandiesToDestroyIndexes()
    {
        return candiesToDestroyIndexes;
    }
    private void ClearCandiesToDestroy()
    {
        candiesToDestroy.Clear();
        candiesToDestroyIndexes.Clear();
    }
    private void RememberIndexesOfDestroyedCandiesHorizontal(int centerColumn, int centerRow)
    {
        for (int column = centerColumn - 1; column <= centerColumn + 1; column++)
        {
            if (CandiesToDestroyIndexesDoNotContainIndex(column, centerRow))
            {
                Vector2Int indexToRemember = new Vector2Int(column, centerRow);
                
                candiesToDestroyIndexes.Add(indexToRemember);
            }
        }
    }
    private void RememberIndexesOfDestroyedCandiesVertical(int centerColumn, int centerRow)
    {
        for (int row = centerRow - 1; row <= centerRow + 1; row++)
        {
            if (CandiesToDestroyIndexesDoNotContainIndex(centerColumn, row))
            {
                candiesToDestroyIndexes.Add(new Vector2Int(centerColumn, row));
            }
        }
    }

    private bool CandiesToDestroyIndexesDoNotContainIndex(int column, int row)
    {
        for (int index = 0; index < candiesToDestroyIndexes.Count; index++)
        {
            if (candiesToDestroyIndexes[index].x == column && candiesToDestroyIndexes[index].y == row)
            {
                return false;
            }
        }
        return true;
    }

    public void Enter()
    {
        StartCoroutine(DestroyMatches());
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
