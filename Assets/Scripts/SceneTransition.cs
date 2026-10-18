using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] private Animator animator;

    public void TransitionToScene(string sceneName)
    {
        if (animator == null || !animator.gameObject.activeSelf){return;}

        StartCoroutine(LoadNewScene(sceneName));
    }

    IEnumerator LoadNewScene(string sceneName)
    {
        animator.SetTrigger("Transition");

        yield return new WaitForSeconds(1.0f);

        SceneManager.LoadScene(sceneName);
    }
}
