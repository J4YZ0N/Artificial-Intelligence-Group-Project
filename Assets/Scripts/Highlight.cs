using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Highlight
{
	static Renderer mRenderer;
	static Material mMaterial;

	static public void ChangeHighlightTarget(GameObject instance)
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
