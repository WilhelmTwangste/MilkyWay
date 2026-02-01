using UnityEngine;
using TMPro;
using Firebase;
using Firebase.Auth;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SignOutManager : MonoBehaviour
{
    public GameObject signOutButtonModern; // Кнопка выхода (Modern UI Pack)
    public TextMeshProUGUI errorTextModern; // Текст для отображения ошибок или сообщений (Modern UI Pack)
    private FirebaseAuth auth;

    private void Start()
    {
        Debug.Log("SignOutManager: Start called.");

        // Проверка привязки компонентов
        if (signOutButtonModern == null)
        {
            Debug.LogError("SignOutManager: SignOutButtonModern is not assigned in the Inspector.");
            if (errorTextModern != null)
            {
                errorTextModern.text = "Кнопка выхода не назначена!";
            }
            return;
        }

        if (errorTextModern == null)
        {
            Debug.LogWarning("SignOutManager: ErrorTextModern is not assigned in the Inspector.");
        }
        else if (!errorTextModern.gameObject.activeInHierarchy)
        {
            Debug.LogWarning("SignOutManager: errorTextModern game object is inactive!");
            errorTextModern.gameObject.SetActive(true);
        }

        // Инициализация Firebase
        InitializeFirebase();
    }

    private async void InitializeFirebase()
    {
        try
        {
            var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
            Debug.Log("SignOutManager: Firebase CheckAndFixDependenciesAsync completed.");

            if (dependencyStatus == DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;
                Debug.Log("SignOutManager: Firebase initialized successfully.");

                // Привязка метода к кнопке
                AddModernButtonListener(signOutButtonModern, SignOutUser);
                Debug.Log("SignOutManager: Modern UI Pack Sign out button listener added.");
            }
            else
            {
                errorTextModern.text = $"Ошибка инициализации Firebase: {dependencyStatus}";
                Debug.LogError($"SignOutManager: Firebase initialization failed: {dependencyStatus}");
            }
        }
        catch (System.Exception ex)
        {
            errorTextModern.text = $"Ошибка инициализации Firebase: {ex.Message}";
            Debug.LogError($"SignOutManager: Firebase initialization failed: {ex.Message}");
        }
    }

    private void SignOutUser()
    {
        Debug.Log("SignOutManager: SignOutUser called.");
        if (auth == null)
        {
            Debug.LogError("SignOutManager: FirebaseAuth is null.");
            errorTextModern.text = "Firebase не инициализирован!";
            return;
        }

        try
        {
            // Отключаем взаимодействие с кнопкой
            SetButtonInteractable(signOutButtonModern, false);

            // Выход из аккаунта
            auth.SignOut();
            Debug.Log("SignOutManager: User signed out successfully.");

            // Установка сообщения перед сменой сцены
            errorTextModern.text = "Выход выполнен!";
            Debug.Log("SignOutManager: Sign out message set.");

            // Перенаправление на сцену регистрации
            LoadRegistrationSceneAsync();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"SignOutManager: Sign out failed: {ex.Message}");
            errorTextModern.text = $"Ошибка выхода: {ex.Message}";
            SetButtonInteractable(signOutButtonModern, true);
        }
    }

    private async void LoadRegistrationSceneAsync()
    {
        Debug.Log("SignOutManager: LoadRegistrationSceneAsync started.");

        int sceneIndex = SceneUtility.GetBuildIndexByScenePath("Scenes/RegistrationScene");
        if (sceneIndex == -1)
        {
            Debug.LogError("SignOutManager: RegistrationScene not found in Build Settings!");
            errorTextModern.text = "Ошибка: RegistrationScene не добавлена в Build Settings!";
            SetButtonInteractable(signOutButtonModern, true);
            return;
        }

        if (SceneManager.GetActiveScene().name == "RegistrationScene")
        {
            Debug.Log("SignOutManager: Already in RegistrationScene, no need to load.");
            SetButtonInteractable(signOutButtonModern, true);
            return;
        }

        try
        {
            Debug.Log("SignOutManager: Starting SceneManager.LoadSceneAsync('RegistrationScene').");
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("RegistrationScene");
            if (asyncLoad == null)
            {
                Debug.LogError("SignOutManager: SceneManager.LoadSceneAsync returned null.");
                errorTextModern.text = "Ошибка: Асинхронная загрузка не началась!";
                SetButtonInteractable(signOutButtonModern, true);
                return;
            }

            while (!asyncLoad.isDone)
            {
                Debug.Log($"SignOutManager: LoadSceneAsync progress: {asyncLoad.progress}");
                await Task.Yield();
            }

            Debug.Log("SignOutManager: SceneManager.LoadSceneAsync('RegistrationScene') completed.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"SignOutManager: Failed to load RegistrationScene: {ex.Message}");
            errorTextModern.text = $"Ошибка загрузки сцены: {ex.Message}";
            SetButtonInteractable(signOutButtonModern, true);
        }
    }

    private void AddModernButtonListener(GameObject buttonObject, UnityEngine.Events.UnityAction action)
    {
        var standardButton = buttonObject.GetComponent<Button>();
        if (standardButton != null)
        {
            standardButton.onClick.RemoveAllListeners();
            standardButton.onClick.AddListener(action);
            Debug.Log($"SignOutManager: Found standard Button component on {buttonObject.name}");
            return;
        }

        var buttonComponent = buttonObject.GetComponent<IPointerClickHandler>();
        if (buttonComponent != null)
        {
            EventTrigger trigger = buttonObject.GetComponent<EventTrigger>() ?? buttonObject.AddComponent<EventTrigger>();
            var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            entry.callback.AddListener((eventData) => action.Invoke());
            trigger.triggers.Clear();
            trigger.triggers.Add(entry);
            Debug.Log($"SignOutManager: Added EventTrigger to {buttonObject.name} for Modern UI Pack button.");
        }
        else
        {
            Debug.LogError($"SignOutManager: No clickable component found on {buttonObject.name}. Please check the button setup.");
        }
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
        {
            selectable.interactable = interactable;
        }
        else
        {
            Debug.LogWarning($"SignOutManager: No selectable component found on {buttonObject.name} to set interactable state.");
        }
    }
}