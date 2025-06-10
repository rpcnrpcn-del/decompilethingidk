using UnityEngine;
using UnityEngine.SceneManagement;

public class AnimateInOutManager : MonoBehaviour
{
	public ParticleSystem DefaultEffectPrefab;

	public static bool SuppressAnimations = true;

	public static AnimateInOutManager Instance { get; private set; }

	private void Awake()
	{
		Instance = this;
		SceneManager.sceneUnloaded += SceneManager_sceneUnloaded;
	}

	private void OnApplicationQuit()
	{
		SuppressAnimations = true;
	}

	private void SceneManager_sceneUnloaded(Scene scene)
	{
		SuppressAnimations = true;
		for (int i = 0; i < base.transform.childCount; i++)
		{
			Object.Destroy(base.transform.GetChild(i).gameObject);
		}
	}
}
