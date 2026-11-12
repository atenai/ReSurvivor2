using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 地雷を置くアニメーション（地雷を置くアニメーションの進め方と、混ぜる割合）
/// ※PlayerAnimationFacadeが持ち、地雷を置く動きの段階を見て動かす
/// ※地雷を置いている間の手の位置（銃がヘルメットや胸にめり込まないように右手を離し、左手を両足の間の前へ下ろして、
///   立ち上がる時は体の外側を通して銃の前側へ戻す）は、銃ごとのアニメーションクリップ（PlaceMineHandGun・PlaceMineAssaultRifle・PlaceMineShotGun）に焼き込んである
/// </summary>
public class MineAnimation
{
	//※銃を持ったまま地面の物を拾うアニメーション（右手で銃を持ったまましゃがみ、左手を地面へ下ろして、立ち上がる）を、
	//  ・地雷を置くボタンを押している間：しゃがみ始めから、左手が地面に着くまで（長押しの時間と同じ0.5秒）
	//  ・地雷を置いた後：左手が地面に着いた所から、立ち上がるまで
	//  の2つに分けて使う
	[Tooltip("地雷を置くアニメーションの、しゃがみ始める再生位置（0がアニメーションの始まり、1が終わり）")]
	const float CrouchStartTime = 0.2f;
	[Tooltip("地雷を置くアニメーションの、左手が地面に着く再生位置")]
	const float GroundTime = 0.45f;
	[Tooltip("地雷を置くアニメーションの、立ち上がり終わる再生位置")]
	const float StandUpEndTime = 0.85f;
	[Tooltip("しゃがむ動きのうち、今の立ち姿から地雷を置くアニメーションに切り替え終わるまでの割合（0〜1）")]
	const float BlendInRate = 0.5f;
	[Tooltip("立ち上がる動きのうち、地雷を置くアニメーションから元の立ち姿へ戻し始める割合（0〜1）")]
	const float BlendOutStartRate = 0.6f;
	[Tooltip("立ち上がり終わった時の、立ち上がった割合")]
	const float StandUpEndRate = 1.0f;
	[Tooltip("地雷を置くアニメーションを混ぜない時のレイヤーの重み")]
	const float NoWeight = 0.0f;
	[Tooltip("地雷を置くアニメーションだけにする時のレイヤーの重み")]
	const float FullWeight = 1.0f;

	/// <summary>
	/// 地雷を置く動きの段階に合わせて、地雷を置くアニメーションを動かす
	/// ・しゃがんでいる間：長押しした時間に合わせて、しゃがみ始めから左手が地面に着くまでを進める（やめた時は同じように戻す）
	/// ・立ち上がる間：立ち上がり始めてからの時間に合わせて、左手が地面に着いた所から立ち上がるまでを進める
	/// ※しゃがみ始めと立ち上がりの終わりは、今の立ち姿となめらかに混ぜる（手のIKも、混ぜる割合だけ弱める）
	/// </summary>
	/// <param name="animator">プレイヤーのアニメーター</param>
	/// <param name="minePlacePhase">地雷を置く動きの段階</param>
	/// <param name="crouchRate">しゃがんだ割合（長押しした時間 ÷ 長押しと判定する時間。1以上は、左手が地面に着いたまま）</param>
	/// <param name="standUpRate">立ち上がった割合（立ち上がり始めてからの時間 ÷ 立ち上がるのにかける時間）</param>
	public void UpdateAnimation(Animator animator, EnumManager.MinePlacePhaseTYPE minePlacePhase, float crouchRate, float standUpRate)
	{
		float time = CrouchStartTime;
		float weight = NoWeight;
		switch (minePlacePhase)
		{
			case EnumManager.MinePlacePhaseTYPE.Crouch:
			case EnumManager.MinePlacePhaseTYPE.Cancel:
				time = Mathf.Lerp(CrouchStartTime, GroundTime, crouchRate);
				weight = Mathf.SmoothStep(NoWeight, FullWeight, crouchRate / BlendInRate);
				break;
			case EnumManager.MinePlacePhaseTYPE.StandUp:
				time = Mathf.Lerp(GroundTime, StandUpEndTime, standUpRate);
				weight = Mathf.SmoothStep(FullWeight, NoWeight, Mathf.InverseLerp(BlendOutStartRate, StandUpEndRate, standUpRate));
				break;
			case EnumManager.MinePlacePhaseTYPE.None:
				break;
		}
		animator.SetLayerWeight(PlayerAnimatorParameter.PlaceMineLayerIndex, weight);
		animator.SetFloat(PlayerAnimatorParameter.PlaceMineTime, time);
	}

	/// <summary>
	/// 地雷を置くアニメーションを混ぜないようにする（立ち姿に戻す）
	/// </summary>
	/// <param name="animator">プレイヤーのアニメーター</param>
	public void ResetAnimation(Animator animator)
	{
		animator.SetLayerWeight(PlayerAnimatorParameter.PlaceMineLayerIndex, NoWeight);
	}
}
