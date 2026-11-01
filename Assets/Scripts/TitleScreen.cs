using UnityEngine;

public class TitleScreen : MonoBehaviour
{
    [SerializeField] private SceneTransition sceneLoader;

    public void SpiderButtonPressed()
    {
        if (sceneLoader == null) {return;}

        sceneLoader.TransitionToScene("Main");
    }

    public void SnakeButtonPressed()
    {
        if (sceneLoader == null) {return;}

        sceneLoader.TransitionToScene("Snake");
    }

    public void HumanButtonPressed()
    {
        if (sceneLoader == null) {return;}

        sceneLoader.TransitionToScene("Humanoid");
    }

    public void ExitButtonPressed()
    {
        Application.Quit();
    }
}
