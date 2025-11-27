using System.Collections;
using TMPro;
using UnityEngine;

public class LoadingCanvas : MonoBehaviour
{
    private string[] LoadingStates =
    {
        "Loading ",
        "Loading .",
        "Loading . . ",
        "Loading . . . ",
    };

    [SerializeField]
    public TextMeshProUGUI loadingText;

    private Coroutine loadingCoroutine;
    [SerializeField]
    private Material waveMaterial;

    [SerializeField]
    private float waveSpeed = 0.1f;

    /// <summary>
    /// Controls 
    /// </summary>
    private void Update()
    {
        if (waveMaterial != null)
        {
            float offset = Time.time * waveSpeed;
            waveMaterial.SetTextureOffset("_MainTex", new Vector2(offset, 0));
        }
    }

    /// <summary>
    /// Called when the loading canvas prefab is enabled.
    /// </summary>
    public void OnEnable()
    {
        loadingCoroutine = StartCoroutine(UpdateLoadingText());
    }

    /// <summary>
    /// Called when the loading canvas prefab is disabled.
    /// </summary>
    public void OnDisable()
    {
        if (loadingCoroutine != null)
        {
            StopCoroutine(loadingCoroutine);
            loadingCoroutine = null;
        }
    }

    /// <summary>
    /// Cycles through the loading text states.
    /// </summary>
    /// <returns></returns>
    private IEnumerator UpdateLoadingText()
    {
        int index = 0;
        while (true)
        {
            loadingText.text = LoadingStates[index];
            index = (index + 1) % LoadingStates.Length;
            yield return new WaitForSeconds(0.5f);
        }
    }
}
