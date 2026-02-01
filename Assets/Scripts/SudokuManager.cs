using UnityEngine;
using TMPro;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SudokuManager : MonoBehaviour
{
    public GameObject gridPanel; // Панель для сетки 9x9
    public GameObject cellButtonPrefab; // Префаб кнопки ячейки
    public GameObject[] numberButtons = new GameObject[9]; // Массив для 9 фиксированных кнопок с числами 1-9
    public GameObject checkButtonModern; // Кнопка "Проверить" (Modern UI Pack)
    public GameObject clearButtonModern; // Кнопка "Очистить" (Modern UI Pack)
    public GameObject easyButtonModern; // Кнопка для лёгкого уровня
    public GameObject mediumButtonModern; // Кнопка для среднего уровня
    public GameObject hardButtonModern; // Кнопка для сложного уровня
    public TextMeshProUGUI messageTextModern; // Текст для сообщений (Modern UI Pack)
    public TextMeshProUGUI timerText; // Текст для отображения таймера
    public TextMeshProUGUI difficultyTextModern; // Текст для отображения текущего уровня сложности
    private int[,] grid = new int[9, 9]; // Текущая сетка судоку
    private int[,] solution = new int[9, 9]; // Полное решение
    private GameObject[,] cellButtons = new GameObject[9, 9]; // Кнопки ячеек
    private bool[,] isFixed = new bool[9, 9]; // Фиксированные ячейки
    private FirebaseAuth auth;
    private DatabaseReference dbReference;
    private int selectedRow = -1, selectedCol = -1; // Выбранная ячейка
    private int selectedNumber = 0; // Выбранное число для вставки
    private float elapsedTime = 0f; // Время, прошедшее с начала игры
    private bool isTimerRunning = false; // Флаг, указывающий, работает ли таймер
    private Dictionary<DifficultyLevel, float> bestTimes = new Dictionary<DifficultyLevel, float>(); // Лучшие времена по уровням
    private DifficultyLevel currentDifficulty = DifficultyLevel.Medium; // Текущий уровень сложности

    private enum DifficultyLevel
    {
        Easy,
        Medium,
        Hard
    }

    void Start()
    {
        Debug.Log("SudokuManager: Start called.");
        Debug.Log($"SudokuManager: cellButtonPrefab is {(cellButtonPrefab != null ? cellButtonPrefab.name : "null")}");

        if (cellButtonPrefab == null)
        {
            Debug.LogWarning("SudokuManager: CellButtonPrefab is null, creating programmatically.");
            GameObject prefab = new GameObject("CellButtonPrefab");
            prefab.AddComponent<Image>().color = Color.white;
            prefab.GetComponent<Image>().raycastTarget = true;

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(prefab.transform, false);
            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.fontSize = 36;
            text.color = Color.black;
            text.alignment = TextAlignmentOptions.Center;
            text.rectTransform.sizeDelta = new Vector2(70, 70);
            text.rectTransform.anchoredPosition = Vector2.zero;
            text.raycastTarget = true;
            text.enabled = true;

            Button button = prefab.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.colorMultiplier = 5f;
            button.colors = colors;

            prefab.AddComponent<ButtonHandler>();

            cellButtonPrefab = prefab;
        }

        if (gridPanel == null || cellButtonPrefab == null || numberButtons.Length != 9 || checkButtonModern == null ||
            clearButtonModern == null || messageTextModern == null || timerText == null || easyButtonModern == null ||
            mediumButtonModern == null || hardButtonModern == null || difficultyTextModern == null)
        {
            Debug.LogError("SudokuManager: One or more required fields are not assigned in the Inspector.");
            if (messageTextModern != null)
                messageTextModern.text = "Ошибка: Проверьте привязки в инспекторе!";
            return;
        }

        for (int i = 0; i < numberButtons.Length; i++)
        {
            if (numberButtons[i] == null)
            {
                Debug.LogError($"SudokuManager: Number button at index {i} is not assigned.");
                if (messageTextModern != null) messageTextModern.text = "Ошибка: Не все кнопки чисел привязаны!";
                return;
            }
        }

        if (!messageTextModern.gameObject.activeInHierarchy)
        {
            Debug.LogWarning("SudokuManager: messageTextModern game object is inactive!");
            messageTextModern.gameObject.SetActive(true);
        }

        if (!timerText.gameObject.activeInHierarchy)
        {
            Debug.LogWarning("SudokuManager: timerText game object is inactive!");
            timerText.gameObject.SetActive(true);
        }

        if (!difficultyTextModern.gameObject.activeInHierarchy)
        {
            Debug.LogWarning("SudokuManager: difficultyTextModern game object is inactive!");
            difficultyTextModern.gameObject.SetActive(true);
        }

        // Инициализация лучших времён
        bestTimes[DifficultyLevel.Easy] = PlayerPrefs.GetFloat("SudokuBestTimeEasy", float.MaxValue);
        bestTimes[DifficultyLevel.Medium] = PlayerPrefs.GetFloat("SudokuBestTimeMedium", float.MaxValue);
        bestTimes[DifficultyLevel.Hard] = PlayerPrefs.GetFloat("SudokuBestTimeHard", float.MaxValue);

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;
                dbReference = FirebaseDatabase.DefaultInstance.RootReference;
                Debug.Log("SudokuManager: Firebase initialized.");
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    messageTextModern.text = "Firebase готов!";
                    GeneratePuzzle(GetEmptyCellsForDifficulty(currentDifficulty)); // Генерируем новую головоломку
                    UpdateDifficultyUI();
                });
            }
            else
            {
                Debug.LogError("SudokuManager: Firebase init failed: " + task.Result);
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    messageTextModern.text = "Ошибка Firebase!";
                    GeneratePuzzle(GetEmptyCellsForDifficulty(currentDifficulty)); // Генерируем новую головоломку
                    UpdateDifficultyUI();
                });
            }
        });

        Debug.Log("SudokuManager: Starting UI setup.");
        SetupGrid();
        SetupNumberButtons();
        SetupControlButtons();
        SetupDifficultyButtons();
    }

    void Update()
    {
        if (isTimerRunning)
        {
            elapsedTime += Time.deltaTime;
            UpdateTimerUI();
        }
    }

    void SetupGrid()
    {
        Debug.Log("SudokuManager: Setting up grid.");
        if (gridPanel == null || cellButtonPrefab == null) return;

        foreach (Transform child in gridPanel.transform)
            Destroy(child.gameObject);

        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                Debug.Log($"SudokuManager: Creating cell at ({i}, {j}).");
                GameObject cell = Instantiate(cellButtonPrefab, gridPanel.transform);
                cellButtons[i, j] = cell;

                Button button = cell.GetComponent<Button>();
                if (button != null)
                {
                    ColorBlock colors = button.colors;
                    colors.colorMultiplier = 5f;
                    button.colors = colors;
                }

                ButtonHandler handler = cell.GetComponent<ButtonHandler>();
                if (handler == null) handler = cell.AddComponent<ButtonHandler>();
                int row = i, col = j;
                handler.SetClickAction(() => OnCellClicked(row, col));

                SetButtonInteractable(cell, true);
                Image img = cell.GetComponent<Image>();
                if (img == null)
                {
                    img = cell.AddComponent<Image>();
                    img.sprite = Resources.Load<Sprite>("Square");
                    img.color = Color.white;
                    img.raycastTarget = true;
                    Debug.LogWarning($"SudokuManager: Added Image component to cell at ({i}, {j}).");
                }
                img.raycastTarget = true;

                TextMeshProUGUI text = cell.GetComponentInChildren<TextMeshProUGUI>();
                if (text == null)
                {
                    GameObject textObj = new GameObject("Text");
                    textObj.transform.SetParent(cell.transform, false);
                    text = textObj.AddComponent<TextMeshProUGUI>();
                    text.fontSize = 36;
                    text.color = Color.black;
                    text.alignment = TextAlignmentOptions.Center;
                    text.rectTransform.sizeDelta = new Vector2(70, 70);
                    text.rectTransform.anchoredPosition = Vector2.zero;
                    Debug.LogWarning($"SudokuManager: Added TextMeshProUGUI component to cell at ({i}, {j}).");
                }
                text.text = "";
                text.raycastTarget = true;
                text.enabled = true;

                img.color = isFixed[i, j] ? Color.white : Color.white;
            }
        }
        Debug.Log("SudokuManager: Grid setup completed.");
    }

    void SetupNumberButtons()
    {
        Debug.Log("SudokuManager: Setting up number buttons.");
        for (int i = 0; i < numberButtons.Length; i++)
        {
            if (numberButtons[i] == null) continue;
            int num = i + 1;
            ButtonHandler handler = numberButtons[i].GetComponent<ButtonHandler>();
            if (handler == null) handler = numberButtons[i].AddComponent<ButtonHandler>();
            handler.SetClickAction(() => OnNumberClicked(num));
            SetButtonInteractable(numberButtons[i], true);
            Debug.Log($"SudokuManager: Registered click handler for number button {num} on {numberButtons[i].name}");

            Button button = numberButtons[i].GetComponent<Button>();
            if (button != null)
            {
                ColorBlock colors = button.colors;
                colors.colorMultiplier = 5f;
                button.colors = colors;
            }
        }
        Debug.Log("SudokuManager: Number buttons setup completed.");
    }

    void SetupControlButtons()
    {
        Debug.Log("SudokuManager: Setting up control buttons.");
        if (checkButtonModern != null)
        {
            AddModernButtonListener(checkButtonModern, CheckSolution);
            SetButtonInteractable(checkButtonModern, true);
            Debug.Log("SudokuManager: CheckButtonModern click handler registered.");
            Button button = checkButtonModern.GetComponent<Button>();
            if (button != null)
            {
                ColorBlock colors = button.colors;
                colors.colorMultiplier = 5f;
                button.colors = colors;
            }
        }
        else Debug.LogError("SudokuManager: CheckButtonModern is null.");

        if (clearButtonModern != null)
        {
            AddModernButtonListener(clearButtonModern, ClearGrid);
            SetButtonInteractable(clearButtonModern, true);
            Debug.Log("SudokuManager: ClearButtonModern click handler registered.");
            Button button = clearButtonModern.GetComponent<Button>();
            if (button != null)
            {
                ColorBlock colors = button.colors;
                colors.colorMultiplier = 5f;
                button.colors = colors;
            }
        }
        else Debug.LogError("SudokuManager: ClearButtonModern is null.");
    }

    void SetupDifficultyButtons()
    {
        Debug.Log("SudokuManager: Setting up difficulty buttons.");
        if (easyButtonModern != null)
        {
            AddModernButtonListener(easyButtonModern, () => SetDifficulty(DifficultyLevel.Easy));
            SetButtonInteractable(easyButtonModern, true);
            Debug.Log("SudokuManager: EasyButtonModern click handler registered.");
            Button button = easyButtonModern.GetComponent<Button>();
            if (button != null)
            {
                ColorBlock colors = button.colors;
                colors.colorMultiplier = 5f;
                button.colors = colors;
            }
        }
        else Debug.LogError("SudokuManager: EasyButtonModern is null.");

        if (mediumButtonModern != null)
        {
            AddModernButtonListener(mediumButtonModern, () => SetDifficulty(DifficultyLevel.Medium));
            SetButtonInteractable(mediumButtonModern, true);
            Debug.Log("SudokuManager: MediumButtonModern click handler registered.");
            Button button = mediumButtonModern.GetComponent<Button>();
            if (button != null)
            {
                ColorBlock colors = button.colors;
                colors.colorMultiplier = 5f;
                button.colors = colors;
            }
        }
        else Debug.LogError("SudokuManager: MediumButtonModern is null.");

        if (hardButtonModern != null)
        {
            AddModernButtonListener(hardButtonModern, () => SetDifficulty(DifficultyLevel.Hard));
            SetButtonInteractable(hardButtonModern, true);
            Debug.Log("SudokuManager: HardButtonModern click handler registered.");
            Button button = hardButtonModern.GetComponent<Button>();
            if (button != null)
            {
                ColorBlock colors = button.colors;
                colors.colorMultiplier = 5f;
                button.colors = colors;
            }
        }
        else Debug.LogError("SudokuManager: HardButtonModern is null.");
    }

    private void AddModernButtonListener(GameObject buttonObject, UnityEngine.Events.UnityAction action)
    {
        var standardButton = buttonObject.GetComponent<Button>();
        if (standardButton != null)
        {
            standardButton.onClick.AddListener(action);
            Debug.Log($"SudokuManager: Added click listener to standard Button on {buttonObject.name}");
            return;
        }

        var trigger = buttonObject.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = buttonObject.AddComponent<EventTrigger>();
            Debug.Log($"SudokuManager: Added EventTrigger to {buttonObject.name}");
        }

        var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
        entry.callback.AddListener((eventData) => action.Invoke());
        trigger.triggers.Add(entry);
        Debug.Log($"SudokuManager: Added PointerClick listener to {buttonObject.name}");
    }

    private void SetButtonInteractable(GameObject buttonObject, bool interactable)
    {
        var standardButton = buttonObject.GetComponent<Button>();
        if (standardButton != null)
        {
            standardButton.interactable = interactable;
            return;
        }

        var selectable = buttonObject.GetComponent<Selectable>();
        if (selectable != null)
            selectable.interactable = interactable;
        else
            Debug.LogWarning($"SudokuManager: No selectable component found on {buttonObject.name} to set interactable state.");
    }

    void SetDifficulty(DifficultyLevel difficulty)
    {
        Debug.Log($"SudokuManager: Setting difficulty to {difficulty}.");
        currentDifficulty = difficulty;
        GeneratePuzzle(GetEmptyCellsForDifficulty(difficulty));
        UpdateDifficultyUI();
    }

    int GetEmptyCellsForDifficulty(DifficultyLevel difficulty)
    {
        switch (difficulty)
        {
            case DifficultyLevel.Easy:
                return Random.Range(20, 26); // 20-25 пустых ячеек
            case DifficultyLevel.Medium:
                return Random.Range(30, 36); // 30-35 пустых ячеек
            case DifficultyLevel.Hard:
                return Random.Range(40, 46); // 40-45 пустых ячеек
            default:
                return 30;
        }
    }

    void UpdateDifficultyUI()
    {
        string difficultyName = currentDifficulty switch
        {
            DifficultyLevel.Easy => "Лёгкий",
            DifficultyLevel.Medium => "Средний",
            DifficultyLevel.Hard => "Сложный",
            _ => "Средний"
        };
        difficultyTextModern.text = $"Уровень сложности: {difficultyName}";
        Debug.Log($"SudokuManager: Updated difficulty UI to {difficultyName}.");
    }

    void GeneratePuzzle(int emptyCells)
    {
        Debug.Log("SudokuManager: Generating puzzle.");
        for (int i = 0; i < 9; i++)
            for (int j = 0; j < 9; j++)
            {
                grid[i, j] = 0;
                solution[i, j] = 0;
                isFixed[i, j] = false;
            }

        FillDiagonal();
        FillRemaining(0, 3);
        for (int i = 0; i < 9; i++)
            for (int j = 0; j < 9; j++)
                solution[i, j] = grid[i, j];

        for (int i = 0; i < 9; i++)
            for (int j = 0; j < 9; j++)
                if (grid[i, j] != 0)
                    isFixed[i, j] = true;

        RemoveCells(emptyCells);
        UpdateGridUI();
        SavePuzzleToFirebase();

        ResetTimer();
        StartTimer();
    }

    void FillDiagonal()
    {
        for (int i = 0; i < 9; i += 3)
        {
            List<int> numbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            for (int j = 0; j < 3; j++)
                for (int k = 0; k < 3; k++)
                {
                    int index = Random.Range(0, numbers.Count);
                    grid[i + j, i + k] = numbers[index];
                    numbers.RemoveAt(index);
                }
        }
    }

    bool FillRemaining(int i, int j)
    {
        if (j >= 9 && i < 8)
        {
            i++;
            j = 0;
        }
        if (i >= 9 && j >= 9)
            return true;

        if (i < 3)
        {
            if (j < 3)
                j = 3;
        }
        else if (i < 6)
        {
            if (j == (i / 3) * 3)
                j += 3;
        }
        else
        {
            if (j == 6)
            {
                i++;
                j = 0;
                if (i >= 9)
                    return true;
            }
        }

        for (int num = 1; num <= 9; num++)
        {
            if (IsSafe(i, j, num))
            {
                grid[i, j] = num;
                if (FillRemaining(i, j + 1))
                    return true;
                grid[i, j] = 0;
            }
        }
        return false;
    }

    bool IsSafe(int i, int j, int num)
    {
        return (UnUsedInRow(i, num) && UnUsedInCol(j, num) && UnUsedInBox(i - i % 3, j - j % 3, num));
    }

    bool UnUsedInRow(int i, int num)
    {
        for (int j = 0; j < 9; j++)
            if (grid[i, j] == num)
                return false;
        return true;
    }

    bool UnUsedInCol(int j, int num)
    {
        for (int i = 0; i < 9; i++)
            if (grid[i, j] == num)
                return false;
        return true;
    }

    bool UnUsedInBox(int rowStart, int colStart, int num)
    {
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
                if (grid[rowStart + i, colStart + j] == num)
                    return false;
        return true;
    }

    void RemoveCells(int k)
    {
        Debug.Log($"SudokuManager: Removing {k} cells.");
        while (k > 0)
        {
            int i = Random.Range(0, 9);
            int j = Random.Range(0, 9);
            if (grid[i, j] != 0)
            {
                grid[i, j] = 0;
                isFixed[i, j] = false;
                k--;
            }
        }
    }

    void UpdateGridUI()
    {
        Debug.Log("SudokuManager: Updating grid UI.");
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                if (cellButtons[i, j] == null) continue;

                TextMeshProUGUI text = cellButtons[i, j].GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    text.text = grid[i, j] == 0 ? "" : grid[i, j].ToString();
                    text.enabled = true;
                }

                SetButtonInteractable(cellButtons[i, j], !isFixed[i, j]);

                Image img = cellButtons[i, j].GetComponent<Image>();
                if (img != null)
                {
                    img.color = isFixed[i, j] ? Color.white : Color.white;
                    img.enabled = true;
                }
            }
        }
    }

    void OnCellClicked(int row, int col)
    {
        Debug.Log($"SudokuManager: Cell clicked at ({row}, {col}), isFixed: {isFixed[row, col]}");
        if (!isFixed[row, col])
        {
            if (selectedNumber == 0)
            {
                messageTextModern.text = "Сначала выберите число!";
                return;
            }

            selectedRow = row;
            selectedCol = col;
            grid[row, col] = selectedNumber;
            if (cellButtons[row, col] != null)
            {
                TextMeshProUGUI text = cellButtons[row, col].GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    text.text = selectedNumber.ToString();
                    text.enabled = true;
                    text.gameObject.SetActive(true);
                    Debug.Log($"SudokuManager: Set {selectedNumber} at ({row}, {col})");
                    messageTextModern.text = $"Установлено {selectedNumber} в ячейку ({row + 1}, {col + 1})";
                }
            }
            SavePuzzleToFirebase();
        }
        else
        {
            messageTextModern.text = "Нельзя изменить фиксированную ячейку!";
        }
    }

    void OnNumberClicked(int num)
    {
        Debug.Log($"SudokuManager: Number {num} clicked.");
        selectedNumber = num;
        messageTextModern.text = $"Выбрано число {num}. Выберите ячейку.";
    }

    void CheckSolution()
    {
        Debug.Log("SudokuManager: Checking solution.");
        bool correct = true;
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                if (cellButtons[i, j] == null) continue;

                Image img = cellButtons[i, j].GetComponent<Image>();
                if (grid[i, j] != solution[i, j])
                {
                    correct = false;
                    if (img != null)
                        img.color = Color.red;
                }
                else if (img != null)
                    img.color = isFixed[i, j] ? Color.white : Color.white;
            }
        }
        messageTextModern.text = correct ? "Поздравляем! Решение верно!" : "Есть ошибки!";
        if (correct)
        {
            StopTimer();
            SaveCompletionToFirebase();
            SaveBestTimeToPlayerPrefs(currentDifficulty); // Сохраняем лучший результат для текущего уровня
            ShowEndScene(); // Переход к сцене EndPlay
        }
    }

    void ClearGrid()
    {
        Debug.Log("SudokuManager: Clearing grid.");
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                if (!isFixed[i, j])
                {
                    grid[i, j] = 0;
                    if (cellButtons[i, j] != null)
                    {
                        TextMeshProUGUI text = cellButtons[i, j].GetComponentInChildren<TextMeshProUGUI>();
                        if (text != null) text.text = "";
                        Image img = cellButtons[i, j].GetComponent<Image>();
                        if (img != null) img.color = Color.white;
                    }
                }
            }
        }
        selectedNumber = 0;
        messageTextModern.text = "Сетка очищена.";
        SavePuzzleToFirebase();
    }

    void SavePuzzleToFirebase()
    {
        if (auth?.CurrentUser == null || dbReference == null) return;

        Debug.Log("SudokuManager: Saving puzzle to Firebase.");
        string userId = auth.CurrentUser.UserId;
        var puzzleData = new { grid = grid, isFixed = isFixed, elapsedTime = elapsedTime };
        dbReference.Child("users").Child(userId).Child("sudoku").SetRawJsonValueAsync(JsonUtility.ToJson(puzzleData)).ContinueWith(task =>
        {
            if (task.IsFaulted)
                Debug.LogError("SudokuManager: Failed to save puzzle: " + task.Exception);
            else
                Debug.Log("SudokuManager: Puzzle saved successfully.");
        });
    }

    void SaveCompletionToFirebase()
    {
        if (auth?.CurrentUser == null || dbReference == null) return;

        Debug.Log("SudokuManager: Saving completion to Firebase.");
        string userId = auth.CurrentUser.UserId;
        var completionData = new { completed = true, completionTime = elapsedTime };
        dbReference.Child("users").Child(userId).Child("sudoku_completed").SetRawJsonValueAsync(JsonUtility.ToJson(completionData)).ContinueWith(task =>
        {
            if (task.IsFaulted)
                Debug.LogError("SudokuManager: Failed to save completion: " + task.Exception);
            else
                Debug.Log("SudokuManager: Completion saved successfully.");
        });
    }

    void SaveBestTimeToPlayerPrefs(DifficultyLevel difficulty)
    {
        Debug.Log($"SudokuManager: Checking and saving best time to PlayerPrefs for {difficulty}.");
        float currentBestTime = bestTimes[difficulty];
        if (elapsedTime < currentBestTime)
        {
            bestTimes[difficulty] = elapsedTime;
            string key = $"SudokuBestTime{difficulty}";
            PlayerPrefs.SetFloat(key, elapsedTime);
            PlayerPrefs.Save();
            Debug.Log($"SudokuManager: New best time saved to PlayerPrefs for {difficulty}: {elapsedTime}");
        }
    }

    void LoadBestTimeFromPlayerPrefs()
    {
        Debug.Log("SudokuManager: Loading best times from PlayerPrefs.");
        bestTimes[DifficultyLevel.Easy] = PlayerPrefs.GetFloat("SudokuBestTimeEasy", float.MaxValue);
        bestTimes[DifficultyLevel.Medium] = PlayerPrefs.GetFloat("SudokuBestTimeMedium", float.MaxValue);
        bestTimes[DifficultyLevel.Hard] = PlayerPrefs.GetFloat("SudokuBestTimeHard", float.MaxValue);
        Debug.Log($"SudokuManager: Loaded best times - Easy: {bestTimes[DifficultyLevel.Easy]}, Medium: {bestTimes[DifficultyLevel.Medium]}, Hard: {bestTimes[DifficultyLevel.Hard]}");
    }

    void StartTimer()
    {
        isTimerRunning = true;
        UpdateTimerUI();
    }

    void StopTimer()
    {
        isTimerRunning = false;
    }

    void ResetTimer()
    {
        elapsedTime = 0f;
        UpdateTimerUI();
    }

    void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(elapsedTime / 60);
        int seconds = Mathf.FloorToInt(elapsedTime % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    void ShowEndScene()
    {
        Debug.Log("SudokuManager: Showing EndPlay scene.");
        // Сохраняем текущее время и лучший результат для передачи в EndPlay
        PlayerPrefs.SetFloat("CompletionTime", elapsedTime);
        PlayerPrefs.SetFloat("BestTime", bestTimes[currentDifficulty]); // Сохраняем лучший результат для текущего уровня
        PlayerPrefs.SetInt("CurrentDifficulty", (int)currentDifficulty); // Сохраняем текущий уровень сложности
        PlayerPrefs.Save();
        SceneManager.LoadScene("EndSudoku");
    }

    [System.Serializable]
    private class PuzzleData
    {
        public int[,] grid;
        public bool[,] isFixed;
        public float elapsedTime;
    }
}