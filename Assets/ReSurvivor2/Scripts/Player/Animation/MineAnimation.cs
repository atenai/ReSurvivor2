using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 地雷を置く時の、プレイヤーのアニメーションの設定
/// （地雷を置くアニメーションの進め方・地雷を置いている間の手の位置）
/// ※PlayerCharacterViewが持ち、地雷を置く動きの段階を見て動かす（HandIKは、ここで求めた割合や手をずらす量を使う）
/// </summary>
[System.Serializable]
public class MineAnimation
{
	[Header("地雷を置くアニメーション")]
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
	[Tooltip("立ち上がる動きのうち、左手を地面から銃の前側へ戻し終わる割合（0〜1）（アニメーションのまま戻すと、厚い防弾ベストの中を通るため、地面から銃へまっすぐ戻す）")]
	const float LeftHandReturnEndRate = 0.5f;
	[Tooltip("地雷を置くアニメーションを混ぜない時のレイヤーの重み")]
	const float NoWeight = 0.0f;
	[Tooltip("地雷を置くアニメーションだけにする時のレイヤーの重み")]
	const float FullWeight = 1.0f;
	[Tooltip("今のフレームで、地雷を置くアニメーションが混ざっている割合（アニメーターのレイヤーの重みと同じ）")]
	float weight = NoWeight;
	public float Weight => weight;
	[Tooltip("地雷を置いている間に、左手を銃から離している割合（1でアニメーションどおりに地面へ下ろす、0で銃の前側を握る）")]
	float leftHandFreeWeight = FullWeight;
	public float LeftHandFreeWeight => leftHandFreeWeight;
	[Tooltip("地雷を置いて立ち上がる間に、地面から戻す左手を体の外側へ離す割合（立ち上がりの真ん中で一番離す。アニメーションのままだと、左手が腰のポーチを通るため）")]
	float leftHandOutWeight = NoWeight;

	[Header("地雷を置いている時の手の位置")]
	//※地雷を置くアニメーションは体の細いキャラクター用に作られたもので、しゃがんでいる間、右手で立てて持った銃がヘルメットや胸にめり込み、
	//  左の前腕が左の太ももを貫通する。そのため、地雷を置いている間は、HandIKで手をずらす
	[Tooltip("地雷を置いている時に、右手（と銃）をずらす量（m。キャラクターから見た向きで、xが右、yが上、zが前）")]
	[SerializeField] Vector3 rightHandOffset;
	public Vector3 RightHandOffset => rightHandOffset;
	[Tooltip("地雷を置くためにしゃがんでいる時に、地面へ下ろす左手をずらす量（m。キャラクターから見た向きで、xが右、yが上、zが前）（アニメーションのままだと、左の前腕が左の太ももを貫通するため、両足の間の前へ下ろす）")]
	static readonly Vector3 CrouchLeftHandOffset = new Vector3(0.12f, 0.0f, 0.25f);
	[Tooltip("地雷を置いて立ち上がる時に、地面から戻す左手をずらす量（m。キャラクターから見た向き）（アニメーションのままだと、左手が腰の左後ろのポーチを通るため、体の外側の前へ離す）")]
	static readonly Vector3 StandUpLeftHandOffset = new Vector3(-0.15f, 0.0f, 0.10f);

	/// <summary>
	/// 地雷を置く動きの段階に合わせて、地雷を置くアニメーションを動かす
	/// ・しゃがんでいる間：長押しした時間に合わせて、しゃがみ始めから左手が地面に着くまでを進める（やめた時は同じように戻す）
	/// ・立ち上がる間：立ち上がり始めてからの時間に合わせて、左手が地面に着いた所から立ち上がるまでを進める
	/// ※しゃがみ始めと立ち上がりの終わりは、今の立ち姿となめらかに混ぜる
	/// ※左手は、しゃがんでいる間は銃から離し、立ち上がる時は銃の前側へ戻す（HandIK が使う）
	/// </summary>
	/// <param name="animator">プレイヤーのアニメーター</param>
	/// <param name="minePlacePhase">地雷を置く動きの段階</param>
	/// <param name="crouchRate">しゃがんだ割合（長押しした時間 ÷ 長押しと判定する時間。1以上は、左手が地面に着いたまま）</param>
	/// <param name="standUpRate">立ち上がった割合（立ち上がり始めてからの時間 ÷ 立ち上がるのにかける時間）</param>
	public void UpdateAnimation(Animator animator, EnumManager.MinePlacePhaseTYPE minePlacePhase, float crouchRate, float standUpRate)
	{
		float time = CrouchStartTime;
		weight = NoWeight;
		leftHandFreeWeight = FullWeight;
		leftHandOutWeight = NoWeight;
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
				leftHandFreeWeight = Mathf.SmoothStep(FullWeight, NoWeight, standUpRate / LeftHandReturnEndRate);
				//立ち上がり始めと終わりは0、真ん中で1になるように、なめらかに増やして減らす（Sinの半周期）
				leftHandOutWeight = Mathf.Sin(Mathf.Clamp01(standUpRate) * Mathf.PI);
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
		weight = NoWeight;
		animator.SetLayerWeight(PlayerAnimatorParameter.PlaceMineLayerIndex, weight);
	}

	/// <summary>
	/// 地雷を置いている時に、アニメーションどおりの左手をずらす量を求める（地雷を置くアニメーションが混ざっている割合だけずらす）
	/// ・しゃがんでいる間は、両足の間の前へずらす（左手を銃へ戻すのに合わせて0へ戻す）
	/// ・立ち上がる時は、体の外側へずらす
	/// </summary>
	/// <param name="characterRotation">キャラクターの向き</param>
	/// <returns>左手をずらす量（ワールドの向き）</returns>
	public Vector3 GetLeftHandOffset(Quaternion characterRotation)
	{
		return characterRotation * (CrouchLeftHandOffset * leftHandFreeWeight + StandUpLeftHandOffset * leftHandOutWeight) * weight;
	}
}
