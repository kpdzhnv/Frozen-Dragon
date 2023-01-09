using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PathFinder
{
	Tilemap tilemap;

	public PathFinder(Tilemap tilemap)
	{
		this.tilemap = tilemap;
	}

	class Node
	{
		public Vector3Int point;
		public Node parent;

		public List<Vector3Int> Path()
		{
			List<Vector3Int> path = new List<Vector3Int>();
			var node = this;
			while (node != null)
			{
				path.Add(node.point);
				node = node.parent;
			}
			return path;
		}
	}

	bool Visible(Vector3Int p1, Vector3Int p2)
	{
		var t1 = new Vector3(p1.x, p1.y, p1.z);
		var t2 = new Vector3(p2.x, p2.y, p2.z);
		var delta = t2 - t1;
		int count = (int)(delta.magnitude * 2);
		var step = delta / count;
		for (int i = 0; i < count; ++i)
		{
			var c = t1 + step * i;
			var p = new Vector3Int((int)c.x, (int)c.x, (int)c.z);
			if (tilemap.GetTile(p) != null)
				return false;
		}
		return true;
	}

	public List<Vector3Int> Optimize(List<Vector3Int> path)
	{
		List<Vector3Int> optimized = new List<Vector3Int>();
		var last = path[0];
		optimized.Add(last);
		optimized.Add(path[path.Count - 1]);
		return optimized;

		// TODO: fix
		for (int i = 0; i < path.Count; ++i)
		{
			while (i < path.Count - 2 && Visible(last, path[i + 1]))
				++i;
			last = path[i];
			optimized.Add(last);
		}
		return optimized;
	}

	void Load(Stack<Node> stack, Node parent, Vector3Int target)
	{
		var point = parent.point;
		var delta = target - point;
		var sx = System.Math.Sign(delta.x);
		var sy = System.Math.Sign(delta.y);
		sx = sx == 0 ? 1 : sx;
		sy = sy == 0 ? 1 : sy;
		if (System.Math.Abs(delta.x) > System.Math.Abs(delta.y))
		{
			stack.Push(new Node { point = point + new Vector3Int(-sx, 0, 0), parent = parent });
			stack.Push(new Node { point = point + new Vector3Int(0, -sy, 0), parent = parent });
			stack.Push(new Node { point = point + new Vector3Int(0, sy, 0), parent = parent });
			stack.Push(new Node { point = point + new Vector3Int(sx, 0, 0), parent = parent });
		}
		else
		{
			stack.Push(new Node { point = point + new Vector3Int(0, -sy, 0), parent = parent });
			stack.Push(new Node { point = point + new Vector3Int(-sx, 0, 0), parent = parent });
			stack.Push(new Node { point = point + new Vector3Int(sx, 0, 0), parent = parent });
			stack.Push(new Node { point = point + new Vector3Int(0, sy, 0), parent = parent });
		}
	}

	public List<Vector3Int> Find(Vector3Int start, Vector3Int end)
	{

		Stack<Node> stack = new Stack<Node>();
		HashSet<Vector3Int> passed = new HashSet<Vector3Int>();
		Load(stack, new Node { point = end, parent = null }, start);
		for (int i = 0; i < 1000 && stack.Count > 0; ++i)
		{
			var node = stack.Pop();
			if (passed.Contains(node.point)) continue;
			passed.Add(node.point);

			if (node.point == start)
				return node.Path();
			if (tilemap.GetTile(node.point) != null) continue;
			Load(stack, node, start);
		}
		return null;
	}
}
