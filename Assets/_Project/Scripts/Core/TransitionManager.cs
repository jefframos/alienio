using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionManager : MonoBehaviour
{
    [SerializeField]
    private CanvasGroup transitionCanvasGroup;

    [SerializeField]
    private float fadeDuration = 1f;

    // Singleton instance so that we can call TransitionManager from anywhere.
    public static TransitionManager Instance { get; private set; }

    private void Awake()
    {
        // Create a singleton instance and make sure this object is not destroyed on load.
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }

    public async UniTask FadeOutOnStartAsync()
    {
        if (transitionCanvasGroup == null)
            return;

        // Ensure overlay is visible.
        transitionCanvasGroup.gameObject.SetActive(true);
        transitionCanvasGroup.alpha = 1f;

        await UniTask.Delay(1000); // Yield to ensure the overlay is visible before fading out.
        // Fade out.
        await transitionCanvasGroup.DOFade(0f, fadeDuration).AsyncWaitForCompletion();

        // Disable the overlay.
        transitionCanvasGroup.gameObject.SetActive(false);
    }

    public async UniTask TransitionToSceneAsync(string newSceneName)
    {
        if (transitionCanvasGroup == null)
            return;

        // Make sure the overlay is active and fully transparent.
        transitionCanvasGroup.gameObject.SetActive(true);
        transitionCanvasGroup.alpha = 0f;

        // Fade in (cover the current scene).
        await transitionCanvasGroup.DOFade(1f, fadeDuration).AsyncWaitForCompletion();

        // Get the current active scene.
        Scene currentScene = SceneManager.GetActiveScene();

        // Load the new scene additively.
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(newSceneName, LoadSceneMode.Additive);
        while (!loadOp.isDone)
        {
            await UniTask.Yield();
        }

        // Set the new scene as active.
        Scene newScene = SceneManager.GetSceneByName(newSceneName);
        SceneManager.SetActiveScene(newScene);

        // Unload the previous scene.
        AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(currentScene);
        while (!unloadOp.isDone)
        {
            await UniTask.Yield();
        }

        // Fade out the overlay to reveal the new scene.
        await transitionCanvasGroup.DOFade(0f, fadeDuration).AsyncWaitForCompletion();

        // Optionally disable the overlay.
        transitionCanvasGroup.gameObject.SetActive(false);
    }
}
