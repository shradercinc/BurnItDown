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
    public List<Card> chosenDeck; //the cards you've chosen for each level
    public List<Card> unlockedCards; //cards that you unlock during the game
    public List<Card> burnedCards; //cards that have been burned away

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

        for (int i = 0; i < newSaveData.unlockedCards.Count; i++) //enable all unlocked cards
            newSaveData.unlockedCards[i].gameObject.SetActive(true);

        for (int i = 0; i < newSaveData.burnedCards.Count; i++) //disable all burnt cards
            newSaveData.burnedCards[i].gameObject.SetActive(false);
    }

    public void SaveHand(List<Card> deckToSave)
    {
        //save the new cards for your deck
        List<Card> newCards = deckToSave;
        newSaveData.chosenDeck = newCards;
        ES3.Save("saveData", newSaveData);
    }
}