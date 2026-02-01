using UnityEngine;
using Firebase;
using Firebase.Auth;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class FirstLaunchChecker : MonoBehaviour
{
    private FirebaseAuth auth;

    private async void Start()
    {
        Debug.Log("FirstLaunchChecker: Start called.");
        await CheckFirebaseAsync();
    }

    private async Task CheckFirebaseAsync()
    {
        Debug.Log("FirstLaunchChecker: CheckFirebaseAsync called.");

        try
        {
            var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
            Debug.Log("FirstLaunchChecker: Firebase CheckAndFixDependenciesAsync completed.");

            if (dependencyStatus == DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;
                Debug.Log("FirstLaunchChecker: Firebase initialized successfully.");

                // Проверка текущей сцены
                string currentScene = SceneManager.GetActiveScene().name;
                Debug.Log($"FirstLaunchChecker: Current scene is {currentScene}");

                // Проверка текущего пользователя
                if (auth.CurrentUser != null)
                {
                    Debug.Log($"FirstLaunchChecker: User already signed in: {auth.CurrentUser.UserId}");
                    if (currentScene != "MainScene")
                    {
                        await LoadMainSceneAsync();
                    }
                    else
                    {
                        Debug.Log("FirstLaunchChecker: Already in MainScene, no action needed.");
                    }
                }
                else
                {
                    Debug.Log("FirstLaunchChecker: No user signed in.");
                    if (currentScene != "RegistrationScene")
                    {
                        await LoadRegistrationSceneAsync();
                    }
                    else
                    {
                        Debug.Log("FirstLaunchChecker: Already in RegistrationScene, no action needed.");
                    }
                }
            }
            else
            {
                Debug.LogError($"FirstLaunchChecker: Firebase initialization failed: {dependencyStatus}");
                if (SceneManager.GetActiveScene().name != "RegistrationScene")
                {
                    await LoadRegistrationSceneAsync();
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"FirstLaunchChecker: Firebase initialization failed: {ex.Message}");
            if (SceneManager.GetActiveScene().name != "RegistrationScene")
            {
                await LoadRegistrationSceneAsync();
            }
        }
    }

    private async Task LoadMainSceneAsync()
    {
        Debug.Log("FirstLaunchChecker: LoadMainSceneAsync started.");

        int sceneIndex = SceneUtility.GetBuildIndexByScenePath("Scenes/MainScene");
        if (sceneIndex == -1)
        {
            Debug.LogError("FirstLaunchChecker: MainScene not found in Build Settings!");
            return;
        }

        if (SceneManager.GetActiveScene().name == "MainScene")
        {
            Debug.Log("FirstLaunchChecker: Already in MainScene, no need to load.");
            return;
        }

        try
        {
            Debug.Log("FirstLaunchChecker: Starting SceneManager.LoadSceneAsync('MainScene').");
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("MainScene");
            if (asyncLoad == null)
            {
                Debug.LogError("FirstLaunchChecker: SceneManager.LoadSceneAsync returned null.");
                return;
            }

            while (!asyncLoad.isDone)
            {
                Debug.Log($"FirstLaunchChecker: LoadSceneAsync progress: {asyncLoad.progress}");
                await Task.Yield();
            }

            Debug.Log("FirstLaunchChecker: SceneManager.LoadSceneAsync('MainScene') completed.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"FirstLaunchChecker: Failed to load MainScene: {ex.Message}");
        }
    }

    private async Task LoadRegistrationSceneAsync()
    {
        Debug.Log("FirstLaunchChecker: LoadRegistrationSceneAsync started.");

        int sceneIndex = SceneUtility.GetBuildIndexByScenePath("Scenes/RegistrationScene");
        if (sceneIndex == -1)
        {
            Debug.LogError("FirstLaunchChecker: RegistrationScene not found in Build Settings!");
            return;
        }

        if (SceneManager.GetActiveScene().name == "RegistrationScene")
        {
            Debug.Log("FirstLaunchChecker: Already in RegistrationScene, no need to load.");
            return;
        }

        try
        {
            Debug.Log("FirstLaunchChecker: Starting SceneManager.LoadSceneAsync('RegistrationScene').");
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("RegistrationScene");
            if (asyncLoad == null)
            {
                Debug.LogError("FirstLaunchChecker: SceneManager.LoadSceneAsync returned null.");
                return;
            }

            while (!asyncLoad.isDone)
            {
                Debug.Log($"FirstLaunchChecker: LoadSceneAsync progress: {asyncLoad.progress}");
                await Task.Yield();
            }

            Debug.Log("FirstLaunchChecker: SceneManager.LoadSceneAsync('RegistrationScene') completed.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"FirstLaunchChecker: Failed to load RegistrationScene: {ex.Message}");
        }
    }
}