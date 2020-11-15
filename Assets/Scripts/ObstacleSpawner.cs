using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
	public List<GameObject> mObstaclePrefabs = new List<GameObject> { };

	public float mMinSpawnInterval = 1.0f;
	public float mMaxSpawnInterval = 4.0f;

	const float mStartSpawnTime = -10;
	float mNextSpawnTime = mStartSpawnTime;

	Vector3 mSpawnPoint;

	List<GlobalBounds> mObstacles = new List<GlobalBounds> { };

	GameController mGameController;
	//NeuralNetworkManager mNeuralNetManager;
	
	// closest obstacle to player's right
	GlobalBounds mClosestObstacle;

	// distance
	float mObstacleExtentX;
	float mPlayerMinX;

	void Start()
	{
		mGameController = FindObjectOfType<GameController>();
		//mNeuralNetManager = FindObjectOfType<NeuralNetworkManager>();


		// set resetPosition relative to camera's bounds
		var x = Camera.main.orthographicSize * Camera.main.aspect;
		mSpawnPoint = new Vector3(x, -0.5f, 0);

		SpawnRandom();
		mClosestObstacle = mObstacles[0];
	}

	void Update()
	{
		// find obstacles that have left the screen and ...
		var toDespawn = new List<GlobalBounds> { };
		foreach (var obstacle in mObstacles)
		{
			if (obstacle.Right() < -mSpawnPoint.x)
			{
				toDespawn.Add(obstacle);
			}
		}
		// ... remove them
		foreach (var obstacle in toDespawn)
		{
			Destroy(obstacle.gameObject);
			mObstacles.Remove(obstacle);
		}

		SpawnRandom();

		UpdateClosestToPlayer();
	}

	void SpawnRandom()
	{
		// instantiate new objects at a randomized time interval
		if (Time.time > mNextSpawnTime)
		{
			mNextSpawnTime = Time.time + Random.Range(mMinSpawnInterval, mMaxSpawnInterval);
			DoSpawnRandom();
		}
	}

	// finds the closest Obstacle to player 
	void UpdateClosestToPlayer()
	{
		if (mClosestObstacle == null)
			mClosestObstacle = mObstacles[0];

		var x = mClosestObstacle.Right();

		if (x < mPlayerMinX)
		{
			// assuming the first obstacle with x > player's
			// is the next closest
			foreach (var obstacle in mObstacles)
			{
				if (obstacle.Right() > mPlayerMinX)
					mClosestObstacle = obstacle;
			}
		}
	}

	public GlobalBounds ClosestObstacleToPlayer()
	{
		return mClosestObstacle;
	}

	public void DestroyAllObstacles()
	{
		foreach (var obstacle in mObstacles)
		{
			Destroy(obstacle.gameObject);
		}
		mObstacles.Clear();
		mNextSpawnTime = mStartSpawnTime;
	}

	void DoSpawnRandom()
	{
		// select a random prefab, ...
		var i = Random.Range(0, mObstaclePrefabs.Count);
		// ... instantiate it at the spawn point ...
		var instance = Instantiate(mObstaclePrefabs[i], mSpawnPoint, Quaternion.identity);
		// ... with an obstacle controller ...
		instance.tag = "Obstacle";
		instance.GetOrAddComponent<ObstacleController>();
		// ... a collision box, ...
		var bc = instance.GetOrAddComponent<BoxCollider>();
		// ... that's a trigger, ...
		bc.isTrigger = true;
		// ... and GlobalBounds, ...
		var gb = instance.GetOrAddComponent<GlobalBounds>();
		// ... and finally add its GlobalBound to our list
		mObstacles.Add(gb);
	}
}
