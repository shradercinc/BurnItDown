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
using MyBox;

[Serializable]
public class SaveData
{
    public List<List<string>> savedDecks;

    public SaveData()
    {
    }
}

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;
    Transform canvas;
    [ReadOnly] public SaveData currentSaveData;
    [Tooltip("Card prefab")][SerializeField] Card cardPrefab;

    [Tooltip("Put names of the TSVs in here")] public List<string> playerDecks;
    public List<List<Card>> characterCards = new List<List<Card>>();

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

        bool creationCheck = true;
        foreach (string fileName in playerDecks)
        {
            if (ES3.FileExists(path) && TitleScreen.instance.PassCheck(fileName, path))
                creationCheck = true;
            else
            {
                creationCheck = false;
                break;
            }
        }

        if (creationCheck)
            currentSaveData = ES3.Load<SaveData>("saveData", path);
        else
            NewFile();
    }

    void NewFile()
    {
        currentSaveData = new SaveData();
        DeleteData();
    }

    public void SaveHand(List<List<Card>> deckToSave)
    {
        //save the new cards for your deck
        List<List<string>> newCards = new List<List<string>>();
        for (int i = 0; i<deckToSave.Count; i++)
        {
            foreach (Card card in deckToSave[i])
                newCards[i].Add(card.name);
        }

        currentSaveData.savedDecks = newCards;
        ES3.Save("saveData", currentSaveData, $"{Application.persistentDataPath}/SaveFile.es3");
    }

    public void DeleteData()
    {
        ES3.DeleteFile($"{Application.persistentDataPath}/SaveFile.es3");
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
        //bring back all other objects
        canvas = GameObject.Find("Canvas").transform;
        RightClick.instance.transform.SetParent(canvas);
        RightClick.instance.transform.localPosition = new Vector3(0, 0);

        FPS.instance.transform.SetParent(canvas);
        FPS.instance.transform.localPosition = new Vector3(-850, -500);

        for (int k = 0; k<playerDecks.Count; k++)
        {
            List<CardData> data = CardDataLoader.ReadCardData(playerDecks[k]);
            for (int i = 0; i < data.Count; i++)
            {
                for (int j = 0; j < data[i].maxInv; j++)
                {
                    Card nextCopy = Instantiate(cardPrefab, canvas);
                    nextCopy.transform.localPosition = new Vector3(10000, 10000);
                    nextCopy.CardSetup(data[i]);
                    characterCards[k].Add(nextCopy);
                }
            }
        }
    }

    public void UnloadObjects()
    {
        Preserve(RightClick.instance.gameObject);
        Preserve(FPS.instance.gameObject);
        characterCards.Clear();
    }

    void Preserve(GameObject next)
    {
        next.transform.SetParent(null);
        DontDestroyOnLoad(next);
    }
}