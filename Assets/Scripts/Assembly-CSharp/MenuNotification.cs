using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuNotification : MonoBehaviour
{
	public static List<MenuNotification> All = new List<MenuNotification>();

	[SerializeField]
	private ScreenSpaceNotification.NotificationText title;

	[SerializeField]
	private ScreenSpaceNotification.NotificationText subtitle;

	[SerializeField]
	private FloatValueCurve alphaEaseInCurve;

	private Coroutine playCoroutine;

	public bool IsPlaying
	{
		get
		{
			return playCoroutine != null;
		}
	}

	public FollowTarget FollowTarget { get; private set; }

	private void Awake()
	{
		All.Add(this);
		title.Alpha = 0f;
		subtitle.Alpha = 0f;
		FollowTarget = GetComponent<FollowTarget>();
	}

	private void Start()
	{
		base.gameObject.SetActive(false);
	}

	private void OnDestroy()
	{
		All.Remove(this);
	}

	public void Play(string titleText, float duration = 5f, string subtitleText = null, bool followPlayer = true, bool billboard = false)
	{
		Stop();
		base.gameObject.SetActive(true);
		playCoroutine = StartCoroutine(PlayCoroutine(titleText, subtitleText, duration, followPlayer, billboard));
	}

	public void Stop()
	{
		if (playCoroutine != null)
		{
			StopCoroutine(playCoroutine);
			playCoroutine = null;
			base.gameObject.SetActive(false);
		}
	}

	private IEnumerator PlayCoroutine(string titleText, string subtitleText, float duration, bool followPlayer, bool billboard)
	{
		title.Text.text = titleText;
		subtitle.Text.text = subtitleText;
		Vector3 up = Vector3.up;
		Vector3 forward = base.transform.position - SingletonMonoBehaviour<CameraRig>.Instance.CameraIO.transform.position;
		if (!billboard)
		{
			forward = Vector3.ProjectOnPlane(forward, up).normalized;
		}
		if (FollowTarget == null || !followPlayer)
		{
			base.transform.rotation = Quaternion.LookRotation(forward, up);
		}
		float timer = 0f;
		float easeInDuration = alphaEaseInCurve.Duration;
		float easeOutDuration = easeInDuration;
		float waitDuration = Mathf.Max(0f, duration - easeInDuration - easeOutDuration);
		while (timer < easeInDuration)
		{
			float t = timer / easeInDuration;
			if (FollowTarget == null || !followPlayer)
			{
				base.transform.position += up * 0.001f;
			}
			title.Alpha = alphaEaseInCurve.Evaluate(t);
			timer += Time.deltaTime;
			yield return new WaitForSeconds(Time.deltaTime);
		}
		title.Alpha = 1f;
		subtitle.Alpha = 1f;
		yield return new WaitForSeconds(waitDuration);
		timer = 0f;
		while (timer < easeOutDuration)
		{
			float t2 = 1f - timer / alphaEaseInCurve.Duration;
			if (FollowTarget == null || !followPlayer)
			{
				base.transform.position -= up * 0.001f;
			}
			title.Alpha = alphaEaseInCurve.Evaluate(t2);
			subtitle.Alpha = alphaEaseInCurve.Evaluate(t2);
			timer += Time.deltaTime;
			yield return new WaitForSeconds(Time.deltaTime);
		}
		title.Alpha = 0f;
		subtitle.Alpha = 0f;
		playCoroutine = null;
	}

	public static bool PlayNext(string titleText, float duration = 5f, string subtitleText = null, bool followPlayer = true, Vector3? worldSpacePosition = null, float scaleOverride = 1f, bool billboard = false)
	{
		MenuNotification nextAvailable = GetNextAvailable();
		if (nextAvailable != null)
		{
			if (followPlayer && nextAvailable.FollowTarget != null)
			{
				nextAvailable.FollowTarget.Target = ((!(Player.LocalPlayer != null)) ? SingletonMonoBehaviour<CameraRig>.Instance.CameraIO.transform : Player.LocalPlayer.Head.transform);
				nextAvailable.FollowTarget.ForceUpdate();
			}
			else
			{
				nextAvailable.transform.position = ((!worldSpacePosition.HasValue) ? (Player.LocalPlayer.Head.transform.position + Player.LocalPlayer.Head.transform.forward) : worldSpacePosition.Value);
				Vector3 vector = nextAvailable.transform.position - SingletonMonoBehaviour<CameraRig>.Instance.CameraIO.transform.position;
				if (!billboard)
				{
					vector = Vector3.ProjectOnPlane(vector, Vector3.up).normalized;
				}
				nextAvailable.transform.rotation = Quaternion.LookRotation(vector, Vector3.up);
			}
			nextAvailable.title.Text.transform.localScale = Vector3.one * scaleOverride;
			nextAvailable.subtitle.Text.transform.localScale = Vector3.one * scaleOverride;
			nextAvailable.Play(titleText, duration, subtitleText, followPlayer, billboard);
			return true;
		}
		return false;
	}

	public static MenuNotification GetNextAvailable()
	{
		foreach (MenuNotification item in All)
		{
			if (!item.IsPlaying)
			{
				return item;
			}
		}
		return null;
	}
}
