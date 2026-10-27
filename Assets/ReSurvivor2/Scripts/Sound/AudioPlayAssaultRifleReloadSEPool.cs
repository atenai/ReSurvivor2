using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// アサルトライフルのリロードのSE
/// </summary>
public class AudioPlayAssaultRifleReloadSEPool : MonoBehaviour
{
	[SerializeField] AudioClip audioClip;
	[SerializeField] AudioSource audioSource;

	private bool isReturned = false;
	[Tooltip("鳴っている間についていく位置（銃。動きながら撃ったりリロードしても、音がその場に置いていかれないようにする）")]
	Transform followTarget = null;

	private void OnEnable()
	{
		isReturned = false;
		followTarget = null;
		CancelInvoke();
	}

	/// <summary>
	/// 鳴っている間、ついていく位置（銃）に合わせて動かす
	/// </summary>
	private void LateUpdate()
	{
		if (followTarget == null)
		{
			return;
		}

		transform.position = followTarget.position;
	}

	/// <summary>
	/// サウンドを再生
	/// </summary>
	/// <remarks>
	/// 
	/// PlaySound()
	///    ↓
	/// CancelInvoke()   ← 前回の予約を必ず消す
	///    ↓
	/// Invoke(ReturnToPool, audioClip.length)
	///    ↓
	/// （SE再生中）
	///    ↓
	/// ReturnToPool() ← ちょうど鳴り終わった後に1回だけ呼ばれる
	/// 
	/// </remarks>
	/// <param name="followTarget">鳴っている間についていく位置（銃）</param>
	public void PlaySound(Transform followTarget)
	{
		if (audioSource == null || audioClip == null)
		{
			return;
		}

		isReturned = false;
		this.followTarget = followTarget;

		audioSource.PlayOneShot(audioClip);

		//このコンポーネントに予約されているInvokeを全部キャンセルするという命令
		CancelInvoke();
		//これは「audioClip.length秒後にReturnToPool()を1回だけ呼び出す」というUnityの予約実行
		//つまり「このSEが鳴り終わったら、GameObjectをプールに返す」という意味
		//nameof(ReturnToPool)は「メソッド名変更時にコンパイルエラーになる」「文字列ミスが起きない」「リファクタ耐性が高い」現代Unityの正解書き方
		Invoke(nameof(ReturnToPool), audioClip.length);
	}

	private void ReturnToPool()
	{
		if (isReturned == true)
		{
			return;
		}

		isReturned = true;
		SoundManager.SingletonInstance.AssaultRifleReloadSEPool.ReleaseGameObject(this.gameObject);
	}
}
