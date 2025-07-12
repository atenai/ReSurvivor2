using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlay : MonoBehaviour
{
	[SerializeField] AudioClip audioClip;
	[SerializeField] AudioSource audioSource;
	bool isAudioEnd = false;

	void Start()
	{
		audioSource.PlayOneShot(audioClip);
		isAudioEnd = true;
	}

	void Update()
	{
		//音声が鳴り終えたら
		if (audioSource.isPlaying == false && isAudioEnd == true)
		{
			Destroy(this.gameObject);
		}
	}
}
