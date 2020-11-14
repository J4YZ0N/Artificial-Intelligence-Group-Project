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
	void Start()
	{
		int count = 1;
		NeuralNetwork.addNetworks(count);
		for (int i = 0; i < count; ++i)
		{
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
		}

		for (int i = 0; i < count; ++i)
		{
			var guess = NeuralNetwork.guess(i, new InputData(1.0f, 1.0f, 0.0f, 0.0f));
			Debug.Log(guess.x);
			Debug.Log(guess.y);
			guess = NeuralNetwork.guess(i, new InputData(0.0f, 1.0f, 1.0f, 0.0f));
			Debug.Log(guess.x);
			Debug.Log(guess.y);
			guess = NeuralNetwork.guess(i, new InputData(1.0f, 0.0f, 1.0f, 0.0f));
			Debug.Log(guess.x);
			Debug.Log(guess.y);
		}
	}
}
