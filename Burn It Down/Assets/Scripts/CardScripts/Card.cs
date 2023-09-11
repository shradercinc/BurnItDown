using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Card : MonoBehaviour
{
    [HideInInspector] public Image image;
    [HideInInspector] public SendChoice choiceScript;
    [HideInInspector] public TMP_Text textBox;

    private void Start()
    {
        //if the save manager doesn't already have this card, put it in there
        if (!SaveManager.instance.allCards.Contains(this))
        {
            SaveManager.instance.allCards.Add(this);
            image = GetComponent<Image>();
            choiceScript = GetComponent<SendChoice>();
            textBox = this.transform.GetChild(1).GetComponent<TMP_Text>();
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
}
