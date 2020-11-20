using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour
{
	// index into the plugin's array of neural networks
	[System.NonSerialized]
	public int mIndex;

	int mJumpCount = 2;

	// value updated by nn manager
	[System.NonSerialized]
	public bool mShouldJump = false;
	/*[System.NonSerialized]
	public bool mShouldDoubleJump = false;*/

	const float mJumpPower = 4.5f;

	[System.NonSerialized]
	public GlobalBounds mGlobalBounds;
	Animator mAnimator;
	Rigidbody mRigidBody;

	[System.NonSerialized]
	public float mTimeOfDeath = -1;

	Vector3 mStartPosition;

	void Start()
	{
		mStartPosition = transform.position;
		mAnimator = GetComponent<Animator>();
		mRigidBody = GetComponent<Rigidbody>();
		mGlobalBounds = GetComponent<GlobalBounds>();
	}

	public void Restart()
	{
		transform.position = mStartPosition;
		mTimeOfDeath = -1;
		mShouldJump = false;
		//mShouldDoubleJump = false;
		mJumpCount = 2;
	}

	public bool HasJumped()
	{
		return mJumpCount > 0;
	}

	void Update()
	{
		Jump();
	}

	void Jump()
	{
		if (mJumpCount < 2 && mShouldJump && mRigidBody.velocity.y <= 0)
		{
			if (mJumpCount == 0)
				mAnimator.SetBool("Jumping", true);
			else
				mAnimator.SetBool("Double Jumping", true);
			mRigidBody.velocity = new Vector3(0, mJumpPower, 0);
			++mJumpCount;
		}
	}

	private void OnCollisionEnter(Collision col)
	{
		if (col.gameObject.CompareTag("Ground"))
		{
			mJumpCount = 0;
			mAnimator.SetBool("Jumping", false);
			mAnimator.SetBool("Double Jumping", false);
		}
	}

	// Detects if player hits obstacle
	void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("ShortObstacle") || other.gameObject.CompareTag("TallObstacle"))
			mTimeOfDeath = Time.time;
			mAnimator.SetBool("Death", true);
	}

	public Vector2 BottomLeft()
	{
		return mGlobalBounds.BottomLeft();
	}
}
