using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// for getting an object's corners in global space
// needed for the AI's input as well as changing
// AI's watched obstacle
public class GlobalBounds : MonoBehaviour
{
	Vector3 mExtents;

	// Start is called before the first frame update
	void Start()
	{
		var bc = GetComponent<BoxCollider>();
		if (bc != null)
		{
			mExtents = bc.bounds.extents;
		}
		else
		{
			mExtents = GetComponent<BoxCollider2D>().bounds.extents;
		}
	}

	public float Left()
	{
		return transform.position.x - mExtents.x;
	}
	public float Right()
	{
		return transform.position.x + mExtents.x;
	}
	public float Top()
	{
		return transform.position.y + mExtents.y;
	}
	public float Bottom()
	{
		return transform.position.x - mExtents.y;
	}

	public Vector2 BottomLeft()
	{
		return transform.position - mExtents;
	}
	public Vector2 BottomRight()
	{
		var offsets = new Vector3(mExtents.x, -mExtents.y, 0);
		return transform.position + offsets;
	}
	public Vector2 TopRight()
	{
		return transform.position + mExtents;
	}
	public Vector2 TopLeft()
	{
		var offsets = new Vector3(-mExtents.x, mExtents.y, 0);
		return transform.position + offsets;
	}
}
