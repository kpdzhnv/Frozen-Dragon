using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "Item", menuName = "Frozen-Dragon/Item", order = 0)]
public class Item : ScriptableObject {
	public TileBase[] tiles;
	public TileBase[] growTiles;
	public GameObject plant;
	public int profit;
}
