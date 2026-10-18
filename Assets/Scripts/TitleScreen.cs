using UnityEngine;

public class TitleScreen : MonoBehaviour
{
    [SerializeField] private SceneTransition sceneLoader;

    public void PlayButtonPressed()
    {
        if (sceneLoader == null) {return;}

        sceneLoader.TransitionToScene("Main");
    }

    public void ExitButtonPressed()
    {
        Application.Quit();
    }
}
