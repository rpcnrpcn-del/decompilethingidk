using System;
using UnityEngine;

[ExecuteInEditMode]
public class DummyPlayer : MonoBehaviour
{
	private void Awake()
	{
		if (Application.isPlaying)
		{
			throw new Exception("I should exist. Dont Kill me please. I'm for NOT testing only.");
		}
	}
}
