using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// NavMesh による経路情報を参照して、Rigidbody.velocity を直接操作して移動するコンポーネント。
/// NavMeshAgentは経路計算（SetDestination 等）にのみ利用し、実際の位置更新は行わない。
/// Rigidbodyの水平速度を操作して物理挙動と衝突挙動を維持する。
/// </summary>
public class NavMeshRigidbodyMover : MonoBehaviour
{
	// 経路計算用の NavMeshAgent（位置・回転の自動更新はオフにする）
	[SerializeField] NavMeshAgent navMeshAgent;

	// 移動に使う Rigidbody（Y方向の速度は保持するため直接操作する）
	[SerializeField] Rigidbody rb;

	// 目標速度（水平）
	[SerializeField] float speed = 3.5f;

	// 水平速度の変化を滑らかにするための加速度（MoveTowards の最大変化量）
	[SerializeField] float acceleration = 10f;

	/// <summary>
	/// Awake で NavMeshAgent の自動位置・回転更新をオフにする。
	/// これにより NavMeshAgent は経路計算（path, steeringTarget 等）専用になり、
	/// 実際の移動は Rigidbody.velocity によって行う。
	/// </summary>
	void Awake()
	{
		// NavMeshAgent による Transform 更新を無効化
		navMeshAgent.updatePosition = false;
		navMeshAgent.updateRotation = false;
	}

	/// <summary>
	/// 毎フレーム、プレイヤーの位置を NavMeshAgent に通知して経路を再計算させる。
	/// SetDestination は非同期に経路を計算する場合があるため、
	/// 経路が計算中かどうかは FixedUpdate 側で hasPath / pathPending / pathStatus を参照して扱う。
	/// </summary>
	void Update()
	{
		// 常にプレイヤーを目的地に設定する（敵 AI などの追従目的）
		navMeshAgent.SetDestination(Player.SingletonInstance.transform.position);
	}

	/// <summary>
	/// 物理更新毎に Rigidbody.velocity を計算して設定する。
	/// NavMeshAgentが有効で経路を持っている場合は、steeringTargetを使って次の向きを決定する。
	/// 垂直成分（Y）は既存のrb.velocity.yを維持して、ジャンプや重力挙動を壊さない。
	/// 目標速度へはVector3.MoveTowardsによって加速度で滑らかに近づける。
	/// 十分に小さい距離では停止する。
	/// </summary>
	void FixedUpdate()
	{
		// 経路を持っていない場合は水平速度を 0 にする（Y は維持）
		if (navMeshAgent.hasPath == false)
		{
			rb.velocity = new Vector3(0, rb.velocity.y, 0);
			return;
		}

		// NavMeshAgent が示す次の操舵目標点（steeringTarget）を取得
		Vector3 steerTarget = navMeshAgent.steeringTarget;

		// 現在位置から操舵目標点へのベクトルを計算（Y は無視して水平のみ扱う）
		Vector3 dir = steerTarget - this.transform.position;
		dir.y = 0;

		// 目標までの水平距離
		float dist = dir.magnitude;

		// 近距離（十分に近い）なら停止させる
		if (dist < 0.1f)
		{
			rb.velocity = new Vector3(0, rb.velocity.y, 0);
			return;
		}

		// 進みたい方向の単位ベクトルに最大速度を掛けて目標速度を決定
		Vector3 desiredVel = dir.normalized * speed;

		// 現在の水平速度（Y成分は除く）
		Vector3 horizontalVel = new Vector3(rb.velocity.x, 0, rb.velocity.z);

		// 目標の水平速度成分だけを取り出す
		Vector3 targetHorizontal = new Vector3(desiredVel.x, 0, desiredVel.z);

		// 現在の水平速度を目標の水平速度へ向かって滑らかに変化させる
		// acceleration * Time.fixedDeltaTime がこのフレームで許容する最大の変化量
		Vector3 newHorizontal = Vector3.MoveTowards(horizontalVel, targetHorizontal, acceleration * Time.fixedDeltaTime);

		// Y 成分は保持して、Rigidbody の速度を更新する
		rb.velocity = new Vector3(newHorizontal.x, rb.velocity.y, newHorizontal.z);

		// 十分に速度がある場合は向きを滑らかに回転させる（視覚的な向き合わせ）
		if (0.01f < newHorizontal.sqrMagnitude)
		{
			this.transform.rotation = Quaternion.Slerp(this.transform.rotation, Quaternion.LookRotation(newHorizontal.normalized), 10f * Time.fixedDeltaTime);
		}
	}

	/// <summary>
	/// コンポーネントやオブジェクトが無効化されたら速度をリセットする。
	/// これにより停止状態で無効化され、再有効化時に不意な移動が発生しない。
	/// </summary>
	void OnDisable()
	{
		rb.velocity = Vector3.zero;
	}
}
