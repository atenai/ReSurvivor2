using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 銃ごとの、プレイヤーのアニメーションの設定
/// （左手で銃を握る位置・リロード中のマガジンの動き・リロード中の左手のIK・構えていない時やダッシュの時の銃の持ち方）
/// ※GunAnimationFacadeが銃ごとに持ち、PlayerCharacterViewとHandIKが銃の状態を見て使う（銃のクラスからは使わない）
/// </summary>
[System.Serializable]
public class GunAnimation
{
	[Header("左手で銃を握る位置")]
	[Tooltip("左手で銃の前側を握る位置・向き（銃の子に置いた目印。向きはIK用の手の向き）")]
	[SerializeField] Transform leftHandGripPos;
	public Transform LeftHandGripPos => leftHandGripPos;

	[Header("リロード")]
	//※リロードのアニメーションは、左手で銃からマガジンを抜いて腰のポーチへ持っていき、また銃に差し込む動きになっているため、
	//  その間はマガジンを左手の子にして、左手と一緒に動かす
	//  （ショットガンは、右手で腰のポーチから弾を取って装填口に込めるので、その間は弾を右手の子にする。手の動きはShotGunReloadAnimationが作る）
	//※タイミングはどれもリロードアニメーションの再生位置（0がアニメーションの始まり、1が終わり）で決める
	[Tooltip("リロード中に手で抜き取って差し込むマガジンのモデル（マガジンと一緒に動かす弾も入れる。ショットガンは込める弾）")]
	[SerializeField] Transform[] magazineModels;
	[Tooltip("手に持っている間だけ見せるか？（ショットガンの弾のように、銃に入っている時は見えない物はオンにする）")]
	[SerializeField] bool isMagazineShownOnlyInHand;
	[Tooltip("マガジンを手に持たせる位置・向き（手の骨の子に置いた目印。ショットガンの弾は右手）。空の場合は、抜き取った時の位置・向きのまま左手に持たせる")]
	[SerializeField] Transform magazineHandPos;
	[Tooltip("左手でマガジンを抜き取るタイミング（リロードアニメーションの再生位置）。ショットガンは右手がポーチに着いて弾を取るタイミング")]
	[SerializeField] float magazineOutTime;
	public float MagazineOutTime => magazineOutTime;
	[Tooltip("マガジンを銃に差し込むタイミング（リロードアニメーションの再生位置）")]
	[SerializeField] float magazineInTime;
	public float MagazineInTime => magazineInTime;
	[Tooltip("差し込んだマガジンを銃の元の位置まで動かす速さ（m/秒）")]
	const float MagazineMoveSpeed = 0.8f;
	[Tooltip("差し込んだマガジンを銃の元の向きまで回す速さ（度/秒）")]
	const float MagazineRotateSpeed = 360.0f;
	[Tooltip("マガジンを銃に付けている時の親")]
	Transform[] magazineParents;
	[Tooltip("マガジンを銃に付けている時の位置（親から見た位置）")]
	Vector3[] magazineLocalPositions;
	[Tooltip("マガジンを銃に付けている時の向き（親から見た向き）")]
	Quaternion[] magazineLocalRotations;
	[Tooltip("マガジンを手に持っているか？")]
	bool isMagazineInHand = false;

	[Header("リロード中の左手のIK（ハンドガン・アサルトライフル）")]
	//※リロードの始まりと終わりは、左手を銃の前側を握る目印（IK）とアニメーションどおりの位置の間でなめらかに移す
	//  （一瞬で切り替えると、左手が20cmほどワープして見えるため）
	//※ショットガンのリロードは、左手で銃を握ったまま右手で弾を込めるので、ここは使わない（手の動きはShotGunReloadAnimationが作る）
	[Tooltip("左手を目印からアニメーションどおりの位置へ移し終わるタイミング（リロードアニメーションの再生位置）。アニメーションの始まりからここまでで移す")]
	[SerializeField] float leftHandIKReleaseTime;
	[Tooltip("左手をアニメーションどおりの位置から目印へ戻し始めるタイミング（リロードアニメーションの再生位置）。ここからアニメーションの終わりまでで戻す")]
	[SerializeField] float leftHandIKReturnTime;
	//※リロードのアニメーションは体の形が違うキャラクター用に作られたもので、マガジンを抜く瞬間と差し込む瞬間の左手が、マガジンから数cmずれている。
	//  そのため、その2つの瞬間の前後だけ、左手をIKでマガジンを持つ位置に合わせる（マガジンが手から浮いたり、差し込む時にずれて見えたりしないようにする）
	[Tooltip("マガジンを銃に付けたまま左手で持つ時の左手の位置・向き（銃の子に置いた目印。向きはIK用の手の向き）。ショットガンは空")]
	[SerializeField] Transform magazineLeftHandGripPos;
	public Transform MagazineLeftHandGripPos => magazineLeftHandGripPos;
	[Tooltip("マガジンを抜き取る瞬間・差し込む瞬間の前後で、左手をマガジンを持つ位置に合わせる長さ（リロードアニメーションの再生位置。この分だけ前からなめらかに合わせ始め、この分だけ後までなめらかに戻す）")]
	const float MagazineGripBlendTime = 0.06f;
	[Tooltip("左手をアニメーションどおりの位置にする時のIKの効き具合")]
	const float NoIKWeight = 0.0f;
	[Tooltip("左手を目印にぴったり合わせる時のIKの効き具合")]
	const float FullIKWeight = 1.0f;
	[Tooltip("リロードアニメーションの終わりの再生位置")]
	const float ReloadAnimationEndTime = 1.0f;

	[Header("構えていない時の銃の位置")]
	//※構えていない時のアニメーションは体の細いキャラクター用に作られたもので、このキャラクターでは右手や銃や左腕が防弾ベストやポーチにめり込む。
	//  そのため、構えていない時は、右手をIKで体から離す（銃は右手の子なので一緒に動き、左手も銃を握ったままついていく）
	[Tooltip("構えていない時（立ち止まっている時・歩いている時）に、右手（と銃）をずらす量（m。キャラクターから見た向きで、xが右、yが上、zが前）")]
	[SerializeField] Vector3 relaxedRightHandOffset;
	public Vector3 RelaxedRightHandOffset => relaxedRightHandOffset;
	[Tooltip("構えていない時（立ち止まっている時・歩いている時）に、右手（と銃）を右手の位置を中心に回す角度（度。キャラクターから見た向きで、xが前後の傾き、yが左右の向き、zがひねり）")]
	[SerializeField] Vector3 relaxedRightHandAngle;
	public Vector3 RelaxedRightHandAngle => relaxedRightHandAngle;
	[Tooltip("構えずにリロードしている時に、右手（と銃）をずらす量（m。キャラクターから見た向きで、xが右、yが上、zが前）")]
	[SerializeField] Vector3 relaxedReloadRightHandOffset;
	public Vector3 RelaxedReloadRightHandOffset => relaxedReloadRightHandOffset;
	[Tooltip("構えずにリロードしている時に、右手（と銃）を右手の位置を中心に回す角度（度。キャラクターから見た向き）")]
	[SerializeField] Vector3 relaxedReloadRightHandAngle;
	public Vector3 RelaxedReloadRightHandAngle => relaxedReloadRightHandAngle;

	[Header("ダッシュしている時の銃の持ち方（アサルトライフル・ショットガン）")]
	//※ダッシュしている時は、右手だけで銃を縦に持ち上げて走る（ゴーストリコン フューチャーソルジャーのダッシュのような持ち方）。
	//  左手は銃から離して、ダッシュのアニメーションどおりに振る。ハンドガンは片手で持つ別のアニメーションなので使わない
	[Tooltip("ダッシュしている時の右手の位置（m。右肩からのずれで、キャラクターから見た向き。xが右、yが上、zが前）")]
	[SerializeField] Vector3 dashRightHandPosition;
	public Vector3 DashRightHandPosition => dashRightHandPosition;
	[Tooltip("ダッシュしている時の銃の傾き（度。銃口をまっすぐ上に向けて銃の上側を後ろに向けた向きから、xが銃口を前へ倒す角度、yが銃身を軸に銃の上側を右へ回す角度、zが銃口を右へ倒す角度）")]
	[SerializeField] Vector3 dashGunAngle;
	public Vector3 DashGunAngle => dashGunAngle;

	[Header("ハンドガンに持ち替えた時の、左手での持ち方（アサルトライフル・ショットガン）")]
	//※弾が切れてハンドガンに持ち替えた時は、この銃を左手に持ち替えて、マックス・ペイン3のように、腰の高さで銃口を前に向け、
	//  銃を斜め（銃口が下、ストックが上）にして、銃の上側を、カバンの取っ手のように左手で上からつかんで持つ
	[Tooltip("左手で銃の上側をつかむ位置・向き（銃の子に置いた目印。向きはIK用の手の向き。手のひらを銃の左側に当て、指を銃の上から右側へ回し、人差し指を銃口側に向けた手。銃を斜めに持つ分だけ、手を銃に対して傾けておく）")]
	[SerializeField] Transform sideArmLeftHandGripPos;
	public Transform SideArmLeftHandGripPos => sideArmLeftHandGripPos;
	[Tooltip("左手で持っている時に、下ろした左手をずらす量（m。キャラクターから見た向きで、xが右、yが上、zが前）（銃が体や、体に付けたほかの銃（左の腰のアサルトライフル）に当たらないように離す）")]
	[SerializeField] Vector3 sideArmCarryLeftHandOffset;
	public Vector3 SideArmCarryLeftHandOffset => sideArmCarryLeftHandOffset;
	[Tooltip("左手で持っている時の銃の向き（度。キャラクターから見た向きで、銃口を正面に向けて水平にした向きから、xが銃口を下へ向ける角度（プラスで銃口が下、ストックが上の斜めの持ち方）、yが銃口を右へ向ける角度、zが銃身を軸に銃の上側を右へ倒す角度）")]
	[SerializeField] Vector3 sideArmCarryGunAngle;
	public Vector3 SideArmCarryGunAngle => sideArmCarryGunAngle;
	[Tooltip("左手で持っている時の、左手の指の曲げ具合（1でアニメーションどおりの握りこぶし、0でまっすぐ伸ばした指）（握りこぶしのままだと、太い銃に指がめり込むため、銃の太さに合わせて指を開く）")]
	[Range(0.0f, 1.0f)]
	[SerializeField] float sideArmCarryFingerCurl = 1.0f;
	public float SideArmCarryFingerCurl => sideArmCarryFingerCurl;

	[Header("武器を切り替える時の、銃をしまう・取り出す動き")]
	//※銃は、右手で持っている位置から、体のしまう場所（ハンドガンは右太もものホルスター、アサルトライフルは左の腰、ショットガンは背中）まで、
	//  右手（銃を握る位置）が中継点の方へ膨らんだ曲線を通るように動かす（取り出す時は、同じ曲線を逆にたどる）。
	//  途中で通る姿勢を決めた銃は、持っている位置から途中の姿勢までを曲線で動かし、途中の姿勢からしまう場所まではまっすぐ動かす
	[Tooltip("銃をしまう・取り出す時に、右手（銃を握る位置）の通る曲線を膨らませる中継点（m。右肩からのずれで、キャラクターから見た向き。xが右、yが上、zが前）（銃や右手が体にめり込まないように、体の外側に置く。右肩からのずれなので、構えて上半身の向きが変わってもついていく）")]
	[SerializeField] Vector3 weaponSwitchWaypoint;
	public Vector3 WeaponSwitchWaypoint => weaponSwitchWaypoint;
	[Tooltip("中継点での銃の向き（度。キャラクターから見た向きで、銃口を正面に向けて水平にした向きから、xが銃口を下へ向ける角度（マイナスで上）、yが銃口を右へ向ける角度、zが銃身を軸に銃の上側を右へ倒す角度）")]
	[SerializeField] Vector3 weaponSwitchWaypointGunAngle;
	public Vector3 WeaponSwitchWaypointGunAngle => weaponSwitchWaypointGunAngle;
	[Tooltip("中継点で、銃を中継点での銃の向きにする割合（0で、持っている向きとしまう向きのちょうど間の向きのまま回す。1で、中継点での銃の向きを通るように回す）")]
	[Range(0.0f, 1.0f)]
	[SerializeField] float weaponSwitchWaypointGunAngleWeight;
	public float WeaponSwitchWaypointGunAngleWeight => weaponSwitchWaypointGunAngleWeight;
	[Tooltip("銃をしまう動き（持っている位置が0、しまう場所が1）のうち、途中の姿勢を通る所（1で、途中の姿勢を通らずに、しまう場所まで曲線で動かす）")]
	[Range(0.0f, 1.0f)]
	[SerializeField] float weaponSwitchViaPoseRate = 1.0f;
	public float WeaponSwitchViaPoseRate => weaponSwitchViaPoseRate;
	[Tooltip("途中の姿勢の、右手（銃を握る位置）の位置（m。右肩からのずれで、キャラクターから見た向き。xが右、yが上、zが前）")]
	[SerializeField] Vector3 weaponSwitchViaHandPosition;
	public Vector3 WeaponSwitchViaHandPosition => weaponSwitchViaHandPosition;
	[Tooltip("途中の姿勢の、銃の向き（度。キャラクターから見た向きで、銃口を正面に向けて水平にした向きから、xが銃口を下へ向ける角度、yが銃口を右へ向ける角度、zが銃身を軸に銃の上側を右へ倒す角度）")]
	[SerializeField] Vector3 weaponSwitchViaGunAngle;
	public Vector3 WeaponSwitchViaGunAngle => weaponSwitchViaGunAngle;
	[Tooltip("途中の姿勢からしまう場所まで銃を動かす時に、銃の中心の通る線を膨らませる量（m。途中の姿勢としまう場所のちょうど間からのずれで、キャラクターから見た向き。xが右、yが上、zが前）（まっすぐ動かすと、銃が体にめり込むため、体の外側を通す）")]
	[SerializeField] Vector3 weaponSwitchViaToStowBulge;
	public Vector3 WeaponSwitchViaToStowBulge => weaponSwitchViaToStowBulge;
	[Tooltip("銃をしまう・取り出す時に、右手で銃の前側を持つ位置・向き（銃の子に置いた目印。向きはIK用の手の向き）（空の時は、グリップを握ったまま動かす）（ショットガンは、右脇の下から右手で銃の前側を持って背中との間を動かし、前に出した所で左手に前側を持たせて、右手はグリップに持ち替える）")]
	[SerializeField] Transform weaponSwitchRightHandForendGripPos;
	public Transform WeaponSwitchRightHandForendGripPos => weaponSwitchRightHandForendGripPos;
	[Tooltip("右手で銃の前側を持って動かす間に、しまう場所へ近づくほど、右手を銃口の方へずらす距離（m）（持っている位置の近くでは、右手を体に近い所で持たせて腕が伸びきらないようにし、しまう場所では、右手を右腰の後ろにある銃の前側で持たせる）")]
	[SerializeField] float weaponSwitchRightHandForendSlideDistance;
	public float WeaponSwitchRightHandForendSlideDistance => weaponSwitchRightHandForendSlideDistance;
	[Tooltip("銃をしまう動き（持っている位置が0、しまう場所が1）のうち、右手をグリップから離し終わる所（1で離さない）（1未満の銃は、左手で銃の前側を握ったまま、しまう場所まで左手で持っていく。アサルトライフルは、左の腰まで右手が届かず、右腕がお腹の上に伸びきって体に埋もれるため）")]
	[Range(0.0f, 1.0f)]
	[SerializeField] float weaponSwitchRightHandReleaseRate = 1.0f;
	public float WeaponSwitchRightHandReleaseRate => weaponSwitchRightHandReleaseRate;
	[Tooltip("左手でしまう場所まで持っていく時に、左手で銃を外側から持つ位置・向き（銃の子に置いた目印。向きはIK用の手の向き）（空の時は、銃の前側を下から握ったまま持っていく）（しまう時は、右手をグリップから離す前に左手を下から外側へ持ち替え、取り出す時は、右手でグリップを握ってから左手を下から握る持ち方へ戻す）")]
	[SerializeField] Transform weaponSwitchLeftHandCarryGripPos;
	public Transform WeaponSwitchLeftHandCarryGripPos => weaponSwitchLeftHandCarryGripPos;
	[Tooltip("右手を体の外側や後ろへ伸ばす時に、右肘を向ける位置（m。右肩からのずれで、キャラクターから見た向き。xが右、yが上、zが前）")]
	[SerializeField] Vector3 weaponSwitchRightElbowHintOffset;
	public Vector3 WeaponSwitchRightElbowHintOffset => weaponSwitchRightElbowHintOffset;
	[Tooltip("右肘の向きを決める効き具合の最大（しまう場所に近いほど強く効かせる。0で効かせない）")]
	[Range(0.0f, 1.0f)]
	[SerializeField] float weaponSwitchRightElbowHintWeight;
	public float WeaponSwitchRightElbowHintWeight => weaponSwitchRightElbowHintWeight;

	/// <summary>
	/// マガジンを銃に付けている時の親・位置・向きを覚えておく（差し込む時に元に戻すため）
	/// </summary>
	public void Initialize()
	{
		magazineParents = new Transform[magazineModels.Length];
		magazineLocalPositions = new Vector3[magazineModels.Length];
		magazineLocalRotations = new Quaternion[magazineModels.Length];
		for (int i = 0; i < magazineModels.Length; i++)
		{
			magazineParents[i] = magazineModels[i].parent;
			magazineLocalPositions[i] = magazineModels[i].localPosition;
			magazineLocalRotations[i] = magazineModels[i].localRotation;
		}
	}

	/// <summary>
	/// リロード中のマガジンの動き（抜き取る → 手で持つ → 差し込む。ショットガンは、ポーチから取った弾を右手で持って込める）
	/// </summary>
	/// <param name="isReloadTimeActive">この銃がリロード中か？</param>
	/// <param name="reloadAnimationTime">リロードアニメーションの再生位置（0がアニメーションの始まり、1が終わり）</param>
	/// <param name="isReloadAnimationForward">リロードアニメーションを前へ進めているか？（巻き戻している間はfalse）</param>
	/// <param name="hand_l">左手の骨</param>
	public void UpdateReloadMagazine(bool isReloadTimeActive, float reloadAnimationTime, bool isReloadAnimationForward, Transform hand_l)
	{
		//アニメーションを巻き戻している間（ショットガンが次の弾を取りにポーチへ戻る間）は、手に何も持たない
		bool isMagazineOut = isReloadTimeActive == true && isReloadAnimationForward == true && magazineOutTime <= reloadAnimationTime && reloadAnimationTime < magazineInTime;
		if (isMagazineOut == true && isMagazineInHand == false)
		{
			TakeOutMagazine(hand_l);
		}
		else if (isMagazineOut == false && isMagazineInHand == true)
		{
			PutInMagazine();
		}

		MoveMagazineToGun();
	}

	/// <summary>
	/// マガジンを銃から抜き取って手に持たせる（ショットガンの弾は、ポーチから取って右手に持たせる）
	/// </summary>
	/// <param name="hand_l">左手の骨</param>
	void TakeOutMagazine(Transform hand_l)
	{
		isMagazineInHand = true;
		foreach (Transform magazineModel in magazineModels)
		{
			if (magazineHandPos != null)
			{
				//手に持たせる目印の位置・向きで持たせる
				magazineModel.SetParent(magazineHandPos, false);
				magazineModel.localPosition = Vector3.zero;
				magazineModel.localRotation = Quaternion.identity;
			}
			else
			{
				//抜き取った時の位置・向きのまま左手の子にする（以後は左手と一緒に動く）
				magazineModel.SetParent(hand_l, true);
			}
			magazineModel.gameObject.SetActive(true);
		}
	}

	/// <summary>
	/// 手に持っているマガジンを銃に差し込む（銃の子に戻す。元の位置へはMoveMagazineToGunで少しずつ動かす）
	/// </summary>
	void PutInMagazine()
	{
		isMagazineInHand = false;
		for (int i = 0; i < magazineModels.Length; i++)
		{
			magazineModels[i].SetParent(magazineParents[i], true);
		}
	}

	/// <summary>
	/// 差し込んだマガジンを銃の元の位置・向きまで動かす（手の位置と元の位置のずれを、差し込む動きに見せる）
	/// </summary>
	void MoveMagazineToGun()
	{
		if (isMagazineInHand == true)
		{
			return;
		}

		for (int i = 0; i < magazineModels.Length; i++)
		{
			magazineModels[i].localPosition = Vector3.MoveTowards(magazineModels[i].localPosition, magazineLocalPositions[i], MagazineMoveSpeed * Time.deltaTime);
			magazineModels[i].localRotation = Quaternion.RotateTowards(magazineModels[i].localRotation, magazineLocalRotations[i], MagazineRotateSpeed * Time.deltaTime);
			//銃に入っている時は見えない物は、元の位置まで入ったら隠す
			if (isMagazineShownOnlyInHand == true && magazineModels[i].localPosition == magazineLocalPositions[i])
			{
				magazineModels[i].gameObject.SetActive(false);
			}
		}
	}

	/// <summary>
	/// リロード中の左手のIKの効き具合を求める（0でアニメーションどおりの左手、1で銃の前側を握る目印に合わせた左手）
	/// </summary>
	/// <param name="reloadAnimationTime">リロードアニメーションの再生位置（0がアニメーションの始まり、1が終わり）</param>
	/// <returns>左手のIKの効き具合</returns>
	public float GetReloadLeftHandIKWeight(float reloadAnimationTime)
	{
		//Mathf.SmoothStepは、0〜1の割合を「ゆっくり動き出して、ゆっくり止まる」形に変えて、2つの値の間を求める関数
		if (reloadAnimationTime < leftHandIKReleaseTime)
		{
			return Mathf.SmoothStep(FullIKWeight, NoIKWeight, reloadAnimationTime / leftHandIKReleaseTime);
		}

		if (leftHandIKReturnTime < reloadAnimationTime)
		{
			return Mathf.SmoothStep(NoIKWeight, FullIKWeight, (reloadAnimationTime - leftHandIKReturnTime) / (ReloadAnimationEndTime - leftHandIKReturnTime));
		}

		return NoIKWeight;
	}

	/// <summary>
	/// リロード中に、左手をマガジンを持つ位置に合わせる効き具合を求める
	/// （0でアニメーションどおりの左手、1でマガジンを持つ目印に合わせた左手。マガジンを抜き取る瞬間と差し込む瞬間にちょうど1になる）
	/// </summary>
	/// <param name="reloadAnimationTime">リロードアニメーションの再生位置（0がアニメーションの始まり、1が終わり）</param>
	/// <returns>左手のIKの効き具合</returns>
	public float GetReloadMagazineGripWeight(float reloadAnimationTime)
	{
		return Mathf.Max(GetMagazineGripWeight(reloadAnimationTime, magazineOutTime), GetMagazineGripWeight(reloadAnimationTime, magazineInTime));
	}

	/// <summary>
	/// 指定したタイミングの前後で、左手をマガジンを持つ位置に合わせる効き具合を求める（タイミングちょうどで1、MagazineGripBlendTime以上離れると0）
	/// </summary>
	/// <param name="reloadAnimationTime">リロードアニメーションの再生位置</param>
	/// <param name="gripTime">マガジンを持つ位置にぴったり合わせるタイミング（リロードアニメーションの再生位置）</param>
	/// <returns>左手のIKの効き具合</returns>
	float GetMagazineGripWeight(float reloadAnimationTime, float gripTime)
	{
		return Mathf.SmoothStep(FullIKWeight, NoIKWeight, Mathf.Abs(reloadAnimationTime - gripTime) / MagazineGripBlendTime);
	}
}
