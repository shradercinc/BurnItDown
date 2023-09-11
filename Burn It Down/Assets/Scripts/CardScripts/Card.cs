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

    private void Awake()
    {
        image = GetComponent<Image>();
        choiceScript = GetComponent<SendChoice>();
        textBox = this.transform.GetChild(1).GetComponent<TMP_Text>();
    }
}
