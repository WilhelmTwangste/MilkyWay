using TMPro;
using UnityEngine;
using Firebase.Database;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using UnityEngine.EventSystems;
using Firebase.Auth;
using UnityEngine.UI;
using Firebase;

public class RegistrationManager : MonoBehaviour
{
    public TMP_InputField signInEmailFieldModern; // Поле email для входа
    public TMP_InputField signInPasswordFieldModern; // Поле пароля для входа
    public TMP_InputField registerEmailFieldModern; // Поле email для регистрации
    public TMP_InputField registerPasswordFieldModern; // Поле пароля для регистрации
    public TMP_InputField ageFieldModern; // Поле возраста для регистрации
    public GameObject registerButtonModern;
    public GameObject signInButtonModern;
    public TextMeshProUGUI signInErrorTextModern; // Поле ошибок для входа
    public TextMeshProUGUI registerErrorTextModern; // Поле ошибок для регистрации
    private FirebaseAuth auth;
    private DatabaseReference dbReference;
    private bool isProcessing = false;

    private void Start()
    {
        if (signInEmailFieldModern == null || signInPasswordFieldModern == null || registerEmailFieldModern == null ||
            registerPasswordFieldModern == null || ageFieldModern == null || registerButtonModern == null ||
            signInButtonModern == null || signInErrorTextModern == null || registerErrorTextModern == null)
        {
            Debug.LogError("RegistrationManager: One or more UI components are not assigned in the Inspector.");
            if (signInErrorTextModern != null)
                signInErrorTextModern.text = "Ошибка: Проверьте привязки в инспекторе!";
            if (registerErrorTextModern != null)
                registerErrorTextModern.text = "Ошибка: Проверьте привязки в инспекторе!";
            return;
        }

        if (!signInErrorTextModern.gameObject.activeInHierarchy)
            signInErrorTextModern.gameObject.SetActive(true);

        if (!registerErrorTextModern.gameObject.activeInHierarchy)
            registerErrorTextModern.gameObject.SetActive(true);

        InitializeFirebase();
    }

    private async void InitializeFirebase()
    {
        try
        {
            var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
            if (dependencyStatus != DependencyStatus.Available)
            {
                Debug.LogError("RegistrationManager: Firebase initialization failed: " + dependencyStatus);
                signInErrorTextModern.text = "Ошибка инициализации Firebase!";
                registerErrorTextModern.text = "Ошибка инициализации Firebase!";
                return;
            }

            auth = FirebaseAuth.DefaultInstance;
            dbReference = FirebaseDatabase.DefaultInstance.RootReference;

            if (auth.CurrentUser != null)
            {
                Debug.Log("RegistrationManager: User already signed in: " + auth.CurrentUser.UserId);
                int age = await GetUserAgeFromFirebase(auth.CurrentUser.UserId);
                await LoadSceneBasedOnAgeAsync(auth.CurrentUser.UserId, age);
                return;
            }

            AddModernButtonListener(registerButtonModern, RegisterUser);
            AddModernButtonListener(signInButtonModern, SignInUser);

            signInErrorTextModern.text = "Введите логин и пароль для входа!";
            registerErrorTextModern.text = "Введите логин, пароль и возраст для регистрации!";
        }
        catch (System.Exception ex)
        {
            Debug.LogError("RegistrationManager: Firebase initialization failed: " + ex.Message);
            signInErrorTextModern.text = "Ошибка инициализации Firebase: " + ex.Message;
            registerErrorTextModern.text = "Ошибка инициализации Firebase: " + ex.Message;
        }
    }

    private void AddModernButtonListener(GameObject buttonObject, UnityEngine.Events.UnityAction action)
    {
        var standardButton = buttonObject.GetComponent<Button>();
        if (standardButton != null)
        {
            standardButton.onClick.RemoveAllListeners();
            standardButton.onClick.AddListener(action);
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
        }
        else
        {
            Debug.LogError("RegistrationManager: No clickable component found on " + buttonObject.name);
        }
    }

    private void RegisterUser()
    {
        if (isProcessing)
            return;

        if (auth == null || dbReference == null)
        {
            registerErrorTextModern.text = "Firebase не инициализирован!";
            Debug.LogError("RegistrationManager: Firebase not initialized.");
            return;
        }

        registerErrorTextModern.text = "";
        isProcessing = true;

        SetButtonInteractable(registerButtonModern, false);
        SetButtonInteractable(signInButtonModern, false);

        string email = registerEmailFieldModern.text;
        string password = registerPasswordFieldModern.text;
        int age;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(ageFieldModern.text))
        {
            registerErrorTextModern.text = "Все поля должны быть заполнены!";
            ResetProcessingState();
            return;
        }

        if (!IsValidEmail(email))
        {
            registerErrorTextModern.text = "Введите корректный email!";
            ResetProcessingState();
            return;
        }

        if (password.Length < 6)
        {
            registerErrorTextModern.text = "Пароль должен содержать минимум 6 символов!";
            ResetProcessingState();
            return;
        }

        if (!int.TryParse(ageFieldModern.text, out age) || age < 0)
        {
            registerErrorTextModern.text = "Введите корректный возраст!";
            ResetProcessingState();
            return;
        }

        RegisterUserAsync(email, password, age);
    }

    private async void RegisterUserAsync(string email, string password, int age)
    {
        try
        {
            var task = await auth.CreateUserWithEmailAndPasswordAsync(email, password);
            FirebaseUser user = task.User;

            await SaveUserDataAsync(user.UserId, age);
            registerErrorTextModern.text = "Регистрация успешна! Переход в игру...";

            await LoadSceneBasedOnAgeAsync(user.UserId, age);
        }
        catch (System.Exception ex)
        {
            registerErrorTextModern.text = ParseFirebaseError(ex, true);
            Debug.LogError("RegistrationManager: Registration failed: " + ex.Message);
            ResetProcessingState();
        }
    }

    private void SignInUser()
    {
        if (isProcessing)
            return;

        if (auth == null)
        {
            signInErrorTextModern.text = "Firebase не инициализирован!";
            Debug.LogError("RegistrationManager: Firebase not initialized.");
            return;
        }

        signInErrorTextModern.text = "";
        isProcessing = true;

        SetButtonInteractable(registerButtonModern, false);
        SetButtonInteractable(signInButtonModern, false);

        string email = signInEmailFieldModern.text;
        string password = signInPasswordFieldModern.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            signInErrorTextModern.text = "Введите email и пароль!";
            ResetProcessingState();
            return;
        }

        if (!IsValidEmail(email))
        {
            signInErrorTextModern.text = "Введите корректный email!";
            ResetProcessingState();
            return;
        }

        if (password.Length < 6)
        {
            signInErrorTextModern.text = "Пароль должен содержать минимум 6 символов!";
            ResetProcessingState();
            return;
        }

        SignInUserAsync(email, password);
    }

    private async void SignInUserAsync(string email, string password)
    {
        try
        {
            var task = await auth.SignInWithEmailAndPasswordAsync(email, password);
            FirebaseUser user = task.User;

            int age = await GetUserAgeFromFirebase(user.UserId);
            signInErrorTextModern.text = "Вход успешен! Переход в игру...";

            await LoadSceneBasedOnAgeAsync(user.UserId, age);
        }
        catch (System.Exception ex)
        {
            signInErrorTextModern.text = "Неверный логин или пароль!";
            Debug.LogError("RegistrationManager: Sign in failed: " + ex.Message);
            ResetProcessingState();
        }
    }

    private void ResetProcessingState()
    {
        isProcessing = false;
        SetButtonInteractable(registerButtonModern, true);
        SetButtonInteractable(signInButtonModern, true);
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
    }

    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrEmpty(email)) return false;
        string emailPattern = @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$";
        return Regex.IsMatch(email, emailPattern);
    }

    private string ParseFirebaseError(System.Exception ex, bool isRegistration)
    {
        string message = ex.Message.ToLower();

        if (isRegistration)
        {
            if (message.Contains("invalid email")) return "Неверный формат email.";
            if (message.Contains("email already in use")) return "Email уже зарегистрирован.";
            if (message.Contains("weak password")) return "Пароль слишком слабый.";
            if (message.Contains("invalid credential")) return "Некорректные данные.";
        }
        else
        {
            if (message.Contains("wrong password")) return "Неверный пароль.";
            if (message.Contains("user not found")) return "Пользователь не найден.";
            if (message.Contains("invalid email")) return "Неверный email.";
            if (message.Contains("too many requests")) return "Слишком много попыток.";
            if (message.Contains("user disabled")) return "Аккаунт заблокирован.";
        }

        return "Произошла ошибка.";
    }

    private async Task SaveUserDataAsync(string userId, int age)
    {
        try
        {
            await dbReference.Child("users").Child(userId).Child("age").SetValueAsync(age);
        }
        catch (System.Exception ex)
        {
            registerErrorTextModern.text = "Ошибка сохранения данных: " + ex.Message;
            Debug.LogError("RegistrationManager: Failed to save user data: " + ex);
            ResetProcessingState();
            throw;
        }
    }

    private async Task<int> GetUserAgeFromFirebase(string userId)
    {
        try
        {
            var snapshot = await dbReference.Child("users").Child(userId).Child("age").GetValueAsync();
            if (snapshot.Exists)
                return int.Parse(snapshot.Value.ToString());
            else
                return -1;
        }
        catch (System.Exception ex)
        {
            Debug.LogError("RegistrationManager: Failed to fetch age: " + ex.Message);
            return -1;
        }
    }

    private async Task LoadSceneBasedOnAgeAsync(string userId, int age)
    {
        if (age < 0 && userId != null)
            age = await GetUserAgeFromFirebase(userId);

        string targetScene = age >= 18 ? "MainScene" : "MainSceneYoung";
        int sceneIndex = SceneUtility.GetBuildIndexByScenePath($"Scenes/{targetScene}");
        if (sceneIndex == -1)
        {
            Debug.LogError("RegistrationManager: " + targetScene + " not found in Build Settings!");
            signInErrorTextModern.text = "Ошибка: Сцена не добавлена в Build Settings!";
            registerErrorTextModern.text = "Ошибка: Сцена не добавлена в Build Settings!";
            ResetProcessingState();
            return;
        }

        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == targetScene)
        {
            ResetProcessingState();
            return;
        }

        try
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetScene);
            if (asyncLoad == null)
            {
                Debug.LogError("RegistrationManager: SceneManager.LoadSceneAsync returned null for " + targetScene);
                signInErrorTextModern.text = "Ошибка: Асинхронная загрузка не началась!";
                registerErrorTextModern.text = "Ошибка: Асинхронная загрузка не началась!";
                ResetProcessingState();
                return;
            }

            while (!asyncLoad.isDone)
                await Task.Yield();

        }
        catch (System.Exception ex)
        {
            Debug.LogError("RegistrationManager: Failed to load " + targetScene + ": " + ex.Message);
            signInErrorTextModern.text = "Ошибка загрузки сцены: " + ex.Message;
            registerErrorTextModern.text = "Ошибка загрузки сцены: " + ex.Message;
            ResetProcessingState();
        }
    }
}