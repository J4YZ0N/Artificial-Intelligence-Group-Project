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
	List<AI> mAIs;

	// constant minimum x bound of player in global space
	// used when trying to find or adjust neural network inputs
	public float mPlayerMinX;

	bool runTest = false;

	GameController sGameController;

	void Start()
	{
		if (runTest)
		{
			PredictionTest();
			enabled = false;
			return;
		}

		sGameController = FindObjectOfType<GameController>();

		// add neural networks
		NeuralNetwork.addNetworks(mCount);

		// add AI objects
		for (int i = 0; i < mCount; ++i)
		{
			var r = Random.Range(0, mPlayerPrefabs.Count);
			var instance = Instantiate(mPlayerPrefabs[r], mSpawnPosition, Quaternion.identity);
			var ai = instance.GetComponent<AI>();
			if (ai == null)
				ai = instance.AddComponent<AI>();

			// give AI an index to a neural network
			ai.mIndex = i;
			ai.sGameController = sGameController;
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
