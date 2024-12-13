using UnityEngine;
using System.Collections;

public class Candy : MonoBehaviour
{
    [SerializeField] private CandyType candyType;
    private float shrinkPercent = 0.1f;
    public CandyType GetCandyType => candyType;
    private Coroutine pulsatingCoroutine;
    private Vector3 originalSize;
    private void Start()
    {
        originalSize = transform.localScale;
    }

    public void StartPulsating()
    {
        pulsatingCoroutine = StartCoroutine(Pulsate());
    }
    public void StopPulsating()
    {
        if (pulsatingCoroutine != null)
        {
            StopCoroutine(pulsatingCoroutine);
            pulsatingCoroutine = null;
        }
        transform.localScale = originalSize;
    }
    private IEnumerator Pulsate()
    {
        Vector3 originalSize = transform.localScale;
        Vector3 newSize = new Vector2(originalSize.x - shrinkPercent, originalSize.y - shrinkPercent);
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            transform.localScale = newSize;
            yield return new WaitForSeconds(0.5f);
            transform.localScale = originalSize;
        }
    }
}
public enum CandyType
{
    HalfCircleRed,
    RoundRedWhite,
    SquareBlue,
    TriangleBlueTop,
    UmbrelaHandleVioletWhite
}

