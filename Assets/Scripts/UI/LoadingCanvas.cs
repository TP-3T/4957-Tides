using UnityEngine;
using UnityEngine.UI;

public class LoadingCanvas : MonoBehaviour
{
    public GameObject LoadingScreen;
    public Slider ProgressBar;

    /// <summary>
    /// Starts loading the main game asynchronously.
    /// </summary>
    public void LoadingMainGame()
    {
        StartCoroutine(LoadingMainGameAsync());
    }

    /// <summary>
    /// Coroutine to handle the loading process.
    /// Currently just a placeholder, CoreLogic 
    /// can add whatever elements that will be 
    /// tracked for progress during loading.
    /// </summary>
    /// <returns></returns>
    IEnumerator LoadingMainGameAsync()
    {
        ProgressBar.value = 0f;

        // Simulate loading process
        float loadProgress = 0f;
        while (loadProgress < 1f)
        {
            loadProgress += 0.1f;
            ProgressBar.value = loadProgress;
            yield return new WaitForSeconds(0.2f);
        }
    }
}
