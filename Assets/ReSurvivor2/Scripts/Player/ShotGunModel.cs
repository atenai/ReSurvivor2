using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Knife.Effects;

/// <summary>
/// ショットガンモデル
/// </summary>
public class ShotGunModel : MonoBehaviour
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
	public Transform ShotGunMuzzleTransform => shotGunMuzzleTransform;
	[Tooltip("ショットガンの薬莢の生成座標位置")]
	[SerializeField] Transform shotGunBulletCasingTransform;
	public Transform ShotGunBulletCasingTransform => shotGunBulletCasingTransform;
	[Tooltip("ショットガンの硝煙の生成座標位置")]
	[SerializeField] Transform shotGunAfterFireSmokeTransform;
	public Transform ShotGunAfterFireSmokeTransform => shotGunAfterFireSmokeTransform;
	//↓アセットストアのプログラム↓//
	[Tooltip("ショットガンのマズルフラッシュと薬莢")]
	[SerializeField] ParticleGroupEmitter[] shotGunShotEmitters;
	[Tooltip("ショットガンの硝煙")]
	[SerializeField] ParticleGroupPlayer shotGunAfterFireSmoke;
	//↑アセットストアのプログラム↑//

	void Start()
	{

	}

	void Update()
	{
		if (ScreenUI.SingletonInstance.IsPause == true)
		{
			return;
		}

		if (ScreenUI.SingletonInstance.IsComputerMenuActive == true)
		{
			return;
		}

		//↑ロード中に動かせる処理
		if (InGameManager.SingletonInstance.IsGamePlayReady == false)
		{
			return;
		}
		//↓ロード中に動かせない処理
	}

	/// <summary>
	/// マズルフラッシュのエフェクトと薬莢を出す（アセットストアで買ったコードをそのままもってきている）
	/// </summary>
	public void ShotGunMuzzleFlashAndShell()
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
	public void ShotGunSmoke()
	{
		if (shotGunAfterFireSmoke != null)
		{
			shotGunAfterFireSmoke.Play();
		}
	}
}
