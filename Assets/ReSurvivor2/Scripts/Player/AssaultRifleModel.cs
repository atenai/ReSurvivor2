using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Knife.Effects;

/// <summary>
/// アサルトライフルモデル
/// </summary>
public class AssaultRifleModel : GunModelBase
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
	public Transform AssaultRifleMuzzleTransform => assaultRifleMuzzleTransform;
	[Tooltip("アサルトライフルの薬莢の生成座標位置")]
	[SerializeField] Transform assaultRifleBulletCasingTransform;
	public Transform AssaultRifleBulletCasingTransform => assaultRifleBulletCasingTransform;
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
	/// マズルフラッシュのエフェクトと薬莢を出す（アセットストアで買ったコードをそのままもってきている）
	/// </summary>
	public void AssaultRifleMuzzleFlashAndShell()
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
	public void AssaultRifleSmoke()
	{
		if (assaultRifleAfterFireSmoke != null)
		{
			assaultRifleAfterFireSmoke.Play();
		}
	}

}
