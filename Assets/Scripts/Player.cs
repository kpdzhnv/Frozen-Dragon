using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;


public class Player : MonoBehaviour
{
	[System.Serializable]
	public struct CountedItem {
		public Item item;
		public int count;
	}

	public struct PlantedPlant {
		public Item item;
		public Plant plant;
	}

	public Grid grid;
	public Tilemap baseTilemap;
	public Dragon dragon;
	public GameObject pointerObject;
	public Tile testtile;
	public CountedItem[] items;

	private bool isPlanting = false;
	private bool isHarvesting = false;
	private int targetItem = 0;
	private int currentItem = 0;

	Camera mainCamera;
	Transform pointer;

	Dictionary<Vector3Int, PlantedPlant> planted = new Dictionary<Vector3Int, PlantedPlant>();

	void Start()
	{
		mainCamera = Camera.main;
		pointer = GameObject.Instantiate(pointerObject).transform;
	}

	void Plant()
	{
		isPlanting = false;
		var gridPos = grid.WorldToCell(dragon.target);
		gridPos.z = 0;
		var cellCenter = grid.CellToWorld(gridPos);
		var tile = baseTilemap.GetTile(gridPos);
		ref var countedItem = ref items[targetItem];
		var item = countedItem.item;
		if (item.tiles.Contains(tile) && countedItem.count > 0) {
			planted.Add(gridPos, new PlantedPlant{
				item = item,
				plant = GameObject.Instantiate(item.plant, cellCenter + new Vector3(0, 0.5f, 0), Quaternion.identity).GetComponent<Plant>()
			});
			var newTile = item.growTiles[Random.Range(0, item.growTiles.Length)];
			baseTilemap.SetTile(gridPos, newTile);
			countedItem.count -= 1;
		}
	}

	void Harvest()
	{
		isHarvesting = false;
		var gridPos = grid.WorldToCell(dragon.target);
		gridPos.z = 0;
		var cellCenter = grid.CellToWorld(gridPos);
		var tile = baseTilemap.GetTile(gridPos);

		if (planted.ContainsKey(gridPos) && planted[gridPos].plant.grown) {
			var plantedItem = planted[gridPos];
			var item = plantedItem.item;
			for (int i = 0; i < items.Length; ++i) 
				if (items[i].item == item) {
					items[i].count += item.profit;
					break;
				}
			Destroy(plantedItem.plant.gameObject);
			planted.Remove(gridPos);
			var newTile = item.tiles[Random.Range(0, item.tiles.Length)];
			baseTilemap.SetTile(gridPos, newTile);
		}
	}

	void Update()
	{
		// change item on scroll
		var step = (int)Input.mouseScrollDelta.y;
		currentItem = (currentItem - step + items.Length) % items.Length;
		Debug.Log($"currentItem: {items[currentItem].item.name}");

		// highlight the selected tile
		var target = mainCamera.ScreenToWorldPoint(Input.mousePosition);
		var gridPos = grid.WorldToCell(target);
		gridPos.z = 0;
		var cellCenter = grid.CellToWorld(gridPos);
		pointer.position = new Vector3(cellCenter.x, cellCenter.y, pointer.position.z);

		// movement on click
		if (Input.GetAxis("Fire1") > 0)
		{
			dragon.target = new Vector2(cellCenter.x, cellCenter.y);
			targetItem = currentItem;
			var tile = baseTilemap.GetTile(gridPos);
			ref var countedItem = ref items[targetItem];
			var item = countedItem.item;

			if (item.tiles.Contains(tile))
				isPlanting = true;
			if (planted.ContainsKey(gridPos))
				isHarvesting = true;
		}

		if (isPlanting && dragon.Done)
			Plant();
		if (isHarvesting && dragon.Done)
			Harvest();
	}
}
