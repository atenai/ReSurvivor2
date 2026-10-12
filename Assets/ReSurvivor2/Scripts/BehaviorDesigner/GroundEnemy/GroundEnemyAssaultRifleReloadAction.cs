using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

/// <summary>
/// リロードするタスク
/// </summary>
[TaskCategory("GroundEnemy")]
public class GroundEnemyAssaultRifleReloadAction : Action
{
	GroundEnemyPresenter groundEnemy;

	// Taskが処理される直前に呼ばれる
	public override void OnStart()
	{
		groundEnemy = this.GetComponent<GroundEnemyPresenter>();

		groundEnemy.GroundEnemyCharacterView.InitAnimation();
		groundEnemy.GroundEnemyCharacterView.Animator.SetBool("b_isReload", true);
		InitMove();
		InitReload();
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
				groundEnemy.GroundEnemyCharacterView.Animator.SetBool("b_isReload", false);
			}
		}
	}
}