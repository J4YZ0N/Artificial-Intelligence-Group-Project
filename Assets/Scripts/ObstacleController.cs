using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using Util;

public class ObstacleController : MonoBehaviour
{
	// information for neural network
	float top;

	// set by ObstacleSpawner
	public GameController gameController;

	// Start is called before the first frame update
	void Start()
	{
		var box = GetComponent<BoxCollider>();
		top = box.bounds.extents.y;
	}

	// Update is called once per frame
	void Update()
	{
		// Moves obstacle left across screen
		Vector2 newPosition = new Vector2(gameController.Speed * Time.deltaTime, 0.0f);
		Vector2 currentPosition = transform.position;

		currentPosition -= newPosition;
		transform.position = currentPosition;
	}

	// relevant information for neural network
	public Vector2 getTopMiddle()
	{
		return new Vector2(transform.position.x, transform.position.y + top);
	}
}