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
        TitleScreen.instance.GenerateCards();
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
}