using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using Util;

public class ObstacleController : MonoBehaviour
{
    public float resetPosition = 6.0f;
    public float boundary = -6.0f;

    public GameController gameController;

    // Start is called before the first frame update
    void Start()
    {
        gameController = FindObjectOfType<GameController>();
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
        if (transform.position.x <= boundary)
        {
            transform.position = new Vector2(resetPosition, -0.5f);
        }
    }
}