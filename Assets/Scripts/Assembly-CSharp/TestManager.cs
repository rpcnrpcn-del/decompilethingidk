using System.Collections;
using UnityEngine;

public class TestManager : SingletonMonoBehaviour<TestManager>
{
	public enum TestMode
	{
		MoveBetweenActivities = 0,
		WaintInActivity = 1
	}

	public TestMode Mode;

	public string activityName = "lockerroom";

	private int defaultCullingMask = -1;

	private bool _mainCameraCulled;

	private bool mainCameraCulled
	{
		get
		{
			return _mainCameraCulled;
		}
		set
		{
			_mainCameraCulled = value;
			Camera component = SingletonMonoBehaviour<CameraRig>.Instance.CameraIO.GetComponent<Camera>();
			if (defaultCullingMask < 0)
			{
				defaultCullingMask = component.cullingMask;
			}
			component.cullingMask = ((!_mainCameraCulled) ? defaultCullingMask : 0);
		}
	}

	private void Awake()
	{
		Object.Destroy(base.gameObject);
	}

	private void Start()
	{
		QualitySettings.SetQualityLevel(0);
		mainCameraCulled = true;
		StartCoroutine(RunTestMode());
	}

	private IEnumerator RunTestMode()
	{
		OutfitManager.Instance.RandomizePlayerOutfit();
		if (Mode == TestMode.MoveBetweenActivities)
		{
			while (true)
			{
				if (Player.LocalPlayer == null)
				{
					yield return null;
				}
				else
				{
					RandomlyMovePlayer();
					float timer = Random.Range(15, 60);
					while (timer > 0f)
					{
						timer -= Time.deltaTime;
						if (Random.value < 0.01f)
						{
							RandomlyMovePlayer();
						}
						yield return null;
					}
					while (RecRoomSceneManager.Instance == null)
					{
						yield return null;
					}
					RecRoomSceneManager.Instance.SwitchActivity((!(Random.value < 0.25f)) ? null : "lockerroom", false);
				}
			}
		}
		if (Mode != TestMode.WaintInActivity)
		{
			yield break;
		}
		while (Player.LocalPlayer == null)
		{
			yield return null;
		}
		RecRoomSceneManager.Instance.SwitchActivity(activityName, false);
		while (true)
		{
			RandomlyMovePlayer();
			float timer2 = Random.Range(25, 60);
			while (timer2 > 0f)
			{
				timer2 -= Time.deltaTime;
				if (Random.value < 0.01f)
				{
					RandomlyMovePlayer();
				}
				yield return null;
			}
		}
	}

	private void RandomlyMovePlayer()
	{
		if (Player.LocalPlayer != null)
		{
			Vector3 position = SingletonMonoBehaviour<CameraRig>.Instance.CameraIO.transform.position;
			position.y = Random.Range(1f, 2f);
			SingletonMonoBehaviour<CameraRig>.Instance.CameraIO.transform.position = position;
			Vector3 localPosition = SingletonMonoBehaviour<CameraRig>.Instance.CameraIO.transform.localPosition;
			localPosition.x = Random.Range(-4f, 4f);
			localPosition.z = Random.Range(-4f, 4f);
			SingletonMonoBehaviour<CameraRig>.Instance.CameraIO.transform.localPosition = localPosition;
		}
	}

	private void OnGUI()
	{
		GUILayout.BeginArea(new Rect(300f, 300f, 300f, 300f));
		GUILayout.BeginHorizontal();
		GUILayout.Box("Scene : " + SceneManagerHelper.ActiveSceneName);
		if (GUILayout.Button((!mainCameraCulled) ? "Stop render" : "Start render"))
		{
			mainCameraCulled = !mainCameraCulled;
		}
		GUILayout.EndHorizontal();
		GUILayout.EndArea();
	}
}
