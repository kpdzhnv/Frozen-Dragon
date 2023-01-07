using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "Item", menuName = "Frozen-Dragon/Item", order = 0)]
public class Item : ScriptableObject {
	public Tile[] tiles;
	public Tile[] growTiles;
	public GameObject value;
}
