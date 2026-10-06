using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

/// <summary>
/// プレイヤーをハンドガンで射撃するタスク
/// </summary>
[TaskCategory("GroundEnemy")]
public class GroundEnemyHandGunFireAction : Action
{
	GroundEnemy groundEnemy;
	bool isEnd = false;

	[UnityEngine.Tooltip("射撃間隔")]
	[SerializeField] float shootTime = 2.0f;
	[UnityEngine.Tooltip("射撃カウント")]
	float count = 0.0f;
	[UnityEngine.Tooltip("レイの長さ")]
	[SerializeField] float range = 100.0f;
	[UnityEngine.Tooltip("銃のダメージ")]
	[SerializeField] float Damage = 10.0f;
	[UnityEngine.Tooltip("弾の速度")]
	[SerializeField] float bulletSpeed = 110.0f;

	// Taskが処理される直前に呼ばれる
	public override void OnStart()
	{
		groundEnemy = this.GetComponent<GroundEnemy>();

		groundEnemy.InitAnimation();
		groundEnemy.Animator.SetBool("b_isRifleFire", true);
		InitMove();
	}

	/// <summary>
	/// 移動の初期化
	/// </summary> 
	void InitMove()
	{
		groundEnemy.Rigidbody.velocity = Vector3.zero;
		isEnd = false;
	}

	// Tick毎に呼ばれる
	public override TaskStatus OnUpdate()
	{
		if (isEnd == true)
		{
			isEnd = false;
			groundEnemy.InitAnimation();
			//射撃終了
			return TaskStatus.Success;
		}

		if (groundEnemy.IsChase == false)
		{
			groundEnemy.InitAnimation();
			return TaskStatus.Success;

		}

		return TaskStatus.Running;
	}

	public override void OnFixedUpdate()
	{
		if (groundEnemy == null)
		{
			return;
		}

		// TargetPlayer が破棄されている可能性があるためチェックする
		if (groundEnemy.TargetPlayer == null)
		{
			groundEnemy.IsChase = false;
			return;
		}

		RotateToDirectionTarget();
		Shot();
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

	void Shot()
	{
		count = count + Time.deltaTime;
		if (shootTime < count)
		{
			count = 0.0f;
			HandGunFire();
			groundEnemy.CurrentMagazine = groundEnemy.CurrentMagazine - 1;//現在のマガジンの弾数を-1する
			isEnd = true;
			return;
		}
	}

	/// <summary>
	/// 弾を発射
	/// </summary> 
	void HandGunFire()
	{
		//銃口から弾を撃つ（当たり判定・ダメージ・着弾エフェクト・弾道エフェクトは EnemyBullet が行う）
		groundEnemy.FireBullet(0.0f, range, Damage, bulletSpeed);
	}
}