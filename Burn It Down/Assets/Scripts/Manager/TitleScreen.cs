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

    public bool CompareCreationDates(DateTime saveDataCreation)
    {
        DateTime creationDate = File.GetCreationTime(Path.Combine
        (Application.dataPath, $"Resources/{SaveManager.instance.fileToLoad}.txt"));

        bool answer = creationDate > saveDataCreation;
        errorText.SetActive(answer);
        return answer;
    }

    public void Delete()
    {
        SaveManager.instance.DeleteData();
    }
}