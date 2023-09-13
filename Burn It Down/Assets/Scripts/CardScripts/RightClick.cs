using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RightClick : MonoBehaviour
{
    public static RightClick instance;
    public Image bigImage;
    public TMP_Text cardText;

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            this.transform.GetChild(0).gameObject.SetActive(false);
    }

    public void ChangeCard(Card newCard)
    {
        this.transform.GetChild(0).gameObject.SetActive(true);
        bigImage = newCard.image;
        cardText = newCard.textBox;
    }
}
