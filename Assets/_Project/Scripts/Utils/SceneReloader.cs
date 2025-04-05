using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneReloader
{
    public static void ReloadCurrentScene()
    {
        // Get the currently active scene.
        Scene activeScene = SceneManager.GetActiveScene();
        // Reload the scene by its name.
        SceneManager.LoadScene(activeScene.name);
    }
}
