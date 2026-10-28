using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ハンドガンへの持ち替えのアニメーション
/// （アサルトライフル・ショットガンの弾が切れた時に、メインの銃を左手に持ち替えて、右手で太もものホルスターからハンドガンを抜く。
///   左手は腰の高さで、メインの銃の上側をカバンの取っ手のようにつかんで、銃口を前に向けて持つ（マックス・ペイン3の持ち方）。
///   銃は斜め（銃口が下、ストックが上）にして、指が銃にめり込まないように持つ。
///   しまう時は逆に、ハンドガンをホルスターに戻して、メインの銃を右手に持ち直す）
/// ※PlayerCharacterViewが持ち、銃（GunModelFacade）の持ち替えの段階を見て動かす（HandIKは、ここで求めた割合や手の位置・向きを使う）
/// </summary>
public class SideArmAnimation
{
	[Tooltip("手を動かさない時の割合")]
	const float NoWeight = 0.0f;
	[Tooltip("手を目標の位置・向きにぴったり合わせる時の割合")]
	const float FullWeight = 1.0f;
	[Tooltip("ハンドガンを抜く動き・しまう動きの、終わりの進み具合")]
	const float EndProgress = 1.0f;

	//↓メインの銃を左手で持つのに使う↓//
	//※左手は、片手で狙う時の下ろした腕の位置（HandGunOneHandAimLeftArmレイヤーのアニメーション）で、銃の上側をつかんで持つ。
	//  銃の向き（銃口を前に向けて斜めに下げる）を先に決めて、左手の向きは、銃をつかむ目印から求める（銃をつかむ位置・銃の向きは、銃ごとのアニメーションの設定（GunAnimation）で決める）
	[Tooltip("今のフレームで、左手に持っている（持ち替える）メインの銃のアニメーションの設定（ハンドガンに持ち替えていない時はnull）")]
	GunAnimation mainGunAnimation;
	[Tooltip("今のフレームで、右手をホルスターへ動かしている割合（1で、右手のハンドガンがホルスターのハンドガンと重なる）")]
	float rightHandHolsterWeight = NoWeight;
	public float RightHandHolsterWeight => rightHandHolsterWeight;
	[Tooltip("今のフレームで、左手をメインの銃を持つ位置・向きへ動かしている割合")]
	float leftHandCarryWeight = NoWeight;
	public float LeftHandCarryWeight => leftHandCarryWeight;
	[Tooltip("ハンドガンを抜いている間か？（左手は、持ち替える前の位置・向きから、メインの銃を持つ位置・向きへ動かす）")]
	bool isDrawing = false;
	public bool IsDrawing => isDrawing;
	[Tooltip("右手をホルスターへ動かす目標の、ハンドガンの位置（背骨を構えに合わせて曲げる分を戻した位置）")]
	Vector3 holsterPosition;
	[Tooltip("右手をホルスターへ動かす目標の、ハンドガンの向き（背骨を構えに合わせて曲げる分を戻した向き）")]
	Quaternion holsterRotation;
	[Tooltip("背骨を構えに合わせて曲げる分を戻す回転（IKの後のLateUpdateで曲げる分の逆）")]
	Quaternion inverseSpineAimRotation = Quaternion.identity;

	//↓メインの銃を持つ左手の指を開くのに使う↓//
	[Tooltip("メインの銃を持つ時に開く、左手の指の骨（5本の、付け根・中・先の骨）")]
	static readonly HumanBodyBones[] LeftFingerBones =
	{
		HumanBodyBones.LeftThumbProximal, HumanBodyBones.LeftThumbIntermediate, HumanBodyBones.LeftThumbDistal,
		HumanBodyBones.LeftIndexProximal, HumanBodyBones.LeftIndexIntermediate, HumanBodyBones.LeftIndexDistal,
		HumanBodyBones.LeftMiddleProximal, HumanBodyBones.LeftMiddleIntermediate, HumanBodyBones.LeftMiddleDistal,
		HumanBodyBones.LeftRingProximal, HumanBodyBones.LeftRingIntermediate, HumanBodyBones.LeftRingDistal,
		HumanBodyBones.LeftLittleProximal, HumanBodyBones.LeftLittleIntermediate, HumanBodyBones.LeftLittleDistal,
	};
	[Tooltip("左手の指の骨の、まっすぐ伸ばした時（Tポーズ）の向き（親の骨から見た向き）")]
	Quaternion[] leftFingerStraightLocalRotations;

	//↓銃のモデルの持ち替えに使う↓//
	[Tooltip("ハンドガンのビュー（手に持つハンドガンと、ホルスターのハンドガン）")]
	HandGunView handGunView;
	[Tooltip("アニメーションどおりの左手の位置・向きをIK用の向きで取り出すための目印（左手の骨の子）")]
	Transform leftHandAnimationPos;
	[Tooltip("左手に持ち替えたメインの銃のモデル（まだ持ち替えていない時はnull）")]
	Transform mainGunModel;
	[Tooltip("メインの銃を左手に持っているか？")]
	bool isMainGunInLeftHand = false;
	[Tooltip("メインの銃を右手に戻して、元の位置・向きへ動かしている間か？")]
	bool isMainGunReturning = false;
	[Tooltip("メインの銃を右手で持っている時の親（右手の骨）")]
	Transform mainGunRightHandParent;
	[Tooltip("メインの銃を右手で持っている時の位置（親から見た位置）")]
	Vector3 mainGunRightHandLocalPosition;
	[Tooltip("メインの銃を右手で持っている時の向き（親から見た向き）")]
	Quaternion mainGunRightHandLocalRotation;
	[Tooltip("両手で持っている銃の、左手の骨から見た位置（持ち替える前に、毎フレーム覚えておく。左手に持ち替えた時は、ここから握り直す）")]
	Vector3 mainGunGripLocalPosition = Vector3.zero;
	[Tooltip("両手で持っている銃の、左手の骨から見た向き（持ち替える前に、毎フレーム覚えておく）")]
	Quaternion mainGunGripLocalRotation = Quaternion.identity;
	[Tooltip("銃から見た、左手で銃をつかむ目印の向き")]
	Quaternion mainGunToGripRotation = Quaternion.identity;
	[Tooltip("左手で銃の上側をつかんで持っている時の、左手の骨から見た銃の位置")]
	Vector3 mainGunCarryLocalPosition = Vector3.zero;
	[Tooltip("左手で銃の上側をつかんで持っている時の、左手の骨から見た銃の向き")]
	Quaternion mainGunCarryLocalRotation = Quaternion.identity;
	[Tooltip("メインの銃を右手に戻し始めた時の位置（右手の骨から見た位置）")]
	Vector3 mainGunReturnStartLocalPosition;
	[Tooltip("メインの銃を右手に戻し始めた時の向き（右手の骨から見た向き）")]
	Quaternion mainGunReturnStartLocalRotation;

	/// <summary>
	/// 初期化
	/// </summary>
	/// <param name="handGunView">ハンドガンのビュー</param>
	/// <param name="leftHandAnimationPos">アニメーションどおりの左手の位置・向きをIK用の向きで取り出すための目印</param>
	/// <param name="animator">プレイヤーのアニメーター（左手の指の骨の、まっすぐ伸ばした時の向きを覚えておく）</param>
	public void Initialize(HandGunView handGunView, Transform leftHandAnimationPos, Animator animator)
	{
		this.handGunView = handGunView;
		this.leftHandAnimationPos = leftHandAnimationPos;

		//アバターに入っているTポーズの骨の向きから、指をまっすぐ伸ばした時の向きを探す
		SkeletonBone[] skeleton = animator.avatar.humanDescription.skeleton;
		leftFingerStraightLocalRotations = new Quaternion[LeftFingerBones.Length];
		for (int i = 0; i < LeftFingerBones.Length; i++)
		{
			string boneName = animator.GetBoneTransform(LeftFingerBones[i]).name;
			foreach (SkeletonBone skeletonBone in skeleton)
			{
				if (skeletonBone.name == boneName)
				{
					leftFingerStraightLocalRotations[i] = skeletonBone.rotation;
				}
			}
		}
	}

	/// <summary>
	/// 持ち替えの段階に合わせて、手を動かす割合と、右手をホルスターへ動かす目標を求める（手のIKの前に呼ぶ）
	/// ・抜く時：右手はホルスターへ行ってから、片手で狙う構えへ。左手はメインの銃を持ったまま下ろす（下ろしながら、銃の前側から銃の上側へ持ち直す）
	/// ・しまう時：右手はホルスターへ行ってから、メインの銃を持つ構えへ。左手は下ろした位置から、メインの銃の前側へ戻す
	/// </summary>
	/// <param name="phase">ハンドガンに持ち替える動きの段階</param>
	/// <param name="progress">ハンドガンを抜く動き・しまう動きの進み具合（0が始まり、1が終わり）</param>
	/// <param name="spineAimRotation">背骨を構えに合わせて曲げる回転（IKの後のLateUpdateで曲げる分。ワールドの向き）</param>
	/// <param name="spinePosition">構えに合わせて曲げる背骨の骨の位置</param>
	/// <param name="mainGunAnimation">左手に持っている（持ち替える）メインの銃のアニメーションの設定（ハンドガンに持ち替えていない時はnull）</param>
	public void UpdateWeights(EnumManager.SideArmPhaseTYPE phase, float progress, Quaternion spineAimRotation, Vector3 spinePosition, GunAnimation mainGunAnimation)
	{
		this.mainGunAnimation = mainGunAnimation;
		rightHandHolsterWeight = NoWeight;
		leftHandCarryWeight = NoWeight;
		isDrawing = false;
		switch (phase)
		{
			case EnumManager.SideArmPhaseTYPE.Draw:
				rightHandHolsterWeight = GetHolsterWeight(progress);
				leftHandCarryWeight = Mathf.SmoothStep(NoWeight, FullWeight, progress);
				isDrawing = true;
				break;
			case EnumManager.SideArmPhaseTYPE.Aim:
				leftHandCarryWeight = FullWeight;
				break;
			case EnumManager.SideArmPhaseTYPE.Holster:
				rightHandHolsterWeight = GetHolsterWeight(progress);
				leftHandCarryWeight = Mathf.SmoothStep(FullWeight, NoWeight, Mathf.InverseLerp(GunModelFacade.SideArmHolsterReachRate, EndProgress, progress));
				break;
			case EnumManager.SideArmPhaseTYPE.None:
				break;
		}

		//※背骨は、IKの後（LateUpdate）で構えに合わせて曲げるため、そのままホルスターを目標にすると、曲げた後の右手がホルスターからずれる。
		//  そのため、曲げる分を戻した位置・向きを目標にする（曲げた後に、ちょうどホルスターに来る）
		Transform holster = handGunView.GetHandGunModelBodyDecoration.transform;
		inverseSpineAimRotation = Quaternion.Inverse(spineAimRotation);
		holsterPosition = spinePosition + inverseSpineAimRotation * (holster.position - spinePosition);
		holsterRotation = inverseSpineAimRotation * holster.rotation;
	}

	/// <summary>
	/// 右手をホルスターへ動かす割合を求める（抜く動き・しまう動きの真ん中で、ちょうどホルスターに着く）
	/// </summary>
	/// <param name="progress">ハンドガンを抜く動き・しまう動きの進み具合</param>
	/// <returns>右手をホルスターへ動かす割合</returns>
	float GetHolsterWeight(float progress)
	{
		if (progress < GunModelFacade.SideArmHolsterReachRate)
		{
			return Mathf.SmoothStep(NoWeight, FullWeight, progress / GunModelFacade.SideArmHolsterReachRate);
		}

		return Mathf.SmoothStep(FullWeight, NoWeight, Mathf.InverseLerp(GunModelFacade.SideArmHolsterReachRate, EndProgress, progress));
	}

	/// <summary>
	/// 右手をホルスターへ動かす時の、右手（とハンドガン）を右手の位置を中心に回す回転を求める（右手のハンドガンが、ホルスターのハンドガンと同じ向きになる）
	/// </summary>
	/// <returns>右手を回す回転（ワールドの向き）</returns>
	public Quaternion GetRightHandHolsterRotation()
	{
		return holsterRotation * Quaternion.Inverse(handGunView.GetHandGunModel.transform.rotation);
	}

	/// <summary>
	/// 右手をホルスターへ動かす時の、右手の位置を求める（右手のハンドガンが、ホルスターのハンドガンの位置に来る）
	/// </summary>
	/// <param name="handRotation">右手（とハンドガン）を右手の位置を中心に回す回転</param>
	/// <param name="rightHandPosition">アニメーションどおりの右手の位置</param>
	/// <returns>右手の位置</returns>
	public Vector3 GetRightHandHolsterPosition(Quaternion handRotation, Vector3 rightHandPosition)
	{
		//右手を中心に回した後の、ハンドガンから右手までの位置関係のまま、ハンドガンをホルスターの位置へ動かす
		return holsterPosition + handRotation * (rightHandPosition - handGunView.GetHandGunModel.transform.position);
	}

	/// <summary>
	/// メインの銃を左手で持っている時に、下ろした左手をずらす量を求める
	/// </summary>
	/// <param name="characterRotation">キャラクターの向き</param>
	/// <returns>左手をずらす量（ワールドの向き）</returns>
	public Vector3 GetLeftHandCarryOffset(Quaternion characterRotation)
	{
		return characterRotation * mainGunAnimation.SideArmCarryLeftHandOffset;
	}

	/// <summary>
	/// メインの銃を左手で持っている時の、左手の向き（IK用の手の向き）を求める
	/// （銃を決めた向き（銃口を前に向けて斜めに下げる）にした時に、左手が銃をつかむ目印に重なる向き）
	/// ※背骨は、IKの後（LateUpdate）で構えに合わせて曲げるので、曲げた後に銃が決めた向きになるように、曲げる分を戻した向きにする
	/// </summary>
	/// <param name="characterRotation">キャラクターの向き</param>
	/// <returns>左手の向き（ワールドの向き）</returns>
	public Quaternion GetLeftHandCarryRotation(Quaternion characterRotation)
	{
		Vector3 angle = mainGunAnimation.SideArmCarryGunAngle;
		Quaternion gunRotation = characterRotation * Quaternion.Euler(angle.x, angle.y, -angle.z);
		return inverseSpineAimRotation * gunRotation * mainGunToGripRotation;
	}

	/// <summary>
	/// メインの銃を左手で持っている時に、左手の指を銃の太さに合わせて開く（手のIKの処理の中で呼ぶ）
	/// （アニメーションどおりの握りこぶしのままだと、太い銃に指がめり込むため、指の骨をまっすぐ伸ばした向きへ近づける）
	/// </summary>
	/// <param name="animator">プレイヤーのアニメーター</param>
	public void OpenLeftHandFingers(Animator animator)
	{
		if (leftHandCarryWeight <= NoWeight)
		{
			return;
		}

		//左手をメインの銃を持つ位置・向きへ動かしている割合だけ、指を開く
		float curl = Mathf.Lerp(FullWeight, mainGunAnimation.SideArmCarryFingerCurl, leftHandCarryWeight);
		for (int i = 0; i < LeftFingerBones.Length; i++)
		{
			Transform bone = animator.GetBoneTransform(LeftFingerBones[i]);
			animator.SetBoneLocalRotation(LeftFingerBones[i], Quaternion.Slerp(leftFingerStraightLocalRotations[i], bone.localRotation, curl));
		}
	}

	/// <summary>
	/// 持ち替えの段階に合わせて、銃のモデルを持ち替える（アニメーションで手が動いた後に呼ぶ）
	/// ・ハンドガン：右手がホルスターに着いた時に、ホルスターのハンドガンと右手のハンドガンを入れ替える
	/// ・メインの銃：抜き始めに左手に持ち替えて、抜く間に銃の前側から銃の上側へ持ち直す。しまう時に右手がホルスターに着いたら、右手に戻して元の位置・向きへ動かす
	/// </summary>
	/// <param name="gunModelFacade">銃</param>
	/// <param name="gunViewFacade">ガンビュー ファサード</param>
	/// <param name="hand_l">左手の骨</param>
	public void UpdateModels(GunModelFacade gunModelFacade, GunViewFacade gunViewFacade, Transform hand_l)
	{
		EnumManager.SideArmPhaseTYPE phase = gunModelFacade.SideArmPhase;
		float progress = gunModelFacade.SideArmPhaseProgress;
		bool isPastHolster = GunModelFacade.SideArmHolsterReachRate <= progress;
		bool isHandGunInHand = phase == EnumManager.SideArmPhaseTYPE.Aim || (phase == EnumManager.SideArmPhaseTYPE.Draw && isPastHolster == true) || (phase == EnumManager.SideArmPhaseTYPE.Holster && isPastHolster == false);
		bool isMainGunCarried = phase == EnumManager.SideArmPhaseTYPE.Draw || phase == EnumManager.SideArmPhaseTYPE.Aim || (phase == EnumManager.SideArmPhaseTYPE.Holster && isPastHolster == false);

		//※持ち替えていない時のハンドガンの見せ方は、銃を切り替えた時のまま（GunViewFacade.SwitchWeaponModel）
		if (phase != EnumManager.SideArmPhaseTYPE.None)
		{
			handGunView.GetHandGunModel.SetActive(isHandGunInHand);
			handGunView.GetHandGunModelBodyDecoration.SetActive(isHandGunInHand == false);
		}
		else if (isMainGunReturning == false)
		{
			//持ち替える前は、両手で持っている銃の、左手から見た位置・向きを覚えておく
			//（抜き始めのフレームでは、アニメーションが切り替わって右手（と銃）が先に動いてしまうため、1つ前のフレームの位置・向きで左手に持たせる）
			Transform gunModel = gunViewFacade.FindGunModelTransform(gunModelFacade.GetGunTYPE);
			mainGunGripLocalPosition = hand_l.InverseTransformPoint(gunModel.position);
			mainGunGripLocalRotation = Quaternion.Inverse(hand_l.rotation) * gunModel.rotation;
		}

		if (isMainGunCarried == true && isMainGunInLeftHand == false)
		{
			HoldMainGunInLeftHand(gunViewFacade.FindGunModelTransform(gunModelFacade.SideArmMainGunTYPE), hand_l);
		}
		else if (isMainGunCarried == false && isMainGunInLeftHand == true)
		{
			HoldMainGunInRightHand();
		}

		//抜く間は、左手に持ったメインの銃を、銃の前側を握った位置・向きから、銃の上側をつかんだ位置・向きへ動かす（左手を下ろしながら持ち直す）
		float regripRate = FullWeight;
		if (phase == EnumManager.SideArmPhaseTYPE.Draw)
		{
			regripRate = Mathf.SmoothStep(NoWeight, FullWeight, progress);
		}
		MoveMainGunInLeftHand(regripRate);

		//しまう時は、右手がホルスターに着いてから終わりまでの間に、右手に戻したメインの銃を元の位置・向きへ動かす
		//（ステージを移る時などに、持ち替えを途中でやめた時は、すぐに元の位置・向きへ戻す）
		float returnRate = EndProgress;
		if (phase == EnumManager.SideArmPhaseTYPE.Holster)
		{
			returnRate = Mathf.InverseLerp(GunModelFacade.SideArmHolsterReachRate, EndProgress, progress);
		}
		MoveMainGunToRightHand(returnRate);
	}

	/// <summary>
	/// メインの銃を左手に持ち替える（1つ前のフレームで両手で持っていた時の、左手から見た位置・向きのまま、左手の骨の子にする。この後は左手と一緒に動く）
	/// ※左手で銃の上側をつかんだ時の、左手から見た銃の位置・向き（持ち直した後の位置・向き）も、ここで求めておく
	/// </summary>
	/// <param name="gunModel">メインの銃のモデル</param>
	/// <param name="hand_l">左手の骨</param>
	void HoldMainGunInLeftHand(Transform gunModel, Transform hand_l)
	{
		mainGunModel = gunModel;
		mainGunRightHandParent = gunModel.parent;
		mainGunRightHandLocalPosition = gunModel.localPosition;
		mainGunRightHandLocalRotation = gunModel.localRotation;
		gunModel.SetParent(hand_l, false);
		gunModel.localPosition = mainGunGripLocalPosition;
		gunModel.localRotation = mainGunGripLocalRotation;

		//左手のIK用の向きの目印が、銃の「左手で銃をつかむ目印」に重なる時の、銃の位置・向き（左手の骨から見た位置・向き）
		Transform gripPos = mainGunAnimation.SideArmLeftHandGripPos;
		mainGunToGripRotation = Quaternion.Inverse(gunModel.rotation) * gripPos.rotation;
		Vector3 gunToGripPosition = Quaternion.Inverse(gunModel.rotation) * (gripPos.position - gunModel.position);
		mainGunCarryLocalRotation = leftHandAnimationPos.localRotation * Quaternion.Inverse(mainGunToGripRotation);
		mainGunCarryLocalPosition = leftHandAnimationPos.localPosition - mainGunCarryLocalRotation * gunToGripPosition;
		isMainGunInLeftHand = true;
		isMainGunReturning = false;
	}

	/// <summary>
	/// メインの銃を右手に戻す（今の見た目の位置・向きのまま、親だけを右手の骨に戻す。元の位置・向きへはMoveMainGunToRightHandで動かす）
	/// </summary>
	void HoldMainGunInRightHand()
	{
		mainGunModel.SetParent(mainGunRightHandParent, true);
		mainGunReturnStartLocalPosition = mainGunModel.localPosition;
		mainGunReturnStartLocalRotation = mainGunModel.localRotation;
		isMainGunInLeftHand = false;
		isMainGunReturning = true;
	}

	/// <summary>
	/// 左手に持ったメインの銃を、銃の前側を握った位置・向きから、銃の上側をつかんだ位置・向きへ動かす
	/// </summary>
	/// <param name="regripRate">持ち直した割合（0で銃の前側を握ったまま、1で銃の上側をつかんだ位置・向き）</param>
	void MoveMainGunInLeftHand(float regripRate)
	{
		if (isMainGunInLeftHand == false)
		{
			return;
		}

		mainGunModel.localPosition = Vector3.Lerp(mainGunGripLocalPosition, mainGunCarryLocalPosition, regripRate);
		mainGunModel.localRotation = Quaternion.Slerp(mainGunGripLocalRotation, mainGunCarryLocalRotation, regripRate);
	}

	/// <summary>
	/// 右手に戻したメインの銃を、元の位置・向きへ動かす
	/// </summary>
	/// <param name="returnRate">元の位置・向きへ動かした割合（0で右手に戻した時のまま、1で元の位置・向き）</param>
	void MoveMainGunToRightHand(float returnRate)
	{
		if (isMainGunReturning == false)
		{
			return;
		}

		float rate = Mathf.SmoothStep(NoWeight, FullWeight, returnRate);
		mainGunModel.localPosition = Vector3.Lerp(mainGunReturnStartLocalPosition, mainGunRightHandLocalPosition, rate);
		mainGunModel.localRotation = Quaternion.Slerp(mainGunReturnStartLocalRotation, mainGunRightHandLocalRotation, rate);
		if (EndProgress <= returnRate)
		{
			isMainGunReturning = false;
		}
	}
}
