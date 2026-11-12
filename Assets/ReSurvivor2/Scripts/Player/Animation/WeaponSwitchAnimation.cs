using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 武器を切り替える時のアニメーション
/// （今持っている銃を、右手で体のしまう場所（ハンドガンは右の腰、アサルトライフルは左の腰、ショットガンは背中）へしまってから、
///   右手を切り替える先の銃へ伸ばして、しまう場所から取り出して持つ）
/// ※両手と銃の動きは、上半身のリロードのレイヤーの2つのステートのアニメーションクリップに焼き込んである
///   ・WeaponPutAway：しまう銃ごと（PutAway〇〇）
///   ・WeaponTakeOut：しまった銃と取り出す銃の組み合わせごと（TakeOut〇〇To〇〇。右手は、しまった所から取り出す銃へ伸ばす）
///   どちらも、構えていない時の動き（〇〇Relaxed）と構えている時の動き（〇〇Aim）を、構えている割合で混ぜる
/// ※銃は、右手の骨の子の目印（GunPose）の子にしてあり、アニメーションクリップが目印を動かす（しまい終わった時・取り出し始めた時に、しまってある銃とぴったり重なる）
/// ※GunAnimationFacadeが持ち、銃の状態（IReadOnlyGunModel）の切り替えの段階を見て、アニメーターのパラメーターを渡す
/// </summary>
public class WeaponSwitchAnimation
{
	[Tooltip("しまった銃の種類（取り出すアニメーションクリップを、しまった銃と取り出す銃の組み合わせで選ぶ）")]
	EnumManager.GunTYPE putAwayGunTYPE = EnumManager.GunTYPE.HandGun;

	/// <summary>
	/// 武器を切り替える動きの段階に合わせて、しまう・取り出すアニメーションを動かす（銃の更新処理の後に呼ぶ）
	/// ※切り替えていない時は再生位置を変えない（取り出し終わってアニメーションを抜ける間に、手が急に動かないようにする）
	/// </summary>
	/// <param name="animator">プレイヤーのアニメーター</param>
	/// <param name="gunState">銃の状態を読むための窓口</param>
	public void UpdateAnimation(Animator animator, IReadOnlyGunModel gunState)
	{
		EnumManager.WeaponSwitchPhaseTYPE phase = gunState.WeaponSwitchPhase;
		animator.SetBool(PlayerAnimatorParameter.IsWeaponPutAway, phase == EnumManager.WeaponSwitchPhaseTYPE.PutAway);
		animator.SetBool(PlayerAnimatorParameter.IsWeaponTakeOut, phase == EnumManager.WeaponSwitchPhaseTYPE.TakeOut);
		if (phase == EnumManager.WeaponSwitchPhaseTYPE.None)
		{
			return;
		}

		//しまっている間は、今持っている銃がしまう銃（しまい終わった時に、今持っている銃が取り出す銃に変わる）
		if (phase == EnumManager.WeaponSwitchPhaseTYPE.PutAway)
		{
			putAwayGunTYPE = gunState.GetGunTYPE;
		}
		animator.SetFloat(PlayerAnimatorParameter.PreviousGunType, (float)putAwayGunTYPE);
		animator.SetFloat(PlayerAnimatorParameter.WeaponSwitchTime, gunState.WeaponSwitchPhaseProgress);
	}
}
