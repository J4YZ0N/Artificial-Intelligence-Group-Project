using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
	public List<GameObject> mObstaclePrefabs = new List<GameObject> { };

	public float mMinSpawnInterval = 1.0f;
	public float mMaxSpawnInterval = 4.0f;

	float mNextSpawnTime = 0;

	Vector3 mSpawnPoint;

	List<Transform> mObstacles = new List<Transform> { };

	GameController mGameController;

	void Start()
	{
		mGameController = FindObjectOfType<GameController>();

		// set resetPosition relative to camera's bounds
		var x = Camera.main.orthographicSize * Camera.main.aspect;
		mSpawnPoint = new Vector3(x, -0.5f, 0);
	}

	void Update()
	{
		// find obstacles that have left the screen and ...
		var toDespawn = new List<Transform> { };
		foreach (var obstacle in mObstacles)
		{
			if (obstacle.position.x < -mSpawnPoint.x)
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

		// instantiate new objects at a randomized time interval
		if (Time.time > mNextSpawnTime)
		{
			mNextSpawnTime = Time.time + Random.Range(mMinSpawnInterval, mMaxSpawnInterval);
			SpawnRandom();
		}
	}

	public void DestroyAllObstacles()
	{
		foreach (var obstacle in mObstacles)
		{
			Destroy(obstacle.gameObject);
		}
		mObstacles.Clear();
	}

	void SpawnRandom()
	{
		// select a random prefab, ...
		var i = Random.Range(0, mObstaclePrefabs.Count);
		// ... instantiate it at the spawn point ...
		var instance = Instantiate(mObstaclePrefabs[i], mSpawnPoint, Quaternion.identity);
		// ... with an obstacle controller ...
		instance.tag = "Obstacle";
		var ctrlr = instance.GetComponent<ObstacleController>();
		ctrlr.gameController = mGameController;
		if (ctrlr == null)
			instance.AddComponent<ObstacleController>();
		// .. and a collision box, ...
		var cb = instance.GetComponent<BoxCollider>();
		if (cb == null)
			cb = instance.AddComponent<BoxCollider>();
		// ... make it a trigger, ...
		cb.isTrigger = true;
		// ... and finally add its transform to our list
		mObstacles.Add(instance.transform);
	}
}
