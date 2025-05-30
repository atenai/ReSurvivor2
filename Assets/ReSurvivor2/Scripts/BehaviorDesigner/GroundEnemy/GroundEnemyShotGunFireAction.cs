using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

/// <summary>
/// プレイヤーをショットガンで射撃するタスク
/// </summary>
[TaskCategory("GroundEnemy")]
public class GroundEnemyShotGunFireAction : Action
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
	float randomAngle = 15.0f;
	[UnityEngine.Tooltip("ショットガンが一度で出る弾の数")]
	[SerializeField] int shotGunBullet = 10;

	// Taskが処理される直前に呼ばれる
	public override void OnStart()
	{
		groundEnemy = this.GetComponent<GroundEnemy>();

		InitAnimation();
		InitMove();
	}

	/// <summary>
	/// アニメーションの初期化処理
	/// </summary>
	void InitAnimation()
	{
		groundEnemy.Animator.SetFloat("f_moveSpeed", 0.0f);
		groundEnemy.Animator.SetBool("b_isReload", false);
		groundEnemy.Animator.SetBool("b_isRifleAim", true);
		groundEnemy.Animator.SetBool("b_isRifleFire", false);
		groundEnemy.Animator.SetBool("b_isGrenadeEquip", false);
		groundEnemy.Animator.SetBool("b_isGrenadeThrow", false);
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
			groundEnemy.Animator.SetFloat("f_moveSpeed", 0.0f);
			groundEnemy.Animator.SetBool("b_isReload", false);
			groundEnemy.Animator.SetBool("b_isRifleAim", false);
			groundEnemy.Animator.SetBool("b_isRifleFire", false);
			groundEnemy.Animator.SetBool("b_isGrenadeEquip", false);
			groundEnemy.Animator.SetBool("b_isGrenadeThrow", false);
			//射撃終了
			return TaskStatus.Success;
		}

		if (groundEnemy.IsChase == false)
		{
			groundEnemy.Animator.SetFloat("f_moveSpeed", 0.0f);
			groundEnemy.Animator.SetBool("b_isReload", false);
			groundEnemy.Animator.SetBool("b_isRifleAim", false);
			groundEnemy.Animator.SetBool("b_isRifleFire", false);
			groundEnemy.Animator.SetBool("b_isGrenadeEquip", false);
			groundEnemy.Animator.SetBool("b_isGrenadeThrow", false);
			//追跡終了
			return TaskStatus.Success;
		}

		//実行中
		return TaskStatus.Running;
	}

	public override void OnFixedUpdate()
	{
		RotateToDirectionTarget();
		Shot();
	}

	/// <summary>
	/// ターゲットの方を向く
	/// </summary> 
	void RotateToDirectionTarget()
	{
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
		groundEnemy.Animator.SetBool("b_isRifleFire", false);

		count = count + Time.deltaTime;
		if (shootTime < count)
		{
			count = 0.0f;

			groundEnemy.Animator.SetBool("b_isRifleFire", true);
			Fire();
			groundEnemy.CurrentMagazine = groundEnemy.CurrentMagazine - 1;//現在のマガジンの弾数を-1する

			isEnd = true;
			return;
		}
	}

	/// <summary>
	/// 弾を発射
	/// </summary> 
	void Fire()
	{
		groundEnemy.BulletCasingSE();
		groundEnemy.MuzzleFlashAndShell();
		groundEnemy.AfterFireSmoke();

		groundEnemy.FireSE();

		for (int i = 0; i < shotGunBullet; i++)
		{
			Vector3 direction = this.transform.forward;
			direction = Quaternion.AngleAxis(Random.Range(-randomAngle, randomAngle), this.transform.up) * direction;
			direction = Quaternion.AngleAxis(Random.Range(-randomAngle, randomAngle), this.transform.right) * direction;

			Vector3 pos = new Vector3(this.transform.position.x, this.transform.position.y + 1f, this.transform.position.z);
			Ray ray = new Ray(pos, direction);
			Debug.DrawRay(ray.origin, ray.direction * range, Color.red, 10.0f);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, range) == true)//もしRayを投射して何らかのコライダーに衝突したら
			{
				if (hit.collider.gameObject.CompareTag("Player"))//※間違ってオブジェクトの設定にレイヤーとタグを間違えるなよおれｗ
				{
					//ダメージ
					Player player = hit.transform.GetComponent<Player>();
					if (player != null)
					{
						player.TakeDamage(Damage);
						groundEnemy.Shaker();
						//敵マーカー表示
						IndicatorManager.SingletonInstance.ShowIndicator(groundEnemy);
					}
				}
			}
		}
	}
}