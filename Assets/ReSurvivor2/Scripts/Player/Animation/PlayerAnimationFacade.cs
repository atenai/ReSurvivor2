using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// プレイヤーアニメーションファサード
/// プレイヤーの体のアニメーション（移動・構え・地雷を置く・手のIK）を管理する窓口
/// ※PlayerCharacterViewが持つ（銃のクラスからは使わない）
///   銃のアニメーション（リロード・ハンドガンへの持ち替え・武器の切り替えなど）は、GunAnimationFacadeが管理する。手のIKは、そこで求めた手の位置・向きを使う
/// </summary>
[System.Serializable]
public class PlayerAnimationFacade
{
	[Tooltip("手のIK")]
	[SerializeField] HandIK handIK = new HandIK();
	[Tooltip("地雷を置く時のアニメーション")]
	readonly MineAnimation mineAnimation = new MineAnimation();
	[Tooltip("移動のアニメーション")]
	readonly MoveAnimation moveAnimation = new MoveAnimation();
	[Tooltip("構えのアニメーション")]
	readonly AimAnimation aimAnimation = new AimAnimation();

	/// <summary>
	/// アニメーションどおりの左手の位置・向きをIK用の向きで取り出すための目印（左手の骨の子）
	/// </summary>
	public Transform LeftHandAnimationPos => handIK.LeftHandAnimationPos;

	/// <summary>
	/// 今のフレームで、上半身をひねった分の回転（構えに合わせて背骨を曲げる時に、ひねる前の向きで曲げる量を求めるのに使う）
	/// </summary>
	public Quaternion UpperBodyTwistRotation => handIK.UpperBodyTwistRotation;

	/// <summary>
	/// 今のフレームで、ハンドガンを片手で狙っている割合（構えに合わせて背骨を曲げる量を、片手で狙う時は減らすのに使う）
	/// </summary>
	public float HandGunOneHandAimWeight => handIK.HandGunOneHandAimWeight;

	/// <summary>
	/// 初期化
	/// </summary>
	/// <param name="animator">プレイヤーのアニメーター</param>
	public void Initialize(Animator animator)
	{
		handIK.Initialize(animator);
	}

	/// <summary>
	/// 移動のアニメーションを動かす
	/// </summary>
	/// <param name="animator">プレイヤーのアニメーター</param>
	/// <param name="moveX">移動の左右の値（移動の向き × 移動速度の割合。長さ1で走り、移動スティックを少しだけ倒した時は歩く速さの割合）</param>
	/// <param name="moveY">移動の前後の値（移動の向き × 移動速度の割合）</param>
	/// <param name="isDash">ダッシュしているか？</param>
	/// <param name="isPlacingMine">地雷を置いている間か？（その場で止まっているアニメーションにする）</param>
	/// <param name="isHandGun">ハンドガンを持っているか？</param>
	public void UpdateMoveAnimation(Animator animator, float moveX, float moveY, bool isDash, bool isPlacingMine, bool isHandGun)
	{
		moveAnimation.UpdateAnimation(animator, moveX, moveY, isDash, isPlacingMine, isHandGun);
	}

	/// <summary>
	/// 構えのアニメーションを動かす（構えている割合・ハンドガンの片手で狙う構え・近距離の構え）
	/// </summary>
	/// <param name="animator">プレイヤーのアニメーター</param>
	/// <param name="isAim">構えているか？</param>
	/// <param name="isEnemyAlert">警戒体制の敵がいるか？（いる時は、ハンドガンを最初から両手で狙う）</param>
	/// <param name="isEnemyClose">狙っている方向に近い敵がいるか？（いる時は、ハンドガンを近距離の構えにする）</param>
	/// <param name="isSideArm">ハンドガンに持ち替えている間か？</param>
	public void UpdateAimAnimation(Animator animator, bool isAim, bool isEnemyAlert, bool isEnemyClose, bool isSideArm)
	{
		aimAnimation.UpdateAnimation(animator, isAim, isEnemyAlert, isEnemyClose, isSideArm);
	}

	/// <summary>
	/// 地雷を置く動きの段階に合わせて、地雷を置くアニメーションを動かす
	/// </summary>
	/// <param name="animator">プレイヤーのアニメーター</param>
	/// <param name="minePlacePhase">地雷を置く動きの段階</param>
	/// <param name="crouchRate">しゃがんだ割合（長押しした時間 ÷ 長押しと判定する時間。1以上は、左手が地面に着いたまま）</param>
	/// <param name="standUpRate">立ち上がった割合（立ち上がり始めてからの時間 ÷ 立ち上がるのにかける時間）</param>
	public void UpdatePlaceMineAnimation(Animator animator, EnumManager.MinePlacePhaseTYPE minePlacePhase, float crouchRate, float standUpRate)
	{
		mineAnimation.UpdateAnimation(animator, minePlacePhase, crouchRate, standUpRate);
	}

	/// <summary>
	/// 移動・構え・地雷を置くアニメーションをリセットする（立ち姿に戻す）
	/// </summary>
	/// <param name="animator">プレイヤーのアニメーター</param>
	public void ResetAnimation(Animator animator)
	{
		moveAnimation.ResetAnimation(animator);
		aimAnimation.ResetAnimation(animator);
		mineAnimation.ResetAnimation(animator);
	}

	/// <summary>
	/// 手のIKを動かす（アニメーターのIKの処理（OnAnimatorIK）の中で、銃のアニメーションの割合を求めた後に呼ぶ）
	/// </summary>
	/// <param name="gunAnimationFacade">銃のアニメーションの窓口（ハンドガンへの持ち替えの手の位置・向きを使う）</param>
	/// <param name="gunAnimation">今持っている銃のアニメーションの設定</param>
	/// <param name="gunModelTransform">今持っている銃のモデル（前向きが銃身、上向きが銃の上側）</param>
	/// <param name="handGunView">ハンドガンを持っている時は、ハンドガンのビュー（ほかの銃の時はnull）</param>
	/// <param name="spineAimRotation">前のフレームで、構えに合わせて脊椎ボーンを曲げた回転（ワールドの向き）</param>
	/// <param name="spinePosition">前のフレームで、構えに合わせて曲げた脊椎ボーンの位置</param>
	/// <param name="aimRay">弾の飛ぶ線（カメラの位置から正面へ）</param>
	public void UpdateIK(GunAnimationFacade gunAnimationFacade, GunAnimation gunAnimation, Transform gunModelTransform, HandGunView handGunView, Quaternion spineAimRotation, Vector3 spinePosition, Ray aimRay)
	{
		handIK.UpdateIK(gunAnimation, gunModelTransform, handGunView, gunAnimationFacade.SideArmAnimation, spineAimRotation, spinePosition, aimRay);
	}
}
