using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour
{
	// index into the plugin's array of neural networks
	[System.NonSerialized]
	public int mIndex;
	// shared game controller to change game mode
	// todo: should be in neural network manager instead
	//		to restart only when all AI have died
	/*[System.NonSerialized]
	public GameController sGameController;*/

	int mJumpCount = 2;

	// value updated by nn manager
	[System.NonSerialized]
	public bool mShouldJump = false;
	[System.NonSerialized]
	public bool mShouldDoubleJump = false;

	const float mJumpPower = 4.5f;

	[System.NonSerialized]
	public GlobalBounds mGlobalBounds;
	Rigidbody mRigidBody;

	[System.NonSerialized]
	public float mTimeOfDeath = -1;

	Vector3 mStartPosition;

	//ObstacleSpawner sObstacleSpawner;
	
	// keep a record of actions in case they lead to AI's
	// death. hopefully can be used to retrain AI.
	/*const int mRecordCount = 4; // must be a power of 2
	[System.NonSerialized]
	public bool[] mActionRecord = new bool[mRecordCount];
	int mNextRecordIndex = 0;*/

	void Start()
	{
		mStartPosition = transform.position;
		mRigidBody = GetComponent<Rigidbody>();
		mGlobalBounds = GetComponent<GlobalBounds>();
	}

	public void Restart()
	{
		transform.position = mStartPosition;
		mTimeOfDeath = -1;
		mShouldJump = false;
		mShouldDoubleJump = false;
		mJumpCount = 2;
	}

	public bool HasJumped()
	{
		return mJumpCount > 0;
	}

	void Update()
	{
		// todo: grab prediction? or do that in nn manager?
		//	probably should do it in nn manager since inputs
		//	should be the same for every AI (or should they?)

		Jump();

		// add result to the record
		/*mActionRecord[mNextRecordIndex] = mShouldJump;
		// wraps index if mRecordCount is a power of 2
		mNextRecordIndex = (mNextRecordIndex + 1) % mRecordCount;*/
	}

	void Jump()
	{
		var jump = mJumpCount < 2 && mShouldJump;
		var doubleJump = mJumpCount == 1 && mShouldDoubleJump;
		if (jump && mRigidBody.velocity.y <= 0)
		{
			mRigidBody.velocity = new Vector3(0, mJumpPower, 0);
			++mJumpCount;
		}
	}

	void Duck()
	{
		// if grounded
		if (mJumpCount == 0)
		{
			transform.localScale = new Vector3(
				transform.localScale.x,
				transform.localScale.y / 2,
				transform.localScale.z);
			transform.position = new Vector3(
				transform.position.x,
				transform.position.y / 2,
				transform.position.z);
		}
	}

	private void OnCollisionEnter(Collision col)
	{
		if (col.gameObject.CompareTag("Ground"))
			mJumpCount = 0;
	}

	/*private void OnCollisionExit(Collision col)
	{
		if (col.gameObject.CompareTag("Ground"))
			mIsGrounded = false;
	}*/

	// Detects if player hits obstacle
	void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("Obstacle"))
			mTimeOfDeath = Time.time;
	}

	public Vector2 BottomLeft()
	{
		return mGlobalBounds.BottomLeft();
	}
}
