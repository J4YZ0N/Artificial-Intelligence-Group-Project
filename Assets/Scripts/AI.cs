using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour
{
	// index into the plugin's array of neural networks
	[System.NonSerialized]
	public int mIndex;
	[System.NonSerialized]
	public GameController sGameController;

	bool isGrounded = false;

	GlobalBounds mGlobalBounds;
	Rigidbody mRigidBody;

	ObstacleSpawner sObstacleSpawner;

	void Start()
	{
		mRigidBody = GetComponent<Rigidbody>();
		mGlobalBounds = GetComponent<GlobalBounds>();
	}

	void Update()
	{
		// todo: grab 
	}

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
			sGameController.Initialize("Over");
	}

	Vector2 GetBottomLeft()
	{
		return mGlobalBounds.BottomLeft();
	}
}
