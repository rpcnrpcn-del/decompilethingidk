using UnityEngine;

public class PlatformManagerBootstrapper : MonoBehaviour
{
	[Header("VR SDK Managers")]
	[SerializeField]
	private PlatformManager steamPlatformPrefab;

	[SerializeField]
	private PlatformManager oculusPlatformPrefab;

	private void Awake()
	{
		Object.Instantiate(steamPlatformPrefab, base.transform);
		Camera.main.gameObject.AddComponent<SteamVR_Camera>();
	}
}
