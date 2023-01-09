using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class Inventory
{
	public const int maxItemInSlot = 9;
	public const int maxInventorySize = 9;
	[System.Serializable]
	public struct Slot
	{
		public Item item;
		[Range(1, maxItemInSlot)]
		public int count;
	}
	public Slot[] items;

	public int Length => items.Length;

	public bool CanHarvest(Item item)
	{
		if (items.Length < maxInventorySize)
			return true;
		for (int i = 0; i < items.Length; ++i)
		{
			if (items[i].item == item && items[i].count < maxItemInSlot)
				return true;
		}
		return false;
	}

	private void Add(Slot slot)
	{
		var newItems = new Slot[Length + 1];
		for (int i = 0; i < Length; ++i)
			newItems[i] = items[i];
		newItems[Length] = slot;
		items = newItems;
	}

	private int IncSlot(int index, int count)
	{
		ref var slot = ref items[index];
		var lastSlotCount = slot.count;
		slot.count = Mathf.Min(slot.count + count, maxItemInSlot);
		count -= slot.count - lastSlotCount;
		return count;
	}

	public void Harvest(Item item)
	{
		int count = item.profit;
		for (int i = 0; i < items.Length && count > 0; ++i)
			if (item == items[i].item || items[i].count <= 0)
			{
				items[i].item = item;
				count = IncSlot(i, count);
			}
		while (count > 0 && items.Length < maxInventorySize)
		{
			Add(new Slot { item = item, count = 0 });
			count = IncSlot(Length - 1, count);
		}
	}

	public Item CanPlant(int slotIndex, TileBase tile)
	{
		if (slotIndex < 0 || slotIndex >= items.Length) return null;
		if (!items[slotIndex].item.tiles.Contains(tile)) return null;
		return items[slotIndex].item;
	}

	public Item TryPlant(int slotIndex, TileBase tile)
	{
		Item item = CanPlant(slotIndex, tile);
		if (item == null) return null;
		if (items[slotIndex].count <= 0) return null;
		items[slotIndex].count -= 1;
		return items[slotIndex].item;
	}
}