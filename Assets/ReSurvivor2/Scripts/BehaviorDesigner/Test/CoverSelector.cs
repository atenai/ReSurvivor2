using UnityEngine;

public class CoverSelector : MonoBehaviour
{
	[Tooltip("カバーポイント")]
	[SerializeField] CoverPoint[] coverPoints;

	[Tooltip("敵からの最大探索距離")]
	[SerializeField] float maxDistance = 30f;

	[Tooltip("遮蔽物判定に使うレイヤーマスク")]
	[SerializeField] LayerMask obstructionMask = ~0;

	Vector3 resultCoverPosition;

	[SerializeField] Target target;

	void Awake()
	{
		if (coverPoints == null || coverPoints.Length == 0)
		{
			coverPoints = FindObjectsOfType<CoverPoint>();
		}
		resultCoverPosition = transform.position;
	}

	void Update()
	{
		FindBestCover();
		UpdateTargetPosition();
	}

	// プレイヤー視点から遮られている（＝隠れられる）カバーポイントを探す
	// 見つかれば ResultCoverPosition に代入して true を返す
	bool FindBestCover()
	{
		if (coverPoints == null || coverPoints.Length == 0)
		{
			resultCoverPosition = transform.position;
			return false;
		}

		float bestDist = float.MaxValue;
		CoverPoint best = null;

		foreach (var cp in coverPoints)
		{
			if (cp == null) continue;
			float distToEnemy = Vector3.Distance(transform.position, cp.Position);
			if (distToEnemy > maxDistance) continue;

			Vector3 dir = cp.Position - Player.SingletonInstance.transform.position;
			float dist = dir.magnitude;
			if (dist <= 0.01f) continue;

			RaycastHit hit;
			if (Physics.Raycast(Player.SingletonInstance.transform.position, dir.normalized, out hit, dist, obstructionMask))
			{
				// プレイヤーからカバーポイントへの途中で何かに当たる -> 視線が遮られている
				if (hit.collider != null && hit.collider.gameObject != cp.gameObject)
				{
					if (distToEnemy < bestDist)
					{
						bestDist = distToEnemy;
						best = cp;
					}
				}
			}
		}

		if (best != null)
		{
			resultCoverPosition = best.Position;
			return true;
		}

		// フォールバック: プレイヤーからもっとも遠いカバーポイントを使う
		float bestScore = -1f;
		foreach (var cp in coverPoints)
		{
			if (cp == null) continue;
			float dE = Vector3.Distance(transform.position, cp.Position);
			if (dE > maxDistance) continue;
			float dP = Vector3.Distance(Player.SingletonInstance.transform.position, cp.Position);
			if (dP > bestScore)
			{
				bestScore = dP;
				best = cp;
			}
		}

		if (best != null)
		{
			resultCoverPosition = best.Position;
			return true;
		}

		resultCoverPosition = transform.position;
		return false;
	}

	/// <summary>
	/// 毎フレーム、プレイヤーの位置を NavMeshAgent に通知して経路を再計算させる。
	/// SetDestination は非同期に経路を計算する場合があるため、
	/// 経路が計算中かどうかは FixedUpdate 側で hasPath / pathPending / pathStatus を参照して扱う。
	/// </summary>
	void UpdateTargetPosition()
	{
		// 常にプレイヤーを目的地に設定する（敵 AI などの追従目的）
		target.NavMeshAgent.SetDestination(resultCoverPosition);
	}


	void FixedUpdate()
	{
		MoveTargetPositon();
	}

	/// <summary>
	/// 物理更新毎に Rigidbody.velocity を計算して設定する。
	/// NavMeshAgentが有効で経路を持っている場合は、steeringTargetを使って次の向きを決定する。
	/// 垂直成分（Y）は既存のrb.velocity.yを維持して、ジャンプや重力挙動を壊さない。
	/// 目標速度へはVector3.MoveTowardsによって加速度で滑らかに近づける。
	/// 十分に小さい距離では停止する。
	/// </summary>
	void MoveTargetPositon()
	{
		// 経路を持っていない場合は水平速度を 0 にする（Y は維持）
		if (target.NavMeshAgent.hasPath == false)
		{
			target.Rigidbody.velocity = new Vector3(0, target.Rigidbody.velocity.y, 0);
			return;
		}

		// NavMeshAgent が示す次の操舵目標点（steeringTarget）を取得
		Vector3 steerTarget = target.NavMeshAgent.steeringTarget;

		// 現在位置から操舵目標点へのベクトルを計算（Y は無視して水平のみ扱う）
		Vector3 dir = steerTarget - this.transform.position;
		dir.y = 0;

		// 目標までの水平距離
		float dist = dir.magnitude;

		// 近距離（十分に近い）なら停止させる
		if (dist < 0.1f)
		{
			target.Rigidbody.velocity = new Vector3(0, target.Rigidbody.velocity.y, 0);
			return;
		}

		// 進みたい方向の単位ベクトルに最大速度を掛けて目標速度を決定
		Vector3 desiredVel = dir.normalized * target.NavMeshAgent.speed;

		// 現在の水平速度（Y成分は除く）
		Vector3 horizontalVel = new Vector3(target.Rigidbody.velocity.x, 0, target.Rigidbody.velocity.z);

		// 目標の水平速度成分だけを取り出す
		Vector3 targetHorizontal = new Vector3(desiredVel.x, 0, desiredVel.z);

		// 現在の水平速度を目標の水平速度へ向かって滑らかに変化させる
		// acceleration * Time.fixedDeltaTime がこのフレームで許容する最大の変化量
		Vector3 newHorizontal = Vector3.MoveTowards(horizontalVel, targetHorizontal, target.NavMeshAgent.acceleration * Time.fixedDeltaTime);

		// Y 成分は保持して、Rigidbody の速度を更新する
		target.Rigidbody.velocity = new Vector3(newHorizontal.x, target.Rigidbody.velocity.y, newHorizontal.z);

		// 十分に速度がある場合は向きを滑らかに回転させる（視覚的な向き合わせ）
		if (0.01f < newHorizontal.sqrMagnitude)
		{
			this.transform.rotation = Quaternion.Slerp(this.transform.rotation, Quaternion.LookRotation(newHorizontal.normalized), 10f * Time.fixedDeltaTime);
		}

		// ★これが重要：Agent内部位置を物理位置に同期
		target.NavMeshAgent.nextPosition = target.Rigidbody.position;
	}

	void OnDrawGizmosSelected()
	{
		if (coverPoints != null)
		{
			Gizmos.color = Color.yellow;
			foreach (var cp in coverPoints)
				if (cp != null) Gizmos.DrawSphere(cp.Position, 0.15f);
		}

		Gizmos.color = Color.green;
		Gizmos.DrawSphere(resultCoverPosition, 0.2f);
		Gizmos.DrawLine(transform.position, resultCoverPosition);
	}
}
