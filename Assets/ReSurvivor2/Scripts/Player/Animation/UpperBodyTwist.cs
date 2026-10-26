using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 上半身（背骨）を、キャラクターの上向きを軸にしてひねる（ハンドガンを片手で狙う時に使う）
/// ※顔は正面を向いたままにするため、首と頭は逆向きに戻す
/// ※IKのパスの中だけで使える、骨の回転を書き換える関数（SetBoneLocalRotation）を使うので、HandIKのIKの処理の中で使う
/// </summary>
public class UpperBodyTwist
{
	[Tooltip("プレイヤーのアニメーター")]
	readonly Animator animator;
	[Tooltip("上半身をひねる時に回す背骨の骨（下から順）")]
	readonly HumanBodyBones[] twistSpineBones = { HumanBodyBones.Spine, HumanBodyBones.Chest, HumanBodyBones.UpperChest };
	[Tooltip("上半身をひねる時に、背骨の骨それぞれが受け持つ割合（合計で1。背骨全体で少しずつ回して、自然にひねる）")]
	readonly float[] twistSpineRatios = { 0.3f, 0.35f, 0.35f };
	[Tooltip("上半身をひねっても顔は正面を向くように、ひねりと逆向きに回す首と頭の骨")]
	readonly HumanBodyBones[] twistHeadBones = { HumanBodyBones.Neck, HumanBodyBones.Head };
	[Tooltip("首と頭の骨それぞれが、ひねりを打ち消す割合（合計で1）")]
	readonly float[] twistHeadRatios = { 0.5f, 0.5f };
	[Tooltip("ひねっていない時の角度")]
	const float NoTwistAngle = 0.0f;
	[Tooltip("今のフレームで、上半身をひねっている角度（度。マイナスが左）")]
	float upperBodyTwistAngle = NoTwistAngle;
	[Tooltip("今のフレームで、ひねる前の背骨の骨の位置（ひねりの中心）")]
	Vector3[] twistSpinePositions;

	/// <summary>
	/// コンストラクタ
	/// </summary>
	/// <param name="animator">プレイヤーのアニメーター</param>
	public UpperBodyTwist(Animator animator)
	{
		this.animator = animator;
	}

	/// <summary>
	/// 上半身をひねっていない状態にする（毎フレーム、ひねる前に呼ぶ）
	/// </summary>
	public void ResetTwist()
	{
		upperBodyTwistAngle = NoTwistAngle;
	}

	/// <summary>
	/// 上半身（背骨）を、キャラクターの上向きを軸にしてひねる（顔は正面を向いたままにするため、首と頭は逆向きに戻す）
	/// </summary>
	/// <param name="angle">ひねる角度（度。マイナスが左）</param>
	public void Twist(float angle)
	{
		upperBodyTwistAngle = angle;
		if (twistSpinePositions == null)
		{
			twistSpinePositions = new Vector3[twistSpineBones.Length];
		}
		Vector3 axis = animator.transform.up;
		for (int i = 0; i < twistSpineBones.Length; i++)
		{
			Transform bone = animator.GetBoneTransform(twistSpineBones[i]);
			twistSpinePositions[i] = bone.position;
			RotateBone(twistSpineBones[i], Quaternion.AngleAxis(angle * twistSpineRatios[i], axis));
		}

		for (int i = 0; i < twistHeadBones.Length; i++)
		{
			RotateBone(twistHeadBones[i], Quaternion.AngleAxis(-angle * twistHeadRatios[i], axis));
		}
	}

	/// <summary>
	/// 骨を、今のアニメーションの向きから、ワールドの向きでさらに回す
	/// （親の骨も回している時は、親の回転に重ねて回る）
	/// </summary>
	/// <param name="humanBodyBone">回す骨</param>
	/// <param name="worldRotation">ワールドの向きで回す回転</param>
	void RotateBone(HumanBodyBones humanBodyBone, Quaternion worldRotation)
	{
		Transform bone = animator.GetBoneTransform(humanBodyBone);
		animator.SetBoneLocalRotation(humanBodyBone, bone.localRotation * Quaternion.Inverse(bone.rotation) * worldRotation * bone.rotation);
	}

	/// <summary>
	/// 上半身をひねる前の位置（腕や手など、背骨の子の位置）を、ひねった後の位置に直す
	/// </summary>
	/// <param name="position">ひねる前の位置</param>
	/// <returns>ひねった後の位置</returns>
	public Vector3 TwistPoint(Vector3 position)
	{
		if (upperBodyTwistAngle == NoTwistAngle)
		{
			return position;
		}

		//背骨の骨を下から順に、その骨の位置を中心に回す
		//（回すたびに、ひねった後の骨の位置へ中心を合わせる。下の骨を回すと、上の骨の位置も一緒に回るため）
		Vector3 axis = animator.transform.up;
		for (int i = 0; i < twistSpinePositions.Length; i++)
		{
			Vector3 pivot = TwistSpinePosition(i);
			position = pivot + Quaternion.AngleAxis(upperBodyTwistAngle * twistSpineRatios[i], axis) * (position - pivot);
		}
		return position;
	}

	/// <summary>
	/// 上半身をひねった後の、背骨の骨の位置を求める（その骨より下の骨を回した分だけ動いた位置）
	/// </summary>
	/// <param name="index">背骨の骨の番号（下から順）</param>
	/// <returns>ひねった後の骨の位置</returns>
	Vector3 TwistSpinePosition(int index)
	{
		Vector3 axis = animator.transform.up;
		Vector3 position = twistSpinePositions[index];
		for (int i = 0; i < index; i++)
		{
			Vector3 pivot = TwistSpinePosition(i);
			position = pivot + Quaternion.AngleAxis(upperBodyTwistAngle * twistSpineRatios[i], axis) * (position - pivot);
		}
		return position;
	}

	/// <summary>
	/// 上半身をひねった分の回転（背骨の子の向きは、この分だけ回る）を求める
	/// </summary>
	/// <returns>ひねった分の回転</returns>
	public Quaternion GetTwistRotation()
	{
		return Quaternion.AngleAxis(upperBodyTwistAngle, animator.transform.up);
	}
}
