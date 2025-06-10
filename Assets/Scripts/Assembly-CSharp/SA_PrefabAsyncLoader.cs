using System;
using System.Collections;
using UnityEngine;

public class SA_PrefabAsyncLoader : MonoBehaviour
{
	private string PrefabPath;

	public event Action<GameObject> ObjectLoadedAction = delegate
	{
	};

	public static SA_PrefabAsyncLoader Create(string name)
	{
		SA_PrefabAsyncLoader sA_PrefabAsyncLoader = new GameObject("PrefabAsyncLoader").AddComponent<SA_PrefabAsyncLoader>();
		sA_PrefabAsyncLoader.LoadAsync(name);
		return sA_PrefabAsyncLoader;
	}

	public void LoadAsync(string name)
	{
		PrefabPath = name;
		StartCoroutine(Load());
	}

	public IEnumerator Load()
	{
		ResourceRequest request = Resources.LoadAsync(PrefabPath);
		yield return request;
		if (request.asset == null)
		{
			Debug.LogWarning("Prefab not found at path: " + PrefabPath);
		}
		GameObject loadedObject = UnityEngine.Object.Instantiate(request.asset) as GameObject;
		this.ObjectLoadedAction(loadedObject);
	}
}
