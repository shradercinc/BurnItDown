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

    public bool PassCheck(string deck, string saveDataPath)
    {
        DateTime creationDate = File.GetCreationTime(Path.Combine
        (Application.dataPath, $"Resources/{deck}.txt"));

        bool answer = creationDate < File.GetCreationTime(saveDataPath);
        errorText.SetActive(answer);
        return answer;
    }

    public void Delete()
    {
        SaveManager.instance.DeleteData();
    }
}