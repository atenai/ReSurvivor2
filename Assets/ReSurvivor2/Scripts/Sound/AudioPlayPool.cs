using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlayPool : MonoBehaviour
{
	[SerializeField] AudioClip audioClip;
	[SerializeField] AudioSource audioSource;
	//bool isAudioEnd = false;

	void OnEnable()
	{
		//isAudioEnd = true;
		audioSource.PlayOneShot(audioClip);
	}

	void Start()
	{
		//isAudioEnd = true;
		audioSource.PlayOneShot(audioClip);
	}

	void Update()
	{
		//音声が鳴り終えたら
		if (audioSource.isPlaying == false /* && isAudioEnd == true */)
		{
			//isAudioEnd = false;
			SoundManager.SingletonInstance.ReleaseGameObject(this.gameObject);
		}
	}
}
