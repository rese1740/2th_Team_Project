using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapDisabler : MonoBehaviour
{

    void Awake()
    {
        GetComponent<TilemapRenderer>().enabled = false;
    }
    void Update()
    {
        
    }
}
