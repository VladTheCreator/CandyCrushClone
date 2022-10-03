using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardFiller : MonoBehaviour
{
    private GridGenerator generator;
    private MatchChecker checker;
    private void Awake()
    {
        generator = GetComponent<GridGenerator>();
        checker = GetComponent<MatchChecker>(); 
    }
    public IEnumerator FillEmptyIndexes()
    {
        yield return new WaitForSeconds(1f);
        List<Vector2Int> emptyIndexes = generator.FindEmpty();
        for (int i = 0; i < emptyIndexes.Count; i++)
        {
            Vector2 position = generator.IndexToPosition(emptyIndexes[i]);
            Candy prefab = generator.GetRandomCandyPrefab();
            Candy instance = Instantiate(prefab, position, Quaternion.identity);
            generator.SetCandyByIndex(emptyIndexes[i], instance);
        }
        if (checker.FindMatches())
        {
            checker.DestroyMatches();
        }
    }
}
