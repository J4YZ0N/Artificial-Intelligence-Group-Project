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
	const string DLL_NAME = "neural_network_v3";

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

	// save the specific neural network at index
	[DllImport(DLL_NAME)]
	public static extern
	void save(int index, string filename);

	// replace neural network at index with saved data
	[DllImport(DLL_NAME)]
	public static extern
	void load(int index, string filename);
}
