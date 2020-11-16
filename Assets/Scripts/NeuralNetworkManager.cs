using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeuralNetworkManager : MonoBehaviour
{
	public List<GameObject> mPlayerPrefabs;

	// position for players to spawn at
	public Vector3 mSpawnPosition = new Vector3(-3.0f, 0, 0);

	// amount of neural networks to create
	const int mCount = 500;
	int mActiveCount = mCount;

	// list of AI players
	List<AI> mAIs = new List<AI>();

	// constant minimum x bound of player in global space
	// used when trying to find closest obstacle to the right
	[System.NonSerialized]
	public float mPlayerMinX;

	GameController sGameController;
	ObstacleSpawner sObstacleSpawner;

	float mHistoricallyBestFitness = -Mathf.Infinity;

	float mCurrentBestFitness;
	int mCurrentBestIndex;

	float mStartTime;

	void Start()
	{
		sGameController = GetComponent<GameController>();
		sObstacleSpawner = GetComponent<ObstacleSpawner>();

		// add neural networks
		NeuralNetwork.addNetworks(mCount);

		// add an AI for each neural network
		for (int i = 0; i < mCount; ++i)
		{
			var r = Random.Range(0, mPlayerPrefabs.Count);
			var instance = Instantiate(
				mPlayerPrefabs[r], mSpawnPosition, Quaternion.identity);
			instance.GetOrAddComponent<BoxCollider>();
			instance.GetOrAddComponent<GlobalBounds>();

			var ai = instance.GetOrAddComponent<AI>();
			// give AI an index to a neural network
			ai.mIndex = i;
			//ai.sGameController = sGameController;

			mAIs.Add(ai);
		}

		mStartTime = Time.time;
		mCurrentBestFitness = -Mathf.Infinity;
		mCurrentBestIndex = 0;

		SetPlayerMinX();
	}

	void ReactivateAIs()
	{
		foreach (var ai in mAIs)
		{
			ai.gameObject.SetActive(true);
			ai.Restart();
			//mAIs.Add(ai);
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
		//List<AI> toDeactivate = new List<AI>();

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
				var dist_fitness = sObstacleSpawner.ClosestObstacleToPlayer().Right() - mPlayerMinX;
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
						Debug.Log("Historically Best Fitness: " + mCurrentBestFitness + " (AI SAVED)");
					}
				}

				ai.gameObject.SetActive(false);
				--mActiveCount;
			}
		}

		// foreach (var ai in mAIs)
		// {
		// 	if (ai.isActiveAndEnabled && ai.mTimeOfDeath > 0)
		// 	{
		// 		toDeactivate.Add(ai);
		// 	}
		// }

		// for (int i = 0; i < toDeactivate.Count; ++i)
		// {
		// 	var value = Fitness(i);
		// 	if (value > mCurrentBestFitness)
		// 	{
		// 		mCurrentBestFitness = value;
		// 		mCurrentBestIndex = toDeactivate[i].mIndex;
		// 		Debug.Log("Current Best = " + mCurrentBestFitness);
		// 		if (value > mBestFitness)
		// 		{
		// 			mBestFitness = value;
		// 			Debug.Log("Current Best = " + mBestFitness + " saved");
		// 			NeuralNetwork.save(toDeactivate[i].mIndex);
		// 		}
		// 	}
		// 	//mAIs.Remove(toDeactivate[i]);
		// 	toDeactivate[i].gameObject.SetActive(false);
		// 	--mActiveCount;
		// 	//mInactiveAIs.Add(toDeactivate[i]);
		// }
	}

	// fitness of mAIs[i]
	// float Fitness(int i)
	// {
	// 	return (mAIs[0].mTimeOfDeath - mStartTime) - DistanceToClosestObstacle(i);
	// }
	// float DistanceToClosestObstacle(int i)
	// {
	// 	return Vector2.Dot(mAIs[0].BottomLeft(),
	// 		sObstacleSpawner.ClosestObstacleToPlayer().TopRight());
	// }

	//
	void SetJumpPredictions()
	{
		var distance = sObstacleSpawner
			.ClosestObstacleToPlayer().TopRight() - new Vector2(
				mSpawnPosition.x, mSpawnPosition.y) * 0.5f;
		distance.Normalize();
		
		// todo: get values for input data
		foreach (var ai in mAIs)
		{
			if (!ai.isActiveAndEnabled)
				continue;
			var inputs = new InputData(
				distance.x / sObstacleSpawner.spawnPoint.x,
				distance.y / 5.0f,
				ai.transform.position.y / 5.0f,
				0);
			var outputs = NeuralNetwork.guess(ai.mIndex, inputs);
			ai.mShouldJump = outputs.x > outputs.y;
		}
	}

	// 
	void SetPlayerMinX()
	{
		float maxOffset = 0;
		foreach (var player in mPlayerPrefabs)
		{
			maxOffset = Mathf.Max(maxOffset,
				player.GetComponent<BoxCollider>().bounds.extents.x);
		}
		mPlayerMinX = mSpawnPosition.x - maxOffset;
	}

	/*void PredictionTest()
	{
		NeuralNetwork.addNetworks(1);

		for (int j = 0; j < 100; ++j)
		{
			NeuralNetwork.train(0, new InputData(1.0f, 1.0f, 0.0f, 0.0f), new OutputData(1.0f, 0.0f));
			NeuralNetwork.train(0, new InputData(0.0f, 1.0f, 1.0f, 0.0f), new OutputData(1.0f, 0.0f));
			NeuralNetwork.train(0, new InputData(1.0f, 0.0f, 1.0f, 0.0f), new OutputData(1.0f, 0.0f));
			NeuralNetwork.train(0, new InputData(1.0f, 1.0f, 1.0f, 0.0f), new OutputData(1.0f, 0.0f));
			NeuralNetwork.train(0, new InputData(1.0f, 0.0f, 0.0f, 0.0f), new OutputData(0.0f, 0.0f));
			NeuralNetwork.train(0, new InputData(0.0f, 1.0f, 0.0f, 0.0f), new OutputData(0.0f, 0.0f));
			NeuralNetwork.train(0, new InputData(0.0f, 0.0f, 1.0f, 0.0f), new OutputData(0.0f, 0.0f));
			NeuralNetwork.train(0, new InputData(0.0f, 0.0f, 0.0f, 0.0f), new OutputData(0.0f, 0.0f));
		}

		var guess = NeuralNetwork.guess(0,
			new InputData(1.0f, 1.0f, 0.0f, 0.0f));
		Debug.Log(guess.x);
		Debug.Log(guess.y);
		guess = NeuralNetwork.guess(0,
			new InputData(0.0f, 1.0f, 1.0f, 0.0f));
		Debug.Log(guess.x);
		Debug.Log(guess.y);
		guess = NeuralNetwork.guess(0,
			new InputData(1.0f, 0.0f, 1.0f, 0.0f));
		Debug.Log(guess.x);
		Debug.Log(guess.y);
	}*/
}
