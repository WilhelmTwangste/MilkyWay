using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class NovelManager : MonoBehaviour
{
    [SerializeField] private Image backgroundImage;          // Фон
    [SerializeField] private Image leftCharacter;            // Левый персонаж
    [SerializeField] private Image rightCharacter;           // Правый персонаж
    [SerializeField] private TextMeshProUGUI nameText;       // Текст имени
    [SerializeField] private TextMeshProUGUI dialogueText;   // Текст диалога
    [SerializeField] private GameObject choicesPanel;        // Панель выборов
    [SerializeField] private Button[] choiceButtons;         // Кнопки выбора
    [SerializeField] private Button tapArea;                 // Прозрачная кнопка для тапов
    [SerializeField] private AudioSource audioSource;        // Аудио
    [SerializeField] private TextAsset dialogueJson;         // JSON с диалогами

    private List<Dialogue> dialogues;                        // Список диалогов
    private int currentDialogueIndex = 0;                    // Текущий индекс
    private bool isChoicesActive = false;                    // Активна ли панель выборов
    private List<int> chosenOptions = new List<int>();       // Список выбранных ID

    private void Awake()
    {
        if (dialogueJson == null)
        {
            Debug.LogError("Dialogue JSON не назначен в инспекторе!");
            return;
        }
        DialogueWrapper wrapper = JsonUtility.FromJson<DialogueWrapper>(dialogueJson.text);
        if (wrapper == null || wrapper.dialogues == null)
        {
            Debug.LogError("Ошибка при парсинге JSON!");
            return;
        }
        dialogues = wrapper.dialogues.ToList();
        Debug.Log($"Загружено диалогов: {dialogues.Count}");
    }

    private void Start()
    {
        if (backgroundImage == null || leftCharacter == null || rightCharacter == null ||
            nameText == null || dialogueText == null || choicesPanel == null ||
            choiceButtons == null || choiceButtons.Length == 0 || tapArea == null)
        {
            Debug.LogError("Один или несколько UI-элементов не назначены в инспекторе!");
            return;
        }

        if (!nameText.gameObject.activeInHierarchy || !dialogueText.gameObject.activeInHierarchy)
        {
            Debug.LogWarning("Диалоговая панель или текстовые элементы выключены!");
            nameText.gameObject.SetActive(true);
            dialogueText.gameObject.SetActive(true);
        }

        if (!choicesPanel.activeInHierarchy)
        {
            Debug.LogWarning("ChoicesPanel выключена на старте!");
            choicesPanel.SetActive(false);
        }
        ShowDialogue(currentDialogueIndex);
        if (tapArea != null)
        {
            tapArea.onClick.AddListener(OnScreenTap);
            Debug.Log("TapArea обработчик привязан");
            tapArea.gameObject.SetActive(true);
        }
    }

    private void Update()
    {
        if (!isChoicesActive)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("Клик мыши зарегистрирован");
                OnScreenTap();
            }
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                Debug.Log("Тап зарегистрирован");
                OnScreenTap();
            }
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Пробел: обход панели выборов");
            isChoicesActive = false;
            choicesPanel.SetActive(false);
            ShowDialogue(currentDialogueIndex + 1);
        }
    }

    private void ShowDialogue(int index)
    {
        if (index < 0 || index >= dialogues.Count)
        {
            Debug.Log("Конец истории!");
            ShowResults();
            return;
        }

        Dialogue dialogue = dialogues[index];
        currentDialogueIndex = index;

        if (nameText != null) nameText.text = dialogue.characterName;
        if (dialogueText != null) dialogueText.text = dialogue.text;
        Debug.Log($"Показан диалог {index}: {dialogue.text}");

        // Проверяем и устанавливаем фон
        if (backgroundImage != null && !backgroundImage.gameObject.activeInHierarchy)
        {
            backgroundImage.gameObject.SetActive(true); // Активируем, если выключен
        }
        if (backgroundImage != null && !string.IsNullOrEmpty(dialogue.background))
        {
            Sprite bgSprite = Resources.Load<Sprite>("Backgrounds/" + dialogue.background);
            if (bgSprite != null)
            {
                backgroundImage.sprite = bgSprite;
                Debug.Log($"Фон установлен: {dialogue.background}");
            }
            else
            {
                Debug.LogWarning($"Спрайт фона {dialogue.background} не найден!");
            }
        }

        // Сбрасываем видимость обоих персонажей
        if (leftCharacter != null) leftCharacter.color = Color.clear;
        if (rightCharacter != null) rightCharacter.color = Color.clear;

        // Загружаем спрайт персонажа
        if (!string.IsNullOrEmpty(dialogue.characterSprite))
        {
            Sprite charSprite = Resources.Load<Sprite>("Characters/" + dialogue.characterSprite);
            if (charSprite != null)
            {
                if (leftCharacter != null && dialogue.characterPosition.ToLower() == "left")
                {
                    leftCharacter.sprite = charSprite;
                    leftCharacter.color = Color.white;
                    Debug.Log($"Персонаж {dialogue.characterSprite} отображен слева");
                }
                else if (rightCharacter != null && dialogue.characterPosition.ToLower() == "right")
                {
                    rightCharacter.sprite = charSprite;
                    rightCharacter.color = Color.white;
                    Debug.Log($"Персонаж {dialogue.characterSprite} отображен справа");
                }
                else
                {
                    Debug.LogWarning($"Некорректная позиция персонажа или компонент отсутствует: {dialogue.characterPosition}");
                }
            }
            else
            {
                Debug.LogWarning($"Спрайт персонажа {dialogue.characterSprite} не найден!");
            }
        }

        isChoicesActive = dialogue.hasChoices;
        choicesPanel.SetActive(isChoicesActive);
        Debug.Log($"Панель выборов: {(isChoicesActive ? "активна" : "неактивна")}");

        if (isChoicesActive)
        {
            for (int i = 0; i < choiceButtons.Length; i++)
            {
                if (i < dialogue.choices.Length)
                {
                    choiceButtons[i].gameObject.SetActive(true);
                    var buttonText = choiceButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonText != null) buttonText.text = dialogue.choices[i].choiceText;
                    else Debug.LogWarning($"Кнопка {i} не имеет TextMeshProUGUI!");
                    choiceButtons[i].interactable = true;
                    int branchIndex = dialogue.choices[i].branchIndex;
                    choiceButtons[i].onClick.RemoveAllListeners();
                    choiceButtons[i].onClick.AddListener(() => ChooseOption(branchIndex));
                    choiceButtons[i].onClick.AddListener(() => Debug.Log($"Кнопка {i} нажата, ведет к {branchIndex}"));
                    Debug.Log($"Кнопка {i}: {dialogue.choices[i].choiceText}, ведет к индексу {branchIndex}");
                }
                else choiceButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void OnScreenTap()
    {
        if (isChoicesActive)
        {
            Debug.Log("Тап игнорируется: активна панель выборов");
            return;
        }

        Debug.Log($"Переход к диалогу {currentDialogueIndex + 1}");
        ShowDialogue(currentDialogueIndex + 1);
    }

    private void ChooseOption(int branchIndex)
    {
        chosenOptions.Add(branchIndex); // Запоминаем выбор
        Debug.Log($"Запомнен выбор: {branchIndex}");
        isChoicesActive = false;
        choicesPanel.SetActive(false);
        ShowDialogue(branchIndex);
    }

    private void ShowResults()
    {
        // Сохраняем текущий объект, но отключаем UI для следующей сцены
        if (backgroundImage != null) backgroundImage.gameObject.SetActive(false);
        if (leftCharacter != null) leftCharacter.gameObject.SetActive(false);
        if (rightCharacter != null) rightCharacter.gameObject.SetActive(false);
        if (nameText != null) nameText.gameObject.SetActive(false);
        if (dialogueText != null) dialogueText.gameObject.SetActive(false);
        if (choicesPanel != null) choicesPanel.SetActive(false);
        if (tapArea != null) tapArea.gameObject.SetActive(false);

        DontDestroyOnLoad(gameObject);
        SceneManager.LoadScene("ResultsHistory");
    }

    // Метод для получения статистики (можно вызвать из сцены Results)
    public string GetResultsText()
    {
        string resultText = "Итоги твоего приключения:\n";

        if (chosenOptions.Contains(1)) // Пример: выбор "В лес" (ID = 1)
            resultText += "- Ты пошла в лес и нашла листья.\n";
        if (chosenOptions.Contains(3)) // Пример: выбор "В город" (ID = 3)
            resultText += "- Ты пошла в город и купила еду.\n";

        if (chosenOptions.Count == 0)
            resultText += "- Ты не сделала ни одного выбора.\n";

        resultText += "\nНажми кнопку, чтобы вернуться на главную страницу.";
        return resultText;
    }

    // Публичное свойство для доступа к chosenOptions
    public List<int> ChosenOptions
    {
        get { return chosenOptions; }
    }
}

[System.Serializable]
public class DialogueWrapper
{
    public Dialogue[] dialogues;
}