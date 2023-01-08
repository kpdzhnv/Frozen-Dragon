using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class Dragon : MonoBehaviour
{
	public Vector2 target;
	public float speed = 1;

	public bool Done => Delta().magnitude < 0.3;

	Rigidbody2D body;
	Animator animator;

	void Start()
	{
		body = GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();
	}

	// distance to target
	private Vector2 Delta() {
		var pos = transform.position;
		return target - new Vector2(pos.x, pos.y);
	}

	private void FixedUpdate()
	{
		if (Done)
		{
			body.velocity = new Vector3(0, 0);
			return;
		}
		var delta = Delta();
		animator.SetBool("Left", delta.x < 0);
		body.velocity = delta.normalized * speed;
	}
}
