using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 構えのアニメーション（構えている割合・ハンドガンの片手で狙う構え・近距離の構え）
/// ※PlayerCharacterViewが持ち、プレイヤーの状態を受け取って動かす
/// </summary>
public class AimAnimation
{
	[Tooltip("アニメーターに渡す構えの値（構えていない時）")]
	const float NotAimValue = 0.0f;
	[Tooltip("アニメーターに渡す構えの値（構えている時）")]
	const float AimValue = 1.0f;
	[Tooltip("アニメーターに渡す構えの値が、構えの切り替えに追いつくまでのおおよその時間（秒）（待機・移動の構えの切り替えの遷移時間と同じにする）")]
	const float AimValueDampTime = 0.25f;

	[Header("ハンドガンの片手で狙う構え")]
	[Tooltip("ハンドガンを構え始めてから、片手で狙う時間（秒）（この後、左手を添えて両手で狙う）")]
	const float OneHandAimTime = 1.0f;
	[Tooltip("片手で狙う構えから、両手で狙う構えに移るのにかける時間（秒）")]
	const float TwoHandAimBlendTime = 0.3f;
	[Tooltip("アニメーターに渡す、片手で狙っている割合（片手で狙っている時）")]
	const float OneHandAimValue = 1.0f;
	[Tooltip("アニメーターに渡す、片手で狙っている割合（両手で狙っている時）")]
	const float TwoHandAimValue = 0.0f;
	[Tooltip("構え続けている時間（秒）（構えていない時）")]
	const float NotAimContinueTime = 0.0f;
	[Tooltip("構え続けている時間（秒）")]
	float aimContinueTime = NotAimContinueTime;
	[Tooltip("ハンドガンに持ち替えた時に、片手で狙う構えに移るのにかけるおおよその時間（秒）（ハンドガンを抜く間に、上半身をひねって右腕を伸ばす）")]
	const float SideArmOneHandAimDampTime = 0.15f;

	[Header("ハンドガンの近距離の構え")]
	[Tooltip("アニメーターに渡す、近距離の構え（C.A.R.システムのような構え）の割合（狙っている敵が近い時）")]
	const float CloseQuarterAimValue = 1.0f;
	[Tooltip("アニメーターに渡す、近距離の構えの割合（狙っている敵が近くない時）")]
	const float NotCloseQuarterAimValue = 0.0f;
	[Tooltip("アニメーターに渡す近距離の構えの割合が、切り替えに追いつくまでのおおよその時間（秒）")]
	const float CloseQuarterAimDampTime = 0.1f;

	/// <summary>
	/// 構えのアニメーションを動かす
	/// ※SetFloatの3つ目の引数（dampTime）を渡すと、値が一瞬で切り替わらず、その時間をかけてなめらかに近づく
	/// </summary>
	/// <param name="animator">プレイヤーのアニメーター</param>
	/// <param name="isAim">構えているか？</param>
	/// <param name="isEnemyAlert">警戒体制の敵がいるか？</param>
	/// <param name="isEnemyClose">狙っている方向に近い敵がいるか？</param>
	/// <param name="isSideArm">ハンドガンに持ち替えている間か？（左手でメインの銃を持っているので、ハンドガンは右手だけで狙う）</param>
	public void UpdateAnimation(Animator animator, bool isAim, bool isEnemyAlert, bool isEnemyClose, bool isSideArm)
	{
		animator.SetBool(PlayerAnimatorParameter.IsAim, isAim);
		animator.SetFloat(PlayerAnimatorParameter.Aim, isAim ? AimValue : NotAimValue, AimValueDampTime, Time.deltaTime);
		//ハンドガンを構え始めた時は、まず片手で狙い、少ししてから左手を添えて両手で狙う（敵が警戒体制の時は、最初から両手で狙う）
		UpdateOneHandAim(animator, isAim, isEnemyAlert, isSideArm);
		//狙っている方向に近い敵がいる時は、ハンドガンを近距離の構え（C.A.R.システムのような構え）にする
		//（構えていない時も変えておき、構え始めた時には、もう近距離の構えの割合になっているようにする）
		//※ハンドガンに持ち替えている間は、両手で持つ近距離の構えにはしない
		bool isCloseQuarterAim = isEnemyClose == true && isSideArm == false;
		animator.SetFloat(PlayerAnimatorParameter.CloseQuarterAim, isCloseQuarterAim == true ? CloseQuarterAimValue : NotCloseQuarterAimValue, CloseQuarterAimDampTime, Time.deltaTime);
	}

	/// <summary>
	/// ハンドガンを構えた時の、片手で狙う構えと両手で狙う構えを切り替える
	/// ・構え始めた時は、まず片手で狙い、少ししてから左手を添えて両手で狙う
	/// ・敵が警戒体制の時は、片手で狙わずに、最初から両手で狙う
	/// ・ハンドガンに持ち替えている間は、ずっと片手で狙う
	/// </summary>
	/// <param name="animator">プレイヤーのアニメーター</param>
	/// <param name="isAim">構えているか？</param>
	/// <param name="isEnemyAlert">警戒体制の敵がいるか？</param>
	/// <param name="isSideArm">ハンドガンに持ち替えている間か？</param>
	void UpdateOneHandAim(Animator animator, bool isAim, bool isEnemyAlert, bool isSideArm)
	{
		if (isAim == false)
		{
			//構えをやめた時は、片手で狙っている割合はそのままにしておく（構えをやめるアニメーションの間に、左手が急に銃から離れないようにする）
			aimContinueTime = NotAimContinueTime;
			return;
		}

		if (isSideArm == true)
		{
			animator.SetFloat(PlayerAnimatorParameter.OneHandAim, OneHandAimValue, SideArmOneHandAimDampTime, Time.deltaTime);
			return;
		}

		if (isEnemyAlert == true)
		{
			if (aimContinueTime <= NotAimContinueTime)
			{
				//構え始めた時は、片手で狙う時間を飛ばして、すぐに両手で狙う
				aimContinueTime = OneHandAimTime + TwoHandAimBlendTime;
			}
			else if (aimContinueTime < OneHandAimTime)
			{
				//片手で狙っている途中で警戒体制になった時は、そこから両手で狙う構えに移る（左手が急に銃に飛びつかないようにする）
				aimContinueTime = OneHandAimTime;
			}
		}

		aimContinueTime = aimContinueTime + Time.deltaTime;
		animator.SetFloat(PlayerAnimatorParameter.OneHandAim, Mathf.SmoothStep(OneHandAimValue, TwoHandAimValue, (aimContinueTime - OneHandAimTime) / TwoHandAimBlendTime));
	}

	/// <summary>
	/// 構えていない時に戻す
	/// </summary>
	/// <param name="animator">プレイヤーのアニメーター</param>
	public void ResetAnimation(Animator animator)
	{
		animator.SetBool(PlayerAnimatorParameter.IsAim, false);
		animator.SetFloat(PlayerAnimatorParameter.Aim, NotAimValue);
	}
}
