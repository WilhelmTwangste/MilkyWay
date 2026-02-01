using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Добавляем эту строку

public class PauseManager : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject continueButton;
    public GameObject menuButton;
    public GameObject pauseButton;
    private bool isPaused = false;

    void Start()
    {
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false);
        }
        else
        {
            Debug.LogWarning("PauseMenu не назначен!");
        }

        AddModernButtonListener(continueButton, ResumeGame);
        AddModernButtonListener(menuButton, GoToMenu);
        AddModernButtonListener(pauseButton, TogglePause);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        if (isPaused)
        {
            PauseGame();
        }
        else
        {
            ResumeGame();
        }
    }

    private void PauseGame()
    {
        Time.timeScale = 0f;
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(true);
        }
    }

    private void ResumeGame()
    {
        Debug.Log("Игра возобновлена");
        Time.timeScale = 1f;
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false);
        }
        isPaused = false;
    }

    private void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }

    private void AddModernButtonListener(GameObject buttonObj, UnityEngine.Events.UnityAction action)
    {
        if (buttonObj == null)
        {
            Debug.LogWarning("Кнопка не назначена!");
            return;
        }

        Debug.Log($"Привязка обработчика к кнопке {buttonObj.name}");

        var button = buttonObj.GetComponent<Button>(); // Здесь была ошибка CS0246
        if (button != null)
        {
            Debug.Log($"Найден стандартный Button компонент на {buttonObj.name}");
            button.onClick.AddListener(action);
            return;
        }

        var clickHandler = buttonObj.GetComponent<IPointerClickHandler>();
        if (clickHandler != null)
        {
            Debug.Log($"Добавлен EventTrigger к {buttonObj.name} для Modern UI Pack кнопки.");
            EventTrigger trigger = buttonObj.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = buttonObj.AddComponent<EventTrigger>();
            }
            EventTrigger.Entry entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            entry.callback.AddListener((eventData) => { action(); });
            trigger.triggers.Add(entry);
        }
        else
        {
            Debug.LogWarning($"На объекте {buttonObj.name} не найден кликабельный компонент. Проверьте настройки кнопки.");
        }
    }
}