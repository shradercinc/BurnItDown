using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleScreen : MonoBehaviour
{
    Button deleteFile;

    private void Awake()
    {
        deleteFile = GameObject.Find("Delete Deck").GetComponent<Button>();
        deleteFile.onClick.AddListener(Delete);
    }

    public void Delete()
    {
        SaveManager.instance.DeleteData();
    }
}
