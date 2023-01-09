using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class Dragon : MonoBehaviour
{
	public float speed = 1;

	public Animator fire;

	List<Vector2> path = null;

	float stepDelta;

	Rigidbody2D body;
	Animator animator;
	int pathPoint = 0;

	public bool Done => path == null || pathPoint >= path.Count;

	void Start()
	{
		body = GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();
	}

	// distance to target
	private Vector2 Delta(Vector2 target)
	{
		var pos = transform.position;
		return target - new Vector2(pos.x, pos.y);
	}

	public void UpdatePath(List<Vector2> path, float delta = 0.1f)
	{
		pathPoint = 0;
		if (path == null || path.Count == 0)
			this.path = null;
		this.path = path;
		this.stepDelta = delta;
	}

	public void Fire()
	{
		fire.SetTrigger("Fire");
	}

	private void FixedUpdate()
	{
		if (Done)
		{
			body.velocity = new Vector3(0, 0);
			return;
		}

		Vector2 delta = Delta(path[pathPoint]);
		while (delta.magnitude < stepDelta)
		{
			pathPoint += 1;
			if (Done) return;
			delta = Delta(path[pathPoint]);
		}
		animator.SetBool("Left", delta.x < 0);
		body.velocity = delta.normalized * speed;
	}
}
