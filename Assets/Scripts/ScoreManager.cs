using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI HighScoreText;
    [SerializeField] public TextMeshProUGUI TextScore;

    public static int score = 0;
    public static int score1;
    int highscore;
    public static int highscore1;
    public static int scoreEnd;

    //public static ScoreManager Instance { get; private set; }

    //private Score score;
    //[SerializeField] public Text currentScoreText;
    //[SerializeField] public Text highScoreText;
    //public GameObject gameOverPanel;
    //public Text gameOverCurrentScoreText;
    //public Text gameOverHighScoreText;

    //private void Update()
    //{
    //    // Обновляем текст текущего счета и рекорда
    //    currentScoreText.text = "Счет: " + score.currentScore;
    //    highScoreText.text = "Рекорд: " + score.highScore;
    //}

    //public void ShowGameOver()
    //{
    //    // Отображаем панель завершения игры
    //    gameOverPanel.SetActive(true);
    //    gameOverCurrentScoreText.text = "Your Score: " + score.currentScore;
    //    gameOverHighScoreText.text = "High Score: " + score.highScore;
    //}

    void Start()
    {
        score = 0;
    }

    public void Update()
    {
        highscore = score;
        highscore1 = score;
        scoreEnd = score;
        TextScore.text = "Счет: " + highscore.ToString();
        if (PlayerPrefs.GetInt("score") <= highscore)
        {
            PlayerPrefs.SetInt("score", highscore);
        }
        HighScoreText.text = "Рекорд: " + PlayerPrefs.GetInt("score").ToString();
        PlayerPrefs.SetInt("scoreEnd", scoreEnd);
        PlayerPrefs.SetInt("highscore1", highscore1);
        scoreEnd = score;
        highscore1 = highscore;
    }
}