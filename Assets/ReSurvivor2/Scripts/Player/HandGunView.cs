using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Knife.Effects;

/// <summary>
/// ハンドガンビュー
/// MVPパターンのView担当
/// </summary>
public class HandGunView : MonoBehaviour
{
	[Header("ハンドガンモデル")]
	[Tooltip("キャラクターの手に持っているハンドガンのモデル")]
	[SerializeField] GameObject handGunModel;
	public GameObject GetHandGunModel => handGunModel;
	[Tooltip("キャラクターの体についているハンドガンのモデル")]
	[SerializeField] GameObject handGunModelBodyDecoration;
	public GameObject GetHandGunModelBodyDecoration => handGunModelBodyDecoration;
	[Tooltip("ハンドガンのマズルフラッシュの生成座標位置")]
	[SerializeField] Transform handGunMuzzleTransform;
	public Transform HandGunMuzzleTransform => handGunMuzzleTransform;
	[Tooltip("ハンドガンの薬莢の生成座標位置")]
	[SerializeField] Transform handGunBulletCasingTransform;
	[Tooltip("ハンドガンの硝煙の生成座標位置")]
	[SerializeField] Transform handGunAfterFireSmokeTransform;
	public Transform HandGunAfterFireSmokeTransform => handGunAfterFireSmokeTransform;
	//↓アセットストアのプログラム↓//
	[Tooltip("ハンドガンのマズルフラッシュと薬莢")]
	[SerializeField] ParticleGroupEmitter[] handGunShotEmitters;
	[Tooltip("ハンドガンの硝煙")]
	[SerializeField] ParticleGroupPlayer handGunAfterFireSmoke;
	//↑アセットストアのプログラム↑//

	/// <summary>
	/// 撃った時の見た目と音を出す（薬莢の音 → マズルフラッシュと薬莢 → 硝煙 → 射撃音の順）
	/// </summary>
	public void HandGunFire()
	{
		HandGunBulletCasingSE();
		HandGunMuzzleFlashAndShell();
		HandGunSmoke();
		HandGunFireSE();
	}

	/// <summary>
	/// マズルフラッシュのエフェクトと薬莢を出す（アセットストアで買ったコードをそのままもってきている）
	/// </summary>
	void HandGunMuzzleFlashAndShell()
	{
		if (handGunShotEmitters != null)
		{
			foreach (var effect in handGunShotEmitters)
			{
				effect.Emit(1);
			}
		}
	}

	/// <summary>
	/// 硝煙のエフェクトを出す（アセットストアで買ったコードをそのままもってきている）
	/// </summary>
	void HandGunSmoke()
	{
		if (handGunAfterFireSmoke != null)
		{
			handGunAfterFireSmoke.Play();
		}
	}

	/// <summary>
	/// ハンドガンの射撃SE
	/// </summary>
	void HandGunFireSE()
	{
		SoundManager.SingletonInstance.HandGunShootSEPool.GetGameObject(handGunMuzzleTransform);
	}

	/// <summary>
	/// ハンドガンのリロードSE（リロードを始めた時に鳴らす）
	/// </summary>
	public void HandGunReloadSE()
	{
		SoundManager.SingletonInstance.HandGunReloadSEPool.GetGameObject(handGunBulletCasingTransform);
	}

	/// <summary>
	/// ハンドガンの薬莢SE
	/// </summary>
	void HandGunBulletCasingSE()
	{
		SoundManager.SingletonInstance.HandGunBulletCasingSEPool.GetGameObject(handGunBulletCasingTransform);
	}
}
