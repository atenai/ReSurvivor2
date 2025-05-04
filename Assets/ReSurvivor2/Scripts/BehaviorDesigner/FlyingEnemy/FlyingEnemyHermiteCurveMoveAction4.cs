using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

/// <summary>
/// フライングエネミーがエルミート追尾するタスク
/// </summary>
[TaskCategory("FlyingEnemy")]
public class FlyingEnemyHermiteCurveMoveAction4 : Action
{
	FlyingEnemy flyingEnemy;
	GameObject target;

	[UnityEngine.Tooltip("スタート地点")]
	Vector3 startPos;
	[UnityEngine.Tooltip("向きベクトルを出す為ようのオブジェクトのトランスフォーム")]
	Vector3 startDir;
	[UnityEngine.Tooltip("折り返し地点")]
	Vector3 turningPos;
	[UnityEngine.Tooltip("向きベクトルを出す為ようのオブジェクトのトランスフォーム")]
	Vector3 turningDir;
	[UnityEngine.Tooltip("エンド地点")]
	Vector3 endPos;
	[UnityEngine.Tooltip("向きベクトルを出す為ようのオブジェクトのトランスフォーム")]
	Vector3 endDir;
	[UnityEngine.Tooltip("エネミーの向きとターゲットの向きの内積")]
	float dot = 1.0f;
	[UnityEngine.Tooltip("同じ方向か？")]
	bool isSameDirection = true;
	[UnityEngine.Tooltip("補完スタート地点")]
	Vector3 complementStartPos;
	[UnityEngine.Tooltip("向きベクトルを出す為ようのオブジェクトのトランスフォーム")]
	Vector3 complementStartDir;
	[UnityEngine.Tooltip("補完エンド地点")]
	Vector3 complementEndPos;
	[UnityEngine.Tooltip("向きベクトルを出す為ようのオブジェクトのトランスフォーム")]
	Vector3 complementEndDir;
	[UnityEngine.Tooltip("エルミートのカーブ度を入れる変数")]
	float curve;
	[UnityEngine.Tooltip("エルミートのカーブ度が反転してるか？")]
	bool isCurve = false;

	//回転処理の変数系
	float directionTargetTime = 0.0f;
	Quaternion lookRotationTarget;
	bool isTargetRotEnd = false;

	//プランナーにいじってもらう変数
	[SerializeField] float period = 1.0f;
	float periodRatio = 1.0f;
	[UnityEngine.Tooltip("エンド座標をターゲットからどれくらい離れた位置にするのかを指定する数値")]
	[SerializeField] float specifiedPos = 8.0f;
	[UnityEngine.Tooltip("ターゲット座標をプレイヤーからどれくらい離れた位置にするのかを指定する数値")]
	[SerializeField] float extrusionPos = 0.0f;
	[UnityEngine.Tooltip("エルミートのカーブ度の数値")]
	[SerializeField] float curveDegree = 10.0f;
	[UnityEngine.Tooltip("エルミート座標位置から離れすぎた場合の丸め値")]
	[SerializeField] float moveLimit = 15.0f;
	[UnityEngine.Tooltip("エルミートカーブの向きベクトルを逆にするか？")]
	[SerializeField] bool isInverseVector = false;

	//初期化する必要がある変数
	float hermiteTime = 0.0f;
	[UnityEngine.Tooltip("次のエルミート移動をに進むか？")]
	bool isNext = false;
	[UnityEngine.Tooltip("処理を終了するか？")]
	bool isEnd = false;

#if UNITY_EDITOR
	//デバッグ用変数
	[SerializeField] GameObject obj1;//プレハブをGameObject型で取得（デバッグ用）
	[SerializeField] GameObject obj2;//プレハブをGameObject型で取得（デバッグ用）
	[SerializeField] bool isSlowMotion = false;
	[SerializeField] bool isLowFrameRate = false;
#endif

	// Taskが処理される直前に呼ばれる
	public override void OnStart()
	{
		flyingEnemy = this.GetComponent<FlyingEnemy>();

		target = flyingEnemy.TargetPlayer;

		//periodRatio = 1.0f ÷ 0.5f = 2.0f
		periodRatio = 1.0f / period;
		SameDirection();
		InitRotateToDirectionTarget();
		InitHermiteMove();
	}

	void SameDirection()
	{
		if (0 < dot)
		{
			isSameDirection = true;
			//Debug.Log("<color=red>1</color>");
		}
		else if (dot <= 0)
		{
			isSameDirection = false;
			//Debug.Log("<color=#AFDFE4>7</color>");
		}
	}

	/// <summary>
	/// ターゲットの方を向く初期化処理
	/// </summary>
	void InitRotateToDirectionTarget()
	{
		directionTargetTime = 0.0f;
		isTargetRotEnd = false;
		//対象オブジェクトの位置 – 自分のオブジェクトの位置 = 対象オブジェクトの向きベクトルが求められる
		Vector3 direction = target.transform.position - this.transform.position;
		//単純に左右だけを見るようにしたいので、y軸の数値を0にする
		direction.y = 0;
		//2点間の角度を求める
		//第一引数に向きたい方向の向きベクトルを入れてあげる、それによってどのくらい回転させれば良いのか？の数値を求めることができる
		lookRotationTarget = Quaternion.LookRotation(direction, Vector3.up);
	}

	/// <summary>
	/// エルミート移動をする為の初期化処理
	/// </summary>
	void InitHermiteMove()
	{
		if (isCurve == false)
		{
			if (isInverseVector == true)
			{
				curve = curveDegree * -1.0f;
			}
			else
			{
				curve = curveDegree * 1.0f;
			}
		}
		else if (isCurve == true)
		{
			if (isInverseVector == true)
			{
				curve = curveDegree * 1.0f;
			}
			else
			{
				curve = curveDegree * -1.0f;
			}
		}
		startPos = new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z);
		startDir = new Vector3(startPos.x, startPos.y - curve, startPos.z);//スタートの向きベクトル

		Vector3 localTurningPos = new Vector3(target.transform.position.x, target.transform.position.y, target.transform.position.z);
		if (target.transform.position.x <= this.transform.position.x)//ターゲットがプレイヤーより左の場合
		{
			localTurningPos.x = localTurningPos.x - extrusionPos;
		}
		else if (target.transform.position.x > this.transform.position.x)//ターゲットがプレイヤーより右の場合
		{
			localTurningPos.x = localTurningPos.x + extrusionPos;
		}
		turningPos = localTurningPos;//中間
		turningDir = new Vector3(turningPos.x, turningPos.y + curve, turningPos.z);//中間の向きベクトル

		Vector3 localEndPos = new Vector3(turningPos.x, turningPos.y, turningPos.z);
		if (target.transform.position.x <= this.transform.position.x)//ターゲットがプレイヤーより左の場合
		{
			localEndPos.x = (localEndPos.x + specifiedPos);
		}
		else if (target.transform.position.x > this.transform.position.x)//ターゲットがプレイヤーより右の場合
		{
			localEndPos.x = (localEndPos.x - specifiedPos);
		}
		endPos = localEndPos;//エンド
		endDir = new Vector3(endPos.x, endPos.y - curve, endPos.z);//エンドの向きベクトル

		//補完地点を保存
		if (isSameDirection == true)
		{
			//Debug.Log("<color=blue>2</color>");
			complementStartPos = endPos;//エンド
			complementStartDir = endDir;//エンドの向きベクトル
			complementEndPos = turningPos;//中間
			complementEndDir = turningDir;//中間の向きベクトル
		}

		//初期化する必要がある変数
		//hermiteTime = 0.0f;
		isNext = false;
		isEnd = false;
	}

	// 更新時に呼ばれる
	public override TaskStatus OnUpdate()
	{
		if (isEnd == true)
		{
			return TaskStatus.Success;
		}

		return TaskStatus.Running;
	}

	public override void OnFixedUpdate()
	{
		DebugGameSpeed();
		PutDirectionInInnerProduct();
		RotateToDirectionTarget();
		MoveTarget();
	}

	/// <summary>
	/// 内積を使いエネミーの向きとプレイヤーの方向のコサイン数値を調べる
	/// </summary>
	void PutDirectionInInnerProduct()
	{
		Vector3 targetDir = target.transform.position - this.transform.position;
		dot = Vector3.Dot(this.transform.forward, targetDir);
#if UNITY_EDITOR
		Ray playerRay = new Ray(this.transform.position, this.transform.forward);
		Debug.DrawRay(playerRay.origin, playerRay.direction * 2.0f, Color.red);
		Ray targetRay = new Ray(this.transform.position, targetDir);
		Debug.DrawRay(targetRay.origin, targetRay.direction * 2.0f, Color.blue);
#endif
	}

	/// <summary>
	/// ターゲットの方を向く
	/// </summary>
	void RotateToDirectionTarget()
	{
		//Time.deltaTimeは0.1f
		directionTargetTime = directionTargetTime + Time.deltaTime;
		//↑で求めたどのくらい回転させれば良いのか？の数値を元に回転させる(Lerpのtは0～1)
		this.transform.rotation = Quaternion.Lerp(this.transform.rotation, lookRotationTarget, directionTargetTime);
	}

	/// <summary>
	/// ターゲットに向けて移動する
	/// </summary>
	void MoveTarget()
	{
		hermiteTime = hermiteTime + (Time.fixedDeltaTime * periodRatio);

		while (1.0f < hermiteTime)//もしエルミート用の数値が1を超えたらカーブの向きを反対にする
		{
			if (isSameDirection == true)
			{
				if (isNext == false)
				{
					//Debug.Log("<color=yellow>4</color>");
					isNext = true;
				}
				else if (isNext == true)
				{
					//Debug.Log("<color=purple>6</color>");
					Vector3 velocity = Vector3.zero;
					flyingEnemy.Rigidbody.velocity = velocity;
					//isEnd = true;//エルミート移動の処理を終了する
					//return;
					SameDirection();
					InitRotateToDirectionTarget();
					InitHermiteMove();
				}
			}
			else if (isSameDirection == false)
			{
				//Debug.Log("<color=#808080>9</color>");
				isSameDirection = true;
				//カーブを反転させる
				if (isCurve == false)
				{
					isCurve = true;
				}
				else if (isCurve == true)
				{
					isCurve = false;
				}

				if (isCurve == false)
				{
					if (isInverseVector == true)
					{
						curve = curveDegree * -1.0f;
					}
					else
					{
						curve = curveDegree * 1.0f;
					}
				}
				else if (isCurve == true)
				{
					if (isInverseVector == true)
					{
						curve = curveDegree * 1.0f;
					}
					else
					{
						curve = curveDegree * -1.0f;
					}
				}
				startPos = new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z);
				startDir = new Vector3(startPos.x, startPos.y - curve, startPos.z);

				Vector3 localTurningPos = new Vector3(target.transform.position.x, target.transform.position.y, target.transform.position.z);
				if (target.transform.position.x <= this.transform.position.x)//ターゲットがプレイヤーより左の場合
				{
					localTurningPos.x = localTurningPos.x - extrusionPos;
				}
				else if (target.transform.position.x > this.transform.position.x)//ターゲットがプレイヤーより右の場合
				{
					localTurningPos.x = localTurningPos.x + extrusionPos;
				}
				turningPos = localTurningPos;//中間
				turningDir = new Vector3(turningPos.x, turningPos.y + curve, turningPos.z);//中間の向きベクトル

				Vector3 localEndPos = new Vector3(turningPos.x, turningPos.y, turningPos.z);
				if (target.transform.position.x <= this.transform.position.x)//ターゲットがプレイヤーより左の場合
				{
					localEndPos.x = (localEndPos.x + specifiedPos);
				}
				else if (target.transform.position.x > this.transform.position.x)//ターゲットがプレイヤーより右の場合
				{
					localEndPos.x = (localEndPos.x - specifiedPos);
				}
				endPos = localEndPos;//エンド
				endDir = new Vector3(endPos.x, endPos.y - curve, endPos.z);//エンドの向きベクトル
			}

			hermiteTime = hermiteTime - 1.0f;
		}

		if (isSameDirection == false)
		{
			//Debug.Log("<color=white>8</color>");
			DebugSemiCircle(complementStartPos, complementEndPos, complementStartDir, complementEndDir);
			HermiteMove(complementStartPos, complementEndPos, complementStartDir, complementEndDir);
		}
		else if (isSameDirection == true)
		{
			if (isNext == false)
			{
				//Debug.Log("<color=green>3</color>");
				DebugSemiCircle(startPos, turningPos, startDir, turningDir);
				HermiteMove(startPos, turningPos, startDir, turningDir);
			}
			else if (isNext == true)
			{
				//Debug.Log("<color=orange>5</color>");
				DebugSemiCircle(turningPos, endPos, turningDir, endDir);
				HermiteMove(turningPos, endPos, turningDir, endDir);
			}
		}
	}

	/// <summary>
	/// エルミート移動をする
	/// </summary>
	void HermiteMove(Vector3 _startPos, Vector3 _endPos, Vector3 _startDir, Vector3 _endDir)
	{
		Vector3 p0 = _startPos;//スタート座標
		Vector3 p1 = _endPos;//エンド座標

		//二点間のベクトルの求め方。
		//Vector3 vec = ( target.transform.position - transform.position ).normalized;//target方向の向きが出る。
		Vector3 v0 = _startDir - p0;//スタート座標の向きベクトル
		Vector3 v1 = _endDir - p1;//エンド座標の向きベクトル

		//エルミートの公式を使い目的座標位置を出す
		Vector3 nextHermitePos = GetHermiteCurvePoint(p0, v0, p1, v1, hermiteTime);

		//エネミーとエルミートポイントの距離を出す
		float sqrCurrentDistance = Vector3.SqrMagnitude(nextHermitePos - this.transform.position);
		float maxDistance = (moveLimit * Time.fixedDeltaTime);
		if (maxDistance * maxDistance < sqrCurrentDistance)
		{
			//Debug.Log("<color=red>丸め値、移動</color>");
			Vector3 roundNumber = TargetVector(nextHermitePos).normalized * moveLimit;
			flyingEnemy.Rigidbody.velocity = roundNumber;
		}
		else
		{
			//Debug.Log("<color=blue>通常値、移動</color>");
			flyingEnemy.Rigidbody.velocity = TargetVector(nextHermitePos) / Time.fixedDeltaTime;//目標地点に即座に移動してほしい為にTime.fixedDeltaTimeで割る;
		}
	}

	/// <summary>
	/// エネミーからターゲットへの向きベクトルを出す
	/// </summary>
	Vector3 TargetVector(Vector3 _targetPos)
	{
		return (_targetPos - this.transform.position);
	}

	/// <summary>
	/// エルミート曲線の公式
	/// Vector3 GetHermiteCurvePoint(Vector3 座標0, Vector3 座標0の向きベクトル, Vector3 座標1, Vector3 座標1の向きベクトル, float 時間（0～1）)
	/// </summary>
	Vector3 GetHermiteCurvePoint(Vector3 p0, Vector3 v0, Vector3 p1, Vector3 v1, float t1)
	{
		float t2 = t1 * t1;
		float t3 = t1 * t1 * t1;
		float mP0 = 2 * t3 - 3 * t2 + 0 + 1;
		float mV0 = t3 - 2 * t2 + t1;
		float mP1 = -2 * t3 + 3 * t2;
		float mV1 = t3 - t2;

		return new Vector3
		(
			(p0.x * mP0) + (v0.x * mV0) + (p1.x * mP1) + (v1.x * mV1),
			(p0.y * mP0) + (v0.y * mV0) + (p1.y * mP1) + (v1.y * mV1),
			(p0.z * mP0) + (v0.z * mV0) + (p1.z * mP1) + (v1.z * mV1)
		);
	}

	/// <summary>
	/// エルミートの円を描くデバッグ用関数
	/// </summary> 
	void DebugCircle()
	{
#if UNITY_EDITOR
		Vector3 p0 = startPos;//スタート座標
		Vector3 p1 = turningPos;//エンド座標

		//二点間のベクトルの求め方。
		//Vector3 vec = ( target.transform.position - transform.position ).normalized;//target方向の向きが出る。
		Vector3 v0 = startDir - p0;//スタート座標の向きベクトル
		Vector3 v1 = turningDir - p1;//エンド座標の向きベクトル

		//10回繰り返す（0～1の間を0.1ずつ増やしていく）
		for (float t = 0; t < 1; t += 0.1f)
		{
			Vector3 hermitePos = GetHermiteCurvePoint(p0, v0, p1, v1, t);
			GameObject debugGameObject1 = UnityEngine.Object.Instantiate(obj2, hermitePos, Quaternion.identity);
			UnityEngine.Object.Destroy(debugGameObject1, 5.0f); // 5秒後にゲームオブジェクトを削除
		}

		GameObject debugGameObjectStartPos = UnityEngine.Object.Instantiate(obj1, startPos, Quaternion.identity);//プレハブを元に、インスタンスを生成（デバッグ用）               
		UnityEngine.Object.Destroy(debugGameObjectStartPos, 5.0f);// 5秒後にゲームオブジェクトを削除
		GameObject debugGameObjectTurningPos = UnityEngine.Object.Instantiate(obj1, turningPos, Quaternion.identity);//プレハブを元に、インスタンスを生成（デバッグ用）               
		UnityEngine.Object.Destroy(debugGameObjectTurningPos, 5.0f);// 5秒後にゲームオブジェクトを削除

		Vector3 p2 = turningPos;//スタート座標
		Vector3 p3 = endPos;//エンド座標

		//二点間のベクトルの求め方。
		//Vector3 vec = ( target.transform.position - transform.position ).normalized;//target方向の向きが出る。
		Vector3 v2 = turningDir - p2;//スタート座標の向きベクトル
		Vector3 v3 = endDir - p3;//エンド座標の向きベクトル

		//10回繰り返す（0～1の間を0.1ずつ増やしていく）
		for (float t = 0; t < 1; t += 0.1f)
		{
			Vector3 hermitePos = GetHermiteCurvePoint(p2, v2, p3, v3, t);
			GameObject debugGameObject2 = UnityEngine.Object.Instantiate(obj2, hermitePos, Quaternion.identity);
			UnityEngine.Object.Destroy(debugGameObject2, 5.0f); // 5秒後にゲームオブジェクトを削除
		}

		GameObject debugGameObjectEndPos = UnityEngine.Object.Instantiate(obj1, endPos, Quaternion.identity);//プレハブを元に、インスタンスを生成（デバッグ用）
		UnityEngine.Object.Destroy(debugGameObjectEndPos, 5.0f);// 5秒後にゲームオブジェクトを削除
#endif
	}

	/// <summary>
	/// エルミートの半円を描くデバッグ用関数
	/// </summary> 
	void DebugSemiCircle(Vector3 _nowPos, Vector3 _nextPos, Vector3 _nowDir, Vector3 _nextDir)
	{
#if UNITY_EDITOR
		Vector3 p0 = _nowPos;//スタート座標
		Vector3 p1 = _nextPos;//エンド座標

		//二点間のベクトルの求め方。
		//Vector3 vec = ( target.transform.position - transform.position ).normalized;//target方向の向きが出る。
		Vector3 v0 = _nowDir - p0;//スタート座標の向きベクトル
		Vector3 v1 = _nextDir - p1;//エンド座標の向きベクトル

		//10回繰り返す（0～1の間を0.1ずつ増やしていく）
		for (float t = 0; t < 1; t += 0.1f)
		{
			Vector3 hermitePos = GetHermiteCurvePoint(p0, v0, p1, v1, t);
			GameObject debugGameObject = UnityEngine.Object.Instantiate(obj2, hermitePos, Quaternion.identity);
			// 5秒後にゲームオブジェクトを削除
			UnityEngine.Object.Destroy(debugGameObject, 5.0f);
		}

		GameObject debugGameObjectNowPos = UnityEngine.Object.Instantiate(obj1, _nowPos, Quaternion.identity);//プレハブを元に、インスタンスを生成（デバッグ用）               
		UnityEngine.Object.Destroy(debugGameObjectNowPos, 5.0f);// 5秒後にゲームオブジェクトを削除
		GameObject debugGameObjectEextPos = UnityEngine.Object.Instantiate(obj1, _nextPos, Quaternion.identity);//プレハブを元に、インスタンスを生成（デバッグ用）
		UnityEngine.Object.Destroy(debugGameObjectEextPos, 5.0f);// 5秒後にゲームオブジェクトを削除
#endif
	}

	/// <summary>
	/// ゲームスピードを変更するデバッグ用関数
	/// </summary> 
	void DebugGameSpeed()
	{
#if UNITY_EDITOR
		if (isSlowMotion == true)
		{
			Time.timeScale = 0.1f;//ゲーム全体の速度を遅くする（10%）
		}

		if (isLowFrameRate == true)
		{
			Application.targetFrameRate = 5;//ゲーム全体のフレームレートを落とす
		}
#endif
	}
}