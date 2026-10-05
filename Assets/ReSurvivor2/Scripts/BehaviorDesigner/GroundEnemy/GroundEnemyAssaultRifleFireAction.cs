using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

/// <summary>
/// プレイヤーをアサルトライフルで射撃するタスク
/// </summary>
[TaskCategory("GroundEnemy")]
public class GroundEnemyAssaultRifleFireAction : Action
{
	GroundEnemy groundEnemy;
	bool isEnd = false;

	[UnityEngine.Tooltip("射撃間隔")]
	float shootTime = 1.5f;
	[UnityEngine.Tooltip("射撃カウント")]
	float count = 0.0f;
	[UnityEngine.Tooltip("レイの長さ")]
	[SerializeField] float range = 100.0f;
	[UnityEngine.Tooltip("銃のダメージ")]
	[SerializeField] float Damage = 10.0f;
	[UnityEngine.Tooltip("散乱角度")]
	float randomAngle = 7.0f;
	[UnityEngine.Tooltip("連射回数")]
	int rapidFire = 3;
	[UnityEngine.Tooltip("連射回数のカウント")]
	int fireCount = 0;
	[UnityEngine.Tooltip("弾の速度")]
	[SerializeField] float bulletSpeed = 120.0f;

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
			//追跡終了
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
		for (int i = 0; i < rapidFire; i++)
		{
			count = count + Time.deltaTime;
			if (shootTime < count)
			{
				count = 0.0f;
				Fire();
				groundEnemy.CurrentMagazine = groundEnemy.CurrentMagazine - 1;//現在のマガジンの弾数を-1する

				fireCount++;
				if (rapidFire <= fireCount)
				{
					fireCount = 0;
					isEnd = true;
					return;
				}
			}
		}
	}

	/// <summary>
	/// 弾を発射
	/// </summary> 
	void Fire()
	{
		groundEnemy.AssaultRifleBulletCasingSE();
		groundEnemy.MuzzleFlashAndShell();
		groundEnemy.AfterFireSmoke();
		groundEnemy.AssaultRifleFireSE();

		//銃口から弾を撃つ（当たり判定・ダメージ・着弾エフェクト・弾道エフェクトは EnemyBullet が行う）
		groundEnemy.FireBullet(randomAngle, range, Damage, bulletSpeed);
	}
}