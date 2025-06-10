using System;
using UnityEngine;

[ExecuteInEditMode]
public class DummyPlayer : MonoBehaviour
{
	private void Awake()
	{
		if (Application.isPlaying)
		{
			throw new Exception("I shouldn't exist. Kill me please. I'm for testing only.");
		}
	}
}
