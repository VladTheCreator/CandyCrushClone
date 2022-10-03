using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    [SerializeField] private int width;
    public int Width => width;
    [SerializeField] private int height;
    public int Height => height;

    [SerializeField] private Candy[] candyPrefabs;

    private Candy[,] candyInstances;

    private int generationStep = 1;
    public int GenerationStep => generationStep;
    private Vector2 startPosition;
    public Vector2 StartPosition => startPosition;
   

    public void InitializeGrid()
    {
        candyInstances = new Candy[width, height];
        startPosition = transform.position;
        for (int column = 0; column < width; column += generationStep)
        {
            for (int row = 0; row < height; row += generationStep)
            {
                Vector3 position = new Vector2(startPosition.x + column, startPosition.y + row);
                Candy random = GetRandomCandyPrefab();
                Candy candyInstance = Instantiate(random, position, Quaternion.identity);
                candyInstances[column, row] = candyInstance;
            }
        }
    }
   
    public Candy GetRandomCandyPrefab()
    {
        int randomIndex = Random.Range(0, candyPrefabs.Length);
        return candyPrefabs[randomIndex];
    }
   
    public List<Vector2Int> FindEmpty()
    {
        List<Vector2Int> empty = new List<Vector2Int>();
        for (int column = 0; column < width; column++)
        {
            for (int row = 0; row < height; row++)
            {
                if (candyInstances[column, row] == null)
                {
                    empty.Add(new Vector2Int(column, row));
                }
            }
        }
        return empty;
    }
    public List<Candy> CurrentLeftAndRightCandy(int column, int row)
    {
        List<Candy> checkRangeLR = new List<Candy>();
        if (NotOnTheLeftOrRightEdge(column, row))
        {
            int leftBoundRL = column - 1;
            int rightBoundRL = column + 1;
            for (int columnToCheck = leftBoundRL; columnToCheck <= rightBoundRL; columnToCheck++)
            {
                checkRangeLR.Add(candyInstances[columnToCheck, row]);
            }
        }
        return checkRangeLR;
    }
    public List<Candy> CurrentTopAndBottomCandy(int column, int row)
    {
        List<Candy> checkRangeTB = new List<Candy>();
        if (NotOnTheTopOrBottomEdge(column, row))
        {
            int lowerBoundTB = row - 1;
            int upperBoundTB = row + 1;
            for (int rowToCheck = lowerBoundTB; rowToCheck <= upperBoundTB; rowToCheck++)
            {
                checkRangeTB.Add(candyInstances[column, rowToCheck]);
            }
        }
        return checkRangeTB;
    }
    public void ReplaceCandy(int column, int row, List<CandyType> typesToAvoid)
    {
        Candy candyPrefab = GetAnyCandyExcept(typesToAvoid);
        Destroy(candyInstances[column, row].gameObject);
        Vector2 position = candyInstances[column, row].transform.position;
        candyInstances[column, row] = Instantiate(candyPrefab, position,
            Quaternion.identity);
    }
    private bool NotOnTheLeftOrRightEdge(int column, int row)
    {
        bool notOnTheRightEdge = column < width - 1;
        bool notOnTheLeftEdge = column > 0;
        return notOnTheLeftEdge && notOnTheRightEdge;
    }
    private bool NotOnTheTopOrBottomEdge(int column, int row)
    {
        bool notOnTheTopEdge = row < height - 1;
        bool notOnTheBottomEdge = row > 0;
        return notOnTheTopEdge && notOnTheBottomEdge;
    }

    private Candy GetAnyCandyExcept(List<CandyType> typeToAvoid)
    {
        List<Candy> potentialCandies = candyPrefabs.ToList();
        List<Candy> candiesToRemove = new List<Candy>();
        for (int i = 0; i < potentialCandies.Count; i++)
        {
            for (int j = 0; j < typeToAvoid.Count; j++)
            {
                if (potentialCandies[i].GetCandyType == typeToAvoid[j])
                {
                    candiesToRemove.Add(potentialCandies[i]);
                }
            }
        }
        foreach (Candy candy in candiesToRemove)
        {
            potentialCandies.Remove(candy);
        }
        int random = Random.Range(0, potentialCandies.Count);
        Candy candyPrefab = potentialCandies[random];
        return candyPrefab;
    }

    public void SwopCandies(Vector2Int firstCandyIndexes, Vector2Int secondCandyIndexes)
    {
        Candy first = candyInstances[firstCandyIndexes.x, firstCandyIndexes.y];
        Candy second = candyInstances[secondCandyIndexes.x, secondCandyIndexes.y];

        Vector2 temporaryPositionContainer = first.transform.position;
        first.transform.position = second.transform.position;
        second.transform.position = temporaryPositionContainer;

        candyInstances[firstCandyIndexes.x, firstCandyIndexes.y] = second;
        candyInstances[secondCandyIndexes.x, secondCandyIndexes.y] = first;

    }
    public void SetCandiesToNullByIndexes(List<Vector2Int> indexes)
    {
        for (int column = 0; column < width; column++)
        {
            for (int row = 0; row < height; row++)
            {
                SetIndexMatchToNullFor(column, row, indexes);
            }
        }
    }
    private void SetIndexMatchToNullFor(int column, int row, List<Vector2Int> indexesToCheck)
    {
        for (int i = 0; i < indexesToCheck.Count; i++)
        {
            if (column == indexesToCheck[i].x && row == indexesToCheck[i].y)
            {
                candyInstances[column, row] = null;
            }
        }
    }
    public void ResetCandyToIndex(Vector2Int originalIndex, Vector2Int newIndex)
    {
        candyInstances[newIndex.x, newIndex.y] = candyInstances[originalIndex.x, originalIndex.y];
        candyInstances[originalIndex.x, originalIndex.y] = null;
    }
    public void SetCandyByIndex(Vector2Int index, Candy value)
    {
        candyInstances[index.x, index.y] = value;
    }
    public Candy GetCandyUnderIndex(Vector2Int index)
    {
        return candyInstances[index.x, index.y];
    }
    public Vector2 IndexToPosition(Vector2Int index)
    {
        return new Vector2(startPosition.x + index.x, startPosition.y + index.y);
    }
}
