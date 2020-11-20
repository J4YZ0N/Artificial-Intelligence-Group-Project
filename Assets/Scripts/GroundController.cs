using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundController : MonoBehaviour
{
	float mOffscreen;
	float mSize = 1.000001f;

	GameController sGameController;

	void Start()
	{

		sGameController = FindObjectOfType<GameController>();

		mOffscreen = -Camera.main.orthographicSize * Camera.main.aspect;
	}

	void Update()
	{

		var maxX = -1000f;
		var offset = new Vector3(sGameController.Speed * Time.deltaTime, 0, 0);

		foreach (Transform child in transform)
		{
			child.position -= offset;
			maxX = Mathf.Max(maxX, child.position.x);
		}

		foreach (Transform child in transform)
		{
			if (child.position.x < mOffscreen)
			{
				child.position = new Vector3(maxX + mSize,
					child.position.y, child.position.z);
				break;
			}
		}
	}
}
