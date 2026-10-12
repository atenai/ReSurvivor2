using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// プレイヤーにグレネードを投げるタスク
/// 構える(Grenade_Equip) → 投げる(Grenade_Throw_1) → 元の姿勢(IdleAndWalk)に戻る、までのアニメーションが終わるまで実行し続ける
/// </summary>
[TaskCategory("GroundEnemy")]
public class GroundEnemyGrenadeAction : Action
{
	GroundEnemyPresenter groundEnemy;

	[UnityEngine.Tooltip("グレネード")]
	[SerializeField] GameObject grenadeGameObjectPrefab;
	[UnityEngine.Tooltip("グレネードを構え始めてから投げるまでの時間")]
	float attackTime = 2.0f;
	[UnityEngine.Tooltip("攻撃カウント")]
	float count = 0.0f;
	[UnityEngine.Tooltip("投げるアニメーションを開始したか？")]
	bool isThrow = false;
	[UnityEngine.Tooltip("グレネードを手から離したか？")]
	bool isRelease = false;
	[UnityEngine.Tooltip("投げる前に追跡が終わったので、構えるのをやめたか？")]
	bool isCancel = false;

	[UnityEngine.Tooltip("アニメーターのレイヤー番号（Base Layer）")]
	const int BaseLayer = 0;
	[UnityEngine.Tooltip("待機・歩きのアニメーションのステート名")]
	const string IdleStateName = "IdleAndWalk";
	[UnityEngine.Tooltip("グレネードを構えるアニメーションのステート名")]
	const string EquipStateName = "Grenade_Equip";
	[UnityEngine.Tooltip("グレネードを投げるアニメーションのステート名")]
	const string ThrowStateName = "Grenade_Throw_1";
	[UnityEngine.Tooltip("構えるアニメーションがここまで進んだら構え終わり（1 = 最後まで再生した）")]
	const float EquipEndNormalizedTime = 1.0f;
	[UnityEngine.Tooltip("投げるアニメーションがここまで進んだらグレネードを手から離す（0～1。0.35は腕が頭の真上を通る瞬間。腕の振りが速いので、フレームレートが低くても振り下ろしきる前に離せるよう少し早めにしている）")]
	const float ReleaseNormalizedTime = 0.35f;
	[UnityEngine.Tooltip("グレネードを投げる力（インパルス）")]
	const float ThrowForce = 15.0f;
	[UnityEngine.Tooltip("プレイヤーの足元からどれだけ上を狙うか（少し上を狙うことで落下を補正する）")]
	const float TargetHeight = 1.0f;

	// Taskが処理される直前に呼ばれる
	public override void OnStart()
	{
		groundEnemy = this.GetComponent<GroundEnemyPresenter>();

		groundEnemy.GroundEnemyCharacterView.InitAnimation();
		groundEnemy.GroundEnemyCharacterView.Animator.SetBool("b_isGrenadeEquip", true);
		InitMove();
		InitThrow();
	}

	/// <summary>
	/// 移動の初期化
	/// </summary>
	void InitMove()
	{
		groundEnemy.Rigidbody.velocity = Vector3.zero;
	}

	/// <summary>
	/// 投げる処理の初期化
	/// </summary>
	void InitThrow()
	{
		count = 0.0f;
		isThrow = false;
		isRelease = false;
		isCancel = false;
	}

	// Tick毎に呼ばれる
	public override TaskStatus OnUpdate()
	{
		//まだ投げ始めていない時に追跡が終わったら、構えるのをやめる（投げ始めていたら最後まで投げる）
		if (groundEnemy.IsChase == false && isThrow == false && isCancel == false)
		{
			isCancel = true;
			groundEnemy.GroundEnemyCharacterView.InitAnimation();
		}

		//GetCurrentAnimatorStateInfoは今再生中のステート、GetNextAnimatorStateInfoは切り替え中（ブレンド中）の切り替え先のステートの情報を返す
		AnimatorStateInfo currentState = groundEnemy.GroundEnemyCharacterView.Animator.GetCurrentAnimatorStateInfo(BaseLayer);
		AnimatorStateInfo nextState = groundEnemy.GroundEnemyCharacterView.Animator.GetNextAnimatorStateInfo(BaseLayer);

		//構えるアニメーションに切り替わり始めたら、銃を左手に持ち替える（右手でグレネードを投げるため）
		if (currentState.IsName(EquipStateName) == true || nextState.IsName(EquipStateName) == true)
		{
			groundEnemy.GroundEnemyCharacterView.HoldWeaponInLeftHand();
		}

		//投げるアニメーションが腕を振り下ろす所まで進んだら、グレネードを手から離す
		if (isThrow == true && isRelease == false && currentState.IsName(ThrowStateName) == true && ReleaseNormalizedTime <= currentState.normalizedTime)
		{
			isRelease = true;
			//投げ終わった後に、もう一度構えるアニメーションに戻らないようにする
			groundEnemy.GroundEnemyCharacterView.Animator.SetBool("b_isGrenadeEquip", false);
			GrenadeSpawn();
			groundEnemy.CurrentGrenade = groundEnemy.CurrentGrenade - 1;//現在のグレネード数を-1する
		}

		//投げ終わって（または構えるのをやめて）待機アニメーションに戻りきったら、銃を右手に戻してタスクを終了する
		if ((isRelease == true || isCancel == true) && currentState.IsName(IdleStateName) == true && groundEnemy.GroundEnemyCharacterView.Animator.IsInTransition(BaseLayer) == false)
		{
			groundEnemy.GroundEnemyCharacterView.HoldWeaponInRightHand();
			groundEnemy.GroundEnemyCharacterView.InitAnimation();
			//投擲終了
			return TaskStatus.Success;
		}

		//実行中
		return TaskStatus.Running;
	}

	public override void OnFixedUpdate()
	{
		if (groundEnemy == null)
		{
			return;
		}

		//ゲームクリアーおよびゲームオーバーシーンに遷移した際に必要となる処理
		if (groundEnemy.TargetPlayer == null)
		{
			Debug.Log("<color=red>プレイヤーが破棄されています</color>");
			groundEnemy.IsChase = false;
			return;
		}

		RotateToDirectionTarget();
		Throw();
	}

	/// <summary>
	/// タスクが終了した時に呼ばれる（死亡などで途中で中断された時も呼ばれる）
	/// </summary>
	public override void OnEnd()
	{
		//死亡で中断された時は、銃を左手に持ったまま倒れさせる（右手に戻すと、倒れ始めた瞬間に銃が右手へ瞬間移動して見えるため）
		if (groundEnemy.GetHitPoint().IsDead == true)
		{
			return;
		}

		//それ以外の理由で途中で中断された場合は、銃を右手に戻しておく
		groundEnemy.GroundEnemyCharacterView.HoldWeaponInRightHand();
	}

	/// <summary>
	/// ターゲットの方を向く
	/// </summary>
	void RotateToDirectionTarget()
	{
		if (groundEnemy == null || groundEnemy.TargetPlayer == null)
		{
			return;
		}

		//対象オブジェクトの位置 – 自分のオブジェクトの位置 = 対象オブジェクトの向きベクトルが求められる
		Vector3 direction = groundEnemy.TargetPlayer.transform.position - groundEnemy.transform.position;
		//単純に左右だけを見るようにしたいので、y軸の数値を0にする
		direction.y = 0;
		//第一引数に向きたい方向の向きベクトルを入れてあげる、それによってどのくらい回転させれば良いのか？の数値を求めることができる
		Quaternion lookRotation = Quaternion.LookRotation(direction, Vector3.up);
		//↑で求めたどのくらい回転させれば良いのか？の数値を元に回転させる
		groundEnemy.transform.rotation = Quaternion.Lerp(groundEnemy.transform.rotation, lookRotation, Time.deltaTime * 10f);
	}

	/// <summary>
	/// 投げる（構え終わってから攻撃間隔が経ったら、投げるアニメーションを開始する）
	/// </summary>
	void Throw()
	{
		if (isThrow == true || isCancel == true)
		{
			return;
		}

		count = count + Time.deltaTime;

		//構えるアニメーションを最後まで再生して、グレネードを肩の上まで持ち上げ終わったか？
		AnimatorStateInfo currentState = groundEnemy.GroundEnemyCharacterView.Animator.GetCurrentAnimatorStateInfo(BaseLayer);
		bool isEquipEnd = currentState.IsName(EquipStateName) == true && EquipEndNormalizedTime <= currentState.normalizedTime && groundEnemy.GroundEnemyCharacterView.Animator.IsInTransition(BaseLayer) == false;

		if (attackTime < count && isEquipEnd == true)
		{
			isThrow = true;
			groundEnemy.GroundEnemyCharacterView.Animator.SetTrigger("t_isGrenadeThrow");
		}
	}

	/// <summary>
	/// グレネードを右手の位置に生成して、プレイヤーの方へ投げる
	/// </summary>
	void GrenadeSpawn()
	{
		if (groundEnemy == null || groundEnemy.TargetPlayer == null)
		{
			return;
		}

		//手を離す瞬間の右手の位置から投げる
		Vector3 spawnPosition = groundEnemy.GroundEnemyCharacterView.Animator.GetBoneTransform(HumanBodyBones.RightHand).position;
		GameObject localGameObject = UnityEngine.Object.Instantiate(grenadeGameObjectPrefab, spawnPosition, this.transform.rotation);

		//投げた本人の体のコライダーとはぶつからないようにする
		//（画面外にいる間はアニメーターが骨を動かさないので、右手の位置が体の中のままになっていることがあるため）
		Collider grenadeCollider = localGameObject.GetComponent<Collider>();
		if (grenadeCollider != null)
		{
			Physics.IgnoreCollision(grenadeCollider, groundEnemy.EnemyCollider);
		}

		Rigidbody rb = localGameObject.GetComponent<Rigidbody>();
		if (rb == null)
		{
			return;
		}

		// プレイヤー方向へ投げる
		Vector3 targetPosition = groundEnemy.TargetPlayer.transform.position;
		// 少し上を狙うことで落下を補正
		targetPosition.y += TargetHeight;
		Vector3 direction = (targetPosition - spawnPosition).normalized;

		rb.AddForce(direction * ThrowForce, ForceMode.Impulse);
	}
}
