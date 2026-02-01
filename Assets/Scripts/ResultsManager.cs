using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class ResultsManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI resultsText;
    [SerializeField] private TextMeshProUGUI analysisText;
    [SerializeField] private GameObject backButton;

    private void Start()
    {
        NovelManager novelManager = FindObjectOfType<NovelManager>();
        if (novelManager != null)
        {
            if (resultsText != null)
            {
                resultsText.text = novelManager.GetResultsText();
            }
            else
            {
                Debug.LogError("ResultsText не назначен в инспекторе!");
            }

            if (analysisText != null)
            {
                analysisText.text = GenerateAnalysisText(novelManager.ChosenOptions);
            }
            else
            {
                Debug.LogError("AnalysisText не назначен в инспекторе!");
            }
        }
        else
        {
            Debug.LogError("NovelManager не найден! Убедись, что DontDestroyOnLoad работает.");
        }

        if (backButton != null)
        {
            Button backButtonComponent = backButton.GetComponent<Button>();
            if (backButtonComponent == null)
            {
                Debug.LogError("На объекте BackButton отсутствует компонент Button! Добавьте компонент Button в инспекторе.");
                return;
            }
            backButtonComponent.onClick.AddListener(GoToMainMenu);
        }
        else
        {
            Debug.LogError("BackButton не назначен в инспекторе!");
        }
    }

    private void GoToMainMenu()
    {
        SceneManager.LoadScene("MainNovell");
        NovelManager novelManager = FindObjectOfType<NovelManager>();
        if (novelManager != null) Destroy(novelManager.gameObject);
    }

    private string GenerateAnalysisText(List<int> chosenOptions)
    {
        string analysis = "Анализ твоего пути:\n";
        int correctAnswers = 0;
        int totalQuestions = 0;

        if (chosenOptions != null)
        {
            // Проверка ответов на вопросы
            if (chosenOptions.Contains(18)) correctAnswers++; // Лифт
            if (chosenOptions.Contains(19)) totalQuestions++; // Лифт (ошибка)
            if (chosenOptions.Contains(22)) correctAnswers++; // Свет
            if (chosenOptions.Contains(23)) totalQuestions++; // Свет (ошибка)
            if (chosenOptions.Contains(26)) correctAnswers++; // Время
            if (chosenOptions.Contains(27)) totalQuestions++; // Время (ошибка)

            totalQuestions += correctAnswers;

            // Анализ концовок
            if (chosenOptions.Contains(35))
                analysis += "Вы достигли научного триумфа и мирового признания, но потеряли семью.\n";
            else if (chosenOptions.Contains(39))
                analysis += "Отказ от помощи привел к забвению твоей теории.\n";
            else if (chosenOptions.Contains(47))
                analysis += "Вы нашли баланс между семьей и наукой, но теория осталась незавершенной.\n";
            else if (chosenOptions.Contains(55) || chosenOptions.Contains(57))
                analysis += "Ваш выбор привел к трагедии или забвению.\n";

            // Оценка внимательности
            analysis += $"- Вы ответили правильно на {correctAnswers} из {totalQuestions} вопросов. ";
            analysis += correctAnswers == totalQuestions ? "Вы были очень внимательны!" : "Попробуйте лучше запомнить материал.\n";
        }
        else
        {
            analysis += "- Нет данных для анализа.\n";
        }

        return analysis;
    }
}