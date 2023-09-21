using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class Card : MonoBehaviour, IPointerClickHandler
{
    [HideInInspector] public Image image;
    [HideInInspector] public SendChoice choiceScript;

    [HideInInspector] public int energyCost;
    public enum CardType { Violent, NonViolent};
    [HideInInspector] public CardType thisType;

    [HideInInspector] public TMP_Text textName;
    [HideInInspector] public TMP_Text textCost;
    [HideInInspector] public TMP_Text textDescr;

    private void Start()
    {
        //if the save manager doesn't already have this card, put it in there
        if (!SaveManager.instance.allCards.Contains(this))
        {
            SaveManager.instance.allCards.Add(this);
            image = GetComponent<Image>();
            choiceScript = GetComponent<SendChoice>();

            textName = this.transform.GetChild(1).GetComponent<TMP_Text>();
            textCost = this.transform.GetChild(2).GetComponent<TMP_Text>();
            textDescr = this.transform.GetChild(3).GetComponent<TMP_Text>();

            Setup();
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            Debug.Log("right clicked");
            RightClick.instance.ChangeCard(this);
        }
    }

    public virtual void Setup()
    {
    }

    public virtual bool CanPlay()
    {
        return false;
    }

    public virtual IEnumerator PlayEffect()
    {
        yield return null;
    }
}
