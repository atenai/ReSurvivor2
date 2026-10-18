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
		groundEnemy.GroundEnemyModel.IsReloadTimeActive = true;
		groundEnemy.AssaultRifleReloadSE();
	}

	// Tick毎に呼ばれる
	public override TaskStatus OnUpdate()
	{
		if (groundEnemy.GroundEnemyModel.IsReloadTimeActive == true)
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
	/// 決まった時間で終わらせるとリロードの動きが途中で切れてしまうため、リロードのアニメーションが
	/// 銃を右手に戻すところまで再生されたら完了にする
	/// </summary>
	void Reload()
	{
		if (groundEnemy.GroundEnemyModel.IsReloadTimeActive == true)//リロードがオンになったら
		{
			if (groundEnemy.GroundEnemyCharacterView.IsReloadAnimationFinished() == true)//リロードのアニメーションが終わったら
			{
				groundEnemy.GroundEnemyModel.CurrentMagazine = groundEnemy.GroundEnemyModel.MagazineCapacity;//弾リセット
				groundEnemy.GroundEnemyModel.IsReloadTimeActive = false;//リロードのオフ

				//リロードアニメーションをオフ
				groundEnemy.GroundEnemyCharacterView.Animator.SetBool("b_isReload", false);
			}
		}
	}
}