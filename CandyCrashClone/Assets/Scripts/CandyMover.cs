using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CandyMover : MonoBehaviour
{
    private GridGenerator generator;
    private BoardFiller filler;
    private void Awake()
    {
        generator = GetComponent<GridGenerator>();
        filler = GetComponent<BoardFiller>();
    }
    private Dictionary<int, List<Vector2Int>> CreateColumnEmptyIndexesPairs(List<Vector2Int> destroyedCandyIndexes)
    {
        Dictionary<int, List<Vector2Int>> columnEmptyIndexesPairs =
            new Dictionary<int, List<Vector2Int>>();
        for (int index = 0; index < destroyedCandyIndexes.Count; index++)
        {
            int key = destroyedCandyIndexes[index].x;
            if (columnEmptyIndexesPairs.ContainsKey(key))
            {
                columnEmptyIndexesPairs[key].Add(destroyedCandyIndexes[index]);
            }
            else
            {
                columnEmptyIndexesPairs.Add(key, new List<Vector2Int> { destroyedCandyIndexes[index] });
            }

        }
        return columnEmptyIndexesPairs;
    }
    private List<List<int>> DevideEmptyIndexesByProximity(List<Vector2Int> emptyIndexesInColumn)
    {
        List<int> emptyYInColumn = GetOnlyY(emptyIndexesInColumn);
        int lowestEmptyYIndex = FindLowestY(emptyYInColumn);
        List<List<int>> emptyIndexesGroups = new List<List<int>>();
        emptyIndexesGroups.Add(new List<int>());
        int EmptyIndexGroupCount = 0;
        List<int> currentGroup = emptyIndexesGroups[EmptyIndexGroupCount];
        currentGroup.Add(lowestEmptyYIndex);
        emptyYInColumn.Remove(lowestEmptyYIndex);
        emptyYInColumn = SortedIncrease(emptyYInColumn);

        for (int j = 0; j < emptyYInColumn.Count; j++)
        {
            int lastInCurrentGroup = currentGroup[currentGroup.Count - 1];
            if (emptyYInColumn[j] == lastInCurrentGroup + 1)
            {
                currentGroup.Add(emptyYInColumn[j]);
            }
            else if (emptyYInColumn[j] > lastInCurrentGroup + 1)
            {
                EmptyIndexGroupCount++;
                emptyIndexesGroups.Add(new List<int>());
                currentGroup = emptyIndexesGroups[EmptyIndexGroupCount];
                currentGroup.Add(emptyYInColumn[j]);
            }
        }
        return emptyIndexesGroups;
    }
    private List<int> SortedIncrease(List<int> emptyYInColumn)
    {
        var sortedY = from y in emptyYInColumn
                      orderby y
                      select y;
        return sortedY.ToList();
    }
    private List<List<int>> DevideCandyToMoveByGaps(List<List<int>> indexesEmptyGroups)
    {
        List<List<int>> indexesToMoveGroups = new List<List<int>>();
        for (int i = 0; i < indexesEmptyGroups.Count; i++)
        {
            indexesToMoveGroups.Add(new List<int>());
            List<int> group = indexesEmptyGroups[i];
            int yAfterGroup = group[group.Count - 1] + 1;
            if (i < indexesEmptyGroups.Count - 1)
            {
                List<int> nextGroup = indexesEmptyGroups[i + 1];
                int previouseYNextGroup = nextGroup[0] - 1;
                for (int j = yAfterGroup; j <= previouseYNextGroup; j++)
                {
                    indexesToMoveGroups[i].Add(j);
                }
            }
            else if (i == indexesEmptyGroups.Count - 1)
            {
                int highestRow = generator.Height - 1;
                for (int j = yAfterGroup; j <= highestRow; j++)
                {
                    indexesToMoveGroups[i].Add(j);
                }
            }
        }
        return indexesToMoveGroups;
    }
    private void MoveGropsDown(int column, List<List<int>> emptyIndexesGroups,
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
        yield return new WaitForSeconds(1f);
       
        Dictionary<int, List<Vector2Int>> columnEmptyIndexesPairs =
            CreateColumnEmptyIndexesPairs(destroyedCandyIndexes);
       

        for (int i = 0; i < columnEmptyIndexesPairs.Keys.Count; i++)
        {
            int column = columnEmptyIndexesPairs.Keys.ToList()[i];
            List<Vector2Int> emptyIndexesInColumn = columnEmptyIndexesPairs[column];

            List<List<int>> emptyIndexesGroups = DevideEmptyIndexesByProximity(emptyIndexesInColumn);
            List<List<int>> indexesToMoveGroups = DevideCandyToMoveByGaps(emptyIndexesGroups);
            MoveGropsDown(column, emptyIndexesGroups, indexesToMoveGroups);
        }

        StartCoroutine(filler.FillEmptyIndexes());
    }

    private List<int> GetOnlyY(List<Vector2Int> indexes)
    {
        List<int> onlyY = new List<int>();
        for (int i = 0; i < indexes.Count; i++)
        {
            onlyY.Add(indexes[i].y);
        }
        return onlyY;
    }
    private int FindLowestY(List<int> emptyYInColumn)
    {
        int lowestYIndex = emptyYInColumn[0];
        for (int i = 0; i < emptyYInColumn.Count; i++)
        {
            if (emptyYInColumn[i] < lowestYIndex)
            {
                lowestYIndex = emptyYInColumn[i];
            }
        }
        return lowestYIndex;
    }
}
