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
///   どちらも、構えていない時の動き（〇〇Relaxed）と構えている時の動き（〇〇Aim）があり、切り替え始めた時の構えでどちらか一方を選ぶ
/// ※銃は、右手の骨の子の目印（GunPose）の子にしてあり、アニメーションクリップが目印を動かす（しまい終わった時・取り出し始めた時に、しまってある銃とぴったり重なる）
/// ※GunAnimationFacadeが持ち、銃の状態（IReadOnlyGunModel）の切り替えの段階を見て、アニメーターのパラメーターを渡す
/// </summary>
public class WeaponSwitchAnimation
{
	[Tooltip("構えていない時の動きを使う値（アニメーターのパラメーターの値）")]
	const float RelaxedValue = 0.0f;
	[Tooltip("構えている時の動きを使う値（アニメーターのパラメーターの値）")]
	const float AimValue = 1.0f;
	[Tooltip("切り替え始めた時の構えている割合が、この値以上なら構えている時の動きを使う")]
	const float AimThreshold = 0.5f;

	[Tooltip("しまった銃の種類（取り出すアニメーションクリップを、しまった銃と取り出す銃の組み合わせで選ぶ）")]
	EnumManager.GunTYPE putAwayGunTYPE = EnumManager.GunTYPE.HandGun;
	[Tooltip("前のフレームの、武器を切り替える動きの段階（切り替え始めたフレームを見つけるのに使う）")]
	EnumManager.WeaponSwitchPhaseTYPE previousPhase = EnumManager.WeaponSwitchPhaseTYPE.None;
	[Tooltip("今の切り替えで使う動き（構えていない時の動きか、構えている時の動きか）")]
	float switchAimValue = RelaxedValue;

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
		bool isSwitchStart = previousPhase == EnumManager.WeaponSwitchPhaseTYPE.None && phase != EnumManager.WeaponSwitchPhaseTYPE.None;
		previousPhase = phase;
		if (phase == EnumManager.WeaponSwitchPhaseTYPE.None)
		{
			return;
		}

		//切り替え始めた時に、構えていない時の動きと構えている時の動きのどちらを使うかを決めて、切り替え終わるまで変えない
		//※2つの動きを混ぜると、腕の筋肉の値が大きく違う所（腕のひねりなど）で、混ぜた腕が一瞬あらぬ形になり、手と銃が跳んで銃が体に埋まるため
		//  （構えている割合は、構えを切り替えてから少しずつ変わるので、構えた直後・構えをやめた直後に切り替えると、途中の割合で混ざってしまう）
		if (isSwitchStart == true)
		{
			switchAimValue = RelaxedValue;
			if (AimThreshold <= animator.GetFloat(PlayerAnimatorParameter.Aim))
			{
				switchAimValue = AimValue;
			}
		}
		animator.SetFloat(PlayerAnimatorParameter.WeaponSwitchAim, switchAimValue);

		//しまっている間は、今持っている銃がしまう銃（しまい終わった時に、今持っている銃が取り出す銃に変わる）
		if (phase == EnumManager.WeaponSwitchPhaseTYPE.PutAway)
		{
			putAwayGunTYPE = gunState.GetGunTYPE;
		}
		animator.SetFloat(PlayerAnimatorParameter.PreviousGunType, (float)putAwayGunTYPE);
		animator.SetFloat(PlayerAnimatorParameter.WeaponSwitchTime, gunState.WeaponSwitchPhaseProgress);
	}
}
