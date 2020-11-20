using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeuralNetworkManager : MonoBehaviour
{
	public List<GameObject> mPlayerPrefabs;

	// position for players to spawn at
	public Vector3 mSpawnPosition = new Vector3(-3.0f, 0, 0);

	// amount of neural networks to create
	const int mCount = 200;
	int mActiveCount = mCount;

	// list of AI players
	List<AI> mAIs = new List<AI>();

	// constant minimum x bound of player in global space
	// used when trying to find closest obstacle to the right
	public float mPlayerLeft;

	GameController sGameController;
	ObstacleSpawner sObstacleSpawner;

	float mHistoricallyBestFitness = -Mathf.Infinity;

	float mCurrentBestFitness;
	int mCurrentBestIndex;

	float mStartTime;

	void Start()
	{
		mPlayerLeft = FindPlayerLeft();

		sGameController = GetComponent<GameController>();
		sObstacleSpawner = GetComponent<ObstacleSpawner>();

		// add neural networks
		NeuralNetwork.addNetworks(mCount);

		// add an AI for each neural network
		for (int i = 0; i < mCount; ++i)
		{
			var r = Random.Range(0, mPlayerPrefabs.Count);
			var instance = Instantiate(
				mPlayerPrefabs[r], mSpawnPosition, Quaternion.Euler(0,90,0));
			instance.GetOrAddComponent<BoxCollider>();
			instance.GetOrAddComponent<GlobalBounds>();

			// give AI an index to a neural network
			var ai = instance.GetOrAddComponent<AI>();
			ai.mIndex = i;
			mAIs.Add(ai);
		}

		mStartTime = Time.time;
		mCurrentBestFitness = -Mathf.Infinity;
		mCurrentBestIndex = 0;
	}

	void ReactivateAIs()
	{
		foreach (var ai in mAIs)
		{
			ai.gameObject.SetActive(true);
			ai.Restart();
		}
		
		mActiveCount = mCount;
		mStartTime = Time.time;
		mCurrentBestFitness = -Mathf.Infinity;
		mCurrentBestIndex = 0;
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.F5))
		{
			NeuralNetwork.save(0);
			Debug.Log("AI Saved from NeuralNetwork0");
		}
		else if (Input.GetKeyDown(KeyCode.F8))
		{
			NeuralNetwork.load(1);
			Debug.Log("AI Loaded to NeuralNetwork1");
		}
		
		// if all AIs are dead before CleanupDeadAIs() is called,
		// this means the game was restarted.
		if (mActiveCount == 0)
			ReactivateAIs();
		else
			DeactivateDeadAIs();

		// if all AI are dead ...
		if (mActiveCount == 0)
		{
			// replace all neural networks with the one that had
			// the best fitness in this generation
			NeuralNetwork.replaceOthers(mCurrentBestIndex);

			// load the saved AI with the best fitness from all
			// generations into index 1 of the NeuralNetwork array
			NeuralNetwork.load(1);
			
			// mutate all but the first 2 AI
			NeuralNetwork.mutate(2, mCount - 1);

			// switch to game over mode
			sGameController.Initialize("Over");
		}
		else
		{
			SetJumpPredictions();
		}
	}

	void DeactivateDeadAIs()
	{
		foreach (var ai in mAIs)
		{
			// ignore already deactivated AIs
			if (!ai.isActiveAndEnabled)
				continue;

			// if AI is dead then ...
			if	(ai.mTimeOfDeath > 0)
			{
				// ... calculate its fitness and ...
				var time_fitness = (ai.mTimeOfDeath - mStartTime);
				var dist_fitness = sObstacleSpawner.ClosestObstacleToPlayer().Right() - mPlayerLeft;
				var fitness = time_fitness - dist_fitness * 0.8f;
				// ... compare it to the current best
				if (fitness > mCurrentBestFitness)
				{
					mCurrentBestFitness = fitness;
					mCurrentBestIndex = ai.mIndex;
					Debug.Log("Current Best Fitness: " + mCurrentBestFitness);

					if (fitness > mHistoricallyBestFitness)
					{
						mHistoricallyBestFitness = fitness;
						NeuralNetwork.save(ai.mIndex);
						Debug.Log("Historically Best Fitness: "
							+ mCurrentBestFitness + " (AI SAVED)");
					}
				}

				ai.gameObject.SetActive(false);
				--mActiveCount;
			}
		}
	}

	void SetJumpPredictions()
	{
		var obstacle = sObstacleSpawner.ClosestObstacleToPlayer();
		var obstacleDist = Utilities.Map(obstacle.Right(),
			mPlayerLeft, sObstacleSpawner.spawnPoint.x, 0, 1);
		var obstacleHeight = obstacle.CompareTag("TallObstacle") ? 1f : 0f; // is tall yes : no

		// Get values for input data
		foreach (var ai in mAIs)
		{
			if (!ai.isActiveAndEnabled)
				continue;

			var aiHeight = ai.HasJumped() ? 1f : 0f; // has jumped yes : no

			var inputs = new InputData(
				obstacleDist,
				obstacleHeight,
				aiHeight, 
				0);

			var outputs = NeuralNetwork.guess(ai.mIndex, inputs);
			ai.mShouldJump = outputs.x > outputs.y;
			ai.mShouldDoubleJump = outputs.y > 0.6f;
		}
	}

	public float FindPlayerLeft()
	{
		float maxOffset = 0;
		foreach (var player in mPlayerPrefabs)
		{
			maxOffset = Mathf.Max(maxOffset,
				player.GetComponent<BoxCollider>().bounds.extents.x);
		}
		return mSpawnPosition.x - maxOffset;
	}
}
