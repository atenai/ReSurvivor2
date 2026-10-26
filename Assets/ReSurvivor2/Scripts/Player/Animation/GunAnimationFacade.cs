using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ガンアニメーションファサード
/// 銃ごとの、プレイヤーのアニメーションの設定（GunAnimation）をまとめて持つ
/// ※PlayerCharacterViewが持つ（銃のクラスからは使わない）
/// </summary>
[System.Serializable]
public class GunAnimationFacade
{
	[Tooltip("ハンドガンを持っている時のアニメーションの設定")]
	[SerializeField] GunAnimation handGunAnimation = new GunAnimation();
	[Tooltip("アサルトライフルを持っている時のアニメーションの設定")]
	[SerializeField] GunAnimation assaultRifleAnimation = new GunAnimation();
	[Tooltip("ショットガンを持っている時のアニメーションの設定")]
	[SerializeField] GunAnimation shotGunAnimation = new GunAnimation();

	/// <summary>
	/// 初期化（マガジンを銃に付けている時の親・位置・向きを覚えておく）
	/// </summary>
	public void Initialize()
	{
		handGunAnimation.Initialize();
		assaultRifleAnimation.Initialize();
		shotGunAnimation.Initialize();
	}

	/// <summary>
	/// 銃の種類から、その銃を持っている時のアニメーションの設定を探す
	/// </summary>
	/// <param name="gunTYPE">銃の種類</param>
	/// <returns>アニメーションの設定（見つからなかった場合はnull）</returns>
	public GunAnimation FindGunAnimation(EnumManager.GunTYPE gunTYPE)
	{
		switch (gunTYPE)
		{
			case EnumManager.GunTYPE.HandGun:
				return handGunAnimation;
			case EnumManager.GunTYPE.AssaultRifle:
				return assaultRifleAnimation;
			case EnumManager.GunTYPE.ShotGun:
				return shotGunAnimation;
		}
		return null;
	}

	/// <summary>
	/// 銃ごとに、リロード中のマガジンを動かす（抜き取る → 左手で持つ → 差し込む）
	/// ※アニメーションで手が動いた後に動かす
	/// </summary>
	/// <param name="gunModelFacade">銃</param>
	/// <param name="reloadAnimationTime">リロードアニメーションの再生位置</param>
	/// <param name="isReloadAnimationForward">リロードアニメーションを前へ進めているか？</param>
	/// <param name="leftHand">左手の骨</param>
	public void UpdateReloadMagazines(GunModelFacade gunModelFacade, float reloadAnimationTime, bool isReloadAnimationForward, Transform leftHand)
	{
		handGunAnimation.UpdateReloadMagazine(gunModelFacade.FindGunBase(EnumManager.GunTYPE.HandGun).IsReloadTimeActive, reloadAnimationTime, isReloadAnimationForward, leftHand);
		assaultRifleAnimation.UpdateReloadMagazine(gunModelFacade.FindGunBase(EnumManager.GunTYPE.AssaultRifle).IsReloadTimeActive, reloadAnimationTime, isReloadAnimationForward, leftHand);
		shotGunAnimation.UpdateReloadMagazine(gunModelFacade.FindGunBase(EnumManager.GunTYPE.ShotGun).IsReloadTimeActive, reloadAnimationTime, isReloadAnimationForward, leftHand);
	}
}
