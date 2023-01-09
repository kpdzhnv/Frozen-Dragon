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
		public float delta = 0.1f;
		public Type type;
		public Tile highlight;
		public Vector3Int point;
		public System.Func<bool> exec;
	}

	[System.Serializable]
	public struct PreplantedPlant
	{
		public Vector3Int point;
		public Item item;
	}

	public PreplantedPlant[] preplantedPlant;

	public AudioSource pum;

	public Grid grid;
	public Tilemap baseTilemap;
	public Tilemap highlightTilemap;
	public Tilemap obastaclTilemap;

	public TileBase snowTile;

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

	PathFinder pathfinder;

	bool revertHighlight = false;
	Vector3Int lasthighlight;

	public UIManagerScript uims;
	void Start()
	{
		mainCamera = Camera.main;
		dragon = GetComponent<Dragon>();
		pathfinder = new PathFinder(obastaclTilemap);
		foreach (var p in preplantedPlant)
			PlantPlant(p.point, p.item);
	}


	void Update()
	{
		if (uims.dialogueStarted)
			return;
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
		if (actionQueue.Count > 0 && dragon.Done)
		{
			ExecuteAction();
			if (actionQueue.Count > 0)
				UpdatePath();
		}
	}

	private void UpdatePath()
	{
		var action = actionQueue.First();
		var path = FindPath(action.point);
		if (path == null)
			ClearActions();
		else
			dragon.UpdatePath(path, action.delta);
	}

	private List<Vector2> FindPath(Vector3Int target)
	{
		var rawPath = pathfinder.Find(WorldToCell(dragon.transform.position), target);
		if (rawPath == null) return null;
		rawPath = pathfinder.Optimize(rawPath);
		return rawPath.Select(v =>
		{
			var t = CellToWorld(v);
			Debug.DrawLine(t, t + new Vector3(0, 0.1f, 0), Color.blue, 10);
			return new Vector2(t.x, t.y);
		}).ToList();
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
		dragon.UpdatePath(null);
	}

	private void DeleteAction(Action action)
	{
		highlightTilemap.SetTile(action.point, null);
		activeActions.Remove(action.point);
	}

	private void PushAction(Action action)
	{
		pum.Play();
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

		if (actionQueue.Count == 1)
			UpdatePath();
	}

	void PlantPlant(Vector3Int point, Item item)
	{
		var cellCenter = CellToWorld(point);
		planted.Add(point, new PlantedPlant
		{
			item = item,
			plant = GameObject.Instantiate(item.plant, cellCenter + new Vector3(0, 0.5f, 0), Quaternion.identity).GetComponent<Plant>()
		});
		var newTile = item.growTiles[Random.Range(0, item.growTiles.Length)];
		baseTilemap.SetTile(point, newTile);
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
		if (pathfinder.Find(WorldToCell(dragon.transform.position), point) == null)
			return MakeErrorAction(point);

		var obstaclTile = obastaclTilemap.GetTile(point);
		if (obstaclTile == snowTile)
		{
			if (currentSlot <= 0 || (currentSlot - 1) >= inventory.Length)
				return MakeErrorAction(point);

			ref var slot = ref inventory.items[currentSlot - 1];
			if (slot.count < slot.item.fireCost)
				return MakeErrorAction(point);

			return MakeFireAction(point, currentSlot - 1);
		}

		if (obstaclTile != null)
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
				var tile = baseTilemap.GetTile(point);
				var item = inventory.TryPlant(slotIndex, tile);
				if (item == null) return false;
				PlantPlant(point, item);
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

	Action MakeFireAction(Vector3Int point, int slot)
	{
		return new Action
		{
			type = Action.Type.Queue,
			delta = 1f,
			highlight = highlight.Fire,
			point = point,
			exec = () =>
			{
				if (inventory.Length <= slot) return false;
				ref var item = ref inventory.items[slot];
				if (item.count < item.item.fireCost) return false;
				var tile = obastaclTilemap.GetTile(point);
				if (tile != snowTile) return false;

				obastaclTilemap.SetTile(point, null);
				item.count -= item.item.fireCost;
				dragon.Fire();
				return true;
			}
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

#if UNITY_EDITOR
	private void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		foreach (var p in preplantedPlant)
			Gizmos.DrawSphere(CellToWorld(p.point) + new Vector3(0, 0.25f, 0), 0.1f);
	}
#endif
}
