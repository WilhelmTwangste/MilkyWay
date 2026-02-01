using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Endgame : MonoBehaviour
{
    [SerializeField] public Text endTextScore;

    void Start()
    {
        int scoreEnd = PlayerPrefs.GetInt("scoreEnd", 0);
        endTextScore.text = "¬аш счет: " + scoreEnd.ToString();
    }
}