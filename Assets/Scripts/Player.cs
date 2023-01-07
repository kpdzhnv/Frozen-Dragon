using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
	public Grid grid;
	public Dragon dragon;
	public Transform pointer;

	Camera mainCamera;

	void Start()
	{
		mainCamera = Camera.main;
	}

	void Update()
	{
		var target = mainCamera.ScreenToWorldPoint(Input.mousePosition);
		var gridPos = grid.WorldToCell(target);
		var cellCenter = grid.CellToWorld(gridPos);
		pointer.position = new Vector3(cellCenter.x, cellCenter.y, pointer.position.z);
		if (Input.GetAxis("Fire1") <= 0)
			return;
		dragon.target = new Vector2(cellCenter.x, cellCenter.y);
	}
}
