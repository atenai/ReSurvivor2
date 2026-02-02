using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Knife.Effects;

/// <summary>
/// ハンドガンモデル
/// </summary>
public class HandGunModel : MonoBehaviour
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
	public Transform HandGunBulletCasingTransform => handGunBulletCasingTransform;
	[Tooltip("ハンドガンの硝煙の生成座標位置")]
	[SerializeField] Transform handGunAfterFireSmokeTransform;
	public Transform HandGunAfterFireSmokeTransform => handGunAfterFireSmokeTransform;
	//↓アセットストアのプログラム↓//
	[Tooltip("ハンドガンのマズルフラッシュと薬莢")]
	[SerializeField] ParticleGroupEmitter[] handGunShotEmitters;
	[Tooltip("ハンドガンの硝煙")]
	[SerializeField] ParticleGroupPlayer handGunAfterFireSmoke;
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
	public void HandGunMuzzleFlashAndShell()
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
	public void HandGunSmoke()
	{
		if (handGunAfterFireSmoke != null)
		{
			handGunAfterFireSmoke.Play();
		}
	}
}
