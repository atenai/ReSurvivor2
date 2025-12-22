using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// アサルトライフルの射撃のSE
/// </summary>
public class AudioPlayAssaultRifleShootSEPool : MonoBehaviour
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
		SoundManager.SingletonInstance.AssaultRifleShootSEPool.ReleaseGameObject(this.gameObject);
	}
}
