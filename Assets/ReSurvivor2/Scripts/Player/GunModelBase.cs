using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ガンモデル
/// </summary>
public abstract class GunModelBase : MonoBehaviour
{
	[Header("リロード")]
	//※リロードのアニメーションは、左手で銃からマガジンを抜いて腰のポーチへ持っていき、また銃に差し込む動きになっているため、
	//  その間はマガジンを左手の子にして、左手と一緒に動かす
	//※タイミングはどれもリロードアニメーションの再生位置（0がアニメーションの始まり、1が終わり）で決める
	[Tooltip("左手の骨")]
	[SerializeField] Transform hand_l;
	[Tooltip("リロード中に左手で抜き取って差し込むマガジンのモデル（マガジンと一緒に動かす弾も入れる）")]
	[SerializeField] Transform[] magazineModels;
	[Tooltip("左手に持っている間だけ見せるか？（ショットガンの弾のように、銃に入っている時は見えない物はオンにする）")]
	[SerializeField] bool isMagazineShownOnlyInHand;
	[Tooltip("マガジンを左手に持たせる位置・向き（左手の骨の子に置いた目印）。空の場合は、抜き取った時の位置・向きのまま左手に持たせる")]
	[SerializeField] Transform magazineLeftHandPos;
	[Tooltip("左手でマガジンを抜き取るタイミング（リロードアニメーションの再生位置）。ショットガンは左手が最初にポーチに着いて弾を取るタイミング")]
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
	[Tooltip("マガジンを左手に持っているか？")]
	bool isMagazineInLeftHand = false;

	[Header("リロード中の左手のIK")]
	//※リロードの始まりと終わりは、左手を銃の前側を握る目印（IK）とアニメーションどおりの位置の間でなめらかに移す
	//  （一瞬で切り替えると、左手が20cmほどワープして見えるため）
	[Tooltip("左手を目印からアニメーションどおりの位置へ移し終わるタイミング（リロードアニメーションの再生位置）。アニメーションの始まりからここまでで移す")]
	[SerializeField] float leftHandIKReleaseTime;
	[Tooltip("左手をアニメーションどおりの位置から目印へ戻し始めるタイミング（リロードアニメーションの再生位置）。ここからアニメーションの終わりまでで戻す")]
	[SerializeField] float leftHandIKReturnTime;
	//※リロードのアニメーションは体の形が違うキャラクター用に作られたもので、マガジンを抜く瞬間と差し込む瞬間の左手が、マガジンから数cmずれている。
	//  そのため、その2つの瞬間の前後だけ、左手をIKでマガジンを持つ位置に合わせる（マガジンが手から浮いたり、差し込む時にずれて見えたりしないようにする）
	[Tooltip("マガジンを銃に付けたまま左手で持つ時の左手の位置・向き（銃の子に置いた目印。向きはIK用の手の向き）")]
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
	[Tooltip("ダッシュしている時に、右手（と銃）をずらす量（m。キャラクターから見た向きで、xが右、yが上、zが前）（ダッシュのアニメーションは銃を胸に抱え込むので、走っている時とは別の量にする）")]
	[SerializeField] Vector3 relaxedDashRightHandOffset;
	public Vector3 RelaxedDashRightHandOffset => relaxedDashRightHandOffset;
	[Tooltip("ダッシュしている時に、右手（と銃）を右手の位置を中心に回す角度（度。キャラクターから見た向きで、xが前後の傾き、yが左右の向き、zがひねり）")]
	[SerializeField] Vector3 relaxedDashRightHandAngle;
	public Vector3 RelaxedDashRightHandAngle => relaxedDashRightHandAngle;

	/// <summary>
	/// この銃の種類（どの銃のリロード中かを調べるのに使う）
	/// </summary>
	protected abstract EnumManager.GunTYPE GunType { get; }

	void Start()
	{
		//マガジンを銃に付けている時の親・位置・向きを覚えておく（差し込む時に元に戻すため）
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

	void LateUpdate()
	{
		UpdateReloadMagazine();
	}

	/// <summary>
	/// リロード中のマガジンの動き（抜き取る → 左手で持つ → 差し込む）
	/// </summary>
	void UpdateReloadMagazine()
	{
		GunBase gunBase = PlayerCameraManager.SingletonInstance.GetGunFacade.FindGunBase(GunType);
		//アニメーションを巻き戻している間（ショットガンが次の弾を取りにポーチへ戻る間）は、手に何も持たない
		bool isMagazineOut = gunBase.IsReloadTimeActive == true && gunBase.IsReloadAnimationForward == true && magazineOutTime <= gunBase.ReloadAnimationTime && gunBase.ReloadAnimationTime < magazineInTime;
		if (isMagazineOut == true && isMagazineInLeftHand == false)
		{
			TakeOutMagazine();
		}
		else if (isMagazineOut == false && isMagazineInLeftHand == true)
		{
			if (gunBase.IsReloadTimeActive == true)
			{
				PutInMagazine();
			}
			else
			{
				//差し込む前にリロードが止まった（ショットガンのリロード中に撃った）時は、持っていた弾はポーチに戻したことにする
				CancelMagazine();
			}
		}

		MoveMagazineToGun();
	}

	/// <summary>
	/// マガジンを銃から抜き取って左手に持たせる
	/// </summary>
	void TakeOutMagazine()
	{
		isMagazineInLeftHand = true;
		foreach (Transform magazineModel in magazineModels)
		{
			if (magazineLeftHandPos != null)
			{
				//左手に持たせる目印の位置・向きで持たせる
				magazineModel.SetParent(magazineLeftHandPos, false);
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
	/// 左手に持っているマガジンを銃に差し込む（銃の子に戻す。元の位置へはMoveMagazineToGunで少しずつ動かす）
	/// </summary>
	void PutInMagazine()
	{
		isMagazineInLeftHand = false;
		for (int i = 0; i < magazineModels.Length; i++)
		{
			magazineModels[i].SetParent(magazineParents[i], true);
		}
	}

	/// <summary>
	/// 左手に持っているマガジンを、すぐに銃の元の位置・向きに戻す（銃に入っている時は見えない物は隠す）
	/// </summary>
	void CancelMagazine()
	{
		isMagazineInLeftHand = false;
		for (int i = 0; i < magazineModels.Length; i++)
		{
			magazineModels[i].SetParent(magazineParents[i], false);
			magazineModels[i].localPosition = magazineLocalPositions[i];
			magazineModels[i].localRotation = magazineLocalRotations[i];
			if (isMagazineShownOnlyInHand == true)
			{
				magazineModels[i].gameObject.SetActive(false);
			}
		}
	}

	/// <summary>
	/// 差し込んだマガジンを銃の元の位置・向きまで動かす（左手の位置と元の位置のずれを、差し込む動きに見せる）
	/// </summary>
	void MoveMagazineToGun()
	{
		if (isMagazineInLeftHand == true)
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
		float inWeight = GetMagazineGripWeight(reloadAnimationTime, magazineInTime);
		//手に持っている間だけ見せる物（ショットガンの弾）は、銃から抜き取らずにポーチから出てくるので、抜き取る瞬間は合わせない
		if (isMagazineShownOnlyInHand == true)
		{
			return inWeight;
		}

		return Mathf.Max(GetMagazineGripWeight(reloadAnimationTime, magazineOutTime), inWeight);
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
