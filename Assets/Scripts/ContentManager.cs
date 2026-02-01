using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Auth;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro; // Добавляем пространство имен для TextMeshPro

public class SimpleContentManager : MonoBehaviour
{
    public TextMeshProUGUI errorText; // Изменяем на TextMeshProUGUI для отображения ошибок
    private FirebaseAuth auth;
    private DatabaseReference dbReference;
    private bool isProcessing = false;

    void Start()
    {
        Debug.Log("SimpleContentManager: Start called.");

        // Инициализация Firebase
        InitializeFirebase();
    }

    private void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            Debug.Log("SimpleContentManager: Firebase CheckAndFixDependenciesAsync completed.");
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;
                dbReference = FirebaseDatabase.DefaultInstance.RootReference;
                Debug.Log("SimpleContentManager: Firebase initialized successfully.");

                // Запускаем корутину для загрузки возраста
                StartCoroutine(LoadUserAgeCoroutine());
            }
            else
            {
                Debug.LogError($"SimpleContentManager: Firebase initialization failed: {dependencyStatus}");
                if (errorText != null) errorText.text = "Ошибка: Firebase не инициализирован!";
                LoadSceneWithError("RegistrationScene", "Firebase не инициализирован!");
            }
        });
    }

    private IEnumerator LoadUserAgeCoroutine()
    {
        Debug.Log("SimpleContentManager: LoadUserAgeCoroutine started.");

        if (auth.CurrentUser == null)
        {
            Debug.LogWarning("SimpleContentManager: No user logged in, cannot load age.");
            if (errorText != null) errorText.text = "Ошибка: Пользователь не авторизован!";
            LoadSceneWithError("RegistrationScene", "Пользователь не авторизован!");
            yield break;
        }

        string userId = auth.CurrentUser.UserId;
        Debug.Log($"SimpleContentManager: Loading age for user {userId}");

        // Запрашиваем возраст с тайм-аутом
        var task = dbReference.Child("users").Child(userId).Child("age").GetValueAsync();
        float timeout = 10f; // Тайм-аут 10 секунд
        float elapsed = 0f;

        while (!task.IsCompleted && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (!task.IsCompleted)
        {
            Debug.LogError("SimpleContentManager: Timeout while loading age.");
            if (errorText != null) errorText.text = "Ошибка: Превышено время ожидания загрузки возраста!";
            LoadSceneWithError("RegistrationScene", "Превышено время ожидания!");
            yield break;
        }

        if (task.IsFaulted)
        {
            Debug.LogError($"SimpleContentManager: Failed to load age: {task.Exception?.Message}");
            if (errorText != null) errorText.text = "Ошибка загрузки возраста!";
            LoadSceneWithError("RegistrationScene", "Ошибка загрузки возраста!");
            yield break;
        }

        DataSnapshot snapshot = task.Result;
        if (snapshot.Exists && snapshot.Value != null)
        {
            if (int.TryParse(snapshot.Value.ToString(), out int age))
            {
                Debug.Log($"SimpleContentManager: Loaded age: {age}");
                LoadSceneBasedOnAge(age);
            }
            else
            {
                Debug.LogWarning("SimpleContentManager: Invalid age format in database.");
                if (errorText != null) errorText.text = "Ошибка: Некорректный формат возраста!";
                LoadSceneWithError("RegistrationScene", "Некорректный формат возраста!");
            }
        }
        else
        {
            Debug.LogWarning("SimpleContentManager: Age data not found in database.");
            if (errorText != null) errorText.text = "Ошибка: Возраст не указан!";
            LoadSceneWithError("RegistrationScene", "Возраст не указан!");
        }
    }

    private void LoadSceneBasedOnAge(int age)
    {
        Debug.Log($"SimpleContentManager: Loading scene based on age {age}");
        string sceneToLoad = age < 18 ? "MainSceneYoung" : "MainScene";
        Debug.Log($"SimpleContentManager: Selected scene to load: {sceneToLoad}");

        LoadScene(sceneToLoad);
    }

    private void LoadScene(string sceneName)
    {
        Debug.Log($"SimpleContentManager: LoadScene called for scene: {sceneName}");

        // Проверка, добавлена ли сцена в Build Settings
        int sceneIndex = SceneUtility.GetBuildIndexByScenePath($"Scenes/{sceneName}");
        if (sceneIndex == -1)
        {
            Debug.LogError($"SimpleContentManager: Scene '{sceneName}' not found in Build Settings!");
            if (errorText != null) errorText.text = $"Ошибка: Сцена {sceneName} не найдена в Build Settings!";
            LoadSceneWithError("RegistrationScene", $"Сцена {sceneName} не найдена!");
            return;
        }

        // Проверка текущей сцены
        string currentScene = SceneManager.GetActiveScene().name;
        Debug.Log($"SimpleContentManager: Current scene: {currentScene}");
        if (currentScene == sceneName)
        {
            Debug.Log($"SimpleContentManager: Already on {sceneName}, no need to load.");
            return;
        }

        // Синхронная загрузка сцены
        try
        {
            Debug.Log($"SimpleContentManager: Loading scene '{sceneName}' synchronously.");
            SceneManager.LoadScene(sceneName);
            Debug.Log($"SimpleContentManager: Scene '{sceneName}' loaded successfully.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"SimpleContentManager: Failed to load scene '{sceneName}': {ex.Message}");
            if (errorText != null) errorText.text = $"Ошибка загрузки сцены: {ex.Message}";
            LoadSceneWithError("RegistrationScene", $"Ошибка загрузки сцены: {ex.Message}");
        }
    }

    private void LoadSceneWithError(string sceneName, string errorMessage)
    {
        Debug.Log($"SimpleContentManager: LoadSceneWithError called for scene: {sceneName}, Error: {errorMessage}");
        SceneManager.LoadScene(sceneName);
    }
}