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
    public List<string> chosenDeck; //the cards you've chosen for each level

    public SaveData()
    {
    }
}

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;
    public SaveData newSaveData = new SaveData();
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
        if (File.Exists(path)) //if there's a save file, get it
            newSaveData = ES3.Load<SaveData>("saveData");
    }

    public void SaveHand(List<Transform> deckToSave)
    {
        //save the new cards for your deck
        List<string> newCards = new List<string>();
        for (int i = 0; i < deckToSave.Count; i++)
            newCards.Add(deckToSave[i].name);
        newSaveData.chosenDeck = newCards;
        ES3.Save("saveData", newSaveData);
    }

    public void DeleteData()
    {
        string path = $"{Application.persistentDataPath}/SaveFile.es3";
        File.Delete(path);
    }
}