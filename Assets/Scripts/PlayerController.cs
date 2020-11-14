using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Util;

public class PlayerController : MonoBehaviour
{
	public float jumpPower;
	public GameController gameController;

	private Rigidbody rigidBody;

	//private BoxCollider boxCollider;

	public bool isGrounded = true;

	float top;

	// Start is called before the first frame update
	void Start()
	{
		rigidBody = GetComponent<Rigidbody>();
		var boxCollider = GetComponent<BoxCollider>();
		top = boxCollider.extents.y;
	}

	// Update is called once per frame
	void Update()
	{
		// Jumps when space is pressed
		if (isGrounded && Input.GetAxis("Jump") > 0)
		{
			rigidBody.velocity = new Vector3(0.0f, jumpPower, 0.0f);
		}
	}

	// Detects collision for ground
	private void OnCollisionEnter(Collision col)
	{
		if (col.gameObject.CompareTag("Ground"))
		{
			isGrounded = true;
		}
	}

	private void OnCollisionExit(Collision col)
	{
		isGrounded = false;
	}

	// Detects if player hits obstacle
	void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("Obstacle"))
			gameController.Initialize("Over");
	}

	Vector2 getBottomMiddle()
	{
		return new Vector2(transform.position.x, transform.position.y - top);
	}
}
