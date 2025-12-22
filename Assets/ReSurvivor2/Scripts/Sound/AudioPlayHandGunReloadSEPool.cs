using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ハンドガンのリロードのSE
/// </summary>
public class AudioPlayHandGunReloadSEPool : MonoBehaviour
{
	[SerializeField] AudioClip audioClip;
	[SerializeField] AudioSource audioSource;

	private bool isReturned = false;

	private void OnEnable()
	{
		isReturned = false;
		CancelInvoke();
	}

	public void PlaySound()
	{
		if (audioSource == null || audioClip == null)
		{
			return;
		}

		isReturned = false;

		audioSource.PlayOneShot(audioClip);

		CancelInvoke();
		Invoke(nameof(ReturnToPool), audioClip.length);
	}

	private void ReturnToPool()
	{
		if (isReturned == true)
		{
			return;
		}

		isReturned = true;
		SoundManager.SingletonInstance.HandGunReloadSEPool.ReleaseGameObject(this.gameObject);
	}
}
