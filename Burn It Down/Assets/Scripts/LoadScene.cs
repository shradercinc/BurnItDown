using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    public void NextScene(int n)
    {
        CustomSceneManager.instance.UnloadObjects();
        SceneManager.LoadScene(n);
    }
}
