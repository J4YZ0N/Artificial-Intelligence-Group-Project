using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class UnityEngineExtensions
{
	static public T GetOrAddComponent<T>(this GameObject obj) where T : Component
	{
		return obj.GetComponent<T>() ?? obj.AddComponent<T>();
	}
}
