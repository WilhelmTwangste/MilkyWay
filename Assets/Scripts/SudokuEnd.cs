using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class SudokuEnd : MonoBehaviour
{
    public TextMeshProUGUI completionTimeText; // Текстовое поле для времени решения
    public TextMeshProUGUI bestTimeText; // Текстовое поле для лучшего времени текущего уровня
    public TextMeshProUGUI easyBestTimeText; // Текстовое поле для лучшего времени на лёгком уровне
    public TextMeshProUGUI mediumBestTimeText; // Текстовое поле для лучшего времени на среднем уровне
    public TextMeshProUGUI hardBestTimeText; // Текстовое поле для лучшего времени на сложном уровне
    public TextMeshProUGUI currentDifficultyText; // Текстовое поле для текущего уровня сложности

    private enum DifficultyLevel
    {
        Easy,
        Medium,
        Hard
    }

    void Start()
    {
        // Проверка и отображение времени решения
        if (completionTimeText != null)
        {
            float completionTime = PlayerPrefs.GetFloat("CompletionTime", 0f);
            completionTimeText.text = $"Время решения: {FormatTime(completionTime)}";
        }
        else
        {
            Debug.LogError("completionTimeText не назначен в инспекторе!");
        }

        // Загружаем текущий уровень сложности
        DifficultyLevel currentDifficulty = (DifficultyLevel)PlayerPrefs.GetInt("CurrentDifficulty", (int)DifficultyLevel.Medium);

        // Отображение текущего уровня сложности
        if (currentDifficultyText != null)
        {
            string difficultyName = currentDifficulty switch
            {
                DifficultyLevel.Easy => "лёгкий",
                DifficultyLevel.Medium => "средний",
                DifficultyLevel.Hard => "сложный",
                _ => "средний"
            };
            currentDifficultyText.text = $"Уровень сложности: {difficultyName}";
        }
        else
        {
            Debug.LogError("currentDifficultyText не назначен в инспекторе!");
        }

        // Проверка и отображение лучшего времени для текущего уровня
        if (bestTimeText != null)
        {
            float bestTime = PlayerPrefs.GetFloat("BestTime", float.MaxValue);
            bestTimeText.text = bestTime != float.MaxValue ? $"Лучший результат (текущий уровень): {FormatTime(bestTime)}" : "Лучший результат (текущий уровень): Нет данных";
        }
        else
        {
            Debug.LogError("bestTimeText не назначен в инспекторе!");
        }

        // Проверка и отображение лучшего времени для лёгкого уровня
        if (easyBestTimeText != null)
        {
            float easyBestTime = PlayerPrefs.GetFloat("SudokuBestTimeEasy", float.MaxValue);
            easyBestTimeText.text = easyBestTime != float.MaxValue ? $"Лучший результат (лёгкий): {FormatTime(easyBestTime)}" : "Лучший результат (Лёгкий): нет данных";
        }
        else
        {
            Debug.LogError("easyBestTimeText не назначен в инспекторе!");
        }

        // Проверка и отображение лучшего времени для среднего уровня
        if (mediumBestTimeText != null)
        {
            float mediumBestTime = PlayerPrefs.GetFloat("SudokuBestTimeMedium", float.MaxValue);
            mediumBestTimeText.text = mediumBestTime != float.MaxValue ? $"Лучший результат (средний): {FormatTime(mediumBestTime)}" : "Лучший результат (Средний): нет данных";
        }
        else
        {
            Debug.LogError("mediumBestTimeText не назначен в инспекторе!");
        }

        // Проверка и отображение лучшего времени для сложного уровня
        if (hardBestTimeText != null)
        {
            float hardBestTime = PlayerPrefs.GetFloat("SudokuBestTimeHard", float.MaxValue);
            hardBestTimeText.text = hardBestTime != float.MaxValue ? $"Лучший результат (сложный): {FormatTime(hardBestTime)}" : "Лучший результат (Сложный): нет данных";
        }
        else
        {
            Debug.LogError("hardBestTimeText не назначен в инспекторе!");
        }
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        return $"{minutes:00}:{seconds:00}";
    }
}