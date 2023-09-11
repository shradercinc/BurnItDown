using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashingCards : MonoBehaviour
{
    public static FlashingCards instance;
    [HideInInspector] public float opacity = 1;
    [HideInInspector] public bool decrease = true;

    private void Awake()
    {
        instance = this;
    }

    private void FixedUpdate()
    {
        if (decrease)
            opacity -= 0.05f;
        else
            opacity += 0.05f;
        if (opacity < 0 || opacity > 1)
            decrease = !decrease;
    }
}
