using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    public void NextScene(int n)
    {
        for (int i = 0; i < SaveManager.instance.allCards.Count; i++)
        {
            //DontDestroyOnLoad doesn't work if a card is a child of something else,
            //so I have to set the card's parent to null before giving it DontDestroyOnLoad
            SaveManager.instance.allCards[i].transform.SetParent(null);
            DontDestroyOnLoad(SaveManager.instance.allCards[i].gameObject);
        }

        SceneManager.LoadScene(n);
    }
}
