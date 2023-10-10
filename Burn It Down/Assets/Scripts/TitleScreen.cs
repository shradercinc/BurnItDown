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

    private void Awake()
    {
        deleteFile = GameObject.Find("Delete Deck").GetComponent<Button>();
        deleteFile.onClick.AddListener(Delete);
        errorText = GameObject.Find("Error Text");
    }

    private void Start()
    {
        DateTime creationDate = File.GetCreationTime(Path.Combine(Application.dataPath, "Resources/CardData.txt"));
        errorText.SetActive(creationDate.ToFileTimeUtc() > SaveManager.instance.currentSaveData.creationDate.ToFileTimeUtc());
    }

    public void Delete()
    {
        SaveManager.instance.DeleteData();
        errorText.SetActive(false);
    }
}