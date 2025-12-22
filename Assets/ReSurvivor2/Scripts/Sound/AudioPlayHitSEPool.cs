using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlayHitSEPool : MonoBehaviour
{
	[SerializeField] AudioClip audioClip;
	[SerializeField] AudioSource audioSource;

	private bool isReturned = false;

	private void OnEnable()
	{
		isReturned = false;
		CancelInvoke();
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
	public void PlaySound()
	{
		if (audioSource == null || audioClip == null)
		{
			return;
		}

		isReturned = false;

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
		SoundManager.SingletonInstance.HitSEPool.ReleaseGameObject(this.gameObject);
	}
}
