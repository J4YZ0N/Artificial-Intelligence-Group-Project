using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
	public List<GameObject> mObstaclePrefabs = new List<GameObject>();

	public float mMinSpawnInterval = 1.0f;
	public float mMaxSpawnInterval = 4.0f;

	const float mStartSpawnTime = -10;
	float mNextSpawnTime = mStartSpawnTime;

	Vector3 mSpawnPoint;

	public Vector3 spawnPoint {
		get { return mSpawnPoint; }
	}

	List<GlobalBounds> mObstacles = new List<GlobalBounds>();

	// Upside-down obstacles for the AIs to hit their heads on
	List<GlobalBounds> mUpsidedownObstacles = new List<GlobalBounds>();

	GameController mGameController;

	ChangeTheme mThemeChanger;

	// closest obstacle to player's right
	GlobalBounds mClosestObstacle;

	// distance
	float mPlayerMinX;

	void Start()
	{
		mGameController = GetComponent<GameController>();
		mPlayerMinX = GetComponent<NeuralNetworkManager>().FindPlayerLeft();
		mThemeChanger = GetComponent<ChangeTheme>();

		mObstaclePrefabs.Add(mThemeChanger.currentTheme.obstacle1);
		mObstaclePrefabs.Add(mThemeChanger.currentTheme.obstacle2);
		mObstaclePrefabs.Add(mThemeChanger.currentTheme.obstacle3);

		// set resetPosition relative to camera's bounds
		var x = Camera.main.orthographicSize * Camera.main.aspect;
		mSpawnPoint = new Vector3(x, 0, 0);

		DoSpawnRandom();
		mClosestObstacle = mObstacles[0];
	}

	void Update()
	{
		// destroy obstacles that have left the screen
		for (int i = mObstacles.Count - 1; i >= 0; --i)
		{
			if (mObstacles[i].Right() < -mSpawnPoint.x)
			{
				Destroy(mObstacles[i].gameObject);
				mObstacles.RemoveAt(i);
			}
		}

		AttemptSpawnRandom();

		UpdateClosestToPlayer();
	}

	void AttemptSpawnRandom()
	{
		// instantiate new objects at a randomized time interval
		if (Time.time > mNextSpawnTime)
		{
			DoSpawnRandom();
		}
	}

	public void ChangeObstacles()
    {
		mObstaclePrefabs.Clear();
		mObstaclePrefabs.Add(mThemeChanger.currentTheme.obstacle1);
		mObstaclePrefabs.Add(mThemeChanger.currentTheme.obstacle2);
		mObstaclePrefabs.Add(mThemeChanger.currentTheme.obstacle3);
	}

	// finds the closest Obstacle to player 
	void UpdateClosestToPlayer()
	{
		if (mClosestObstacle == null)
		{
			mClosestObstacle = mObstacles[0];
		}

		var x = mClosestObstacle.Right();

		if (x < mPlayerMinX)
		{
			// assuming the first obstacle with x > player's
			// is the next closest
			float minDist = 1000;
			foreach (var obstacle in mObstacles)
			{
				float dist = obstacle.Right() - mPlayerMinX;
				if (obstacle.Right() > mPlayerMinX && dist < minDist)
				{
					mClosestObstacle = obstacle;
					minDist = dist;
				}
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
		foreach (var obstacle in mUpsidedownObstacles)
		{
			Destroy(obstacle.gameObject);
		}
		mObstacles.Clear();
		mUpsidedownObstacles.Clear();
		mNextSpawnTime = mStartSpawnTime;
	}

	void DoSpawnRandom()
	{
		// set the next spawn time
		mNextSpawnTime = Time.time + Random.Range(mMinSpawnInterval, mMaxSpawnInterval);

		// select a random prefab, ...
		var i = Random.Range(0, mObstaclePrefabs.Count);

			// ... instantiate it at the spawn point ...
			var instance = Instantiate(mObstaclePrefabs[i], mSpawnPoint, Quaternion.identity);
			// ... with an obstacle controller ...
			//instance.tag = "Obstacle";
			var oc = instance.GetOrAddComponent<ObstacleController>();
			oc.gameController = mGameController;
			// ... a collision box, ...
			var bc = instance.GetOrAddComponent<BoxCollider>();
			// ... that's a trigger, ...
			bc.isTrigger = true;
			// ... and GlobalBounds, ...
			var gb = instance.GetOrAddComponent<GlobalBounds>();
			// ... and finally add its GlobalBound to our list
			mObstacles.Add(gb);

		if (instance.CompareTag("ShortObstacle"))
		{
			// ... instantiate it at the spawn point ...
			var instance2 = Instantiate(mObstaclePrefabs[1], mSpawnPoint, Quaternion.identity);
			if (instance2.CompareTag("ShortObstacle"))
			{
				instance2.transform.position = new Vector3(instance2.transform.position.x, 3f, instance2.transform.position.z);
				instance2.transform.eulerAngles += new Vector3(0, 0, 180);
				// ... with an obstacle controller ...
				//instance2.tag = "Obstacle";
				var oc2 = instance2.GetOrAddComponent<ObstacleController>();
				oc2.gameController = mGameController;
				// ... a collision box, ...
				var bc2 = instance2.GetOrAddComponent<BoxCollider>();
				// ... that's a trigger, ...
				bc2.isTrigger = true;
				// ... and GlobalBounds, ...
				var gb2 = instance2.GetOrAddComponent<GlobalBounds>();
				// ... and finally add its GlobalBound to our list
				mUpsidedownObstacles.Add(gb2);
			}
            else
            {
				Destroy(instance2);
            }
		}
	}
}
