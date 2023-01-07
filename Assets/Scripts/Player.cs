using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Player : MonoBehaviour
{
	public Grid grid;
	public Tilemap baseTilemap;
	public Dragon dragon;
	public GameObject pointerObject;
	public Tile testtile;

	public Item[] items;

	private bool toDo = false;
	private int targetItem = 0;
	private int currentItem = 0;

	Camera mainCamera;
	Transform pointer;

	Dictionary<Vector3Int, GameObject> map = new Dictionary<Vector3Int, GameObject>();

	void Start()
	{
		mainCamera = Camera.main;
		pointer = GameObject.Instantiate(pointerObject).transform;
	}

	void Do()
	{
		toDo = false;
		var gridPos = grid.WorldToCell(dragon.target);
		gridPos.z = 0;
		var cellCenter = grid.CellToWorld(gridPos);
		var tile = baseTilemap.GetTile(gridPos);
		var item = items[targetItem];
		if (item.tiles.Contains(tile)) {
			GameObject.Instantiate(item.plant, cellCenter + new Vector3(0, 0.5f, 0), Quaternion.identity);
			var newTile = item.growTiles[Random.Range(0, item.growTiles.Length)];
			baseTilemap.SetTile(gridPos, newTile);
		}
	}

	void Update()
	{
		var step = (int)Input.mouseScrollDelta.y;
		currentItem = (currentItem + step + items.Length) % items.Length;
		Debug.Log($"currentItem: {items[currentItem]}");

		var target = mainCamera.ScreenToWorldPoint(Input.mousePosition);
		var gridPos = grid.WorldToCell(target);
		gridPos.z = 0;
		var cellCenter = grid.CellToWorld(gridPos);
		pointer.position = new Vector3(cellCenter.x, cellCenter.y, pointer.position.z);
		if (Input.GetAxis("Fire1") > 0)
		{
			dragon.target = new Vector2(cellCenter.x, cellCenter.y);
			targetItem = currentItem;
			toDo = true;
		}
		if (toDo && dragon.Done)
			Do();
	}
}
