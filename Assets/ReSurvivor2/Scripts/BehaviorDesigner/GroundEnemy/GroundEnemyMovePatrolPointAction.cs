using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

/// <summary>
/// 指定座標をパトロールするタスク
/// </summary>
[TaskCategory("GroundEnemy")]
public class GroundEnemyMovePatrolPointAction : Action
{
	GroundEnemy groundEnemy;
	[UnityEngine.Tooltip("移動終了フラグ")]
	bool isEnd = false;
	[UnityEngine.Tooltip("エネミーが止まってほしい座標位置の範囲")]
	float endPos = 0.5f;

	//パトロールポイントの座標
	Vector3 patrolPointPos;

	//前進できない際の処理
	Vector3 startPos;
	float oldDistance = 0.0f;
	float endCount = 0.0f;
	[UnityEngine.Tooltip("前進できない際の強制終了時間")]
	float endTime = 1.0f;

	// Taskが処理される直前に呼ばれる
	public override void OnStart()
	{
		groundEnemy = this.GetComponent<GroundEnemy>();

		InitAnimation();
		InitMove();
		InitEnemyCanNotMove();
		SetPatrolPoint();
		AdjustNavMeshPosition();
	}

	/// <summary>
	/// アニメーションの初期化処理
	/// </summary>
	void InitAnimation()
	{
		groundEnemy.Animator.SetFloat("f_moveSpeed", 0.5f);
		groundEnemy.Animator.SetBool("b_isReload", false);
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
		isEnd = false;
	}

	/// <summary>
	/// パトロールポイントをセットする
	/// </summary>
	void SetPatrolPoint()
	{
		//巡回地点が設定されていなければ
		if (groundEnemy.PatrolPoints.Count == 0)
		{
			//Debug.Log("<color=red>パトロールポイントが無い</color>");
			return;
		}

		//現在選択されている配列の座標を巡回地点の座標に代入
		patrolPointPos = groundEnemy.PatrolPoints[groundEnemy.PatrolPointNumber].transform.position;

		UpdateTargetPosition();
	}

	/// <summary>
	/// エネミーの移動距離が全く変わらない場合の強制終了カウントの初期化処理
	/// </summary>
	void InitEnemyCanNotMove()
	{
		startPos = groundEnemy.transform.position;
		endCount = 0.0f;
		oldDistance = 0.0f;
	}

	// Tick毎に呼ばれる
	public override TaskStatus OnUpdate()
	{
		if (groundEnemy.IsChase == true)
		{
			//プレイヤーを発見した
			return TaskStatus.Success;
		}

		if (isEnd == true)
		{
			isEnd = false;
			Debug.Log("<color=green>パトロール移動の終了</color>");
			groundEnemy.Rigidbody.velocity = Vector3.zero;
			NextPatrolPoint();
			//目的地にたどりついた
			return TaskStatus.Success;
		}

		UpdateTargetPosition();
		//移動実行中
		return TaskStatus.Running;
	}

	/// <summary>
	/// 次のパトロールポイントを出す
	/// </summary>
	void NextPatrolPoint()
	{
		Debug.Log("<color=orange>今のパトロールポイント " + groundEnemy.PatrolPointNumber + "</color>");
		//配列の中から次の巡回地点を選択（必要に応じて繰り返し）
		groundEnemy.PatrolPointNumber = (groundEnemy.PatrolPointNumber + 1) % groundEnemy.PatrolPoints.Count;
		Debug.Log("<color=yellow>次のパトロールポイント " + groundEnemy.PatrolPointNumber + "</color>");
	}

	/// <summary>
	/// 毎フレーム、プレイヤーの位置を NavMeshAgent に通知して経路を再計算させる。
	/// SetDestination は非同期に経路を計算する場合があるため、
	/// 経路が計算中かどうかは FixedUpdate 側で hasPath / pathPending / pathStatus を参照して扱う。
	/// </summary>
	void UpdateTargetPosition()
	{
		// 常にプレイヤーを目的地に設定する（敵 AI などの追従目的）
		groundEnemy.NavMeshAgent.SetDestination(patrolPointPos);
	}

	public override void OnFixedUpdate()
	{
		float currentDistance = Vector3.Distance(startPos, groundEnemy.transform.position);
		//エネミーの移動距離が全く変わらない場合
		if (oldDistance == currentDistance)
		{
			//強制終了用のカウントを足す
			endCount = endCount + Time.deltaTime;
			//Debug.Log("<color=blue>endCount " + endCount + "</color>");
		}

		//エネミーがその場から前に進めず距離計算ができない場合に、秒数で移動アクションを終了させる処理
		if (endTime <= endCount)
		{
			isEnd = true;
		}

		MovePatrolPoint();

		//エネミーの移動距離が全く変わらない場合の強制終了カウント用の距離計算処理
		oldDistance = currentDistance;
	}

	/// <summary>
	/// パトロールポイントに移動する
	/// </summary>
	void MovePatrolPoint()
	{
		float sqrCurrentDistance = Vector3.SqrMagnitude(patrolPointPos - groundEnemy.transform.position);
		//Debug.Log("<color=red>sqrCurrentDistance: " + sqrCurrentDistance + "</color>");
		if (sqrCurrentDistance < endPos)
		{
			isEnd = true;
			return;
		}

		AdjustNavMeshPosition();
		MoveTargetPositon();
	}

	/// <summary>
	///  ナビメッシュの位置とエネミーの位置が0.5m以上ズレたら補正
	/// </summary>
	void AdjustNavMeshPosition()
	{
		float sqr = (groundEnemy.NavMeshAgent.nextPosition - groundEnemy.Rigidbody.position).sqrMagnitude;
		if (0.25f < sqr)
		{
			groundEnemy.NavMeshAgent.nextPosition = groundEnemy.Rigidbody.position;
		}
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
		if (groundEnemy.NavMeshAgent.hasPath == false)
		{
			groundEnemy.Rigidbody.velocity = new Vector3(0, groundEnemy.Rigidbody.velocity.y, 0);
			return;
		}

		// NavMeshAgent が示す次の操舵目標点（steeringTarget）を取得
		Vector3 steerTarget = groundEnemy.NavMeshAgent.steeringTarget;

		// 現在位置から操舵目標点へのベクトルを計算（Y は無視して水平のみ扱う）
		Vector3 dir = steerTarget - groundEnemy.transform.position;
		dir.y = 0;

		// 進みたい方向の単位ベクトルに最大速度を掛けて目標速度を決定
		Vector3 desiredVel = dir.normalized * groundEnemy.NavMeshAgent.speed;

		// 現在の水平速度（Y成分は除く）
		Vector3 horizontalVel = new Vector3(groundEnemy.Rigidbody.velocity.x, 0, groundEnemy.Rigidbody.velocity.z);

		// 目標の水平速度成分だけを取り出す
		Vector3 targetHorizontal = new Vector3(desiredVel.x, 0, desiredVel.z);

		// 現在の水平速度を目標の水平速度へ向かって滑らかに変化させる
		// acceleration * Time.fixedDeltaTime がこのフレームで許容する最大の変化量
		Vector3 newHorizontal = Vector3.MoveTowards(horizontalVel, targetHorizontal, groundEnemy.NavMeshAgent.acceleration * Time.fixedDeltaTime);

		// Y 成分は保持して、Rigidbody の速度を更新する
		groundEnemy.Rigidbody.velocity = new Vector3(newHorizontal.x, groundEnemy.Rigidbody.velocity.y, newHorizontal.z);

		// 十分に速度がある場合は向きを滑らかに回転させる（視覚的な向き合わせ）
		if (0.01f < newHorizontal.sqrMagnitude)
		{
			groundEnemy.transform.rotation = Quaternion.Slerp(groundEnemy.transform.rotation, Quaternion.LookRotation(newHorizontal.normalized), 10f * Time.fixedDeltaTime);
		}

		// ★これが重要：Agent内部位置を物理位置に同期
		groundEnemy.NavMeshAgent.nextPosition = groundEnemy.Rigidbody.position;
	}
}