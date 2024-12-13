using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CandyMover : MonoBehaviour
{
    private GridGenerator generator;
    private BoardFiller filler;
    private bool _moveCandiesCoroutineIsRunning;
    public bool MoveCandiesCoroutineIsRunning => _moveCandiesCoroutineIsRunning;
    public bool FillEmptyIndexesCoroutineIsRunning => filler.FillEmptyIndexesCoroutineIsRunning;
    private void Awake()
    {
        generator = GetComponent<GridGenerator>();
        filler = GetComponent<BoardFiller>();
    }
    /// <summary>
    /// Create a dictionary where key is a number of a column and the value is the empty spaces in that column
    /// </summary>
    /// <param name="emptySpaces"> Empty spaces on the board </param>
    /// <returns></returns>
    private Dictionary<int, List<Vector2Int>> ColumnToEmptySpacesDictionary(List<Vector2Int> emptySpaces)
    {
        Dictionary<int, List<Vector2Int>> columnToEmptySpacesDictionary = new Dictionary<int, List<Vector2Int>>();
        foreach (Vector2Int emptySpace in emptySpaces)
        {
            int key = emptySpace.x;
            if (columnToEmptySpacesDictionary.ContainsKey(key))
            {
                columnToEmptySpacesDictionary[key].Add(emptySpace);
            }
            else
            {
                columnToEmptySpacesDictionary.Add(key, new List<Vector2Int> { emptySpace });
            }

        }
        return columnToEmptySpacesDictionary;
    }

    private List<List<int>> GroupConsecutiveEmptySpaces(List<Vector2Int> emptySpacesInColumn)
    {
        List<int> yValues = emptySpacesInColumn.Select(space => space.y).ToList();
        int lowest = yValues.Min();
        List<List<int>> emptySpacesGroups = new List<List<int>>{ new List<int>() };
        int groupCount = 0;
        List<int> currentGroup = emptySpacesGroups[groupCount];
        currentGroup.Add(lowest);
        yValues = yValues.OrderBy(y => y).ToList();

        foreach (int y in yValues)
        {
            if (y == currentGroup.Last() + 1)
            {
                currentGroup.Add(y);
            }
            else if (y > currentGroup.Last() + 1)
            {
                groupCount++;
                emptySpacesGroups.Add(new List<int>());
                currentGroup = emptySpacesGroups[groupCount];
                currentGroup.Add(y);
            }
        }
        return emptySpacesGroups;
    }
    private List<List<int>> GroupCandyAroundEmptySpaces(List<List<int>> groupsOfConsecutiveEmptySpaces)
    {
        List<List<int>> candyGroups = new List<List<int>>();
        for (int i = 0; i < groupsOfConsecutiveEmptySpaces.Count; i++)
        {
            candyGroups.Add(new List<int>());
            List<int> group = groupsOfConsecutiveEmptySpaces[i];
            int firstCandyAboveThisGroup = group.Last() + 1;
            if (i < groupsOfConsecutiveEmptySpaces.Count - 1)
            {
                List<int> nextGroup = groupsOfConsecutiveEmptySpaces[i + 1];
                int firstCandyBelowNextGroup = nextGroup[0] - 1;
                for (int j = firstCandyAboveThisGroup; j <= firstCandyBelowNextGroup; j++)
                {
                    candyGroups[i].Add(j);
                }
            }
            else if (i == groupsOfConsecutiveEmptySpaces.Count - 1)
            {
                int highestRow = generator.Height - 1;
                for (int j = firstCandyAboveThisGroup; j <= highestRow; j++)
                {
                    candyGroups[i].Add(j);
                }
            }
        }
        return candyGroups;
    }
    private void MoveCandyGropsDown(int column, List<List<int>> emptyIndexesGroups,
        List<List<int>> indexesToMoveGroups)
    {
        for (int i = 0; i < indexesToMoveGroups.Count; i++)
        {
            for (int j = 0; j < indexesToMoveGroups[i].Count; j++)
            {
                List<int> candyToMoveGroup = indexesToMoveGroups[i];
                Vector2Int originalIndex = new Vector2Int(column, candyToMoveGroup[j]);
                int spaces = CountSpacesUntilIndex(i, emptyIndexesGroups);
                Vector2Int indexToMoveTo = new Vector2Int(column, candyToMoveGroup[j] - spaces);

                Vector2 positionToMove = generator.IndexToPosition(indexToMoveTo);
                Candy candyToMove = generator.GetCandyUnderIndex(originalIndex);
                candyToMove.transform.position = positionToMove;
                generator.ResetCandyToIndex(originalIndex, indexToMoveTo);
            }
        }
    }
    /// <summary>
    /// Includes current index
    /// </summary>
    private int CountSpacesUntilIndex(int index, List<List<int>> emptyIndexesGroups)
    {
        int amountOfSpacesInclusive = 0;
        for (int i = 0; i <= index; i++)
        {
            amountOfSpacesInclusive += emptyIndexesGroups[i].Count;
        }
        return amountOfSpacesInclusive;
    }
    public IEnumerator MoveCandiesDown(List<Vector2Int> destroyedCandyIndexes)
    {
        _moveCandiesCoroutineIsRunning = true;
        yield return new WaitForSeconds(1f);
       
        Dictionary<int, List<Vector2Int>> columnToEmptySpacesDictionary = ColumnToEmptySpacesDictionary(destroyedCandyIndexes);

        foreach (int column in columnToEmptySpacesDictionary.Keys)
        {
            List<Vector2Int> emptySpacesInColumn = columnToEmptySpacesDictionary[column];
            List<List<int>> groupsOfConsecutiveEmptySpaces = GroupConsecutiveEmptySpaces(emptySpacesInColumn);
            List<List<int>> groupsOfCandiesAroundEmptySpaces = GroupCandyAroundEmptySpaces(groupsOfConsecutiveEmptySpaces);
            MoveCandyGropsDown(column, groupsOfConsecutiveEmptySpaces, groupsOfCandiesAroundEmptySpaces);
        }

        StartCoroutine(filler.FillEmptyIndexes());
        _moveCandiesCoroutineIsRunning = false;
    }
}
