using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUi : MonoBehaviour
{
	public RectTransform inventoryTransform;
	public Player player;

	private List<Transform> items = new List<Transform>();

	void Start()
	{
		var count = inventoryTransform.childCount;
		if (count != Inventory.maxInventorySize + 1)
			throw new System.ArgumentException("inventoryTransform.childCount != Inventory.maxInventorySize + 1");

		for (int i = 0; i < count; ++i)
			items.Add(inventoryTransform.GetChild(i));
	}

	void Update()
	{
		for (int i = 0; i < items.Count; ++i)
		{
			var backgroundImage = items[i].GetComponent<Image>();
			if (i == player.currentSlot)
				backgroundImage.color = new Color(1, 1, 1);
			else
				backgroundImage.color = new Color(1, 1, 1, 0.5f);

			if (i == 0) continue;

			var textObject = items[i].GetChild(0);
			var morkovka = items[i].GetChild(1);
			var image = morkovka.GetComponent<Image>();
			var text = textObject.GetComponent<TextMeshProUGUI>();

			var slot = i - 1;
			if (slot >= 0 && slot < player.inventory.Length)
			{
				var item = player.inventory.items[slot];
				image.sprite = item.item.icon;
				text.text = item.count.ToString();
			} else
				image.color = new Color(0, 0, 0, 0);
		}
	}
}
