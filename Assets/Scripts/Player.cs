using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Dragon))]
public class Player : MonoBehaviour
{
	public struct PlantedPlant
	{
		public Item item;
		public Plant plant;
	}

	[System.Serializable]
	public struct Highlight
	{
		public Tile Walk;
		public Tile Error;
		public Tile Fire;
		public Tile Plant;
		public Tile Harvest;
	}


	class Action
	{
		public enum Type { Queue, Soft, Impossible };
		public Type type { get; set; }
		public Tile highlight { get; set; }
		public Vector3Int point { get; set; }
		public System.Func<bool> exec;
	}


	public Grid grid;
	public Tilemap baseTilemap;
	public Tilemap highlightTilemap;

	public Inventory inventory;
	public Highlight highlight;

	[Range(0, Inventory.maxInventorySize)]
	public int currentSlot = 0;

	private int MaxSlotCount => Inventory.maxInventorySize + 1;

	Camera mainCamera;
	Dragon dragon;

	Dictionary<Vector3Int, PlantedPlant> planted = new Dictionary<Vector3Int, PlantedPlant>();
	HashSet<Vector3Int> activeActions = new HashSet<Vector3Int>();
	LinkedList<Action> actionQueue = new LinkedList<Action>();


	bool revertHighlight = false;
	Vector3Int lasthighlight;

	void Start()
	{
		mainCamera = Camera.main;
		dragon = GetComponent<Dragon>();
	}

	void Update()
	{
		// revert last highlight
		if (revertHighlight)
			highlightTilemap.SetTile(lasthighlight, null);

		// change item on scroll
		var step = -(int)Input.mouseScrollDelta.y;
		currentSlot = (currentSlot + step + MaxSlotCount) % MaxSlotCount;

		// create action
		var target = mainCamera.ScreenToWorldPoint(Input.mousePosition);
		var cellTarget = WorldToCell(target);
		var action = CreateAction(cellTarget);

		// inout
		if (Input.GetMouseButtonDown(0))
			PushAction(action);
		if (Input.GetMouseButtonDown(1))
			ClearActions();

		// highlight current cell
		if (highlightTilemap.GetTile(cellTarget) == null)
		{
			revertHighlight = true;
			lasthighlight = cellTarget;
			highlightTilemap.SetTile(cellTarget, action.highlight);
		}
		else
			revertHighlight = false;

		// update target
		if (actionQueue.Count > 0)
			dragon.target = CellToWorld(actionQueue.First().point);

		// execute action
		if (actionQueue.Count > 0 && dragon.Done)
			ExecuteAction();
	}


	private Vector3 CellToWorld(Vector3Int point)
	{
		var result = grid.CellToWorld(point);
		result.z = 0;
		return result;
	}

	private Vector3Int WorldToCell(Vector3 point)
	{
		point.z = 0;
		return grid.WorldToCell(point);
	}

	void HighlightAction(Action action)
	{
		var tile = highlightTilemap.GetTile(action.point);
		highlightTilemap.SetTile(action.point, action.highlight);
	}

	private void ClearActions()
	{
		foreach (var action in actionQueue)
			DeleteAction(action);
		actionQueue.Clear();
		dragon.target = dragon.transform.position;
	}

	private void DeleteAction(Action action)
	{
		highlightTilemap.SetTile(action.point, null);
		activeActions.Remove(action.point);
	}

	private void PushAction(Action action)
	{
		if (action.type == Action.Type.Impossible) return;
		if (activeActions.Contains(action.point)) return;
		if (actionQueue.Count > 0 && actionQueue.Last().type == Action.Type.Soft)
		{
			DeleteAction(actionQueue.Last());
			actionQueue.RemoveLast();
		}
		highlightTilemap.SetTile(action.point, action.highlight);
		activeActions.Add(action.point);
		actionQueue.AddLast(action);
	}

	private void ExecuteAction()
	{
		var action = actionQueue.First();
		if (action.exec())
		{
			actionQueue.RemoveFirst();
			DeleteAction(action);
		}
		else
			ClearActions();
	}

	private Action CreateAction(Vector3Int point)
	{
		if (activeActions.Contains(point))
			return MakeErrorAction(point);

		var tile = baseTilemap.GetTile(point);
		if (currentSlot == 0)
		{
			if (planted.ContainsKey(point) && planted[point].plant.grown)
				return MakeHarvestAction(point);
		}
		else
		{
			var inventorySlot = currentSlot - 1;
			var item = inventory.CanPlant(inventorySlot, tile);
			if (item != null)
				return MakePlantAction(point, inventorySlot);
		}
		return MakeWalkAction(point);
	}


	Action MakeWalkAction(Vector3Int point)
	{
		return new Action
		{
			type = Action.Type.Soft,
			highlight = highlight.Walk,
			point = point,
			exec = () => { return true; }
		};
	}

	Action MakePlantAction(Vector3Int point, int slotIndex)
	{
		return new Action
		{
			type = Action.Type.Queue,
			highlight = highlight.Plant,
			point = point,
			exec = () =>
			{
				if (planted.ContainsKey(point)) return false;
				var cellCenter = CellToWorld(point);
				var tile = baseTilemap.GetTile(point);
				var item = inventory.TryPlant(slotIndex, tile);
				if (item == null) return false;
				planted.Add(point, new PlantedPlant
				{
					item = item,
					plant = GameObject.Instantiate(item.plant, cellCenter + new Vector3(0, 0.5f, 0), Quaternion.identity).GetComponent<Plant>()
				});
				var newTile = item.growTiles[Random.Range(0, item.growTiles.Length)];
				baseTilemap.SetTile(point, newTile);
				return true;
			}
		};
	}

	Action MakeHarvestAction(Vector3Int point)
	{
		return new Action
		{
			type = Action.Type.Queue,
			highlight = highlight.Harvest,
			point = point,
			exec = () =>
			{
				if (!planted.ContainsKey(point)) return false;
				if (!planted[point].plant.grown) return false;

				var plantedItem = planted[point];
				var item = plantedItem.item;

				if (!inventory.CanHarvest(item)) return false;

				inventory.Harvest(item);
				Destroy(plantedItem.plant.gameObject);
				planted.Remove(point);
				var newTile = item.tiles[Random.Range(0, item.tiles.Length)];
				baseTilemap.SetTile(point, newTile);
				return true;
			}
		};
	}

	Action MakeFireAction(Vector3Int point)
	{
		return new Action
		{
			type = Action.Type.Queue,
			highlight = highlight.Fire,
			point = point,
			exec = () => { throw new System.NotImplementedException(); }
		};
	}

	Action MakeErrorAction(Vector3Int point)
	{
		return new Action
		{
			type = Action.Type.Impossible,
			highlight = highlight.Error,
			point = point,
			exec = () => { throw new System.ApplicationException($"Execute impossible action for {point}"); }
		};
	}

}
