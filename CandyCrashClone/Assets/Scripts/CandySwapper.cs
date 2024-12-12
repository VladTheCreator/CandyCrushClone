using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class CandySwapper : MonoBehaviour
{
    private GridGenerator generator;
    private MatchChecker checker;
    private MatchDestroyer destroyer;
    private SpaceOcupied[,] spaceOcupiedByCandies;
    Vector2 startMousePosition = Vector2.zero;
    Vector2Int? clickedCandyIndexes = null;
    private Vector2Int[] lastSwoped;
    private void Awake()
    {
        generator = GetComponent<GridGenerator>();
        checker = GetComponent<MatchChecker>();
        destroyer = GetComponent<MatchDestroyer>();
        lastSwoped = new Vector2Int[2];
        DefineSpaceOcupiedByCandies();
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePositionWorld = GetMousePositionWorld(Input.mousePosition);
            startMousePosition = mousePositionWorld;
            clickedCandyIndexes = GetClickedCandyIndexes(mousePositionWorld);
        }
        if (Input.GetMouseButtonUp(0))
        {
            //if player found match don't give him/her a hint
            checker.StopCoroutineHighlightPossibleMatches();

            if (clickedCandyIndexes != null)
            {
                Vector2 mousePositionWorld = GetMousePositionWorld(Input.mousePosition);
                SwopCandiesInDirectionOfSwap(mousePositionWorld);
                if (checker.FindMatches())
                {
                    checker.DestroyMatches();
                }
                else
                {
                    StartCoroutine(CancelSwop());
                    checker.StartCoroutineHighlightPossibleMatches();
                }
            }
            if (!destroyer.DestroyMatchesCoroutineIsRunning
                && !destroyer.MoveCandyCoroutineIsRunning
                && !destroyer.FillEmptyIndexesCoroutineIsRunning)
            {
                checker.StartCoroutineHighlightPossibleMatches();
            }
        }
    }
    private void SwopCandiesInDirectionOfSwap(Vector2 mousePositionWorld)
    {
        if (SwipeInDirection(Vector2.left, mousePositionWorld))
        {
            if (clickedCandyIndexes.Value.x > 0)
            {
                generator.SwopCandies(clickedCandyIndexes.Value, clickedCandyIndexes.Value - OneX());
                RegisterLastSwop(clickedCandyIndexes.Value, clickedCandyIndexes.Value - OneX());
            }
        }
        if (SwipeInDirection(Vector2.right, mousePositionWorld))
        {
            if (clickedCandyIndexes.Value.x < generator.Width - 1)
            {
                generator.SwopCandies(clickedCandyIndexes.Value, clickedCandyIndexes.Value + OneX());
                RegisterLastSwop(clickedCandyIndexes.Value, clickedCandyIndexes.Value + OneX());
            }
        }
        if (SwipeInDirection(Vector2.down, mousePositionWorld))
        {
            if (clickedCandyIndexes.Value.y > 0)
            {
                generator.SwopCandies(clickedCandyIndexes.Value, clickedCandyIndexes.Value - OneY());
                RegisterLastSwop(clickedCandyIndexes.Value, clickedCandyIndexes.Value - OneY());
            }
        }
        if (SwipeInDirection(Vector2.up, mousePositionWorld))
        {
            if (clickedCandyIndexes.Value.y < generator.Height - 1)
            {
                generator.SwopCandies(clickedCandyIndexes.Value, clickedCandyIndexes.Value + OneY());
                RegisterLastSwop(clickedCandyIndexes.Value, clickedCandyIndexes.Value + OneY());
            }
        }
    }
    private Vector2Int OneX()
    {
        return new Vector2Int(1, 0);
    }
    private Vector2Int OneY()
    {
        return new Vector2Int(0, 1);
    }
    private bool SwipeInDirection(Vector3 directionToCheck, Vector2 endMousePosition)
    {
        Vector2 directionOfSwipe = (endMousePosition - startMousePosition).normalized;
        var angleDelta = Math.Abs(Vector2.SignedAngle(directionToCheck, directionOfSwipe));
        float angleMargine = 45;
        if (angleDelta <= angleMargine)
            return true;
        else
            return false;
    }
    private void RegisterLastSwop(Vector2Int original, Vector2Int toSwopWith)
    {
        lastSwoped[0] = original;
        lastSwoped[1] = toSwopWith;
    }

    private Vector2Int? GetClickedCandyIndexes(Vector2 mousePositionWorld)
    {
        Vector2Int? indexes = null;
        for (int column = 0; column < generator.Width; column++)
        {
            for (int row = 0; row < generator.Height; row++)
            {
                SpaceOcupied space = spaceOcupiedByCandies[column, row];
                if (PositionInBoundsOf(mousePositionWorld, space))
                {
                    indexes = new Vector2Int(column, row);
                }
            }
        }
        return indexes;
    }
    private bool PositionInBoundsOf(Vector3 position, SpaceOcupied space)
    {
        bool fitLeft = position.x > space.xMin;
        bool fitRight = position.x < space.xMax;
        bool fitTop = position.y < space.yMax;
        bool fitBottom = position.y > space.yMin;
        return fitLeft && fitRight && fitTop && fitBottom;
    }
    private void DefineSpaceOcupiedByCandies()
    {
        spaceOcupiedByCandies = new SpaceOcupied[generator.Width, generator.Height];
        for (int column = 0; column < generator.Width; column++)
        {
            for (int row = 0; row < generator.Height; row++)
            {
                float xMin = column;
                float xMax = column + generator.GenerationStep;
                float yMin = row;
                float yMax = row + generator.GenerationStep;
                spaceOcupiedByCandies[column, row] = new SpaceOcupied(xMin, xMax, yMin, yMax);
            }
        }
    }
    private Vector2 GetMousePositionWorld(Vector3 mousePositionScreen)
    {
        Vector2 mousePositionWorld = Camera.main.ScreenToWorldPoint(mousePositionScreen);
        return mousePositionWorld;
    }
    private IEnumerator CancelSwop()
    {
        yield return new WaitForSeconds(0.2f);
        generator.SwopCandies(lastSwoped[0], lastSwoped[1]);
    }
}
public struct SpaceOcupied
{
    public SpaceOcupied(float xMin, float xMax, float yMin, float yMax)
    {
        this.xMin = xMin;
        this.xMax = xMax;
        this.yMin = yMin;
        this.yMax = yMax;
    }
    public float xMin;
    public float xMax;
    public float yMin;
    public float yMax;
}
