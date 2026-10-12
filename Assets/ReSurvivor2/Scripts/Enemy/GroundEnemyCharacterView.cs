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
	[Tooltip("手に持っている銃のモデル（右手でグレネードを投げる間だけ左手に持ち替える）")]
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
	[Tooltip("今の待機・移動中の左手のIKの効き具合（0でアニメーションのまま、1で銃をぴったり握る）")]
	float idleHandIKWeight = 0.0f;

	void Awake()
	{
		InitWeaponModel();
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
		MoveWeaponToHoldPosition();
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
	/// 射撃・待機・移動のアニメーションは別の体型のキャラクター用に作られたもので、そのままでは銃床が顔にめり込んだり左手が銃から離れたりするため、
	/// 射撃中は右手を構える目印に合わせ、左手はいつも右手に持った銃の前側を握るようにする
	/// </summary>
	void OnAnimatorIK(int layerIndex)
	{
		fireHandIKWeight = MoveHandIKWeight(fireHandIKWeight, IsAnimatorState(FireStateName));
		idleHandIKWeight = MoveHandIKWeight(idleHandIKWeight, IsAnimatorState(IdleAndWalkStateName));

		//OnAnimatorIKが呼ばれる時点では、骨はまだIKをかける前の今のアニメーションどおりの位置にあるので、
		//右手の骨の子の目印から、アニメーションどおりの右手の位置・向きが分かる
		//右手は射撃中だけ構える目印に合わせる（右手の子の銃も一緒に動く。待機・移動中はアニメーションのまま）
		//Lerp・Slerpは、第1引数と第2引数の間を第3引数の割合（0〜1）で混ぜた位置・向きを返す
		Vector3 rightHandPosition = Vector3.Lerp(rightHandAnimationPos.position, rightHandFirePos.position, fireHandIKWeight);
		Quaternion rightHandRotation = Quaternion.Slerp(rightHandAnimationPos.rotation, rightHandFirePos.rotation, fireHandIKWeight);
		//SetIKPositionWeight・SetIKRotationWeightは、アニメーションの手とIKの手をどれだけ混ぜるか（0でアニメーションのまま、1でIKの手にぴったり）
		//ここでは混ぜ具合を上でLerp・Slerpして決めているので、重みは右手を動かす時は1にする
		float rightHandWeight = NoIKWeight;
		if (NoIKWeight < fireHandIKWeight)
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
		Vector3 leftHandPosition = rightHandPosition + rightHandRotation * leftHandGripPos.localPosition;
		Quaternion leftHandRotation = rightHandRotation * leftHandGripPos.localRotation;
		float leftHandWeight = Mathf.Min(idleHandIKWeight + fireHandIKWeight, FullIKWeight);
		animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, leftHandWeight);
		animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, leftHandWeight);
		animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandPosition);
		animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandRotation);
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
