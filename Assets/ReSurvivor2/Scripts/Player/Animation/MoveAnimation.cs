using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 移動のアニメーション（移動の入力・ダッシュ・持っている銃に合わせた移動のアニメーション）
/// ※PlayerCharacterViewが持ち、プレイヤーの状態を受け取って動かす
/// </summary>
public class MoveAnimation
{
	[Tooltip("アニメーターに渡す移動の値が、移動の入力に追いつくまでのおおよその時間（秒）（移動の入力はキーを押した瞬間に0から1に変わるので、立ち止まる・歩き出す時にアニメーションが一瞬で切り替わらないようにする）")]
	const float MoveSpeedDampTime = 0.1f;
	[Tooltip("アニメーターに渡す移動の値（移動していない時）")]
	const float NotMoveValue = 0.0f;
	[Tooltip("アニメーターに渡すダッシュの割合（ダッシュしている時）")]
	const float DashValue = 1.0f;
	[Tooltip("アニメーターに渡すダッシュの割合（ダッシュしていない時）")]
	const float NotDashValue = 0.0f;
	[Tooltip("アニメーターに渡すダッシュの割合が、ダッシュの切り替えに追いつくまでのおおよその時間（秒）")]
	const float DashDampTime = 0.2f;

	/// <summary>
	/// 移動のアニメーションを動かす
	/// ※SetFloatの3つ目の引数（dampTime）を渡すと、値が一瞬で切り替わらず、その時間をかけてなめらかに近づく
	/// </summary>
	/// <param name="animator">プレイヤーのアニメーター</param>
	/// <param name="inputHorizontal">移動の左右の入力</param>
	/// <param name="inputVertical">移動の前後の入力</param>
	/// <param name="isDash">ダッシュしているか？</param>
	/// <param name="isPlacingMine">地雷を置いている間か？</param>
	/// <param name="isHandGun">ハンドガンを持っているか？</param>
	public void UpdateAnimation(Animator animator, float inputHorizontal, float inputVertical, bool isDash, bool isPlacingMine, bool isHandGun)
	{
		//地雷を置いている間は、その場で止まっているので、移動のアニメーションも止まっている時のアニメーションにする
		animator.SetFloat(PlayerAnimatorParameter.MoveSpeedX, isPlacingMine == true ? NotMoveValue : inputHorizontal, MoveSpeedDampTime, Time.deltaTime);
		animator.SetFloat(PlayerAnimatorParameter.MoveSpeedY, isPlacingMine == true ? NotMoveValue : inputVertical, MoveSpeedDampTime, Time.deltaTime);
		//ダッシュしている時は、構えていない時の走りを、急いで走るダッシュのアニメーションに変える
		animator.SetFloat(PlayerAnimatorParameter.Dash, isDash == true ? DashValue : NotDashValue, DashDampTime, Time.deltaTime);
		//ハンドガンは、構えていない時に片手で持つので、構えていない時の移動のアニメーションをライフル・ショットガンと分ける
		animator.SetBool(PlayerAnimatorParameter.IsHandGun, isHandGun);
	}

	/// <summary>
	/// 移動のアニメーションを止まっている時に戻す（ダッシュもやめる）
	/// </summary>
	/// <param name="animator">プレイヤーのアニメーター</param>
	public void ResetAnimation(Animator animator)
	{
		animator.SetFloat(PlayerAnimatorParameter.MoveSpeedX, NotMoveValue);
		animator.SetFloat(PlayerAnimatorParameter.MoveSpeedY, NotMoveValue);
		animator.SetFloat(PlayerAnimatorParameter.Dash, NotDashValue);
	}
}
