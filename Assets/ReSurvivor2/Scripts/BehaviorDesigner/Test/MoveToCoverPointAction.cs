using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 指定されたカバーポジションへ移動するタスク
/// </summary>
[TaskCategory("Enemy/Cover")]
public class MoveToCoverPointAction : Action
{
	Target target;
	[UnityEngine.Tooltip("移動終了フラグ")]
	bool isEnd = false;
	/// <summary>
	/// 移動速度
	/// </summary>
	float speed = 3.0f;
	/// <summary>
	/// 水平速度の変化を滑らかにするための加速度（MoveTowards の最大変化量） 
	/// </summary>
	float acceleration = 10f;

	//ターゲット座標位置の変数
	Vector3 targetPos;

#if UNITY_EDITOR
	[SerializeField] GameObject obj;//プレハブをGameObject型で取得（デバッグ用）
#endif

	//移動処理系の変数
	[UnityEngine.Tooltip("エネミーが止まってほしい座標位置の範囲")]
	[SerializeField] float endPos = 0.1f;

	//前進できない際の処理
	Vector3 startPos;
	float oldDistance = 0.0f;
	float endCount = 0.0f;
	[UnityEngine.Tooltip("前進できない際の強制終了時間")]
	float endTime = 1.0f;

	// Taskが処理される直前に呼ばれる
	public override void OnStart()
	{
		target = this.GetComponent<Target>();

		InitMove();
		InitEnemyCanNotMove();
		TargetPos();
		AdjustNavMeshPosition();
	}

	void TargetPos()
	{
		targetPos = target.ResultCoverPosition;
	}

	void InitMove()
	{
		target.Rigidbody.velocity = Vector3.zero;
		isEnd = false;
	}

	/// <summary>
	/// エネミーの移動距離が全く変わらない場合の強制終了カウントの初期化処理
	/// </summary>
	void InitEnemyCanNotMove()
	{
		startPos = target.transform.position;
		endCount = 0.0f;
		oldDistance = 0.0f;
	}

	// 更新時に呼ばれる
	public override TaskStatus OnUpdate()
	{
		if (isEnd == true)
		{
			isEnd = false;
			//目的地にたどりついた
			return TaskStatus.Success;
		}

		UpdateTargetPosition();
		//実行中
		return TaskStatus.Running;
	}

	/// <summary>
	/// 毎フレーム、プレイヤーの位置を NavMeshAgent に通知して経路を再計算させる。
	/// SetDestination は非同期に経路を計算する場合があるため、
	/// 経路が計算中かどうかは FixedUpdate 側で hasPath / pathPending / pathStatus を参照して扱う。
	/// </summary>
	void UpdateTargetPosition()
	{
		// 常にプレイヤーを目的地に設定する（敵 AI などの追従目的）
		target.NavMeshAgent.SetDestination(targetPos);
	}

	public override void OnFixedUpdate()
	{
		float currentDistance = RounndFloat(Vector3.Distance(startPos, target.transform.position));
		//Debug.Log("<color=red>currentDistance " + currentDistance + "</color>");
		//Debug.Log("<color=blue>oldDistance " + oldDistance + "</color>");
		//エネミーの移動距離が全く変わらない場合
		if (oldDistance == currentDistance)
		{
			//強制終了用のカウントを足す
			endCount = endCount + Time.deltaTime;
			//Debug.Log("<color=green>endCount " + endCount + "</color>");
		}

		//エネミーがその場から前に進めず距離計算ができない場合に、秒数で移動アクションを終了させる処理
		if (endTime <= endCount)
		{
			isEnd = true;
		}

		Move();

		//エネミーの移動距離が全く変わらない場合の強制終了カウント用の距離計算処理
		oldDistance = currentDistance;
	}

	/// <summary>
	/// 小数点第2以下を切り捨てる処理
	/// 12.345 を 12.3 にしたい場合
	/// </summary>
	float RounndFloat(float number)
	{
		//-------------------------------
		//0.31 を 0.3 にしたい場合（小数点第一以下切り捨ての例）
		//float calculation1 = 0.31f;
		//float calculation2 = calculation1 * 10;           //←0.31 を 一時的に10倍にし 3.1 にする
		//float calculation3 = Mathf.Floor(calculation2);   //←3.1 の .1 部分をFloorで切り捨て 3 にし
		//float result = calculation3 / 10;                 //その後10で割る事で 0.3 になり目的達成
		//-------------------------------

		float calculation1 = number;
		float calculation2 = calculation1 * 10;
		float calculation3 = Mathf.Floor(calculation2);
		float result = calculation3 / 10;

		return result;
	}

	void Move()
	{
		float sqrCurrentDistance = Vector3.SqrMagnitude(targetPos - this.transform.position);
		if (sqrCurrentDistance <= endPos)
		{
			//Debug.Log("<color=red>移動の終了</color>");
			target.Rigidbody.velocity = Vector3.zero;
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
		float sqr = (target.NavMeshAgent.nextPosition - target.Rigidbody.position).sqrMagnitude;
		if (0.25f < sqr)
		{
			target.NavMeshAgent.nextPosition = target.Rigidbody.position;
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
		if (target.NavMeshAgent.hasPath == false)
		{
			target.Rigidbody.velocity = new Vector3(0, target.Rigidbody.velocity.y, 0);
			return;
		}

		// NavMeshAgent が示す次の操舵目標点（steeringTarget）を取得
		Vector3 steerTarget = target.NavMeshAgent.steeringTarget;

		// 現在位置から操舵目標点へのベクトルを計算（Y は無視して水平のみ扱う）
		Vector3 dir = steerTarget - target.transform.position;
		dir.y = 0;

		// 進みたい方向の単位ベクトルに最大速度を掛けて目標速度を決定
		Vector3 desiredVel = dir.normalized * speed;

		// 現在の水平速度（Y成分は除く）
		Vector3 horizontalVel = new Vector3(target.Rigidbody.velocity.x, 0, target.Rigidbody.velocity.z);

		// 目標の水平速度成分だけを取り出す
		Vector3 targetHorizontal = new Vector3(desiredVel.x, 0, desiredVel.z);

		// 現在の水平速度を目標の水平速度へ向かって滑らかに変化させる
		// acceleration * Time.fixedDeltaTime がこのフレームで許容する最大の変化量
		Vector3 newHorizontal = Vector3.MoveTowards(horizontalVel, targetHorizontal, acceleration * Time.fixedDeltaTime);

		// Y 成分は保持して、Rigidbody の速度を更新する
		target.Rigidbody.velocity = new Vector3(newHorizontal.x, target.Rigidbody.velocity.y, newHorizontal.z);

		// 十分に速度がある場合は向きを滑らかに回転させる（視覚的な向き合わせ）
		if (0.01f < newHorizontal.sqrMagnitude)
		{
			target.transform.rotation = Quaternion.Slerp(target.transform.rotation, Quaternion.LookRotation(newHorizontal.normalized), 10f * Time.fixedDeltaTime);
		}

		// ★これが重要：Agent内部位置を物理位置に同期
		target.NavMeshAgent.nextPosition = target.Rigidbody.position;
	}
}