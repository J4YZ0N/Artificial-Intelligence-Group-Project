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
	const int mCount = 10;

	// list of AI players
	List<AI> mAIs = new List<AI>();

	// constant minimum x bound of player in global space
	// used when trying to find or adjust neural network inputs
	[System.NonSerialized]
	public float mPlayerMinX;

	bool runTest = false;

	GameController sGameController;
	ObstacleSpawner sObstacleSpawner;

	void Start()
	{
		if (runTest)
		{
			PredictionTest();
			enabled = false;
			return;
		}

		sGameController = GetComponent<GameController>();
		sObstacleSpawner = GetComponent<ObstacleSpawner>();

		// add neural networks
		NeuralNetwork.addNetworks(mCount);

		// add an AI for each neural network
		AddAIs();

		SetPlayerMinX();
	}

	void AddAIs()
	{
		for (int i = 0; i < mCount; ++i)
		{
			var r = Random.Range(0, mPlayerPrefabs.Count);
			var instance = Instantiate(mPlayerPrefabs[r], mSpawnPosition, Quaternion.identity);
			instance.GetOrAddComponent<BoxCollider>();
			instance.GetOrAddComponent<GlobalBounds>();

			var ai = instance.GetOrAddComponent<AI>();
			// give AI an index to a neural network
			ai.mIndex = i;
			//ai.sGameController = sGameController;

			mAIs.Add(ai);
		}
	}

	void Update()
	{
		if (mAIs.Count == 0)
			AddAIs();
		CleanupDeadAIs();
		SetJumpPredictions();
		if (mAIs.Count == 0)
		{
			sGameController.Initialize("Over");
			return;
		}
	}

	void CleanupDeadAIs()
	{
		List<AI> toDestroy = new List<AI>();

		foreach (var ai in mAIs)
		{
			if (ai.mTimeOfDeath > 0)
			{
				toDestroy.Add(ai);
			}
		}

		// if all AI are dead, find the best
		// fitness and set them all to that
		// value
		if (toDestroy.Count == mAIs.Count)
		{
			float bestValue = fitness(0);
			int bestIndex = mAIs[0].mIndex;
			for (int i = 1; i < mAIs.Count; ++i)
			{
				var value = fitness(i);
				if (value > bestValue)
				{
					bestValue = value;
					bestIndex = mAIs[i].mIndex;
				}
			}

			NeuralNetwork.replaceOthers(bestIndex);
		}

		foreach (var ai in toDestroy)
		{
			mAIs.Remove(ai);
			Destroy(ai.gameObject);
		}
	}

	// fitness of AI in mAIs[i]
	// note: NOT THE SAME AS NEURAL NETWORK INDEX!
	float fitness(int i)
	{
		float dist = Vector2.Dot(mAIs[0].BottomLeft(),
			sObstacleSpawner.ClosestObstacleToPlayer().TopRight());
		return mAIs[0].mTimeOfDeath - dist;
	}

	//
	void SetJumpPredictions()
	{
		var topRight = sObstacleSpawner.ClosestObstacleToPlayer()
			.TopRight();
		// todo: get values for input data
		var inputs = new InputData( topRight.x, topRight.y, 0, 0 );
		foreach (var ai in mAIs)
		{
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

	void PredictionTest()
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
	}
}
