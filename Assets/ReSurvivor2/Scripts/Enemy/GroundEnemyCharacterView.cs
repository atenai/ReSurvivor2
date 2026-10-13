using UnityEngine;

/// <summary>
/// 地上敵キャラクタービュー
/// MVPパターンのView担当
/// </summary>
public class GroundEnemyCharacterView : MonoBehaviour
{
	[Tooltip("アニメーター")]
	[SerializeField] Animator animator;
	public Animator Animator => animator;

	[Header("銃の持ち替え")]
	[Tooltip("手に持っている銃のモデル（右手でグレネードを投げる間だけ左手に持ち替える。リロードで右手がマガジンを取りに行く間は、左手で握る位置に置く）")]
	[SerializeField] Transform weaponModel;
	[Tooltip("銃を左手で握る位置（左手の骨の子に置いた目印。持ち替えた銃をこの位置・向きに合わせる）")]
	[SerializeField] Transform weaponLeftHandHoldPos;
	[Tooltip("持ち替えた銃を握る位置まで動かす速さ（m/秒）")]
	const float WeaponHoldMoveSpeed = 0.8f;
	[Tooltip("持ち替えた銃を握る向きまで回す速さ（度/秒）")]
	const float WeaponHoldRotateSpeed = 200.0f;
	[Tooltip("銃を右手に持っている時の親（右手の骨）")]
	Transform weaponRightHandParent;
	[Tooltip("銃を右手に持っている時の位置（親から見た位置）")]
	Vector3 weaponRightHandLocalPosition;
	[Tooltip("銃を右手に持っている時の向き（親から見た向き）")]
	Quaternion weaponRightHandLocalRotation;
	[Tooltip("銃を左手に持ち替えているか？")]
	bool isWeaponInLeftHand = false;

	[Header("手のIK")]
	//※IKで使う手の向きは手の骨の向きそのものではなく、Unityのヒューマノイド共通の手の向き（Tポーズで腕の伸びる方向が前）なので、
	//  手のIKの目印の向きはどれもその向きで置いてある（目印を回すと、その分だけ手が回る）
	[Tooltip("アニメーションどおりの右手の位置・向きをIK用の向きで取り出すための目印（右手の骨の子に置いた目印。位置は骨と同じで、向きだけIK用の手の向きにしてある）")]
	[SerializeField] Transform rightHandAnimationPos;
	[Tooltip("射撃中に右手を合わせる目印（胸の骨の子に置いた目印。右手に持っている銃も右手と一緒に動く）")]
	[SerializeField] Transform rightHandFirePos;
	//※OnAnimatorIKが呼ばれる時点では銃はまだIKをかける前の位置にあるため、左手の目印は銃の子ではなく右手の目印の子に置く
	[Tooltip("左手で銃の前側を握る目印（右手の目印の子に置いた目印。目印のローカルの位置・向きが「右手から見た左手の位置・向き」になる）")]
	[SerializeField] Transform leftHandGripPos;
	[Tooltip("手のIKを効かせ始めてから目印にぴったり合うまでの時間（秒）（アニメーターのステートの遷移時間と同じにする）")]
	const float HandIKBlendTime = 0.25f;
	[Tooltip("IKを効かせない時の重み（アニメーションのままの手）")]
	const float NoIKWeight = 0.0f;
	[Tooltip("IKを最大まで効かせる時の重み（目印にぴったり合わせた手）")]
	const float FullIKWeight = 1.0f;
	[Tooltip("アニメーターのベースレイヤーの番号")]
	const int BaseLayerIndex = 0;
	[Tooltip("アニメーターの射撃ステートの名前")]
	const string FireStateName = "Fire";
	[Tooltip("アニメーターの待機・移動ステートの名前")]
	const string IdleAndWalkStateName = "IdleAndWalk";
	[Tooltip("今の射撃中の手のIKの効き具合（0でアニメーションのまま、1で目印にぴったり合わせる）")]
	float fireHandIKWeight = 0.0f;
	[Tooltip("今の待機・移動中の手のIKの効き具合（0でアニメーションのまま、1で銃を体から離して両手でぴったり握る）")]
	float idleHandIKWeight = 0.0f;

	[Header("待機・移動中の腕のめり込み対策")]
	//※待機・移動のアニメーションは体の細いキャラクター用に作られたもので、そのままでは腕と銃が体の装備（ベストや腰のポーチ）にめり込むため、
	//  銃を体の前へ離し、両肘を体の外側へ向ける
	[Tooltip("待機・移動中に、右手（と右手に持った銃・それを握る左手）をアニメーションの位置から動かす量（m）（キャラクターから見た向き。xが右、yが上、zが前）")]
	static readonly Vector3 IdleRightHandOffset = new Vector3(0.0f, 0.0f, 0.12f);
	[Tooltip("待機・移動中に、右肘を向ける位置（アニメーションの右肘の位置からずらす量）（m）（キャラクターから見た向き。xが右、yが上、zが前）")]
	static readonly Vector3 IdleRightElbowHintOffset = new Vector3(0.4f, 0.0f, 0.15f);
	[Tooltip("待機・移動中に、左肘を向ける位置（アニメーションの左肘の位置からずらす量）（m）（キャラクターから見た向き。xが右、yが上、zが前）")]
	static readonly Vector3 IdleLeftElbowHintOffset = new Vector3(-0.4f, 0.0f, 0.15f);
	[Tooltip("右肘の骨")]
	Transform rightElbow;
	[Tooltip("左肘の骨")]
	Transform leftElbow;

	[Header("リロード")]
	//※リロードのアニメーションは、右手でマガジン（ショットガンは弾）を取りに行く間、左手で銃を持っている動きになっているため、
	//  その間は銃を右手から離してアニメーションが想定している位置（左手の前）に置き、左手をIKで銃の前側に合わせて握らせる
	//  （右手の子のままだと、銃が右手と一緒に腰のポーチまで振り回される。アニメーションの左手は握りこぶしのまま銃から離れているため、IKで握らせる）
	//※また、リロードのアニメーションは体の細いキャラクター用に作られたもので、そのままでは銃床がお腹や腰にめり込むため、
	//  リロード中はずっと、銃を左手で握る位置を中心に回して銃床を体の右外へ出し、両手をIKで銃に合わせる
	[Tooltip("リロード中に銃を置く位置（左手の骨の子に置いた目印。リロードアニメーションで両手が銃を持っている瞬間の、左手から見た銃の位置・向き）")]
	[SerializeField] Transform weaponReloadHoldPos;
	[Tooltip("リロード中に左手を合わせる目印（weaponReloadHoldPosの子に置いた目印。銃の前側を左手で握る位置・向き）")]
	[SerializeField] Transform leftHandReloadGripPos;
	[Tooltip("リロード中に銃を左手で持ち始めるタイミング（リロードアニメーションの再生位置。0が始まり、1が終わり）")]
	[SerializeField] float reloadHoldStartTime;
	[Tooltip("リロード中に銃を右手に戻すタイミング（リロードアニメーションの再生位置）。ここでリロード完了になる")]
	[SerializeField] float reloadHoldEndTime;
	[Tooltip("マガジンのモデル（リロード中に右手で抜き取って差し込む。マガジンの無い銃は空のままにする）")]
	[SerializeField] Transform magazineModel;
	[Tooltip("右手でマガジンを抜き取るタイミング（リロードアニメーションの再生位置）")]
	[SerializeField] float magazineOutTime;
	[Tooltip("マガジンを銃に差し込むタイミング（リロードアニメーションの再生位置）")]
	[SerializeField] float magazineInTime;
	[Tooltip("差し込んだマガジンを元の位置まで動かす速さ（m/秒）")]
	const float MagazineMoveSpeed = 0.8f;
	[Tooltip("差し込んだマガジンを元の向きまで回す速さ（度/秒）")]
	const float MagazineRotateSpeed = 360.0f;
	[Tooltip("アニメーターのリロードステートの名前")]
	const string ReloadStateName = "Reload";
	[Tooltip("アニメーターの死亡フラグの名前")]
	const string IsDeadParameterName = "b_isDead";
	[Tooltip("リロード中に、銃を左手で握る位置を中心に回す角度（度）（体の上下の軸まわり。マイナスで銃口が左、銃床が右へ動く）")]
	const float ReloadWeaponYawAngle = -30.0f;
	[Tooltip("今のリロード中の手のIKの効き具合（0でアニメーションのまま、1で銃を回して両手をIKで銃に合わせる）")]
	float reloadIKWeight = 0.0f;
	[Tooltip("今のリロード中の持ち替えの効き具合（0で銃は右手のまま、1で銃をアニメーションの位置に置いて左手で握る）")]
	float reloadHoldWeight = 0.0f;
	[Tooltip("リロード中に銃を置く位置（OnAnimatorIKの時点の、IKをかける前の手から求めた位置）")]
	Vector3 reloadWeaponPosition;
	[Tooltip("リロード中に銃を置く向き（OnAnimatorIKの時点の、IKをかける前の手から求めた向き）")]
	Quaternion reloadWeaponRotation;
	[Tooltip("リロード中に銃を置く位置・向きを求めたフレームの番号（このフレームで求めていなければ、銃を動かさない）")]
	int reloadWeaponFrame = 0;
	[Tooltip("マガジンを銃に付けている時の親")]
	Transform magazineParent;
	[Tooltip("マガジンを銃に付けている時の位置（親から見た位置）")]
	Vector3 magazineLocalPosition;
	[Tooltip("マガジンを銃に付けている時の向き（親から見た向き）")]
	Quaternion magazineLocalRotation;
	[Tooltip("マガジンを右手に持っているか？")]
	bool isMagazineInRightHand = false;

	void Awake()
	{
		InitWeaponModel();
		InitElbow();
		InitMagazine();
	}

	/// <summary>
	/// マガジンを銃に付けている時の親・位置・向きを覚えておく（右手から銃に戻す時に使う）
	/// </summary>
	void InitMagazine()
	{
		if (magazineModel == null)
		{
			return;
		}

		magazineParent = magazineModel.parent;
		magazineLocalPosition = magazineModel.localPosition;
		magazineLocalRotation = magazineModel.localRotation;
	}

	/// <summary>
	/// 肘の骨を取得する（待機・移動中に肘を向ける位置の基準にする）
	/// </summary>
	void InitElbow()
	{
		//GetBoneTransformは、ヒューマノイドの決まった部位（ここでは肘＝前腕の付け根）の骨のTransformを返す
		rightElbow = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
		leftElbow = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
	}

	/// <summary>
	/// 銃を右手に持っている時の親・位置・向きを覚えておく（左手から右手に戻す時に使う）
	/// </summary>
	void InitWeaponModel()
	{
		weaponRightHandParent = weaponModel.parent;
		weaponRightHandLocalPosition = weaponModel.localPosition;
		weaponRightHandLocalRotation = weaponModel.localRotation;
	}

	/// <summary>
	/// アニメーションで骨が動いた後に呼ばれる更新処理
	/// </summary>
	void LateUpdate()
	{
		UpdateReload();
		MoveWeaponToHoldPosition();
		PlaceWeaponForReload();
		MoveMagazineToHoldPosition();
	}

	/// <summary>
	/// リロードアニメーションの再生位置に合わせて、銃を左手で持つ間の効き具合を変えたり、右手でマガジンを抜き差ししたりする
	/// </summary>
	void UpdateReload()
	{
		float reloadTime;
		bool isReload = TryGetReloadTime(out reloadTime);
		bool isDead = animator.GetBool(IsDeadParameterName);

		//右手でマガジン（弾）を取りに行く間は、銃を左手で持つ（一瞬で切り替えると左手がワープして見えるため、少しずつ効かせる）
		float targetWeight = NoIKWeight;
		if (isDead == false && isReload == true && reloadHoldStartTime <= reloadTime && reloadTime < reloadHoldEndTime)
		{
			targetWeight = FullIKWeight;
		}
		//MoveTowardsは、今の値から目標の値へ、第3引数の量だけ近づけた値を返す（目標を通り越さない）
		reloadHoldWeight = Mathf.MoveTowards(reloadHoldWeight, targetWeight, Time.deltaTime / HandIKBlendTime);

		//倒された時はマガジンを持ったまま倒れる（銃に戻すと、倒れ始めた瞬間にマガジンが瞬間移動して見えるため）
		if (isDead == true)
		{
			return;
		}

		UpdateMagazine(isReload, reloadTime);
	}

	/// <summary>
	/// リロード中は、銃をOnAnimatorIKで求めた位置・向き（銃床が体にめり込まないよう回した位置）に置く
	/// （右手で持つ間は右手もIKで同じ位置に合わせているので、右手の子の銃はもともとこの位置にある。左手で持つ間は右手から離して置く）
	/// </summary>
	void PlaceWeaponForReload()
	{
		//画面外でアニメーションが止まっている時など、このフレームで置く位置を求めていない時は動かさない（右手に持ったまま）
		if ((reloadIKWeight <= NoIKWeight && reloadHoldWeight <= NoIKWeight) || isWeaponInLeftHand == true || reloadWeaponFrame != Time.frameCount)
		{
			return;
		}

		weaponModel.position = reloadWeaponPosition;
		weaponModel.rotation = reloadWeaponRotation;
	}

	/// <summary>
	/// 右手でマガジンを抜き取ってから差し込むまでの間、マガジンを右手に持たせる
	/// </summary>
	/// <param name="isReload">リロード中か？</param>
	/// <param name="reloadTime">リロードアニメーションの再生位置</param>
	void UpdateMagazine(bool isReload, float reloadTime)
	{
		if (magazineModel == null)
		{
			return;
		}

		bool isOutTime = isReload == true && magazineOutTime <= reloadTime && reloadTime < magazineInTime;
		if (isOutTime == true && isMagazineInRightHand == false)
		{
			//今の見た目の位置・向きのまま親だけを右手の骨に変える（右手と一緒に動くので、右手で抜き取ったように見える）
			magazineModel.SetParent(weaponRightHandParent, true);
			isMagazineInRightHand = true;
		}
		else if (isOutTime == false && isMagazineInRightHand == true)
		{
			//今の見た目の位置・向きのまま親だけを銃に戻す（元の位置・向きへは、LateUpdateで少しずつ動かす＝差し込む）
			magazineModel.SetParent(magazineParent, true);
			isMagazineInRightHand = false;
		}
	}

	/// <summary>
	/// 銃に戻したマガジンを、元の位置・向きまで少しずつ動かす（差し込んでいるように見せる）
	/// </summary>
	void MoveMagazineToHoldPosition()
	{
		if (magazineModel == null || isMagazineInRightHand == true)
		{
			return;
		}

		//MoveTowards・RotateTowardsは、今の値から目標の値へ、第3引数の量だけ近づけた値を返す（目標を通り越さない）
		magazineModel.localPosition = Vector3.MoveTowards(magazineModel.localPosition, magazineLocalPosition, MagazineMoveSpeed * Time.deltaTime);
		magazineModel.localRotation = Quaternion.RotateTowards(magazineModel.localRotation, magazineLocalRotation, MagazineRotateSpeed * Time.deltaTime);
	}

	/// <summary>
	/// リロードアニメーションの再生位置を取得する（リロードへ遷移している途中も含む）
	/// </summary>
	/// <param name="reloadTime">リロードアニメーションの再生位置（0が始まり、1が終わり）</param>
	/// <returns>リロード中か？</returns>
	bool TryGetReloadTime(out float reloadTime)
	{
		AnimatorStateInfo current = animator.GetCurrentAnimatorStateInfo(BaseLayerIndex);
		if (current.IsName(ReloadStateName) == true)
		{
			reloadTime = current.normalizedTime;
			return true;
		}

		if (animator.IsInTransition(BaseLayerIndex) == true)
		{
			AnimatorStateInfo next = animator.GetNextAnimatorStateInfo(BaseLayerIndex);
			if (next.IsName(ReloadStateName) == true)
			{
				reloadTime = next.normalizedTime;
				return true;
			}
		}

		reloadTime = 0.0f;
		return false;
	}

	/// <summary>
	/// リロードのアニメーションが、銃を右手に戻すところまで再生されたか？（ここでリロード完了）
	/// </summary>
	public bool IsReloadAnimationFinished()
	{
		AnimatorStateInfo current = animator.GetCurrentAnimatorStateInfo(BaseLayerIndex);
		return current.IsName(ReloadStateName) == true && reloadHoldEndTime <= current.normalizedTime;
	}

	/// <summary>
	/// 持ち替えた銃を、持ち替え先の手で握る位置・向きまで少しずつ動かす（一瞬で動かすと銃がワープして見えるため）
	/// </summary>
	void MoveWeaponToHoldPosition()
	{
		//右手で握る時は覚えておいた元の位置・向き、左手で握る時は目印（weaponLeftHandHoldPos）の位置・向きそのもの
		Vector3 targetPosition = weaponRightHandLocalPosition;
		Quaternion targetRotation = weaponRightHandLocalRotation;
		if (isWeaponInLeftHand == true)
		{
			targetPosition = Vector3.zero;
			targetRotation = Quaternion.identity;
		}

		//MoveTowards・RotateTowardsは、今の値から目標の値へ、第3引数の量だけ近づけた値を返す（目標を通り越さない）
		weaponModel.localPosition = Vector3.MoveTowards(weaponModel.localPosition, targetPosition, WeaponHoldMoveSpeed * Time.deltaTime);
		weaponModel.localRotation = Quaternion.RotateTowards(weaponModel.localRotation, targetRotation, WeaponHoldRotateSpeed * Time.deltaTime);
	}

	/// <summary>
	/// IK用のUnity標準の関数（アニメーターのレイヤーの「IK Pass」をオンにすると、アニメーションの計算の後に毎フレーム呼ばれる）
	/// 射撃・待機・移動のアニメーションは別の体型のキャラクター用に作られたもので、そのままでは銃床が顔にめり込んだり、
	/// 左手が銃から離れたり、腕が体の装備にめり込んだりするため、
	/// 射撃中は右手を構える目印に合わせ、待機・移動中は銃を体の前へ離して両肘を外側へ向け、左手はいつも銃の前側を握るようにする
	/// </summary>
	void OnAnimatorIK(int layerIndex)
	{
		fireHandIKWeight = MoveHandIKWeight(fireHandIKWeight, IsAnimatorState(FireStateName));
		idleHandIKWeight = MoveHandIKWeight(idleHandIKWeight, IsAnimatorState(IdleAndWalkStateName));
		reloadIKWeight = MoveHandIKWeight(reloadIKWeight, IsAnimatorState(ReloadStateName));

		//OnAnimatorIKが呼ばれる時点では、骨はまだIKをかける前の今のアニメーションどおりの位置にあるので、
		//右手の骨の子の目印から、アニメーションどおりの右手の位置・向きが分かる
		//待機・移動中は、右手を体の前へ離す（右手の子の銃も一緒に動く。キャラクターの向きに合わせて回してから足す）
		Vector3 idleRightHandPosition = rightHandAnimationPos.position + animator.transform.rotation * IdleRightHandOffset * idleHandIKWeight;
		//射撃中は、右手を構える目印に合わせる
		//Lerp・Slerpは、第1引数と第2引数の間を第3引数の割合（0〜1）で混ぜた位置・向きを返す
		Vector3 rightHandPosition = Vector3.Lerp(idleRightHandPosition, rightHandFirePos.position, fireHandIKWeight);
		Quaternion rightHandRotation = Quaternion.Slerp(rightHandAnimationPos.rotation, rightHandFirePos.rotation, fireHandIKWeight);

		//リロード中に銃を置く位置・向きを求める（右手で持つ間の位置は、ここまでで求めた右手の位置・向きから求める。LateUpdateで銃を置くためにも覚えておく）
		UpdateReloadWeaponPose(rightHandPosition, rightHandRotation);

		//リロード中に右手で銃を持つ間（左手で持つ前と後）は、右手を回した銃のグリップに合わせる
		//（右手の骨から見た銃の位置・向きは決まっているので、銃の位置・向きから右手の骨の位置・向きが逆に求まる。
		//  IK用の手の向きにするため、右手の骨から見たIK用の手の向き＝rightHandAnimationPosのローカルの向きを最後に掛ける）
		Quaternion reloadRightHandBoneRotation = reloadWeaponRotation * Quaternion.Inverse(weaponRightHandLocalRotation);
		Vector3 reloadRightHandPosition = reloadWeaponPosition - reloadRightHandBoneRotation * weaponRightHandLocalPosition;
		Quaternion reloadRightHandRotation = reloadRightHandBoneRotation * rightHandAnimationPos.localRotation;
		//銃を回し始めた瞬間から右手を銃に合わせる（回す角度はリロード中の手のIKの効き具合で少しずつ大きくなるので、右手もなめらかに動く）
		//左手で持つ間は、右手はアニメーションのまま（マガジンを取りに行く）
		float reloadRightHandWeight = NoIKWeight;
		if (NoIKWeight < reloadIKWeight)
		{
			reloadRightHandWeight = FullIKWeight - reloadHoldWeight;
		}
		rightHandPosition = Vector3.Lerp(rightHandPosition, reloadRightHandPosition, reloadRightHandWeight);
		rightHandRotation = Quaternion.Slerp(rightHandRotation, reloadRightHandRotation, reloadRightHandWeight);
		//SetIKPositionWeight・SetIKRotationWeightは、アニメーションの手とIKの手をどれだけ混ぜるか（0でアニメーションのまま、1でIKの手にぴったり）
		//ここでは混ぜ具合を上でLerp・Slerpして決めているので、重みは右手を動かす時は1にする
		float rightHandWeight = NoIKWeight;
		if (NoIKWeight < fireHandIKWeight || NoIKWeight < idleHandIKWeight || NoIKWeight < reloadRightHandWeight)
		{
			rightHandWeight = FullIKWeight;
		}
		animator.SetIKPositionWeight(AvatarIKGoal.RightHand, rightHandWeight);
		animator.SetIKRotationWeight(AvatarIKGoal.RightHand, rightHandWeight);
		animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandPosition);
		animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandRotation);

		//左手は、射撃中も待機・移動中も、右手から見て銃の前側を握る位置・向きに合わせる
		//（左手の目印は右手の目印の子なので、目印のローカルの位置・向きがそのまま「右手から見た左手の位置・向き」になる）
		//こうすると、待機と射撃が切り替わって銃が動いている間も、左手が銃から離れない
		Vector3 gripLeftHandPosition = rightHandPosition + rightHandRotation * leftHandGripPos.localPosition;
		Quaternion gripLeftHandRotation = rightHandRotation * leftHandGripPos.localRotation;
		float gripLeftHandWeight = idleHandIKWeight + fireHandIKWeight;

		//リロード中は、左手をリロード中の銃の前側を握る位置に合わせる
		//（リロード用の左手の目印はweaponReloadHoldPosの子なので、目印のローカルの位置・向きがそのまま「銃から見た左手の位置・向き」になる）
		Vector3 reloadLeftHandPosition = reloadWeaponPosition + reloadWeaponRotation * leftHandReloadGripPos.localPosition;
		Quaternion reloadLeftHandRotation = reloadWeaponRotation * leftHandReloadGripPos.localRotation;
		//待機・射撃とリロードが切り替わる間は、2つの重みの割合で2つの目印の間をなめらかに動かす
		//InverseLerpは、第1引数〜第2引数の範囲の中で第3引数がどの割合の位置にあるか（0〜1）を返す（範囲の幅が0の時は0を返す）
		float reloadRate = Mathf.InverseLerp(NoIKWeight, gripLeftHandWeight + reloadIKWeight, reloadIKWeight);
		Vector3 leftHandPosition = Vector3.Lerp(gripLeftHandPosition, reloadLeftHandPosition, reloadRate);
		Quaternion leftHandRotation = Quaternion.Slerp(gripLeftHandRotation, reloadLeftHandRotation, reloadRate);
		float leftHandWeight = Mathf.Min(gripLeftHandWeight + reloadIKWeight, FullIKWeight);
		animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, leftHandWeight);
		animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, leftHandWeight);
		animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandPosition);
		animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandRotation);

		//待機・移動中は、両肘を体の外側・少し前へ向ける（肘や前腕が体の装備にめり込まないようにする）
		//SetIKHintPositionは、手のIKで腕を曲げる時に肘をどちらへ向けるかの目安の位置（ヒント）を決める（手の位置は変わらない）
		//SetIKHintPositionWeightは、そのヒントをどれだけ効かせるか（0でUnityのおまかせ、1でヒントの方へ向ける）
		animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, idleHandIKWeight);
		animator.SetIKHintPosition(AvatarIKHint.RightElbow, rightElbow.position + animator.transform.rotation * IdleRightElbowHintOffset);
		animator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, idleHandIKWeight);
		animator.SetIKHintPosition(AvatarIKHint.LeftElbow, leftElbow.position + animator.transform.rotation * IdleLeftElbowHintOffset);
	}

	/// <summary>
	/// リロード中に銃を置く位置・向きを求める
	/// 右手で持つ間は右手で持った位置、左手で持つ間はアニメーションが想定している左手の前の位置にし（切り替わる間は持ち替えの効き具合で混ぜる）、
	/// 銃床が体にめり込まないよう、左手で握る位置を中心に体の上下の軸まわりに回す
	/// </summary>
	/// <param name="rightHandGoalPosition">待機・射撃の手のIKを効かせた右手の位置（IK用）</param>
	/// <param name="rightHandGoalRotation">待機・射撃の手のIKを効かせた右手の向き（IK用の手の向き）</param>
	void UpdateReloadWeaponPose(Vector3 rightHandGoalPosition, Quaternion rightHandGoalRotation)
	{
		//IK用の手の向きから右手の骨の向きに戻して（rightHandAnimationPosのローカルの向き＝右手の骨から見たIK用の手の向き）、右手で持った銃の位置・向きを求める
		//（待機中は銃を体の前へ離しているので、IKをかける前のアニメーションどおりの右手ではなく、IKを効かせた右手から求める）
		Quaternion rightHandBoneRotation = rightHandGoalRotation * Quaternion.Inverse(rightHandAnimationPos.localRotation);
		Vector3 rightHeldPosition = rightHandGoalPosition + rightHandBoneRotation * weaponRightHandLocalPosition;
		Quaternion rightHeldRotation = rightHandBoneRotation * weaponRightHandLocalRotation;
		Vector3 position = Vector3.Lerp(rightHeldPosition, weaponReloadHoldPos.position, reloadHoldWeight);
		Quaternion rotation = Quaternion.Slerp(rightHeldRotation, weaponReloadHoldPos.rotation, reloadHoldWeight);

		//左手で握る位置（リロード用の左手の目印はweaponReloadHoldPosの子なので、ローカルの位置がそのまま「銃から見た左手の位置」になる）
		Vector3 pivot = position + rotation * leftHandReloadGripPos.localPosition;
		//AngleAxisは、第2引数の軸まわりに第1引数の角度だけ回す回転を返す
		Quaternion turn = Quaternion.AngleAxis(ReloadWeaponYawAngle * reloadIKWeight, animator.transform.up);
		reloadWeaponPosition = pivot + turn * (position - pivot);
		reloadWeaponRotation = turn * rotation;
		reloadWeaponFrame = Time.frameCount;
	}

	/// <summary>
	/// 手のIKの重みを、ステートに入っている間は少しずつ効かせ、ステートを出たら少しずつ戻す（一瞬で切り替えると手と銃がワープして見えるため）
	/// 銃を左手に持ち替えている間（グレネードを投げる間）はIKを効かせない
	/// </summary>
	/// <param name="currentWeight">今の重み</param>
	/// <param name="isInState">IKを効かせるステートに入っているか？</param>
	/// <returns>次の重み</returns>
	float MoveHandIKWeight(float currentWeight, bool isInState)
	{
		float targetWeight = NoIKWeight;
		if (isInState == true && isWeaponInLeftHand == false)
		{
			targetWeight = FullIKWeight;
		}

		//MoveTowardsは、今の値から目標の値へ、第3引数の量だけ近づけた値を返す（目標を通り越さない）
		return Mathf.MoveTowards(currentWeight, targetWeight, Time.deltaTime / HandIKBlendTime);
	}

	/// <summary>
	/// アニメーターが指定したステートか？（そのステートへ遷移している途中もそのステートとして扱う）
	/// </summary>
	/// <param name="stateName">ステートの名前</param>
	bool IsAnimatorState(string stateName)
	{
		if (animator.IsInTransition(BaseLayerIndex) == true)
		{
			//遷移中は遷移先のステートで判断する（そのステートから別のステートへ遷移している途中は、IKを戻し始める）
			return animator.GetNextAnimatorStateInfo(BaseLayerIndex).IsName(stateName);
		}

		return animator.GetCurrentAnimatorStateInfo(BaseLayerIndex).IsName(stateName);
	}

	/// <summary>
	/// ルートモーションをオフにする（アニメーションで体が動かないようにする。死亡した時だけオンにする）
	/// </summary>
	public void InitRootMotion()
	{
		animator.applyRootMotion = false;
	}

	/// <summary>
	/// アニメーションの初期化処理
	/// </summary>
	public void InitAnimation()
	{
		animator.SetFloat("f_moveSpeed", 0.0f);
		animator.SetBool("b_isReload", false);
		animator.SetBool("b_isRifleFire", false);
		animator.SetBool("b_isGrenadeEquip", false);
		//使われずに残っているトリガーを消す（残っていると次にグレネードを構えた瞬間に投げてしまう）
		animator.ResetTrigger("t_isGrenadeThrow");
	}

	/// <summary>
	/// 銃を左手に持ち替える（右手でグレネードを投げる間、銃が右手と一緒に振り回されないようにする）
	/// </summary>
	public void HoldWeaponInLeftHand()
	{
		if (isWeaponInLeftHand == true)
		{
			return;
		}

		//SetParentの第2引数をtrueにすると、今の見た目の位置・向きのまま親だけを左手の目印に変える
		//（左手で握る位置・向きへは、LateUpdateで少しずつ動かす）
		weaponModel.SetParent(weaponLeftHandHoldPos, true);
		isWeaponInLeftHand = true;
	}

	/// <summary>
	/// 銃を右手に戻す
	/// </summary>
	public void HoldWeaponInRightHand()
	{
		if (isWeaponInLeftHand == false)
		{
			return;
		}

		//今の見た目の位置・向きのまま親だけを右手の骨に戻す（元の握る位置・向きへは、LateUpdateで少しずつ動かす）
		weaponModel.SetParent(weaponRightHandParent, true);
		isWeaponInLeftHand = false;
	}
}
