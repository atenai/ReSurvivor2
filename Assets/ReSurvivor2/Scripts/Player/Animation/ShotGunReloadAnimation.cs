using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ショットガンのリロードのアニメーション（バイオハザード レクイエムのような、弾を1発ずつ込める動き）
/// （左手で銃を握ったまま、銃を体の前で斜め（ストックが右肩の上、銃口が左下）に持ち上げ、
///   右手で右腰のポーチから弾を取って、装填口に込める。込め終わったら、右手でグリップを握り直して、元の持ち方に戻す）
/// ※リロード中は、銃を左手の骨の子にして、左手と一緒に動かす
/// ※両手と銃の位置・向きは、リロードアニメーションの再生位置から求める（リロードのステートのアニメーションは、上半身の立ち姿だけにしてある）
///   2発目以降は、リロードアニメーションの再生位置が「装填口 → ポーチ」と巻き戻るので、右手も装填口からポーチへ戻って次の弾を取る
/// ※PlayerCharacterViewが持ち、HandIKはここで求めた手の位置・向きを使う（銃のクラスからは使わない）
/// </summary>
[System.Serializable]
public class ShotGunReloadAnimation
{
	[Header("弾を込める時の銃の持ち方")]
	[Tooltip("弾を込める間、左手で銃を握る位置・向き（銃の子に置いた目印。向きはIK用の手の向き）（右手で弾を込める装填口を空けるため、普段より銃口側を握る）")]
	[SerializeField] Transform leftHandHoldGripPos;
	[Tooltip("弾を込める時の左手の位置（m。胸の骨からのずれで、キャラクターから見た向き。xが右、yが上、zが前）")]
	[SerializeField] Vector3 holdLeftHandPosition;
	[Tooltip("弾を込める時の銃の向き（度。キャラクターから見た向きで、銃口を正面に向けて水平にした向きから、zが銃身を軸に銃の上側を右へ倒す角度、xが銃口を下へ向ける角度、yが銃口を右へ向ける角度。z → x → y の順に回す）")]
	[SerializeField] Vector3 holdGunAngle;
	[Tooltip("弾を込めている間に、左肘を向ける位置（m。左肩からのずれで、キャラクターから見た向き。xが右、yが上、zが前）（左手を体の前に持ってくると、肘が体の内側に入って前腕が体にめり込むため、肘を体の外側へ張り出す）")]
	[SerializeField] Vector3 leftElbowHintOffset;
	public Vector3 LeftElbowHintOffset => leftElbowHintOffset;

	[Header("右手で弾を込める動き")]
	[Tooltip("右手で弾を取る位置・向き（右腰のポーチ。腰の骨の子に置いた目印。向きはIK用の手の向き）")]
	[SerializeField] Transform rightHandShellPouchPos;
	//※弾は、指先でつまんで運び（以前のリロードのアニメーションの左手と同じ持ち方）、弾の先を装填口に当ててから、親指で銃身に沿って押し込む
	[Tooltip("右手で弾の先を装填口に当てる位置・向き（銃の子に置いた目印。向きはIK用の手の向き。右手に持った弾の先が、装填口の入り口に来る位置）")]
	[SerializeField] Transform rightHandShellInsertPos;
	[Tooltip("右手で弾を装填口に押し込み終わる位置・向き（銃の子に置いた目印。向きはIK用の手の向き。弾を装填口の入り口から、銃身に沿って銃口の方へ押し込んだ位置）")]
	[SerializeField] Transform rightHandShellPushPos;
	[Tooltip("右手をグリップ・ポーチ・装填口の間で動かす時に、体や銃に当たらないように、前へふくらませる量（m）")]
	[SerializeField] float rightHandPathBulge;
	[Tooltip("弾を込めている間に、右肘を向ける位置（m。右肩からのずれで、キャラクターから見た向き。xが右、yが上、zが前）（右手を胸の前の装填口へ回すと、肘が下がって二の腕が銃に当たるため、肘を体の外側へ張り出す）")]
	[SerializeField] Vector3 rightElbowHintOffset;
	public Vector3 RightElbowHintOffset => rightElbowHintOffset;

	[Header("タイミング（リロードアニメーションの再生位置。0がアニメーションの始まり、1が終わり）")]
	//※右手がポーチに着いて弾を取るタイミングと、装填口に弾を込めるタイミングは、
	//  ショットガンのアニメーションの設定（GunAnimation）の、マガジンを抜き取るタイミング・差し込むタイミングを使う
	[Tooltip("銃を弾を込める持ち方にし終わるタイミング（アニメーションの始まりからここまでで、左手で銃を持ち上げる）")]
	[SerializeField] float gunHoldTime;
	[Tooltip("右手でグリップを離すタイミング（ここから弾を取るタイミングまでで、右手をポーチへ動かす）")]
	[SerializeField] float rightHandReleaseTime;
	[Tooltip("右手で弾を装填口に押し込み始めるタイミング（弾を取るタイミングからここまでで、弾の先を装填口に当て、ここから弾を込めるタイミングまでで押し込む）")]
	[SerializeField] float rightHandPushTime;
	[Tooltip("右手でグリップを握り直すタイミング（弾を込めるタイミングからここまでで、右手を装填口からグリップへ戻す）")]
	[SerializeField] float rightHandRegripTime;
	[Tooltip("銃を元の持ち方へ戻し始めるタイミング（ここからアニメーションの終わりまでで戻す）")]
	[SerializeField] float gunReturnTime;
	[Tooltip("リロードアニメーションの終わりの再生位置")]
	const float ReloadAnimationEndTime = 1.0f;

	[Tooltip("動かさない時の割合")]
	const float NoWeight = 0.0f;
	[Tooltip("目標の位置・向きにぴったり合わせる時の割合")]
	const float FullWeight = 1.0f;
	[Tooltip("弾を込めている間に、頭を装填口へ向ける効き具合（0で向けない、1でまっすぐ向ける）")]
	const float HeadLookAtWeight = 0.5f;
	[Tooltip("頭を装填口へ向ける時に、首を回せる大きさ（Unityのヒューマノイドの視線のIKの設定。0で好きなだけ回せる、1で全く回せない）")]
	const float HeadLookAtClampWeight = 0.5f;
	[Tooltip("リロードが終わった後に、右手に戻した銃を元の位置・向きへ動かすのにかける時間（秒）")]
	const float GunReturnBlendTime = 0.1f;

	[Tooltip("ショットガンのモデル")]
	Transform gunModel;
	[Tooltip("ショットガンのアニメーションの設定（弾を取るタイミング・込めるタイミングを使う）")]
	GunAnimation shotGunAnimation;
	[Tooltip("アニメーションどおりの左手の位置・向きをIK用の向きで取り出すための目印（左手の骨の子）")]
	Transform leftHandAnimationPos;
	[Tooltip("右手でグリップを握っている時の、銃から見たIK用の右手の位置")]
	Vector3 rightHandGripPositionInGun;
	[Tooltip("右手でグリップを握っている時の、銃から見たIK用の右手の向き")]
	Quaternion rightHandGripRotationInGun;
	[Tooltip("弾を込める間、左手で握る位置で銃を持った時の、左手の骨から見た銃の位置")]
	Vector3 gunHoldLocalPosition;
	[Tooltip("弾を込める間、左手で握る位置で銃を持った時の、左手の骨から見た銃の向き")]
	Quaternion gunHoldLocalRotation;
	[Tooltip("左手で銃の前側を握る目印で銃を持った時の、左手の骨から見た銃の位置（元の持ち方に戻す時は、ここへ持ち直す）")]
	Vector3 gunForeEndLocalPosition;
	[Tooltip("左手で銃の前側を握る目印で銃を持った時の、左手の骨から見た銃の向き")]
	Quaternion gunForeEndLocalRotation;

	//↓今のフレームの動き↓//
	[Tooltip("今のフレームで、両手と銃をリロードの動きで動かすか？")]
	bool isActive = false;
	public bool IsActive => isActive;
	[Tooltip("今のフレームの、リロードアニメーションの再生位置")]
	float animationTime = ReloadAnimationEndTime;
	[Tooltip("今のフレームで、銃を弾を込める持ち方にしている割合（0で普通の持ち方、1で弾を込める持ち方）。左手で握る位置も、この割合で持ち直す")]
	float holdWeight = NoWeight;
	[Tooltip("背骨を構えに合わせて曲げる分を戻す回転（IKの後のLateUpdateで曲げる分の逆）")]
	Quaternion inverseSpineAimRotation = Quaternion.identity;
	[Tooltip("構えに合わせて曲げる背骨の骨の位置")]
	Vector3 spinePosition = Vector3.zero;
	[Tooltip("左手の目標の位置")]
	Vector3 leftHandPosition;
	public Vector3 LeftHandPosition => leftHandPosition;
	[Tooltip("左手の目標の向き（IK用の手の向き）")]
	Quaternion leftHandRotation;
	public Quaternion LeftHandRotation => leftHandRotation;
	[Tooltip("右手の目標の位置")]
	Vector3 rightHandPosition;
	public Vector3 RightHandPosition => rightHandPosition;
	[Tooltip("右手の目標の向き（IK用の手の向き）")]
	Quaternion rightHandRotation;
	public Quaternion RightHandRotation => rightHandRotation;
	[Tooltip("頭を向ける位置（装填口）")]
	Vector3 headLookAtPosition;
	public Vector3 HeadLookAtPosition => headLookAtPosition;
	public float HeadLookAtIKWeight => holdWeight * HeadLookAtWeight;
	public float ElbowHintWeight => holdWeight;
	public float RightFingersShellGripWeight => GetShellGripWeight(animationTime);
	public float HeadLookAtIKClampWeight => HeadLookAtClampWeight;

	//↓銃のモデルの持ち替えに使う↓//
	[Tooltip("銃を左手に持っているか？")]
	bool isGunInLeftHand = false;
	[Tooltip("銃を右手に戻して、元の位置・向きへ動かしている間か？")]
	bool isGunReturning = false;
	[Tooltip("銃を右手に戻して、元の位置・向きへ動かした割合")]
	float gunReturnRate = NoWeight;
	[Tooltip("銃を右手で持っている時の親（右手の骨）")]
	Transform gunRightHandParent;
	[Tooltip("銃を右手で持っている時の位置（親から見た位置）")]
	Vector3 gunRightHandLocalPosition;
	[Tooltip("銃を右手で持っている時の向き（親から見た向き）")]
	Quaternion gunRightHandLocalRotation;
	[Tooltip("両手で持っている銃の、左手の骨から見た位置（リロードしていない間、毎フレーム覚えておく。リロードを始めた時は、ここから左手に持たせる）")]
	Vector3 gunGripLocalPosition = Vector3.zero;
	[Tooltip("両手で持っている銃の、左手の骨から見た向き（リロードしていない間、毎フレーム覚えておく）")]
	Quaternion gunGripLocalRotation = Quaternion.identity;
	[Tooltip("銃を右手に戻し始めた時の位置（右手の骨から見た位置）")]
	Vector3 gunReturnStartLocalPosition;
	[Tooltip("銃を右手に戻し始めた時の向き（右手の骨から見た向き）")]
	Quaternion gunReturnStartLocalRotation;

	/// <summary>
	/// 初期化（右手で持っている銃と、両手の目印の位置関係を覚えておく）
	/// </summary>
	/// <param name="gunModel">ショットガンのモデル（右手の骨の子）</param>
	/// <param name="shotGunAnimation">ショットガンのアニメーションの設定</param>
	/// <param name="rightHandAnimationPos">アニメーションどおりの右手の位置・向きをIK用の向きで取り出すための目印（右手の骨の子）</param>
	/// <param name="leftHandAnimationPos">アニメーションどおりの左手の位置・向きをIK用の向きで取り出すための目印（左手の骨の子）</param>
	public void Initialize(Transform gunModel, GunAnimation shotGunAnimation, Transform rightHandAnimationPos, Transform leftHandAnimationPos)
	{
		this.gunModel = gunModel;
		this.shotGunAnimation = shotGunAnimation;
		this.leftHandAnimationPos = leftHandAnimationPos;
		rightHandGripRotationInGun = Quaternion.Inverse(gunModel.rotation) * rightHandAnimationPos.rotation;
		rightHandGripPositionInGun = Quaternion.Inverse(gunModel.rotation) * (rightHandAnimationPos.position - gunModel.position);

		//左手のIK用の向きの目印が、弾を込める間に左手で握る目印・銃の前側を握る目印に重なる時の、銃の位置・向き（左手の骨から見た位置・向き）
		GetGunLocalPose(leftHandHoldGripPos, out gunHoldLocalPosition, out gunHoldLocalRotation);
		GetGunLocalPose(shotGunAnimation.LeftHandGripPos, out gunForeEndLocalPosition, out gunForeEndLocalRotation);
	}

	/// <summary>
	/// 左手のIK用の向きの目印が、銃の子にある目印に重なる時の、左手の骨から見た銃の位置・向きを求める
	/// </summary>
	/// <param name="gripPos">左手で銃を握る目印（銃の子。向きはIK用の手の向き）</param>
	/// <param name="position">左手の骨から見た銃の位置</param>
	/// <param name="rotation">左手の骨から見た銃の向き</param>
	void GetGunLocalPose(Transform gripPos, out Vector3 position, out Quaternion rotation)
	{
		rotation = leftHandAnimationPos.localRotation * Quaternion.Inverse(gripPos.localRotation);
		position = leftHandAnimationPos.localPosition - rotation * gripPos.localPosition;
	}

	/// <summary>
	/// リロードの進み具合に合わせて、銃を弾を込める持ち方にする割合を求める（手のIKの前に呼ぶ）
	/// </summary>
	/// <param name="isReloading">ショットガンを持っていて、リロード中か？</param>
	/// <param name="reloadAnimationTime">リロードアニメーションの再生位置</param>
	/// <param name="spineAimRotation">背骨を構えに合わせて曲げる回転（IKの後のLateUpdateで曲げる分。ワールドの向き）</param>
	/// <param name="spinePosition">構えに合わせて曲げる背骨の骨の位置</param>
	public void UpdateWeights(bool isReloading, float reloadAnimationTime, Quaternion spineAimRotation, Vector3 spinePosition)
	{
		//リロードが終わったフレームは、まだ銃を左手に持っているので、アニメーションの終わり（普通の持ち方）の位置・向きにする（LateUpdateで右手に戻す）
		isActive = isReloading == true || isGunInLeftHand == true;
		animationTime = isReloading == true ? reloadAnimationTime : ReloadAnimationEndTime;
		holdWeight = GetHoldWeight(animationTime);
		inverseSpineAimRotation = Quaternion.Inverse(spineAimRotation);
		this.spinePosition = spinePosition;
	}

	/// <summary>
	/// 銃を弾を込める持ち方にしている割合を求める（始まりに持ち上げ、終わりに元の持ち方へ戻す）
	/// </summary>
	/// <param name="time">リロードアニメーションの再生位置</param>
	/// <returns>銃を弾を込める持ち方にしている割合</returns>
	float GetHoldWeight(float time)
	{
		if (time <= gunHoldTime)
		{
			return Mathf.SmoothStep(NoWeight, FullWeight, time / gunHoldTime);
		}

		if (gunReturnTime <= time)
		{
			return Mathf.SmoothStep(FullWeight, NoWeight, (time - gunReturnTime) / (ReloadAnimationEndTime - gunReturnTime));
		}

		return FullWeight;
	}

	/// <summary>
	/// 両手の目標の位置・向きと、頭を向ける位置を求める（HandIKが、手のIKの中で呼ぶ）
	/// ・銃：普通の持ち方（右手で持っている時の位置・向き）から、左手を中心に回しながら、弾を込める持ち方へ動かす
	/// ・左手：銃を握ったまま（握る位置は、銃の前側から、弾を込める間に握る位置へ持ち直す）
	/// ・右手：グリップ → ポーチ（弾を取る）→ 装填口（弾を込める）→ グリップ。2発目以降は、装填口からポーチへ戻って次の弾を取る
	/// </summary>
	/// <param name="animator">プレイヤーのアニメーター</param>
	/// <param name="normalRightHandPosition">普通の持ち方の右手の位置（構えていない時は、アニメーションどおりの右手を体から離した位置）</param>
	/// <param name="normalRightHandRotation">普通の持ち方の右手の向き（IK用の手の向き）</param>
	public void UpdateTargets(Animator animator, Vector3 normalRightHandPosition, Quaternion normalRightHandRotation)
	{
		Quaternion characterRotation = animator.transform.rotation;
		//普通の持ち方の銃の位置・向き（右手でグリップを握っている銃）
		Quaternion normalGunRotation = normalRightHandRotation * Quaternion.Inverse(rightHandGripRotationInGun);
		Vector3 normalGunPosition = normalRightHandPosition - normalGunRotation * rightHandGripPositionInGun;

		//銃から見たIK用の左手の位置・向き（左手の骨から見た銃の位置・向きの逆）
		Vector3 gunLocalPosition;
		Quaternion gunLocalRotation;
		GetGunLocalPoseInLeftHand(out gunLocalPosition, out gunLocalRotation);
		Quaternion inverseGunLocalRotation = Quaternion.Inverse(gunLocalRotation);
		Quaternion leftHandRotationInGun = inverseGunLocalRotation * leftHandAnimationPos.localRotation;
		Vector3 leftHandPositionInGun = inverseGunLocalRotation * (leftHandAnimationPos.localPosition - gunLocalPosition);

		//銃は、左手を中心に回しながら、普通の持ち方から弾を込める持ち方へ動かす（ストックが頭の右側を通って、右肩の上へ上がる）
		Transform chest = animator.GetBoneTransform(HumanBodyBones.UpperChest);
		Quaternion holdGunRotation = characterRotation * Quaternion.Euler(holdGunAngle.x, holdGunAngle.y, -holdGunAngle.z);
		Vector3 holdLeftHandWorldPosition = chest.position + characterRotation * holdLeftHandPosition;
		Quaternion gunRotation = Quaternion.Slerp(normalGunRotation, holdGunRotation, holdWeight);
		leftHandPosition = Vector3.Lerp(normalGunPosition + normalGunRotation * leftHandPositionInGun, holdLeftHandWorldPosition, holdWeight);
		leftHandRotation = gunRotation * leftHandRotationInGun;
		Vector3 gunPosition = leftHandPosition - gunRotation * leftHandPositionInGun;

		//右手の行き先（グリップ・ポーチ・装填口）
		//※ポーチは腰に付いているので、IKの後に背骨を曲げても動かない。そのため、曲げる分を戻した位置・向きを目標にする（曲げた後に、ちょうどポーチに来る）
		Vector3 gripPosition = gunPosition + gunRotation * rightHandGripPositionInGun;
		Quaternion gripRotation = gunRotation * rightHandGripRotationInGun;
		Vector3 pouchPosition = spinePosition + inverseSpineAimRotation * (rightHandShellPouchPos.position - spinePosition);
		Quaternion pouchRotation = inverseSpineAimRotation * rightHandShellPouchPos.rotation;
		Vector3 insertPosition = gunPosition + gunRotation * rightHandShellInsertPos.localPosition;
		Quaternion insertRotation = gunRotation * rightHandShellInsertPos.localRotation;
		Vector3 pushPosition = gunPosition + gunRotation * rightHandShellPushPos.localPosition;
		Quaternion pushRotation = gunRotation * rightHandShellPushPos.localRotation;
		Vector3 bulge = characterRotation * Vector3.forward * rightHandPathBulge;

		//※2発目以降に装填口からポーチへ戻る時は、リロードアニメーションの再生位置が巻き戻るので、「押し込む → 装填口に当てる → ポーチ」と逆にたどる
		float pouchTime = shotGunAnimation.MagazineOutTime;
		float insertTime = shotGunAnimation.MagazineInTime;
		if (animationTime <= rightHandReleaseTime)
		{
			SetRightHand(gripPosition, gripRotation);
		}
		else if (animationTime <= pouchTime)
		{
			//グリップを離して、ポーチへ弾を取りに行く
			BlendRightHand(gripPosition, gripRotation, pouchPosition, pouchRotation, bulge, Mathf.SmoothStep(NoWeight, FullWeight, (animationTime - rightHandReleaseTime) / (pouchTime - rightHandReleaseTime)));
		}
		else if (animationTime <= rightHandPushTime)
		{
			//ポーチで取った弾を持っていき、弾の先を装填口に当てる
			BlendRightHand(pouchPosition, pouchRotation, insertPosition, insertRotation, bulge, Mathf.SmoothStep(NoWeight, FullWeight, (animationTime - pouchTime) / (rightHandPushTime - pouchTime)));
		}
		else if (animationTime <= insertTime)
		{
			//拳ごと銃身に沿って、弾を装填口に押し込む（押し込み終わったら、弾は手から離れて銃の中に入る）
			BlendRightHand(insertPosition, insertRotation, pushPosition, pushRotation, Vector3.zero, (animationTime - rightHandPushTime) / (insertTime - rightHandPushTime));
		}
		else if (animationTime <= rightHandRegripTime)
		{
			//最後の弾を込めたら、グリップを握り直す
			BlendRightHand(pushPosition, pushRotation, gripPosition, gripRotation, bulge, Mathf.SmoothStep(NoWeight, FullWeight, (animationTime - insertTime) / (rightHandRegripTime - insertTime)));
		}
		else
		{
			SetRightHand(gripPosition, gripRotation);
		}

		//頭は、装填口を見る
		headLookAtPosition = insertPosition;
	}

	/// <summary>
	/// 右手の目標の位置・向きを決める
	/// </summary>
	/// <param name="position">右手の位置</param>
	/// <param name="rotation">右手の向き（IK用の手の向き）</param>
	void SetRightHand(Vector3 position, Quaternion rotation)
	{
		rightHandPosition = position;
		rightHandRotation = rotation;
	}

	/// <summary>
	/// 右手の目標の位置・向きを、2つの位置・向きの間で混ぜる（途中は前へふくらませる）
	/// </summary>
	/// <param name="fromPosition">動かし始める位置</param>
	/// <param name="fromRotation">動かし始める向き</param>
	/// <param name="toPosition">動かし終わる位置</param>
	/// <param name="toRotation">動かし終わる向き</param>
	/// <param name="bulge">真ん中で前へふくらませる量（ワールドの向き）</param>
	/// <param name="rate">動かした割合（0で動かし始め、1で動かし終わり）</param>
	void BlendRightHand(Vector3 fromPosition, Quaternion fromRotation, Vector3 toPosition, Quaternion toRotation, Vector3 bulge, float rate)
	{
		rightHandPosition = Vector3.Lerp(fromPosition, toPosition, rate) + bulge * Mathf.Sin(rate * Mathf.PI);
		rightHandRotation = Quaternion.Slerp(fromRotation, toRotation, rate);
	}

	/// <summary>
	/// 右手の指を、弾をつまむ形にする割合を求める
	/// （グリップを離してからポーチに着くまでで弾をつまむ形にし、最後の弾を込めてからグリップを握り直すまでで、元の握り方に戻す）
	/// </summary>
	/// <param name="time">リロードアニメーションの再生位置</param>
	/// <returns>弾をつまむ形にする割合（0でリロードのステートどおりの握り方、1で弾をつまむ形）</returns>
	float GetShellGripWeight(float time)
	{
		float pouchTime = shotGunAnimation.MagazineOutTime;
		float insertTime = shotGunAnimation.MagazineInTime;
		if (time <= rightHandReleaseTime || rightHandRegripTime <= time)
		{
			return NoWeight;
		}

		if (time <= pouchTime)
		{
			return Mathf.SmoothStep(NoWeight, FullWeight, (time - rightHandReleaseTime) / (pouchTime - rightHandReleaseTime));
		}

		if (insertTime <= time)
		{
			return Mathf.SmoothStep(FullWeight, NoWeight, (time - insertTime) / (rightHandRegripTime - insertTime));
		}

		return FullWeight;
	}

	/// <summary>
	/// 左手の骨から見た銃の位置・向きを求める（銃を持ち上げる割合で、弾を込める間に握る位置・向きへ持ち直す）
	/// ・持ち上げる時：リロードを始めた時の握り方から持ち直す
	/// ・元の持ち方に戻す時：銃の前側を握る目印の握り方へ持ち直す
	///   （ハンドガンをしまった直後などで、リロードを始めた時の左手が目印からずれていても、リロードの後に左手がワープしないようにする）
	/// </summary>
	/// <param name="position">左手の骨から見た銃の位置</param>
	/// <param name="rotation">左手の骨から見た銃の向き</param>
	void GetGunLocalPoseInLeftHand(out Vector3 position, out Quaternion rotation)
	{
		Vector3 gripPosition = gunGripLocalPosition;
		Quaternion gripRotation = gunGripLocalRotation;
		if (gunReturnTime <= animationTime)
		{
			gripPosition = gunForeEndLocalPosition;
			gripRotation = gunForeEndLocalRotation;
		}

		position = Vector3.Lerp(gripPosition, gunHoldLocalPosition, holdWeight);
		rotation = Quaternion.Slerp(gripRotation, gunHoldLocalRotation, holdWeight);
	}

	/// <summary>
	/// リロードに合わせて、銃のモデルを持ち替える（アニメーションで手が動いた後に呼ぶ）
	/// ・リロードを始めたら、銃を左手の骨の子にする（1つ前のフレームで両手で持っていた時の、左手から見た位置・向きのまま）
	/// ・リロード中は、左手で握る位置を持ち直す
	/// ・リロードが終わったら、銃を右手の骨の子に戻して、元の位置・向きへ動かす
	/// </summary>
	/// <param name="isShotGunHeld">ショットガンを持っているか？</param>
	/// <param name="isReloading">ショットガンを持っていて、リロード中か？</param>
	/// <param name="hand_l">左手の骨</param>
	public void UpdateModel(bool isShotGunHeld, bool isReloading, Transform hand_l)
	{
		if (isReloading == true && isGunInLeftHand == false)
		{
			HoldGunInLeftHand(hand_l);
		}
		else if (isReloading == false && isGunInLeftHand == true)
		{
			HoldGunInRightHand();
		}

		if (isGunInLeftHand == true)
		{
			Vector3 gunLocalPosition;
			Quaternion gunLocalRotation;
			GetGunLocalPoseInLeftHand(out gunLocalPosition, out gunLocalRotation);
			gunModel.localPosition = gunLocalPosition;
			gunModel.localRotation = gunLocalRotation;
			return;
		}

		if (isGunReturning == true)
		{
			MoveGunToRightHand();
			return;
		}

		//リロードしていない間は、両手で持っている銃の、左手から見た位置・向きを覚えておく
		//（リロードを始めたフレームでは、アニメーションが切り替わって右手（と銃）が先に動いてしまうため、1つ前のフレームの位置・向きで左手に持たせる）
		if (isShotGunHeld == true)
		{
			gunGripLocalPosition = hand_l.InverseTransformPoint(gunModel.position);
			gunGripLocalRotation = Quaternion.Inverse(hand_l.rotation) * gunModel.rotation;
		}
	}

	/// <summary>
	/// 銃を左手に持ち替える（左手の骨の子にする。この後は左手と一緒に動く）
	/// </summary>
	/// <param name="hand_l">左手の骨</param>
	void HoldGunInLeftHand(Transform hand_l)
	{
		gunRightHandParent = gunModel.parent;
		gunRightHandLocalPosition = gunModel.localPosition;
		gunRightHandLocalRotation = gunModel.localRotation;
		gunModel.SetParent(hand_l, false);
		isGunInLeftHand = true;
		isGunReturning = false;
	}

	/// <summary>
	/// 銃を右手に戻す（今の見た目の位置・向きのまま、親だけを右手の骨に戻す。元の位置・向きへはMoveGunToRightHandで動かす）
	/// </summary>
	void HoldGunInRightHand()
	{
		gunModel.SetParent(gunRightHandParent, true);
		gunReturnStartLocalPosition = gunModel.localPosition;
		gunReturnStartLocalRotation = gunModel.localRotation;
		gunReturnRate = NoWeight;
		isGunInLeftHand = false;
		isGunReturning = true;
	}

	/// <summary>
	/// 右手に戻した銃を、元の位置・向きへ動かす（リロードの終わりで、右手はもうグリップを握っているので、ずれはわずか）
	/// </summary>
	void MoveGunToRightHand()
	{
		gunReturnRate = Mathf.MoveTowards(gunReturnRate, FullWeight, Time.deltaTime / GunReturnBlendTime);
		float rate = Mathf.SmoothStep(NoWeight, FullWeight, gunReturnRate);
		gunModel.localPosition = Vector3.Lerp(gunReturnStartLocalPosition, gunRightHandLocalPosition, rate);
		gunModel.localRotation = Quaternion.Slerp(gunReturnStartLocalRotation, gunRightHandLocalRotation, rate);
		if (FullWeight <= gunReturnRate)
		{
			isGunReturning = false;
		}
	}
}
