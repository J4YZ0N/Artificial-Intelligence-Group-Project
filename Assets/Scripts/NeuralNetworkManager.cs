using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public struct InputData
{
	public InputData(float x, float y, float z, float w)
	{
		this.x = x;
		this.y = y;
		this.z = z;
		this.w = w;
	}

	public float x;
	public float y;
	public float z;
	public float w;
}
public struct OutputData
{
	public OutputData(float x, float y)
	{
		this.x = x;
		this.y = y;
	}

	public float x;
	public float y;
}

public class NeuralNetwork
{
	const string DLL_NAME = "neural_network";

	// add 'count' networks to the array of NeuralNetworks
	// all networks start with random weights
	[DllImport(DLL_NAME)]
	public static extern
	void addNetworks(int count);

	// get the outputs of NeuralNetwork at 'index' with given 'inputs'
	[DllImport(DLL_NAME)]
	public static extern
	OutputData guess(int index, InputData inputs);

	// trains the NeuralNetwork at 'index' with given 'inputs', 'answers'
	// answers = expected outputs
	[DllImport(DLL_NAME)]
	public static extern
	void train(int index, InputData inputs, OutputData answers);

	// replaces all NeuralNetworks with copies of NeuralNetwork at 'index'
	[DllImport(DLL_NAME)]
	public static extern
	void replaceOthers(int index);

	// mutate neural networks in range lo, hi
	[DllImport(DLL_NAME)]
	public static extern
	void mutate(int lo, int hi);

	// save the specific neural network at index
	[DllImport(DLL_NAME)]
	public static extern
	void save(int index);

	// replace neural network at index with saved data
	[DllImport(DLL_NAME)]
	public static extern
	void load(int index);
}

public class NeuralNetworkManager : MonoBehaviour
{
	public List<GameObject> mPlayerPrefabs;

	// position for players to spawn at
	public Vector3 mSpawnPosition = new Vector3(-3.0f, 0, 0);

	// amount of neural networks to create
	const int mCount = 256;
	int mActiveCount = mCount;

	// list of AI players
	List<AI> mAIs = new List<AI>();

	// constant minimum x bound of player in global space
	// used when trying to find closest obstacle to the right
	[System.NonSerialized]
	public float mPlayerMinX;

	GameController sGameController;
	ObstacleSpawner sObstacleSpawner;

	float mBestFitness = -Mathf.Infinity;

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
		List<AI> toDeactivate = new List<AI>();

		foreach (var ai in mAIs)
		{
			if (ai.isActiveAndEnabled && ai.mTimeOfDeath > 0)
			{
				toDeactivate.Add(ai);
			}
		}

		for (int i = 0; i < toDeactivate.Count; ++i)
		{
			var value = Fitness(i);
			if (value > mCurrentBestFitness)
			{
				mCurrentBestFitness = value;
				mCurrentBestIndex = toDeactivate[i].mIndex;
				Debug.Log("Current Best = " + mCurrentBestFitness);
				if (value > mBestFitness)
				{
					mBestFitness = value;
					Debug.Log("Current Best = " + mBestFitness + " saved");
					NeuralNetwork.save(toDeactivate[i].mIndex);
				}
			}
			//mAIs.Remove(toDeactivate[i]);
			toDeactivate[i].gameObject.SetActive(false);
			--mActiveCount;
			//mInactiveAIs.Add(toDeactivate[i]);
		}
	}

	// fitness of AI in mAIs[i]
	// note: NOT THE SAME AS NEURAL NETWORK INDEX!
	float Fitness(int i)
	{
		return (mAIs[0].mTimeOfDeath - mStartTime) - DistanceToClosestObstacle(i);
	}

	float DistanceToClosestObstacle(int i)
	{
		return Vector2.Dot(mAIs[0].BottomLeft(),
			sObstacleSpawner.ClosestObstacleToPlayer().TopRight());
	}

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
			var inputs = new InputData( distance.x, distance.y, ai.transform.position.y / 2.0f, 0 );
			var outputs = NeuralNetwork.guess(ai.mIndex, inputs);
			//Debug.Log("x: " + outputs.x + ", y: " + outputs.y);
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
