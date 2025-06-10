using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PUNPrefabInstantiate : MonoBehaviour
{
	[Serializable]
	public struct PrefabContainer
	{
		public string Name;

		public GameObject Prefab;

		public Transform[] CachedTransforms;
	}

	public static List<PUNPrefabInstantiate> All = new List<PUNPrefabInstantiate>();

	[Tooltip("Instantiate scene prefabs that belong to no one.")]
	public bool SceneLevelInstantiate = true;

	public PrefabContainer[] Containers;

	public int SceneIndex { get; private set; }

	private void Awake()
	{
		SceneIndex = SceneManager.GetActiveScene().buildIndex;
		if (GetCurrent() != null)
		{
			Debug.LogError("You have multiple scene PUNPrefabInstantiates associated with the same scene." + SceneManager.GetActiveScene());
		}
		All.Add(this);
	}

	private void OnDestroy()
	{
		All.Remove(this);
	}

	public void Start()
	{
		if (SceneLevelInstantiate)
		{
			InstantiatePrefabs();
		}
	}

	public void InstantiatePrefabs()
	{
		if ((!PhotonNetwork.isMasterClient && SceneLevelInstantiate) || Containers == null || !(PUNNetworkManager.Instance != null))
		{
			return;
		}
		for (int i = 0; i < Containers.Length; i++)
		{
			for (int j = 0; j < Containers[i].CachedTransforms.Length; j++)
			{
				Transform transform = Containers[i].CachedTransforms[j];
				int num = Containers[i].Prefab.GetComponentsInChildren<PhotonView>(true).Length;
				int[] array = new int[num];
				for (int k = 0; k < array.Length; k++)
				{
					array[k] = ((!SceneLevelInstantiate) ? PhotonNetwork.AllocateViewID() : PhotonNetwork.AllocateSceneViewID());
				}
				PUNNetworkManager.Instance.photonView.RPC("SpawnOnNetwork", PhotonTargets.AllBuffered, SceneIndex, i, transform.position, transform.rotation, transform.localScale, array);
				PhotonNetwork.SendOutgoingCommands();
			}
		}
	}

	public static PUNPrefabInstantiate GetCurrent()
	{
		return Get(SceneManager.GetActiveScene().buildIndex);
	}

	public static PUNPrefabInstantiate Get(int sceneIndex)
	{
		foreach (PUNPrefabInstantiate item in All)
		{
			if (item != null && item.SceneIndex == sceneIndex)
			{
				return item;
			}
		}
		return null;
	}

	public void SpawnOnNetwork(int containerIndex, Vector3 pos, Quaternion rot, Vector3 scale, int[] ids)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(Containers[containerIndex].Prefab, pos, rot);
		gameObject.transform.localScale = scale;
		PhotonView[] componentsInChildren = gameObject.GetComponentsInChildren<PhotonView>(true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].viewID = ids[i];
		}
	}
}
