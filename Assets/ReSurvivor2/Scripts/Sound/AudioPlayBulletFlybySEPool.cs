using UnityEngine;

/// <summary>
/// 弾がかすめた時の風切り音（1回分）
/// 複数の音からランダムに1つ選び、少しだけ音の高さを変えて鳴らす
/// </summary>
public class AudioPlayBulletFlybySEPool : MonoBehaviour
{
	[Tooltip("風切り音（この中からランダムに鳴らす）")]
	[SerializeField] AudioClip[] audioClips;
	[SerializeField] AudioSource audioSource;
	[Tooltip("音の高さのばらつき（BasePitch ± この値）")]
	[SerializeField] float pitchRandom = 0.06f;

	[Tooltip("基準の音の高さ（1.0 = 元の高さ）")]
	const float BasePitch = 1.0f;
	[Tooltip("鳴り終わる時間を計算する時の、音の高さの下限（0 で割らないようにする）")]
	const float MinPitch = 0.01f;

	private bool isReturned = false;

	private void OnEnable()
	{
		isReturned = false;
		CancelInvoke();
	}

	/// <summary>
	/// サウンドを再生
	/// </summary>
	public void PlaySound()
	{
		if (audioSource == null || audioClips == null || audioClips.Length == 0)
		{
			ReturnToPool();
			return;
		}

		AudioClip audioClip = audioClips[Random.Range(0, audioClips.Length)];
		if (audioClip == null)
		{
			ReturnToPool();
			return;
		}

		isReturned = false;
		audioSource.pitch = BasePitch + Random.Range(-pitchRandom, pitchRandom);
		audioSource.PlayOneShot(audioClip);
		//前回の予約を消してから、鳴り終わった後にプールへ返す予約をする
		CancelInvoke();
		Invoke(nameof(ReturnToPool), audioClip.length / Mathf.Max(audioSource.pitch, MinPitch));
	}

	private void ReturnToPool()
	{
		if (isReturned == true)
		{
			return;
		}

		isReturned = true;
		if (SoundManager.SingletonInstance != null && SoundManager.SingletonInstance.BulletFlybySEPool != null)
		{
			SoundManager.SingletonInstance.BulletFlybySEPool.ReleaseGameObject(this.gameObject);
		}
	}
}
