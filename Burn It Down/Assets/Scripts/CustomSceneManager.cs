using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CustomSceneManager : MonoBehaviour
{
    public static CustomSceneManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Transform canvas = GameObject.Find("Canvas").transform;

        //bring back all other objects
        RightClick.instance.transform.SetParent(canvas);
        FPS.instance.transform.localPosition = new Vector3(0, 0);

        FPS.instance.transform.SetParent(canvas);
        FPS.instance.transform.localPosition = new Vector3(0, 500);
    }

    public void UnloadObjects()
    {
        //DontDestroyOnLoad doesn't work if an object is a child of something else,
        //so I have to set everything's parent to null before giving it DontDestroyOnLoad
        for (int i = 0; i < SaveManager.instance.allCards.Count; i++)
            Preserve(SaveManager.instance.allCards[i].gameObject);

        Preserve(RightClick.instance.gameObject);
        Preserve(FPS.instance.gameObject);
    }

    void Preserve(GameObject next)
    {
        next.transform.SetParent(null);
        DontDestroyOnLoad(next);
    }
}