using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RightClick : MonoBehaviour
{
    public static RightClick instance;
    public Image bigImage;

    public TMP_Text cardName;
    public TMP_Text cardCost;
    public TMP_Text cardDescr;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            this.transform.GetChild(0).gameObject.SetActive(false);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            this.transform.GetChild(0).gameObject.SetActive(false);
    }

    public void ChangeCard(Card newCard)
    {
        Debug.Log(newCard.name);
        this.transform.GetChild(0).gameObject.SetActive(true);
        bigImage = newCard.image;

        this.cardName.text = newCard.textName.text;
        this.cardCost.text = newCard.textCost.text;
        this.cardDescr.text = newCard.textDescr.text;
    }
}
