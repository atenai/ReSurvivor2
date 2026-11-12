using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 手のIK（アニメーションどおりの手の位置・向きを、銃の持ち方に合わせて直す）
/// ※PlayerAnimationFacadeが持ち、アニメーターのIKの処理（OnAnimatorIK）の中で、今持っている銃のアニメーションの設定を渡して動かす
/// ※アニメーターのIKパスは一番上のレイヤー（UpperBodyReload）だけにオンにしてある
///   （複数のレイヤーでオンにすると、IKの処理が何度も呼ばれ、前のレイヤーのIKの結果が「アニメーションどおりの左手」に混ざってしまうため）
/// ※リロード・武器の切り替え・地雷を置く動きは、手の動きをアニメーションクリップに焼き込んであるので、そのアニメーションが混ざっている割合だけIKを弱める
/// </summary>
[System.Serializable]
public class HandIK
{
	//※IKで使う手の向きは手の骨の向きそのものではなく、Unityのヒューマノイド共通の手の向きなので、
	//  アニメーションどおりの手の位置・向きをIKで使うには、骨の向きをIK用の向きに直した目印を使う
	[Tooltip("アニメーションどおりの左手の位置・向きをIK用の向きで取り出すための目印（左手の骨の子に置いた目印。位置は骨と同じで、向きだけIK用の手の向きにしてある）")]
	[SerializeField] Transform leftHandAnimationPos;
	public Transform LeftHandAnimationPos => leftHandAnimationPos;
	[Tooltip("アニメーションどおりの右手の位置・向きをIK用の向きで取り出すための目印（右手の骨の子に置いた目印。位置は骨と同じで、向きだけIK用の手の向きにしてある）")]
	[SerializeField] Transform rightHandAnimationPos;
	[Tooltip("両手のIKのゴール（焼き込んだアニメーションを再生している間は、まとめて弱める）")]
	static readonly AvatarIKGoal[] HandGoals = { AvatarIKGoal.LeftHand, AvatarIKGoal.RightHand };
	[Tooltip("両肘のIKのヒント（焼き込んだアニメーションを再生している間は、まとめて弱める）")]
	static readonly AvatarIKHint[] ElbowHints = { AvatarIKHint.LeftElbow, AvatarIKHint.RightElbow };
	[Tooltip("プレイヤーのアニメーター")]
	Animator animator;
	[Tooltip("IKを効かせない時の重み（アニメーションどおりの手）")]
	const float NoIKWeight = 0.0f;
	[Tooltip("IKを最大まで効かせる時の重み（目印にぴったり合わせた手）")]
	const float FullIKWeight = 1.0f;

	//↓構えていない時に、右手（と銃）を体から離すのに使う↓//
	[Tooltip("今のフレームで、右手（と銃）を右手の位置を中心に回す回転")]
	Quaternion relaxedRotation = Quaternion.identity;
	[Tooltip("今のフレームで、右手（と銃）をずらす量")]
	Vector3 relaxedOffset = Vector3.zero;
	[Tooltip("今のフレームで、片手（右手）だけで銃を持つ割合（0で両手持ち、1で片手持ち。左手は銃を握らず、アニメーションどおりにする）")]
	float oneHandWeight = NoIKWeight;
	//↓ダッシュしている時に、アサルトライフル・ショットガンを右手だけで縦に持ち上げるのに使う↓//
	[Tooltip("今のフレームで、銃を縦に持ち上げている割合（構えていない時の移動で、ダッシュのアニメーションが混ざっている割合）")]
	float dashRaiseWeight = NoIKWeight;
	[Tooltip("銃を縦に持ち上げている時に、右肘を向ける位置（m。右肩からのずれで、キャラクターから見た向き）（右肘は体の外側へ張り出す）")]
	static readonly Vector3 DashRaiseRightElbowHintOffset = new Vector3(0.35f, -0.15f, -0.05f);
	[Tooltip("右腕を走りのアニメーションにしておくレイヤーを、ダッシュの割合の何倍で効かせるか（ダッシュのアニメーションの右腕が混ざり始めるより早く、走りの右腕にしておく）")]
	const float DashRightArmWeightScale = 2.0f;
	//↓ハンドガンの持ち方に使う↓//
	[Tooltip("上半身のひねり（ハンドガンを片手で狙う時にひねる）")]
	UpperBodyTwist upperBodyTwist;
	/// <summary>
	/// 今のフレームで、上半身をひねった分の回転（構えに合わせて背骨を曲げる時に、ひねる前の向きで曲げる量を求めるのに使う）
	/// </summary>
	public Quaternion UpperBodyTwistRotation => upperBodyTwist.GetTwistRotation();
	/// <summary>
	/// 今のフレームで、ハンドガンを片手で狙っている割合（構えに合わせて背骨を曲げる量を、片手で狙う時は減らすのに使う）
	/// </summary>
	public float HandGunOneHandAimWeight => handGunIK.OneHandAimWeight;
	[Tooltip("ハンドガンの持ち方のIK")]
	HandGunIK handGunIK;
	//↓ハンドガンに持ち替える時に、左手を持ち替える前の位置から動かし始めるのに使う↓//
	[Tooltip("前のフレームまでの左手のIKの位置（キャラクターから見た位置）。ハンドガンを抜いている間は、抜き始める前の位置のまま変えない")]
	Vector3 drawStartLeftHandLocalPosition = Vector3.zero;
	[Tooltip("前のフレームまでの左手のIKの向きを、左の前腕から見た向き。ハンドガンを抜いている間は、抜き始める前の向きのまま変えない")]
	Quaternion drawStartLeftHandToForearmRotation = Quaternion.identity;

	/// <summary>
	/// 初期化
	/// </summary>
	/// <param name="animator">プレイヤーのアニメーター</param>
	public void Initialize(Animator animator)
	{
		this.animator = animator;
		upperBodyTwist = new UpperBodyTwist(animator);
		handGunIK = new HandGunIK(animator, rightHandAnimationPos, upperBodyTwist);
	}

	/// <summary>
	/// 手のIKを動かす（アニメーターのIKの処理（OnAnimatorIK）の中で呼ぶ）
	/// </summary>
	/// <param name="gunAnimation">今持っている銃のアニメーションの設定</param>
	/// <param name="gunModelTransform">今持っている銃のモデル（前向きが銃身、上向きが銃の上側）</param>
	/// <param name="handGunView">ハンドガンを持っている時は、ハンドガンのビュー（ほかの銃の時はnull）</param>
	/// <param name="sideArmAnimation">ハンドガンへの持ち替えのアニメーション</param>
	/// <param name="spineAimRotation">前のフレームで、構えに合わせて脊椎ボーンを曲げた回転（ワールドの向き）</param>
	/// <param name="spinePosition">前のフレームで、構えに合わせて曲げた脊椎ボーンの位置</param>
	/// <param name="aimRay">弾の飛ぶ線（カメラの位置から正面へ）</param>
	public void UpdateIK(GunAnimation gunAnimation, Transform gunModelTransform, HandGunView handGunView, SideArmAnimation sideArmAnimation, Quaternion spineAimRotation, Vector3 spinePosition, Ray aimRay)
	{
		UpdateHoldStyle(gunAnimation, gunModelTransform, handGunView, spineAimRotation, spinePosition, aimRay);
		UpdateHoldStyleForSideArm(sideArmAnimation);
		SetRightHandIK();
		//※近距離の構え（ハンドガン）と、銃を縦に持ち上げるダッシュ（アサルトライフル・ショットガン）は、同時には起きない
		if (NoIKWeight < dashRaiseWeight)
		{
			SetElbowHint(AvatarIKHint.RightElbow, HumanBodyBones.RightUpperArm, dashRaiseWeight, DashRaiseRightElbowHintOffset);
		}
		else
		{
			SetElbowHint(AvatarIKHint.RightElbow, HumanBodyBones.RightUpperArm, handGunIK.CloseQuarterAimWeight, HandGunIK.CloseQuarterAimRightElbowHintOffset);
		}
		SetElbowHint(AvatarIKHint.LeftElbow, HumanBodyBones.LeftUpperArm, handGunIK.CloseQuarterAimWeight, HandGunIK.CloseQuarterAimLeftElbowHintOffset);
		SetLeftHandIK(gunAnimation, sideArmAnimation);

		//リロード・武器の切り替え（上半身のリロードのレイヤーの、何もしていない時以外のステート）と、地雷を置く動き（地雷を置くレイヤー）は、
		//両手の動きをアニメーションクリップに焼き込んであるので、そのアニメーションが混ざっている割合だけIKを弱める
		//※焼き込んだアニメーションから何もしていないステートへ戻る間は、IKをすぐに効かせる
		//  （焼き込んだ動きの終わりは、いつもの持ち方の手と同じ位置なので手は跳ばず、戻る途中も両手が銃を握ったままになる）
		float bakedAnimationWeight = NoIKWeight;
		if (PlayerAnimatorParameter.IsGoingToState(animator, PlayerAnimatorParameter.UpperBodyReloadLayerIndex, PlayerAnimatorParameter.NoReloadStateHash) == false)
		{
			bakedAnimationWeight = FullIKWeight - PlayerAnimatorParameter.GetStateWeight(animator, PlayerAnimatorParameter.UpperBodyReloadLayerIndex, PlayerAnimatorParameter.NoReloadStateHash);
		}
		FadeOutIK(Mathf.Max(bakedAnimationWeight, animator.GetLayerWeight(PlayerAnimatorParameter.PlaceMineLayerIndex)));
	}

	/// <summary>
	/// 焼き込んだアニメーションを再生している割合だけ、両手と両肘のIKを弱める
	/// （焼き込んだ動きの始まりと終わりは、いつもの持ち方の手と同じ位置なので、ステートが切り替わる途中も手が跳ばない）
	/// </summary>
	/// <param name="bakedAnimationWeight">焼き込んだアニメーションが混ざっている割合（0でIKをそのまま効かせ、1でIKを使わない）</param>
	void FadeOutIK(float bakedAnimationWeight)
	{
		if (bakedAnimationWeight <= NoIKWeight)
		{
			return;
		}

		float keepWeight = FullIKWeight - bakedAnimationWeight;
		foreach (AvatarIKGoal goal in HandGoals)
		{
			animator.SetIKPositionWeight(goal, animator.GetIKPositionWeight(goal) * keepWeight);
			animator.SetIKRotationWeight(goal, animator.GetIKRotationWeight(goal) * keepWeight);
		}
		foreach (AvatarIKHint hint in ElbowHints)
		{
			animator.SetIKHintPositionWeight(hint, animator.GetIKHintPositionWeight(hint) * keepWeight);
		}
	}

	/// <summary>
	/// 銃の持ち方を求める
	/// ・構えていない時に、右手（と銃）を体から離す量（構えていない時のアニメーションは体の細いキャラクター用に作られたもので、そのままだと右手や銃や左腕が防弾ベストやポーチにめり込むため）
	/// ・アサルトライフル・ショットガンを、右手だけで縦に持ち上げる割合（ダッシュしている時）
	/// ・ハンドガンの持ち方（片手で持つ割合・片手で狙う構え・近距離の構え。HandGunIKが求める）
	/// ※それぞれの動きがどれだけ混ざっているかに合わせて効かせるので、構えた時や構えをやめた時は、アニメーションと一緒になめらかに切り替わる
	///   （立ち止まっている時・歩いている時は「移動のレイヤーのステート」が混ざっている割合。リロード中・武器の切り替え中は、焼き込んだアニメーションの手を使うので効かせない）
	/// </summary>
	/// <param name="gunAnimation">今持っている銃のアニメーションの設定（銃ごとに体から離す量が違う）</param>
	/// <param name="gunModelTransform">今持っている銃のモデル（前向きが銃身、上向きが銃の上側）</param>
	/// <param name="handGunView">ハンドガンを持っている時は、ハンドガンのビュー（ほかの銃の時はnull）</param>
	/// <param name="spineAimRotation">前のフレームで、構えに合わせて脊椎ボーンを曲げた回転（ワールドの向き）</param>
	/// <param name="spinePosition">前のフレームで、構えに合わせて曲げた脊椎ボーンの位置</param>
	/// <param name="aimRay">弾の飛ぶ線（カメラの位置から正面へ）</param>
	void UpdateHoldStyle(GunAnimation gunAnimation, Transform gunModelTransform, HandGunView handGunView, Quaternion spineAimRotation, Vector3 spinePosition, Ray aimRay)
	{
		float relaxedLocomotionWeight = PlayerAnimatorParameter.GetStateWeight(animator, PlayerAnimatorParameter.LocomotionLayerIndex, PlayerAnimatorParameter.NormalIdleAndWalkStateHash);
		float noReloadWeight = PlayerAnimatorParameter.GetStateWeight(animator, PlayerAnimatorParameter.UpperBodyReloadLayerIndex, PlayerAnimatorParameter.NoReloadStateHash);
		Quaternion characterRotation = animator.transform.rotation;
		Quaternion relaxedLocomotionRotation = characterRotation * Quaternion.Euler(gunAnimation.RelaxedRightHandAngle) * Quaternion.Inverse(characterRotation);
		float relaxedWeight = relaxedLocomotionWeight * noReloadWeight;
		relaxedRotation = Quaternion.Slerp(Quaternion.identity, relaxedLocomotionRotation, relaxedWeight);
		relaxedOffset = characterRotation * gunAnimation.RelaxedRightHandOffset * relaxedWeight;

		//ダッシュしている時は（アサルトライフル・ショットガン）、右手だけで銃を縦に持ち上げる（左手は銃から離して、アニメーションどおりに振る）
		//（構えていない時の移動のステートで、ダッシュのアニメーションが混ざっている割合だけ効かせる。ハンドガンは別のステートなので、0になる）
		float dash = animator.GetFloat(PlayerAnimatorParameter.Dash);
		dashRaiseWeight = dash * relaxedWeight;
		//※ダッシュのアニメーションは、手に小さい物を持って腕を大きく振る動きなので、右腕をそのまま使うと、
		//  ・銃を持ち上げる途中で、振っている腕の向きから回すため、銃の向きが一瞬ひっくり返る
		//  ・右手が小さく握った拳になり、銃のグリップを握らない
		//  そのため、右腕と右手の指だけは、専用のレイヤーで走りのアニメーション（銃を持つ腕）にしておき、そこから銃を持ち上げる
		animator.SetLayerWeight(PlayerAnimatorParameter.DashRightArmLayerIndex, Mathf.Clamp01(dash * DashRightArmWeightScale) * relaxedWeight);
		if (NoIKWeight < dashRaiseWeight)
		{
			Quaternion dashRaiseRotation = GetDashRaiseRotation(gunModelTransform, gunAnimation.DashGunAngle);
			Vector3 handPosition = animator.GetBoneTransform(HumanBodyBones.RightUpperArm).position + characterRotation * gunAnimation.DashRightHandPosition;
			relaxedRotation = Quaternion.Slerp(relaxedRotation, dashRaiseRotation, dashRaiseWeight);
			relaxedOffset = Vector3.Lerp(relaxedOffset, handPosition - rightHandAnimationPos.position, dashRaiseWeight);
		}

		//ハンドガンの持ち方が混ざっている割合を求めて、片手（右手）だけで銃を持つ割合を決める
		//（ハンドガンの構えていない時の移動のステート・片手で狙う構え・銃を縦に持ち上げるダッシュが混ざっている割合）
		handGunIK.UpdateWeights(noReloadWeight);
		oneHandWeight = handGunIK.OneHandLocomotionWeight + handGunIK.OneHandAimWeight + dashRaiseWeight;

		upperBodyTwist.ResetTwist();
		if (handGunView == null)
		{
			return;
		}

		handGunIK.UpdateHoldStyle(handGunView, spineAimRotation, spinePosition, aimRay, ref relaxedRotation, ref relaxedOffset);
	}

	/// <summary>
	/// ハンドガンを抜く時・しまう時に、右手を太もものホルスターへ動かす
	/// （ホルスターに着いた時に、右手のハンドガンがホルスターのハンドガンとぴったり重なるようにする。そこで、見せるハンドガンを入れ替える）
	/// </summary>
	/// <param name="sideArmAnimation">ハンドガンへの持ち替えのアニメーション</param>
	void UpdateHoldStyleForSideArm(SideArmAnimation sideArmAnimation)
	{
		float holsterWeight = sideArmAnimation.RightHandHolsterWeight;
		if (holsterWeight <= NoIKWeight)
		{
			return;
		}

		Quaternion holsterRotation = sideArmAnimation.GetRightHandHolsterRotation();
		Vector3 handPosition = sideArmAnimation.GetRightHandHolsterPosition(holsterRotation, rightHandAnimationPos.position);
		relaxedRotation = Quaternion.Slerp(relaxedRotation, holsterRotation, holsterWeight);
		relaxedOffset = Vector3.Lerp(relaxedOffset, handPosition - rightHandAnimationPos.position, holsterWeight);
	}

	/// <summary>
	/// ダッシュしている時に、銃を縦に持ち上げるように、右手（と銃）を右手の位置を中心に回す回転を求める
	/// （銃口をまっすぐ上に向けて銃の上側を後ろに向けた向きから、決めた角度だけ傾けた向きにする）
	/// </summary>
	/// <param name="gunTransform">銃のモデル（前向きが銃身、上向きが銃の上側）</param>
	/// <param name="gunAngle">銃の傾き（度。xが銃口を前へ倒す角度、yが銃身を軸に銃の上側を右へ回す角度、zが銃口を右へ倒す角度）</param>
	/// <returns>右手（と銃）を回す回転</returns>
	Quaternion GetDashRaiseRotation(Transform gunTransform, Vector3 gunAngle)
	{
		//キャラクターから見た向きで、まっすぐ上に向けた銃を、前（右向きの軸で回す）と右（前向きの軸で逆に回す）へ倒してから、銃身を軸にひねる
		Quaternion uprightRotation = Quaternion.LookRotation(Vector3.up, Vector3.back);
		Quaternion tiltRotation = Quaternion.AngleAxis(gunAngle.x, Vector3.right) * Quaternion.AngleAxis(-gunAngle.z, Vector3.forward);
		Quaternion gunRotation = animator.transform.rotation * tiltRotation * uprightRotation * Quaternion.AngleAxis(-gunAngle.y, Vector3.forward);
		return gunRotation * Quaternion.Inverse(gunTransform.rotation);
	}

	/// <summary>
	/// キャラクターの右手の位置と角度を合わせる（構えていない時だけ、アニメーションどおりの右手を体から離す。銃は右手の子なので一緒に動く）
	/// </summary>
	void SetRightHandIK()
	{
		if (relaxedOffset == Vector3.zero && relaxedRotation == Quaternion.identity)
		{
			animator.SetIKPositionWeight(AvatarIKGoal.RightHand, NoIKWeight);
			animator.SetIKRotationWeight(AvatarIKGoal.RightHand, NoIKWeight);
			return;
		}

		animator.SetIKPositionWeight(AvatarIKGoal.RightHand, FullIKWeight);
		animator.SetIKRotationWeight(AvatarIKGoal.RightHand, FullIKWeight);
		animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandAnimationPos.position + relaxedOffset);
		animator.SetIKRotation(AvatarIKGoal.RightHand, relaxedRotation * rightHandAnimationPos.rotation);
	}

	/// <summary>
	/// 近距離の構えの時と、銃を縦に持ち上げている時に、肘を曲げる向きを決める（肘を、肩から決めた量だけずれた位置の方へ曲げる）
	/// ※IKで手を体に引き寄せたり持ち上げたりすると、そのままでは肘が下に垂れて、二の腕が体にめり込むため
	/// </summary>
	/// <param name="elbowHint">肘</param>
	/// <param name="upperArmBone">肘と同じ側の肩（二の腕の付け根）の骨</param>
	/// <param name="weight">肘の向きを決める効き具合（0でIKに任せる、1で決めた位置の方へ曲げる）</param>
	/// <param name="offset">肘を向ける位置の、肩からのずれ（キャラクターから見た向き）</param>
	void SetElbowHint(AvatarIKHint elbowHint, HumanBodyBones upperArmBone, float weight, Vector3 offset)
	{
		animator.SetIKHintPositionWeight(elbowHint, weight);
		if (weight <= NoIKWeight)
		{
			return;
		}

		animator.SetIKHintPosition(elbowHint, animator.GetBoneTransform(upperArmBone).position + animator.transform.rotation * offset);
	}

	/// <summary>
	/// キャラクターの左手の位置と角度を合わせる
	/// ※IKの重みを1未満にすると、手が目印とアニメーションの間のどこに来るかがはっきりしないため、
	///   重みは1のままにして、合わせる位置・角度の方を「アニメーションどおりの左手 → 銃の前側を握る目印」の順に混ぜる
	/// ※銃の子にある目印は、右手を体から離す前の銃の位置にあるので、右手と同じだけ動かしてから使う
	/// ※片手で持つ時（ハンドガンを構えていない時）は、左手で銃の前側を握らない
	/// ※ハンドガンに持ち替えている時は、左手でメインの銃を持つ（抜いている間は、持ち替える前の左手から、メインの銃を持つ左手へ動かす）
	/// </summary>
	/// <param name="gunAnimation">今持っている銃のアニメーションの設定（左手で銃の前側を握る目印を使う）</param>
	/// <param name="sideArmAnimation">ハンドガンへの持ち替えのアニメーション</param>
	void SetLeftHandIK(GunAnimation gunAnimation, SideArmAnimation sideArmAnimation)
	{
		float gripWeight = FullIKWeight - oneHandWeight;
		float carryWeight = sideArmAnimation.LeftHandCarryWeight;
		if (gripWeight <= NoIKWeight && handGunIK.OneHandAimWeight <= NoIKWeight && carryWeight <= NoIKWeight && sideArmAnimation.IsDrawing == false)
		{
			animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, NoIKWeight);
			animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, NoIKWeight);
			return;
		}

		//※上半身をひねっている時は、アニメーションどおりの左手も、ひねった分だけ動いた後の位置・向きにする
		//※片手で狙っている時は、下ろした左手を体の外側へ少し離す（ひねった上半身に、左腕がめり込まないようにする）
		Transform leftHandGripPos = gunAnimation.LeftHandGripPos;
		Vector3 animationPosition = upperBodyTwist.TwistPoint(leftHandAnimationPos.position) + handGunIK.GetOneHandAimLeftHandOffset();
		Quaternion rotation = upperBodyTwist.GetTwistRotation() * leftHandAnimationPos.rotation;
		Vector3 targetPosition = Vector3.Lerp(animationPosition, MoveAwayFromBody(leftHandGripPos.position), gripWeight);
		Quaternion targetRotation = Quaternion.Slerp(rotation, relaxedRotation * leftHandGripPos.rotation, gripWeight);

		//ハンドガンを抜いている間は、抜き始める前の左手（メインの銃の前側を握った手）の位置から動かし、
		//左手の向きは、抜き始める前の手首の曲げ方のまま、左の前腕（上半身をひねった分も含む、アニメーションどおりの向き）と一緒に回す
		//（メインの銃は、抜き始めた時に左手の子にしたので、左手と一緒に動く）
		Quaternion characterRotation = animator.transform.rotation;
		Vector3 characterPosition = animator.transform.position;
		Quaternion forearmRotation = upperBodyTwist.GetTwistRotation() * animator.GetBoneTransform(HumanBodyBones.LeftLowerArm).rotation;
		if (sideArmAnimation.IsDrawing == true)
		{
			targetPosition = characterPosition + characterRotation * drawStartLeftHandLocalPosition;
			targetRotation = forearmRotation * drawStartLeftHandToForearmRotation;
		}
		else
		{
			drawStartLeftHandLocalPosition = Quaternion.Inverse(characterRotation) * (targetPosition - characterPosition);
			drawStartLeftHandToForearmRotation = Quaternion.Inverse(forearmRotation) * targetRotation;
		}

		//メインの銃を左手で持っている時は、下ろした左手で、銃口を前に向けて斜め（銃口が下、ストックが上）にした銃の上側をつかんで持つ
		//（銃は左手の子なので、左手と一緒に動く）
		//※向きは、左の前腕から見た向き（手首の曲げ方）で混ぜて、前腕と一緒に回す（そのまま混ぜると、腕を下ろす途中で手首が大きく折れるため）
		if (NoIKWeight < carryWeight)
		{
			Vector3 carryPosition = animationPosition + sideArmAnimation.GetLeftHandCarryOffset(characterRotation);
			Quaternion carryRotation = sideArmAnimation.GetLeftHandCarryRotation(characterRotation);
			Quaternion inverseForearmRotation = Quaternion.Inverse(forearmRotation);
			targetPosition = Vector3.Lerp(targetPosition, carryPosition, carryWeight);
			targetRotation = forearmRotation * Quaternion.Slerp(inverseForearmRotation * targetRotation, inverseForearmRotation * carryRotation, carryWeight);
		}

		animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, FullIKWeight);
		animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, FullIKWeight);
		animator.SetIKPosition(AvatarIKGoal.LeftHand, targetPosition);
		animator.SetIKRotation(AvatarIKGoal.LeftHand, targetRotation);
	}

	/// <summary>
	/// 銃と一緒に動く位置（銃の子にある目印の位置）を、右手を体から離した後の位置に直す
	/// </summary>
	/// <param name="position">右手を体から離す前の位置</param>
	/// <returns>右手を体から離した後の位置</returns>
	Vector3 MoveAwayFromBody(Vector3 position)
	{
		//アニメーションどおりの右手の位置を中心に回してから、ずらす
		Vector3 rightHandPosition = rightHandAnimationPos.position;
		return rightHandPosition + relaxedRotation * (position - rightHandPosition) + relaxedOffset;
	}
}
