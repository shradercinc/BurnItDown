using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using MyBox;

public class TitleScreen : MonoBehaviour
{
    [Tooltip("card prefab")][SerializeField] Card cardPrefab;
    [Tooltip("the name of the txt file for the card TSV")][SerializeField] string fileToLoad;

    Button deleteFile;
    GameObject errorText;
    Transform canvas;
    public static TitleScreen instance;

    private void Awake()
    {
        instance = this;
        canvas = GameObject.Find("Canvas").transform;
        deleteFile = GameObject.Find("Delete Deck").GetComponent<Button>();
        deleteFile.onClick.AddListener(Delete);
        errorText = GameObject.Find("Error Text");
    }

    void Start()
    {
        //generate all the cards
        List<CardData> data = CardDataLoader.ReadCardData(fileToLoad);

        for (int i = 0; i < data.Count; i++)
        {
            for (int j = 0; j < data[i].maxInv; j++)
            {
                Card nextCopy = Instantiate(cardPrefab, canvas);
                nextCopy.transform.localPosition = new Vector3(10000, 10000);
                nextCopy.CardSetup(data[i]);
            }
        }
    }

    public bool CompareCreationDates(DateTime saveDataCreation)
    {
        DateTime creationDate = File.GetCreationTime(Path.Combine(Application.dataPath, "Resources/CardData.txt"));
        bool answer = creationDate > saveDataCreation;
        errorText.SetActive(answer);
        return answer;
    }

    public void Delete()
    {
        SaveManager.instance.DeleteData();
    }
}