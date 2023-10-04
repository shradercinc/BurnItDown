using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using System;
using System.Reflection;

public class CardReaderTest : MonoBehaviour
{
    [SerializeField] Card cardPrefab;
    Transform canvas;

    private void Awake()
    {
        canvas = GameObject.Find("Canvas").transform;
        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        List<CardData> data = CardDataLoader.ReadCardData();

        for (int i = 0; i < data.Count; i++)
        {
            for (int j = 0; j < data[i].maxInv; j++)
            {
                Card nextCopy = Instantiate(cardPrefab, canvas);
                nextCopy.transform.localPosition = new Vector3(10000, 10000);

                nextCopy.name = data[i].name;
                nextCopy.textName.text = data[i].name;
                nextCopy.textDescr.text = data[i].desc;

                nextCopy.typeOne = ConvertToType(data[i].cat1);
                nextCopy.typeTwo = ConvertToType(data[i].cat2);

                nextCopy.energyCost = data[i].epCost;
                nextCopy.textCost.text = $"{data[i].epCost} Energy";
                nextCopy.violent = data[i].isViolent;

                nextCopy.changeInHP = data[i].chHP;
                nextCopy.changeInMP = data[i].chMP;
                nextCopy.changeInEP = data[i].chEP;
                nextCopy.changeInDraw = data[i].draw;

                nextCopy.stunDuration = data[i].stun;
                nextCopy.range = data[i].range;
                nextCopy.areaOfEffect = data[i].aoe;
                nextCopy.delay = data[i].delay;
                nextCopy.changeInWall = data[i].wHP;

                nextCopy.burnDuration = data[i].burn;
                nextCopy.distractionIntensity = data[i].intn;

                nextCopy.selectCondition = ConvertToCondition(data[i].select);
                AddMethodsToList(nextCopy, data[i].action, nextCopy.effectsInorder);
                nextCopy.nextTurnAbility = data[i].nextAct;
            }
        }
    }

    Card.CardType ConvertToType(string type)
    {
        return type switch
        {
            "draw" => Card.CardType.Draw,
            "atk" => Card.CardType.Attack,
            "dist" => Card.CardType.Distraction,
            "eng" => Card.CardType.Energy,
            "mvmt" => Card.CardType.Movement,
            "misc" => Card.CardType.Misc,
            _ => Card.CardType.None,
        };
    }

    Card.CanPlayCondition ConvertToCondition(string condition)
    {
        return condition switch
        {
            "isGuard" => Card.CanPlayCondition.Guard,
            "isWall" => Card.CanPlayCondition.Wall,
            "isOccupied" => Card.CanPlayCondition.Occupied,
            _ => Card.CanPlayCondition.None,
        };
    }

    void AddMethodsToList(Card nextCopy, string divide, List<IEnumerator> list)
    {
        StringAndMethod dic = new StringAndMethod(nextCopy);

        string[] methodsInStrings = divide.Split('/');
        for (int k = 0; k < methodsInStrings.Length; k++)
        {
            if (dic.dictionary.TryGetValue(methodsInStrings[k], out IEnumerator method))
                list.Add(method);
            else
                Debug.LogError($"\"{methodsInStrings[k]}\" isn't a method");
        }
    }
}
