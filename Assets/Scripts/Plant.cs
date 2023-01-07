using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Plant : MonoBehaviour
{
    [System.NonSerialized]
    public bool grown = false;

    public void Grown() {
        grown = true;
    }
}
