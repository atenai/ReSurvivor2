using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

/// <summary>
/// リロードするタスク
/// </summary>
[TaskCategory("GroundEnemy")]
public class GroundEnemyReloadAction : Action
{
	GroundEnemy groundEnemy;

	// Taskが処理される直前に呼ばれる
	public override void OnStart()
	{
		groundEnemy = this.GetComponent<GroundEnemy>();

		InitAnimation();
		InitMove();
		InitReload();
	}

	/// <summary>
	/// アニメーションの初期化処理
	/// </summary>
	void InitAnimation()
	{
		groundEnemy.Animator.SetFloat("f_moveSpeed", 0.0f);
		groundEnemy.Animator.SetBool("b_isReload", true);
		groundEnemy.Animator.SetBool("b_isRifleAim", false);
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
	}

	/// <summary>
	/// リロードの初期化
	/// </summary>
	void InitReload()
	{
		groundEnemy.IsReloadTimeActive = true;

		groundEnemy.AssaultRifleReloadSE();
		//groundEnemy.ShotGunReloadSE();
	}

	// Tick毎に呼ばれる
	public override TaskStatus OnUpdate()
	{
		if (groundEnemy.IsReloadTimeActive == true)
		{
			//リロード実行中
			return TaskStatus.Running;
		}

		//リロード終了
		return TaskStatus.Success;
	}

	public override void OnFixedUpdate()
	{
		Reload();
	}

	/// <summary>
	/// リロード
	/// </summary>
	void Reload()
	{
		if (groundEnemy.IsReloadTimeActive == true)//リロードがオンになったら
		{
			groundEnemy.ReloadTime += Time.deltaTime;//リロードタイムをプラス

			if (groundEnemy.ReloadTimeDefine <= groundEnemy.ReloadTime)//リロードタイムが10以上になったら
			{
				//弾リセット
				groundEnemy.CurrentMagazine = groundEnemy.MagazineCapacity;

				groundEnemy.ReloadTime = 0.0f;//リロードタイムをリセット
				groundEnemy.IsReloadTimeActive = false;//リロードのオフ

				//リロードアニメーションをオフ
				groundEnemy.Animator.SetBool("b_isReload", false);
			}
		}
	}
}