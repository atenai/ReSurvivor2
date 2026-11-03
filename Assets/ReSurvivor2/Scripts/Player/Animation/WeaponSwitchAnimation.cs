using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 武器を切り替える時のアニメーション
/// （今持っている銃を、右手で体のしまう場所（ハンドガンは右太もものホルスター、アサルトライフルは左の腰、ショットガンは背中）へしまってから、
///   右手を切り替える先の銃へ伸ばして、しまう場所から取り出して持つ）
/// ※銃は、右手（銃を握る位置）が、銃ごとに決めた中継点の方へ膨らんだ曲線を通るように動かす（取り出す時は、同じ曲線を逆にたどる）
///   銃の位置・向きは、背骨を構えに合わせて曲げた後（LateUpdate）に決めるので、しまい終わった時・取り出し始めた時に、しまってある銃とぴったり重なる
/// ※右手は、IKで銃を持つ位置へ動かす。右手で銃の前側を持つ目印がある銃（ショットガン）は、右手をグリップから銃の前側へ持ち替えてから動かす
///   （ショットガンは、右脇の下から右手で銃の前側を持って背中との間を動かし、前に出した所で左手に前側を持たせて、右手はグリップに持ち替える）
/// ※左手は銃から離して、片手で狙う時と同じように下ろす（左の腰へしまうアサルトライフルは、右手をグリップから離して、左手で銃の前側を握ったまま持っていく）
/// ※PlayerCharacterViewが持ち、銃（GunModelFacade）の切り替えの段階を見て動かす（HandIKは、ここで求めた両手の位置・向きと、左手を銃から離す割合を使う）
/// </summary>
public class WeaponSwitchAnimation
{
	[Tooltip("手を動かさない時の割合")]
	const float NoWeight = 0.0f;
	[Tooltip("手を目標の位置・向きにぴったり合わせる時の割合")]
	const float FullWeight = 1.0f;
	[Tooltip("曲線の上の、右手で銃を持っている位置")]
	const float HoldPathRate = 0.0f;
	[Tooltip("曲線の上の、体のしまう場所")]
	const float StowPathRate = 1.0f;
	[Tooltip("銃をしまう動き・取り出す動きの、終わりの進み具合")]
	const float EndProgress = 1.0f;
	[Tooltip("取り出す動きのうち、右手を前の銃をしまった所から、取り出す銃をつかむ所へ伸ばし終わる割合（ここから、銃を持って曲線をたどる）")]
	const float TakeOutCatchRate = 0.4f;
	[Tooltip("しまう動きのうち、左手を銃から離して下ろし終わる割合")]
	const float LeftHandReleaseRate = 0.3f;
	[Tooltip("取り出す動きのうち、左手を銃へ戻し始める割合")]
	const float LeftHandRegripRate = 0.6f;
	[Tooltip("右手を、前の銃をしまった所から取り出す銃をつかむ所へ伸ばす時に、体の前へ膨らませる量（m。キャラクターから見た向きで、xが右、yが上、zが前）（まっすぐ動かすと、右手が腰や太ももにめり込むため）")]
	static readonly Vector3 HandTransferBulge = new Vector3(0.0f, 0.0f, 0.2f);
	[Tooltip("右手を伸ばす時に膨らませる所（前の銃をしまった所が0、取り出す銃をつかむ所が1。真ん中で膨らませる）")]
	const float HandTransferBulgeRate = 0.5f;
	[Tooltip("曲線の真ん中（持っている向きとしまう向きのちょうど間の向きを求めるのに使う）")]
	const float PathMiddleRate = 0.5f;
	//↓右手で銃の前側を持って動かす銃（ショットガン）に使う（曲線の上の位置で決める）↓//
	//※しまう時は、左手で銃の前側を握ったまま、右手をグリップから銃の前側へ持ち替えて、それから左手を離す（取り出す時は逆に、左手で銃の前側を握ってから、右手をグリップへ持ち替える）
	[Tooltip("右手をグリップから銃の前側へ持ち替え終わる、曲線の上の位置")]
	const float RightHandForendSlideRate = 0.15f;
	[Tooltip("右手を銃の前側へ持ち替えた後に、左手を銃から離して下ろし終わる、曲線の上の位置")]
	const float LeftHandForendReleaseRate = 0.3f;
	//↓左手でしまう場所まで持っていく銃（アサルトライフル）に使う↓//
	[Tooltip("右手をグリップから離していく長さ（曲線の上の位置。右手をグリップから離し終わる所の、この分だけ手前から離し始める）")]
	const float RightHandReleaseBlendRate = 0.2f;
	[Tooltip("右手をグリップから離した後に、いつもどおりに銃を持つ右手の位置からずらす量（m。キャラクターから見た向きで、xが右、yが上、zが前）（銃を持っていない右手が体の前に浮かないように、体の横へ下ろす）")]
	static readonly Vector3 ReleasedRightHandOffset = new Vector3(0.12f, -0.08f, 0.02f);
	[Tooltip("取り出す時に、左手をしまう場所の銃へ伸ばす途中で、体の外側へ膨らませる量（m。キャラクターから見た向きで、xが右、yが上、zが前）（まっすぐ動かすと、左手が腰に埋もれるため）")]
	static readonly Vector3 LeftHandCatchBulge = new Vector3(-0.15f, 0.0f, 0.05f);
	[Tooltip("左手を伸ばす途中で膨らませる量を求める係数（離している割合をwとして、4×w×(1-w)。真ん中で1になる）")]
	const float LeftHandCatchBulgeScale = 4.0f;
	[Tooltip("左手を銃の前側の下から外側へ持ち替えていく長さ（曲線の上の位置。右手をグリップから離し始める所の、この分だけ手前から持ち替え始める）")]
	const float LeftHandCarryGripBlendRate = 0.2f;

	[Tooltip("背骨のボーン（構えに合わせて曲げる骨。しまう場所がこの骨の子かどうかで、曲げる分を戻すかを決める）")]
	Transform spine;
	[Tooltip("キャラクター（アニメーターのゲームオブジェクト）")]
	Transform character;
	[Tooltip("右肩（二の腕の付け根）の骨（中継点・途中の姿勢は、右肩からのずれで決める）")]
	Transform rightUpperArm;

	//↓IKの処理の中で求める↓//
	[Tooltip("武器を切り替えている間か？")]
	bool isActive = false;
	public bool IsActive => isActive;
	[Tooltip("武器を切り替える動きの段階")]
	EnumManager.WeaponSwitchPhaseTYPE phase = EnumManager.WeaponSwitchPhaseTYPE.None;
	[Tooltip("今のフレームで、左手を銃から離して下ろしている割合")]
	float leftHandFreeWeight = NoWeight;
	public float LeftHandFreeWeight => leftHandFreeWeight;
	[Tooltip("今のフレームで、銃が曲線の上のどこにあるか（右手で持っている位置が0、しまう場所が1）")]
	float gunPathRate = HoldPathRate;
	[Tooltip("今のフレームで、右手を前の銃をしまった所から、取り出す銃をつかむ所へ伸ばした割合（1で、銃を持っている）")]
	float handTransferRate = FullWeight;
	[Tooltip("今のフレームで、右手をグリップから銃の前側へ持ち替えた割合（0でグリップ、1で銃の前側）")]
	float rightHandForendWeight = NoWeight;
	[Tooltip("今のフレームで、右手をグリップから離した割合（0でグリップを握る、1でいつもどおりに銃を持つ右手の位置へ戻す）")]
	float rightHandReleaseWeight = NoWeight;
	[Tooltip("今のフレームで、左手を銃の前側の下から外側へ持ち替えた割合（0で下から握る、1で外側から持つ）")]
	float leftHandCarryGripWeight = NoWeight;
	[Tooltip("今のフレームの、切り替えている銃のアニメーションの設定")]
	GunAnimation gunAnimation;
	[Tooltip("今のフレームで、位置・向きを動かしている銃のモデル（右手に持つ方。切り替えていない時はnull）")]
	Transform gunModel;
	[Tooltip("今のフレームの、体のしまう場所に付けている銃のモデル")]
	Transform stowedGunModel;
	[Tooltip("動かしている銃のモデルの、右手で持っている時の位置（親（右手の骨）から見た位置）")]
	Vector3 gunHoldLocalPosition;
	[Tooltip("動かしている銃のモデルの、右手で持っている時の向き（親から見た向き）")]
	Quaternion gunHoldLocalRotation;
	[Tooltip("右手（IK用の手の向きの目印）から見た、銃の位置")]
	Vector3 handToGunPosition = Vector3.zero;
	[Tooltip("右手（IK用の手の向きの目印）から見た、銃の向き")]
	Quaternion handToGunRotation = Quaternion.identity;
	[Tooltip("背骨を構えに合わせて曲げる分を戻す回転（IKの後のLateUpdateで曲げる分の逆）")]
	Quaternion inverseSpineAimRotation = Quaternion.identity;
	[Tooltip("構えに合わせて曲げる背骨の骨の位置")]
	Vector3 spinePosition = Vector3.zero;
	[Tooltip("今のフレームで、いつもどおりに銃を持つ右手の位置（キャラクターから見た位置。背骨を構えに合わせて曲げる前）")]
	Vector3 holdHandLocalPosition = Vector3.zero;
	[Tooltip("今のフレームで、いつもどおりに銃を持つ右手の向き（キャラクターから見た向き。背骨を構えに合わせて曲げる前）")]
	Quaternion holdHandLocalRotation = Quaternion.identity;
	[Tooltip("前の銃をしまう時に、右手が最後にいた所の位置（キャラクターから見た位置。背骨を構えに合わせて曲げる前）")]
	Vector3 putAwayHandLocalPosition = Vector3.zero;
	[Tooltip("前の銃をしまう時に、右手が最後にいた所の向き（キャラクターから見た向き。背骨を構えに合わせて曲げる前）")]
	Quaternion putAwayHandLocalRotation = Quaternion.identity;
	[Tooltip("前のフレームで、背中にしまう銃の位置・向きを覚えた、しまってある銃のモデル（覚えていない時はnull）")]
	Transform bentStowedGunModel;
	[Tooltip("前のフレームの、背中にしまう銃の位置（キャラクターから見た位置。背骨を構えに合わせて曲げる前に戻した位置）")]
	Vector3 bentStowedGunLocalPosition = Vector3.zero;
	[Tooltip("前のフレームの、背中にしまう銃の向き（キャラクターから見た向き。背骨を構えに合わせて曲げる前に戻した向き）")]
	Quaternion bentStowedGunLocalRotation = Quaternion.identity;
	[Tooltip("今のフレームで、右肘の向きを決める効き具合（しまう場所に近いほど強くする）")]
	float rightElbowHintWeight = NoWeight;
	public float RightElbowHintWeight => rightElbowHintWeight;
	/// <summary>
	/// 右肘を向ける位置（右肩からのずれで、キャラクターから見た向き）
	/// </summary>
	public Vector3 RightElbowHintOffset => gunAnimation.WeaponSwitchRightElbowHintOffset;
	[Tooltip("右手のIKの目標の位置（背骨を構えに合わせて曲げる前）")]
	Vector3 rightHandPosition;
	public Vector3 RightHandPosition => rightHandPosition;
	[Tooltip("右手のIKの目標の向き（背骨を構えに合わせて曲げる前）")]
	Quaternion rightHandRotation;
	public Quaternion RightHandRotation => rightHandRotation;
	[Tooltip("左手で銃の前側を握る時の、左手のIKの目標の位置（動かしている銃に合わせた位置。背骨を構えに合わせて曲げる前）")]
	Vector3 leftHandGripPosition;
	public Vector3 LeftHandGripPosition => leftHandGripPosition;
	[Tooltip("左手で銃の前側を握る時の、左手のIKの目標の向き（動かしている銃に合わせた向き。背骨を構えに合わせて曲げる前）")]
	Quaternion leftHandGripRotation;
	public Quaternion LeftHandGripRotation => leftHandGripRotation;

	/// <summary>
	/// 初期化
	/// </summary>
	/// <param name="spine">構えに合わせて曲げる背骨の骨</param>
	/// <param name="character">キャラクター（アニメーターのゲームオブジェクト）</param>
	/// <param name="rightUpperArm">右肩（二の腕の付け根）の骨</param>
	public void Initialize(Transform spine, Transform character, Transform rightUpperArm)
	{
		this.spine = spine;
		this.character = character;
		this.rightUpperArm = rightUpperArm;
	}

	/// <summary>
	/// 切り替えの段階に合わせて、銃が曲線の上のどこにあるかと、手を動かす割合を求める（手のIKの前に呼ぶ）
	/// ・しまう時：右手で銃を持ったまま、曲線をしまう場所までたどる。左手は始めに銃から離して下ろす
	/// ・取り出す時：右手を前の銃をしまった所から、取り出す銃をつかむ所へ伸ばしてから、銃を持って曲線を逆にたどる。左手は終わりに銃へ戻す
	/// ※右手で銃の前側を持って動かす銃は、曲線の上の位置に合わせて、右手を銃の前側とグリップの間で持ち替え、左手を離したり握ったりする
	/// ※前のフレームで動かした銃は、ここで右手で持っている位置・向きに戻す（IKの中では、銃が右手で持っている位置にある前提で手の位置を求めるため）
	/// </summary>
	/// <param name="phase">武器を切り替える動きの段階</param>
	/// <param name="progress">銃をしまう動き・取り出す動きの進み具合（0が始まり、1が終わり）</param>
	/// <param name="gunAnimation">切り替えている銃（しまう時はしまう銃、取り出す時は取り出す銃）のアニメーションの設定</param>
	/// <param name="gunModel">切り替えている銃の、右手に持つ方のモデル</param>
	/// <param name="stowedGunModel">切り替えている銃の、体のしまう場所に付けている方のモデル</param>
	/// <param name="spineAimRotation">前のフレームで、構えに合わせて脊椎ボーンを曲げた回転（ワールドの向き）</param>
	/// <param name="spinePosition">前のフレームで、構えに合わせて曲げた脊椎ボーンの位置</param>
	public void UpdateWeights(EnumManager.WeaponSwitchPhaseTYPE phase, float progress, GunAnimation gunAnimation, Transform gunModel, Transform stowedGunModel, Quaternion spineAimRotation, Vector3 spinePosition)
	{
		if (this.gunModel != null)
		{
			this.gunModel.localPosition = gunHoldLocalPosition;
			this.gunModel.localRotation = gunHoldLocalRotation;
			this.gunModel = null;
		}

		this.phase = phase;
		isActive = phase != EnumManager.WeaponSwitchPhaseTYPE.None;
		leftHandFreeWeight = NoWeight;
		rightElbowHintWeight = NoWeight;
		rightHandForendWeight = NoWeight;
		rightHandReleaseWeight = NoWeight;
		leftHandCarryGripWeight = NoWeight;
		if (isActive == false)
		{
			//前の切り替えで覚えた、背中にしまう銃の位置・向きは使わない
			bentStowedGunModel = null;
			return;
		}

		this.gunAnimation = gunAnimation;
		this.gunModel = gunModel;
		this.stowedGunModel = stowedGunModel;
		gunHoldLocalPosition = gunModel.localPosition;
		gunHoldLocalRotation = gunModel.localRotation;
		inverseSpineAimRotation = Quaternion.Inverse(spineAimRotation);
		this.spinePosition = spinePosition;

		switch (phase)
		{
			case EnumManager.WeaponSwitchPhaseTYPE.PutAway:
				gunPathRate = Mathf.SmoothStep(HoldPathRate, StowPathRate, progress);
				handTransferRate = FullWeight;
				leftHandFreeWeight = Mathf.SmoothStep(NoWeight, FullWeight, progress / LeftHandReleaseRate);
				break;
			case EnumManager.WeaponSwitchPhaseTYPE.TakeOut:
				//右手を取り出す銃へ伸ばす間は、銃はしまう場所に置いたまま
				handTransferRate = Mathf.SmoothStep(NoWeight, FullWeight, progress / TakeOutCatchRate);
				gunPathRate = Mathf.Lerp(StowPathRate, HoldPathRate, Mathf.SmoothStep(NoWeight, FullWeight, Mathf.InverseLerp(TakeOutCatchRate, EndProgress, progress)));
				leftHandFreeWeight = Mathf.SmoothStep(FullWeight, NoWeight, Mathf.InverseLerp(LeftHandRegripRate, EndProgress, progress));
				break;
		}

		//右手で銃の前側を持って動かす銃は、曲線の上の位置に合わせて、右手を持ち替え、左手を離す・握る
		if (gunAnimation.WeaponSwitchRightHandForendGripPos != null)
		{
			rightHandForendWeight = Mathf.SmoothStep(NoWeight, FullWeight, gunPathRate / RightHandForendSlideRate);
			leftHandFreeWeight = Mathf.SmoothStep(NoWeight, FullWeight, Mathf.InverseLerp(RightHandForendSlideRate, LeftHandForendReleaseRate, gunPathRate));
		}

		//左手でしまう場所まで持っていく銃は、しまう場所へ近づいたら右手をグリップから離し、左手は銃の前側を握ったままにする
		//（取り出す時は、右手を伸ばす間に左手でしまう場所の銃をつかみ、前に出してから右手でグリップを握る）
		float releaseRate = gunAnimation.WeaponSwitchRightHandReleaseRate;
		if (releaseRate < StowPathRate)
		{
			float releaseStartRate = releaseRate - RightHandReleaseBlendRate;
			rightHandReleaseWeight = Mathf.SmoothStep(NoWeight, FullWeight, Mathf.InverseLerp(releaseStartRate, releaseRate, gunPathRate));
			//左手で銃を外側から持つ目印がある銃は、右手を離す前に、左手を銃の前側の下から外側へ持ち替える（取り出す時は、右手でグリップを握ってから下へ戻す）
			if (gunAnimation.WeaponSwitchLeftHandCarryGripPos != null)
			{
				leftHandCarryGripWeight = Mathf.SmoothStep(NoWeight, FullWeight, Mathf.InverseLerp(releaseStartRate - LeftHandCarryGripBlendRate, releaseStartRate, gunPathRate));
			}
			leftHandFreeWeight = NoWeight;
			if (phase == EnumManager.WeaponSwitchPhaseTYPE.TakeOut)
			{
				leftHandFreeWeight = Mathf.SmoothStep(FullWeight, NoWeight, progress / TakeOutCatchRate);
			}
		}
	}

	/// <summary>
	/// 両手のIKの目標の位置・向きを求める（手のIKの処理の中で、いつもどおりの持ち方を求めた後に呼ぶ）
	/// </summary>
	/// <param name="holdHandPosition">いつもどおりに銃を持つ右手の位置（IKの目標の位置）</param>
	/// <param name="holdHandRotation">いつもどおりに銃を持つ右手の向き（IKの目標の向き）</param>
	/// <param name="rightHandAnimationPos">アニメーションどおりの右手の位置・向きをIK用の向きで取り出すための目印（銃と同じ右手の骨の子）</param>
	/// <param name="rightShoulderPosition">右肩の位置（上半身をひねっている時は、ひねった後の位置）</param>
	public void UpdateHandTargets(Vector3 holdHandPosition, Quaternion holdHandRotation, Transform rightHandAnimationPos, Vector3 rightShoulderPosition)
	{
		Quaternion characterRotation = character.rotation;
		Quaternion inverseCharacterRotation = Quaternion.Inverse(characterRotation);
		Vector3 characterPosition = character.position;
		Quaternion inverseRightHandRotation = Quaternion.Inverse(rightHandAnimationPos.rotation);
		handToGunRotation = inverseRightHandRotation * gunModel.rotation;
		handToGunPosition = inverseRightHandRotation * (gunModel.position - rightHandAnimationPos.position);
		holdHandLocalPosition = inverseCharacterRotation * (holdHandPosition - characterPosition);
		holdHandLocalRotation = inverseCharacterRotation * holdHandRotation;

		//※背骨は、IKの後（LateUpdate）で構えに合わせて曲げるため、背骨の子ではないしまう場所（太もも）は、曲げる分を戻した位置・向きを目標にする
		//※背中のしまう場所は首の骨の子で、IKの後に背骨と一緒に曲がり、構えた時は首と一緒に回るので、
		//  前のフレームで曲げた後の位置・向きから、曲げる分を戻した位置・向きを目標にする（覚えていない最初のフレームは、今の位置・向きのまま）
		Vector3 stowedGunPosition = stowedGunModel.position;
		Quaternion stowedGunRotation = stowedGunModel.rotation;
		if (stowedGunModel.IsChildOf(spine) == false)
		{
			stowedGunPosition = spinePosition + inverseSpineAimRotation * (stowedGunPosition - spinePosition);
			stowedGunRotation = inverseSpineAimRotation * stowedGunRotation;
		}
		else if (bentStowedGunModel == stowedGunModel)
		{
			stowedGunPosition = characterPosition + characterRotation * bentStowedGunLocalPosition;
			stowedGunRotation = characterRotation * bentStowedGunLocalRotation;
		}
		Quaternion stowHandRotation = GetHandRotation(stowedGunRotation);
		Vector3 stowHandPosition = GetHandPosition(stowedGunPosition, stowHandRotation);
		Vector3 gripHandPosition;
		Quaternion gripHandRotation;
		GetPathHandPose(gunPathRate, holdHandPosition, holdHandRotation, stowHandPosition, stowHandRotation, rightShoulderPosition, characterRotation, out gripHandPosition, out gripHandRotation);

		//銃の位置・向き（右手でグリップを握っている時の位置・向き）から、銃に付けた目印の位置・向きを求める
		Vector3 gunPosition = gripHandPosition + gripHandRotation * handToGunPosition;
		Quaternion gunRotation = gripHandRotation * handToGunRotation;
		Quaternion inverseGunModelRotation = Quaternion.Inverse(gunModel.rotation);
		Transform leftHandGripPos = gunAnimation.LeftHandGripPos;
		leftHandGripPosition = gunPosition + gunRotation * (inverseGunModelRotation * (leftHandGripPos.position - gunModel.position));
		leftHandGripRotation = gunRotation * inverseGunModelRotation * leftHandGripPos.rotation;
		Transform carryGripPos = gunAnimation.WeaponSwitchLeftHandCarryGripPos;
		if (carryGripPos != null)
		{
			Vector3 carryPosition = gunPosition + gunRotation * (inverseGunModelRotation * (carryGripPos.position - gunModel.position));
			Quaternion carryRotation = gunRotation * inverseGunModelRotation * carryGripPos.rotation;
			leftHandGripPosition = Vector3.Lerp(leftHandGripPosition, carryPosition, leftHandCarryGripWeight);
			leftHandGripRotation = Quaternion.Slerp(leftHandGripRotation, carryRotation, leftHandCarryGripWeight);
		}
		rightHandPosition = gripHandPosition;
		rightHandRotation = gripHandRotation;
		Transform forendGripPos = gunAnimation.WeaponSwitchRightHandForendGripPos;
		if (forendGripPos != null)
		{
			//しまう場所へ近づくほど、右手を銃口の方へずらす
			Vector3 forendSlide = Vector3.forward * gunAnimation.WeaponSwitchRightHandForendSlideDistance * Mathf.SmoothStep(NoWeight, FullWeight, gunPathRate);
			Vector3 forendPosition = gunPosition + gunRotation * (inverseGunModelRotation * (forendGripPos.position - gunModel.position) + forendSlide);
			Quaternion forendRotation = gunRotation * inverseGunModelRotation * forendGripPos.rotation;
			rightHandPosition = Vector3.Lerp(gripHandPosition, forendPosition, rightHandForendWeight);
			rightHandRotation = Quaternion.Slerp(gripHandRotation, forendRotation, rightHandForendWeight);
		}
		//右手をグリップから離した分だけ、いつもどおりに銃を持つ右手の位置から体の横へ下ろした所へ戻す（銃は左手で持っていく）
		rightHandPosition = Vector3.Lerp(rightHandPosition, holdHandPosition + characterRotation * ReleasedRightHandOffset, rightHandReleaseWeight);
		rightHandRotation = Quaternion.Slerp(rightHandRotation, holdHandRotation, rightHandReleaseWeight);
		//左手でしまう場所の銃をつかむ時は、伸ばす途中で体の外側へ膨らませる
		if (gunAnimation.WeaponSwitchRightHandReleaseRate < StowPathRate && phase == EnumManager.WeaponSwitchPhaseTYPE.TakeOut)
		{
			leftHandGripPosition = leftHandGripPosition + characterRotation * LeftHandCatchBulge * (LeftHandCatchBulgeScale * leftHandFreeWeight * (FullWeight - leftHandFreeWeight));
		}
		//しまう場所に近いほど、右肘を決めた向きへ張り出す（取り出す時は、右手を取り出す銃へ伸ばすのに合わせて効かせる）
		rightElbowHintWeight = gunAnimation.WeaponSwitchRightElbowHintWeight * Mathf.SmoothStep(NoWeight, FullWeight, gunPathRate) * handTransferRate;

		//しまう間は、右手のいる所を覚えておく（取り出す時は、ここから取り出す銃へ右手を伸ばす）
		if (phase == EnumManager.WeaponSwitchPhaseTYPE.PutAway)
		{
			putAwayHandLocalPosition = inverseCharacterRotation * (rightHandPosition - characterPosition);
			putAwayHandLocalRotation = inverseCharacterRotation * rightHandRotation;
			return;
		}

		if (handTransferRate < FullWeight)
		{
			Vector3 putAwayHandPosition = characterPosition + characterRotation * putAwayHandLocalPosition;
			Vector3 transferWaypoint = Vector3.Lerp(putAwayHandPosition, rightHandPosition, HandTransferBulgeRate) + characterRotation * HandTransferBulge;
			rightHandPosition = GetPathPosition(putAwayHandPosition, transferWaypoint, rightHandPosition, handTransferRate);
			rightHandRotation = Quaternion.Slerp(characterRotation * putAwayHandLocalRotation, rightHandRotation, handTransferRate);
		}
	}

	/// <summary>
	/// 銃を、曲線の上の位置・向きへ動かす（背骨を構えに合わせて曲げた後に呼ぶ）
	/// </summary>
	/// <param name="spineBendRotation">今のフレームで、構えに合わせて脊椎ボーンを曲げた回転（ワールドの向き）</param>
	/// <param name="spineBendPosition">構えに合わせて曲げた脊椎ボーンの位置</param>
	public void UpdateGunModel(Quaternion spineBendRotation, Vector3 spineBendPosition)
	{
		if (gunModel == null)
		{
			return;
		}

		//いつもどおりに銃を持つ右手・中継点・途中の姿勢は、背骨と一緒に曲げる（曲げた分も含めたキャラクターの位置・向きから求める）。しまう場所は、曲げた後の位置・向きをそのまま使う
		Quaternion characterRotation = character.rotation;
		Vector3 characterPosition = character.position;
		Vector3 bodyPosition = spineBendPosition + spineBendRotation * (characterPosition - spineBendPosition);
		Quaternion bodyRotation = spineBendRotation * characterRotation;
		Vector3 holdHandPosition = bodyPosition + bodyRotation * holdHandLocalPosition;
		Quaternion holdHandRotation = bodyRotation * holdHandLocalRotation;
		Quaternion stowHandRotation = GetHandRotation(stowedGunModel.rotation);
		Vector3 stowHandPosition = GetHandPosition(stowedGunModel.position, stowHandRotation);
		Vector3 handPosition;
		Quaternion handRotation;
		GetPathHandPose(gunPathRate, holdHandPosition, holdHandRotation, stowHandPosition, stowHandRotation, rightUpperArm.position, bodyRotation, out handPosition, out handRotation);
		gunModel.SetPositionAndRotation(handPosition + handRotation * handToGunPosition, handRotation * handToGunRotation);

		//背中にしまう銃は、曲げる分を戻した位置・向きを覚えておく（次のフレームのIKで、右手の目標に使う）
		if (stowedGunModel.IsChildOf(spine) == true)
		{
			Quaternion inverseCharacterRotation = Quaternion.Inverse(characterRotation);
			Quaternion inverseSpineBendRotation = Quaternion.Inverse(spineBendRotation);
			bentStowedGunModel = stowedGunModel;
			bentStowedGunLocalPosition = inverseCharacterRotation * (spineBendPosition + inverseSpineBendRotation * (stowedGunModel.position - spineBendPosition) - characterPosition);
			bentStowedGunLocalRotation = inverseCharacterRotation * inverseSpineBendRotation * stowedGunModel.rotation;
		}
	}

	/// <summary>
	/// 曲線の上の、右手（銃を握る位置）の位置・向きを求める
	/// （持っている位置から、中継点の方へ膨らんだ曲線で途中の姿勢まで動かし、途中の姿勢からしまう場所までは銃の中心をまっすぐ動かす。
	///   途中の姿勢を通らない銃は、しまう場所まで曲線で動かす）
	/// </summary>
	/// <param name="rate">曲線の上の位置（持っている位置が0、しまう場所が1）</param>
	/// <param name="holdHandPosition">いつもどおりに銃を持つ右手の位置</param>
	/// <param name="holdHandRotation">いつもどおりに銃を持つ右手の向き</param>
	/// <param name="stowHandPosition">しまってある銃を持つ時の右手の位置</param>
	/// <param name="stowHandRotation">しまってある銃を持つ時の右手の向き</param>
	/// <param name="rightShoulderPosition">右肩の位置（中継点・途中の姿勢は、右肩からのずれで決める）</param>
	/// <param name="bodyRotation">中継点・途中の姿勢を、キャラクターから見た向きから直すための、キャラクターの向き（背骨を曲げた後は、曲げた分も含める）</param>
	/// <param name="handPosition">求めた右手の位置</param>
	/// <param name="handRotation">求めた右手の向き</param>
	void GetPathHandPose(float rate, Vector3 holdHandPosition, Quaternion holdHandRotation, Vector3 stowHandPosition, Quaternion stowHandRotation, Vector3 rightShoulderPosition, Quaternion bodyRotation, out Vector3 handPosition, out Quaternion handRotation)
	{
		Vector3 waypoint = rightShoulderPosition + bodyRotation * gunAnimation.WeaponSwitchWaypoint;
		Quaternion waypointHandRotation = GetHandRotation(bodyRotation * GetGunLocalRotation(gunAnimation.WeaponSwitchWaypointGunAngle));
		Vector3 viaHandPosition = stowHandPosition;
		Quaternion viaHandRotation = stowHandRotation;
		float viaPoseRate = gunAnimation.WeaponSwitchViaPoseRate;
		if (viaPoseRate < StowPathRate)
		{
			viaHandPosition = rightShoulderPosition + bodyRotation * gunAnimation.WeaponSwitchViaHandPosition;
			viaHandRotation = GetHandRotation(bodyRotation * GetGunLocalRotation(gunAnimation.WeaponSwitchViaGunAngle));
		}

		if (rate <= viaPoseRate)
		{
			float curveRate = Mathf.InverseLerp(HoldPathRate, viaPoseRate, rate);
			handPosition = GetPathPosition(holdHandPosition, waypoint, viaHandPosition, curveRate);
			handRotation = GetPathRotation(holdHandRotation, waypointHandRotation, viaHandRotation, curveRate);
			return;
		}

		//途中の姿勢からしまう場所までは、銃の中心をまっすぐ動かす（右手の位置をまっすぐ動かすと、銃の中心が弧を描くため）
		float straightRate = Mathf.InverseLerp(viaPoseRate, StowPathRate, rate);
		handRotation = Quaternion.Slerp(viaHandRotation, stowHandRotation, straightRate);
		Vector3 viaGunPosition = viaHandPosition + viaHandRotation * handToGunPosition;
		Vector3 stowGunPosition = stowHandPosition + stowHandRotation * handToGunPosition;
		handPosition = GetHandPosition(Vector3.Lerp(viaGunPosition, stowGunPosition, straightRate), handRotation);
	}

	/// <summary>
	/// 銃の向きを求める（キャラクターから見た向き）
	/// </summary>
	/// <param name="angle">銃の向き（度。銃口を正面に向けて水平にした向きから、xが銃口を下へ向ける角度、yが銃口を右へ向ける角度、zが銃身を軸に銃の上側を右へ倒す角度）</param>
	/// <returns>銃の向き</returns>
	static Quaternion GetGunLocalRotation(Vector3 angle)
	{
		return Quaternion.Euler(angle.x, angle.y, -angle.z);
	}

	/// <summary>
	/// 始まりの向きから、曲線の真ん中で中継点での向きを通って、終わりの向きへ回した向きを求める
	/// （中継点での向きを使う割合が0の時は、真ん中で始まりと終わりのちょうど間の向きを通るので、始まりの向きから終わりの向きへまっすぐ回すのと同じになる）
	/// </summary>
	/// <param name="start">始まりの向き</param>
	/// <param name="waypointRotation">中継点での向き</param>
	/// <param name="end">終わりの向き</param>
	/// <param name="rate">曲線の上の位置（0で始まり、1で終わり）</param>
	/// <returns>曲線の上の向き</returns>
	Quaternion GetPathRotation(Quaternion start, Quaternion waypointRotation, Quaternion end, float rate)
	{
		Quaternion middle = Quaternion.Slerp(Quaternion.Slerp(start, end, PathMiddleRate), waypointRotation, gunAnimation.WeaponSwitchWaypointGunAngleWeight);
		if (rate <= PathMiddleRate)
		{
			return Quaternion.Slerp(start, middle, Mathf.InverseLerp(HoldPathRate, PathMiddleRate, rate));
		}

		return Quaternion.Slerp(middle, end, Mathf.InverseLerp(PathMiddleRate, StowPathRate, rate));
	}

	/// <summary>
	/// 銃を決めた向きにする時の、右手の向きを求める
	/// </summary>
	/// <param name="gunRotation">銃の向き</param>
	/// <returns>右手の向き（IK用の手の向き）</returns>
	Quaternion GetHandRotation(Quaternion gunRotation)
	{
		return gunRotation * Quaternion.Inverse(handToGunRotation);
	}

	/// <summary>
	/// 銃を決めた位置にする時の、右手の位置を求める
	/// </summary>
	/// <param name="gunPosition">銃の位置</param>
	/// <param name="handRotation">右手の向き</param>
	/// <returns>右手の位置</returns>
	Vector3 GetHandPosition(Vector3 gunPosition, Quaternion handRotation)
	{
		return gunPosition - handRotation * handToGunPosition;
	}

	/// <summary>
	/// 始まりと終わりを結び、中継点の方へ膨らんだ曲線（2次ベジェ曲線）の上の位置を求める
	/// </summary>
	/// <param name="start">始まりの位置</param>
	/// <param name="waypoint">中継点</param>
	/// <param name="end">終わりの位置</param>
	/// <param name="rate">曲線の上の位置（0で始まり、1で終わり）</param>
	/// <returns>曲線の上の位置</returns>
	static Vector3 GetPathPosition(Vector3 start, Vector3 waypoint, Vector3 end, float rate)
	{
		return Vector3.Lerp(Vector3.Lerp(start, waypoint, rate), Vector3.Lerp(waypoint, end, rate), rate);
	}
}
