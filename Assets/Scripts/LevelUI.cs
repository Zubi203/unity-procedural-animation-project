using UnityEngine;

public class LevelUI : MonoBehaviour
{
    [SerializeField] private SceneTransition sceneLoader;

    public void BackButtonPressed()
    {
        if (sceneLoader == null) {return;}
        sceneLoader.TransitionToScene("TitleScreen");
    }
}
