using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class PossibleMatchChecker : MonoBehaviour
{
    private GridGenerator generator;
    private float delay = 5f;
    private List<Candy> pulsatingCandy = new List<Candy>();
    private void Awake()
    {
        generator = GetComponent<GridGenerator>();
    }
    //Find matches of two  
    // try to swap candies to make match 3 or more on the right and left sides of match of 2 
    // remember matches of two to avoid them if they can not make match of 3 or more
    //
    public IEnumerator HighlightPossibleMatchOfThreeOrMore()
    {
        yield return new WaitForSeconds(delay);
        List<Vector2Int?[]> matchesOfTwoInRows = FindAllMatchesOfTwoInRows();
        foreach (Vector2Int?[] match in matchesOfTwoInRows)
        {
            Vector2Int? third = FindThirdHorizontalMatchOf(match);
            if (third != null)
            {
                Candy candy = generator.GetCandyUnderIndex(third.Value);
                candy.StartPulsating();
                pulsatingCandy.Add(candy);
                foreach (Vector2Int? index in match)
                {
                    candy = generator.GetCandyUnderIndex(index.Value);
                    candy.StartPulsating();
                    pulsatingCandy.Add(candy);
                }
                break;
            }
        }
        if (matchesOfTwoInRows == null)
        {
            List<Vector2Int?[]> matchesOfTwoInColumns = FindAllMatchesOfTwoInColumns();
            foreach (Vector2Int?[] match in matchesOfTwoInColumns)
            {
                Vector2Int? third = FindThirdVerticalMatchOf(match);
                if (third != null)
                {
                    Candy candy = generator.GetCandyUnderIndex(third.Value);
                    candy.StartPulsating();
                    pulsatingCandy.Add(candy);
                    foreach (Vector2Int? index in match)
                    {
                        candy = generator.GetCandyUnderIndex(index.Value);
                        candy.StartPulsating();
                        pulsatingCandy.Add(candy);
                    }
                    break;
                }
            }
        }
    }
    public void StopAllCandyPulsating()
    {
        foreach (Candy candy in pulsatingCandy)
        {
            if (candy != null)
            {
                candy.StopPulsating();
            }
        }
        pulsatingCandy.Clear();
    }
    private Vector2Int? FindThirdHorizontalMatchOf(Vector2Int?[] matchOfTwo)
    {
        Vector2Int? third = FindMatchOfThreeOrMoreOnTheLeft(matchOfTwo);
        if (third == null)
        {
            third = FindMatchOfThreeOrMoreOnTheRight(matchOfTwo);
            if (third != null) return third;
            else return null;
        }
        else return third;
    }
    private Vector2Int? FindThirdVerticalMatchOf(Vector2Int?[] matchOfTwo)
    {
        Vector2Int? third = FindMatchOfThreeOrMoreBeyond(matchOfTwo);
        if (third == null)
        {
            third = FindMatchOfThreeOrMoreAbove(matchOfTwo);
            if (third != null) return third;
            else return null;
        }
        else return third;
    }

    private Vector2Int? FindMatchOfThreeOrMoreAbove(Vector2Int?[] matchOfTwo)
    {
        Vector2Int withBiggerY = matchOfTwo.OrderByDescending(c => c.Value.y).First().Value;
        if (withBiggerY.y < generator.Height - 1)
        {
            Vector2Int upper = GetUpperFromVerticalMatchOfTwo(matchOfTwo);
            //left
            if (upper.x > 0)
            {
                Vector2Int left = new Vector2Int(upper.x - 1, upper.y);
                if (CandyUnderIndexesMatch(matchOfTwo[0].Value, left))
                {
                    return left;
                }
            }
            //upper
            if (upper.y < generator.Height - 1)
            {
                Vector2Int upperFromUpper = new Vector2Int(upper.x, upper.y + 1);
                if (CandyUnderIndexesMatch(matchOfTwo[0].Value, upperFromUpper))
                {
                    return upperFromUpper;
                }
            }
            //right
            if (upper.x < generator.Width - 1)
            {
                Vector2Int right = new Vector2Int(upper.x + 1, upper.y);
                if (CandyUnderIndexesMatch(matchOfTwo[0].Value, right))
                {
                    return right;
                }
            }
        }
        return null;
    }
    private Vector2Int? FindMatchOfThreeOrMoreBeyond(Vector2Int?[] matchOfTwo)
    {
        Vector2Int withLesserY = matchOfTwo.OrderBy(c => c.Value.y).First().Value;
        if (withLesserY.y > 0)
        {
            Vector2Int lower = GetLowerFromVerticalMatchOfTwo(matchOfTwo);
            //left
            if (lower.x > 0)
            {
                Vector2Int left = new Vector2Int(lower.x - 1, lower.y);
                if (CandyUnderIndexesMatch(matchOfTwo[0].Value, left))
                {
                    return left;
                }
            }
            //lower
            if (lower.y > 0)
            {
                Vector2Int lowerFromLower = new Vector2Int(lower.x, lower.y - 1);
                if (CandyUnderIndexesMatch(matchOfTwo[0].Value, lowerFromLower))
                {
                    return lowerFromLower;
                }
            }
            //right
            if (lower.x < generator.Width - 1)
            {
                Vector2Int right = new Vector2Int(lower.x + 1, lower.y);
                if (CandyUnderIndexesMatch(matchOfTwo[0].Value, right))
                {
                    return right;
                }
            }
        }
        return null;
    }
    private Vector2Int? FindMatchOfThreeOrMoreOnTheRight(Vector2Int?[] matchOfTwo)
    {
        Vector2Int withBiggerX = matchOfTwo.OrderByDescending(c => c.Value.x).First().Value;
        if (withBiggerX.x < generator.Width - 1)
        {
            Vector2Int right = GetRightFromHorizontalMatchOfTwo(matchOfTwo);
            //upper on the right
            if (right.y < generator.Height - 1)
            {
                Vector2Int upper = new Vector2Int(right.x, right.y + 1);
                if (CandyUnderIndexesMatch(matchOfTwo[0].Value, upper))
                {
                    return upper;
                }
            }
            // right on the right
            else if (right.x < generator.Width - 1)
            {
                Vector2Int rightFromRight = new Vector2Int(right.x + 1, right.y);
                if (CandyUnderIndexesMatch(matchOfTwo[0].Value, rightFromRight))
                {
                    return rightFromRight;
                }
            }
            // lower on the right
            else if (right.y > 0)
            {
                Vector2Int lower = new Vector2Int(right.x, right.y - 1);
                if (CandyUnderIndexesMatch(matchOfTwo[0].Value, lower))
                {
                    return lower;
                }
            }
        }
        return null;
    }
    private Vector2Int? FindMatchOfThreeOrMoreOnTheLeft(Vector2Int?[] matchOfTwo)
    {
        Vector2Int withLesserX = matchOfTwo.OrderBy(c => c.Value.x).First().Value;
        if (withLesserX.x > 0)
        {
            Vector2Int left = GetLeftFromHorizontalMatchOfTwo(matchOfTwo);
            //check upper on the left from horizontal match of two
            if (left.y < generator.Height - 1)
            {
                Vector2Int upper = new Vector2Int(left.x, left.y + 1);
                if (CandyUnderIndexesMatch(matchOfTwo[0].Value, upper))
                {
                    return upper;
                }
            }
            //check left on the left from horizontal match of two
            else if (left.x > 0)
            {
                Vector2Int leftFromLeft = new Vector2Int(left.x - 1, left.y);
                if (CandyUnderIndexesMatch(matchOfTwo[0].Value, leftFromLeft))
                {
                    return leftFromLeft;
                }
            }
            //check lower on the left from horizontal match of two
            else if (left.y > 0)
            {
                Vector2Int lower = new Vector2Int(left.x, left.y - 1);
                if (CandyUnderIndexesMatch(matchOfTwo[0].Value, lower))
                {
                    return lower;
                }
            }
        }
        return null;
    }
    private Vector2Int GetLeftFromHorizontalMatchOfTwo(Vector2Int?[] matchOfTwo)
    {
        Vector2Int withLesserX = matchOfTwo.OrderBy(c => c.Value.x).First().Value;
        Vector2Int leftFromMatchOfTwo = new Vector2Int(withLesserX.x - 1, withLesserX.y);
        return leftFromMatchOfTwo;
    }
    private Vector2Int GetLowerFromVerticalMatchOfTwo(Vector2Int?[] matchOfTwo)
    {
        Vector2Int withLesserY = matchOfTwo.OrderBy(c => c.Value.y).First().Value;
        Vector2Int lowerFromMatchOfTwo = new Vector2Int(withLesserY.x, withLesserY.y - 1);
        return lowerFromMatchOfTwo;
    }
    private Vector2Int GetRightFromHorizontalMatchOfTwo(Vector2Int?[] matchOfTwo)
    {
        Vector2Int withBiggerX = matchOfTwo.OrderByDescending(c => c.Value.x).First().Value;
        Vector2Int rightFromMatchOfTwo = new Vector2Int(withBiggerX.x + 1, withBiggerX.y);
        return rightFromMatchOfTwo;
    }
    private Vector2Int GetUpperFromVerticalMatchOfTwo(Vector2Int?[] matchOfTwo)
    {
        Vector2Int withBiggerY = matchOfTwo.OrderByDescending(c => c.Value.y).First().Value;
        Vector2Int upperFromMatchOfTwo = new Vector2Int(withBiggerY.x, withBiggerY.y + 1);
        return upperFromMatchOfTwo;
    }
    public List<Vector2Int?[]> FindAllMatchesOfTwoInRows()
    {
        List<Vector2Int?[]> allMatchesInRows = new List<Vector2Int?[]>();
        for (int i = 0; i < generator.Width; i++)
        {
            for (int j = 0; j < generator.Height; j++)
            {
                Vector2Int current = new Vector2Int(i, j);
                if (current.x > 0)
                {
                    Vector2Int previouse = new Vector2Int(i - 1, j);
                    if (CandyUnderIndexesMatch(current, previouse))
                    {
                        allMatchesInRows.Add(new Vector2Int?[2]);
                        int lastIndex = allMatchesInRows.Count - 1;
                        allMatchesInRows[lastIndex][0] = previouse;
                        allMatchesInRows[lastIndex][1] = current;
                    }
                }
            }
        }
        return allMatchesInRows;
    }
    private List<Vector2Int?[]> FindAllMatchesOfTwoInColumns()
    {
        List<Vector2Int?[]> allMatchesInColumns = new List<Vector2Int?[]>();
        for (int i = 0; i < generator.Width; i++)
        {
            for (int j = 0; j < generator.Height; j++)
            {
                Vector2Int current = new Vector2Int(i, j);
                if (current.y > 0)
                {
                    Vector2Int previouse = new Vector2Int(i, j - 1);
                    if (CandyUnderIndexesMatch(current, previouse))
                    {
                        allMatchesInColumns.Add(new Vector2Int?[2]);
                        int lastIndex = allMatchesInColumns.Count - 1;
                        allMatchesInColumns[lastIndex][0] = previouse;
                        allMatchesInColumns[lastIndex][1] = current;
                    }
                }
            }
        }
        return allMatchesInColumns;
    }
    private bool CandyUnderIndexesMatch(Vector2Int firstIndex, Vector2Int secondIndex)
    {
        CandyType firstType = generator.GetCandyUnderIndex(firstIndex).GetCandyType;
        CandyType secondType = generator.GetCandyUnderIndex(secondIndex).GetCandyType;
        return firstType == secondType;
    }
}
