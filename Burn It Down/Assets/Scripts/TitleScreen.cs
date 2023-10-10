using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class TitleScreen : MonoBehaviour
{
    Button deleteFile;
    GameObject errorText;
    public static TitleScreen instance;

    private void Awake()
    {
        instance = this;
        deleteFile = GameObject.Find("Delete Deck").GetComponent<Button>();
        deleteFile.onClick.AddListener(Delete);
        errorText = GameObject.Find("Error Text");
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