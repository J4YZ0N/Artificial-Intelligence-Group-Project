﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using Util;

public class GameController : MonoBehaviour
{
	[Header("Obstacles")]
	// Variables to change level theme
	public LevelThemes plain;
	//public LevelThemes desert;
	//public LevelThemes ice;
	//public LevelThemes lava;
	//public LevelThemes grave;
	//public LevelThemes fort;

	// Variables to create obstacles
	//public GameObject obstacle;
	//private GameObject instantiation;
	//public List<GameObject> obstacles;
	private float speed = 2.0f;

	public bool allowSpeedUp = false;
	public bool allowAutoplay = true;

	[Header("Score")]
	// Variables to save and display score
	private int score;
	public Text scoreLabel;

	[Header("UI")]
	// Variables for UI text and buttons
	public GameObject title;
	public GameObject startButton;
	public GameObject gameOver;
	public GameObject restartButton;

	// Public variables with getter and setter for speed
	public float Speed
	{
		get
		{
			return speed;
		}

		set
		{
			speed = value;
		}
	}

	// Public variables with getter and setter for score
	public int Score
	{
		get
		{
			return score;
		}

		set
		{
			score = value;
			scoreLabel.text = "Score: " + score.ToString();
		}
	}

	ObstacleSpawner obstacleSpawner;
	NeuralNetworkManager neuralNetworkManager;

	// Start is called before the first frame update
	void Start()
	{
		// ignore collisions between GameObjects on the Player layer
		Physics.IgnoreLayerCollision(8, 8);

		obstacleSpawner = GetComponent<ObstacleSpawner>();
		neuralNetworkManager = GetComponent<NeuralNetworkManager>();

		if (obstacleSpawner == null)
		{
			Debug.Log("GameController also needs an ObstacleSpawner component");
			enabled = false;
			return;
		}

		Initialize("Open");
	}

	// Changes game mode between the title screen, playable game, and game over screen
	public void Initialize(string mode)
	{
		switch (mode)
		{
			// Title screen
			case "Open":
				BeginTitleScreen();
				break;
			// Gameplay screen
			case "Play":
				BeginPlay();
				break;
			// Game over screen
			case "Over":
				BeginGameOver();
				break;
			default:
				Debug.Log("mode " + mode + " defaulted");
				break;
		}

		// creates an pool of obstacles
		/*obstacles = new List<GameObject>();

		for (int i = 0; i < 5; i++)
		{
				//obstacles.Add(Instantiate(obstacle));
		}*/
	}


	void BeginTitleScreen()
	{
		scoreLabel.enabled = false;
		gameOver.SetActive(false);
		restartButton.SetActive(false);

		obstacleSpawner.enabled = false;
		neuralNetworkManager.enabled = false;
	}

	void BeginPlay()
	{
		scoreLabel.enabled = true;
		title.SetActive(false);
		startButton.SetActive(false);
		gameOver.SetActive(false);
		restartButton.SetActive(false);

		Score = 0;
		InvokeRepeating("AddScore", 0.0f, 1.0f);
		Speed = 2.0f;
		InvokeRepeating("UpSpeed", 0.0f, 1.0f);

		/*GameObject.Destroy(instantiation);
		instantiation = Instantiate(obstacle);*/

		obstacleSpawner.enabled = true;
		neuralNetworkManager.enabled = true;
	}

	void BeginGameOver()
	{
		title.SetActive(false);
		startButton.SetActive(false);
		gameOver.SetActive(true);
		restartButton.SetActive(true);

		CancelInvoke("UpSpeed");
		Speed = 0.0f;
		CancelInvoke("AddScore");


		obstacleSpawner.DestroyAllObstacles();

		if (allowAutoplay)
			Initialize("Play");
		else
		{
			obstacleSpawner.enabled = false;
			neuralNetworkManager.enabled = false;
		}
	}

	// Increases speed every 10 points
	private void UpSpeed()
	{
		if (allowSpeedUp && score % 10 == 0)
		{
			Speed += 1.0f;
		}
	}

	// Adds a point to the score, used with InvokeRepeating() to stagger every second
	private void AddScore()
	{
		Score += 1;
	}
}
