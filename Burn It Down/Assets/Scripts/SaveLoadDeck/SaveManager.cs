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
    public List<Card> savedDeck;
    public List<Card> unlockedCards;

    public SaveData()
    {
    }
}

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;
    public SaveData newSaveData = new SaveData();
    public List<Card> allCards = new List<Card>();

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

    private void Start()
    {
        string path = $"{Application.persistentDataPath}/SaveFile.es3";
        if (File.Exists(path))
            newSaveData = ES3.Load<SaveData>("saveData");

        for (int i = 0; i < newSaveData.unlockedCards.Count; i++)
            newSaveData.unlockedCards[i].gameObject.SetActive(true);
    }

    public void SaveDeck(List<Card> deckToSave)
    {
        List<Card> newCards = deckToSave;
        newSaveData.savedDeck = newCards;
        ES3.Save("saveData", newSaveData);
    }
}