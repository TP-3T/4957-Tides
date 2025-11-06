using UnityEngine;
using TTT.GameEvents;
using UnityEngine.SceneManagement;


public class SceneEvents : MonoBehaviour
{
    private string gameScene = "GameScene";
    public void onStateChangedEvent(UnityEngine.Object EventArgs)
    {

        if(EventArgs is StateSystemChangeEventArgs x)
        {
            SceneManager.LoadScene(gameScene);
        }
    }
}
