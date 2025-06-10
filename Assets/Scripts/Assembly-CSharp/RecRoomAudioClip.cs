using System;
using UnityEngine;

[Serializable]
public class RecRoomAudioClip
{
	public AudioClip audioClip;

	[Range(0f, 1f)]
	public float volume = 1f;

	[Range(0f, 2f)]
	public float pitchVariation;
}
