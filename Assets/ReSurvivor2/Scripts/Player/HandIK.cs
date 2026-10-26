using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 手のIK
/// ※アニメーターのIKパスは一番上のレイヤー（UpperBodyReload）だけにオンにしてある
///   （複数のレイヤーでオンにすると、このOnAnimatorIKが何度も呼ばれ、前のレイヤーのIKの結果が「アニメーションどおりの左手」に混ざってしまうため）
/// </summary>
public class HandIK : MonoBehaviour
{
	[Tooltip("アニメーター")]
	[SerializeField] Animator animator;
	[Tooltip("ハンドガンの左手の位置")]
	[SerializeField] Transform handGunLeftHandPos;
	[Tooltip("アサルトライフルの左手の位置")]
	[SerializeField] Transform assaultRifleLeftHandPos;
	[Tooltip("ショットガンの左手の位置")]
	[SerializeField] Transform shotGunLeftHandPos;
	//※IKで使う手の向きは手の骨の向きそのものではなく、Unityのヒューマノイド共通の手の向きなので、
	//  アニメーションどおりの手の位置・向きをIKで使うには、骨の向きをIK用の向きに直した目印を使う
	[Tooltip("アニメーションどおりの左手の位置・向きをIK用の向きで取り出すための目印（左手の骨の子に置いた目印。位置は骨と同じで、向きだけIK用の手の向きにしてある）")]
	[SerializeField] Transform leftHandAnimationPos;
	[Tooltip("アニメーションどおりの右手の位置・向きをIK用の向きで取り出すための目印（右手の骨の子に置いた目印。位置は骨と同じで、向きだけIK用の手の向きにしてある）")]
	[SerializeField] Transform rightHandAnimationPos;
	[Tooltip("IKを効かせない時の重み（アニメーションどおりの手）")]
	const float NoIKWeight = 0.0f;
	[Tooltip("IKを最大まで効かせる時の重み（目印にぴったり合わせた手）")]
	const float FullIKWeight = 1.0f;
	[Tooltip("左手を銃の前側を握る目印に合わせる、今の効き具合（0でアニメーションどおりの左手、1で目印に合わせた左手）")]
	float leftHandWeight = FullIKWeight;
	[Tooltip("左手をマガジンを持つ目印に合わせる、今の効き具合")]
	float magazineGripWeight = NoIKWeight;
	[Tooltip("リロードが途中で終わった時（ショットガンのリロード中に撃った時）に、左手を銃の前側を握る目印へ戻すのにかける時間（秒）（アニメーターのリロードステートから抜ける遷移時間と同じにする）")]
	const float ReloadCancelBlendTime = 0.25f;

	//↓構えていない時に、右手（と銃）を体から離すのに使う↓//
	[Tooltip("アニメーターの移動のレイヤーの番号")]
	const int LocomotionLayerIndex = 0;
	[Tooltip("アニメーターの、ハンドガンを片手で構えている間だけ左腕を下ろしておくレイヤーの番号")]
	const int HandGunOneHandAimLeftArmLayerIndex = 1;
	[Tooltip("アニメーターの、ダッシュしている間だけ右腕と右手の指を走りのアニメーション（銃を持つ腕）にしておくレイヤーの番号")]
	const int DashRightArmLayerIndex = 2;
	[Tooltip("アニメーターの上半身のリロードのレイヤーの番号")]
	const int UpperBodyReloadLayerIndex = 4;
	[Tooltip("構えていない時の移動のステート（アニメーターのステート名から作った番号）")]
	readonly int normalIdleAndWalkStateHash = Animator.StringToHash("NormalIdleAndWalk");
	[Tooltip("リロードしていない時のステート（アニメーターのステート名から作った番号）")]
	readonly int noReloadStateHash = Animator.StringToHash("NoReload");
	[Tooltip("リロードのアニメーションで「構えていない時の動き」と「構えている時の動き」を混ぜる割合のパラメーター名（0で構えていない時の動き、1で構えている時の動き）")]
	const string AimParameterName = "f_aim";
	[Tooltip("構えていない時の移動で、走りのアニメーションとダッシュのアニメーションを混ぜる割合のパラメーター名（0で走り、1でダッシュ）")]
	const string DashParameterName = "f_dash";
	[Tooltip("今のフレームで、右手（と銃）を右手の位置を中心に回す回転")]
	Quaternion relaxedRotation = Quaternion.identity;
	[Tooltip("今のフレームで、右手（と銃）をずらす量")]
	Vector3 relaxedOffset = Vector3.zero;
	//↓ハンドガンを構えていない時に、片手（右手）だけで持つのに使う↓//
	[Tooltip("ハンドガンを構えていない時の移動のステート（アニメーターのステート名から作った番号）")]
	readonly int handGunNormalIdleAndWalkStateHash = Animator.StringToHash("HandGunNormalIdleAndWalk");
	[Tooltip("ハンドガンのリロードのステート（アニメーターのステート名から作った番号）")]
	readonly int handGunReloadStateHash = Animator.StringToHash("HandGunReload");
	[Tooltip("今のフレームで、片手（右手）だけで銃を持つ割合（0で両手持ち、1で片手持ち。左手は銃を握らず、アニメーションどおりにする）")]
	float oneHandWeight = NoIKWeight;
	[Tooltip("片手で持って走っている時に、銃口を上に向けてよい一番大きい角度（度。水平が0で、上向きがプラス）")]
	const float MaxMuzzleElevation = -10.0f;
	[Tooltip("銃口の角度を抑え始める、上限の手前の幅（度）（上限の手前からなめらかに抑えて、銃口がいきなり止まって見えないようにする）")]
	const float MuzzleElevationSoftRange = 20.0f;
	//↓ハンドガンを構え始めた時に、片手（右手）で狙うのに使う↓//
	[Tooltip("ハンドガンを構えている時の移動のステート（アニメーターのステート名から作った番号）")]
	readonly int handGunAimIdleAndWalkStateHash = Animator.StringToHash("HandGunAimIdleAndWalk");
	[Tooltip("ハンドガンを片手で狙っている割合のパラメーター名（1で片手、0で両手。構え始めてからの時間に合わせて、PlayerManagerPresenterが変える）")]
	const string OneHandAimParameterName = "f_oneHandAim";
	[Tooltip("今のフレームで、ハンドガンを片手で狙っている割合")]
	float oneHandAimWeight = NoIKWeight;
	[Tooltip("片手で狙う時に、下ろした左手をずらす量（m。ひねる前のキャラクターから見た向きで、xが右、yが上、zが前）（ひねった上半身に左腕がめり込まないように、体の外側へ離す）")]
	static readonly Vector3 OneHandAimLeftHandOffset = new Vector3(-0.16f, 0.0f, 0.03f);
	[Tooltip("片手で狙う時の、右腕の伸ばし具合（肩から手までの距離が、腕の長さのどれだけになるか。1でまっすぐ伸ばしきった腕）")]
	const float OneHandAimArmStretch = 0.95f;
	[Tooltip("ハンドガンを両手で構えている時に、銃口を上げる角度（度）（両手で構えるアニメーションのままだと、銃口が弾の飛ぶ向き（カメラの正面）より3.5°ほど下を向くため）")]
	const float HandGunTwoHandAimRaiseAngle = 3.5f;
	[Tooltip("片手で狙う時に、上半身を左にひねる角度（度）（右肩を狙う方向へ出して、伸ばした右腕と背中の向きをそろえる）")]
	const float OneHandAimTwistAngle = 80.0f;
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
	//↓ハンドガンを構えて狙っている敵が近い時に、近距離の構え（C.A.R.システムのような構え）にするのに使う↓//
	[Tooltip("ハンドガンを近距離の構えにしている割合のパラメーター名（1で近距離の構え、0で普通の構え。狙っている敵が近いかに合わせて、PlayerManagerPresenterが変える）")]
	const string CloseQuarterAimParameterName = "f_closeQuarterAim";
	[Tooltip("今のフレームで、ハンドガンを近距離の構えにしている割合")]
	float closeQuarterAimWeight = NoIKWeight;
	[Tooltip("近距離の構えの時に、両手で構えるアニメーションの銃の位置からずらす量（m。キャラクターから見た向きで、xが右、yが上、zが前）（銃を顔の前の近くへ引き寄せて、両肘を曲げて構える）")]
	static readonly Vector3 CloseQuarterAimGunOffset = new Vector3(0.05f, 0.07f, -0.22f);
	[Tooltip("近距離の構えの時に、銃を内側（左）へ傾ける角度（度。銃の上側を、まっすぐ上から左へ倒す角度）")]
	const float CloseQuarterAimCantAngle = 45.0f;
	[Tooltip("近距離の構えの時に、右肘を向ける位置（m。右肩からのずれで、キャラクターから見た向き）（右肘は体の外側の少し下へ張り出す）")]
	static readonly Vector3 CloseQuarterAimRightElbowHintOffset = new Vector3(0.30f, -0.15f, -0.05f);
	[Tooltip("近距離の構えの時に、左肘を向ける位置（m。左肩からのずれで、キャラクターから見た向き）（左肘は横へ張り出して、左腕が胸の前を横切るようにする）")]
	static readonly Vector3 CloseQuarterAimLeftElbowHintOffset = new Vector3(-0.30f, -0.05f, 0.05f);
	//↓ダッシュしている時に、アサルトライフル・ショットガンを右手だけで縦に持ち上げるのに使う↓//
	[Tooltip("今のフレームで、銃を縦に持ち上げている割合（構えていない時の移動で、ダッシュのアニメーションが混ざっている割合）")]
	float dashRaiseWeight = NoIKWeight;
	[Tooltip("銃を縦に持ち上げている時に、右肘を向ける位置（m。右肩からのずれで、キャラクターから見た向き）（右肘は体の外側へ張り出す）")]
	static readonly Vector3 DashRaiseRightElbowHintOffset = new Vector3(0.35f, -0.15f, -0.05f);
	[Tooltip("右腕を走りのアニメーションにしておくレイヤーを、ダッシュの割合の何倍で効かせるか（ダッシュのアニメーションの右腕が混ざり始めるより早く、走りの右腕にしておく）")]
	const float DashRightArmWeightScale = 2.0f;
	//↓地雷を置く時に、左手を太ももや腰のポーチから離すのに使う↓//
	[Tooltip("今のフレームで、地雷を置く時にアニメーションどおりの左手をずらす量（ワールドの向き。ずらす量はMineAnimationが求める）")]
	Vector3 placeMineLeftHandOffset = Vector3.zero;

	/// <summary>
	/// IK用のUnity標準の関数
	/// </summary>
	void OnAnimatorIK()
	{
		PlayerCharacterView playerCharacterView = PlayerManagerPresenter.SingletonInstance.PlayerCharacterView;
		GunAnimation gunAnimation = playerCharacterView.FindGunAnimation(PlayerCameraManager.SingletonInstance.GunModelFacade.GetGunTYPE);

		UpdateHoldStyle(gunAnimation, FindGunModelTransform(playerCharacterView.GunViewFacade, PlayerCameraManager.SingletonInstance.GunModelFacade.GetGunTYPE), PlayerCameraManager.SingletonInstance.GunModelFacade.GetGunTYPE == EnumManager.GunTYPE.HandGun ? playerCharacterView.GunViewFacade.HandGunView : null);
		UpdateHoldStyleForPlaceMine(playerCharacterView.MineAnimation);
		SetRightHandIK();
		//※近距離の構え（ハンドガン）と、銃を縦に持ち上げるダッシュ（アサルトライフル・ショットガン）は、同時には起きない
		if (NoIKWeight < dashRaiseWeight)
		{
			SetElbowHint(AvatarIKHint.RightElbow, HumanBodyBones.RightUpperArm, dashRaiseWeight, DashRaiseRightElbowHintOffset);
		}
		else
		{
			SetElbowHint(AvatarIKHint.RightElbow, HumanBodyBones.RightUpperArm, closeQuarterAimWeight, CloseQuarterAimRightElbowHintOffset);
		}
		SetElbowHint(AvatarIKHint.LeftElbow, HumanBodyBones.LeftUpperArm, closeQuarterAimWeight, CloseQuarterAimLeftElbowHintOffset);

		if (PlayerCameraManager.SingletonInstance.GunModelFacade.GunModelBase.IsReloadTimeActive == true)
		{
			//リロード中は、左手でマガジンを交換するために、アニメーションどおりの位置へなめらかに移す
			//（マガジンを抜き取る瞬間と差し込む瞬間の前後だけ、マガジンを持つ目印に合わせる）
			leftHandWeight = gunAnimation.GetReloadLeftHandIKWeight(playerCharacterView.ReloadAnimationTime);
			magazineGripWeight = gunAnimation.GetReloadMagazineGripWeight(playerCharacterView.ReloadAnimationTime);
		}
		else
		{
			//リロードしていない時は、銃の前側を握る目印に合わせる
			//（リロードが途中で終わった時に左手がワープしないように、一瞬ではなくなめらかに戻す。普通に終わった時は、もう目印に合っている）
			leftHandWeight = Mathf.MoveTowards(leftHandWeight, FullIKWeight, Time.deltaTime / ReloadCancelBlendTime);
			magazineGripWeight = Mathf.MoveTowards(magazineGripWeight, NoIKWeight, Time.deltaTime / ReloadCancelBlendTime);
		}

		SetLeftHandIK(gunAnimation, FindLeftHandPos(PlayerCameraManager.SingletonInstance.GunModelFacade.GetGunTYPE));
	}

	/// <summary>
	/// 銃の持ち方を求める
	/// ・構えていない時に、右手（と銃）を体から離す量（構えていない時のアニメーションは体の細いキャラクター用に作られたもので、そのままだと右手や銃や左腕が防弾ベストやポーチにめり込むため）
	/// ・ハンドガンを片手で持つ割合（構えていない時と、構え始めの少しの間）
	/// ・アサルトライフル・ショットガンを、右手だけで縦に持ち上げる割合（ダッシュしている時）
	/// ※それぞれの動きがどれだけ混ざっているかに合わせて効かせるので、構えた時や構えをやめた時は、アニメーションと一緒になめらかに切り替わる
	///   （立ち止まっている時・歩いている時は「移動のレイヤーのステート」、リロード中は「リロードのアニメーションの構えていない時の動き（1 - f_aim）」が混ざっている割合）
	/// </summary>
	/// <param name="gunAnimation">今持っている銃のアニメーションの設定（銃ごとに体から離す量が違う）</param>
	/// <param name="gunModelTransform">今持っている銃のモデル（前向きが銃身、上向きが銃の上側）</param>
	/// <param name="handGunView">ハンドガンを持っている時は、ハンドガンのビュー（ほかの銃の時はnull）</param>
	void UpdateHoldStyle(GunAnimation gunAnimation, Transform gunModelTransform, HandGunView handGunView)
	{
		float relaxedLocomotionWeight = GetStateWeight(LocomotionLayerIndex, normalIdleAndWalkStateHash);
		float relaxedReloadWeight = FullIKWeight - animator.GetFloat(AimParameterName);
		float noReloadWeight = GetStateWeight(UpperBodyReloadLayerIndex, noReloadStateHash);
		Quaternion characterRotation = animator.transform.rotation;
		Quaternion relaxedLocomotionRotation = Quaternion.Slerp(Quaternion.identity, characterRotation * Quaternion.Euler(gunAnimation.RelaxedRightHandAngle) * Quaternion.Inverse(characterRotation), relaxedLocomotionWeight);
		Quaternion relaxedReloadRotation = Quaternion.Slerp(Quaternion.identity, characterRotation * Quaternion.Euler(gunAnimation.RelaxedReloadRightHandAngle) * Quaternion.Inverse(characterRotation), relaxedReloadWeight);
		relaxedRotation = Quaternion.Slerp(relaxedReloadRotation, relaxedLocomotionRotation, noReloadWeight);
		relaxedOffset = characterRotation * Vector3.Lerp(gunAnimation.RelaxedReloadRightHandOffset * relaxedReloadWeight, gunAnimation.RelaxedRightHandOffset * relaxedLocomotionWeight, noReloadWeight);

		//ダッシュしている時は（アサルトライフル・ショットガン）、右手だけで銃を縦に持ち上げる（左手は銃から離して、アニメーションどおりに振る）
		//（構えていない時の移動のステートで、ダッシュのアニメーションが混ざっている割合だけ効かせる。ハンドガンは別のステートなので、0になる）
		float dash = animator.GetFloat(DashParameterName);
		dashRaiseWeight = dash * relaxedLocomotionWeight * noReloadWeight;
		//※ダッシュのアニメーションは、手に小さい物を持って腕を大きく振る動きなので、右腕をそのまま使うと、
		//  ・銃を持ち上げる途中で、振っている腕の向きから回すため、銃の向きが一瞬ひっくり返る
		//  ・右手が小さく握った拳になり、銃のグリップを握らない
		//  そのため、右腕と右手の指だけは、専用のレイヤーで走りのアニメーション（銃を持つ腕）にしておき、そこから銃を持ち上げる
		animator.SetLayerWeight(DashRightArmLayerIndex, Mathf.Clamp01(dash * DashRightArmWeightScale) * relaxedLocomotionWeight * noReloadWeight);
		if (NoIKWeight < dashRaiseWeight)
		{
			Quaternion dashRaiseRotation = GetDashRaiseRotation(gunModelTransform, gunAnimation.DashGunAngle);
			Vector3 handPosition = animator.GetBoneTransform(HumanBodyBones.RightUpperArm).position + characterRotation * gunAnimation.DashRightHandPosition;
			relaxedRotation = Quaternion.Slerp(relaxedRotation, dashRaiseRotation, dashRaiseWeight);
			relaxedOffset = Vector3.Lerp(relaxedOffset, handPosition - rightHandAnimationPos.position, dashRaiseWeight);
		}

		//ハンドガンを構えていない時は、片手（右手）だけで持つ
		//（立ち止まっている時・歩いている時は「ハンドガンの構えていない時の移動のステート」、リロード中は「ハンドガンのリロードの構えていない時の動き」が混ざっている割合）
		float oneHandLocomotionWeight = GetStateWeight(LocomotionLayerIndex, handGunNormalIdleAndWalkStateHash) * noReloadWeight;
		//ハンドガンを構えて狙っている敵が近い時は、近距離の構えにする
		float handGunAimWeight = GetStateWeight(LocomotionLayerIndex, handGunAimIdleAndWalkStateHash) * noReloadWeight;
		float closeQuarterAim = animator.GetFloat(CloseQuarterAimParameterName);
		closeQuarterAimWeight = closeQuarterAim * handGunAimWeight;
		//ハンドガンを構え始めた時は、少しの間だけ片手（右手）で狙う（左腕は、専用のレイヤーで下ろしておく）
		//（近距離の構えの時は、片手で狙わずに、すぐに両手で構える）
		oneHandAimWeight = animator.GetFloat(OneHandAimParameterName) * handGunAimWeight * (FullIKWeight - closeQuarterAim);
		animator.SetLayerWeight(HandGunOneHandAimLeftArmLayerIndex, oneHandAimWeight);
		oneHandWeight = oneHandLocomotionWeight + oneHandAimWeight + GetStateWeight(UpperBodyReloadLayerIndex, handGunReloadStateHash) * relaxedReloadWeight + dashRaiseWeight;

		upperBodyTwistAngle = NoTwistAngle;
		if (handGunView == null)
		{
			return;
		}

		//片手で持って走っている時は、腕を前に振っても銃口が上を向かないように、右手首を回す
		if (NoIKWeight < oneHandLocomotionWeight)
		{
			relaxedRotation = Quaternion.Slerp(Quaternion.identity, GetMuzzleDownRotation(handGunView), oneHandLocomotionWeight) * relaxedRotation;
		}

		//片手で狙う時は、上半身を左にひねって右肩を前に出し、右腕を右肩の正面へまっすぐ伸ばして、銃口を正面に向ける
		if (NoIKWeight < oneHandAimWeight)
		{
			TwistUpperBody(-OneHandAimTwistAngle * oneHandAimWeight);
			//※右手（と銃）は背骨の子なので、ひねった分だけ動いた後の位置・向きから、腕を伸ばした位置・向きへ移す
			Vector3 twistedHandPosition = TwistPoint(rightHandAnimationPos.position);
			Vector3 handPosition = Vector3.Lerp(twistedHandPosition, GetOneHandAimHandPosition(twistedHandPosition), oneHandAimWeight);
			Quaternion aimRotation = Quaternion.Slerp(Quaternion.identity, GetOneHandAimRotation(GetTwistRotation() * GetMuzzleDirection(handGunView)), oneHandAimWeight);
			relaxedRotation = aimRotation * GetTwistRotation() * relaxedRotation;
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
		float twoHandAimWeight = handGunAimWeight - oneHandAimWeight;
		if (NoIKWeight < twoHandAimWeight)
		{
			relaxedRotation = Quaternion.AngleAxis(-HandGunTwoHandAimRaiseAngle * twoHandAimWeight, animator.transform.right) * relaxedRotation;
		}
	}

	/// <summary>
	/// 地雷を置いている間の銃の持ち方を求める（地雷を置くアニメーションが混ざっている割合だけ、ほかの持ち方から切り替える）
	/// ・右手（と銃）は、地雷を置いている時のずらし量で体から離す（銃を縦に持ち上げるダッシュの持ち方はやめる）
	/// ・左手は銃から離して、地面へ下ろす（左の太ももを避けて、両足の間の前へ下ろす）
	///   立ち上がる時は、地面から銃の前側へ戻す（ハンドガンは片手で持つので、体の外側を通して下ろした位置へ戻す）
	/// </summary>
	/// <param name="mineAnimation">地雷を置く時のアニメーションの設定</param>
	void UpdateHoldStyleForPlaceMine(MineAnimation mineAnimation)
	{
		float placeMineWeight = mineAnimation.Weight;
		//しゃがんでいる間は両足の間の前へ、立ち上がる時は体の外側へ、左手をずらす
		placeMineLeftHandOffset = mineAnimation.GetLeftHandOffset(animator.transform.rotation);
		if (placeMineWeight <= NoIKWeight)
		{
			return;
		}

		relaxedRotation = Quaternion.Slerp(relaxedRotation, Quaternion.identity, placeMineWeight);
		relaxedOffset = Vector3.Lerp(relaxedOffset, animator.transform.rotation * mineAnimation.RightHandOffset, placeMineWeight);
		dashRaiseWeight = Mathf.Lerp(dashRaiseWeight, NoIKWeight, placeMineWeight);
		oneHandWeight = Mathf.Lerp(oneHandWeight, FullIKWeight, placeMineWeight * mineAnimation.LeftHandFreeWeight);
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
	/// 上半身（背骨）を、キャラクターの上向きを軸にしてひねる（顔は正面を向いたままにするため、首と頭は逆向きに戻す）
	/// ※IKのパスの中だけで使える、骨の回転を書き換える関数（SetBoneLocalRotation）を使う
	/// </summary>
	/// <param name="angle">ひねる角度（度。マイナスが左）</param>
	void TwistUpperBody(float angle)
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
	Vector3 TwistPoint(Vector3 position)
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
	Quaternion GetTwistRotation()
	{
		return Quaternion.AngleAxis(upperBodyTwistAngle, animator.transform.up);
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
		Vector3 elbowPosition = animator.GetBoneTransform(HumanBodyBones.RightLowerArm).position;
		float armLength = Vector3.Distance(shoulderPosition, elbowPosition) + Vector3.Distance(elbowPosition, rightHandAnimationPos.position);
		Vector3 twistedShoulderPosition = TwistPoint(shoulderPosition);
		Vector3 armDirection = (animator.transform.forward * armLength + Vector3.up * (twistedHandPosition.y - twistedShoulderPosition.y)).normalized;
		return twistedShoulderPosition + armDirection * armLength * OneHandAimArmStretch;
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
	/// <param name="handGunModel">ハンドガンのモデル</param>
	/// <returns>銃口の向き（長さ1）</returns>
	Vector3 GetMuzzleDirection(HandGunView handGunView)
	{
		return (handGunView.HandGunMuzzleTransform.position - handGunView.GetHandGunModel.transform.position).normalized;
	}

	/// <summary>
	/// 銃口が上を向きすぎないように、右手（と銃）を右手の位置を中心に回す回転を求める
	/// （片手で持って走るアニメーションは、手に物を持って走る動きなので、そのままだと腕を前に振った時に銃口が45度ほど上を向いてしまうため）
	/// </summary>
	/// <param name="handGunModel">ハンドガンのモデル</param>
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
	///   重みは1のままにして、合わせる位置・角度の方を「アニメーションどおりの左手 → マガジンを持つ目印 → 銃の前側を握る目印」の順に混ぜる
	/// ※銃の子にある目印は、右手を体から離す前の銃の位置にあるので、右手と同じだけ動かしてから使う
	/// ※片手で持つ時（ハンドガンを構えていない時）は、左手で銃の前側を握らない（マガジンを持つ目印には、リロード中は合わせる）
	/// </summary>
	/// <param name="gunAnimation">今持っている銃のアニメーションの設定</param>
	/// <param name="leftHandPos">左手で銃の前側を握る目印</param>
	void SetLeftHandIK(GunAnimation gunAnimation, Transform leftHandPos)
	{
		float gripWeight = leftHandWeight * (FullIKWeight - oneHandWeight);
		if (gripWeight <= NoIKWeight && magazineGripWeight <= NoIKWeight && oneHandAimWeight <= NoIKWeight && placeMineLeftHandOffset == Vector3.zero)
		{
			animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, NoIKWeight);
			animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, NoIKWeight);
			return;
		}

		//※上半身をひねっている時は、アニメーションどおりの左手も、ひねった分だけ動いた後の位置・向きにする
		//※片手で狙っている時は、下ろした左手を体の外側へ少し離す（ひねった上半身に、左腕がめり込まないようにする）
		//※地雷を置く時は、左手を太ももや腰のポーチから離す
		Vector3 animationPosition = TwistPoint(leftHandAnimationPos.position) + GetTwistRotation() * (animator.transform.rotation * OneHandAimLeftHandOffset) * oneHandAimWeight + placeMineLeftHandOffset;
		Vector3 position = Vector3.Lerp(animationPosition, MoveAwayFromBody(gunAnimation.MagazineLeftHandGripPos.position), magazineGripWeight);
		Quaternion rotation = Quaternion.Slerp(GetTwistRotation() * leftHandAnimationPos.rotation, relaxedRotation * gunAnimation.MagazineLeftHandGripPos.rotation, magazineGripWeight);
		animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, FullIKWeight);
		animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, FullIKWeight);
		animator.SetIKPosition(AvatarIKGoal.LeftHand, Vector3.Lerp(position, MoveAwayFromBody(leftHandPos.position), gripWeight));
		animator.SetIKRotation(AvatarIKGoal.LeftHand, Quaternion.Slerp(rotation, relaxedRotation * leftHandPos.rotation, gripWeight));
	}

	/// <summary>
	/// レイヤーの中で、指定したステートのアニメーションがどれだけ混ざっているかを求める（0で全く混ざっていない、1でそのステートだけ）
	/// ※ステートが切り替わる遷移の間は、遷移の進み具合に合わせて混ざる割合が変わる
	/// </summary>
	/// <param name="layerIndex">アニメーターのレイヤーの番号</param>
	/// <param name="stateHash">ステート（ステート名から作った番号）</param>
	/// <returns>ステートのアニメーションが混ざっている割合</returns>
	float GetStateWeight(int layerIndex, int stateHash)
	{
		float currentStateWeight = animator.GetCurrentAnimatorStateInfo(layerIndex).shortNameHash == stateHash ? FullIKWeight : NoIKWeight;
		if (animator.IsInTransition(layerIndex) == false)
		{
			return currentStateWeight;
		}

		float nextStateWeight = animator.GetNextAnimatorStateInfo(layerIndex).shortNameHash == stateHash ? FullIKWeight : NoIKWeight;
		return Mathf.Lerp(currentStateWeight, nextStateWeight, animator.GetAnimatorTransitionInfo(layerIndex).normalizedTime);
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

	/// <summary>
	/// 銃の種類から、右手に持っている銃のモデルを探す
	/// </summary>
	/// <param name="gunModelFacade">銃のモデル</param>
	/// <param name="gunTYPE">銃の種類</param>
	/// <returns>銃のモデル（前向きが銃身、上向きが銃の上側）</returns>
	Transform FindGunModelTransform(GunViewFacade gunModelFacade, EnumManager.GunTYPE gunTYPE)
	{
		switch (gunTYPE)
		{
			case EnumManager.GunTYPE.HandGun:
				return gunModelFacade.HandGunView.GetHandGunModel.transform;
			case EnumManager.GunTYPE.AssaultRifle:
				return gunModelFacade.AssaultRifleView.GetAssaultRifleModel.transform;
			case EnumManager.GunTYPE.ShotGun:
				return gunModelFacade.ShotGunView.GetShotGunModel.transform;
		}
		return null;
	}

	/// <summary>
	/// 銃の種類から、左手で銃の前側を握る目印を探す
	/// </summary>
	/// <param name="gunTYPE">銃の種類</param>
	/// <returns>左手の目印</returns>
	Transform FindLeftHandPos(EnumManager.GunTYPE gunTYPE)
	{
		switch (gunTYPE)
		{
			case EnumManager.GunTYPE.HandGun:
				return handGunLeftHandPos;
			case EnumManager.GunTYPE.AssaultRifle:
				return assaultRifleLeftHandPos;
			case EnumManager.GunTYPE.ShotGun:
				return shotGunLeftHandPos;
		}
		return null;
	}
}
