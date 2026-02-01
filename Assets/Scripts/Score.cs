using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Score : MonoBehaviour
{
    //public int currentScore { get; private set; }
    //public int highScore { get; private set; }

    //private void Start()
    //{
    //    // Загружаем рекорд из PlayerPrefs
    //    highScore = PlayerPrefs.GetInt("HighScore", 0);
    //    currentScore = 0;
    //}

    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    // Проверяем, что игрок пересекает платформу
    //    if (collision.CompareTag("Bonus"))
    //    {
    //        // Добавляем очки за проход платформы
    //        AddScore(1); // Например, за каждую платформу даем 1 очко
    //    }
    //}

    //public void AddScore(int points)
    //{
    //    currentScore += points;

    //    // Проверяем, если текущий счет больше рекорда
    //    if (currentScore > highScore)
    //    {
    //        highScore = currentScore;
    //        PlayerPrefs.SetInt("HighScore", highScore);
    //        PlayerPrefs.Save();
    //    }
    //}

    //public void ResetScore()
    //{
    //    currentScore = 0;
    //}
    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.CompareTag("Player"))
        {
            ScoreManager.score++;
            //ScoreManager.Instance.UpdateScoreText();
            Destroy(gameObject);
        }
    }
}