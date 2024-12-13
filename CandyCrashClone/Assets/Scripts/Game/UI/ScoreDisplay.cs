using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreDisplay : MonoBehaviour
{
    public void DisplayScore(int score)
    {
        GetComponent<TMP_Text>().text = "Score: " + score.ToString();
    }
}
