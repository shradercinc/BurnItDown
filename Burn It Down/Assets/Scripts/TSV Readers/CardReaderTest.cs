using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardReaderTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        string[,] newGrid = LevelLoader.LoadLevelGrid();
    }
}
