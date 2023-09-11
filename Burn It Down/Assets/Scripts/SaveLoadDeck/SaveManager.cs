using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using System.IO;

[System.Serializable]
public class SaveData
{
    public List<Card> yourDeck;

    public SaveData()
    {
    }
}

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void Start()
    {
        string path = $"{Application.persistentDataPath}/SaveFile.es3";
        if (File.Exists(path))
        {
            SaveData readData = ES3.Load<SaveData>("saveData");
            StartCoroutine(Load(readData));
        }
    }

    public void Save()
    {

    }

    public IEnumerator Load(SaveData saveData)
    {
        yield return null;
    }
}