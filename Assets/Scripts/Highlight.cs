using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// highlights a single object
public class Highlight : MonoBehaviour
{
	//public Material mHighlightMaterial;

	Renderer mRenderer;
	Material mMaterial;

	public void ChangeHighlightTarget(GameObject instance)
	{
		if (mRenderer != null)
		{
			mRenderer.material = mMaterial;
		}

		var renderer = instance.GetComponent<Renderer>();
		if (renderer != null)
		{
			mRenderer = renderer;
			mMaterial = renderer.material;
			renderer.material = null;
		}
		else
		{
			Debug.Log("Highlight target has no Renderer");
		}
	}
}
