using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ハンドガンの持ち方のIK（HandIKが使う）
/// ・構えていない時：片手（右手）だけで持って走る時に、銃口が上を向きすぎないように右手首を回す
/// ・構え始めの少しの間：片手（右手）で狙う（上半身を左にひねって、右腕を正面へまっすぐ伸ばす）
/// ・狙っている敵が近い時：近距離の構え（C.A.R.システムのような構え）にする
/// ・両手で構えている時：銃口を少し上げる。両腕を伸ばして、銃を少し上げる
/// </summary>
public class HandGunIK
{
	[Tooltip("IKを効かせない時の重み（アニメーションどおりの手）")]
	const float NoIKWeight = 0.0f;
	[Tooltip("IKを最大まで効かせる時の重み（目印にぴったり合わせた手）")]
	const float FullIKWeight = 1.0f;
	[Tooltip("プレイヤーのアニメーター")]
	readonly Animator animator;
	[Tooltip("アニメーションどおりの右手の位置・向きをIK用の向きで取り出すための目印")]
	readonly Transform rightHandAnimationPos;
	[Tooltip("上半身のひねり（片手で狙う時に使う）")]
	readonly UpperBodyTwist upperBodyTwist;

	//↓構えていない時に、片手（右手）だけで持つのに使う↓//
	[Tooltip("片手で持って走っている時に、銃口を上に向けてよい一番大きい角度（度。水平が0で、上向きがプラス）")]
	const float MaxMuzzleElevation = -10.0f;
	[Tooltip("銃口の角度を抑え始める、上限の手前の幅（度）（上限の手前からなめらかに抑えて、銃口がいきなり止まって見えないようにする）")]
	const float MuzzleElevationSoftRange = 20.0f;
	[Tooltip("今のフレームで、構えていない時に片手（右手）だけで持っている割合")]
	float oneHandLocomotionWeight = NoIKWeight;
	public float OneHandLocomotionWeight => oneHandLocomotionWeight;

	//↓構え始めた時に、片手（右手）で狙うのに使う↓//
	[Tooltip("今のフレームで、構えているアニメーションが混ざっている割合")]
	float aimWeight = NoIKWeight;
	[Tooltip("今のフレームで、片手で狙っている割合")]
	float oneHandAimWeight = NoIKWeight;
	public float OneHandAimWeight => oneHandAimWeight;
	[Tooltip("片手で狙う時に、下ろした左手をずらす量（m。ひねる前のキャラクターから見た向きで、xが右、yが上、zが前）（ひねった上半身に左腕がめり込まないように、体の外側へ離す）")]
	static readonly Vector3 OneHandAimLeftHandOffset = new Vector3(-0.16f, 0.0f, 0.03f);
	[Tooltip("片手で狙う時の、右腕の伸ばし具合（肩から手までの距離が、腕の長さのどれだけになるか。1でまっすぐ伸ばしきった腕）")]
	const float OneHandAimArmStretch = 0.95f;
	[Tooltip("片手で狙う時に、上半身を左にひねる角度（度）（右肩を狙う方向へ出して、伸ばした右腕と背中の向きをそろえる）")]
	const float OneHandAimTwistAngle = 80.0f;
	[Tooltip("両手で構えている時に、銃口を上げる角度（度）（両手で構えるアニメーションのままだと、銃口が弾の飛ぶ向き（カメラの正面）より3.5°ほど下を向くため）")]
	const float TwoHandAimRaiseAngle = 3.5f;
	[Tooltip("両手で構えている時の、右腕の伸ばし具合（肩から手までの距離が、腕の長さのどれだけになるか。1でまっすぐ伸ばしきった腕）（両手で構えるアニメーションのままだと、0.87ほどで肘が曲がっているため）")]
	const float TwoHandAimArmStretch = 0.96f;
	[Tooltip("両手で構えている時に、右肩を中心にして右腕を上へ回す角度（度）（両手で構えるアニメーションのままだと、銃が胸の高さにあるため、顔の高さへ近づける）")]
	const float TwoHandAimArmRaiseAngle = 8.0f;

	//↓狙っている敵が近い時に、近距離の構え（C.A.R.システムのような構え）にするのに使う↓//
	[Tooltip("今のフレームで、近距離の構えにしている割合")]
	float closeQuarterAimWeight = NoIKWeight;
	public float CloseQuarterAimWeight => closeQuarterAimWeight;
	[Tooltip("近距離の構えの時に、両手で構えるアニメーションの銃の位置からずらす量（m。キャラクターから見た向きで、xが右、yが上、zが前）（銃を顔の前の近くへ引き寄せて、両肘を曲げて構える）")]
	static readonly Vector3 CloseQuarterAimGunOffset = new Vector3(0.05f, 0.07f, -0.22f);
	[Tooltip("近距離の構えの時に、銃を内側（左）へ傾ける角度（度。銃の上側を、まっすぐ上から左へ倒す角度）")]
	const float CloseQuarterAimCantAngle = 45.0f;
	[Tooltip("近距離の構えの時に、右肘を向ける位置（m。右肩からのずれで、キャラクターから見た向き）（右肘は体の外側の少し下へ張り出す）")]
	public static readonly Vector3 CloseQuarterAimRightElbowHintOffset = new Vector3(0.30f, -0.15f, -0.05f);
	[Tooltip("近距離の構えの時に、左肘を向ける位置（m。左肩からのずれで、キャラクターから見た向き）（左肘は横へ張り出して、左腕が胸の前を横切るようにする）")]
	public static readonly Vector3 CloseQuarterAimLeftElbowHintOffset = new Vector3(-0.30f, -0.05f, 0.05f);

	/// <summary>
	/// コンストラクタ
	/// </summary>
	/// <param name="animator">プレイヤーのアニメーター</param>
	/// <param name="rightHandAnimationPos">アニメーションどおりの右手の位置・向きをIK用の向きで取り出すための目印</param>
	/// <param name="upperBodyTwist">上半身のひねり</param>
	public HandGunIK(Animator animator, Transform rightHandAnimationPos, UpperBodyTwist upperBodyTwist)
	{
		this.animator = animator;
		this.rightHandAnimationPos = rightHandAnimationPos;
		this.upperBodyTwist = upperBodyTwist;
	}

	/// <summary>
	/// それぞれの持ち方が、今どれだけ混ざっているかを求める
	/// ※ハンドガン以外を持っている時は、ハンドガンのステートが混ざっていないので、どれも0になる
	/// </summary>
	/// <param name="noReloadWeight">リロードしていない割合（リロード中はハンドガンの持ち方をやめる）</param>
	public void UpdateWeights(float noReloadWeight)
	{
		//構えていない時は、片手（右手）だけで持つ
		oneHandLocomotionWeight = PlayerAnimatorParameter.GetStateWeight(animator, PlayerAnimatorParameter.LocomotionLayerIndex, PlayerAnimatorParameter.HandGunNormalIdleAndWalkStateHash) * noReloadWeight;
		//構えて狙っている敵が近い時は、近距離の構えにする
		aimWeight = PlayerAnimatorParameter.GetStateWeight(animator, PlayerAnimatorParameter.LocomotionLayerIndex, PlayerAnimatorParameter.HandGunAimIdleAndWalkStateHash) * noReloadWeight;
		float closeQuarterAim = animator.GetFloat(PlayerAnimatorParameter.CloseQuarterAim);
		closeQuarterAimWeight = closeQuarterAim * aimWeight;
		//構え始めた時は、少しの間だけ片手（右手）で狙う（左腕は、専用のレイヤーで下ろしておく）
		//（近距離の構えの時は、片手で狙わずに、すぐに両手で構える）
		oneHandAimWeight = animator.GetFloat(PlayerAnimatorParameter.OneHandAim) * aimWeight * (FullIKWeight - closeQuarterAim);
		animator.SetLayerWeight(PlayerAnimatorParameter.HandGunOneHandAimLeftArmLayerIndex, oneHandAimWeight);
	}

	/// <summary>
	/// ハンドガンの持ち方に合わせて、右手（と銃）を回す回転と、ずらす量を直す
	/// ※ref：渡した変数そのものを、この関数の中で書き換える
	/// </summary>
	/// <param name="handGunView">ハンドガンのビュー</param>
	/// <param name="relaxedRotation">右手（と銃）を右手の位置を中心に回す回転</param>
	/// <param name="relaxedOffset">右手（と銃）をずらす量</param>
	public void UpdateHoldStyle(HandGunView handGunView, ref Quaternion relaxedRotation, ref Vector3 relaxedOffset)
	{
		//片手で持って走っている時は、腕を前に振っても銃口が上を向かないように、右手首を回す
		if (NoIKWeight < oneHandLocomotionWeight)
		{
			relaxedRotation = Quaternion.Slerp(Quaternion.identity, GetMuzzleDownRotation(handGunView), oneHandLocomotionWeight) * relaxedRotation;
		}

		//片手で狙う時は、上半身を左にひねって右肩を前に出し、右腕を右肩の正面へまっすぐ伸ばして、銃口を正面に向ける
		if (NoIKWeight < oneHandAimWeight)
		{
			upperBodyTwist.Twist(-OneHandAimTwistAngle * oneHandAimWeight);
			//※右手（と銃）は背骨の子なので、ひねった分だけ動いた後の位置・向きから、腕を伸ばした位置・向きへ移す
			Vector3 twistedHandPosition = upperBodyTwist.TwistPoint(rightHandAnimationPos.position);
			Vector3 handPosition = Vector3.Lerp(twistedHandPosition, GetOneHandAimHandPosition(twistedHandPosition), oneHandAimWeight);
			Quaternion aimRotation = Quaternion.Slerp(Quaternion.identity, GetOneHandAimRotation(upperBodyTwist.GetTwistRotation() * GetMuzzleDirection(handGunView)), oneHandAimWeight);
			relaxedRotation = aimRotation * upperBodyTwist.GetTwistRotation() * relaxedRotation;
			relaxedOffset = relaxedOffset + handPosition - rightHandAnimationPos.position;
		}

		//近距離の構えの時は、銃を顔の前の近くへ引き寄せて、内側（左）へ傾ける（両手で持ったまま、両肘を曲げて構える）
		if (NoIKWeight < closeQuarterAimWeight)
		{
			Quaternion closeQuarterAimRotation = GetCloseQuarterAimRotation(handGunView.GetHandGunModel.transform);
			Vector3 handPosition = GetCloseQuarterAimHandPosition(handGunView.GetHandGunModel.transform, closeQuarterAimRotation);
			relaxedRotation = Quaternion.Slerp(relaxedRotation, closeQuarterAimRotation, closeQuarterAimWeight);
			relaxedOffset = Vector3.Lerp(relaxedOffset, handPosition - rightHandAnimationPos.position, closeQuarterAimWeight);
		}

		//両手で構えている時は（近距離の構えも含む）、銃口を少し上げる（両手で構えるアニメーションのままだと、銃口が弾の飛ぶ向きより少し下を向くため）
		//（片手で狙っている割合の分は上げない。右手の位置を中心に、キャラクターの右向きの軸で回す）
		float twoHandAimWeight = aimWeight - oneHandAimWeight;
		if (NoIKWeight < twoHandAimWeight)
		{
			relaxedRotation = Quaternion.AngleAxis(-TwoHandAimRaiseAngle * twoHandAimWeight, animator.transform.right) * relaxedRotation;
		}

		//両手で構えている時は、両腕を伸ばして、銃を少し上げる（銃の向きは変えずに、右手の位置だけを動かす。左手は銃の前側を握る目印に合わせるので、銃と一緒に動く）
		//（近距離の構えは銃を顔の前へ引き寄せる構えなので、近距離の構えの割合の分は動かさない）
		float twoHandAimArmWeight = twoHandAimWeight - closeQuarterAimWeight;
		if (NoIKWeight < twoHandAimArmWeight)
		{
			relaxedOffset = relaxedOffset + (GetTwoHandAimHandPosition() - rightHandAnimationPos.position) * twoHandAimArmWeight;
		}
	}

	/// <summary>
	/// 片手で狙う時に、下ろした左手をずらす量を求める（ひねった上半身に、左腕がめり込まないように、体の外側へ少し離す）
	/// </summary>
	/// <returns>左手をずらす量（ワールドの向き）</returns>
	public Vector3 GetOneHandAimLeftHandOffset()
	{
		return upperBodyTwist.GetTwistRotation() * (animator.transform.rotation * OneHandAimLeftHandOffset) * oneHandAimWeight;
	}

	/// <summary>
	/// 近距離の構えの時に、右手（と銃）を右手の位置を中心に回す回転を求める
	/// （銃身の左右の向きをキャラクターの前向きに合わせてから、銃身を軸にして、銃の上側を決めた角度まで左へ倒す）
	/// ※両手で構えるアニメーションの銃の向きから回すので、アニメーションの上下の揺れは残る
	/// </summary>
	/// <param name="gunTransform">ハンドガンのモデル（前向きが銃身、上向きが銃の上側）</param>
	/// <returns>右手（と銃）を回す回転</returns>
	Quaternion GetCloseQuarterAimRotation(Transform gunTransform)
	{
		Quaternion aimRotation = GetOneHandAimRotation(gunTransform.forward);
		Vector3 barrelDirection = aimRotation * gunTransform.forward;
		Vector3 uprightDirection = Vector3.ProjectOnPlane(animator.transform.up, barrelDirection);
		float cantAngle = Vector3.SignedAngle(uprightDirection, aimRotation * gunTransform.up, barrelDirection);
		return Quaternion.AngleAxis(CloseQuarterAimCantAngle - cantAngle, barrelDirection) * aimRotation;
	}

	/// <summary>
	/// 近距離の構えの時の右手の位置を求める
	/// （銃が、両手で構えるアニメーションの銃の位置から決めた量だけずれた位置に来るようにする）
	/// </summary>
	/// <param name="gunTransform">ハンドガンのモデル</param>
	/// <param name="closeQuarterAimRotation">近距離の構えの時に、右手（と銃）を右手の位置を中心に回す回転</param>
	/// <returns>右手の位置</returns>
	Vector3 GetCloseQuarterAimHandPosition(Transform gunTransform, Quaternion closeQuarterAimRotation)
	{
		Vector3 gunPosition = gunTransform.position + animator.transform.rotation * CloseQuarterAimGunOffset;
		//右手を中心に回した後の、銃から右手までの位置関係のまま、銃をずらした位置へ右手を動かす
		return gunPosition + closeQuarterAimRotation * (rightHandAnimationPos.position - gunTransform.position);
	}

	/// <summary>
	/// 片手で狙う時の右手の位置を求める
	/// （ひねった後の右肩から、手の高さはそのままで、キャラクターの前向きへ腕の長さ近くまで伸ばした位置）
	/// </summary>
	/// <param name="twistedHandPosition">上半身をひねった後の、アニメーションどおりの右手の位置</param>
	/// <returns>右手の位置</returns>
	Vector3 GetOneHandAimHandPosition(Vector3 twistedHandPosition)
	{
		Vector3 shoulderPosition = animator.GetBoneTransform(HumanBodyBones.RightUpperArm).position;
		float armLength = GetRightArmLength();
		Vector3 twistedShoulderPosition = upperBodyTwist.TwistPoint(shoulderPosition);
		Vector3 armDirection = (animator.transform.forward * armLength + Vector3.up * (twistedHandPosition.y - twistedShoulderPosition.y)).normalized;
		return twistedShoulderPosition + armDirection * armLength * OneHandAimArmStretch;
	}

	/// <summary>
	/// 両手で構えている時の右手の位置を求める
	/// （アニメーションどおりの右手を、右肩を中心にしてキャラクターの右向きの軸で上へ回し、腕の長さ近くまで伸ばした位置）
	/// </summary>
	/// <returns>右手の位置</returns>
	Vector3 GetTwoHandAimHandPosition()
	{
		Vector3 shoulderPosition = animator.GetBoneTransform(HumanBodyBones.RightUpperArm).position;
		Vector3 armDirection = (Quaternion.AngleAxis(-TwoHandAimArmRaiseAngle, animator.transform.right) * (rightHandAnimationPos.position - shoulderPosition)).normalized;
		return shoulderPosition + armDirection * GetRightArmLength() * TwoHandAimArmStretch;
	}

	/// <summary>
	/// 右腕の長さ（肩から肘まで ＋ 肘から手まで）を求める
	/// </summary>
	/// <returns>右腕の長さ（m）</returns>
	float GetRightArmLength()
	{
		Vector3 shoulderPosition = animator.GetBoneTransform(HumanBodyBones.RightUpperArm).position;
		Vector3 elbowPosition = animator.GetBoneTransform(HumanBodyBones.RightLowerArm).position;
		return Vector3.Distance(shoulderPosition, elbowPosition) + Vector3.Distance(elbowPosition, rightHandAnimationPos.position);
	}

	/// <summary>
	/// 片手で狙う時と近距離の構えの時に、銃口の左右の向きをキャラクターの前向きに合わせる回転を求める
	/// （両手で構えるアニメーションでは、銃が体の真ん中に寄っていて、銃口が少し内側を向いているため）
	/// </summary>
	/// <param name="muzzleDirection">今の銃口の向き</param>
	/// <returns>右手（と銃）を回す回転</returns>
	Quaternion GetOneHandAimRotation(Vector3 muzzleDirection)
	{
		return Quaternion.FromToRotation(Vector3.ProjectOnPlane(muzzleDirection, Vector3.up), Vector3.ProjectOnPlane(animator.transform.forward, Vector3.up));
	}

	/// <summary>
	/// ハンドガンの銃口の向きを求める
	/// </summary>
	/// <param name="handGunView">ハンドガンのビュー</param>
	/// <returns>銃口の向き（長さ1）</returns>
	Vector3 GetMuzzleDirection(HandGunView handGunView)
	{
		return (handGunView.HandGunMuzzleTransform.position - handGunView.GetHandGunModel.transform.position).normalized;
	}

	/// <summary>
	/// 銃口が上を向きすぎないように、右手（と銃）を右手の位置を中心に回す回転を求める
	/// （片手で持って走るアニメーションは、手に物を持って走る動きなので、そのままだと腕を前に振った時に銃口が45度ほど上を向いてしまうため）
	/// </summary>
	/// <param name="handGunView">ハンドガンのビュー</param>
	/// <returns>銃口を下げる回転</returns>
	Quaternion GetMuzzleDownRotation(HandGunView handGunView)
	{
		Vector3 muzzleDirection = GetMuzzleDirection(handGunView);
		float elevation = Mathf.Asin(muzzleDirection.y) * Mathf.Rad2Deg;
		float softLimitStartElevation = MaxMuzzleElevation - MuzzleElevationSoftRange;
		if (elevation <= softLimitStartElevation)
		{
			return Quaternion.identity;
		}

		//上限に近づくほどゆっくり上がり、上限は超えない角度に直す（Tanhは、どんなに大きな値でも1を超えない、なめらかな関数）
		float limitedElevation = softLimitStartElevation + MuzzleElevationSoftRange * (float)System.Math.Tanh((elevation - softLimitStartElevation) / MuzzleElevationSoftRange);
		//銃口の左右の向きはそのままで、上下の角度だけ下げる
		return Quaternion.AngleAxis(elevation - limitedElevation, Vector3.Cross(Vector3.up, muzzleDirection).normalized);
	}
}
