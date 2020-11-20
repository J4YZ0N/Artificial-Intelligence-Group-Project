# Artificial Intelligence Group Project Documentation

## Table of Contents
1. [Introduction](#introduction)
2. [Neural Network Plugin](#plugin)
	- [Utilities](#utilities)
		- [Traits](#traits)
		- [Random](#random)
	- [Mathematics](#mathematics)
		- [Matrix](#matrix)
		- [Related Functions](#math_related_functions)
	- [Machine Learning](#machine_learning)
		- [Neural Network](#neural_network)
		- [Related Functions](#learning_related_functions)
		- [Wrapper](#wrapper)
3. [Unity Components](#unity_components)
	- [Artificial Intelligence](#artificial_intelligence)
		- [Neural Network](#unity_neural_network)
		- [AI](#ai)
		- [Neural Network Manager](#neural_network_manager)
	- [Gameplay](#gameplay)
		- [Change Theme](#change_theme)
		- [Game Controller](#game_controller)
		- [Global Bounds](#global_bounds)
		- [Ground Controller](#ground_controller)
		- [Highlight](#highlight)
		- [Level Themes](#level_themes)
		- [Obstacle Controller](#obstacle_controller)
		- [Obstacle Spawner](#obstacle_spawner)
		- [Unity Engine Extensiont](#unity_engine_extensions)

## Introduction <a name="introduction"></a>

Artificial Intelligence Group Project is an implementation of a deep neural network and genetic machine learning algorithm for playing an endless runner.

The game is a simple endless runner with several different obstacles, some tall and some short, and the player can avoid them by a single or double (air) jump. The player dies if they touch any obstacle. Each short obstacle that spawns in has an obstacle above it to create a gap that will kill players that try to double jump. Occasionally the game's theme will change, replacing the ground tiles and background, and a new set of obstacles to jump over.

The neural network implementation and storage are written in C++ and imported into an Unity3D project as a plugin.

## Neural Network Plugin <a name="plugin"></a>

The Neural Network Plugin consists of four parts: utilities, a custom matrix class, a neural network class, and an API wrapper to a neural network container.

### Utilities <a name="utilities"></a>

#### Traits <a name="traits"></a>

Defined in "traits.h"

Some modern C++ random distribution functions take integers as input and others take real numbers. Providing the incorrect input results in undefined behaviour, and so RequireReal and RequireInteger are used to prevent compilation of random distribution functions with incorrect input being implicitly cast, and also to select appropriate function overloads.

#### Random <a name="random"></a>

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

### Mathematics <a name="mathematics"></a>

#### Matrix <a name="matrix"></a>

Defined in "matrix.h".

"mat<Rows, Cols>" is a basic matrix struct implemented with element-wise and scalar math operations. The element-wise operations also include multiplication, resulting in the [Hadamard product](https://en.wikipedia.org/wiki/Hadamard_product_(matrices)) instead of the more familiar [matrix product](https://en.wikipedia.org/wiki/Matrix_multiplication). Its members are kept public to allow trivial copy assignment.

| Methods | Description |
| :-----: | :---------: |
| mat<Rows, Cols>.randomFill() | replaces all elements with random values from interval [-1.0 and 1.0]. |
| mat<Rows, Cols>.setTranspose() | replaces matrix data with its transpose (only implemented for square matrices) |
| mat<Rows, Cols>.getTranspose() | returns a new matrix representing this one's transpose |

##### Related Functions <a name="math_related_functions"></a>

| Function | Description |
| :-----: | :---------: |
| product(mat<R1, C1R2> const& lo, mat<C1R2, C2> const& ro) | returns the [matrix product](https://en.wikipedia.org/wiki/Matrix_multiplication) between the matrices lo and ro. (only implemented if Cols of lo is equal to Rows of ro) |
| asColVec(std::array<float, N> const& arr) | creates a column vector (i.e. a mat<N, 1>) from a std::array<float, N> |
| asRowVec(std::array<float, N> const& arr) | creates a row vector (i.e. a mat<1, N>) from a std::array<float, N> |

### Machine Learning <a name="machine_learning"></a>

#### Neural Network <a name="neural_network"></a>

Defined in "neural_network.h"

"NeuralNetwork<InputCount, HiddenCount, OutputCount>" is a deep neural network with 3 hidden layers. Each NeuralNetwork has 'InputCount' input nodes, 3 hidden layers with 'HiddenCount' nodes each, and 'OutputCount' output nodes. It stores the connections between each layer as weight matrices rather than perceptrons holding weights at each node. Additionally, each node has a bias, also stored in a matrix, which is added to the matrix product between layerWeights and layerInputs, and finally activations are applied before values are then sent to the next layer.

| Methods | Description |
| :-----: | :---------: |
| randomize() | Replaces all weights and biases with random values. |
| mutate() | Each weight or bias has a random chance of being offset by a random value from a normal distribution. |
| guess(std::array<float, InputCount> const& inputs) | Applies forward propagation to inputs and returns outputs as a mat<OutputCount, 1>. |
| train(std::array<float, InputCount> const& inputs, std::array<float, OutputCount> const& answers) | Applies back propagation to adjust weights and biases to bring the outputs of guess(inputs) closer to the target output, answers. |
| save(const char* filename) | Saves the NeuralNetwork as a binary file to "filename". Default filename is "nnd.bin". |
| load(const char* filename) | Copies over this neural network with a neural network stored in the binary file, "filename". Default filename is "nnd.bin". |

##### Related Functions <a name="ml_related_functions"></a>

| Function | Description |
| :-----: | :---------: |
| mutate(float& x, float chance) | Random chance to offset x, where 0 < chance < 1.0. Offset will usually be at or close to 0. |
| float fast_sigmoid(float x) | Returns sigmoid estimation where -1 >= x <= 1. Used as activation function for each  |
| undo_sigmoid(float y) | Derivate of sigmoid. Returns an x value given result y of sigmoid(x). |
| apply_activation(mat<Rows, Cols>& m) | Applies activation function, fast_sigmoid(x), to each element, x, in m. |
| remove_activation(mat<Rows, Cols>& m) | Applies derivate of activation function to each element, x, in m. |
| apply_mutation(mat<Rows, Cols>& m, float chance) | Applies mutate(x, chance) to every element, x, of m. |

#### Wrapper <a name="wrapper"></a>

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

Unity Components are broken into Artificial Intelligence scripts and Gameplay scripts. Artificial Inetlligence handles all the calculations related to the AI component of the program, while Gameplay handles the actual playable game portion.

### Artificial Intelligence <a name="artificial_intelligence"></a>

#### Neural Network <a name="unity_neural_network"></a>

Interface into the neural network plugin. See [Neural Network Plugin](#plugin) for more details.

| Method | Description |
| :-----: | :---------: |
| addNetworks(int count) | Adds 'count' networks to the array of NeuralNetworks. |
| guess(int index, InputData inputs) | Returns the outputs of NeuralNetwork at 'index' with given 'inputs'. |
| train(int index, InputData inputs, OutputData answers) | Trains the NeuralNetwork at 'index' with given 'inputs', 'answers'. |
| replaceOthers(int index) | Replaces all NeuralNetworks with copies of NeuralNetwork at 'index'. |
| mutate(int lo, int hi) | Mutates neural networks in range lo, hi. |
| save(int index) | Saves the specific neural network at 'index'. |
| load(int index) | Replaces the neural network at 'index' with saved data. |

#### AI <a name="ai"></a>

AI component managed by the Neural Network Manager and used for controlling a player object. It stores an index associated with one of the neural networks. 

| Method | Description |
| :-----: | :---------: |
| Start() | Gets start position as well as components for the animator, rigidbody, and global bounds. |
| Restart() | Resets this AI to the state it needs to be in at the start of a new game. |
| HasJumped() | Returns true if the AI is not touching the ground. |
| Update() | Calls Jump() every frame. |
| Jump() | The AI player jumps if 3 conditions are met: it hasn't double jumped, it doesn't already have an upward velocity, and if mShouldJump, updated by NeuralNetworkManager by calling the guess function of the AI's associated neural network, is true |

#### Neural Network Manager <a name="neural_network_manager"></a>

Manager responsible for calling methods from [Neural Network](#unity_neural_network) and also for AI instances. It creates an equal amount of neural networks from the plugin's side and gives the index of each to an associated AI instance.

| Method | Description |
| :-----: | :---------: |
| Start() | Gets game controller and obstacle spawner components. Adds all neural networks, and an AI for each neural network. Sets the start time to the current Time.time, the best fitness to -Mathf.Infinity, and the best index to 0. |
| ReactivateAIs() | All AI's are set to active and restarted. |
| Update() | Saves and loads neural network data using F5 and F8, respectively. When all AI are dead, the neural network updates the current best data into index 1. All but the first 2 AI are mutated. Switches to game over mode. |
| DeactivateDeadAIs() | Check's dead AI's score against current best score. The higher score between the two is saved. If it is higher than the current best, it is also compared to the historically best score. The higher score between the two is saved. |
| SetJumpPredictions() | Calls NeuralNetwork.guess(index, inputs) for each AI instance |
| FindPlayerLeft() | Returns the leftmost coordinate of the player. |
| OnCollisionEnter(Collision col) | Resets jump count if col is tagged as a ground object. |
| OnTriggerEnter(Collider other) | If other has an Obstacle tag, the AI's TimeOfDeath will be set, which will be used by NeuralNetworkManager to both disable this GameObject and also  |
| BottomLeft() | Returns the position of the object's bottom left corner.  |

### Gameplay <a name="gameplay"></a>

#### Change Theme <a name="change_theme"></a>

Stores prefabs of game objects with variying visual themes and cycles between them when called.

| Method | Description |
| :-----: | :---------: |
| Start() | Initiates list of themes. Sets current theme to 'plain'. |
| Change() | Periodically changes background theme. Updates skybox accordingly. |

#### Game Controller <a name="game_controller"></a>

Handles the running of the game UI, game intialization, and score calculation.

| Method | Description |
| :-----: | :---------: |
| Start() | Gets obstacle spawner, neural network manager, and change theme components. |
| Initialize(string mode) | Changes game mode between title screen, playable game, and game over screen. |
| BeginTitleScreen() | Disables score label, game over state, restart button, obstacle spawner, and neural network manager. |
| BeginPlay() | Enables score label, title state, start button, game over state, restart button, obstacle spawner, and neural network manager. Sets score to 0, speed to 2.0. |
| BeginGameOver() | Disables title state, start button, obstacle spawner, and neural network manager. Enables game over state and restart button. |
| UpSpeed() | Increases the speed by 1.0 every 100 points. |
| AddScore() | Adds 1 point to the score. |

#### Global Bounds <a name="global_bounds"></a>

Used on the player and obstacles in order to check their position compared to other game objects.

| Method | Description |
| :-----: | :---------: |
| Start() | Gets the object's box collider. |
| Left() | Returns the x position of the object's left side. |
| Right() | Returns the x position of the object's right side. |
| Top() | Returns the y position of the object's top side. |
| Bottom() | Returns the y position of the object's bottom side. |
| BottomLeft() | Returns the position of the object's bottom left corner. |
| BottomRight() | Returns the position of the object's bottom right corner. |
| TopRight() | Returns the position of the object's top right corner. |
| TopLeft() | Returns the position of the object's top left corner. |

#### Ground Controller <a name="ground_controller"></a>

Moves ground cubes left across screen and resets them to the right when they move off the left side. Used to simulate the effect of the character running.

| Method | Description |
| :-----: | :---------: |
| Start() | Initializes the game controller. |
| Update() | Updates the position of ground objects. |

#### Highlight <a name="highlight"></a>

Highlights object closest to the player, used for bug testing.

| Method | Description |
| :-----: | :---------: |
| ChangeHighlightTarget(GameObject instance) | Adds a material that highlights a game object. |

#### Level Themes <a name="level_themes"></a>

Class defining GameObjects relating to theme. Objects include the ground, 3 obstacles, and the field.

#### Obstacle Controller <a name="obstacle_controller"></a>

Moves Obstacle left across the screen, also gets the top middle of the obstacle for use in [Neural Network](#unity_neural_netork)

| Method | Description |
| :-----: | :---------: |
| Start() | Gets the object's box collider and its top bounds value on the y axis. |
| Update() | Moves obstacles left across the screen. |
| getTopMiddle() | Returns the top middle position value. It is used for the neural network. |

#### Obstacle Spawner <a name="obstacle_spawner"></a>

Grabs obstacle prefabs from [Change Theme](#change_theme) and spawns them at the right side of the screen. If the object is short it spawns a second obstacle above it on the ceiling.

| Method | Description |
| :-----: | :---------: |
| Start() | Gets game controller, neural network manager, and change themes components. Adds 3 obstacles to the list of obstacle prefabs. Sets the spawn point relative tot he camera's bounds. |
| Update() | Destroys obstacles that have left the screen and calls AttemptSpawnRandom() and UpdateClosestToPlayer(). |
| AttemptSpawnRandom() | Instantiates new objects at a random time interval. |
| ChangeObstacles() | Clears obstacle prefabs and adds 3 new obstacles. |
| UpdateClosestToPlayer() | Finds and highlights the closest obstacle to the player. |
| ClosestObstacleToPlayer() | Returns the closest obstacle to the player. |
| DestroyAllObstacles() | Destroys all obstacles and clears them from the list of obstacles and the list of upside down obstacles. Sets the next spawn time to the start spawn time. |
| DoSpawnRandom() | Determines the next spawn time using a randomized spawn interval. Instantiates obstacles with an obstacle controller, box collider, and global bounds. Adds the obstacle to the list of obstacles. |

#### Unity Engine Extensions <a name="unity_engine_extensions"></a>

Adds a function for use in Unity scripts that searches for a component and adds one if one is not found.

| Method | Description |
| :-----: | :---------: |
| GetOrAddComponent<T>(this GameObject obj) | Either adds a new component or returns the current component. |
