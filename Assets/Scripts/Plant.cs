using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Plant : MonoBehaviour
{
    Tile[] growTiles;

    void Start()
    {
    }

    void Update()
    {
    }

    public void Grown() {
        Debug.Log($"Grown!!! {transform}");
    }
}
