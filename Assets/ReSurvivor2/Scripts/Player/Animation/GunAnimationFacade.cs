using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ガンアニメーションファサード
/// 銃のアニメーション（銃ごとの設定・リロード・ショットガンのリロード・ハンドガンへの持ち替え・武器の切り替え）を管理する窓口
/// ※PlayerCharacterViewが持つ（銃のクラスからは使わない）。銃の状態は、IReadOnlyGunModelから読む
///   プレイヤーの体のアニメーション（移動・構え・地雷を置く・手のIK）は、PlayerAnimationFacadeが管理する。手のIKは、ここで求めた手の位置・向きを使う
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
	[Tooltip("ショットガンのリロードのアニメーション（左手で銃を斜めに持ち上げ、右手で弾を込める）")]
	[SerializeField] ShotGunReloadAnimation shotGunReloadAnimation = new ShotGunReloadAnimation();
	public ShotGunReloadAnimation ShotGunReloadAnimation => shotGunReloadAnimation;
	[Tooltip("ハンドガンへの持ち替えのアニメーション")]
	readonly SideArmAnimation sideArmAnimation = new SideArmAnimation();
	public SideArmAnimation SideArmAnimation => sideArmAnimation;
	[Tooltip("武器を切り替える時のアニメーション（今持っている銃を体にしまってから、切り替える先の銃を取り出す）")]
	readonly WeaponSwitchAnimation weaponSwitchAnimation = new WeaponSwitchAnimation();
	public WeaponSwitchAnimation WeaponSwitchAnimation => weaponSwitchAnimation;

	/// <summary>
	/// リロードアニメーションの再生位置
	/// </summary>
	public float ReloadAnimationTime => reloadAnimation.ReloadAnimationTime;

	/// <summary>
	/// 初期化（マガジンを銃に付けている時の親・位置・向きを覚えておき、ハンドガンへの持ち替え・武器の切り替え・ショットガンのリロードの準備をする）
	/// </summary>
	/// <param name="animator">プレイヤーのアニメーター</param>
	/// <param name="gunViewFacade">ガンビューファサード</param>
	/// <param name="spine">キャラクターの脊椎ボーン</param>
	/// <param name="rightUpperArm">キャラクターの右肩ボーン</param>
	/// <param name="leftHandAnimationPos">アニメーションどおりの左手の位置・向きをIK用の向きで取り出すための目印</param>
	/// <param name="rightHandAnimationPos">アニメーションどおりの右手の位置・向きをIK用の向きで取り出すための目印</param>
	public void Initialize(Animator animator, GunViewFacade gunViewFacade, Transform spine, Transform rightUpperArm, Transform leftHandAnimationPos, Transform rightHandAnimationPos)
	{
		handGunAnimation.Initialize();
		assaultRifleAnimation.Initialize();
		shotGunAnimation.Initialize();
		sideArmAnimation.Initialize(gunViewFacade.HandGunView, leftHandAnimationPos, animator);
		weaponSwitchAnimation.Initialize(spine, animator.transform, rightUpperArm);
		shotGunReloadAnimation.Initialize(gunViewFacade.ShotGunView.GetShotGunModel.transform, shotGunAnimation, rightHandAnimationPos, leftHandAnimationPos);
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
	/// ショットガンを持っていて、リロード中か？
	/// </summary>
	/// <param name="gunState">銃の状態を読むための窓口</param>
	/// <returns>ショットガンを持っていて、リロード中ならtrue</returns>
	bool IsShotGunReloading(IReadOnlyGunModel gunState)
	{
		return gunState.GetGunTYPE == EnumManager.GunTYPE.ShotGun && gunState.IsReloadTimeActive == true;
	}

	/// <summary>
	/// 銃の状態に合わせて、リロードのアニメーションを動かす（銃の更新処理の後に呼ぶ）
	/// </summary>
	/// <param name="animator">プレイヤーのアニメーター</param>
	/// <param name="gunState">銃の状態を読むための窓口</param>
	public void UpdateReloadAnimation(Animator animator, IReadOnlyGunModel gunState)
	{
		reloadAnimation.UpdateAnimation(animator, gunState, shotGunAnimation);
	}

	/// <summary>
	/// ハンドガンへの持ち替え・ショットガンのリロード・武器の切り替えの、手や銃を動かす割合を求める（アニメーターのIKの処理（OnAnimatorIK）の中で、手のIKの前に呼ぶ）
	/// </summary>
	/// <param name="gunState">銃の状態を読むための窓口</param>
	/// <param name="gunViewFacade">ガンビューファサード</param>
	/// <param name="spineAimRotation">前のフレームで、構えに合わせて脊椎ボーンを曲げた回転（ワールドの向き）</param>
	/// <param name="spinePosition">前のフレームで、構えに合わせて曲げた脊椎ボーンの位置</param>
	public void UpdateWeights(IReadOnlyGunModel gunState, GunViewFacade gunViewFacade, Quaternion spineAimRotation, Vector3 spinePosition)
	{
		GunAnimation sideArmMainGunAnimation = gunState.IsSideArmActive == true ? FindGunAnimation(gunState.SideArmMainGunTYPE) : null;
		sideArmAnimation.UpdateWeights(gunState.SideArmPhase, gunState.SideArmPhaseProgress, gunState.SideArmHolsterReachRate, spineAimRotation, spinePosition, sideArmMainGunAnimation);
		shotGunReloadAnimation.UpdateWeights(IsShotGunReloading(gunState), reloadAnimation.ReloadAnimationTime, spineAimRotation, spinePosition);
		//※武器を切り替えている間は、今持っている銃（しまう時はしまう銃、取り出す時は取り出す銃）を動かす（前のフレームで動かした銃は、ここで右手で持つ位置に戻す）
		EnumManager.GunTYPE gunTYPE = gunState.GetGunTYPE;
		weaponSwitchAnimation.UpdateWeights(gunState.WeaponSwitchPhase, gunState.WeaponSwitchPhaseProgress, FindGunAnimation(gunTYPE), gunViewFacade.FindGunModelTransform(gunTYPE), gunViewFacade.FindGunModelBodyDecorationTransform(gunTYPE), spineAimRotation, spinePosition);
	}

	/// <summary>
	/// リロード中のマガジン・ハンドガンに持ち替える時の銃のモデル・ショットガンのリロード中の銃を動かす（アニメーションで手が動いた後に呼ぶ）
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
		//ショットガンのリロード中は、銃を左手に持ち替える
		shotGunReloadAnimation.UpdateModel(gunState.GetGunTYPE == EnumManager.GunTYPE.ShotGun, IsShotGunReloading(gunState), leftHand);
	}

	/// <summary>
	/// 武器を切り替えている間は、背骨を曲げた後に、銃をしまう・取り出す動きの位置・向きへ動かす（体のしまう場所にぴったり重ねるため）
	/// </summary>
	/// <param name="spineBendRotation">今のフレームで、構えに合わせて脊椎ボーンを曲げた回転（ワールドの向き）</param>
	/// <param name="spineBendPosition">構えに合わせて曲げた脊椎ボーンの位置</param>
	public void UpdateWeaponSwitchGunModel(Quaternion spineBendRotation, Vector3 spineBendPosition)
	{
		weaponSwitchAnimation.UpdateGunModel(spineBendRotation, spineBendPosition);
	}
}
