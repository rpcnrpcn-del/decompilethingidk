using System;
using UnityEngine;

[Serializable]
public class PlayerGameObjectMap
{
	[SerializeField]
	private GameObject[] gameObjects;

	public GameObject GetForPlayer(int playerIndex)
	{
		GameObject result = null;
		if (gameObjects.Length > 0)
		{
			result = gameObjects[playerIndex % gameObjects.Length];
		}
		return result;
	}

	public GameObject GetRandom()
	{
		GameObject result = null;
		if (gameObjects.Length > 0)
		{
			result = gameObjects[UnityEngine.Random.Range(0, gameObjects.Length)];
		}
		return result;
	}
}
