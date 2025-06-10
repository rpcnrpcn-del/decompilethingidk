using UnityEngine;

public class Radio : Tool
{
	[Header("Audio")]
	[SerializeField]
	private AudioSource audioSource;

	[SerializeField]
	private RecRoomAudioClip[] tracks;

	[SerializeField]
	private bool randomInitialTrack;

	private int currentTrackNumber;

	protected override void Start()
	{
		base.Start();
		if (base.hasAuthority)
		{
			if (randomInitialTrack)
			{
				currentTrackNumber = -1;
				PlayRandomTrack(true);
			}
			else
			{
				base.photonView.RPC("RpcRadioPlayTrack", PhotonTargets.All, 0, 0f);
			}
		}
	}

	public override void OnInputDown()
	{
		int num = (currentTrackNumber + 1) % tracks.Length;
		base.photonView.RPC("RpcRadioPlayTrack", PhotonTargets.All, num, 0f);
	}

	public void Update()
	{
		if (!audioSource.isPlaying && base.hasAuthority)
		{
			PlayRandomTrack();
		}
	}

	private void PlayRandomTrack(bool randomTrackPosition = false)
	{
		int num = Random.Range(0, tracks.Length);
		if (num == currentTrackNumber)
		{
			num++;
			if (num > tracks.Length - 1)
			{
				num = 0;
			}
		}
		float num2 = 0f;
		if (randomTrackPosition)
		{
			num2 = Random.Range(0f, tracks[num].audioClip.length * 0.9f);
		}
		base.photonView.RPC("RpcRadioPlayTrack", PhotonTargets.All, num, num2);
	}

	private void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
	{
		if (base.hasAuthority && audioSource.isPlaying)
		{
			base.photonView.RPC("RpcRadioPlayTrack", PhotonTargets.All, currentTrackNumber, audioSource.time);
		}
	}

	[PunRPC]
	public void RpcRadioPlayTrack(int trackNumber, float time)
	{
		if (trackNumber < tracks.Length && tracks[trackNumber] != null)
		{
			audioSource.Stop();
			audioSource.clip = tracks[trackNumber].audioClip;
			audioSource.Play();
			audioSource.time = time;
			currentTrackNumber = trackNumber;
		}
	}
}
