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
