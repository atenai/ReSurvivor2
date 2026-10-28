using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// プレイヤーのアニメーター（アニメーターコントローラー）の、パラメーターの名前・レイヤーの番号・ステートの番号
/// ※プレイヤーのアニメーションのクラスは、アニメーターの名前や番号をすべてここから使う
///   （アニメーターコントローラーのパラメーター・レイヤー・ステートの名前や並びを変えた時は、ここだけ直せばよい）
/// </summary>
public static class PlayerAnimatorParameter
{
	//↓パラメーターの名前↓//
	[Tooltip("移動の左右の入力（-1〜1）")]
	public const string MoveSpeedX = "f_moveSpeedX";
	[Tooltip("移動の前後の入力（-1〜1）")]
	public const string MoveSpeedY = "f_moveSpeedY";
	[Tooltip("ダッシュの割合（0で走り、1でダッシュ）")]
	public const string Dash = "f_dash";
	[Tooltip("構えているか？")]
	public const string IsAim = "b_isAim";
	[Tooltip("構えている割合（0で構えていない、1で構えている。構えの切り替えに合わせて、なめらかに変わる。リロードのアニメーションで「構えていない時の動き」と「構えている時の動き」を混ぜる割合にも使う）")]
	public const string Aim = "f_aim";
	[Tooltip("ハンドガンを持っているか？（ハンドガンは構えていない時に片手で持つので、移動のアニメーションをライフル・ショットガンと分ける）")]
	public const string IsHandGun = "b_isHandGun";
	[Tooltip("ハンドガンを片手で狙っている割合（1で片手、0で両手）")]
	public const string OneHandAim = "f_oneHandAim";
	[Tooltip("ハンドガンを近距離の構え（C.A.R.システムのような構え）にしている割合（1で近距離の構え、0で普通の構え）")]
	public const string CloseQuarterAim = "f_closeQuarterAim";
	[Tooltip("リロードアニメーションの再生位置（0がアニメーションの始まり、1が終わり）")]
	public const string ReloadAnimationTime = "f_reloadAnimationTime";
	[Tooltip("ハンドガンのリロードアニメーションをオンにするか？")]
	public const string IsHandGunReload = "b_isHandGunReload";
	[Tooltip("アサルトライフルのリロードアニメーションをオンにするか？")]
	public const string IsAssaultRifleReload = "b_isAssaultRifleReload";
	[Tooltip("ショットガンのリロードアニメーションをオンにするか？")]
	public const string IsShotGunReload = "b_isShotGunReload";
	[Tooltip("地雷を置くアニメーションの再生位置（0がアニメーションの始まり、1が終わり）")]
	public const string PlaceMineTime = "f_placeMineTime";

	//↓レイヤーの番号（アニメーターコントローラーのレイヤーの並び）↓//
	//※IKパスは一番上のレイヤー（UpperBodyReload）だけにオンにしてある（複数のレイヤーでオンにすると、IKの処理が何度も呼ばれるため）
	[Tooltip("移動のレイヤーの番号")]
	public const int LocomotionLayerIndex = 0;
	[Tooltip("ハンドガンを片手で構えている間だけ、左腕を下ろしておくレイヤーの番号")]
	public const int HandGunOneHandAimLeftArmLayerIndex = 1;
	[Tooltip("ダッシュしている間だけ、右腕と右手の指を走りのアニメーション（銃を持つ腕）にしておくレイヤーの番号")]
	public const int DashRightArmLayerIndex = 2;
	[Tooltip("地雷を置くアニメーションのレイヤーの番号（全身）")]
	public const int PlaceMineLayerIndex = 3;
	[Tooltip("上半身のリロードのレイヤーの番号")]
	public const int UpperBodyReloadLayerIndex = 4;
	[Tooltip("ショットガンの弾を右手で持っている間だけ、右手の指を弾をつまむ形（親指と人差し指で弾の根元をつまむ、指だけのアニメーション）にしておくレイヤーの番号")]
	public const int ShotGunShellRightFingersLayerIndex = 5;

	//↓ステートの番号（ステート名から作った番号）↓//
	[Tooltip("構えていない時の移動のステート（アサルトライフル・ショットガン）")]
	public static readonly int NormalIdleAndWalkStateHash = Animator.StringToHash("NormalIdleAndWalk");
	[Tooltip("ハンドガンを構えていない時の移動のステート")]
	public static readonly int HandGunNormalIdleAndWalkStateHash = Animator.StringToHash("HandGunNormalIdleAndWalk");
	[Tooltip("ハンドガンを構えている時の移動のステート")]
	public static readonly int HandGunAimIdleAndWalkStateHash = Animator.StringToHash("HandGunAimIdleAndWalk");
	[Tooltip("リロードしていない時のステート（上半身のリロードのレイヤー）")]
	public static readonly int NoReloadStateHash = Animator.StringToHash("NoReload");
	[Tooltip("ハンドガンのリロードのステート（上半身のリロードのレイヤー）")]
	public static readonly int HandGunReloadStateHash = Animator.StringToHash("HandGunReload");
	[Tooltip("ステートのアニメーションが全く混ざっていない時の割合")]
	const float NoStateWeight = 0.0f;
	[Tooltip("そのステートのアニメーションだけの時の割合")]
	const float FullStateWeight = 1.0f;

	/// <summary>
	/// レイヤーの中で、指定したステートのアニメーションがどれだけ混ざっているかを求める（0で全く混ざっていない、1でそのステートだけ）
	/// ※ステートが切り替わる遷移の間は、遷移の進み具合に合わせて混ざる割合が変わる
	/// </summary>
	/// <param name="animator">プレイヤーのアニメーター</param>
	/// <param name="layerIndex">アニメーターのレイヤーの番号</param>
	/// <param name="stateHash">ステート（ステート名から作った番号）</param>
	/// <returns>ステートのアニメーションが混ざっている割合</returns>
	public static float GetStateWeight(Animator animator, int layerIndex, int stateHash)
	{
		float currentStateWeight = animator.GetCurrentAnimatorStateInfo(layerIndex).shortNameHash == stateHash ? FullStateWeight : NoStateWeight;
		if (animator.IsInTransition(layerIndex) == false)
		{
			return currentStateWeight;
		}

		float nextStateWeight = animator.GetNextAnimatorStateInfo(layerIndex).shortNameHash == stateHash ? FullStateWeight : NoStateWeight;
		return Mathf.Lerp(currentStateWeight, nextStateWeight, animator.GetAnimatorTransitionInfo(layerIndex).normalizedTime);
	}
}
