using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchDestroyer : MonoBehaviour
{
    private GridGenerator generator;
    private ScoreCounter scoreCounter;
    private CandyMover candyMover;
    private List<Candy> candiesToDestroy;
    private List<Vector2Int> candiesToDestroyIndexes;
    private bool _destroyMatchesCoroutineIsRunning;
    public bool DestroyMatchesCoroutineIsRunning => _destroyMatchesCoroutineIsRunning;
    public bool MoveCandyCoroutineIsRunning => candyMover.MoveCandiesCoroutineIsRunning;
    public bool FillEmptyIndexesCoroutineIsRunning => candyMover.FillEmptyIndexesCoroutineIsRunning;
    public List<Vector2Int> CandiesToDestroyIndexes => candiesToDestroyIndexes;
    private void Awake()
    {
        generator = GetComponent<GridGenerator>();
        scoreCounter = GetComponent<ScoreCounter>();
        candyMover = GetComponent<CandyMover>();
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

    internal IEnumerator DestroyMatches(Dictionary<Vector2Int, MatchType> centerInMatchAndMatchType,
        System.Action unregisterAllMatchesInMatchChecker)
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
       // Debug.Log(candiesToDestroyIndexes.Count);
        scoreCounter.IncreaseScore(candiesToDestroyIndexes, this);
        //Debug.Log(candiesToDestroyIndexes.Count);
        unregisterAllMatchesInMatchChecker.Invoke();
        StartCoroutine(candyMover.MoveCandiesDown(candiesToDestroyIndexes));
        _destroyMatchesCoroutineIsRunning = false;
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
}
