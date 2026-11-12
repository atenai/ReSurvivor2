using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ガンアニメーションファサード
/// 銃のアニメーション（銃ごとの設定・リロード・武器の切り替え・ハンドガンへの持ち替え）を管理する窓口
/// ※PlayerCharacterViewが持つ（銃のクラスからは使わない）。銃の状態は、IReadOnlyGunModelから読む
///   プレイヤーの体のアニメーション（移動・構え・地雷を置く・手のIK）は、PlayerAnimationFacadeが管理する。手のIKは、ここで求めた手の位置・向きを使う
/// ※リロード・武器の切り替えの手と銃の動きは、アニメーションクリップに焼き込んであるので、ここではアニメーターのパラメーターを渡すだけ
///   （銃は右手の骨の子の目印（GunPose）の子にしてあり、ショットガンのリロード・武器の切り替えのアニメーションクリップが目印を動かす）
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
	[Tooltip("リロードのアニメーション")]
	[SerializeField] ReloadAnimation reloadAnimation = new ReloadAnimation();
	[Tooltip("武器を切り替える時のアニメーション（今持っている銃を体にしまってから、切り替える先の銃を取り出す）")]
	readonly WeaponSwitchAnimation weaponSwitchAnimation = new WeaponSwitchAnimation();
	[Tooltip("ハンドガンへの持ち替えのアニメーション")]
	readonly SideArmAnimation sideArmAnimation = new SideArmAnimation();
	public SideArmAnimation SideArmAnimation => sideArmAnimation;

	/// <summary>
	/// 初期化（マガジンを銃に付けている時の親・位置・向きを覚えておき、ハンドガンへの持ち替えの準備をする）
	/// </summary>
	/// <param name="animator">プレイヤーのアニメーター</param>
	/// <param name="gunViewFacade">ガンビューファサード</param>
	/// <param name="leftHandAnimationPos">アニメーションどおりの左手の位置・向きをIK用の向きで取り出すための目印</param>
	public void Initialize(Animator animator, GunViewFacade gunViewFacade, Transform leftHandAnimationPos)
	{
		handGunAnimation.Initialize();
		assaultRifleAnimation.Initialize();
		shotGunAnimation.Initialize();
		sideArmAnimation.Initialize(gunViewFacade.HandGunView, leftHandAnimationPos, animator);
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
	/// 銃の状態に合わせて、銃のアニメーション（リロード・武器の切り替え）を動かす（銃の更新処理の後に呼ぶ）
	/// ※今持っている銃の種類も渡す（地雷を置く・武器を切り替えるアニメーションで、銃ごとのアニメーションクリップを選ぶ）
	/// </summary>
	/// <param name="animator">プレイヤーのアニメーター</param>
	/// <param name="gunState">銃の状態を読むための窓口</param>
	public void UpdateGunAnimation(Animator animator, IReadOnlyGunModel gunState)
	{
		animator.SetFloat(PlayerAnimatorParameter.GunType, (float)gunState.GetGunTYPE);
		reloadAnimation.UpdateAnimation(animator, gunState, shotGunAnimation);
		weaponSwitchAnimation.UpdateAnimation(animator, gunState);
	}

	/// <summary>
	/// ハンドガンへの持ち替えの、手を動かす割合を求める（アニメーターのIKの処理（OnAnimatorIK）の中で、手のIKの前に呼ぶ）
	/// </summary>
	/// <param name="gunState">銃の状態を読むための窓口</param>
	/// <param name="spineAimRotation">前のフレームで、構えに合わせて脊椎ボーンを曲げた回転（ワールドの向き）</param>
	/// <param name="spinePosition">前のフレームで、構えに合わせて曲げた脊椎ボーンの位置</param>
	public void UpdateWeights(IReadOnlyGunModel gunState, Quaternion spineAimRotation, Vector3 spinePosition)
	{
		GunAnimation sideArmMainGunAnimation = gunState.IsSideArmActive == true ? FindGunAnimation(gunState.SideArmMainGunTYPE) : null;
		sideArmAnimation.UpdateWeights(gunState.SideArmPhase, gunState.SideArmPhaseProgress, gunState.SideArmHolsterReachRate, spineAimRotation, spinePosition, sideArmMainGunAnimation);
	}

	/// <summary>
	/// リロード中のマガジン・ハンドガンに持ち替える時の銃のモデルを動かす（アニメーションで手が動いた後に呼ぶ）
	/// </summary>
	/// <param name="gunState">銃の状態を読むための窓口</param>
	/// <param name="gunViewFacade">ガンビューファサード</param>
	/// <param name="leftHand">左手の骨</param>
	public void UpdateModels(IReadOnlyGunModel gunState, GunViewFacade gunViewFacade, Transform leftHand)
	{
		//リロード中のマガジンを、銃ごとに動かす（抜き取る → 左手で持つ → 差し込む）
		float reloadAnimationTime = reloadAnimation.ReloadAnimationTime;
		bool isReloadAnimationForward = reloadAnimation.IsReloadAnimationForward;
		handGunAnimation.UpdateReloadMagazine(gunState.IsGunReloadTimeActive(EnumManager.GunTYPE.HandGun), reloadAnimationTime, isReloadAnimationForward, leftHand);
		assaultRifleAnimation.UpdateReloadMagazine(gunState.IsGunReloadTimeActive(EnumManager.GunTYPE.AssaultRifle), reloadAnimationTime, isReloadAnimationForward, leftHand);
		shotGunAnimation.UpdateReloadMagazine(gunState.IsGunReloadTimeActive(EnumManager.GunTYPE.ShotGun), reloadAnimationTime, isReloadAnimationForward, leftHand);
		//ハンドガンに持ち替える時の、銃のモデルを持ち替える
		sideArmAnimation.UpdateModels(gunState, gunViewFacade, leftHand);
	}
}
