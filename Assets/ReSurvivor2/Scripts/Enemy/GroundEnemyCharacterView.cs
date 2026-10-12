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

	[Header("射撃中の手のIK")]
	[Tooltip("射撃中に右手を合わせる目印（胸の骨の子に置いた目印。右手に持っている銃も右手と一緒に動く）")]
	[SerializeField] Transform rightHandFirePos;
	//※OnAnimatorIKが呼ばれる時点では銃はまだIKをかける前の位置にあるため、左手の目印は銃の子ではなく右手の目印の子に置く
	[Tooltip("射撃中に左手を合わせる目印（右手の目印の子に置いた目印。右手と一緒に動いた銃の前側を左手で握る位置・向き）")]
	[SerializeField] Transform leftHandFirePos;
	[Tooltip("手のIKを効かせ始めてから目印にぴったり合うまでの時間（秒）（アニメーターの射撃ステートへの遷移時間と同じにする）")]
	const float FireHandIKBlendTime = 0.25f;
	[Tooltip("IKを効かせない時の重み（アニメーションのままの手）")]
	const float NoIKWeight = 0.0f;
	[Tooltip("IKを最大まで効かせる時の重み（目印にぴったり合わせた手）")]
	const float FullIKWeight = 1.0f;
	[Tooltip("アニメーターのベースレイヤーの番号")]
	const int BaseLayerIndex = 0;
	[Tooltip("アニメーターの射撃ステートの名前")]
	const string FireStateName = "Fire";
	[Tooltip("今の手のIKの効き具合（0でアニメーションのまま、1で目印にぴったり合わせる）")]
	float fireHandIKWeight = 0.0f;

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
	/// 射撃アニメーションは別の体型のキャラクター用に作られたもので、そのままでは銃床が顔にめり込んだり左手が銃から離れたりするため、
	/// 射撃中だけ両手を目印の位置・向きに合わせて、銃を肩に当ててしっかり構えた形にする
	/// </summary>
	void OnAnimatorIK(int layerIndex)
	{
		UpdateFireHandIKWeight();

		//SetIKPositionWeight・SetIKRotationWeightは、アニメーションの手と目印の手をどれだけ混ぜるか（0でアニメーションのまま、1で目印にぴったり）
		//※SetIKRotationで渡す向きは手の骨の向きそのものではなく、Unityのヒューマノイド共通の手の向き（Tポーズで腕の伸びる方向が前）なので、
		//  目印の向きもその向きで置いてある（目印を回すと、その分だけ手が回る）
		//右手を構える位置に合わせる（右手の子の銃も一緒に動く）
		animator.SetIKPositionWeight(AvatarIKGoal.RightHand, fireHandIKWeight);
		animator.SetIKRotationWeight(AvatarIKGoal.RightHand, fireHandIKWeight);
		animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandFirePos.position);
		animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandFirePos.rotation);

		//左手で銃の前側を握る
		animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, fireHandIKWeight);
		animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, fireHandIKWeight);
		animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandFirePos.position);
		animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandFirePos.rotation);
	}

	/// <summary>
	/// 射撃中は手のIKを少しずつ効かせ、射撃が終わったら少しずつ戻す（一瞬で切り替えると手と銃がワープして見えるため）
	/// </summary>
	void UpdateFireHandIKWeight()
	{
		float targetWeight = NoIKWeight;
		if (IsFireState() == true && isWeaponInLeftHand == false)
		{
			targetWeight = FullIKWeight;
		}

		//MoveTowardsは、今の値から目標の値へ、第3引数の量だけ近づけた値を返す（目標を通り越さない）
		fireHandIKWeight = Mathf.MoveTowards(fireHandIKWeight, targetWeight, Time.deltaTime / FireHandIKBlendTime);
	}

	/// <summary>
	/// 射撃ステートか？（射撃ステートへ遷移している途中も射撃ステートとして扱う）
	/// </summary>
	bool IsFireState()
	{
		if (animator.IsInTransition(BaseLayerIndex) == true)
		{
			//遷移中は遷移先のステートで判断する（射撃から別のステートへ遷移している途中は、IKを戻し始める）
			return animator.GetNextAnimatorStateInfo(BaseLayerIndex).IsName(FireStateName);
		}

		return animator.GetCurrentAnimatorStateInfo(BaseLayerIndex).IsName(FireStateName);
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
