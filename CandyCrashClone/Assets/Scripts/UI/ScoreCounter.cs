using System;
using System.Collections.Generic;
using UnityEngine;

public class ScoreCounter : MonoBehaviour
{
    private int score = 0;
    private GridGenerator generator;
    [SerializeField] private ScoreDisplay scoreDisplay; 
    private void Awake()
    {
        generator = GetComponent<GridGenerator>();
    }

    public void IncreaseScore(List<Vector2Int> destroyedCandyIndexes, MatchDestroyer destroyer)
    {
        Debug.Log(destroyer.CandiesToDestroyIndexes.Count);
        FindHorizontalMatches(destroyedCandyIndexes, destroyer);
        
        FindVerticalMatches(destroyedCandyIndexes);
        scoreDisplay.DisplayScore(score);
    }
    private void FindVerticalMatches(List<Vector2Int> destroyedCandyIndexes)
    {
        List<Vector2Int> destroyedCandyIndexesClone = CloneList(destroyedCandyIndexes);
        for (int column = 0; column < generator.Width; column++)
        {
            List<List<int>> verticalMatches = new List<List<int>>();
            int verticalMatchesCount = 0;
            if (ColumnHasThreeOrMoreDestroyedIndexes(column, destroyedCandyIndexesClone))
            {
                verticalMatches.Add(new List<int>());
                Vector2Int lowest = GetLowestDestroyedInColumn(column, destroyedCandyIndexesClone);
                destroyedCandyIndexesClone.Remove(lowest);
                verticalMatches[verticalMatchesCount].Add(lowest.y);
                for (int row = 0; row < generator.Width; row++)
                {
                    if (CandyIndexesContain(destroyedCandyIndexesClone, column, row))
                    {
                        int lastIndex = verticalMatches[verticalMatchesCount].Count - 1;
                        int last = verticalMatches[verticalMatchesCount][lastIndex];
                        if (OnTheNextRowAbove(last, row))
                        {
                            verticalMatches[verticalMatchesCount].Add(row);
                        }
                        else
                        {
                            verticalMatches.Add(new List<int>());
                            verticalMatchesCount++;
                            verticalMatches[verticalMatchesCount].Add(row);
                        }
                    }
                }
                DesideHowMuchPointsToAdd(verticalMatches);
            }
        }
    }
    private void DesideHowMuchPointsToAdd(List<List<int>> matches)
    {
        for (int i = 0; i < matches.Count; i++)
        {
            if (matches[i].Count == 3)
            {
                score += 10;
            }
            else if (matches[i].Count == 4)
            {
                score += 20;
            }
            else if (matches[i].Count == 5)
            {
                score += 40;
            }
        }
    }
    private List<Vector2Int> CloneList(List<Vector2Int> destroyedCandyIndexes)
    {
        List<Vector2Int> newList = new List<Vector2Int>();
        for (int i = 0; i < destroyedCandyIndexes.Count; i++)
        {
            newList.Add(destroyedCandyIndexes[i]);
        }
        return newList;
    }
    private void FindHorizontalMatches(List<Vector2Int> destroyedCandyIndexes, MatchDestroyer destroyer)
    {
        List<Vector2Int> destroyedCandyIndexesClone = CloneList(destroyedCandyIndexes); // clone to safely remove elements
        for (int row = 0; row < generator.Height; row++)
        {
            List<List<int>> horizontalMatches = new List<List<int>>();
            int horizontalMatchesCount = 0;
            if (RowHasThreeOrMoreDestroyedIndexes(row, destroyedCandyIndexesClone))
            {
                horizontalMatches.Add(new List<int>());
                Vector2Int leftest = GetLeftestDestroyedInRow(row, destroyedCandyIndexesClone);
                destroyedCandyIndexesClone.Remove(leftest);
                horizontalMatches[horizontalMatchesCount].Add(leftest.x);
               
                for (int column = 0; column < generator.Width; column++)
                {
                    if (CandyIndexesContain(destroyedCandyIndexesClone, column, row))
                    {
                        int lastIndex = horizontalMatches[horizontalMatchesCount].Count - 1;
                        int last = horizontalMatches[horizontalMatchesCount][lastIndex];
                        if (OnTheNextColumnRight(last, column))
                        {
                            horizontalMatches[horizontalMatchesCount].Add(column);
                        }
                        else
                        {
                            horizontalMatches.Add(new List<int>());
                            horizontalMatchesCount++;
                            horizontalMatches[horizontalMatchesCount].Add(column);
                        }
                    }
                }
                DesideHowMuchPointsToAdd(horizontalMatches);
            }
        }
    }

    private Vector2Int GetLowestDestroyedInColumn(int column, List<Vector2Int> destroyedCandyIndexes)
    {
        int lowest = generator.Height - 1;
        foreach (Vector2Int index in destroyedCandyIndexes)
        {
            if (index.x == column)
            {
                if (index.y < lowest)
                {
                    lowest = index.y;
                }
            }
        }
        return new Vector2Int(column, lowest);
    }
    private Vector2Int GetLeftestDestroyedInRow(int row, List<Vector2Int> destroyedCandyIndexes)
    {
        int leftest = generator.Width - 1;
        foreach (Vector2Int index in destroyedCandyIndexes)
        {
            if (index.y == row)
            {
                if (index.x < leftest)
                {
                    leftest = index.x;
                }
            }
        }
        return new Vector2Int(leftest, row);
    }

    private bool OnTheNextRowAbove(int previouseRow, int currentRow)
    {
        if (currentRow - 1 == previouseRow) return true;
        else return false;
    }
    private bool OnTheNextColumnRight(int previouseColumn, int currentColumn)
    {
        if (currentColumn - 1 == previouseColumn) return true;
        else return false;
    }
    private bool ColumnHasThreeOrMoreDestroyedIndexes(int column, List<Vector2Int> destroyedCandyIndexes)
    {
        int destroyedIndexesInColumnCount = 0;
        foreach (Vector2Int index in destroyedCandyIndexes)
        {
            if (index.x == column)
            {
                destroyedIndexesInColumnCount++;
            }
        }
        return destroyedIndexesInColumnCount > 2;
    }
    private bool RowHasThreeOrMoreDestroyedIndexes(int row, List<Vector2Int> destroyedCandyIndexes)
    {
        int destroyedIndexesInRowCount = 0;
        foreach (Vector2Int index in destroyedCandyIndexes)
        {
            if (index.y == row)
            {
                destroyedIndexesInRowCount++;
            }
        }
        return destroyedIndexesInRowCount > 2;
    }
    private bool CandyIndexesContain(List<Vector2Int> candyIndexes, int coulumn, int row)
    {
        foreach (Vector2Int index in candyIndexes)
        {
            if (index.x == coulumn && index.y == row)
            {
                return true;
            }
        }
        return false;
    }
}
