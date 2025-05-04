using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

/// <summary>
/// プレイヤーの方向を向くタスク
/// </summary>
[TaskCategory("FlyingEnemy")]
public class FlyingEnemyRotateToDirectionPlayerAction : Action
{
	FlyingEnemy flyingEnemy;
	float t = 0.0f;
	Quaternion lookRotation;

	// Taskが処理される直前に呼ばれる
	public override void OnStart()
	{
		flyingEnemy = this.GetComponent<FlyingEnemy>();
		//Debug.Log("<color=blue>OnStart</color>");
		t = 0.0f;
		//対象オブジェクトの位置 – 自分のオブジェクトの位置 = 対象オブジェクトの向きベクトルが求められる
		Vector3 direction = flyingEnemy.TargetPlayer.transform.position - this.transform.position;
		//単純に左右だけを見るようにしたいので、y軸の数値を0にする
		direction.y = 0;
		//2点間の角度を求める
		//第一引数に向きたい方向の向きベクトルを入れてあげる、それによってどのくらい回転させれば良いのか？の数値を求めることができる
		lookRotation = Quaternion.LookRotation(direction, Vector3.up);
	}

	// 更新時に呼ばれる
	public override TaskStatus OnUpdate()
	{
		//Debug.Log("<color=red>回転処理</color>");

		//Time.deltaTimeは0.1f
		t = t + Time.deltaTime;
		//Debug.Log("<color=green>t :" + t + "</color>");
		//↑で求めたどのくらい回転させれば良いのか？の数値を元に回転させる(Lerpのtは0～1)
		this.transform.rotation = Quaternion.Lerp(this.transform.rotation, lookRotation, t);

		const float endRot = 1.0f;
		if (endRot <= t)
		{
			flyingEnemy.IsRotateToDirectionPlayer = false;
			return TaskStatus.Success;
		}

		return TaskStatus.Running;
	}
}