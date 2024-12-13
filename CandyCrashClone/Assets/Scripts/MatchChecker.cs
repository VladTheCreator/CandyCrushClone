using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MatchChecker : MonoBehaviour, IState
{
    private GridGenerator generator;
    private MatchDestroyer destroyer;
    private List<Vector2Int> centersInMatches;
    private Dictionary<Vector2Int, List<CandyType>> centerInMatchAndBadTypes;
    private Dictionary<Vector2Int, MatchType> centerInMatchAndMatchType;
    private PossibleMatchChecker possibleMatchChecker;
    private Coroutine highlightPossibleMatches;
    private StateMachine owner;
    public MatchType GetMatchType(Vector2Int centerInMatch)
    {
        return centerInMatchAndMatchType[centerInMatch];
    }
    private void Awake()
    {
        generator = GetComponent<GridGenerator>();
        destroyer = GetComponent<MatchDestroyer>();
        possibleMatchChecker = GetComponent<PossibleMatchChecker>();
        centersInMatches = new List<Vector2Int>();
        centerInMatchAndBadTypes = new Dictionary<Vector2Int, List<CandyType>>();
        centerInMatchAndMatchType = new Dictionary<Vector2Int, MatchType>();
    }
    private void Start()
    {
        generator.InitializeGrid();
        while (FindMatches())
        {
            FindMatches();
            ReplaceMatches();
        }
    }
    public void StartCoroutineHighlightPossibleMatches()
    {
        if (highlightPossibleMatches == null)
        {
            Debug.Log("Highlight possible matches");
            highlightPossibleMatches = StartCoroutine(possibleMatchChecker.HighlightPossibleMatchOfThreeOrMore());
        }
    }
    public void StopCoroutineHighlightPossibleMatches()
    {
        StopCoroutine(highlightPossibleMatches);
        highlightPossibleMatches = null;
    }
    public bool FindMatches()
    {
        for (int column = 0; column < generator.Width; column++)
        {
            for (int row = 0; row < generator.Height; row++)
            {
                CheckNeighbours(column, row);
            }
        }

        if (centersInMatches.Count == 0)
            return false;
        else
            return true;
    }
    public void HighlightMatches()
    {
        foreach (Vector2Int center in centersInMatches)
        {
            List<Candy> leftRight = generator.CurrentLeftAndRightCandy(center.x, center.y);
            List<Candy> bottomTop = generator.CurrentTopAndBottomCandy(center.x, center.y);

            if (centerInMatchAndMatchType[center] == MatchType.horizontal)
            {
                for (int i = 0; i < leftRight.Count; i++)
                {
                    HighlightCandy(leftRight[i]);
                }
            }
            else if (centerInMatchAndMatchType[center] == MatchType.vertical)
            {
                for (int i = 0; i < bottomTop.Count; i++)
                {
                    HighlightCandy(bottomTop[i]);
                }
            }
            else
            {
                for (int i = 0; i < leftRight.Count; i++)
                {
                    HighlightCandy(leftRight[i]);
                }
                for (int i = 0; i < bottomTop.Count; i++)
                {
                    HighlightCandy(bottomTop[i]);
                }
            }
        }
    }
    private void HighlightCandy(Candy candy)
    {
        candy.transform.localScale = new Vector2(0.4f, 0.4f);
    }
    public void ReplaceMatches()
    {
        for (int center = 0; center < centersInMatches.Count; center++)
        {
            List<CandyType> typesToAvoid = centerInMatchAndBadTypes[centersInMatches[center]];
            generator.ReplaceCandy(centersInMatches[center].x, centersInMatches[center].y, typesToAvoid);
        }
        UnregisterAllMatches();
    }
    private void CheckNeighbours(int column, int row)
    {
        List<Candy> checkRangeLeftToRight = generator.CurrentLeftAndRightCandy(column, row);
        List<Candy> checkRangeTopToBottom = generator.CurrentTopAndBottomCandy(column, row);

        List<CandyType> typesToAvoid = new List<CandyType>();
        if (checkRangeLeftToRight.Count > 0 && checkRangeTopToBottom.Count > 0 &&
            AllCandiesHaveValues(checkRangeLeftToRight) && AllCandiesHaveValues(checkRangeTopToBottom))
        {
            CheckRightLeftRange(column, row, checkRangeLeftToRight, ref typesToAvoid);
            CheckTopBottomRange(column, row, checkRangeTopToBottom, ref typesToAvoid);
        }
        else if (checkRangeLeftToRight.Count > 0 && AllCandiesHaveValues(checkRangeLeftToRight))
        {
            CheckRightLeftRange(column, row, checkRangeLeftToRight, ref typesToAvoid);
        }
        else if (checkRangeTopToBottom.Count > 0 && AllCandiesHaveValues(checkRangeTopToBottom))
        {
            CheckTopBottomRange(column, row, checkRangeTopToBottom, ref typesToAvoid);
        }
    }
    private bool AllCandiesHaveValues(List<Candy> range)
    {
        if (!range.Any(candy => candy == null))
            return true;
        else
            return false;
    }
    private void CheckRightLeftRange(int column, int row, List<Candy> range,
        ref List<CandyType> typesToAvoid)
    {
        if (CandiesInRangeAreOfTheSameType(range))
        {
            CandyType typeToAvoid = range[0].GetCandyType;
            typesToAvoid.Add(typeToAvoid);
            RegisterMatch(column, row, typesToAvoid, MatchType.horizontal);
        }
    }
    private void CheckTopBottomRange(int column, int row, List<Candy> range,
       ref List<CandyType> typesToAvoid)
    {
        if (CandiesInRangeAreOfTheSameType(range))
        {
            CandyType typeToAvoid = range[0].GetCandyType;
            typesToAvoid.Add(typeToAvoid);
            RegisterMatch(column, row, typesToAvoid, MatchType.vertical);
        }
    }
    private bool CandyIsCenterForAnotherMatch(Vector2Int centerInMatch)
    {
        return centerInMatchAndMatchType.Keys.Contains(centerInMatch);
    }
    private void RegisterMatch(int column, int row, List<CandyType> typesToAvoid,
        MatchType currentMatchType)
    {
        Vector2Int centerInMatch = new Vector2Int(column, row);
        centersInMatches.Add(centerInMatch);
        centerInMatchAndBadTypes[centerInMatch] = typesToAvoid;
        if (CandyIsCenterForAnotherMatch(centerInMatch))
        {
            SetDoubleMatchType(centerInMatch, currentMatchType);
        }
        else
        {
            centerInMatchAndMatchType[centerInMatch] = currentMatchType;
        }
    }
    private void SetDoubleMatchType(Vector2Int centerInMatch, MatchType currentMatchType)
    {
        MatchType previouseMatchType = centerInMatchAndMatchType[centerInMatch];
        if (previouseMatchType == MatchType.horizontal && currentMatchType == MatchType.vertical)
        {
            centerInMatchAndMatchType[centerInMatch] = MatchType.horizontalAndVertical;
        }
        else if (previouseMatchType == MatchType.vertical && currentMatchType == MatchType.horizontal)
        {
            centerInMatchAndMatchType[centerInMatch] = MatchType.horizontalAndVertical;
        }
    }
    public void UnregisterAllMatches()
    {
        centersInMatches.Clear();
        centerInMatchAndBadTypes.Clear();
        centerInMatchAndMatchType.Clear();
    }
    private bool CandiesInRangeAreOfTheSameType(List<Candy> checkRange)
    {
        return checkRange.All(candy => candy.GetCandyType == checkRange[0].GetCandyType);
    }
    public void Enter()
    {
        HighlightMatches();
        destroyer.SetOwner(owner);
        destroyer.SetCenterInMatchAndMatchType(centerInMatchAndMatchType);
        destroyer.OnDestroyMatches += UnregisterAllMatches;
        owner.ChangeState(destroyer);
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
public enum MatchType
{
    horizontal,
    vertical,
    horizontalAndVertical
}

