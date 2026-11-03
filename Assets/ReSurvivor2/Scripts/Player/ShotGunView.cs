using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Knife.Effects;

/// <summary>
/// ショットガンビュー
/// MVPパターンのView担当
/// </summary>
public class ShotGunView : MonoBehaviour
{
	[Header("ショットガンモデル")]
	[Tooltip("キャラクターの手に持っているショットガンのモデル")]
	[SerializeField] GameObject shotGunModel;
	public GameObject GetShotGunModel => shotGunModel;
	[Tooltip("キャラクターの体についているショットガンのモデル")]
	[SerializeField] GameObject shotGunModelBodyDecoration;
	public GameObject GetShotGunModelBodyDecoration => shotGunModelBodyDecoration;
	[Tooltip("ショットガンのマズルフラッシュの生成座標位置")]
	[SerializeField] Transform shotGunMuzzleTransform;
	[Tooltip("ショットガンの薬莢の生成座標位置")]
	[SerializeField] Transform shotGunBulletCasingTransform;
	[Tooltip("ショットガンの硝煙の生成座標位置")]
	[SerializeField] Transform shotGunAfterFireSmokeTransform;
	public Transform ShotGunAfterFireSmokeTransform => shotGunAfterFireSmokeTransform;
	//↓アセットストアのプログラム↓//
	[Tooltip("ショットガンのマズルフラッシュと薬莢")]
	[SerializeField] ParticleGroupEmitter[] shotGunShotEmitters;
	[Tooltip("ショットガンの硝煙")]
	[SerializeField] ParticleGroupPlayer shotGunAfterFireSmoke;
	//↑アセットストアのプログラム↑//

	/// <summary>
	/// 撃った時の見た目と音を出す（薬莢の音 → マズルフラッシュと薬莢 → 硝煙 → 射撃音の順）
	/// </summary>
	public void ShotGunFire()
	{
		ShotGunBulletCasingSE();
		ShotGunMuzzleFlashAndShell();
		ShotGunSmoke();
		ShotGunFireSE();
	}

	/// <summary>
	/// マズルフラッシュのエフェクトと薬莢を出す（アセットストアで買ったコードをそのままもってきている）
	/// </summary>
	void ShotGunMuzzleFlashAndShell()
	{
		if (shotGunShotEmitters != null)
		{
			foreach (var effect in shotGunShotEmitters)
			{
				effect.Emit(1);
			}
		}
	}

	/// <summary>
	/// 硝煙のエフェクトを出す（アセットストアで買ったコードをそのままもってきている）
	/// </summary>
	void ShotGunSmoke()
	{
		if (shotGunAfterFireSmoke != null)
		{
			shotGunAfterFireSmoke.Play();
		}
	}

	/// <summary>
	/// ショットガンの射撃SE
	/// </summary>
	void ShotGunFireSE()
	{
		SoundManager.SingletonInstance.ShotGunShootSEPool.GetGameObject(shotGunMuzzleTransform);
	}

	/// <summary>
	/// ショットガンのリロードSE（空の銃に弾を込め終わった時に、弾を送り込む音として鳴らす）
	/// </summary>
	public void ShotGunReloadSE()
	{
		SoundManager.SingletonInstance.ShotGunReloadSEPool.GetGameObject(shotGunBulletCasingTransform);
	}

	/// <summary>
	/// ショットガンの弾込めSE（弾を1発込めるたびに鳴らす）
	/// </summary>
	public void ShotGunShellInsertSE()
	{
		SoundManager.SingletonInstance.ShotGunShellInsertSEPool.GetGameObject(shotGunBulletCasingTransform);
	}

	/// <summary>
	/// ショットガンの薬莢SE
	/// </summary>
	void ShotGunBulletCasingSE()
	{
		SoundManager.SingletonInstance.ShotGunBulletCasingSEPool.GetGameObject(shotGunBulletCasingTransform);
	}
}
