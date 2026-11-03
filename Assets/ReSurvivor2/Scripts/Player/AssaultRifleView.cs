using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Knife.Effects;

/// <summary>
/// アサルトライフルビュー
/// MVPパターンのView担当
/// </summary>
public class AssaultRifleView : MonoBehaviour
{
	[Header("アサルトライフルモデル")]
	[Tooltip("キャラクターの手に持っているアサルトライフルのモデル")]
	[SerializeField] GameObject assaultRifleModel;
	public GameObject GetAssaultRifleModel => assaultRifleModel;
	[Tooltip("キャラクターの体についているアサルトライフルのモデル")]
	[SerializeField] GameObject assaultRifleModelBodyDecoration;
	public GameObject GetAssaultRifleModelBodyDecoration => assaultRifleModelBodyDecoration;
	[Tooltip("アサルトライフルのマズルフラッシュの生成座標位置")]
	[SerializeField] Transform assaultRifleMuzzleTransform;
	[Tooltip("アサルトライフルの薬莢の生成座標位置")]
	[SerializeField] Transform assaultRifleBulletCasingTransform;
	[Tooltip("アサルトライフルの硝煙の生成座標位置")]
	[SerializeField] Transform assaultRifleAfterFireSmokeTransform;
	public Transform AssaultRifleAfterFireSmokeTransform => assaultRifleAfterFireSmokeTransform;
	//↓アセットストアのプログラム↓//
	[Tooltip("アサルトライフルのマズルフラッシュと薬莢")]
	[SerializeField] ParticleGroupEmitter[] assaultRifleShotEmitters;
	[Tooltip("アサルトライフルの硝煙")]
	[SerializeField] ParticleGroupPlayer assaultRifleAfterFireSmoke;
	//↑アセットストアのプログラム↑//

	/// <summary>
	/// 撃った時の見た目と音を出す（薬莢の音 → マズルフラッシュと薬莢 → 硝煙 → 射撃音の順）
	/// </summary>
	public void AssaultRifleFire()
	{
		AssaultRifleBulletCasingSE();
		AssaultRifleMuzzleFlashAndShell();
		AssaultRifleSmoke();
		AssaultRifleFireSE();
	}

	/// <summary>
	/// マズルフラッシュのエフェクトと薬莢を出す（アセットストアで買ったコードをそのままもってきている）
	/// </summary>
	void AssaultRifleMuzzleFlashAndShell()
	{
		if (assaultRifleShotEmitters != null)
		{
			foreach (var effect in assaultRifleShotEmitters)
			{
				effect.Emit(1);
			}
		}
	}

	/// <summary>
	/// 硝煙のエフェクトを出す（アセットストアで買ったコードをそのままもってきている）
	/// </summary>
	void AssaultRifleSmoke()
	{
		if (assaultRifleAfterFireSmoke != null)
		{
			assaultRifleAfterFireSmoke.Play();
		}
	}

	/// <summary>
	/// アサルトライフルの射撃SE
	/// </summary>
	void AssaultRifleFireSE()
	{
		SoundManager.SingletonInstance.AssaultRifleShootSEPool.GetGameObject(assaultRifleMuzzleTransform);
	}

	/// <summary>
	/// アサルトライフルのリロードSE（リロードを始めた時に鳴らす）
	/// </summary>
	public void AssaultRifleReloadSE()
	{
		SoundManager.SingletonInstance.AssaultRifleReloadSEPool.GetGameObject(assaultRifleBulletCasingTransform);
	}

	/// <summary>
	/// アサルトライフルの薬莢SE
	/// </summary>
	void AssaultRifleBulletCasingSE()
	{
		SoundManager.SingletonInstance.AssaultRifleBulletCasingSEPool.GetGameObject(assaultRifleBulletCasingTransform);
	}
}
