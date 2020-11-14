using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using Util;

public class ObstacleController : MonoBehaviour
{
	// resetPosition moved to ObstacleSpawner
	// as part of ObstacleSpawner.mSpawnPoint
	/*float resetPosition = 6.0f;
	float boundary = -6.0f;*/

	// information for neural network
	float top;

	//
	public GameController gameController;

	// Start is called before the first frame update
	void Start()
	{
		if (gameController == null)
			gameController = FindObjectOfType<GameController>();

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

		// Resets obstacle position to left side when left boundary is reached
		/*if (transform.position.x <= boundary)
		{
			transform.position = new Vector2(resetPosition, -0.5f);
		}*/
	}

	// relevant information for neural network
	public Vector2 getTopMiddle()
	{
		return new Vector2(transform.position.x, transform.position.y + top);
	}
}