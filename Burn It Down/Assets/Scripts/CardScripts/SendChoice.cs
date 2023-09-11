using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class SendChoice : MonoBehaviour
{
    [HideInInspector] public Button button;
    [HideInInspector] public Image image;
    [HideInInspector] public Image border;
    [HideInInspector] public Card myCard;
    bool enableBorder;

    void Awake()
    {
        myCard = this.GetComponent<Card>();
        image = this.GetComponent<Image>();
        button = this.GetComponent<Button>();
        border = this.transform.GetChild(0).GetComponent<Image>();

        if (button != null)
            button.onClick.AddListener(SendName);
    }

    private void FixedUpdate()
    {
        if (border != null && enableBorder)
        {
            border.color = new Color(1, 1, 1, ChoiceManager.instance.opacity);
        }
        else if (border != null && !enableBorder)
        {
            border.color = new Color(1, 1, 1, 0);
        }
    }

    public void SendName()
    {
        Debug.Log($"you chose {this.name}");
        if (myCard != null)
            ChoiceManager.instance.ReceiveChoice(myCard);
    }

    public void EnableButton(bool border)
    {
        this.gameObject.SetActive(true);
        enableBorder = border;

        if (button != null)
            button.interactable = true;
    }

    public void DisableButton()
    {
        enableBorder = false;
        if (button != null)
            button.interactable = false;
    }
}
