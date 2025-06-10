using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PanelManager : MonoBehaviour
{
	public Animator initiallyOpen;

	private int m_OpenParameterId;

	private Animator m_Open;

	private GameObject m_PreviouslySelected;

	private const string k_OpenTransitionName = "Open";

	private const string k_ClosedStateName = "Closed";

	public void OnEnable()
	{
		m_OpenParameterId = Animator.StringToHash("Open");
		if (!(initiallyOpen == null))
		{
			OpenPanel(initiallyOpen);
		}
	}

	public void OpenPanel(Animator anim)
	{
		if (!(m_Open == anim))
		{
			anim.gameObject.SetActive(true);
			GameObject currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
			anim.transform.SetAsLastSibling();
			CloseCurrent();
			m_PreviouslySelected = currentSelectedGameObject;
			m_Open = anim;
			m_Open.SetBool(m_OpenParameterId, true);
			GameObject selected = FindFirstEnabledSelectable(anim.gameObject);
			SetSelected(selected);
		}
	}

	private static GameObject FindFirstEnabledSelectable(GameObject gameObject)
	{
		GameObject result = null;
		Selectable[] componentsInChildren = gameObject.GetComponentsInChildren<Selectable>(true);
		Selectable[] array = componentsInChildren;
		foreach (Selectable selectable in array)
		{
			if (selectable.IsActive() && selectable.IsInteractable())
			{
				result = selectable.gameObject;
				break;
			}
		}
		return result;
	}

	public void CloseCurrent()
	{
		if (!(m_Open == null))
		{
			m_Open.SetBool(m_OpenParameterId, false);
			SetSelected(m_PreviouslySelected);
			StartCoroutine(DisablePanelDeleyed(m_Open));
			m_Open = null;
		}
	}

	private IEnumerator DisablePanelDeleyed(Animator anim)
	{
		bool closedStateReached = false;
		bool wantToClose = true;
		while (!closedStateReached && wantToClose)
		{
			if (!anim.IsInTransition(0))
			{
				closedStateReached = anim.GetCurrentAnimatorStateInfo(0).IsName("Closed");
			}
			wantToClose = !anim.GetBool(m_OpenParameterId);
			yield return new WaitForEndOfFrame();
		}
		if (wantToClose)
		{
			anim.gameObject.SetActive(false);
		}
	}

	private void SetSelected(GameObject go)
	{
		EventSystem.current.SetSelectedGameObject(go);
		StandaloneInputModule standaloneInputModule = EventSystem.current.currentInputModule as StandaloneInputModule;
		if (!(standaloneInputModule != null))
		{
			EventSystem.current.SetSelectedGameObject(null);
		}
	}
}
