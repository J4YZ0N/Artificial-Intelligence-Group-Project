# Artificial Intelligence Group Project Documentation

## Table of Contents
1. [Introduction](#introduction)
2. [Neural Network Plugin](#plugin)
3. [Unity Components](#unity_components)

## Introduction <a name="introduction"></a>

Artificial Intelligence Group Project is an implementation of a deep neural network and genetic machine learning algorithm for playing an endless runner.

The neural network implementation and storage are written in C++ and imported into an Unity3D project as a plugin.

## Neural Network Plugin <a name="plugin"></a>

The Neural Network Plugin consists of four parts: utilities, a custom matrix class, a neural network class, and an API wrapper to a neural network container.

### Utilities

#### Traits

Defined in "traits.h"

Some modern C++ random distribution functions take integers as input and others take real numbers. Providing the incorrect input results in undefined behaviour, and so RequireReal and RequireInteger are used to prevent compilation of random distribution functions with incorrect input being implicitly cast, and also to select appropriate function overloads.

#### Random

Defined in "random.h"

"random" is a non-copyable class where _all methods and members are static_. It's used as boilerplate to simplify getting random numbers and to efficiently reuse a single random engine.

Modern C++ random number generation requires creating a random device, random engine and then selecting a random distribution function specific to whether the input is an integer or real number.

```c++
#include <random>
int main()
{
	using Engine = std::mt19937;
	using Distribution = std::uniform_int_distribution<short>;

	auto engine = Engine{ std::random_device{}() };
	auto result = Distribution(1, 6)(engine);
	//...
}

```

The problem is that random engines take up a lot of memory, and so it's undesirable to let them go out of scope, especially since they can be reused by any distribution.

| Methods | Description |
| :-----: | :---------: |
| random::get(T lo, T hi) | returns a value of type T between lo (inclusive) and hi (inclusive)|
| random::get(T hi) | returns a value of type T between 0 (inclusive) and hi (inclusive)|
| random::gaussian(T mean, T stddev) | returns a value from a normal distribution with a higher chance of values being closer to the mean give a lower stddev (standard deviation) |

#### Matrix

Defined in "matrix.h".

"mat<Rows, Cols>" is a basic matrix struct implemented with element-wise and scalar math operations. The element-wise operations also include multiplication, resulting in the [Hadamard product](https://en.wikipedia.org/wiki/Hadamard_product_(matrices)) instead of the more familiar Matrix product. Its members are kept public to allow trivial copy assignment.

| Methods | Description |
| :-----: | :---------: |
| mat<Rows, Cols>.randomFill() | replaces all elements with random values from interval [-1.0 and 1.0]. |
| mat<Rows, Cols>.setTranspose() | replaces matrix data with its transpose (only implemented for square matrices) |
| mat<Rows, Cols>.getTranspose() | returns a new matrix representing this one's transpose |

##### Related Functions

| Function | Description |
| :-----: | :---------: |
| product(mat<R1, C1R2> const& lo, mat<C1R2, C2> const& ro) | returns the [matrix product](https://en.wikipedia.org/wiki/Matrix_multiplication) between the matrices lo and ro. (only implemented if Cols of lo is equal to Rows of ro) |
| asColVec(std::array<float, N> const& arr) | creates a column vector (i.e. a mat<N, 1>) from a std::array<float, N> |
| asRowVec(std::array<float, N> const& arr) | creates a row vector (i.e. a mat<1, N>) from a std::array<float, N> |

#### Neural Network

Defined in "neural_network.h"

"NeuralNetwork<InputCount, HiddenCount, OutputCount>" is a deep neural network with 3 hidden layers. Each NeuralNetwork has 'InputCount' input nodes, 3 hidden layers with 'HiddenCount' nodes each, and 'OutputCount' output nodes. It stores the connections between each layer as weight matrices rather than perceptrons at each node. Biases are added and activations are applied before values

| Methods | Description |
| :-----: | :---------: |
| randomize() | Replaces all weights and biases with random values. |
| mutate() | Each weight or bias has a random chance of being offset by a random value from a normal distribution. |
| guess(std::array<float, InputCount> const& inputs) | Applies forward propagation to inputs and returns outputs as a mat<OutputCount, 1>. |
| train(std::array<float, InputCount> const& inputs, std::array<float, OutputCount> const& answers) | Applies back propagation to adjust weights and biases to bring the outputs of guess(inputs) closer to the target output, answers. |
| save(const char* filename) | Saves the NeuralNetwork as a binary file to "filename". Default filename is "nnd.bin". |
| load(const char* filename) | Copies over this neural network with a neural network stored in the binary file, "filename". Default filename is "nnd.bin". |

##### Related Functions

| Function | Description |
| :-----: | :---------: |
| mutate(float& x, float chance) | Random chance to offset x, where 0 < chance < 1.0. Offset will usually be at or close to 0. |
| float fast_sigmoid(float x) | Returns sigmoid estimation where -1 >= x <= 1. Used as activation function for each  |
| undo_sigmoid(float y) | Derivate of sigmoid. Returns an x value given result y of sigmoid(x). |
| apply_activation(mat<Rows, Cols>& m) | Applies activation function, fast_sigmoid(x), to each element, x, in m. |
| remove_activation(mat<Rows, Cols>& m) | Applies derivate of activation function to each element, x, in m. |
| apply_mutation(mat<Rows, Cols>& m, float chance) | Applies mutate(x, chance) to every element, x, of m. |

#### Wrapper

Defined in "wrapper.h"

API wrapper for interfacing with a container of Neural Networks. Each NeuralNetwork has 4 input nodes, 3 hidden layers with 8 nodes each, and 2 output nodes.

| Function | Description |
| :-----: | :---------: |
| addNetworks(int count) | add N elements to a container of NeuralNetwork<4, 8, 2>, where N = count. |
| guess(int index, Vector4D inputs) | Get the outputs of NeuralNetwork at 'index' with given 'inputs'. Assumed index is valid. |
| train(int index, Vector4D inputs, Vector2D answers) |  Trains the NeuralNetwork at 'index' with given 'inputs' and 'answers'. answers = expected outputs. See train method in Neural Network. |
| replaceOthers(int index) | Replaces all NeuralNetworks with copies of NeuralNetwork at 'index'. |
| mutate(int lo, int hi) | Mutate neural networks in range lo (inclusive), hi (inclusive). |
| save(int index) | Save the specific neural network at index. |
| load(int index) | Replace neural network at index with saved data. |

## Unity Components <a name="unity_components"></a>

