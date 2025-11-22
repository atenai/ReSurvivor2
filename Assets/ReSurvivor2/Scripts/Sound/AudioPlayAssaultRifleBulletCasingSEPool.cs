using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// アサルトライフルの薬莢のSE
/// </summary>
public class AudioPlayAssaultRifleBulletCasingSEPool : MonoBehaviour
{
	[SerializeField] AudioClip audioClip;
	[SerializeField] AudioSource audioSource;

	public void PlaySound()
	{
		audioSource.PlayOneShot(audioClip);
	}

	void Update()
	{
		//音声が鳴り終えたら
		if (audioSource.isPlaying == false)
		{
			SoundManager.SingletonInstance.AssaultRifleBulletCasingSEPool.ReleaseGameObject(this.gameObject);
		}
	}
}
