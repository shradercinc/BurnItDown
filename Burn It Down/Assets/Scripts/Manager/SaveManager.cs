using System;
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

[Serializable]
public class SaveData
{
    public List<string> chosenDeck; //the cards you've chosen for each level

    public SaveData()
    {
    }
}

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;
    public SaveData currentSaveData;
    [Tooltip("card prefab")][SerializeField] Card cardPrefab;
    [Tooltip("the name of the txt file for the card TSV")] public string fileToLoad;
    public List<Card> allCards = new List<Card>(); //keeps track of all cards in the game

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
        if (ES3.FileExists(path) &&
        !TitleScreen.instance.CompareCreationDates(File.GetCreationTime(path)))
        {
            currentSaveData = ES3.Load<SaveData>("saveData", path);
        }
        else
        {
            NewFile();
        }
    }

    void NewFile()
    {
        currentSaveData = new SaveData();
        DeleteData();
    }

    public void SaveHand(List<Card> deckToSave)
    {
        //save the new cards for your deck
        List<string> newCards = new List<string>();
        for (int i = 0; i < deckToSave.Count; i++)
            newCards.Add(deckToSave[i].name);

        string path = $"{Application.persistentDataPath}/SaveFile.es3";
        currentSaveData.chosenDeck = newCards;
        ES3.Save("saveData", currentSaveData, path);
    }

    public void DeleteData()
    {
        Debug.Log("deleting save file");
        string path = $"{Application.persistentDataPath}/SaveFile.es3";
        ES3.DeleteFile(path);
        currentSaveData = new SaveData();
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
        RightClick.instance.transform.localPosition = new Vector3(0, 0);

        FPS.instance.transform.SetParent(canvas);
        FPS.instance.transform.localPosition = new Vector3(-850, -500);

        List<CardData> data = CardDataLoader.ReadCardData(fileToLoad);
        for (int i = 0; i < data.Count; i++)
        {
            for (int j = 0; j < data[i].maxInv; j++)
            {
                Card nextCopy = Instantiate(cardPrefab, canvas);
                nextCopy.transform.localPosition = new Vector3(10000, 10000);
                nextCopy.CardSetup(data[i]);
                allCards.Add(nextCopy);
            }
        }
    }

    public void UnloadObjects()
    {
        Preserve(RightClick.instance.gameObject);
        Preserve(FPS.instance.gameObject);
        allCards.Clear();
    }

    void Preserve(GameObject next)
    {
        next.transform.SetParent(null);
        DontDestroyOnLoad(next);
    }
}