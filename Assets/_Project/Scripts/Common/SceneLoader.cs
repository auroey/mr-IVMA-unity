using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [Header("Startup")]
    [SerializeField] private string initialContentSceneName = "10_Anatomy_Main";
    [SerializeField] private bool setContentSceneActive = true;

    [Header("Behavior")]
    [Tooltip("If true, unloads the previously loaded content scene when loading a new one.")]
    [SerializeField] private bool unloadPreviousContentScene = true;

    private string _currentContentSceneName;
    private bool _isLoading;

    private void Start()
    {
        if (!string.IsNullOrWhiteSpace(initialContentSceneName))
        {
            LoadContentScene(initialContentSceneName);
        }
    }

    public void LoadContentScene(string sceneName)
    {
        if (_isLoading) return;
        if (string.IsNullOrWhiteSpace(sceneName)) return;

        StartCoroutine(LoadContentSceneRoutine(sceneName));
    }

    private IEnumerator LoadContentSceneRoutine(string sceneName)
    {
        _isLoading = true;

        // Already loaded: just optionally activate.
        var maybeLoaded = SceneManager.GetSceneByName(sceneName);
        if (maybeLoaded.IsValid() && maybeLoaded.isLoaded)
        {
            _currentContentSceneName = sceneName;
            if (setContentSceneActive) SceneManager.SetActiveScene(maybeLoaded);
            _isLoading = false;
            yield break;
        }

        // Load new content scene additively.
        var loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        if (loadOp == null)
        {
            Debug.LogError($"SceneLoader: failed to start loading scene '{sceneName}'. " +
                           $"Check it is added to Build Settings and the name matches.");
            _isLoading = false;
            yield break;
        }

        while (!loadOp.isDone) yield return null;

        var loadedScene = SceneManager.GetSceneByName(sceneName);
        if (loadedScene.IsValid() && loadedScene.isLoaded)
        {
            if (setContentSceneActive) SceneManager.SetActiveScene(loadedScene);
        }

        // Optionally unload previous content scene (after new one is ready).
        if (unloadPreviousContentScene &&
            !string.IsNullOrWhiteSpace(_currentContentSceneName) &&
            _currentContentSceneName != sceneName)
        {
            var previous = SceneManager.GetSceneByName(_currentContentSceneName);
            if (previous.IsValid() && previous.isLoaded)
            {
                var unloadOp = SceneManager.UnloadSceneAsync(previous);
                if (unloadOp != null)
                {
                    while (!unloadOp.isDone) yield return null;
                }
            }
        }

        _currentContentSceneName = sceneName;
        _isLoading = false;
    }
}

