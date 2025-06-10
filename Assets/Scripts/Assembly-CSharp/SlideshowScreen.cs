using Photon;
using UnityEngine;
using UnityEngine.UI;

public class SlideshowScreen : PunBehaviour
{
	[SerializeField]
	private Texture2D[] slideshow;

	[SerializeField]
	private Renderer screenRenderer;

	[SerializeField]
	private Button rightButton;

	[SerializeField]
	private Button leftButton;

	[SerializeField]
	private bool loop = true;

	private int slideIndex;

	private bool _isVisible;

	public bool IsVisible
	{
		get
		{
			return _isVisible;
		}
		private set
		{
			_isVisible = value;
			screenRenderer.gameObject.SetActive(_isVisible);
			leftButton.gameObject.SetActive(_isVisible);
			rightButton.gameObject.SetActive(_isVisible);
		}
	}

	private void Start()
	{
		IsVisible = false;
		UpdateMaterial();
		rightButton.GetComponentInParent<Canvas>().worldCamera = ViveControllerInput.Instance.ControllerCamera;
		leftButton.GetComponentInParent<Canvas>().worldCamera = ViveControllerInput.Instance.ControllerCamera;
	}

	public void Button_NextSlide()
	{
		slideIndex++;
		if (slideIndex >= slideshow.Length)
		{
			if (loop)
			{
				slideIndex %= slideshow.Length;
			}
			else
			{
				slideIndex = slideshow.Length - 1;
			}
		}
		base.photonView.RPC("RpcSetIndex", PhotonTargets.All, slideIndex);
	}

	public void Button_PrevSlide()
	{
		slideIndex--;
		if (slideIndex < 0)
		{
			if (loop)
			{
				slideIndex += slideshow.Length;
			}
			else
			{
				slideIndex = 0;
			}
		}
		base.photonView.RPC("RpcSetIndex", PhotonTargets.All, slideIndex);
	}

	public void SetVisibility(bool visible)
	{
		if (IsVisible != visible)
		{
			base.photonView.RPC("RpcSetVisibility", PhotonTargets.All, visible);
		}
	}

	private void UpdateMaterial()
	{
		screenRenderer.material.mainTexture = slideshow[slideIndex];
	}

	public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
	{
		base.OnPhotonPlayerConnected(newPlayer);
		if (base.hasAuthority)
		{
			base.photonView.RPC("RpcSetIndex", newPlayer, slideIndex);
			base.photonView.RPC("RpcSetVisibility", newPlayer, IsVisible);
		}
	}

	[PunRPC]
	public void RpcSetIndex(int currentIndex)
	{
		slideIndex = currentIndex;
		UpdateMaterial();
	}

	[PunRPC]
	public void RpcSetVisibility(bool visible)
	{
		IsVisible = visible;
	}
}
